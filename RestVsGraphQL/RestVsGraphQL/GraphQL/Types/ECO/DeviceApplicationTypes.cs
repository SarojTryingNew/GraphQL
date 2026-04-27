using HotChocolate.Types;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.GraphQL.DataLoaders.ECO;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.Types.ECO;

/// <summary>
/// GraphQL type definition for DeviceApplicationDto with field resolvers and computed statistics
/// </summary>
public class DeviceApplicationType : ObjectType<DeviceApplicationDto>
{
    protected override void Configure(IObjectTypeDescriptor<DeviceApplicationDto> descriptor)
    {
        descriptor.Name("DeviceApplication");

        descriptor
            .Field(d => d.PublicTechnicalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(d => d.DisplayText)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(d => d.TypeName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(d => d.LastUpdatedAt)
            .Type<DateTimeType>();

        descriptor
            .Field(d => d.LastModifiedBy)
            .Type<StringType>();

        descriptor
            .Field(d => d.DddVersion)
            .Type<StringType>();

        descriptor
            .Field(d => d.ComDddVersion)
            .Type<StringType>();

        // Field resolver for FunctionGroups
        descriptor
            .Field(d => d.FunctionGroups)
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetFunctionGroups(default!, default!))
            .Type<ListType<FunctionGroupType>>();

        // Field resolver for FunctionBlocks
        descriptor
            .Field(d => d.FunctionBlocks)
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetFunctionBlocks(default!, default!))
            .Type<ListType<FunctionBlockType>>();

        // ===== Scenario 7: Device Statistics Dashboard - Computed Fields =====

        // Computed field: Function Group Count
        descriptor
            .Field("functionGroupCount")
            .Type<IntType>()
            .Description("Total number of function groups in this device")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetFunctionGroupCount(default!, default!));

