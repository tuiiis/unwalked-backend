using Unwalked.Application.Abstractions;

namespace Unwalked.UnitTests.Fakes;

public sealed class DictionaryCategoryRepository(IReadOnlyDictionary<string, int> idsByCode) : ICategoryRepository
{
    public Task<int?> FindIdByCodeAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(idsByCode.TryGetValue(code, out var id) ? id : (int?)null);
}
