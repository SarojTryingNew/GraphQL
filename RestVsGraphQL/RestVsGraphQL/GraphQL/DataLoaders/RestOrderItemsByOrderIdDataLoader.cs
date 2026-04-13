using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// GroupedDataLoader that fetches order items via REST API batch endpoint.
/// </summary>
public class RestOrderItemsByOrderIdDataLoader : GroupedDataLoader<int, OrderItem>
{
    private readonly RestApiClient _restClient;

    public RestOrderItemsByOrderIdDataLoader(
        RestApiClient restClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _restClient = restClient;
    }

    protected override async Task<ILookup<int, OrderItem>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        return await _restClient.GetOrderItemsByOrderIdsAsync(keys);
    }
}
