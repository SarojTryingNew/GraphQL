using HotChocolate.Types;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.Query;

/// <summary>
/// Extended queries for DeviceApplication domain (Scenarios 3-13)
/// </summary>
[ExtendObjectType(typeof(Query))]
public class DeviceApplicationExtendedQueries
{
    /// <summary>
    /// Scenario 3: Get Function Groups by Device
    /// </summary>
    public IEnumerable<FunctionGroupDto> GetFunctionGroups([Service] DataStore dataStore, string devicePublicTechnicalName)
    {
        return dataStore.FunctionGroups.Where(fg => fg.PTNPath.StartsWith(devicePublicTechnicalName + "/"));
    }

    /// <summary>
    /// Scenario 4: Get Single Function Group
    /// </summary>
    public FunctionGroupDto? GetFunctionGroup([Service] DataStore dataStore, string ptnPath)
    {
        return dataStore.FunctionGroups.FirstOrDefault(fg => fg.PTNPath == ptnPath);
    }

    /// <summary>
    /// Scenario 5: Get Function Blocks by Device
    /// </summary>
    public IEnumerable<FunctionBlockDto> GetFunctionBlocks([Service] DataStore dataStore, string devicePublicTechnicalName)
    {
        return dataStore.FunctionBlocks.Where(fb => fb.PtnPath.StartsWith(devicePublicTechnicalName + "/"));
    }

    /// <summary>
    /// Scenario 6: Get Single Function Block
    /// </summary>
    public FunctionBlockDto? GetFunctionBlock([Service] DataStore dataStore, string ptnPath)
    {
        return dataStore.FunctionBlocks.FirstOrDefault(fb => fb.PtnPath == ptnPath);
    }

    /// <summary>
    /// Scenario 7: Get Signals by Function Block
    /// </summary>
    public IEnumerable<SignalDto> GetSignals([Service] DataStore dataStore, string functionBlockPtnPath)
    {
        return dataStore.Signals.Where(s => s.PtnPath.StartsWith(functionBlockPtnPath + "/"));
    }

    /// <summary>
    /// Scenario 8: Get All PTN Paths for a Device
    /// </summary>
    public IEnumerable<string> GetAllPtnPaths([Service] DataStore dataStore, string devicePublicTechnicalName)
    {
        var paths = new List<string> { devicePublicTechnicalName };

        // Add FunctionGroup paths
        paths.AddRange(dataStore.FunctionGroups
            .Where(fg => fg.PTNPath.StartsWith(devicePublicTechnicalName + "/"))
            .Select(fg => fg.PTNPath));

        // Add Function paths
        paths.AddRange(dataStore.Functions
            .Where(f => f.PTNPath.StartsWith(devicePublicTechnicalName + "/"))
            .Select(f => f.PTNPath));

        // Add FunctionBlock paths
        paths.AddRange(dataStore.FunctionBlocks
            .Where(fb => fb.PtnPath.StartsWith(devicePublicTechnicalName + "/"))
            .Select(fb => fb.PtnPath));

        // Add Signal paths
        paths.AddRange(dataStore.Signals
            .Where(s => s.PtnPath.StartsWith(devicePublicTechnicalName + "/"))
            .Select(s => s.PtnPath));

        // Add Subsignal paths
        paths.AddRange(dataStore.Subsignals
            .Where(ss => ss.PtnPath.StartsWith(devicePublicTechnicalName + "/"))
            .Select(ss => ss.PtnPath));

        return paths.Distinct().OrderBy(p => p);
    }

    /// <summary>
    /// Scenario 9: Get All Functions
    /// </summary>
    public IEnumerable<FunctionDto> GetFunctions([Service] DataStore dataStore)
    {
        return dataStore.Functions;
    }

    /// <summary>
    /// Scenario 10: Get Single Function
    /// </summary>
    public FunctionDto? GetFunction([Service] DataStore dataStore, string ptnPath)
    {
        return dataStore.Functions.FirstOrDefault(f => f.PTNPath == ptnPath);
    }

    /// <summary>
    /// Scenario 11: Get Subsignals by Signal
    /// </summary>
    public IEnumerable<SubsignalDto> GetSubsignals([Service] DataStore dataStore, string signalPtnPath)
    {
        return dataStore.Subsignals.Where(ss => ss.PtnPath.StartsWith(signalPtnPath + "/"));
    }

    /// <summary>
    /// Scenario 12: Get CDC Conversions by Signal
    /// </summary>
    public IEnumerable<CdcConversionDto> GetCdcConversions([Service] DataStore dataStore, string signalPtnPath)
    {
        return dataStore.CdcConversions.Where(cdc => cdc.PtnPath.StartsWith(signalPtnPath + "/"));
    }

    /// <summary>
    /// Scenario 13: Get Routings by PTN Path (returns routings where the routing's PtnPath starts with the given path)
    /// </summary>
    public IEnumerable<RoutingDto> GetRoutings([Service] DataStore dataStore, string ptnPath)
    {
        return dataStore.Routings.Where(r => r.PtnPath.StartsWith(ptnPath + "/") || r.PtnPath == ptnPath);
    }

    /// <summary>
    /// Scenario 14: Type-Based Device Grouping
    /// Business Value: Filter devices by type (ProtectionRelay, ControlUnit, etc.) in single query
    /// Allows clients to get devices of specific types without fetching all devices
    /// </summary>
    public IEnumerable<DeviceApplicationDto> GetDeviceApplicationsByType([Service] DataStore dataStore, string typeName)
    {
        return dataStore.DeviceApplications.Where(d => d.TypeName == typeName);
    }

    /// <summary>
    /// Scenario 15: Type-Based Device Grouping (Multiple Types)
    /// Business Value: Filter devices by multiple types in a single query
    /// Example: Get all ProtectionRelays AND ControlUnits at once
    /// </summary>
    public IEnumerable<DeviceApplicationDto> GetDeviceApplicationsByTypes([Service] DataStore dataStore, List<string> typeNames)
    {
        return dataStore.DeviceApplications.Where(d => typeNames.Contains(d.TypeName));
    }

    /// <summary>
    /// Scenario 16: Get All Unique Device Types
    /// Business Value: Discover available device types for filtering UI
    /// Returns distinct list of all TypeNames in the system
    /// </summary>
    public IEnumerable<string> GetDeviceTypes([Service] DataStore dataStore)
    {
        return dataStore.DeviceApplications.Select(d => d.TypeName).Distinct().OrderBy(t => t);
    }
}
