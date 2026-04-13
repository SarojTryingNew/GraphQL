using RestVsGraphQL.GraphQL.DataLoaders;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types;

/// <summary>
/// Type extension for Order using REST-based DataLoaders.
/// Implements GraphQL Gateway pattern.
/// </summary>
[ObjectType<Order>]
public static class RestOrderType
{
    public static async Task<Customer?> GetCustomerAsync(
        [Parent] Order order,
        RestCustomerByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(order.CustomerId, cancellationToken);

    public static async Task<IEnumerable<OrderItem>> GetItemsAsync(
        [Parent] Order order,
        RestOrderItemsByOrderIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(order.Id, cancellationToken);
}
