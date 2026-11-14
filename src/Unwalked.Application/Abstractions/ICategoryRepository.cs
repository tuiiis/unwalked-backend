namespace Unwalked.Application.Abstractions;

public interface ICategoryRepository
{
    Task<int?> FindIdByCodeAsync(string code, CancellationToken cancellationToken);
}
