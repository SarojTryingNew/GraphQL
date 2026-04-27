using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders.POC;

/// <summary>
/// DataLoader for batching Category lookups by ID.
/// Solves N+1 query problems when loading categories for multiple products.
/// </summary>
public class CategoryByIdDataLoader : BatchDataLoader<int, Category>
{
    private readonly DataStore _dataStore;

    public CategoryByIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<int, Category>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var categories = _dataStore.Categories
            .Where(c => keys.Contains(c.Id))
            .ToDictionary(c => c.Id);

        return Task.FromResult<IReadOnlyDictionary<int, Category>>(categories);
    }
}
