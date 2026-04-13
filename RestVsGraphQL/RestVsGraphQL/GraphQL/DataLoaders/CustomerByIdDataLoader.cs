using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// DataLoader for batching Customer lookups by ID.
/// Solves N+1 query problems when loading customers for multiple orders.
/// </summary>
public class CustomerByIdDataLoader : BatchDataLoader<int, Customer>
{
    private readonly DataStore _dataStore;

    public CustomerByIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<int, Customer>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var customers = _dataStore.Customers
            .Where(c => keys.Contains(c.Id))
            .ToDictionary(c => c.Id);

        return Task.FromResult<IReadOnlyDictionary<int, Customer>>(customers);
    }
}
