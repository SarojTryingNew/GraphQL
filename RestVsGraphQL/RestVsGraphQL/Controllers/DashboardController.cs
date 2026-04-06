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

        dashboard.TopProducts = _dataStore.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = _dataStore.Products.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "Unknown",
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
                CustomerName = _dataStore.Customers.FirstOrDefault(c => c.Id == o.CustomerId)?.Name ?? "Unknown",
                TotalAmount = o.TotalAmount,
                Status = o.Status
            })
            .ToList();

        dashboard.TopCustomers = _dataStore.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new CustomerStatsDto
            {
                CustomerId = g.Key,
                CustomerName = _dataStore.Customers.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Unknown",
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
        var customerStats = _dataStore.Customers.Select(c => new
        {
            c.Id,
            c.Name,
            OrderCount = _dataStore.Orders.Count(o => o.CustomerId == c.Id),
            TotalSpent = _dataStore.Orders.Where(o => o.CustomerId == c.Id).Sum(o => o.TotalAmount)
        });

        var productStats = _dataStore.Products.Select(p => new
        {
            p.Id,
            p.Name,
            QuantitySold = _dataStore.OrderItems.Where(oi => oi.ProductId == p.Id).Sum(oi => oi.Quantity),
            Revenue = _dataStore.OrderItems.Where(oi => oi.ProductId == p.Id)
                .Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount / 100))
        });

        var categoryStats = _dataStore.Categories.Select(c => new
        {
            c.Id,
            c.Name,
            ProductCount = _dataStore.Products.Count(p => p.CategoryId == c.Id),
            TotalRevenue = _dataStore.Products
                .Where(p => p.CategoryId == c.Id)
                .SelectMany(p => _dataStore.OrderItems.Where(oi => oi.ProductId == p.Id))
                .Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount / 100))
        });

        return Ok(new
        {
            CustomerStats = customerStats,
            ProductStats = productStats,
            CategoryStats = categoryStats
        });
    }
}
