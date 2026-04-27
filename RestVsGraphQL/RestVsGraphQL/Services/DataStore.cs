using RestVsGraphQL.DTOs;
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

    // SIG Data - stored as separate collections like a real database
    private readonly List<DeviceApplicationDto> _deviceApplications = new();
    private readonly List<FunctionGroupDto> _functionGroups = new();
    private readonly List<FunctionDto> _functions = new();
    private readonly List<FunctionBlockDto> _functionBlocks = new();
    private readonly List<SignalDto> _signals = new();
    private readonly List<SubsignalDto> _subsignals = new();
    private readonly List<CdcConversionDto> _cdcConversions = new();
    private readonly List<RoutingDto> _routings = new();

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

    // SIG Data collections
    public List<DeviceApplicationDto> DeviceApplications => _deviceApplications;
    public List<FunctionGroupDto> FunctionGroups => _functionGroups;
    public List<FunctionDto> Functions => _functions;
    public List<FunctionBlockDto> FunctionBlocks => _functionBlocks;
    public List<SignalDto> Signals => _signals;
    public List<SubsignalDto> Subsignals => _subsignals;
    public List<CdcConversionDto> CdcConversions => _cdcConversions;
    public List<RoutingDto> Routings => _routings;

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

        // Seed DeviceApplication data
        SeedDeviceApplications();
    }

    private void SeedDeviceApplications()
    {
        // ============================================
        // DEVICE 1: IED_001 - Protection Relay
        // ============================================
        var device1 = new DeviceApplicationDto
        {
            PublicTechnicalName = "IED_001",
            DisplayText = "Intelligent Electronic Device 001",
            TypeName = "ProtectionRelay",
            LastUpdatedAt = DateTime.UtcNow.AddDays(-10),
            LastModifiedBy = "system",
            DddVersion = "1.0.0",
            ComDddVersion = "1.0.0"
        };
        _deviceApplications.Add(device1);

        // Function Groups for Device 1
        var fg1_prot = new FunctionGroupDto
        {
            PublicTechnicalName = "PROT",
            DisplayText = "Protection",
            TypeName = "ProtectionGroup",
            PTNPath = "IED_001/PROT",
            IsDeletable = false
        };
        _functionGroups.Add(fg1_prot);

        var fg1_meas = new FunctionGroupDto
        {
            PublicTechnicalName = "MEAS",
            DisplayText = "Measurement",
            TypeName = "MeasurementGroup",
            PTNPath = "IED_001/MEAS",
            IsDeletable = false
        };
        _functionGroups.Add(fg1_meas);

        // Function for PROT group
        var func1_pdif = new FunctionDto
        {
            PublicTechnicalName = "PDIF",
            DisplayText = "Differential Protection",
            TypeName = "DifferentialProtection",
            PTNPath = "IED_001/PROT/PDIF",
            IsDeletable = true
        };
        _functions.Add(func1_pdif);

        // Function Blocks for Device 1
        var fb1_mmxu1 = new FunctionBlockDto
        {
            PublicTechnicalName = "MMXU1",
            DisplayText = "Measurement Unit 1",
            TypeName = "MMXU",
            OriginalName = "MeasurementBlock1",
            PtnPath = "IED_001/MMXU1",
            IsDeletable = false
        };
        _functionBlocks.Add(fb1_mmxu1);

        var fb1_xcbr1 = new FunctionBlockDto
        {
            PublicTechnicalName = "XCBR1",
            DisplayText = "Circuit Breaker 1",
            TypeName = "XCBR",
            OriginalName = "CircuitBreaker1",
            PtnPath = "IED_001/XCBR1",
            IsDeletable = false
        };
        _functionBlocks.Add(fb1_xcbr1);

        // Signals for MMXU1
        var signal1_totw = new SignalDto
        {
            PublicTechnicalName = "TotW",
            DisplayText = "Total Active Power",
            PtnPath = "IED_001/MMXU1/TotW",
            Type = "Analog",
            TypeName = "MeasuredValue",
            CDCType = "MV",
            IsDeletable = true
        };
        _signals.Add(signal1_totw);

        var signal1_totvar = new SignalDto
        {
            PublicTechnicalName = "TotVAr",
            DisplayText = "Total Reactive Power",
            PtnPath = "IED_001/MMXU1/TotVAr",
            Type = "Analog",
            TypeName = "MeasuredValue",
            CDCType = "MV",
            IsDeletable = true
        };
        _signals.Add(signal1_totvar);

        // CDC Conversion for TotW
        var cdc1_conv = new CdcConversionDto
        {
            PublicTechnicalName = "TotW_Conv",
            DisplayText = "Power Conversion",
            PtnPath = "IED_001/MMXU1/TotW/Conv",
            SourceCdc = "MV",
            TargetCdc = "CMV"
        };
        _cdcConversions.Add(cdc1_conv);

        // Routing for TotW
        var routing1 = new RoutingDto
        {
            PtnPath = "IED_001/MMXU1/TotW/Route1",
            IsReadonly = false,
            Value = "Local",
            Options = new List<string> { "Local", "Remote", "Disabled" }
        };
        _routings.Add(routing1);

        // Signal for XCBR1
        var signal1_pos = new SignalDto
        {
            PublicTechnicalName = "Pos",
            DisplayText = "Position",
            PtnPath = "IED_001/XCBR1/Pos",
            Type = "Status",
            TypeName = "StatusValue",
            CDCType = "DPC",
            IsDeletable = false
        };
        _signals.Add(signal1_pos);

        // Subsignal for Pos
        var subsignal1_stval = new SubsignalDto
        {
            PublicTechnicalName = "stVal",
            DisplayText = "Status Value",
            PtnPath = "IED_001/XCBR1/Pos/stVal",
            TypeName = "Boolean",
            CDCType = "SPS",
            IsDeletable = false
        };
        _subsignals.Add(subsignal1_stval);

        // ============================================
        // DEVICE 2: IED_002 - Control Unit
        // ============================================
        var device2 = new DeviceApplicationDto
        {
            PublicTechnicalName = "IED_002",
            DisplayText = "Intelligent Electronic Device 002",
            TypeName = "ControlUnit",
            LastUpdatedAt = DateTime.UtcNow.AddDays(-5),
            LastModifiedBy = "admin",
            DddVersion = "2.1.0",
            ComDddVersion = "2.1.0"
        };
        _deviceApplications.Add(device2);

        // Function Group for Device 2
        var fg2_ctrl = new FunctionGroupDto
        {
            PublicTechnicalName = "CTRL",
            DisplayText = "Control",
            TypeName = "ControlGroup",
            PTNPath = "IED_002/CTRL",
            IsDeletable = false
        };
        _functionGroups.Add(fg2_ctrl);

        // Function Block for Device 2
        var fb2_cswi1 = new FunctionBlockDto
        {
            PublicTechnicalName = "CSWI1",
            DisplayText = "Switch Controller 1",
            TypeName = "CSWI",
            OriginalName = "SwitchControl1",
            PtnPath = "IED_002/CSWI1",
            IsDeletable = true
        };
        _functionBlocks.Add(fb2_cswi1);

        // Signal for CSWI1
        var signal2_pos = new SignalDto
        {
            PublicTechnicalName = "Pos",
            DisplayText = "Switch Position",
            PtnPath = "IED_002/CSWI1/Pos",
            Type = "Control",
            TypeName = "ControlValue",
            CDCType = "SPC",
            IsDeletable = true
        };
        _signals.Add(signal2_pos);

        // ============================================
        // DEVICE 3: IED_003 - Bay Controller
        // ============================================
        var device3 = new DeviceApplicationDto
        {
            PublicTechnicalName = "IED_003",
            DisplayText = "Intelligent Electronic Device 003",
            TypeName = "BayController",
            LastUpdatedAt = DateTime.UtcNow.AddDays(-2),
            LastModifiedBy = "engineer",
            DddVersion = "1.5.2",
            ComDddVersion = "1.5.0"
        };
        _deviceApplications.Add(device3);

        // Function Block for Device 3
        var fb3_mmtr1 = new FunctionBlockDto
        {
            PublicTechnicalName = "MMTR1",
            DisplayText = "Metering 1",
            TypeName = "MMTR",
            OriginalName = "MeteringBlock1",
            PtnPath = "IED_003/MMTR1",
            IsDeletable = true
        };
        _functionBlocks.Add(fb3_mmtr1);

        // Signal for MMTR1
        var signal3_supwh = new SignalDto
        {
            PublicTechnicalName = "SupWh",
            DisplayText = "Supplied Energy",
            PtnPath = "IED_003/MMTR1/SupWh",
            Type = "Analog",
            TypeName = "EnergyValue",
            CDCType = "BCR",
            IsDeletable = true
        };
        _signals.Add(signal3_supwh);

        // Routing for SupWh
        var routing3 = new RoutingDto
        {
            PtnPath = "IED_003/MMTR1/SupWh/Route1",
            IsReadonly = true,
            Value = "Enabled"
        };
        _routings.Add(routing3);
    }
}
