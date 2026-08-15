namespace GovAI.Domain.Companies;

/// <summary>
/// NACE kodu karşılaştırma yardımcıları.
/// Resmî çağrılar çoğu zaman ana grubu ("62") verirken firma verisi tam kodu ("62.01.01") taşır;
/// bu nedenle eşleşme önek (prefix) mantığıyla, kaç hane tuttuğuna göre derecelendirilerek yapılır.
/// </summary>
public static class NaceCode
{
    /// <summary>Noktaları ve boşlukları temizler, büyük harfe çevirir.</summary>
    public static string Normalize(string code) =>
        new(code.Where(char.IsLetterOrDigit).ToArray());

    /// <summary>
    /// Firma kodunun çağrı kodunu karşılayıp karşılamadığını ve eşleşmenin ne kadar kesin olduğunu döner.
    /// 1.0 tam eşleşme, 0.0 hiç eşleşme yok.
    /// </summary>
    public static decimal MatchStrength(string companyCode, string requiredCode)
    {
        var company = Normalize(companyCode);
        var required = Normalize(requiredCode);

        if (company.Length == 0 || required.Length == 0)
        {
            return 0m;
        }

        if (company == required)
        {
            return 1m;
        }

        // Çağrı ana grubu veriyorsa (kısa kod) firma kodu onun altında olmalıdır.
        if (company.StartsWith(required, StringComparison.Ordinal))
        {
            return required.Length switch
            {
                >= 4 => 0.95m,
                3 => 0.85m,
                2 => 0.75m,
                _ => 0.5m
            };
        }

        // Firma daha genel kod tutuyorsa eşleşme zayıftır ama tamamen elenmez.
        if (required.StartsWith(company, StringComparison.Ordinal))
        {
            return 0.6m;
        }

        return 0m;
    }

    /// <summary>Firmanın kodlarından herhangi biri çağrının kabul ettiği kodlarla en iyi hangi güçte eşleşiyor?</summary>
    public static decimal BestMatch(IEnumerable<string> companyCodes, IEnumerable<string> requiredCodes)
    {
        var required = requiredCodes.ToArray();
        if (required.Length == 0)
        {
            // Çağrı sektör kısıtı koymuyorsa herkes uygundur.
            return 1m;
        }

        var best = 0m;
        foreach (var companyCode in companyCodes)
        {
            foreach (var requiredCode in required)
            {
                var strength = MatchStrength(companyCode, requiredCode);
                if (strength > best)
                {
                    best = strength;
                }

                if (best == 1m)
                {
                    return best;
                }
            }
        }

        return best;
    }
}
