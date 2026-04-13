using RestVsGraphQL.GraphQL.DataLoaders;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.GraphQL.Types;

/// <summary>
/// Type extension for Customer using REST-based DataLoaders.
/// </summary>
[ObjectType<Customer>]
public static class RestCustomerType
{
    public static async Task<IEnumerable<Order>> GetOrdersAsync(
        [Parent] Customer customer,
        RestOrdersByCustomerIdDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(customer.Id, cancellationToken);
}
