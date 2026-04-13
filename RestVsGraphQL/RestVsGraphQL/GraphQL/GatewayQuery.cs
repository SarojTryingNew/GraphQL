using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL;

/// <summary>
/// GraphQL Gateway Query - Implements "GraphQL on top of REST" pattern.
/// Instead of accessing DataStore directly, this calls existing REST APIs.
/// This allows GraphQL to act as a facade over microservices/REST endpoints.
/// </summary>
public class GatewayQuery
{
    /// <summary>
    /// Gets all customers by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<IEnumerable<Customer>> GetCustomers([Service] RestApiClient restClient)
        => await restClient.GetCustomersAsync();

    /// <summary>
    /// Gets a customer by ID by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<Customer?> GetCustomer(int id, [Service] RestApiClient restClient)
        => await restClient.GetCustomerByIdAsync(id);

    /// <summary>
    /// Gets all orders by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrders([Service] RestApiClient restClient)
        => await restClient.GetOrdersAsync();

    /// <summary>
    /// Gets an order by ID by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// Note: DataLoaders will still handle nested relations efficiently.
    /// </summary>
    public async Task<Order?> GetOrder(int id, [Service] RestApiClient restClient)
        => await restClient.GetOrderByIdAsync(id);

    /// <summary>
    /// Gets all products by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<IEnumerable<Product>> GetProducts([Service] RestApiClient restClient)
        => await restClient.GetProductsAsync();

    /// <summary>
    /// Gets a product by ID by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<Product?> GetProduct(int id, [Service] RestApiClient restClient)
        => await restClient.GetProductByIdAsync(id);

    /// <summary>
    /// Gets all categories by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// </summary>
    public async Task<IEnumerable<Category>> GetCategories([Service] RestApiClient restClient)
        => await restClient.GetCategoriesAsync();

    /// <summary>
    /// Gets dashboard data by calling the REST API.
    /// GraphQL Gateway → REST API → DataStore
    /// However, field resolvers will still provide selective fetching.
    /// </summary>
    public DashboardViewModel GetDashboard()
        => new DashboardViewModel();
    // Note: This still returns empty model because field resolvers handle individual fields
}
