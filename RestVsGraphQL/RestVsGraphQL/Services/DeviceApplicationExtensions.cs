using RestVsGraphQL.DTOs;

namespace RestVsGraphQL.Services;

public static class DeviceApplicationExtensions
{
    /// <summary>
    /// Load all relationships for a DeviceApplication
    /// </summary>
    public static void LoadRelations(this DeviceApplicationDto device, DataStore dataStore)
    {
        if (device == null) return;

        // Load FunctionGroups for this device
        device.FunctionGroups = dataStore.FunctionGroups
            .Where(fg => fg.PTNPath.StartsWith($"{device.PublicTechnicalName}/"))
            .ToList();

        // Load Functions for each FunctionGroup
        foreach (var fg in device.FunctionGroups ?? Enumerable.Empty<FunctionGroupDto>())
        {
            fg.Functions = dataStore.Functions
                .Where(f => f.PTNPath.StartsWith($"{fg.PTNPath}/"))
                .ToList();

            // Load FunctionBlocks for each FunctionGroup
            fg.FunctionBlocks = dataStore.FunctionBlocks
                .Where(fb => fb.PtnPath.StartsWith($"{fg.PTNPath}/"))
                .ToList();
        }

        // Load FunctionBlocks for this device
        device.FunctionBlocks = dataStore.FunctionBlocks
            .Where(fb => fb.PtnPath.StartsWith($"{device.PublicTechnicalName}/"))
            .ToList();

        // Load Signals for each FunctionBlock
        foreach (var fb in device.FunctionBlocks ?? Enumerable.Empty<FunctionBlockDto>())
        {
            fb.LoadRelations(dataStore);
        }
    }

    /// <summary>
    /// Load relationships for a FunctionBlock
    /// </summary>
    public static void LoadRelations(this FunctionBlockDto functionBlock, DataStore dataStore)
    {
        if (functionBlock == null) return;

        functionBlock.Signals = dataStore.Signals
            .Where(s => s.PtnPath.StartsWith($"{functionBlock.PtnPath}/"))
            .ToList();

        // Load related data for each signal
        foreach (var signal in functionBlock.Signals ?? Enumerable.Empty<SignalDto>())
        {
            signal.LoadRelations(dataStore);
        }
    }

    /// <summary>
    /// Load relationships for a Signal
    /// </summary>
    public static void LoadRelations(this SignalDto signal, DataStore dataStore)
    {
        if (signal == null) return;

        // Load CDC Conversions
        signal.CdcConversions = dataStore.CdcConversions
            .Where(c => c.PtnPath.StartsWith($"{signal.PtnPath}/"))
            .ToList();

        // Load SubSignals
        signal.SubSignals = dataStore.Subsignals
            .Where(ss => ss.PtnPath.StartsWith($"{signal.PtnPath}/"))
            .ToList();

        // Load Routings
        signal.Routings = dataStore.Routings
            .Where(r => r.PtnPath.StartsWith($"{signal.PtnPath}/"))
            .ToList();

        // Load Routings for CDC Conversions
        foreach (var cdc in signal.CdcConversions ?? Enumerable.Empty<CdcConversionDto>())
        {
            cdc.Routings = dataStore.Routings
                .Where(r => r.PtnPath.StartsWith($"{cdc.PtnPath}/"))
                .ToList();
        }

        // Load Routings for SubSignals
        foreach (var ss in signal.SubSignals ?? Enumerable.Empty<SubsignalDto>())
        {
            ss.Routings = dataStore.Routings
                .Where(r => r.PtnPath.StartsWith($"{ss.PtnPath}/"))
                .ToList();
        }
    }

    /// <summary>
    /// Load relationships for a FunctionDto
    /// </summary>
    public static void LoadRelations(this FunctionDto function, DataStore dataStore)
    {
        if (function == null) return;

        // Load FunctionBlocks for this function
        function.FunctionBlocks = dataStore.FunctionBlocks
            .Where(fb => fb.PtnPath.StartsWith($"{function.PTNPath}/"))
            .ToList();

        // Load Signals for this function
        function.Signals = dataStore.Signals
            .Where(s => s.PtnPath.StartsWith($"{function.PTNPath}/"))
            .ToList();

        // Load relations for function blocks
        foreach (var fb in function.FunctionBlocks ?? Enumerable.Empty<FunctionBlockDto>())
        {
            fb.LoadRelations(dataStore);
        }

        // Load relations for signals
        foreach (var signal in function.Signals ?? Enumerable.Empty<SignalDto>())
        {
            signal.LoadRelations(dataStore);
        }
    }

    /// <summary>
    /// Batch load relationships for multiple devices
    /// </summary>
    public static void LoadRelations(this IEnumerable<DeviceApplicationDto> devices, DataStore dataStore)
    {
        foreach (var device in devices)
        {
            device.LoadRelations(dataStore);
        }
    }

    /// <summary>
    /// Get all PTN paths from a device and its nested structures
    /// </summary>
    public static IEnumerable<string> GetAllPtnPaths(this DeviceApplicationDto device, DataStore dataStore)
    {
        var paths = new List<string>();

        // Load relationships first
        device.LoadRelations(dataStore);

        // Add function group paths
        if (device.FunctionGroups != null)
        {
            paths.AddRange(device.FunctionGroups.Select(fg => fg.PTNPath));
            
            foreach (var fg in device.FunctionGroups)
            {
                if (fg.Functions != null)
                    paths.AddRange(fg.Functions.Select(f => f.PTNPath));
            }
        }

        // Add function block paths
        if (device.FunctionBlocks != null)
        {
            paths.AddRange(device.FunctionBlocks.Select(fb => fb.PtnPath));
        }

        // Add signal paths
        var signals = dataStore.Signals
            .Where(s => s.PtnPath.StartsWith($"{device.PublicTechnicalName}/"))
            .ToList();
        
        paths.AddRange(signals.Select(s => s.PtnPath));

        // Add subsignal paths
        foreach (var signal in signals)
        {
            var subsignals = dataStore.Subsignals
                .Where(ss => ss.PtnPath.StartsWith($"{signal.PtnPath}/"))
                .ToList();
            paths.AddRange(subsignals.Select(ss => ss.PtnPath));
        }

        return paths;
    }
}
