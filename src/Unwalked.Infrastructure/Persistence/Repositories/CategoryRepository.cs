using Microsoft.EntityFrameworkCore;
using Unwalked.Application.Abstractions;

namespace Unwalked.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of category lookups.
/// </summary>
public sealed class CategoryRepository(UnwalkedDbContext db) : ICategoryRepository
{
    public async Task<int?> FindIdByCodeAsync(string code, CancellationToken cancellationToken) =>
        await db.Categories
            .Where(category => category.Code == code)
            .Select(category => (int?)category.Id)
            .SingleOrDefaultAsync(cancellationToken);
}
