using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderItemsController : ControllerBase
{
    private readonly DataStore _dataStore;

    public OrderItemsController(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <summary>
    /// Batch endpoint for GraphQL DataLoader efficiency.
    /// GET /api/orderitems/by-orders?orderIds=1,2,3
    /// </summary>
    [HttpGet("by-orders")]
    public ActionResult<IEnumerable<OrderItem>> GetOrderItemsByOrderIds([FromQuery] int[] orderIds)
    {
        var orderItems = _dataStore.OrderItems.Where(oi => orderIds.Contains(oi.OrderId));
        return Ok(orderItems);
    }
}
