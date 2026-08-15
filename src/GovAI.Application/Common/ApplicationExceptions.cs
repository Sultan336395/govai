namespace GovAI.Application.Common;

/// <summary>İstenen kayıt yok — API katmanı 404'e çevirir.</summary>
public sealed class NotFoundException(string entity, object key)
    : Exception($"{entity} bulunamadı (anahtar: {key}).")
{
    public string Entity { get; } = entity;

    public object Key { get; } = key;
}

/// <summary>Kullanıcının bu kayda erişim yetkisi yok — API katmanı 403'e çevirir.</summary>
public sealed class ForbiddenException(string message) : Exception(message);

/// <summary>Girdi doğrulama hatası — API katmanı 400 + alan bazlı hata listesi döner.</summary>
public sealed class ValidationException : Exception
{
    public ValidationException(string field, string message)
        : base("Girdi doğrulaması başarısız.")
    {
        Errors = new Dictionary<string, string[]> { [field] = [message] };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Girdi doğrulaması başarısız.")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

/// <summary>Kimlik doğrulama başarısız — API katmanı 401 döner.</summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);
