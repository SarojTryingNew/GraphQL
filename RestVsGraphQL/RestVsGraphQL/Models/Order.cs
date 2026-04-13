namespace RestVsGraphQL.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// Recalculates the total amount based on order items.
    /// Formula: Sum(Quantity × UnitPrice × (1 - Discount/100))
    /// </summary>
    public void RecalculateTotal()
    {
        TotalAmount = Items.Sum(item => item.Quantity * item.UnitPrice * (1 - item.Discount / 100));
    }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public List<OrderItemNote> Notes { get; set; } = new();
}

public class OrderItemNote
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
