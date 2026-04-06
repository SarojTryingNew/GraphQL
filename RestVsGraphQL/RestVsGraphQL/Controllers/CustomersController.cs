using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly DataStore _dataStore;

    public CustomersController(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Customer>> GetCustomers()
    {
        return Ok(_dataStore.Customers);
    }

    [HttpGet("{id}")]
    public ActionResult<Customer> GetCustomer(int id)
    {
        var customer = _dataStore.Customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpGet("{id}/orders")]
    public ActionResult<IEnumerable<Order>> GetCustomerOrders(int id)
    {
        var customer = _dataStore.Customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
            return NotFound();

        var orders = _dataStore.Orders.Where(o => o.CustomerId == id).ToList();
        return Ok(orders);
    }
}
