using RestVsGraphQL.GraphQL.DataLoaders;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types;

/// <summary>
/// Type extension for Category that adds field resolvers for navigation properties.
/// </summary>
[ObjectType<Category>]
public static class CategoryType
{
    /// <summary>
    /// Resolver for the Products navigation property.
    /// Uses DataLoader to batch-load all products for requested categories.
    /// </summary>
    public static async Task<IEnumerable<Product>> GetProductsAsync(
        [Parent] Category category,
        ProductsByCategoryIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(category.Id, cancellationToken);
}
