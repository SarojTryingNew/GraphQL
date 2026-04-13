using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    private readonly DataStore _dataStore;

    public DashboardController(DashboardService dashboardService, DataStore dataStore)
    {
        _dashboardService = dashboardService;
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult<DashboardViewModel> GetDashboard()
    {
        // REST always returns complete dashboard
        return Ok(_dashboardService.GetCompleteDashboard());
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
