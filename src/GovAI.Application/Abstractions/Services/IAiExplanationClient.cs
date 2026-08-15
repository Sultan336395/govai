using GovAI.Domain.Common;

namespace GovAI.Application.Abstractions.Services;

/// <summary>
/// AI Explanation Service'in Application tarafındaki sözleşmesi.
///
/// Önemli tasarım kararı: AI burada karar verici değildir.
/// - <see cref="ExtractRulesAsync"/> serbest formatlı resmî metni yapılandırılmış kural taslağına çevirir;
///   sonuç danışman onayına düşer ve güven değeri (<c>confidence</c>) ile birlikte saklanır.
/// - <see cref="GenerateExecutiveSummaryAsync"/> zaten hesaplanmış skoru yönetici diline çevirir;
///   skoru değiştirmez, yalnızca anlatır.
/// </summary>
public interface IAiExplanationClient
{
    /// <summary>Çağrı metninden makine-değerlendirilebilir koşul taslakları çıkarır.</summary>
    Task<RuleExtractionResult> ExtractRulesAsync(
        RuleExtractionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Hesaplanmış uygunluk sonucunu yöneticiye hitap eden Türkçe özete dönüştürür.</summary>
    Task<AiSummaryResult> GenerateExecutiveSummaryAsync(
        ExecutiveSummaryRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record RuleExtractionRequest
{
    public required string OpportunityTitle { get; init; }

    public required string NormalizedText { get; init; }

    public SupportCategory? ExpectedCategory { get; init; }

    /// <summary>Modelin uydurma alan adı üretmemesi için izin verilen alanların listesi.</summary>
    public required IReadOnlyDictionary<string, string> AllowedFields { get; init; }
}

public sealed record RuleExtractionResult
{
    public required IReadOnlyList<ExtractedRule> Rules { get; init; }

    public required IReadOnlyList<ExtractedDocument> Documents { get; init; }

    /// <summary>Modelin çıkarımın tamamına dair güveni (0..1).</summary>
    public required decimal Confidence { get; init; }

    public string? Summary { get; init; }

    public DateTimeOffset? Deadline { get; init; }

    public SupportCategory? DetectedCategory { get; init; }

    public string? ModelName { get; init; }
}

public sealed record ExtractedRule
{
    public required string Field { get; init; }

    public required string Operator { get; init; }

    public required string Value { get; init; }

    public required string Dimension { get; init; }

    public required string Severity { get; init; }

    public required string HumanReadable { get; init; }

    public string? SourceExcerpt { get; init; }

    public decimal Confidence { get; init; } = 0.5m;
}

public sealed record ExtractedDocument
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public bool IsMandatory { get; init; } = true;

    public string? IssuingAuthority { get; init; }
}

public sealed record ExecutiveSummaryRequest
{
    public required string CompanyName { get; init; }

    public required string OpportunityTitle { get; init; }

    public required string Publisher { get; init; }

    public required EligibilityVerdict Verdict { get; init; }

    public required decimal FinalScore { get; init; }

    public required IReadOnlyList<string> DimensionHighlights { get; init; }

    public required IReadOnlyList<string> BlockingReasons { get; init; }

    public required IReadOnlyList<string> MissingConditions { get; init; }

    public required IReadOnlyList<string> MissingDocuments { get; init; }

    public DateTimeOffset? Deadline { get; init; }
}

public sealed record AiSummaryResult(string Summary, string ModelName);
