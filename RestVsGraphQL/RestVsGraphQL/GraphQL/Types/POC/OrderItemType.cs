using RestVsGraphQL.GraphQL.DataLoaders.POC;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types.POC;

/// <summary>
/// Type extension for OrderItem that adds field resolvers for navigation properties.
/// </summary>
[ObjectType<OrderItem>]
public static class OrderItemType
{
    /// <summary>
    /// Resolver for the Product navigation property.
    /// Uses DataLoader for efficient batching.
    /// </summary>
    public static async Task<Product?> GetProductAsync(
        [Parent] OrderItem orderItem,
        ProductByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(orderItem.ProductId, cancellationToken);

    /// <summary>
    /// Resolver for the Notes navigation property.
    /// Uses DataLoader to batch-load all notes for requested order items.
    /// </summary>
    public static async Task<IEnumerable<OrderItemNote>> GetNotesAsync(
        [Parent] OrderItem orderItem,
        OrderItemNotesByOrderItemIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(orderItem.Id, cancellationToken);
}
