using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL;

/// <summary>
/// GraphQL Gateway Mutation - Implements "GraphQL on top of REST" pattern.
/// Instead of accessing DataStore directly, this calls existing REST APIs.
/// </summary>
public class GatewayMutation
{
    /// <summary>
    /// Creates an order by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<Order> CreateOrder(
        OrderCreateDto orderDto, 
        [Service] RestApiClient restClient)
    {
        return await restClient.CreateOrderAsync(orderDto);
    }

    /// <summary>
    /// Bulk creates orders by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<BulkOperationResult> BulkCreateOrders(
        BulkOrderCreateRequest request,
        [Service] RestApiClient restClient)
    {
        return await restClient.BulkCreateOrdersAsync(request);
    }

    /// <summary>
    /// Bulk updates orders by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<BulkOperationResult> BulkUpdateOrders(
        BulkOrderUpdateRequest request,
        [Service] RestApiClient restClient)
    {
        return await restClient.BulkUpdateOrdersAsync(request);
    }

    /// <summary>
    /// Bulk deletes orders by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<BulkOperationResult> BulkDeleteOrders(
        BulkOrderDeleteRequest request,
        [Service] RestApiClient restClient)
    {
        return await restClient.BulkDeleteOrdersAsync(request);
    }
}
