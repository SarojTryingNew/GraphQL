using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders;

/// <summary>
/// DataLoader that fetches categories via REST API batch endpoint.
/// </summary>
public class RestCategoryByIdDataLoader : BatchDataLoader<int, Category>
{
    private readonly RestApiClient _restClient;

    public RestCategoryByIdDataLoader(
        RestApiClient restClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _restClient = restClient;
    }

    protected override async Task<IReadOnlyDictionary<int, Category>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        return await _restClient.GetCategoriesByIdsAsync(keys);
    }
}
