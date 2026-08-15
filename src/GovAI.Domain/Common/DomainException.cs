namespace GovAI.Domain.Common;

/// <summary>
/// İş kuralı ihlallerinde fırlatılır. API katmanı bunu 422/400'e çevirir.
/// </summary>
public sealed class DomainException(string message) : Exception(message)
{
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
        {
            throw new DomainException(message);
        }
    }
}
