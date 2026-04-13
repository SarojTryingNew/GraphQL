using RestVsGraphQL.Models;
using System.Threading;

namespace RestVsGraphQL.Services;

public class DataStore
{
    private readonly List<Customer> _customers = new();
    private readonly List<Order> _orders = new();
    private readonly List<Product> _products = new();
    private readonly List<Category> _categories = new();
    private readonly List<OrderItem> _orderItems = new();
    private readonly List<OrderItemNote> _orderItemNotes = new();

    private int _nextCustomerId = 0;
    private int _nextOrderId = 0;
    private int _nextProductId = 0;
    private int _nextCategoryId = 0;
    private int _nextOrderItemId = 0;
    private int _nextOrderItemNoteId = 0;

    public DataStore()
    {
        SeedData();
    }

    public List<Customer> Customers => _customers;
    public List<Order> Orders => _orders;
    public List<Product> Products => _products;
    public List<Category> Categories => _categories;
    public List<OrderItem> OrderItems => _orderItems;
    public List<OrderItemNote> OrderItemNotes => _orderItemNotes;

    // Thread-safe ID generation using Interlocked
    public int GetNextCustomerId() => Interlocked.Increment(ref _nextCustomerId);
    public int GetNextOrderId() => Interlocked.Increment(ref _nextOrderId);
    public int GetNextProductId() => Interlocked.Increment(ref _nextProductId);
    public int GetNextCategoryId() => Interlocked.Increment(ref _nextCategoryId);
    public int GetNextOrderItemId() => Interlocked.Increment(ref _nextOrderItemId);
    public int GetNextOrderItemNoteId() => Interlocked.Increment(ref _nextOrderItemNoteId);

    private void SeedData()
    {
        var categories = new[]
        {
            new Category { Id = GetNextCategoryId(), Name = "Electronics", Description = "Electronic devices and accessories" },
            new Category { Id = GetNextCategoryId(), Name = "Clothing", Description = "Apparel and fashion items" },
            new Category { Id = GetNextCategoryId(), Name = "Books", Description = "Books and publications" },
            new Category { Id = GetNextCategoryId(), Name = "Home & Garden", Description = "Home and garden items" },
        };
        _categories.AddRange(categories);

        var products = new[]
        {
            new Product { Id = GetNextProductId(), Name = "Laptop", Description = "High-performance laptop", Price = 1200m, StockQuantity = 50, CategoryId = 1, Category = categories[0] },
            new Product { Id = GetNextProductId(), Name = "Smartphone", Description = "Latest smartphone", Price = 800m, StockQuantity = 100, CategoryId = 1, Category = categories[0] },
            new Product { Id = GetNextProductId(), Name = "Headphones", Description = "Noise-cancelling headphones", Price = 200m, StockQuantity = 150, CategoryId = 1, Category = categories[0] },
            new Product { Id = GetNextProductId(), Name = "T-Shirt", Description = "Cotton t-shirt", Price = 25m, StockQuantity = 500, CategoryId = 2, Category = categories[1] },
            new Product { Id = GetNextProductId(), Name = "Jeans", Description = "Denim jeans", Price = 60m, StockQuantity = 300, CategoryId = 2, Category = categories[1] },
            new Product { Id = GetNextProductId(), Name = "Novel", Description = "Bestselling novel", Price = 15m, StockQuantity = 200, CategoryId = 3, Category = categories[2] },
            new Product { Id = GetNextProductId(), Name = "Textbook", Description = "Educational textbook", Price = 80m, StockQuantity = 100, CategoryId = 3, Category = categories[2] },
            new Product { Id = GetNextProductId(), Name = "Garden Tools", Description = "Set of garden tools", Price = 45m, StockQuantity = 75, CategoryId = 4, Category = categories[3] },
        };
        _products.AddRange(products);

        var customers = new[]
        {
            new Customer { Id = GetNextCustomerId(), Name = "John Doe", Email = "john@example.com", Phone = "555-0101", CreatedAt = DateTime.UtcNow.AddMonths(-6) },
            new Customer { Id = GetNextCustomerId(), Name = "Jane Smith", Email = "jane@example.com", Phone = "555-0102", CreatedAt = DateTime.UtcNow.AddMonths(-5) },
            new Customer { Id = GetNextCustomerId(), Name = "Bob Johnson", Email = "bob@example.com", Phone = "555-0103", CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new Customer { Id = GetNextCustomerId(), Name = "Alice Williams", Email = "alice@example.com", Phone = "555-0104", CreatedAt = DateTime.UtcNow.AddMonths(-3) },
            new Customer { Id = GetNextCustomerId(), Name = "Charlie Brown", Email = "charlie@example.com", Phone = "555-0105", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
        };
        _customers.AddRange(customers);

        for (int i = 0; i < 20; i++)
        {
            var order = new Order
            {
                Id = GetNextOrderId(),
                CustomerId = customers[i % customers.Length].Id,
                Customer = customers[i % customers.Length],
                OrderDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 180)),
                Status = i % 3 == 0 ? "Pending" : i % 3 == 1 ? "Completed" : "Shipped"
            };

            var itemCount = Random.Shared.Next(1, 5);
            for (int j = 0; j < itemCount; j++)
            {
                var product = products[Random.Shared.Next(products.Length)];
                var quantity = Random.Shared.Next(1, 5);
                var orderItem = new OrderItem
                {
                    Id = GetNextOrderItemId(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    Discount = Random.Shared.Next(0, 20)
                };

                if (Random.Shared.Next(0, 3) == 0)
                {
                    var note = new OrderItemNote
                    {
                        Id = GetNextOrderItemNoteId(),
                        OrderItemId = orderItem.Id,
                        Content = $"Note for order item {orderItem.Id}",
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
                    };
                    orderItem.Notes.Add(note);
                    _orderItemNotes.Add(note);
                }

                order.Items.Add(orderItem);
                _orderItems.Add(orderItem);
            }

            order.TotalAmount = order.Items.Sum(item => item.Quantity * item.UnitPrice * (1 - item.Discount / 100));
            _orders.Add(order);
        }
    }
}
