using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.Query;

public class Query
{
    public IEnumerable<Customer> GetCustomers([Service] DataStore dataStore)
        => dataStore.Customers;

    //public Customer? GetCustomer(int id, [Service] DataStore dataStore)
    //    => dataStore.Customers.FirstOrDefault(c => c.Id == id);

    //public IEnumerable<Order> GetOrders([Service] DataStore dataStore)
    //    => dataStore.Orders;

    //public Order? GetOrder(int id, [Service] DataStore dataStore)
    //{
    //    var order = dataStore.Orders.FirstOrDefault(o => o.Id == id);
    //    if (order == null) return null;

    //    // Use extension method for efficient relation loading
    //    order.LoadRelations(dataStore);

    //    return order;
    //}

    //public IEnumerable<Order> GetOrdersByIds(List<int> ids, [Service] DataStore dataStore)
    //{
    //    var orders = dataStore.Orders.Where(o => ids.Contains(o.Id)).ToList();

    //    // Use extension method for efficient batch relation loading
    //    orders.LoadRelations(dataStore);

    //    return orders;
    //}

    //public IEnumerable<Product> GetProducts([Service] DataStore dataStore)
    //    => dataStore.Products;

    //public Product? GetProduct(int id, [Service] DataStore dataStore)
    //    => dataStore.Products.FirstOrDefault(p => p.Id == id);

    //public IEnumerable<Category> GetCategories([Service] DataStore dataStore)
    //    => dataStore.Categories;

    //public DashboardViewModel GetDashboard()
    //    => new DashboardViewModel();
}
