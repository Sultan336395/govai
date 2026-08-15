namespace GovAI.Api.Infrastructure;

/// <summary>
/// Yetkilendirme politikası adları. Rol listeleri <c>Program.cs</c> içinde tek yerde tanımlanır;
/// controller'lar yalnızca bu sabitleri kullanır.
/// </summary>
public static class Policies
{
    /// <summary>Kiracı yönetimi, kullanıcı açma, kaynak tanımlama gibi sistem işleri.</summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>Firma kartı oluşturma/güncelleme, ERP eşitleme.</summary>
    public const string ManageCompany = "ManageCompany";

    /// <summary>Skor tetikleme, senaryo çalıştırma, kural düzeltme gibi operasyonel işler.</summary>
    public const string Operate = "Operate";

    /// <summary>Salt okuyucu dahil tüm oturum açmış kullanıcılar.</summary>
    public const string Read = "Read";
}