        // Computed field: Function Block Count
        descriptor
            .Field("functionBlockCount")
            .Type<IntType>()
            .Description("Total number of function blocks in this device")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetFunctionBlockCount(default!, default!));

        // Computed field: Function Count
        descriptor
            .Field("functionCount")
            .Type<IntType>()
            .Description("Total number of functions in this device")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetFunctionCount(default!, default!));

        // Computed field: Total Signal Count
        descriptor
            .Field("signalCount")
            .Type<IntType>()
            .Description("Total number of signals across all function blocks")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetSignalCount(default!, default!));

        // Computed field: Analog Signal Count
        descriptor
            .Field("analogSignalCount")
            .Type<IntType>()
            .Description("Number of analog type signals")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetAnalogSignalCount(default!, default!));

        // Computed field: Status Signal Count
        descriptor
            .Field("statusSignalCount")
            .Type<IntType>()
            .Description("Number of status type signals")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetStatusSignalCount(default!, default!));

        // Computed field: Subsignal Count
        descriptor
            .Field("subsignalCount")
            .Type<IntType>()
            .Description("Total number of subsignals")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetSubsignalCount(default!, default!));

        // Computed field: CDC Conversion Count
        descriptor
            .Field("cdcConversionCount")
            .Type<IntType>()
            .Description("Total number of CDC conversions")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetCdcConversionCount(default!, default!));

        // Computed field: Routing Count
        descriptor
            .Field("routingCount")
            .Type<IntType>()
            .Description("Total number of routings")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetRoutingCount(default!, default!));

        // Computed field: Has Configuration
        descriptor
            .Field("hasConfiguration")
            .Type<BooleanType>()
            .Description("Indicates if device has any routing configurations")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetHasConfiguration(default!, default!));

        // Computed field: Configured Routing Count
        descriptor
            .Field("configuredRoutingCount")
            .Type<IntType>()
            .Description("Number of routings that have a configured value")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetConfiguredRoutingCount(default!, default!));

        // Computed field: Editable Routing Count
        descriptor
            .Field("editableRoutingCount")
            .Type<IntType>()
            .Description("Number of routings that are editable (not read-only)")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetEditableRoutingCount(default!, default!));
    }

    private class DeviceApplicationResolvers
    {
        public async Task<IEnumerable<FunctionGroupDto>> GetFunctionGroups(
            [Parent] DeviceApplicationDto device,
            FunctionGroupsByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<IEnumerable<FunctionBlockDto>> GetFunctionBlocks(
            [Parent] DeviceApplicationDto device,
            FunctionBlocksByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        // ===== Scenario 7: Statistics Resolvers =====

        public async Task<int> GetFunctionGroupCount(
            [Parent] DeviceApplicationDto device,
            FunctionGroupCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetFunctionBlockCount(
            [Parent] DeviceApplicationDto device,
            FunctionBlockCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetFunctionCount(
            [Parent] DeviceApplicationDto device,
            FunctionCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetSignalCount(
            [Parent] DeviceApplicationDto device,
            SignalCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetAnalogSignalCount(
            [Parent] DeviceApplicationDto device,
            AnalogSignalCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetStatusSignalCount(
            [Parent] DeviceApplicationDto device,
            StatusSignalCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetSubsignalCount(
            [Parent] DeviceApplicationDto device,
            SubsignalCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetCdcConversionCount(
            [Parent] DeviceApplicationDto device,
            CdcConversionCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetRoutingCount(
            [Parent] DeviceApplicationDto device,
            RoutingCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<bool> GetHasConfiguration(
            [Parent] DeviceApplicationDto device,
            HasConfigurationByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetConfiguredRoutingCount(
            [Parent] DeviceApplicationDto device,
            ConfiguredRoutingCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }

        public async Task<int> GetEditableRoutingCount(
            [Parent] DeviceApplicationDto device,
            EditableRoutingCountByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }
    }
}

/// <summary>
/// GraphQL type definition for FunctionGroupDto with field resolvers
/// </summary>
public class FunctionGroupType : ObjectType<FunctionGroupDto>
{
    protected override void Configure(IObjectTypeDescriptor<FunctionGroupDto> descriptor)
    {
        descriptor.Name("FunctionGroup");

        descriptor
            .Field(fg => fg.PublicTechnicalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fg => fg.DisplayText)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fg => fg.TypeName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fg => fg.PTNPath)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fg => fg.IsDeletable)
            .Type<NonNullType<BooleanType>>();

        // Field resolver for Functions
        descriptor
            .Field(fg => fg.Functions)
            .ResolveWith<FunctionGroupResolvers>(r => r.GetFunctions(default!, default!))
            .Type<ListType<FunctionType>>();

        // Field resolver for FunctionBlocks
        descriptor
            .Field(fg => fg.FunctionBlocks)
            .ResolveWith<FunctionGroupResolvers>(r => r.GetFunctionBlocks(default!, default!))
            .Type<ListType<FunctionBlockType>>();
    }

    private class FunctionGroupResolvers
    {
        public async Task<IEnumerable<FunctionDto>> GetFunctions(
            [Parent] FunctionGroupDto functionGroup,
            FunctionsByFunctionGroupDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(functionGroup.PTNPath);
        }

        public async Task<IEnumerable<FunctionBlockDto>> GetFunctionBlocks(
            [Parent] FunctionGroupDto functionGroup,
            FunctionBlocksByFunctionGroupDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(functionGroup.PTNPath);
        }
    }
}

/// <summary>
/// GraphQL type definition for FunctionDto with field resolvers
/// </summary>
public class FunctionType : ObjectType<FunctionDto>
{
    protected override void Configure(IObjectTypeDescriptor<FunctionDto> descriptor)
    {
        descriptor.Name("Function");

        descriptor
            .Field(f => f.PublicTechnicalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(f => f.DisplayText)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(f => f.TypeName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(f => f.PTNPath)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(f => f.IsDeletable)
            .Type<NonNullType<BooleanType>>();

        // Field resolver for FunctionBlocks
        descriptor
            .Field(f => f.FunctionBlocks)
            .ResolveWith<FunctionResolvers>(r => r.GetFunctionBlocks(default!, default!))
            .Type<ListType<FunctionBlockType>>();

        // Field resolver for Signals
        descriptor
            .Field(f => f.Signals)
            .ResolveWith<FunctionResolvers>(r => r.GetSignals(default!, default!))
            .Type<ListType<SignalType>>();
    }

    private class FunctionResolvers
    {
        public async Task<IEnumerable<FunctionBlockDto>> GetFunctionBlocks(
            [Parent] FunctionDto function,
            FunctionBlocksByFunctionDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(function.PTNPath);
        }

        public async Task<IEnumerable<SignalDto>> GetSignals(
            [Parent] FunctionDto function,
            SignalsByFunctionDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(function.PTNPath);
        }
    }
}

/// <summary>
/// GraphQL type definition for FunctionBlockDto with field resolvers
/// </summary>
public class FunctionBlockType : ObjectType<FunctionBlockDto>
{
    protected override void Configure(IObjectTypeDescriptor<FunctionBlockDto> descriptor)
    {
        descriptor.Name("FunctionBlock");

        descriptor
            .Field(fb => fb.PublicTechnicalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fb => fb.DisplayText)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fb => fb.TypeName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fb => fb.OriginalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fb => fb.PtnPath)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(fb => fb.IsDeletable)
            .Type<NonNullType<BooleanType>>();

        // Field resolver for Signals
        descriptor
            .Field(fb => fb.Signals)
            .ResolveWith<FunctionBlockResolvers>(r => r.GetSignals(default!, default!))
            .Type<ListType<SignalType>>();
    }

    private class FunctionBlockResolvers
    {
        public async Task<IEnumerable<SignalDto>> GetSignals(
            [Parent] FunctionBlockDto functionBlock,
            SignalsByFunctionBlockDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(functionBlock.PtnPath);
        }
    }
}

/// <summary>
/// GraphQL type definition for SignalDto with field resolvers
/// </summary>
public class SignalType : ObjectType<SignalDto>
{
    protected override void Configure(IObjectTypeDescriptor<SignalDto> descriptor)
    {
        descriptor.Name("Signal");

        // Inherit from SubsignalDto fields
        descriptor
            .Field(s => s.PublicTechnicalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(s => s.DisplayText)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(s => s.PtnPath)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(s => s.TypeName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(s => s.CDCType)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(s => s.IsDeletable)
            .Type<NonNullType<BooleanType>>();

        // Signal-specific fields
        descriptor
            .Field(s => s.Type)
            .Type<NonNullType<StringType>>();

        // Field resolvers for relationships
        descriptor
            .Field(s => s.CdcConversions)
            .ResolveWith<SignalResolvers>(r => r.GetCdcConversions(default!, default!))
            .Type<ListType<CdcConversionType>>();

        descriptor
            .Field(s => s.SubSignals)
            .ResolveWith<SignalResolvers>(r => r.GetSubsignals(default!, default!))
            .Type<ListType<SubsignalType>>();

        descriptor
            .Field(s => s.Routings)
            .ResolveWith<SignalResolvers>(r => r.GetRoutings(default!, default!))
            .Type<ListType<RoutingType>>();
    }

    private class SignalResolvers
    {
        public async Task<IEnumerable<CdcConversionDto>> GetCdcConversions(
            [Parent] SignalDto signal,
            CdcConversionsBySignalDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(signal.PtnPath);
        }

        public async Task<IEnumerable<SubsignalDto>> GetSubsignals(
            [Parent] SignalDto signal,
            SubsignalsBySignalDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(signal.PtnPath);
        }

        public async Task<IEnumerable<RoutingDto>> GetRoutings(
            [Parent] SignalDto signal,
            RoutingsByParentPathDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(signal.PtnPath);
        }
    }
}

/// <summary>
/// GraphQL type definition for SubsignalDto
/// </summary>
public class SubsignalType : ObjectType<SubsignalDto>
{
    protected override void Configure(IObjectTypeDescriptor<SubsignalDto> descriptor)
    {
        descriptor.Name("Subsignal");

        descriptor
            .Field(ss => ss.PublicTechnicalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(ss => ss.DisplayText)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(ss => ss.PtnPath)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(ss => ss.TypeName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(ss => ss.CDCType)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(ss => ss.IsDeletable)
            .Type<NonNullType<BooleanType>>();

        // Field resolver for Routings
        descriptor
            .Field(ss => ss.Routings)
            .ResolveWith<SubsignalResolvers>(r => r.GetRoutings(default!, default!))
            .Type<ListType<RoutingType>>();
    }

    private class SubsignalResolvers
    {
        public async Task<IEnumerable<RoutingDto>> GetRoutings(
            [Parent] SubsignalDto subsignal,
            RoutingsByParentPathDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(subsignal.PtnPath);
        }
    }
}

/// <summary>
/// GraphQL type definition for CdcConversionDto
/// </summary>
public class CdcConversionType : ObjectType<CdcConversionDto>
{
    protected override void Configure(IObjectTypeDescriptor<CdcConversionDto> descriptor)
    {
        descriptor.Name("CdcConversion");

        descriptor
            .Field(c => c.PublicTechnicalName)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(c => c.DisplayText)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(c => c.PtnPath)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(c => c.SourceCdc)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(c => c.TargetCdc)
            .Type<NonNullType<StringType>>();

        // Field resolver for Routings
        descriptor
            .Field(c => c.Routings)
            .ResolveWith<CdcConversionResolvers>(r => r.GetRoutings(default!, default!))
            .Type<ListType<RoutingType>>();
    }

    private class CdcConversionResolvers
    {
        public async Task<IEnumerable<RoutingDto>> GetRoutings(
            [Parent] CdcConversionDto cdcConversion,
            RoutingsByParentPathDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(cdcConversion.PtnPath);
        }
    }
}

/// <summary>
/// GraphQL type definition for RoutingDto
/// </summary>
public class RoutingType : ObjectType<RoutingDto>
{
    protected override void Configure(IObjectTypeDescriptor<RoutingDto> descriptor)
    {
        descriptor.Name("Routing");

        descriptor
            .Field(r => r.PtnPath)
            .Type<NonNullType<StringType>>();

        descriptor
            .Field(r => r.IsReadonly)
            .Type<NonNullType<BooleanType>>();

        descriptor
            .Field(r => r.Value)
            .Type<StringType>();

        descriptor
            .Field(r => r.Options)
            .Type<ListType<StringType>>();
    }
}
