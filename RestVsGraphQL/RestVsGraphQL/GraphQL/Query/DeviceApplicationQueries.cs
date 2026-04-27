using HotChocolate.Data;
using HotChocolate.Types;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL.Query;

/// <summary>
/// Core queries for DeviceApplication domain (Scenarios 1-2)
/// Extended queries are in DeviceApplicationExtendedQueries.cs (Scenarios 3-13)
/// </summary>
[ExtendObjectType(typeof(Query))]
public class DeviceApplicationQueries
{
    /// <summary>
    /// Scenario 1: Get all device applications with filtering support
    /// Scenario 5: Type-Based Device Grouping enabled via [UseFiltering]
    /// Example: deviceApplications(where: { typeName: { eq: "ProtectionRelay" } })
    /// </summary>
    [UseFiltering]
    public IQueryable<DeviceApplicationDto> GetDeviceApplications([Service] DataStore dataStore)
    {
        return dataStore.DeviceApplications.AsQueryable();
    }

    /// <summary>
    /// Scenario 2: Get Single Device by Public Technical Name
    /// </summary>
    public DeviceApplicationDto? GetDeviceApplication([Service] DataStore dataStore, string publicTechnicalName)
    {
        var device = dataStore.DeviceApplications.FirstOrDefault(d => d.PublicTechnicalName == publicTechnicalName);
        return device;
    }
}
