using HotChocolate.Execution;
using HotChocolate.Language;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;
using System.Text.Json;

namespace RestVsGraphQL.Services;

/// <summary>
/// Service to execute GraphQL queries and mutations internally (not via HTTP)
/// This allows REST APIs to use GraphQL as an internal data access layer
/// </summary>
public class GraphQLExecutorService
{
    private readonly IRequestExecutorResolver _executorResolver;

    public GraphQLExecutorService(IRequestExecutorResolver executorResolver)
    {
        _executorResolver = executorResolver;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        var executor = await _executorResolver.GetRequestExecutorAsync();

        var query = @"
            query {
                orders {
                    id
                    customerId
                    orderDate
                    status
                    totalAmount
                }
            }";

        var result = await executor.ExecuteAsync(query);
        var resultData = await GetResultDataAsync<JsonElement>(result);

        if (resultData.TryGetProperty("orders", out var ordersElement))
        {
            var ordersJson = ordersElement.GetRawText();
            var orders = JsonSerializer.Deserialize<List<Order>>(ordersJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return orders ?? Enumerable.Empty<Order>();
        }

        return Enumerable.Empty<Order>();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        var executor = await _executorResolver.GetRequestExecutorAsync();

        var query = $@"
            query {{
                order(id: {id}) {{
                    id
                    customerId
                    orderDate
                    status
                    totalAmount
                    customer {{
                        id
                        name
                        email
                    }}
                    items {{
                        id
                        productId
                        quantity
                        unitPrice
                        discount
                        product {{
                            id
                            name
                            price
                        }}
                    }}
                }}
            }}";

        var result = await executor.ExecuteAsync(query);
        var resultData = await GetResultDataAsync<JsonElement>(result);

        if (resultData.TryGetProperty("order", out var orderElement) && orderElement.ValueKind != JsonValueKind.Null)
        {
            var orderJson = orderElement.GetRawText();
            var order = JsonSerializer.Deserialize<Order>(orderJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return order;
        }

        return null;
    }

    public async Task<IEnumerable<Order>> GetOrdersByIdsAsync(List<int> ids)
    {
        var executor = await _executorResolver.GetRequestExecutorAsync();

        var idsString = string.Join(", ", ids);
        var query = $@"
            query {{
                ordersByIds(ids: [{idsString}]) {{
                    id
                    customerId
                    orderDate
                    status
                    totalAmount
                    customer {{
                        id
                        name
                        email
                    }}
                    items {{
                        id
                        productId
                        quantity
                        unitPrice
                        discount
                        product {{
                            id
                            name
                            price
                        }}
                    }}
                }}
            }}";

        var result = await executor.ExecuteAsync(query);
        var resultData = await GetResultDataAsync<JsonElement>(result);

        if (resultData.TryGetProperty("ordersByIds", out var ordersElement))
        {
            var ordersJson = ordersElement.GetRawText();
            var orders = JsonSerializer.Deserialize<List<Order>>(ordersJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return orders ?? Enumerable.Empty<Order>();
        }

        return Enumerable.Empty<Order>();
    }

    public async Task<BulkOperationResult> BulkCreateOrdersAsync(BulkOrderCreateRequest request)
    {
        var executor = await _executorResolver.GetRequestExecutorAsync();

        var mutation = @"
            mutation BulkCreateOrders($request: BulkOrderCreateRequestInput!) {
                bulkCreateOrders(request: $request) {
                    successCount
                    failureCount
                    errors
                    createdIds
                }
            }";

        var result = await executor.ExecuteAsync(
            OperationRequestBuilder.New()
                .SetDocument(mutation)
                .SetVariableValues(new Dictionary<string, object?> { { "request", request } })
                .Build());

        var resultData = await GetResultDataAsync<JsonElement>(result);

        if (resultData.TryGetProperty("bulkCreateOrders", out var bulkResultElement))
        {
            var bulkResultJson = bulkResultElement.GetRawText();
            var bulkResult = JsonSerializer.Deserialize<BulkOperationResult>(bulkResultJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return bulkResult ?? new BulkOperationResult { SuccessCount = 0, FailureCount = request.Orders.Count };
        }

        return new BulkOperationResult { SuccessCount = 0, FailureCount = request.Orders.Count };
    }

    public async Task<BulkOperationResult> BulkUpdateOrdersAsync(BulkOrderUpdateRequest request)
    {
        var executor = await _executorResolver.GetRequestExecutorAsync();

        var mutation = @"
            mutation BulkUpdateOrders($request: BulkOrderUpdateRequestInput!) {
                bulkUpdateOrders(request: $request) {
                    successCount
                    failureCount
                    errors
                }
            }";

        var result = await executor.ExecuteAsync(
            OperationRequestBuilder.New()
                .SetDocument(mutation)
                .SetVariableValues(new Dictionary<string, object?> { { "request", request } })
                .Build());

        var resultData = await GetResultDataAsync<JsonElement>(result);

        if (resultData.TryGetProperty("bulkUpdateOrders", out var bulkResultElement))
        {
            var bulkResultJson = bulkResultElement.GetRawText();
            var bulkResult = JsonSerializer.Deserialize<BulkOperationResult>(bulkResultJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return bulkResult ?? new BulkOperationResult { SuccessCount = 0, FailureCount = request.Orders.Count };
        }

        return new BulkOperationResult { SuccessCount = 0, FailureCount = request.Orders.Count };
    }

    public async Task<BulkOperationResult> BulkDeleteOrdersAsync(BulkOrderDeleteRequest request)
    {
        var executor = await _executorResolver.GetRequestExecutorAsync();

        var idsString = string.Join(", ", request.OrderIds);
        var mutation = $@"
            mutation {{
                bulkDeleteOrders(request: {{ orderIds: [{idsString}] }}) {{
                    successCount
                    failureCount
                    errors
                    deletedIds
                }}
            }}";

        var result = await executor.ExecuteAsync(mutation);
        var resultData = await GetResultDataAsync<JsonElement>(result);

        if (resultData.TryGetProperty("bulkDeleteOrders", out var bulkResultElement))
        {
            var bulkResultJson = bulkResultElement.GetRawText();
            var bulkResult = JsonSerializer.Deserialize<BulkOperationResult>(bulkResultJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return bulkResult ?? new BulkOperationResult { SuccessCount = 0, FailureCount = request.OrderIds.Count };
        }

        return new BulkOperationResult { SuccessCount = 0, FailureCount = request.OrderIds.Count };
    }

    private async Task<JsonElement> GetResultDataAsync<T>(IExecutionResult result)
    {
        var json = result.ToJson();
        var document = JsonDocument.Parse(json);

        if (document.RootElement.TryGetProperty("errors", out var errorsElement) && errorsElement.GetArrayLength() > 0)
        {
            var firstError = errorsElement[0];
            var errorMessage = firstError.TryGetProperty("message", out var msgElement) 
                ? msgElement.GetString() 
                : "Unknown error";
            throw new Exception($"GraphQL Error: {errorMessage}");
        }

        if (document.RootElement.TryGetProperty("data", out var dataElement))
        {
            return dataElement;
        }

        throw new Exception("Failed to execute GraphQL query");
    }
}
