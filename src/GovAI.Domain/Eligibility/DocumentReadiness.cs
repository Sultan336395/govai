using GovAI.Domain.Common;
using GovAI.Domain.Companies;
using GovAI.Domain.Opportunities;

namespace GovAI.Domain.Eligibility;

/// <summary>Başvuru dosyasındaki tek bir evrağın firma tarafındaki karşılığı.</summary>
public sealed record DocumentCheckResult
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required bool IsMandatory { get; init; }

    public required DocumentStatus Status { get; init; }

    public DateOnly? ValidUntil { get; init; }

    public string? IssuingAuthority { get; init; }

    /// <summary>Eksik veya süresi geçmiş belgeler için yapılacak iş.</summary>
    public string? Action { get; init; }
}

/// <summary>
/// Belge ve Başvuru Hazırlık Kontrol Modülü (Modül 8).
/// Çağrının belge listesini firmanın geçerli sertifikalarıyla karşılaştırır.
/// </summary>
public static class DocumentReadinessCalculator
{
    private const decimal MandatoryWeight = 1.0m;
    private const decimal OptionalWeight = 0.5m;

    /// <summary>Belge listesi hiç çıkarılamadıysa kullanılan nötr puan — 1.0 vermek yanıltıcı olurdu.</summary>
    public const decimal NeutralScoreWhenChecklistUnknown = 0.5m;

    public static IReadOnlyList<DocumentCheckResult> Check(
        Opportunity opportunity,
        Company company,
        DateOnly asOf)
    {
        var certificates = company.Certificates
            .GroupBy(c => c.Code.ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.ValidUntil ?? DateOnly.MaxValue).First());

        return opportunity.DocumentChecklist
            .Select(requirement => Evaluate(requirement, certificates, asOf))
            .ToList();
    }

    private static DocumentCheckResult Evaluate(
        DocumentRequirement requirement,
        IReadOnlyDictionary<string, CompanyCertificate> certificates,
        DateOnly asOf)
    {
        if (!certificates.TryGetValue(requirement.Code, out var certificate))
        {
            return new DocumentCheckResult
            {
                Code = requirement.Code,
                Name = requirement.Name,
                IsMandatory = requirement.IsMandatory,
                Status = DocumentStatus.Missing,
                IssuingAuthority = requirement.IssuingAuthority,
                Action = requirement.IssuingAuthority is null
                    ? $"{requirement.Name} belgesini temin edin."
                    : $"{requirement.Name} belgesini {requirement.IssuingAuthority} üzerinden temin edin."
            };
        }

        var expired = !certificate.IsValidOn(asOf);

        return new DocumentCheckResult
        {
            Code = requirement.Code,
            Name = requirement.Name,
            IsMandatory = requirement.IsMandatory,
            Status = expired ? DocumentStatus.Expired : DocumentStatus.Provided,
            ValidUntil = certificate.ValidUntil,
            IssuingAuthority = requirement.IssuingAuthority,
            Action = expired ? $"{requirement.Name} belgesinin geçerliliği dolmuş, yenileyin." : null
        };
    }

    /// <summary>
    /// Belge hazır olma puanı (0..1). Zorunlu belgeler tam, opsiyonel belgeler yarım ağırlıkla sayılır.
    /// Liste boşsa karar verilemediği için nötr puan döner.
    /// </summary>
    public static (decimal Score, string Rationale) Score(IReadOnlyList<DocumentCheckResult> results)
    {
        if (results.Count == 0)
        {
            return (NeutralScoreWhenChecklistUnknown, "Çağrı metninden belge listesi çıkarılamadı; nötr puan uygulandı.");
        }

        decimal earned = 0m, total = 0m;
        foreach (var result in results)
        {
            var weight = result.IsMandatory ? MandatoryWeight : OptionalWeight;
            total += weight;

            earned += result.Status switch
            {
                DocumentStatus.Provided => weight,
                DocumentStatus.NotRequired => weight,
                DocumentStatus.Expired => weight * 0.5m, // yenilenmesi kısa sürer, tamamen sıfır saymak yanıltıcı
                _ => 0m
            };
        }

        var score = total == 0m ? NeutralScoreWhenChecklistUnknown : Math.Round(earned / total, 4);
        var missingMandatory = results.Count(r => r.IsMandatory && r.Status == DocumentStatus.Missing);
        var expiredCount = results.Count(r => r.Status == DocumentStatus.Expired);

        var rationale = missingMandatory == 0 && expiredCount == 0
            ? $"{results.Count} belgenin tamamı hazır."
            : $"{results.Count} belgeden {missingMandatory} zorunlu belge eksik, {expiredCount} belge süresi dolmuş.";

        return (score, rationale);
    }
}
