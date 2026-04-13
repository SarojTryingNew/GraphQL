using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DataStore _dataStore;

    public DashboardController(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult<DashboardViewModel> GetDashboard()
    {
        var dashboard = new DashboardViewModel
        {
            TotalCustomers = _dataStore.Customers.Count,
            TotalOrders = _dataStore.Orders.Count,
            TotalRevenue = _dataStore.Orders.Sum(o => o.TotalAmount),
            PendingOrders = _dataStore.Orders.Count(o => o.Status == "Pending"),
            CompletedOrders = _dataStore.Orders.Count(o => o.Status == "Completed")
        };

        // Optimization: Create dictionary lookups to avoid N+1 queries
        var productLookup = _dataStore.Products.ToDictionary(p => p.Id);
        var customerLookup = _dataStore.Customers.ToDictionary(c => c.Id);

        dashboard.TopProducts = _dataStore.OrderItems
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

        dashboard.RecentOrders = _dataStore.Orders
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

        dashboard.TopCustomers = _dataStore.Orders
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

        dashboard.RevenueByMonth = _dataStore.Orders
            .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

        return Ok(dashboard);
    }

    [HttpGet("stats")]
    public ActionResult GetMultipleStats()
    {
        // Optimization: Pre-aggregate order data to avoid N+1 queries
        var customerOrderStats = _dataStore.Orders
            .GroupBy(o => o.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => new { OrderCount = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) }
            );

        var customerStats = _dataStore.Customers.Select(c => new
        {
            c.Id,
            c.Name,
            OrderCount = customerOrderStats.TryGetValue(c.Id, out var stats) ? stats.OrderCount : 0,
            TotalSpent = customerOrderStats.TryGetValue(c.Id, out var statsSpent) ? statsSpent.TotalSpent : 0m
        });

        // Optimization: Pre-aggregate order item data
        var productOrderItemStats = _dataStore.OrderItems
            .GroupBy(oi => oi.ProductId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount / 100))
                }
            );

        var productStats = _dataStore.Products.Select(p => new
        {
            p.Id,
            p.Name,
            QuantitySold = productOrderItemStats.TryGetValue(p.Id, out var stats) ? stats.QuantitySold : 0,
            Revenue = productOrderItemStats.TryGetValue(p.Id, out var statsRev) ? statsRev.Revenue : 0m
        });

        // Optimization: Pre-build product-to-category lookup
        var productsByCategory = _dataStore.Products
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var categoryStats = _dataStore.Categories.Select(c => new
        {
            c.Id,
            c.Name,
            ProductCount = productsByCategory.TryGetValue(c.Id, out var products) ? products.Count : 0,
            TotalRevenue = productsByCategory.TryGetValue(c.Id, out var categoryProducts)
                ? categoryProducts
                    .SelectMany(p => _dataStore.OrderItems.Where(oi => oi.ProductId == p.Id))
                    .Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount / 100))
                : 0m
        });

        return Ok(new
        {
            CustomerStats = customerStats,
            ProductStats = productStats,
            CategoryStats = categoryStats
        });
    }
}
