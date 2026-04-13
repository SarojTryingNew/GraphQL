using RestVsGraphQL.GraphQL.DataLoaders;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types;

/// <summary>
/// Type extension for Category using REST-based DataLoaders.
/// </summary>
[ObjectType<Category>]
public static class RestCategoryType
{
    public static async Task<IEnumerable<Product>> GetProductsAsync(
        [Parent] Category category,
        RestProductsByCategoryIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(category.Id, cancellationToken);
}
