using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL;

public class Query
{
    public IEnumerable<Customer> GetCustomers([Service] DataStore dataStore)
        => dataStore.Customers;

    public Customer? GetCustomer(int id, [Service] DataStore dataStore)
        => dataStore.Customers.FirstOrDefault(c => c.Id == id);

    public IEnumerable<Order> GetOrders([Service] DataStore dataStore)
        => dataStore.Orders;

    public Order? GetOrder(int id, [Service] DataStore dataStore)
    {
        var order = dataStore.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null) return null;

        // Use extension method for efficient relation loading
        order.LoadRelations(dataStore);

        return order;
    }

    public IEnumerable<Order> GetOrdersByIds(List<int> ids, [Service] DataStore dataStore)
    {
        var orders = dataStore.Orders.Where(o => ids.Contains(o.Id)).ToList();

        // Use extension method for efficient batch relation loading
        orders.LoadRelations(dataStore);

        return orders;
    }

    public IEnumerable<Product> GetProducts([Service] DataStore dataStore)
        => dataStore.Products;

    public Product? GetProduct(int id, [Service] DataStore dataStore)
        => dataStore.Products.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Category> GetCategories([Service] DataStore dataStore)
        => dataStore.Categories;

    public DashboardViewModel GetDashboard([Service] DataStore dataStore)
    {
        var dashboard = new DashboardViewModel
        {
            TotalCustomers = dataStore.Customers.Count,
            TotalOrders = dataStore.Orders.Count,
            TotalRevenue = dataStore.Orders.Sum(o => o.TotalAmount),
            PendingOrders = dataStore.Orders.Count(o => o.Status == "Pending"),
            CompletedOrders = dataStore.Orders.Count(o => o.Status == "Completed")
        };

        // Optimization: Create dictionary lookups to avoid N+1 queries
        var productLookup = dataStore.Products.ToDictionary(p => p.Id);
        var customerLookup = dataStore.Customers.ToDictionary(c => c.Id);

        dashboard.TopProducts = dataStore.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = productLookup.TryGetValue(g.Key, out var product) ? product.Name : "Unknown",
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount / 100))
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToList();

        dashboard.RecentOrders = dataStore.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                CustomerName = customerLookup.TryGetValue(o.CustomerId, out var customer) ? customer.Name : "Unknown",
                TotalAmount = o.TotalAmount,
                Status = o.Status
            })
            .ToList();

        dashboard.TopCustomers = dataStore.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new CustomerStatsDto
            {
                CustomerId = g.Key,
                CustomerName = customerLookup.TryGetValue(g.Key, out var customer) ? customer.Name : "Unknown",
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(5)
            .ToList();

        dashboard.RevenueByMonth = dataStore.Orders
            .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

        return dashboard;
    }
}
