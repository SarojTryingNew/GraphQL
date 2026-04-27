using RestVsGraphQL.DTOs;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.DataLoaders.ECO;

/// <summary>
/// DataLoader for batching FunctionGroup lookups by device PublicTechnicalName
/// </summary>
public class FunctionGroupsByDeviceDataLoader : GroupedDataLoader<string, FunctionGroupDto>
{
    private readonly DataStore _dataStore;

    public FunctionGroupsByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, FunctionGroupDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var functionGroups = _dataStore.FunctionGroups
            .Where(fg => deviceNames.Any(d => fg.PTNPath.StartsWith($"{d}/")))
            .ToLookup(fg => fg.PTNPath.Split('/')[0]);

        return Task.FromResult(functionGroups);
    }
}

/// <summary>
/// DataLoader for batching FunctionBlock lookups by device PublicTechnicalName
/// </summary>
public class FunctionBlocksByDeviceDataLoader : GroupedDataLoader<string, FunctionBlockDto>
{
    private readonly DataStore _dataStore;

    public FunctionBlocksByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, FunctionBlockDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var functionBlocks = _dataStore.FunctionBlocks
            .Where(fb => deviceNames.Any(d => fb.PtnPath.StartsWith($"{d}/")))
            .ToLookup(fb => fb.PtnPath.Split('/')[0]);

        return Task.FromResult(functionBlocks);
    }
}

/// <summary>
/// DataLoader for batching Function lookups by FunctionGroup PTNPath
/// </summary>
public class FunctionsByFunctionGroupDataLoader : GroupedDataLoader<string, FunctionDto>
{
    private readonly DataStore _dataStore;

    public FunctionsByFunctionGroupDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, FunctionDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> functionGroupPaths,
        CancellationToken cancellationToken)
    {
        var functions = _dataStore.Functions
            .Where(f => functionGroupPaths.Any(fg => f.PTNPath.StartsWith($"{fg}/")))
            .ToLookup(f =>
            {
                var parts = f.PTNPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(functions);
    }
}

/// <summary>
/// DataLoader for batching Signal lookups by FunctionBlock PtnPath
/// </summary>
public class SignalsByFunctionBlockDataLoader : GroupedDataLoader<string, SignalDto>
{
    private readonly DataStore _dataStore;

    public SignalsByFunctionBlockDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, SignalDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> functionBlockPaths,
        CancellationToken cancellationToken)
    {
        var signals = _dataStore.Signals
            .Where(s => functionBlockPaths.Any(fb => s.PtnPath.StartsWith($"{fb}/") &&
                       s.PtnPath.Split('/').Length == fb.Split('/').Length + 1))
            .ToLookup(s =>
            {
                var parts = s.PtnPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(signals);
    }
}

/// <summary>
/// DataLoader for batching Subsignal lookups by Signal PtnPath
/// </summary>
public class SubsignalsBySignalDataLoader : GroupedDataLoader<string, SubsignalDto>
{
    private readonly DataStore _dataStore;

    public SubsignalsBySignalDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, SubsignalDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> signalPaths,
        CancellationToken cancellationToken)
    {
        var subsignals = _dataStore.Subsignals
            .Where(ss => signalPaths.Any(s => ss.PtnPath.StartsWith($"{s}/")))
            .ToLookup(ss =>
            {
                var parts = ss.PtnPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(subsignals);
    }
}

/// <summary>
/// DataLoader for batching CdcConversion lookups by Signal PtnPath
/// </summary>
public class CdcConversionsBySignalDataLoader : GroupedDataLoader<string, CdcConversionDto>
{
    private readonly DataStore _dataStore;

    public CdcConversionsBySignalDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, CdcConversionDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> signalPaths,
        CancellationToken cancellationToken)
    {
        var cdcConversions = _dataStore.CdcConversions
            .Where(c => signalPaths.Any(s => c.PtnPath.StartsWith($"{s}/")))
            .ToLookup(c =>
            {
                var parts = c.PtnPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(cdcConversions);
    }
}

/// <summary>
/// DataLoader for batching Routing lookups by parent PtnPath (Signal, Subsignal, or CdcConversion)
/// </summary>
public class RoutingsByParentPathDataLoader : GroupedDataLoader<string, RoutingDto>
{
    private readonly DataStore _dataStore;

    public RoutingsByParentPathDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, RoutingDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> parentPaths,
        CancellationToken cancellationToken)
    {
        var routings = _dataStore.Routings
            .Where(r => parentPaths.Any(p => r.PtnPath.StartsWith($"{p}/")))
            .ToLookup(r =>
            {
                var parts = r.PtnPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(routings);
    }
}

/// <summary>
/// DataLoader for batching FunctionBlock lookups by FunctionGroup PTNPath
/// </summary>
public class FunctionBlocksByFunctionGroupDataLoader : GroupedDataLoader<string, FunctionBlockDto>
{
    private readonly DataStore _dataStore;

    public FunctionBlocksByFunctionGroupDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, FunctionBlockDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> functionGroupPaths,
        CancellationToken cancellationToken)
    {
        var functionBlocks = _dataStore.FunctionBlocks
            .Where(fb => functionGroupPaths.Any(fg => fb.PtnPath.StartsWith($"{fg}/")))
            .ToLookup(fb =>
            {
                var parts = fb.PtnPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(functionBlocks);
    }
}

/// <summary>
/// DataLoader for batching FunctionBlock lookups by Function PTNPath
/// </summary>
public class FunctionBlocksByFunctionDataLoader : GroupedDataLoader<string, FunctionBlockDto>
{
    private readonly DataStore _dataStore;

    public FunctionBlocksByFunctionDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, FunctionBlockDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> functionPaths,
        CancellationToken cancellationToken)
    {
        var functionBlocks = _dataStore.FunctionBlocks
            .Where(fb => functionPaths.Any(f => fb.PtnPath.StartsWith($"{f}/")))
            .ToLookup(fb =>
            {
                var parts = fb.PtnPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(functionBlocks);
    }
}

/// <summary>
/// DataLoader for batching Signal lookups by Function PTNPath
/// </summary>
public class SignalsByFunctionDataLoader : GroupedDataLoader<string, SignalDto>
{
    private readonly DataStore _dataStore;

    public SignalsByFunctionDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<ILookup<string, SignalDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> functionPaths,
        CancellationToken cancellationToken)
    {
        var signals = _dataStore.Signals
            .Where(s => functionPaths.Any(f => s.PtnPath.StartsWith($"{f}/")))
            .ToLookup(s =>
            {
                var parts = s.PtnPath.Split('/');
                return string.Join('/', parts.Take(parts.Length - 1));
            });

        return Task.FromResult(signals);
    }
}

// ===== Scenario 7: Statistical DataLoaders for Device Statistics Dashboard =====

/// <summary>
/// DataLoader for batching FunctionGroup count lookups by device PublicTechnicalName
/// </summary>
public class FunctionGroupCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public FunctionGroupCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.FunctionGroups
            .Where(fg => deviceNames.Any(d => fg.PTNPath.StartsWith($"{d}/")))
            .GroupBy(fg => fg.PTNPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        // Ensure all requested devices have an entry (even if count is 0)
        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching FunctionBlock count lookups by device PublicTechnicalName
/// </summary>
public class FunctionBlockCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public FunctionBlockCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.FunctionBlocks
            .Where(fb => deviceNames.Any(d => fb.PtnPath.StartsWith($"{d}/")))
            .GroupBy(fb => fb.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching Function count lookups by device PublicTechnicalName
/// </summary>
public class FunctionCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public FunctionCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Functions
            .Where(f => deviceNames.Any(d => f.PTNPath.StartsWith($"{d}/")))
            .GroupBy(f => f.PTNPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching Signal count lookups by device PublicTechnicalName
/// </summary>
public class SignalCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public SignalCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Signals
            .Where(s => deviceNames.Any(d => s.PtnPath.StartsWith($"{d}/")))
            .GroupBy(s => s.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching Analog Signal count lookups by device PublicTechnicalName
/// </summary>
public class AnalogSignalCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public AnalogSignalCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Signals
            .Where(s => deviceNames.Any(d => s.PtnPath.StartsWith($"{d}/")) && s.Type == "Analog")
            .GroupBy(s => s.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching Status Signal count lookups by device PublicTechnicalName
/// </summary>
public class StatusSignalCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public StatusSignalCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Signals
            .Where(s => deviceNames.Any(d => s.PtnPath.StartsWith($"{d}/")) && s.Type == "Status")
            .GroupBy(s => s.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching Subsignal count lookups by device PublicTechnicalName
/// </summary>
public class SubsignalCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public SubsignalCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Subsignals
            .Where(ss => deviceNames.Any(d => ss.PtnPath.StartsWith($"{d}/")))
            .GroupBy(ss => ss.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching CdcConversion count lookups by device PublicTechnicalName
/// </summary>
public class CdcConversionCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public CdcConversionCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.CdcConversions
            .Where(c => deviceNames.Any(d => c.PtnPath.StartsWith($"{d}/")))
            .GroupBy(c => c.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching Routing count lookups by device PublicTechnicalName
/// </summary>
public class RoutingCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public RoutingCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Routings
            .Where(r => deviceNames.Any(d => r.PtnPath.StartsWith($"{d}/")))
            .GroupBy(r => r.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching HasConfiguration flag lookups by device PublicTechnicalName
/// </summary>
public class HasConfigurationByDeviceDataLoader : BatchDataLoader<string, bool>
{
    private readonly DataStore _dataStore;

    public HasConfigurationByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, bool>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var hasConfig = _dataStore.Routings
            .Where(r => deviceNames.Any(d => r.PtnPath.StartsWith($"{d}/")))
            .Select(r => r.PtnPath.Split('/')[0])
            .Distinct()
            .ToDictionary(device => device, _ => true);

        // Ensure all requested devices have an entry (false if no routings)
        foreach (var device in deviceNames)
        {
            hasConfig.TryAdd(device, false);
        }

        return Task.FromResult<IReadOnlyDictionary<string, bool>>(hasConfig);
    }
}

/// <summary>
/// DataLoader for batching Configured Routing count lookups by device PublicTechnicalName
/// </summary>
public class ConfiguredRoutingCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public ConfiguredRoutingCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Routings
            .Where(r => deviceNames.Any(d => r.PtnPath.StartsWith($"{d}/")) && 
                       !string.IsNullOrEmpty(r.Value))
            .GroupBy(r => r.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}

/// <summary>
/// DataLoader for batching Editable Routing count lookups by device PublicTechnicalName
/// </summary>
public class EditableRoutingCountByDeviceDataLoader : BatchDataLoader<string, int>
{
    private readonly DataStore _dataStore;

    public EditableRoutingCountByDeviceDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override Task<IReadOnlyDictionary<string, int>> LoadBatchAsync(
        IReadOnlyList<string> deviceNames,
        CancellationToken cancellationToken)
    {
        var counts = _dataStore.Routings
            .Where(r => deviceNames.Any(d => r.PtnPath.StartsWith($"{d}/")) && 
                       !r.IsReadonly)
            .GroupBy(r => r.PtnPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var device in deviceNames)
        {
            counts.TryAdd(device, 0);
        }

        return Task.FromResult<IReadOnlyDictionary<string, int>>(counts);
    }
}
