namespace GovAI.Domain.Scoring;

/// <summary>
/// <c>timingScore</c> hesabı. Amaç yalnızca "süre var mı" değil, "hazırlık için makul süre var mı".
/// Çok yakın son tarih hazırlık riskini, çok uzak son tarih ise aksiyon aciliyetinin düşüklüğünü yansıtır.
/// </summary>
public static class TimingScoreCalculator
{
    /// <summary>Son başvuru tarihi bilinmiyorsa (sürekli açık çağrı) uygulanan nötr puan.</summary>
    public const decimal NoDeadlineScore = 0.7m;

    public static (decimal Score, string Rationale) Calculate(DateTimeOffset? deadline, DateTimeOffset asOf)
    {
        if (deadline is null)
        {
            return (NoDeadlineScore, "Son başvuru tarihi belirtilmemiş (sürekli açık çağrı).");
        }

        var days = (int)Math.Ceiling((deadline.Value - asOf).TotalDays);

        return days switch
        {
            < 0 => (0m, $"Başvuru süresi {Math.Abs(days)} gün önce doldu."),
            0 => (0.10m, "Son başvuru bugün; hazırlık için süre yok."),
            <= 7 => (0.35m, $"Son başvuruya {days} gün kaldı; hazırlık süresi çok kısıtlı."),
            <= 14 => (0.60m, $"Son başvuruya {days} gün kaldı; hızlı hareket edilmeli."),
            <= 30 => (0.85m, $"Son başvuruya {days} gün kaldı; hazırlık için yeterli süre var."),
            <= 90 => (1.00m, $"Son başvuruya {days} gün kaldı; ideal hazırlık aralığı."),
            _ => (0.80m, $"Son başvuruya {days} gün var; aciliyet düşük, takibe alınabilir.")
        };
    }
}
