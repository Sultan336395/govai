using GovAI.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GovAI.Persistence;

public sealed class UnitOfWork(GovAiDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Birden çok aggregate'i tek atomik işlemde günceller (ör. yeniden skorlama + bildirim üretimi).
    /// EF Core'un execution strategy'si geçici bağlantı hatalarında işlemi yeniden dener.
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            await operation(ct);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, cancellationToken);
    }
}
