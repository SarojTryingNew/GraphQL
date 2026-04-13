using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;

namespace RestVsGraphQL.Services;

/// <summary>
/// Service layer containing shared business logic for order operations.
/// Used by both REST API (OrdersController) and GraphQL API (Mutation).
/// This eliminates code duplication and provides a single source of truth.
/// </summary>
public class OrderService
{
    private readonly DataStore _dataStore;

    public OrderService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <summary>
    /// Creates multiple orders in a single bulk operation.
    /// Validates customers and products, handles errors gracefully.
    /// </summary>
    public BulkOperationResult BulkCreateOrders(BulkOrderCreateRequest request)
    {
        var result = new BulkOperationResult();

        // Input validation
        if (request == null || request.Orders == null || !request.Orders.Any())
        {
            result.FailureCount++;
            result.Errors.Add("Request cannot be null or empty");
            return result;
        }

        foreach (var orderDto in request.Orders)
        {
            try
            {
                // Validate order items exist
                if (orderDto.Items == null || !orderDto.Items.Any())
                {
                    result.FailureCount++;
                    result.Errors.Add($"Order must contain at least one item");
                    continue;
                }

                // Validate customer exists
                var customer = _dataStore.Customers.FirstOrDefault(c => c.Id == orderDto.CustomerId);
                if (customer == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Customer {orderDto.CustomerId} not found");
                    continue;
                }

                // Create order
                var order = new Order
                {
                    Id = _dataStore.GetNextOrderId(),
                    CustomerId = orderDto.CustomerId,
                    Customer = customer,
                    OrderDate = DateTime.UtcNow,
                    Status = orderDto.Status
                };

                // Add order items with validation
                foreach (var itemDto in orderDto.Items)
                {
                    // Validate quantity
                    if (itemDto.Quantity <= 0)
                    {
                        result.Errors.Add($"Invalid quantity {itemDto.Quantity} for product {itemDto.ProductId}");
                        continue;
                    }

                    // Validate discount
                    if (itemDto.Discount < 0 || itemDto.Discount > 100)
                    {
                        result.Errors.Add($"Invalid discount {itemDto.Discount}% for product {itemDto.ProductId}. Must be between 0-100");
                        continue;
                    }

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

                    // Add notes to order item
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

                // Skip order if no valid items were added
                if (!order.Items.Any())
                {
                    result.FailureCount++;
                    result.Errors.Add($"Order has no valid items");
                    continue;
                }

                // Calculate total and save
                order.RecalculateTotal();
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

        return result;
    }

    /// <summary>
    /// Updates multiple orders in a single bulk operation.
    /// Supports updating status and adding/modifying order items.
    /// </summary>
    public BulkOperationResult BulkUpdateOrders(BulkOrderUpdateRequest request)
    {
        var result = new BulkOperationResult();

        // Input validation
        if (request == null || request.Orders == null || !request.Orders.Any())
        {
            result.FailureCount++;
            result.Errors.Add("Request cannot be null or empty");
            return result;
        }

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

                // Update status if provided
                if (orderDto.Status != null)
                {
                    order.Status = orderDto.Status;
                }

                // Update items if provided
                if (orderDto.Items != null)
                {
                    foreach (var itemDto in orderDto.Items)
                    {
                        // Validate quantity
                        if (itemDto.Quantity <= 0)
                        {
                            result.Errors.Add($"Invalid quantity {itemDto.Quantity} for order {orderDto.Id}");
                            continue;
                        }

                        // Validate discount
                        if (itemDto.Discount < 0 || itemDto.Discount > 100)
                        {
                            result.Errors.Add($"Invalid discount {itemDto.Discount}% for order {orderDto.Id}. Must be between 0-100");
                            continue;
                        }

                        if (itemDto.Id.HasValue)
                        {
                            // Update existing item
                            var existingItem = _dataStore.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.Id.Value);
                            if (existingItem != null)
                            {
                                existingItem.Quantity = itemDto.Quantity;
                                existingItem.Discount = itemDto.Discount;
                            }
                        }
                        else
                        {
                            // Add new item
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

                    // Recalculate total
                    order.RecalculateTotal();
                }

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(ex.Message);
            }
        }

        return result;
    }

    /// <summary>
    /// Deletes multiple orders in a single bulk operation.
    /// Cascades deletion to order items and notes.
    /// </summary>
    public BulkOperationResult BulkDeleteOrders(BulkOrderDeleteRequest request)
    {
        var result = new BulkOperationResult();

        foreach (var orderId in request.OrderIds)
        {
            try
            {
                var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Order {orderId} not found");
                    continue;
                }

                // Cascade delete: remove notes, then items, then order
                var orderItems = _dataStore.OrderItems.Where(oi => oi.OrderId == orderId).ToList();
                foreach (var item in orderItems)
                {
                    var notes = _dataStore.OrderItemNotes.Where(n => n.OrderItemId == item.Id).ToList();
                    foreach (var note in notes)
                    {
                        _dataStore.OrderItemNotes.Remove(note);
                    }
                    _dataStore.OrderItems.Remove(item);
                }

                _dataStore.Orders.Remove(order);
                
                result.SuccessCount++;
                result.DeletedIds.Add(orderId);
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(ex.Message);
            }
        }

        return result;
    }
}
