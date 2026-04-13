using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : BaseEntityController<Customer>
{
    public CustomersController(DataStore dataStore) : base(dataStore)
    {
    }

    protected override IEnumerable<Customer> GetCollection() => DataStore.Customers;

    protected override Customer? FindById(int id) => DataStore.Customers.FirstOrDefault(c => c.Id == id);

    [HttpGet("{id}/orders")]
    public ActionResult<IEnumerable<Order>> GetCustomerOrders(int id)
    {
        var customer = DataStore.Customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
            return NotFound();

        var orders = DataStore.Orders.Where(o => o.CustomerId == id).ToList();
        return Ok(orders);
    }
}
