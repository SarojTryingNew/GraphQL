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

        order.Customer = dataStore.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
        order.Items = dataStore.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();
        
        foreach (var item in order.Items)
        {
            item.Product = dataStore.Products.FirstOrDefault(p => p.Id == item.ProductId);
            item.Notes = dataStore.OrderItemNotes.Where(n => n.OrderItemId == item.Id).ToList();
        }

        return order;
    }

    public IEnumerable<Product> GetProducts([Service] DataStore dataStore)
        => dataStore.Products;

    public Product? GetProduct(int id, [Service] DataStore dataStore)
    {
        var product = dataStore.Products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            product.Category = dataStore.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        }
        return product;
    }

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

        dashboard.TopProducts = dataStore.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = dataStore.Products.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "Unknown",
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
                CustomerName = dataStore.Customers.FirstOrDefault(c => c.Id == o.CustomerId)?.Name ?? "Unknown",
                TotalAmount = o.TotalAmount,
                Status = o.Status
            })
            .ToList();

        dashboard.TopCustomers = dataStore.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new CustomerStatsDto
            {
                CustomerId = g.Key,
                CustomerName = dataStore.Customers.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Unknown",
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
