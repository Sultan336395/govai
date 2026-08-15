using GovAI.Domain.Common;
using GovAI.Domain.Opportunities;

namespace GovAI.Application.Opportunities;

public sealed record OpportunitySummaryDto(
    Guid Id,
    string Title,
    string Publisher,
    SourceType SourceType,
    SupportCategory SupportCategory,
    DateTimeOffset PublishedAt,
    DateTimeOffset? Deadline,
    int? DaysUntilDeadline,
    decimal? MaxAmount,
    string? Currency,
    bool IsReviewedByConsultant,
    int RuleCount,
    int DocumentCount);

public sealed record OpportunityDetailDto(
    Guid Id,
    string Title,
    string Publisher,
    string? Summary,
    string? SourceUrl,
    SourceType SourceType,
    SupportCategory SupportCategory,
    DateTimeOffset PublishedAt,
    DateTimeOffset? Deadline,
    int? DaysUntilDeadline,
    BudgetDto? Budget,
    decimal RuleExtractionConfidence,
    bool IsReviewedByConsultant,
    IReadOnlyList<OpportunityRuleDto> Rules,
    IReadOnlyList<DocumentRequirementDto> DocumentChecklist);

public sealed record BudgetDto(decimal? MinAmount, decimal? MaxAmount, string Currency, decimal? SupportRate);

public sealed record OpportunityRuleDto(
    Guid Id,
    string Field,
    RuleOperator Operator,
    string Value,
    RuleDimension Dimension,
    RuleSeverity Severity,
    string HumanReadable,
    string? SourceExcerpt,
    decimal Confidence,
    bool IsManuallyOverridden);

public sealed record DocumentRequirementDto(string Code, string Name, bool IsMandatory, string? IssuingAuthority, string? Notes);

/// <summary>Fırsatın elle veya worker tarafından oluşturulması/güncellenmesi.</summary>
public sealed record UpsertOpportunityRequest
{
    public required Guid SourceId { get; init; }

    public Guid? SourceDocumentId { get; init; }

    public required SourceType SourceType { get; init; }

    public required SupportCategory SupportCategory { get; init; }

    public required string Title { get; init; }

    public required string Publisher { get; init; }

    public required DateTimeOffset PublishedAt { get; init; }

    public string? Summary { get; init; }

    public string? SourceUrl { get; init; }

    public DateTimeOffset? Deadline { get; init; }

    public BudgetDto? Budget { get; init; }

    public decimal RuleExtractionConfidence { get; init; } = 1m;

    public IReadOnlyList<UpsertRuleDto> Rules { get; init; } = [];

    public IReadOnlyList<DocumentRequirementDto> DocumentChecklist { get; init; } = [];
}

public sealed record UpsertRuleDto(
    string Field,
    RuleOperator Operator,
    string Value,
    RuleDimension Dimension,
    RuleSeverity Severity,
    string HumanReadable,
    string? SourceExcerpt,
    decimal Confidence);

/// <summary>Danışmanın tek bir kuralı elle düzeltmesi (istisna yönetimi).</summary>
public sealed record OverrideRuleRequest(
    RuleOperator Operator,
    string Value,
    RuleSeverity Severity,
    string HumanReadable);
