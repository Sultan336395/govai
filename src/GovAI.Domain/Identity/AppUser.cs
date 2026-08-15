using GovAI.Domain.Common;

namespace GovAI.Domain.Identity;

/// <summary>
/// Platform kullanıcısı. Kimlik doğrulama JWT ile yapılır; kurumsal kurulumda SSO
/// (OIDC) devreye alındığında <see cref="ExternalSubjectId"/> doldurulur ve parola alanı kullanılmaz.
/// </summary>
public class AppUser : AggregateRoot, IAuditable, ISoftDeletable, ITenantScoped
{
    private AppUser()
    {
    }

    public AppUser(Guid tenantId, string email, string fullName, UserRole role)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(email), "E-posta zorunludur.");
        DomainException.ThrowIf(!email.Contains('@'), "Geçerli bir e-posta adresi giriniz.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(fullName), "Ad soyad zorunludur.");

        TenantId = tenantId;
        Email = email.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        Role = role;
        IsActive = true;
    }

    public Guid TenantId { get; set; }

    public string Email { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    /// <summary>PBKDF2 türetilmiş parola özeti. SSO kullanıcılarında null'dır.</summary>
    public string? PasswordHash { get; private set; }

    /// <summary>Kurumsal SSO sağlayıcısındaki kullanıcı kimliği (OIDC <c>sub</c>).</summary>
    public string? ExternalSubjectId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    /// <summary>Ardışık başarısız giriş sayısı; eşiği aşarsa hesap geçici olarak kilitlenir.</summary>
    public int FailedLoginCount { get; private set; }

    public DateTimeOffset? LockedUntil { get; private set; }

    /// <summary>Danışman rolündeki kullanıcının erişebileceği firmalar; boşsa kiracıdaki tüm firmalar.</summary>
    public string? ScopedCompanyIdsJson { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    private const int MaxFailedLogins = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public bool IsLockedOut(DateTimeOffset asOf) => LockedUntil is not null && LockedUntil > asOf;

    public void SetPasswordHash(string passwordHash)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(passwordHash), "Parola özeti boş olamaz.");
        PasswordHash = passwordHash;
        ExternalSubjectId = null;
    }

    public void LinkExternalIdentity(string subjectId)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(subjectId), "SSO kimliği boş olamaz.");
        ExternalSubjectId = subjectId;
        PasswordHash = null;
    }

    public void ChangeRole(UserRole role) => Role = role;

    public void RecordSuccessfulLogin(DateTimeOffset at)
    {
        LastLoginAt = at;
        FailedLoginCount = 0;
        LockedUntil = null;
    }

    public void RecordFailedLogin(DateTimeOffset at)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= MaxFailedLogins)
        {
            LockedUntil = at.Add(LockoutDuration);
            FailedLoginCount = 0;
        }
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void RestrictToCompanies(string? scopedCompanyIdsJson) => ScopedCompanyIdsJson = scopedCompanyIdsJson;
}
