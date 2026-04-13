using RestVsGraphQL.GraphQL.DataLoaders;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types;

/// <summary>
/// Type extension for OrderItem using REST-based DataLoaders.
/// </summary>
[ObjectType<OrderItem>]
public static class RestOrderItemType
{
    public static async Task<Product?> GetProductAsync(
        [Parent] OrderItem orderItem,
        RestProductByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(orderItem.ProductId, cancellationToken);

    // Note: OrderItemNotes don't have a REST endpoint yet
    // Using the existing DataStore-based loader for now
    public static async Task<IEnumerable<OrderItemNote>> GetNotesAsync(
        [Parent] OrderItem orderItem,
        OrderItemNotesByOrderItemIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(orderItem.Id, cancellationToken);
}
