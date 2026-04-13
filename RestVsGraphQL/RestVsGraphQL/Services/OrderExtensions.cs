using RestVsGraphQL.Models;

namespace RestVsGraphQL.Services;

/// <summary>
/// Extension methods for loading related data for orders.
/// Eliminates duplicate loading logic across controllers and queries.
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Loads all related data for an order (customer, items, products, notes).
    /// Optionally includes nested category data for products.
    /// </summary>
    /// <param name="order">The order to load relations for</param>
    /// <param name="dataStore">The data store containing related entities</param>
    /// <param name="includeCategory">Whether to load product categories (4-level nesting)</param>
    /// <returns>The order with all relations loaded</returns>
    public static Order LoadRelations(this Order order, DataStore dataStore, bool includeCategory = false)
    {
        // Load customer
        order.Customer = dataStore.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
        
        // Load order items
        order.Items = dataStore.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();
        
        // Load product and notes for each item
        foreach (var item in order.Items)
        {
            item.Product = dataStore.Products.FirstOrDefault(p => p.Id == item.ProductId);
            
            // Optionally load category (for nested queries)
            if (includeCategory && item.Product != null)
            {
                item.Product.Category = dataStore.Categories.FirstOrDefault(c => c.Id == item.Product.CategoryId);
            }
            
            item.Notes = dataStore.OrderItemNotes.Where(n => n.OrderItemId == item.Id).ToList();
        }
        
        return order;
    }

    /// <summary>
    /// Loads relations for multiple orders efficiently.
    /// Uses dictionary lookups and batch queries to avoid N+1 problems.
    /// </summary>
    public static IEnumerable<Order> LoadRelations(this IEnumerable<Order> orders, DataStore dataStore, bool includeCategory = false)
    {
        var orderList = orders.ToList();
        if (!orderList.Any()) return orderList;

        // Optimization: Create lookups to avoid repeated FirstOrDefault calls
        var customerLookup = dataStore.Customers.ToDictionary(c => c.Id);
        var productLookup = dataStore.Products.ToDictionary(p => p.Id);
        var categoryLookup = includeCategory ? dataStore.Categories.ToDictionary(c => c.Id) : null;

        // Batch load all order items for these orders
        var orderIds = orderList.Select(o => o.Id).ToHashSet();
        var orderItemsGrouped = dataStore.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .GroupBy(oi => oi.OrderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Batch load all notes for order items
        var orderItemIds = dataStore.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .Select(oi => oi.Id)
            .ToHashSet();
        var notesGrouped = dataStore.OrderItemNotes
            .Where(n => orderItemIds.Contains(n.OrderItemId))
            .GroupBy(n => n.OrderItemId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Populate relations using lookups
        foreach (var order in orderList)
        {
            // Load customer
            if (customerLookup.TryGetValue(order.CustomerId, out var customer))
            {
                order.Customer = customer;
            }

            // Load order items
            if (orderItemsGrouped.TryGetValue(order.Id, out var items))
            {
                order.Items = items;

                // Load product and notes for each item
                foreach (var item in items)
                {
                    if (productLookup.TryGetValue(item.ProductId, out var product))
                    {
                        item.Product = product;

                        // Optionally load category
                        if (includeCategory && categoryLookup != null && 
                            categoryLookup.TryGetValue(product.CategoryId, out var category))
                        {
                            product.Category = category;
                        }
                    }

                    // Load notes
                    if (notesGrouped.TryGetValue(item.Id, out var itemNotes))
                    {
                        item.Notes = itemNotes;
                    }
                }
            }
        }

        return orderList;
    }
}
