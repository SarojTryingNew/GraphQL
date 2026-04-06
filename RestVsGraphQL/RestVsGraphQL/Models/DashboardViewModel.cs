namespace RestVsGraphQL.Models;

public class DashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<CustomerStatsDto> TopCustomers { get; set; } = new();
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentOrderDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CustomerStatsDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}
