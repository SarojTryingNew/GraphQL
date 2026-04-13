using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// GroupedDataLoader that fetches products by category IDs via REST API.
/// </summary>
public class RestProductsByCategoryIdDataLoader : GroupedDataLoader<int, Product>
{
    private readonly RestApiClient _restClient;

    public RestProductsByCategoryIdDataLoader(
        RestApiClient restClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _restClient = restClient;
    }

    protected override async Task<ILookup<int, Product>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        return await _restClient.GetProductsByCategoryIdsAsync(keys);
    }
}
