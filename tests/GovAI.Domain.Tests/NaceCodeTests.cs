using GovAI.Domain.Companies;

namespace GovAI.Domain.Tests;

/// <summary>
/// NACE eşleşmesi ürünün en sık yanlış yapılan yeridir: çağrı ana grubu ("25") verirken
/// firma tam kodu ("25.62.01") tutar. Eşleşme bu yüzden dereceli önek karşılaştırmasıdır.
/// </summary>
public class NaceCodeTests
{
    [Theory]
    [InlineData("2562", "2562", 1.0)]
    [InlineData("25.62", "2562", 1.0)]
    [InlineData("256201", "2562", 0.95)]
    [InlineData("2562", "256", 0.85)]
    [InlineData("2562", "25", 0.75)]
    [InlineData("2562", "62", 0.0)]
    public void Onek_eslesmesi_derecelendirilir(string companyCode, string requiredCode, double expected)
    {
        var strength = NaceCode.MatchStrength(companyCode, requiredCode);
        Assert.Equal((decimal)expected, strength);
    }

    [Fact]
    public void Cagri_sektor_kisiti_koymuyorsa_herkes_uygundur()
    {
        Assert.Equal(1m, NaceCode.BestMatch(["6201"], []));
    }

    [Fact]
    public void Firmanin_kodlarindan_en_iyi_eslesme_secilir()
    {
        var strength = NaceCode.BestMatch(["4711", "2562"], ["25", "26"]);
        Assert.Equal(0.75m, strength);
    }

    [Fact]
    public void Noktalar_ve_bosluklar_normalize_edilir()
    {
        Assert.Equal("256201", NaceCode.Normalize(" 25.62.01 "));
    }
}
