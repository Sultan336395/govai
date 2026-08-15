using GovAI.Application.Abstractions.Services;
using GovAI.Domain.Assessments;
using GovAI.Domain.Auditing;
using GovAI.Domain.Common;
using GovAI.Domain.Companies;
using GovAI.Domain.Identity;
using GovAI.Domain.Notifications;
using GovAI.Domain.Opportunities;
using GovAI.Domain.Sources;
using Microsoft.EntityFrameworkCore;

namespace GovAI.Persistence;

/// <summary>
/// PostgreSQL veri bağlamı. Şema adı <c>govai</c>'dir; böylece aynı veritabanında
/// başka uygulamalarla çakışma olmadan barınabilir.
/// </summary>
public class GovAiDbContext(
    DbContextOptions<GovAiDbContext> options,
    IDateTimeProvider? clock = null,
    ICurrentUser? currentUser = null) : DbContext(options)
{
    public const string Schema = "govai";

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<SourceDocument> SourceDocuments => Set<SourceDocument>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<EligibilityAssessment> Assessments => Set<EligibilityAssessment>();
    public DbSet<ScenarioSimulation> ScenarioSimulations => Set<ScenarioSimulation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GovAiDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    /// <summary>
    /// Oluşturma/güncelleme damgalarını ve soft-delete işaretlerini merkezî olarak uygular;
    /// böylece her serviste tekrar edilmez ve unutulamaz.
    /// </summary>
    private void ApplyAuditInformation()
    {
        var now = clock?.UtcNow ?? DateTimeOffset.UtcNow;
        var actor = currentUser?.Email ?? currentUser?.UserId?.ToString() ?? "system";

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = entry.Entity.CreatedAt == default ? now : entry.Entity.CreatedAt;
                    entry.Entity.CreatedBy ??= actor;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = actor;
                    break;
            }
        }

        // Silme istekleri fiziksel silme yerine pasifleştirmeye çevrilir (KVKK ve izlenebilirlik).
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = now;
        }
    }
}
