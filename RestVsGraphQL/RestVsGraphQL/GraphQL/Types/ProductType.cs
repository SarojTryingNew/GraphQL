using RestVsGraphQL.GraphQL.DataLoaders;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types;

/// <summary>
/// Type extension for Product that adds field resolvers for navigation properties.
/// </summary>
[ObjectType<Product>]
public static class ProductType
{
    /// <summary>
    /// Resolver for the Category navigation property.
    /// Uses DataLoader for efficient batching.
    /// </summary>
    public static async Task<Category?> GetCategoryAsync(
        [Parent] Product product,
        CategoryByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(product.CategoryId, cancellationToken);
}
