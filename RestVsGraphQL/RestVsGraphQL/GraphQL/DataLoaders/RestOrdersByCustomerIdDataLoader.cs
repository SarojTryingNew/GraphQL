using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// GroupedDataLoader that fetches orders by customer IDs via REST API.
/// </summary>
public class RestOrdersByCustomerIdDataLoader : GroupedDataLoader<int, Order>
{
    private readonly RestApiClient _restClient;

    public RestOrdersByCustomerIdDataLoader(
        RestApiClient restClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _restClient = restClient;
    }

    protected override async Task<ILookup<int, Order>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        return await _restClient.GetOrdersByCustomerIdsAsync(keys);
    }
}
