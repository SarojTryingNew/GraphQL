using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.Types.POC;

/// <summary>
/// Type extension for DashboardViewModel with field resolvers.
/// Each field is resolved ONLY when the client requests it.
/// This prevents over-fetching and unnecessary computation.
/// Uses DashboardService to avoid code duplication with REST API.
/// </summary>
[ObjectType<DashboardViewModel>]
public static class DashboardViewModelType
{
    public static int GetTotalCustomers([Service] DashboardService service)
        => service.GetTotalCustomers();

    public static int GetTotalOrders([Service] DashboardService service)
        => service.GetTotalOrders();

    public static decimal GetTotalRevenue([Service] DashboardService service)
        => service.GetTotalRevenue();

    public static int GetPendingOrders([Service] DashboardService service)
        => service.GetPendingOrders();

    public static int GetCompletedOrders([Service] DashboardService service)
        => service.GetCompletedOrders();

    public static List<TopProductDto> GetTopProducts([Service] DashboardService service)
        => service.GetTopProducts();

    public static List<RecentOrderDto> GetRecentOrders([Service] DashboardService service)
        => service.GetRecentOrders();

    public static List<CustomerStatsDto> GetTopCustomers([Service] DashboardService service)
        => service.GetTopCustomers();

    public static Dictionary<string, decimal> GetRevenueByMonth([Service] DashboardService service)
        => service.GetRevenueByMonth();
}
