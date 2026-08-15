using GovAI.Application.Abstractions.Persistence;
using GovAI.Application.Abstractions.Services;
using GovAI.Application.Common;
using GovAI.Domain.Opportunities;
using Microsoft.Extensions.Logging;

namespace GovAI.Application.Opportunities;

/// <summary>
/// Teşvik – Hibe – İhale Sınıflandırma Modülü'nün (Modül 4) use-case servisi.
/// Fırsat kataloğunun okunması, worker'lardan gelen çağrıların kaydı ve danışman onayı burada yönetilir.
/// </summary>
public sealed class OpportunityService(
    IOpportunityRepository opportunities,
    ISourceRepository sources,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IEventPublisher events,
    ILogger<OpportunityService> logger)
{
    public async Task<PagedResult<OpportunitySummaryDto>> SearchAsync(OpportunityQuery query, CancellationToken cancellationToken = default)
    {
        var page = await opportunities.SearchAsync(query, cancellationToken);
        var now = clock.UtcNow;

        return new PagedResult<OpportunitySummaryDto>(
            page.Items.Select(o => ToSummary(o, now)).ToList(),
            page.TotalCount,
            page.Page,
            page.PageSize);
    }

    public async Task<OpportunityDetailDto> GetAsync(Guid opportunityId, CancellationToken cancellationToken = default)
    {
        var opportunity = await opportunities.GetWithRulesAsync(opportunityId, cancellationToken)
                          ?? throw new NotFoundException("Fırsat", opportunityId);

        return ToDetail(opportunity, clock.UtcNow);
    }

    /// <summary>
    /// Çağrıyı oluşturur ya da aynı kaynak dokümandan gelen kaydı günceller.
    /// Parser worker'ı aynı ilanı yeniden işlediğinde kopya kayıt oluşmaz.
    /// </summary>
    public async Task<OpportunityDetailDto> UpsertAsync(UpsertOpportunityRequest request, CancellationToken cancellationToken = default)
    {
        _ = await sources.GetAsync(request.SourceId, cancellationToken)
            ?? throw new NotFoundException("Kaynak", request.SourceId);

        Opportunity? opportunity = null;
        if (request.SourceDocumentId is not null)
        {
            opportunity = await opportunities.GetBySourceDocumentAsync(request.SourceDocumentId.Value, cancellationToken);
        }

        var isNew = opportunity is null;
        if (opportunity is null)
        {
            opportunity = new Opportunity(
                request.SourceId,
                request.SourceType,
                request.SupportCategory,
                request.Title,
                request.Publisher,
                request.PublishedAt);

            await opportunities.AddAsync(opportunity, cancellationToken);
        }

        opportunity.Describe(request.Summary, request.SourceUrl, request.SourceDocumentId);
        opportunity.SetSchedule(request.PublishedAt, request.Deadline);
        opportunity.SetBudget(request.Budget is null
            ? null
            : new BudgetRange(request.Budget.MinAmount, request.Budget.MaxAmount, request.Budget.Currency, request.Budget.SupportRate));

        opportunity.ReplaceRules(request.Rules.Select(ToDomain), request.RuleExtractionConfidence);
        opportunity.ReplaceDocumentChecklist(request.DocumentChecklist.Select(ToDomain));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Fırsat {Action}. OpportunityId={OpportunityId} Kural={RuleCount} Güven={Confidence}",
            isNew ? "oluşturuldu" : "güncellendi", opportunity.Id, request.Rules.Count, request.RuleExtractionConfidence);

        // Yeni/değişmiş çağrı, tüm firmalar için yeniden skorlama tetikler.
        await events.PublishAsync(
            QueueNames.ScoringRequested,
            new { OpportunityId = opportunity.Id, RequestedAt = clock.UtcNow, Reason = isNew ? "OpportunityCreated" : "OpportunityUpdated" },
            cancellationToken);

        return ToDetail(opportunity, clock.UtcNow);
    }

    /// <summary>Danışman, otomatik çıkarılan bir kuralı düzeltir. Elle düzeltilen kurallar korunur.</summary>
    public async Task<OpportunityDetailDto> OverrideRuleAsync(
        Guid opportunityId,
        Guid ruleId,
        OverrideRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var opportunity = await opportunities.GetWithRulesAsync(opportunityId, cancellationToken)
                          ?? throw new NotFoundException("Fırsat", opportunityId);

        var rule = opportunity.Rules.FirstOrDefault(r => r.Id == ruleId)
                   ?? throw new NotFoundException("Kural", ruleId);

        rule.OverrideManually(request.Operator, request.Value, request.Severity, request.HumanReadable);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await events.PublishAsync(
            QueueNames.ScoringRequested,
            new { OpportunityId = opportunityId, RequestedAt = clock.UtcNow, Reason = "RuleOverridden" },
            cancellationToken);

        return ToDetail(opportunity, clock.UtcNow);
    }

    /// <summary>Danışman onayı; skor güvenini tavana taşır.</summary>
    public async Task<OpportunityDetailDto> MarkReviewedAsync(Guid opportunityId, CancellationToken cancellationToken = default)
    {
        var opportunity = await opportunities.GetWithRulesAsync(opportunityId, cancellationToken)
                          ?? throw new NotFoundException("Fırsat", opportunityId);

        opportunity.MarkReviewed();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDetail(opportunity, clock.UtcNow);
    }

    private static OpportunityRule ToDomain(UpsertRuleDto dto) => new(
        dto.Field,
        dto.Operator,
        dto.Value,
        dto.Dimension,
        dto.Severity,
        dto.HumanReadable,
        dto.SourceExcerpt,
        dto.Confidence);

    private static DocumentRequirement ToDomain(DocumentRequirementDto dto) =>
        new(dto.Code, dto.Name, dto.IsMandatory, dto.IssuingAuthority, dto.Notes);

    public static OpportunitySummaryDto ToSummary(Opportunity opportunity, DateTimeOffset now) => new(
        opportunity.Id,
        opportunity.Title,
        opportunity.Publisher,
        opportunity.SourceType,
        opportunity.SupportCategory,
        opportunity.PublishedAt,
        opportunity.Deadline,
        opportunity.DaysUntilDeadline(now),
        opportunity.Budget?.MaxAmount,
        opportunity.Budget?.Currency,
        opportunity.IsReviewedByConsultant,
        opportunity.Rules.Count,
        opportunity.DocumentChecklist.Count);

    public static OpportunityDetailDto ToDetail(Opportunity opportunity, DateTimeOffset now) => new(
        opportunity.Id,
        opportunity.Title,
        opportunity.Publisher,
        opportunity.Summary,
        opportunity.SourceUrl,
        opportunity.SourceType,
        opportunity.SupportCategory,
        opportunity.PublishedAt,
        opportunity.Deadline,
        opportunity.DaysUntilDeadline(now),
        opportunity.Budget is null
            ? null
            : new BudgetDto(opportunity.Budget.MinAmount, opportunity.Budget.MaxAmount, opportunity.Budget.Currency, opportunity.Budget.SupportRate),
        opportunity.RuleExtractionConfidence,
        opportunity.IsReviewedByConsultant,
        opportunity.Rules.Select(r => new OpportunityRuleDto(
            r.Id, r.Field, r.Operator, r.Value, r.Dimension, r.Severity, r.HumanReadable, r.SourceExcerpt, r.Confidence, r.IsManuallyOverridden)).ToList(),
        opportunity.DocumentChecklist.Select(d => new DocumentRequirementDto(d.Code, d.Name, d.IsMandatory, d.IssuingAuthority, d.Notes)).ToList());
}
