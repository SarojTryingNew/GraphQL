using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL;

public class Mutation
{
    public BulkOperationResult BulkCreateOrders(
        BulkOrderCreateRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkCreateOrders(request);
    }

    public BulkOperationResult BulkUpdateOrders(
        BulkOrderUpdateRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkUpdateOrders(request);
    }

    public BulkOperationResult BulkDeleteOrders(
        BulkOrderDeleteRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkDeleteOrders(request);
    }

    public Order CreateOrder(OrderCreateDto orderDto, [Service] DataStore dataStore)
    {
        // Input validation
        if (orderDto == null)
            throw new ArgumentNullException(nameof(orderDto), "Order data cannot be null");

        if (orderDto.Items == null || !orderDto.Items.Any())
            throw new ArgumentException("Order must contain at least one item", nameof(orderDto));

        var customer = dataStore.Customers.FirstOrDefault(c => c.Id == orderDto.CustomerId);
        if (customer == null)
            throw new Exception($"Customer {orderDto.CustomerId} not found");

        var order = new Order
        {
            Id = dataStore.GetNextOrderId(),
            CustomerId = orderDto.CustomerId,
            Customer = customer,
            OrderDate = DateTime.UtcNow,
            Status = orderDto.Status
        };

        foreach (var itemDto in orderDto.Items)
        {
            // Validate quantity
            if (itemDto.Quantity <= 0)
                throw new ArgumentException($"Invalid quantity {itemDto.Quantity} for product {itemDto.ProductId}. Must be greater than 0");

            // Validate discount
            if (itemDto.Discount < 0 || itemDto.Discount > 100)
                throw new ArgumentException($"Invalid discount {itemDto.Discount}% for product {itemDto.ProductId}. Must be between 0-100");

            var product = dataStore.Products.FirstOrDefault(p => p.Id == itemDto.ProductId);
            if (product == null)
                throw new Exception($"Product {itemDto.ProductId} not found");

            var orderItem = new OrderItem
            {
                Id = dataStore.GetNextOrderItemId(),
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
                    Id = dataStore.GetNextOrderItemNoteId(),
                    OrderItemId = orderItem.Id,
                    Content = noteContent,
                    CreatedAt = DateTime.UtcNow
                };
                orderItem.Notes.Add(note);
                dataStore.OrderItemNotes.Add(note);
            }

            order.Items.Add(orderItem);
            dataStore.OrderItems.Add(orderItem);
        }

        // Use consistent total calculation method
        order.RecalculateTotal();
        dataStore.Orders.Add(order);

        return order;
    }
}
