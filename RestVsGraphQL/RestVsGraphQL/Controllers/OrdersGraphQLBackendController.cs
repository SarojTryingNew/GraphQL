using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

/// <summary>
/// REST API Controller that uses GraphQL as an internal layer between REST and the database
/// This demonstrates the architecture: Angular -> REST API -> GraphQL -> DataStore
/// </summary>
[ApiController]
[Route("api/graphql-backend/orders")]
public class OrdersGraphQLBackendController : ControllerBase
{
    private readonly GraphQLExecutorService _graphQLExecutor;

    public OrdersGraphQLBackendController(GraphQLExecutorService graphQLExecutor)
    {
        _graphQLExecutor = graphQLExecutor;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        try
        {
            var orders = await _graphQLExecutor.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        try
        {
            var order = await _graphQLExecutor.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("bulk")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByIds([FromQuery] string ids)
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

        try
        {
            var orders = await _graphQLExecutor.GetOrdersByIdsAsync(orderIds);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkOperationResult>> BulkCreateOrders([FromBody] BulkOrderCreateRequest request)
    {
        try
        {
            var result = await _graphQLExecutor.BulkCreateOrdersAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("bulk")]
    public async Task<ActionResult<BulkOperationResult>> BulkUpdateOrders([FromBody] BulkOrderUpdateRequest request)
    {
        try
        {
            var result = await _graphQLExecutor.BulkUpdateOrdersAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("bulk")]
    public async Task<ActionResult<BulkOperationResult>> BulkDeleteOrders([FromBody] BulkOrderDeleteRequest request)
    {
        try
        {
            var result = await _graphQLExecutor.BulkDeleteOrdersAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
