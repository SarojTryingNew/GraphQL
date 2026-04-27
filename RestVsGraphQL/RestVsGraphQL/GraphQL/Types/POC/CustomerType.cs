using RestVsGraphQL.GraphQL.DataLoaders.POC;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types.POC;

/// <summary>
/// Type extension for Customer that adds field resolvers for navigation properties.
/// </summary>
[ObjectType<Customer>]
public static class CustomerType
{
    /// <summary>
    /// Resolver for the Orders navigation property.
    /// Uses DataLoader to batch-load all orders for requested customers.
    /// </summary>
    public static async Task<IEnumerable<Order>> GetOrdersAsync(
        [Parent] Customer customer,
        OrdersByCustomerIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(customer.Id, cancellationToken);
}
