using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders.POC;

/// <summary>
/// GroupedDataLoader for batching Product lookups by CategoryId.
/// Returns multiple products per category (one-to-many relationship).
/// </summary>
public class ProductsByCategoryIdDataLoader : GroupedDataLoader<int, Product>
{
    private readonly DataStore _dataStore;

    public ProductsByCategoryIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<int, Product>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var products = _dataStore.Products
            .Where(p => keys.Contains(p.CategoryId))
            .ToLookup(p => p.CategoryId);

        return Task.FromResult(products);
    }
}
