using RestVsGraphQL.Models;

namespace RestVsGraphQL.Services;

/// <summary>
/// Service containing shared dashboard calculation logic.
/// Eliminates code duplication between REST and GraphQL implementations.
/// Follows the DRY (Don't Repeat Yourself) principle.
/// </summary>
public class DashboardService
{
    private readonly DataStore _dataStore;

    public DashboardService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public int GetTotalCustomers()
        => _dataStore.Customers.Count;

    public int GetTotalOrders()
        => _dataStore.Orders.Count;

    public decimal GetTotalRevenue()
        => _dataStore.Orders.Sum(o => o.TotalAmount);

    public int GetPendingOrders()
        => _dataStore.Orders.Count(o => o.Status == "Pending");

    public int GetCompletedOrders()
        => _dataStore.Orders.Count(o => o.Status == "Completed");

    public List<TopProductDto> GetTopProducts()
    {
        var productLookup = _dataStore.Products.ToDictionary(p => p.Id);

        return _dataStore.OrderItems
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
    }

    public List<RecentOrderDto> GetRecentOrders()
    {
        var customerLookup = _dataStore.Customers.ToDictionary(c => c.Id);

        return _dataStore.Orders
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
    }

    public List<CustomerStatsDto> GetTopCustomers()
    {
        var customerLookup = _dataStore.Customers.ToDictionary(c => c.Id);

        return _dataStore.Orders
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
    }

    public Dictionary<string, decimal> GetRevenueByMonth()
        => _dataStore.Orders
            .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

    /// <summary>
    /// Gets the complete dashboard with all fields populated.
    /// Used by REST API which always returns all data.
    /// </summary>
    public DashboardViewModel GetCompleteDashboard()
    {
        return new DashboardViewModel
        {
            TotalCustomers = GetTotalCustomers(),
            TotalOrders = GetTotalOrders(),
            TotalRevenue = GetTotalRevenue(),
            PendingOrders = GetPendingOrders(),
            CompletedOrders = GetCompletedOrders(),
            TopProducts = GetTopProducts(),
            RecentOrders = GetRecentOrders(),
            TopCustomers = GetTopCustomers(),
            RevenueByMonth = GetRevenueByMonth()
        };
    }
}
