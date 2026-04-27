
namespace RestVsGraphQL.DTOs
{

    public record DeviceApplicationDto
    {
        public required string PublicTechnicalName { get; set; }

        public string DisplayText { get; set; } = string.Empty;

        public required string TypeName { get; set; }

        public IList<FunctionGroupDto>? FunctionGroups { get; set; }

        public IList<FunctionBlockDto>? FunctionBlocks { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public string? LastModifiedBy { get; set; }

        public string? DddVersion { get; set; }

        public string? ComDddVersion { get; set; }
    }



    public record FunctionGroupDto
    {
        public required string PublicTechnicalName { get; set; }

        public required string DisplayText { get; set; }

        public required string TypeName { get; set; }

        public required string PTNPath { get; set; }

        public IList<FunctionBlockDto>? FunctionBlocks { get; set; }

        public IList<FunctionDto>? Functions { get; set; }

        public bool IsDeletable { get; set; }

    }




    public record FunctionBlockDto
    {
        public required string PublicTechnicalName { get; set; }

        public required string DisplayText { get; set; }

        public required string TypeName { get; set; }

        public required string OriginalName { get; set; }

        /// <summary>
        /// PTN path of the function block starting from and including the device 
        /// </summary>

        public required string PtnPath { get; set; }

        public bool IsDeletable { get; set; }

        public IList<SignalDto>? Signals { get; set; }

    }



    public record SignalDto : SubsignalDto
    {
        public required string Type { get; set; }

        public List<CdcConversionDto>? CdcConversions { get; set; }

        public List<SubsignalDto>? SubSignals { get; set; }
    }





    public record CdcConversionDto : SignalBaseDto
    {
        public required string SourceCdc { get; set; }

        public required string TargetCdc { get; set; }
    }





    public record SubsignalDto : SignalBaseDto
    {
        public required string TypeName { get; set; }

        public required string CDCType { get; set; }

        public bool IsDeletable { get; set; }
    }





    public record FunctionDto
    {
        public required string PublicTechnicalName { get; set; }

        public required string DisplayText { get; set; }

        public required string TypeName { get; set; }

        public required string PTNPath { get; set; }

        public bool IsDeletable { get; set; }

        public IList<FunctionBlockDto>? FunctionBlocks { get; set; }

        public IList<SignalDto>? Signals { get; set; }
    }



    public record SignalBaseDto
    {
        public required string PublicTechnicalName { get; set; }

        public required string DisplayText { get; set; }

        public required string PtnPath { get; set; }

        public List<RoutingDto>? Routings { get; set; }
    }




    public record RoutingDto
    {
        public required string PtnPath { get; set; }

        public bool IsReadonly { get; set; } = true;

        public string? Value { get; set; }

        public List<string>? Options { get; set; }
    }
}
