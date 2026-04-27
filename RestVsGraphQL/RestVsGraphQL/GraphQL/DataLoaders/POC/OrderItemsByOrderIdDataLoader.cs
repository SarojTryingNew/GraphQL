using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders.POC;

/// <summary>
/// GroupedDataLoader for batching OrderItem lookups by OrderId.
/// Returns multiple items per key (one-to-many relationship).
/// </summary>
public class OrderItemsByOrderIdDataLoader : GroupedDataLoader<int, OrderItem>
{
    private readonly DataStore _dataStore;

    public OrderItemsByOrderIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<int, OrderItem>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var orderItems = _dataStore.OrderItems
            .Where(oi => keys.Contains(oi.OrderId))
            .ToLookup(oi => oi.OrderId);

        return Task.FromResult(orderItems);
    }
}
