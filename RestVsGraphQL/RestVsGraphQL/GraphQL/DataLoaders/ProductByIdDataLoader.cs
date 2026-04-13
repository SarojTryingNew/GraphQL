using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// DataLoader for batching Product lookups by ID.
/// Solves N+1 query problems when loading products for multiple order items.
/// </summary>
public class ProductByIdDataLoader : BatchDataLoader<int, Product>
{
    private readonly DataStore _dataStore;

    public ProductByIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<int, Product>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var products = _dataStore.Products
            .Where(p => keys.Contains(p.Id))
            .ToDictionary(p => p.Id);

        return Task.FromResult<IReadOnlyDictionary<int, Product>>(products);
    }
}
