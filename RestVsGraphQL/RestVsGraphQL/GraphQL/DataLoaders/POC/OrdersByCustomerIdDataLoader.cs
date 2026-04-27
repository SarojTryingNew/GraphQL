using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders.POC;

/// <summary>
/// GroupedDataLoader for batching Order lookups by CustomerId.
/// Returns multiple orders per customer (one-to-many relationship).
/// </summary>
public class OrdersByCustomerIdDataLoader : GroupedDataLoader<int, Order>
{
    private readonly DataStore _dataStore;

    public OrdersByCustomerIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<int, Order>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var orders = _dataStore.Orders
            .Where(o => keys.Contains(o.CustomerId))
            .ToLookup(o => o.CustomerId);

        return Task.FromResult(orders);
    }
}
