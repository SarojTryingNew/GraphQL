using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// GroupedDataLoader for batching OrderItemNote lookups by OrderItemId.
/// Returns multiple notes per key (one-to-many relationship).
/// </summary>
public class OrderItemNotesByOrderItemIdDataLoader : GroupedDataLoader<int, OrderItemNote>
{
    private readonly DataStore _dataStore;

    public OrderItemNotesByOrderItemIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<int, OrderItemNote>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var notes = _dataStore.OrderItemNotes
            .Where(n => keys.Contains(n.OrderItemId))
            .ToLookup(n => n.OrderItemId);

        return Task.FromResult(notes);
    }
}
