using GovAI.Application.Abstractions.Persistence;
using GovAI.Application.Abstractions.Services;
using GovAI.Domain.Auditing;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GovAI.Api.Infrastructure;

/// <summary>
/// Bir isteğin denetim kaydına yazılacağını ve hangi eylem adıyla yazılacağını belirtir.
/// Teknik doküman 5.4: "Her skor ve kullanıcı aksiyonu zaman damgası ile kaydedilmelidir."
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AuditedAttribute(string action, string entityType = "") : Attribute
{
    public string Action { get; } = action;

    public string EntityType { get; } = entityType;

    /// <summary>Kaydedilecek varlık kimliğinin okunacağı route parametresi adı.</summary>
    public string RouteKey { get; init; } = "id";
}

/// <summary>
/// <see cref="AuditedAttribute"/> ile işaretlenmiş eylemleri başarıyla tamamlandıklarında audit log'a yazar.
/// Başarısız istekler zaten merkezî hata loguna düşer; denetim kaydı yalnızca gerçekleşen değişiklikleri izler.
/// </summary>
public sealed class AuditActionFilter(
    IAuditLogRepository auditLog,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    ILogger<AuditActionFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var descriptor = context.ActionDescriptor.EndpointMetadata.OfType<AuditedAttribute>().FirstOrDefault();

        var executed = await next();

        if (descriptor is null || executed.Exception is { } && !executed.ExceptionHandled)
        {
            return;
        }

        try
        {
            var entityId = context.RouteData.Values.TryGetValue(descriptor.RouteKey, out var value)
                ? value?.ToString()
                : null;

            var entry = new AuditLogEntry(
                currentUser.TenantId,
                descriptor.Action,
                descriptor.EntityType,
                entityId,
                currentUser.UserId?.ToString(),
                currentUser.Email,
                clock.UtcNow);

            entry.SetRequestContext(currentUser.IpAddress, currentUser.UserAgent, currentUser.CorrelationId);

            await auditLog.AddAsync(entry, context.HttpContext.RequestAborted);
            await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            // Denetim kaydı yazılamazsa kullanıcı isteği başarısız sayılmaz, ancak bu durum
            // operasyonel bir sorundur ve hata seviyesinde loglanır.
            logger.LogError(ex, "Denetim kaydı yazılamadı. Eylem={Action}", descriptor.Action);
        }
    }
}
