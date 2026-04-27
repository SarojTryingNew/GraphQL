using RestVsGraphQL.GraphQL.DataLoaders.POC;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types.POC;

/// <summary>
/// Type extension for Order that adds field resolvers for navigation properties.
/// HotChocolate will only call these resolvers when the client requests these fields.
/// </summary>
[ObjectType<Order>]
public static class OrderType
{
    /// <summary>
    /// Resolver for the Customer navigation property.
    /// Uses DataLoader for efficient batching - prevents N+1 queries.
    /// </summary>
    public static async Task<Customer?> GetCustomerAsync(
        [Parent] Order order,
        CustomerByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(order.CustomerId, cancellationToken);

    /// <summary>
    /// Resolver for the Items navigation property.
    /// Uses DataLoader to batch-load all items for requested orders.
    /// </summary>
    public static async Task<IEnumerable<OrderItem>> GetItemsAsync(
        [Parent] Order order,
        OrderItemsByOrderIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(order.Id, cancellationToken);
}
