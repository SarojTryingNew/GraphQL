using HotChocolate.Types;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.Mutation;

[ExtendObjectType(typeof(Mutation))]
public class DeviceApplicationMutations
{
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

        // Load relationships before returning
        device.LoadRelations(dataStore);
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

        // Load relationships before returning
        existingDevice.LoadRelations(dataStore);
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
