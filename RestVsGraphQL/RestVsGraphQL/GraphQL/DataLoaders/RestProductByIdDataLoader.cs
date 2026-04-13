using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// DataLoader that fetches products via REST API batch endpoint.
/// </summary>
public class RestProductByIdDataLoader : BatchDataLoader<int, Product>
{
    private readonly RestApiClient _restClient;

    public RestProductByIdDataLoader(
        RestApiClient restClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _restClient = restClient;
    }

    protected override async Task<IReadOnlyDictionary<int, Product>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        return await _restClient.GetProductsByIdsAsync(keys);
    }
}
