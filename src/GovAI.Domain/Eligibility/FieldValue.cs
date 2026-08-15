namespace GovAI.Domain.Eligibility;

public enum FieldValueKind
{
    /// <summary>Firma verisinde bu alan yok veya doldurulmamış — kural "Unknown" ile sonuçlanır.</summary>
    Unknown = 0,
    Number = 1,
    Boolean = 2,
    Text = 3,
    /// <summary>Çoklu değer (NACE kodları, sertifikalar, iller gibi).</summary>
    Set = 4
}

/// <summary>
/// Kural motorunun firma profilinden okuduğu tek bir alan değeri.
/// Tip bilgisini taşır ki operatör uyumsuzluğu sessizce yanlış sonuç üretmesin.
/// </summary>
public readonly record struct FieldValue
{
    private FieldValue(FieldValueKind kind, decimal? number, bool? boolean, string? text, IReadOnlySet<string>? set)
    {
        Kind = kind;
        Number = number;
        Boolean = boolean;
        Text = text;
        Set = set;
    }

    public FieldValueKind Kind { get; }

    public decimal? Number { get; }

    public bool? Boolean { get; }

    public string? Text { get; }

    public IReadOnlySet<string>? Set { get; }

    public bool IsKnown => Kind != FieldValueKind.Unknown;

    public static FieldValue Unknown() => new(FieldValueKind.Unknown, null, null, null, null);

    public static FieldValue FromNumber(decimal value) => new(FieldValueKind.Number, value, null, null, null);

    public static FieldValue FromNumber(decimal? value) =>
        value is null ? Unknown() : FromNumber(value.Value);

    public static FieldValue FromBoolean(bool value) => new(FieldValueKind.Boolean, null, value, null, null);

    public static FieldValue FromText(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Unknown()
            : new FieldValue(FieldValueKind.Text, null, null, value.Trim(), null);

    public static FieldValue FromSet(IEnumerable<string> values)
    {
        var set = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim().ToUpperInvariant())
            .ToHashSet();

        return new FieldValue(FieldValueKind.Set, null, null, null, set);
    }

    /// <summary>Raporlarda ve açıklama metinlerinde gösterilecek okunabilir hâli.</summary>
    public string Display() => Kind switch
    {
        FieldValueKind.Number => Number!.Value.ToString("0.####"),
        FieldValueKind.Boolean => Boolean!.Value ? "evet" : "hayır",
        FieldValueKind.Text => Text!,
        FieldValueKind.Set => Set!.Count == 0 ? "(boş)" : string.Join(", ", Set!.Order()),
        _ => "(veri yok)"
    };
}
