using HotChocolate.Types;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.Mutation;

[ExtendObjectType(typeof(Mutation))]
public class DeviceApplicationMutations
{
    /// <summary>
    /// Scenario 18: Create a complete device application with full hierarchy in a single mutation
    /// </summary>
    public DeviceApplicationDto CreateDeviceApplicationWithHierarchy(
        CreateDeviceApplicationInput input,
        [Service] DataStore dataStore)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input), "Device application input cannot be null");

        if (string.IsNullOrWhiteSpace(input.PublicTechnicalName))
            throw new ArgumentException("PublicTechnicalName is required", nameof(input));

        if (dataStore.DeviceApplications.Any(d => d.PublicTechnicalName == input.PublicTechnicalName))
            throw new ArgumentException($"Device application with PublicTechnicalName '{input.PublicTechnicalName}' already exists");

        // Create the root device
        var device = new DeviceApplicationDto
        {
            PublicTechnicalName = input.PublicTechnicalName,
            DisplayText = input.DisplayText,
            TypeName = input.TypeName,
            LastUpdatedAt = DateTime.UtcNow,
            LastModifiedBy = input.LastModifiedBy,
            DddVersion = input.DddVersion,
            ComDddVersion = input.ComDddVersion
        };

        dataStore.DeviceApplications.Add(device);

        // Process nested FunctionGroups
        if (input.FunctionGroups != null)
        {
            foreach (var fgInput in input.FunctionGroups)
            {
                CreateFunctionGroupRecursive(device.PublicTechnicalName, fgInput, dataStore);
            }
        }

        // Process direct FunctionBlocks (under device)
        if (input.FunctionBlocks != null)
        {
            foreach (var fbInput in input.FunctionBlocks)
            {
                CreateFunctionBlockRecursive(device.PublicTechnicalName, fbInput, dataStore);
            }
        }

        return device;
    }

    private void CreateFunctionGroupRecursive(string parentPath, CreateFunctionGroupInput input, DataStore dataStore)
    {
        var ptnPath = $"{parentPath}/{input.PublicTechnicalName}";

        var functionGroup = new FunctionGroupDto
        {
            PublicTechnicalName = input.PublicTechnicalName,
            DisplayText = input.DisplayText,
            TypeName = input.TypeName,
            PTNPath = ptnPath,
            IsDeletable = input.IsDeletable
        };

        dataStore.FunctionGroups.Add(functionGroup);

        // Process nested Functions
        if (input.Functions != null)
        {
            foreach (var funcInput in input.Functions)
            {
                CreateFunctionRecursive(ptnPath, funcInput, dataStore);
            }
        }

        // Process nested FunctionBlocks
        if (input.FunctionBlocks != null)
        {
            foreach (var fbInput in input.FunctionBlocks)
            {
                CreateFunctionBlockRecursive(ptnPath, fbInput, dataStore);
            }
        }
    }

    private void CreateFunctionRecursive(string parentPath, CreateFunctionInput input, DataStore dataStore)
    {
        var ptnPath = $"{parentPath}/{input.PublicTechnicalName}";

        var function = new FunctionDto
        {
            PublicTechnicalName = input.PublicTechnicalName,
            DisplayText = input.DisplayText,
            TypeName = input.TypeName,
            PTNPath = ptnPath,
            IsDeletable = input.IsDeletable
        };

        dataStore.Functions.Add(function);

        // Process nested FunctionBlocks
        if (input.FunctionBlocks != null)
        {
            foreach (var fbInput in input.FunctionBlocks)
            {
                CreateFunctionBlockRecursive(ptnPath, fbInput, dataStore);
            }
        }
    }

    private void CreateFunctionBlockRecursive(string parentPath, CreateFunctionBlockInput input, DataStore dataStore)
    {
        var ptnPath = $"{parentPath}/{input.PublicTechnicalName}";

        var functionBlock = new FunctionBlockDto
        {
            PublicTechnicalName = input.PublicTechnicalName,
            DisplayText = input.DisplayText,
            TypeName = input.TypeName,
            OriginalName = input.OriginalName,
            PtnPath = ptnPath,
            IsDeletable = input.IsDeletable
        };

        dataStore.FunctionBlocks.Add(functionBlock);

        // Process nested Signals
        if (input.Signals != null)
        {
            foreach (var sigInput in input.Signals)
            {
                CreateSignalRecursive(ptnPath, sigInput, dataStore);
            }
        }
    }

    private void CreateSignalRecursive(string parentPath, CreateSignalInput input, DataStore dataStore)
    {
        var ptnPath = $"{parentPath}/{input.PublicTechnicalName}";

        var signal = new SignalDto
        {
            PublicTechnicalName = input.PublicTechnicalName,
            DisplayText = input.DisplayText,
            TypeName = input.TypeName,
            PtnPath = ptnPath,
            CDCType = input.CDCType,
            IsDeletable = input.IsDeletable,
            Type = input.Type
        };

        dataStore.Signals.Add(signal);

        // Process nested Subsignals
        if (input.Subsignals != null)
        {
            foreach (var ssInput in input.Subsignals)
            {
                CreateSubsignalRecursive(ptnPath, ssInput, dataStore);
            }
        }

        // Process nested CdcConversions
        if (input.CdcConversions != null)
        {
            foreach (var cdcInput in input.CdcConversions)
            {
                CreateCdcConversionRecursive(ptnPath, cdcInput, dataStore);
            }
        }

        // Process nested Routings
        if (input.Routings != null)
        {
            int routingIndex = 1;
            foreach (var routingInput in input.Routings)
            {
                CreateRouting($"{ptnPath}/routing{routingIndex}", routingInput, dataStore);
                routingIndex++;
            }
        }
    }

    private void CreateSubsignalRecursive(string parentPath, CreateSubsignalInput input, DataStore dataStore)
    {
        var ptnPath = $"{parentPath}/{input.PublicTechnicalName}";

        var subsignal = new SubsignalDto
        {
            PublicTechnicalName = input.PublicTechnicalName,
            DisplayText = input.DisplayText,
            TypeName = input.TypeName,
            PtnPath = ptnPath,
            CDCType = input.CDCType,
            IsDeletable = input.IsDeletable
        };

        dataStore.Subsignals.Add(subsignal);

        // Process nested Routings
        if (input.Routings != null)
        {
            int routingIndex = 1;
            foreach (var routingInput in input.Routings)
            {
                CreateRouting($"{ptnPath}/routing{routingIndex}", routingInput, dataStore);
                routingIndex++;
            }
        }
    }

    private void CreateCdcConversionRecursive(string parentPath, CreateCdcConversionInput input, DataStore dataStore)
    {
        var ptnPath = $"{parentPath}/{input.PublicTechnicalName}";

        var cdcConversion = new CdcConversionDto
        {
            PublicTechnicalName = input.PublicTechnicalName,
            DisplayText = input.DisplayText,
            PtnPath = ptnPath,
            SourceCdc = input.SourceCdc,
            TargetCdc = input.TargetCdc
        };

        dataStore.CdcConversions.Add(cdcConversion);

        // Process nested Routings
        if (input.Routings != null)
        {
            int routingIndex = 1;
            foreach (var routingInput in input.Routings)
            {
                CreateRouting($"{ptnPath}/routing{routingIndex}", routingInput, dataStore);
                routingIndex++;
            }
        }
    }

    private void CreateRouting(string ptnPath, CreateRoutingInput input, DataStore dataStore)
    {
        var routing = new RoutingDto
        {
            PtnPath = ptnPath,
            IsReadonly = input.IsReadonly,
            Value = input.Value,
            Options = input.Options
        };

        dataStore.Routings.Add(routing);
    }

    /// <summary>
    /// Create a new device application
    /// </summary>
    public DeviceApplicationDto CreateDeviceApplication(
        DeviceApplicationDto deviceApplicationDto,
        [Service] DataStore dataStore)
    {
        if (deviceApplicationDto == null)
            throw new ArgumentNullException(nameof(deviceApplicationDto), "Device application data cannot be null");

        if (string.IsNullOrWhiteSpace(deviceApplicationDto.PublicTechnicalName))
            throw new ArgumentException("PublicTechnicalName is required", nameof(deviceApplicationDto));

        if (dataStore.DeviceApplications.Any(d => d.PublicTechnicalName == deviceApplicationDto.PublicTechnicalName))
            throw new ArgumentException($"Device application with PublicTechnicalName '{deviceApplicationDto.PublicTechnicalName}' already exists");

        deviceApplicationDto.LastUpdatedAt = DateTime.UtcNow;

        // Store device without nested collections (they should be added separately)
        var device = new DeviceApplicationDto
        {
            PublicTechnicalName = deviceApplicationDto.PublicTechnicalName,
            DisplayText = deviceApplicationDto.DisplayText,
            TypeName = deviceApplicationDto.TypeName,
            LastUpdatedAt = deviceApplicationDto.LastUpdatedAt,
            LastModifiedBy = deviceApplicationDto.LastModifiedBy,
            DddVersion = deviceApplicationDto.DddVersion,
            ComDddVersion = deviceApplicationDto.ComDddVersion
        };

        dataStore.DeviceApplications.Add(device);

        // GraphQL field resolvers will load relationships only if requested
        return device;
    }

    /// <summary>
    /// Update an existing device application
    /// </summary>
    public DeviceApplicationDto? UpdateDeviceApplication(
        string publicTechnicalName,
        DeviceApplicationDto deviceApplicationDto,
        [Service] DataStore dataStore)
    {
        if (deviceApplicationDto == null)
            throw new ArgumentNullException(nameof(deviceApplicationDto), "Device application data cannot be null");

        var existingDevice = dataStore.DeviceApplications.FirstOrDefault(d => d.PublicTechnicalName == publicTechnicalName);
        if (existingDevice == null)
            throw new ArgumentException($"Device application with PublicTechnicalName '{publicTechnicalName}' not found");

        // Update properties
        existingDevice.DisplayText = deviceApplicationDto.DisplayText;
        existingDevice.TypeName = deviceApplicationDto.TypeName;
        existingDevice.LastUpdatedAt = DateTime.UtcNow;
        existingDevice.LastModifiedBy = deviceApplicationDto.LastModifiedBy;
        existingDevice.DddVersion = deviceApplicationDto.DddVersion;
        existingDevice.ComDddVersion = deviceApplicationDto.ComDddVersion;

        // GraphQL field resolvers will load relationships only if requested
        return existingDevice;
    }

    /// <summary>
    /// Delete a device application and all its related data (cascade delete)
    /// </summary>
    public bool DeleteDeviceApplication(string publicTechnicalName, [Service] DataStore dataStore)
    {
        var device = dataStore.DeviceApplications.FirstOrDefault(d => d.PublicTechnicalName == publicTechnicalName);
        if (device == null)
            return false;

        // Remove all related data (cascade delete)
        var devicePrefix = $"{publicTechnicalName}/";

        dataStore.Routings.RemoveAll(r => r.PtnPath.StartsWith(devicePrefix));
        dataStore.CdcConversions.RemoveAll(c => c.PtnPath.StartsWith(devicePrefix));
        dataStore.Subsignals.RemoveAll(ss => ss.PtnPath.StartsWith(devicePrefix));
        dataStore.Signals.RemoveAll(s => s.PtnPath.StartsWith(devicePrefix));
        dataStore.Functions.RemoveAll(f => f.PTNPath.StartsWith(devicePrefix));
        dataStore.FunctionBlocks.RemoveAll(fb => fb.PtnPath.StartsWith(devicePrefix));
        dataStore.FunctionGroups.RemoveAll(fg => fg.PTNPath.StartsWith(devicePrefix));
        dataStore.DeviceApplications.Remove(device);

        return true;
    }

    /// <summary>
    /// Scenario 17: Update a routing value
    /// </summary>
    public RoutingDto? UpdateRouting(
        string routingPtnPath, 
        string newValue, 
        [Service] DataStore dataStore)
    {
        var routing = dataStore.Routings.FirstOrDefault(r => r.PtnPath == routingPtnPath);
        if (routing == null)
            throw new ArgumentException($"Routing with PtnPath '{routingPtnPath}' not found");

        if (routing.IsReadonly)
            throw new InvalidOperationException($"Routing '{routingPtnPath}' is read-only and cannot be updated");

        // Validate value is in options if options are specified
        if (routing.Options != null && routing.Options.Any() && !routing.Options.Contains(newValue))
            throw new ArgumentException($"Value '{newValue}' is not a valid option. Valid options: {string.Join(", ", routing.Options)}");

        routing.Value = newValue;

        return routing;
    }
}
