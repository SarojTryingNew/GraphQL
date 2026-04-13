using RestVsGraphQL.GraphQL.DataLoaders;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types;

/// <summary>
/// Type extension for Product using REST-based DataLoaders.
/// </summary>
[ObjectType<Product>]
public static class RestProductType
{
    public static async Task<Category?> GetCategoryAsync(
        [Parent] Product product,
        RestCategoryByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(product.CategoryId, cancellationToken);
}
