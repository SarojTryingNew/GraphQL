using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL;

public class Mutation
{
    public BulkOperationResult BulkCreateOrders(
        BulkOrderCreateRequest request,
        [Service] DataStore dataStore)
    {
        var result = new BulkOperationResult();

        foreach (var orderDto in request.Orders)
        {
            try
            {
                var customer = dataStore.Customers.FirstOrDefault(c => c.Id == orderDto.CustomerId);
                if (customer == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Customer {orderDto.CustomerId} not found");
                    continue;
                }

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
                    var product = dataStore.Products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                    if (product == null)
                    {
                        result.Errors.Add($"Product {itemDto.ProductId} not found for order");
                        continue;
                    }

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

                order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount / 100));
                dataStore.Orders.Add(order);
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

    public BulkOperationResult BulkUpdateOrders(
        BulkOrderUpdateRequest request,
        [Service] DataStore dataStore)
    {
        var result = new BulkOperationResult();

        foreach (var orderDto in request.Orders)
        {
            try
            {
                var order = dataStore.Orders.FirstOrDefault(o => o.Id == orderDto.Id);
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
                            var existingItem = dataStore.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.Id.Value);
                            if (existingItem != null)
                            {
                                existingItem.Quantity = itemDto.Quantity;
                                existingItem.Discount = itemDto.Discount;
                            }
                        }
                        else
                        {
                            var product = dataStore.Products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                            if (product != null)
                            {
                                var newItem = new OrderItem
                                {
                                    Id = dataStore.GetNextOrderItemId(),
                                    OrderId = order.Id,
                                    ProductId = itemDto.ProductId,
                                    Product = product,
                                    Quantity = itemDto.Quantity,
                                    UnitPrice = product.Price,
                                    Discount = itemDto.Discount
                                };
                                dataStore.OrderItems.Add(newItem);
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

        return result;
    }

    public BulkOperationResult BulkDeleteOrders(
        BulkOrderDeleteRequest request,
        [Service] DataStore dataStore)
    {
        var result = new BulkOperationResult();

        foreach (var orderId in request.OrderIds)
        {
            try
            {
                var order = dataStore.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Order {orderId} not found");
                    continue;
                }

                var orderItems = dataStore.OrderItems.Where(oi => oi.OrderId == orderId).ToList();
                foreach (var item in orderItems)
                {
                    var notes = dataStore.OrderItemNotes.Where(n => n.OrderItemId == item.Id).ToList();
                    foreach (var note in notes)
                    {
                        dataStore.OrderItemNotes.Remove(note);
                    }
                    dataStore.OrderItems.Remove(item);
                }

                dataStore.Orders.Remove(order);
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

    public Order CreateOrder(OrderCreateDto orderDto, [Service] DataStore dataStore)
    {
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
            var product = dataStore.Products.FirstOrDefault(p => p.Id == itemDto.ProductId);
            if (product == null)
                continue;

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

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount / 100));
        dataStore.Orders.Add(order);

        return order;
    }
}
