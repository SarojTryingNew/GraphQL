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

    public OrdersController(DataStore dataStore)
    {
        _dataStore = dataStore;
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

        order.Customer = _dataStore.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
        order.Items = _dataStore.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();
        
        foreach (var item in order.Items)
        {
            item.Product = _dataStore.Products.FirstOrDefault(p => p.Id == item.ProductId);
            item.Notes = _dataStore.OrderItemNotes.Where(n => n.OrderItemId == item.Id).ToList();
        }

        return Ok(order);
    }

    [HttpGet("{id}/nested")]
    public ActionResult<Order> GetOrderWithNestedData(int id)
    {
        var order = _dataStore.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound();

        order.Customer = _dataStore.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
        order.Items = _dataStore.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();
        
        foreach (var item in order.Items)
        {
            item.Product = _dataStore.Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (item.Product != null)
            {
                item.Product.Category = _dataStore.Categories.FirstOrDefault(c => c.Id == item.Product.CategoryId);
            }
            item.Notes = _dataStore.OrderItemNotes.Where(n => n.OrderItemId == item.Id).ToList();
        }

        return Ok(order);
    }

    [HttpPost("bulk")]
    public ActionResult<BulkOperationResult> BulkCreateOrders([FromBody] BulkOrderCreateRequest request)
    {
        var result = new BulkOperationResult();

        foreach (var orderDto in request.Orders)
        {
            try
            {
                var customer = _dataStore.Customers.FirstOrDefault(c => c.Id == orderDto.CustomerId);
                if (customer == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Customer {orderDto.CustomerId} not found");
                    continue;
                }

                var order = new Order
                {
                    Id = _dataStore.GetNextOrderId(),
                    CustomerId = orderDto.CustomerId,
                    Customer = customer,
                    OrderDate = DateTime.UtcNow,
                    Status = orderDto.Status
                };

                foreach (var itemDto in orderDto.Items)
                {
                    var product = _dataStore.Products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                    if (product == null)
                    {
                        result.Errors.Add($"Product {itemDto.ProductId} not found for order");
                        continue;
                    }

                    var orderItem = new OrderItem
                    {
                        Id = _dataStore.GetNextOrderItemId(),
                        OrderId = order.Id,
                        ProductId = itemDto.ProductId,
                        Product = product,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        Discount = itemDto.Discount
                    };

                    foreach (var noteContent in itemDto.Notes)
                    {
                        var note = new OrderItemNote
                        {
                            Id = _dataStore.GetNextOrderItemNoteId(),
                            OrderItemId = orderItem.Id,
                            Content = noteContent,
                            CreatedAt = DateTime.UtcNow
                        };
                        orderItem.Notes.Add(note);
                        _dataStore.OrderItemNotes.Add(note);
                    }

                    order.Items.Add(orderItem);
                    _dataStore.OrderItems.Add(orderItem);
                }

                order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount / 100));
                _dataStore.Orders.Add(order);
                result.SuccessCount++;
                result.CreatedIds.Add(order.Id);
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(ex.Message);
            }
        }

        return Ok(result);
    }

    [HttpPut("bulk")]
    public ActionResult<BulkOperationResult> BulkUpdateOrders([FromBody] BulkOrderUpdateRequest request)
    {
        var result = new BulkOperationResult();

        foreach (var orderDto in request.Orders)
        {
            try
            {
                var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderDto.Id);
                if (order == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Order {orderDto.Id} not found");
                    continue;
                }

                if (orderDto.Status != null)
                {
                    order.Status = orderDto.Status;
                }

                if (orderDto.Items != null)
                {
                    foreach (var itemDto in orderDto.Items)
                    {
                        if (itemDto.Id.HasValue)
                        {
                            var existingItem = _dataStore.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.Id.Value);
                            if (existingItem != null)
                            {
                                existingItem.Quantity = itemDto.Quantity;
                                existingItem.Discount = itemDto.Discount;
                            }
                        }
                        else
                        {
                            var product = _dataStore.Products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                            if (product != null)
                            {
                                var newItem = new OrderItem
                                {
                                    Id = _dataStore.GetNextOrderItemId(),
                                    OrderId = order.Id,
                                    ProductId = itemDto.ProductId,
                                    Product = product,
                                    Quantity = itemDto.Quantity,
                                    UnitPrice = product.Price,
                                    Discount = itemDto.Discount
                                };
                                _dataStore.OrderItems.Add(newItem);
                                order.Items.Add(newItem);
                            }
                        }
                    }

                    order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount / 100));
                }

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(ex.Message);
            }
        }

        return Ok(result);
    }
}
