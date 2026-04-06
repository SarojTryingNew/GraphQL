namespace RestVsGraphQL.DTOs;

public class BulkOrderCreateRequest
{
    public List<OrderCreateDto> Orders { get; set; } = new();
}

public class OrderCreateDto
{
    public int CustomerId { get; set; }
    public string Status { get; set; } = "Pending";
    public List<OrderItemCreateDto> Items { get; set; } = new();
}

public class OrderItemCreateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public List<string> Notes { get; set; } = new();
}

public class BulkOrderUpdateRequest
{
    public List<OrderUpdateDto> Orders { get; set; } = new();
}

public class OrderUpdateDto
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public List<OrderItemUpdateDto>? Items { get; set; }
}

public class OrderItemUpdateDto
{
    public int? Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
}

public class BulkOperationResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<int> CreatedIds { get; set; } = new();
}
