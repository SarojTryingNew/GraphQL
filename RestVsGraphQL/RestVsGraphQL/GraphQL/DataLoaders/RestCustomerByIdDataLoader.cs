using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// DataLoader that fetches customers via REST API instead of directly from DataStore.
/// Implements the "GraphQL Gateway on top of REST" pattern.
/// Uses batch endpoint for optimal performance.
/// </summary>
public class RestCustomerByIdDataLoader : BatchDataLoader<int, Customer>
{
    private readonly RestApiClient _restClient;

    public RestCustomerByIdDataLoader(
        RestApiClient restClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _restClient = restClient;
    }

    protected override async Task<IReadOnlyDictionary<int, Customer>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        // Calls REST batch endpoint: GET /api/customers/batch?ids=1,2,3,4,5
        return await _restClient.GetCustomersByIdsAsync(keys);
    }
}
