using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly DataStore _dataStore;
    private readonly OrderService _orderService;

    public OrdersController(DataStore dataStore, OrderService orderService)
    {
        _dataStore = dataStore;
        _orderService = orderService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetOrders()
    {
        return Ok(_dataStore.Orders);
    }

    [HttpGet("{id}")]
    public ActionResult<Order> GetOrder(int id)
    {
        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound();

        // Use extension method for loading relations
        order.LoadRelations(_dataStore);

        return Ok(order);
    }

    [HttpGet("{id}/nested")]
    public ActionResult<Order> GetOrderWithNestedData(int id)
    {
        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound();

        // Use extension method with category loading (4-level nesting)
        order.LoadRelations(_dataStore, includeCategory: true);

        return Ok(order);
    }

    [HttpGet("bulk")]
    public ActionResult<IEnumerable<Order>> GetOrdersByIds([FromQuery] string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
            return BadRequest("Order IDs are required");

        var orderIds = ids.Split(',')
            .Select(id => int.TryParse(id.Trim(), out var result) ? result : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        if (!orderIds.Any())
            return BadRequest("No valid order IDs provided");

        var orders = _dataStore.Orders.Where(o => orderIds.Contains(o.Id)).ToList();

        // Use extension method for batch loading
        orders.LoadRelations(_dataStore);

        return Ok(orders);
    }

    [HttpPost("bulk")]
    public ActionResult<BulkOperationResult> BulkCreateOrders([FromBody] BulkOrderCreateRequest request)
    {
        // Delegate to service layer (shared with GraphQL)
        return Ok(_orderService.BulkCreateOrders(request));
    }

    [HttpPut("bulk")]
    public ActionResult<BulkOperationResult> BulkUpdateOrders([FromBody] BulkOrderUpdateRequest request)
    {
        // Delegate to service layer (shared with GraphQL)
        return Ok(_orderService.BulkUpdateOrders(request));
    }

    [HttpDelete("bulk")]
    public ActionResult<BulkOperationResult> BulkDeleteOrders([FromBody] BulkOrderDeleteRequest request)
    {
        // Delegate to service layer (shared with GraphQL)
        return Ok(_orderService.BulkDeleteOrders(request));
    }
}
