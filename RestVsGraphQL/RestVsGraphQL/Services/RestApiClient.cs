using System.Text;
using System.Text.Json;
using RestVsGraphQL.Models;
using RestVsGraphQL.DTOs;

namespace RestVsGraphQL.Services;

/// <summary>
/// Client service that calls REST APIs.
/// GraphQL Gateway uses this to delegate to existing REST endpoints.
/// This implements the "GraphQL on top of REST" pattern.
/// </summary>
public class RestApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public RestApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["RestApi:BaseUrl"] ?? "http://localhost:5072/api";
    }

    // Orders API calls
    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/orders");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Order>>() 
            ?? Array.Empty<Order>();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/orders/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Order>();
    }

    public async Task<Order> CreateOrderAsync(OrderCreateDto orderDto)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(orderDto),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/orders", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Order>() 
            ?? throw new Exception("Failed to create order");
    }

    // Customers API calls
    public async Task<IEnumerable<Customer>> GetCustomersAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/customers");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Customer>>() 
            ?? Array.Empty<Customer>();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/customers/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Customer>();
    }

    // Products API calls
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/products");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Product>>() 
            ?? Array.Empty<Product>();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/products/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>();
    }

    // Dashboard API call
    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/dashboard");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DashboardViewModel>() 
            ?? throw new Exception("Failed to get dashboard");
    }

    // Categories API calls
    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/categories");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Category>>() 
            ?? Array.Empty<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/categories/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>();
    }

    // Bulk operations
    public async Task<BulkOperationResult> BulkCreateOrdersAsync(BulkOrderCreateRequest request)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/orders/bulk", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BulkOperationResult>() 
            ?? throw new Exception("Failed to bulk create orders");
    }

    public async Task<BulkOperationResult> BulkUpdateOrdersAsync(BulkOrderUpdateRequest request)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PutAsync($"{_baseUrl}/orders/bulk", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BulkOperationResult>() 
            ?? throw new Exception("Failed to bulk update orders");
    }

    public async Task<BulkOperationResult> BulkDeleteOrdersAsync(BulkOrderDeleteRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{_baseUrl}/orders/bulk"),
            Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BulkOperationResult>() 
            ?? throw new Exception("Failed to bulk delete orders");
    }

    // Batch endpoints for DataLoader efficiency
    public async Task<IReadOnlyDictionary<int, Customer>> GetCustomersByIdsAsync(IEnumerable<int> ids)
    {
        var idsParam = string.Join(",", ids);
        var response = await _httpClient.GetAsync($"{_baseUrl}/customers/batch?ids={idsParam}");
        response.EnsureSuccessStatusCode();
        var customers = await response.Content.ReadFromJsonAsync<IEnumerable<Customer>>() 
            ?? Array.Empty<Customer>();
        return customers.ToDictionary(c => c.Id);
    }

    public async Task<IReadOnlyDictionary<int, Product>> GetProductsByIdsAsync(IEnumerable<int> ids)
    {
        var idsParam = string.Join(",", ids);
        var response = await _httpClient.GetAsync($"{_baseUrl}/products/batch?ids={idsParam}");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>() 
            ?? Array.Empty<Product>();
        return products.ToDictionary(p => p.Id);
    }

    public async Task<IReadOnlyDictionary<int, Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids)
    {
        var idsParam = string.Join(",", ids);
        var response = await _httpClient.GetAsync($"{_baseUrl}/categories/batch?ids={idsParam}");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<Category>>() 
            ?? Array.Empty<Category>();
        return categories.ToDictionary(c => c.Id);
    }

    public async Task<ILookup<int, Order>> GetOrdersByCustomerIdsAsync(IEnumerable<int> customerIds)
    {
        var idsParam = string.Join(",", customerIds);
        var response = await _httpClient.GetAsync($"{_baseUrl}/orders/by-customers?customerIds={idsParam}");
        response.EnsureSuccessStatusCode();
        var orders = await response.Content.ReadFromJsonAsync<IEnumerable<Order>>() 
            ?? Array.Empty<Order>();
        return orders.ToLookup(o => o.CustomerId);
    }

    public async Task<ILookup<int, OrderItem>> GetOrderItemsByOrderIdsAsync(IEnumerable<int> orderIds)
    {
        var idsParam = string.Join(",", orderIds);
        var response = await _httpClient.GetAsync($"{_baseUrl}/orderitems/by-orders?orderIds={idsParam}");
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<IEnumerable<OrderItem>>() 
            ?? Array.Empty<OrderItem>();
        return items.ToLookup(oi => oi.OrderId);
    }

    public async Task<ILookup<int, Product>> GetProductsByCategoryIdsAsync(IEnumerable<int> categoryIds)
    {
        var idsParam = string.Join(",", categoryIds);
        var response = await _httpClient.GetAsync($"{_baseUrl}/products/by-categories?categoryIds={idsParam}");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>() 
            ?? Array.Empty<Product>();
        return products.ToLookup(p => p.CategoryId);
    }
}
