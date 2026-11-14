using Microsoft.EntityFrameworkCore;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Users;

namespace Unwalked.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(UnwalkedDbContext db) : IUserRepository
{
    public Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

    public async Task<bool> TryAddAsync(UserAccount user, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            db.Entry(user).State = EntityState.Detached;
            return false;
        }
    }
}
