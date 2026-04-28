namespace RestVsGraphQL.DTOs;

/// <summary>
/// Input type for creating a complete DeviceApplication with nested hierarchy
/// </summary>
public record CreateDeviceApplicationInput
{
    public required string PublicTechnicalName { get; set; }
    public string DisplayText { get; set; } = string.Empty;
    public required string TypeName { get; set; }
    public string? LastModifiedBy { get; set; }
    public string? DddVersion { get; set; }
    public string? ComDddVersion { get; set; }

    // Nested collections
    public List<CreateFunctionGroupInput>? FunctionGroups { get; set; }
    public List<CreateFunctionBlockInput>? FunctionBlocks { get; set; }
}

/// <summary>
/// Input type for creating FunctionGroup with nested data
/// </summary>
public record CreateFunctionGroupInput
{
    public required string PublicTechnicalName { get; set; }
    public required string DisplayText { get; set; }
    public required string TypeName { get; set; }
    public bool IsDeletable { get; set; } = true;

    // Nested collections
    public List<CreateFunctionInput>? Functions { get; set; }
    public List<CreateFunctionBlockInput>? FunctionBlocks { get; set; }
}

/// <summary>
/// Input type for creating Function with nested data
/// </summary>
public record CreateFunctionInput
{
    public required string PublicTechnicalName { get; set; }
    public required string DisplayText { get; set; }
    public required string TypeName { get; set; }
    public bool IsDeletable { get; set; } = true;

    // Nested collections
    public List<CreateFunctionBlockInput>? FunctionBlocks { get; set; }
}

/// <summary>
/// Input type for creating FunctionBlock with nested data
/// </summary>
public record CreateFunctionBlockInput
{
    public required string PublicTechnicalName { get; set; }
    public required string DisplayText { get; set; }
    public required string TypeName { get; set; }
    public required string OriginalName { get; set; }
    public bool IsDeletable { get; set; } = true;

    // Nested collections
    public List<CreateSignalInput>? Signals { get; set; }
}

/// <summary>
/// Input type for creating Signal with nested data
/// </summary>
public record CreateSignalInput
{
    public required string PublicTechnicalName { get; set; }
    public required string DisplayText { get; set; }
    public required string TypeName { get; set; }
    public required string CDCType { get; set; }
    public required string Type { get; set; }  // "Analog" or "Status"
    public bool IsDeletable { get; set; } = true;

    // Nested collections
    public List<CreateSubsignalInput>? Subsignals { get; set; }
    public List<CreateCdcConversionInput>? CdcConversions { get; set; }
    public List<CreateRoutingInput>? Routings { get; set; }
}

/// <summary>
/// Input type for creating Subsignal with nested data
/// </summary>
public record CreateSubsignalInput
{
    public required string PublicTechnicalName { get; set; }
    public required string DisplayText { get; set; }
    public required string TypeName { get; set; }
    public required string CDCType { get; set; }
    public bool IsDeletable { get; set; } = true;

    // Nested collections
    public List<CreateRoutingInput>? Routings { get; set; }
}

/// <summary>
/// Input type for creating CdcConversion with nested data
/// </summary>
public record CreateCdcConversionInput
{
    public required string PublicTechnicalName { get; set; }
    public required string DisplayText { get; set; }
    public required string SourceCdc { get; set; }
    public required string TargetCdc { get; set; }

    // Nested collections
    public List<CreateRoutingInput>? Routings { get; set; }
}

/// <summary>
/// Input type for creating Routing
/// </summary>
public record CreateRoutingInput
{
    public bool IsReadonly { get; set; }
    public string? Value { get; set; }
    public List<string>? Options { get; set; }
}
