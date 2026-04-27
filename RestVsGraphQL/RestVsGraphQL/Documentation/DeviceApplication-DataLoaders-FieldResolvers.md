# Field Resolvers and DataLoaders for DeviceApplications

This document explains the implementation of GraphQL field resolvers and DataLoaders for DeviceApplication entities.

## Overview

Instead of eagerly loading all relationships using `LoadRelations()`, we now use:
- **Field Resolvers** - Load data only when requested in the query
- **DataLoaders** - Batch and cache database queries for efficiency

## Architecture

### Before (Eager Loading)
```csharp
public IEnumerable<DeviceApplicationDto> GetDeviceApplications([Service] DataStore dataStore)
{
    var devices = dataStore.DeviceApplications.ToList();
    devices.LoadRelations(dataStore);  // ❌ Loads ALL relationships
    return devices;
}
```

**Problems:**
- Loads data not requested in the query
- Multiple queries per request
- No batching or caching

### After (Field Resolvers + DataLoaders)
```csharp
public IEnumerable<DeviceApplicationDto> GetDeviceApplications([Service] DataStore dataStore)
{
    return dataStore.DeviceApplications;  // ✅ Just return devices
}

// Field resolver in DeviceApplicationType
public async Task<IEnumerable<FunctionGroupDto>> GetFunctionGroups(
    [Parent] DeviceApplicationDto device,
    FunctionGroupsByDeviceDataLoader dataLoader)
{
    return await dataLoader.LoadAsync(device.PublicTechnicalName);  // ✅ Batched!
}
```

**Benefits:**
- Only loads requested fields
- Batches multiple requests
- Caches results per request

## Components

### 1. DataLoaders (`DeviceApplicationDataLoaders.cs`)

Seven DataLoaders for batching queries:

```csharp
// Example: Batch load function groups for multiple devices
public class FunctionGroupsByDeviceDataLoader : GroupedDataLoader<string, FunctionGroupDto>
{
    protected override Task<ILookup<string, FunctionGroupDto>> LoadGroupedBatchAsync(
        IReadOnlyList<string> deviceNames,  // Multiple device names
        CancellationToken cancellationToken)
    {
        // Single query for all devices!
        var functionGroups = _dataStore.FunctionGroups
            .Where(fg => deviceNames.Any(d => fg.PTNPath.StartsWith($"{d}/")))
            .ToLookup(fg => fg.PTNPath.Split('/')[0]);

        return Task.FromResult(functionGroups);
    }
}
```

**Available DataLoaders:**
1. `FunctionGroupsByDeviceDataLoader` - Device → FunctionGroups
2. `FunctionBlocksByDeviceDataLoader` - Device → FunctionBlocks
3. `FunctionsByFunctionGroupDataLoader` - FunctionGroup → Functions
4. `SignalsByFunctionBlockDataLoader` - FunctionBlock → Signals
5. `SubsignalsBySignalDataLoader` - Signal → Subsignals
6. `CdcConversionsBySignalDataLoader` - Signal → CdcConversions
7. `RoutingsByParentPathDataLoader` - Any → Routings

### 2. GraphQL Types (`DeviceApplicationTypes.cs`)

Type definitions with field resolvers:

```csharp
public class DeviceApplicationType : ObjectType<DeviceApplicationDto>
{
    protected override void Configure(IObjectTypeDescriptor<DeviceApplicationDto> descriptor)
    {
        descriptor.Name("DeviceApplication");

        // Simple field
        descriptor.Field(d => d.PublicTechnicalName).Type<NonNullType<StringType>>();

        // Field with resolver (loaded on-demand)
        descriptor
            .Field(d => d.FunctionGroups)
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetFunctionGroups(default!, default!))
            .Type<ListType<FunctionGroupType>>();
    }

    private class DeviceApplicationResolvers
    {
        public async Task<IEnumerable<FunctionGroupDto>> GetFunctionGroups(
            [Parent] DeviceApplicationDto device,
            FunctionGroupsByDeviceDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(device.PublicTechnicalName);
        }
    }
}
```

**Available Types:**
1. `DeviceApplicationType`
2. `FunctionGroupType`
3. `FunctionType`
4. `FunctionBlockType`
5. `SignalType`
6. `SubsignalType`
7. `CdcConversionType`
8. `RoutingType`

### 3. Updated Queries (`DeviceApplicationQueries.cs`)

Queries now return plain entities:

```csharp
// No more manual relationship loading!
public IEnumerable<DeviceApplicationDto> GetDeviceApplications([Service] DataStore dataStore)
{
    return dataStore.DeviceApplications;  // GraphQL handles the rest
}
```

## How It Works

### Example Query
```graphql
query {
  deviceApplications {
    publicTechnicalName
    functionGroups {
      displayText
      functions {
        displayText
      }
    }
  }
}
```

### Execution Flow

1. **Query root**: `GetDeviceApplications()` returns 3 devices
   ```
   [IED_001, IED_002, IED_003]
   ```

2. **Field resolver triggered**: GraphQL sees `functionGroups` field requested
   - Calls `DeviceApplicationResolvers.GetFunctionGroups()` for each device
   - DataLoader collects requests: `["IED_001", "IED_002", "IED_003"]`

3. **Batch execution**: DataLoader fires **ONE** query
   ```csharp
   // Instead of 3 queries, just 1!
   _dataStore.FunctionGroups
       .Where(fg => ["IED_001", "IED_002", "IED_003"]
           .Any(d => fg.PTNPath.StartsWith($"{d}/")))
   ```

4. **Result cached**: Results returned to each device

5. **Nested resolvers**: If `functions` requested, same batching happens

## Performance Comparison

### Scenario: Get 3 devices with function groups

**Before (Eager Loading):**
```
Query 1: Get all devices
Query 2: Get function groups for IED_001
Query 3: Get function groups for IED_002
Query 4: Get function groups for IED_003
Query 5: Get functions for each group...
Total: 10+ queries
```

**After (DataLoaders):**
```
Query 1: Get all devices
Query 2: Batch get function groups for [IED_001, IED_002, IED_003]
Query 3: Batch get functions for all groups
Total: 3 queries
```

**Improvement: 70% fewer queries! 🚀**

## Query Examples

### 1. Minimal Query (Only loads what's requested)
```graphql
query {
  deviceApplications {
    publicTechnicalName
    displayText
  }
}
```
**Queries executed:** 1 (just devices, no relationships)

### 2. With Function Groups
```graphql
query {
  deviceApplications {
    publicTechnicalName
    functionGroups {
      displayText
    }
  }
}
```
**Queries executed:** 2 (devices + batched function groups)

### 3. Deep Nesting
```graphql
query {
  deviceApplications {
    publicTechnicalName
    functionBlocks {
      displayText
      signals {
        type
        cdcConversions {
          sourceCdc
        }
        routings {
          value
        }
      }
    }
  }
}
```
**Queries executed:** 5 (devices, function blocks, signals, cdc conversions, routings) - all batched!

## Benefits

### 1. **Automatic Optimization**
- GraphQL only requests needed fields
- No over-fetching
- No under-fetching

### 2. **Batching**
- Multiple parent entities → Single query
- Prevents N+1 query problem
- Scales with data size

### 3. **Caching**
- Results cached per request
- Multiple references to same entity → Single load
- Reduces database pressure

### 4. **Maintainability**
- Clear separation of concerns
- Type-safe resolvers
- Easy to add new fields

### 5. **Performance**
- Fewer database queries
- Smaller payloads
- Faster response times

## Configuration (Program.cs)

All components registered:

```csharp
builder.Services
    .AddGraphQLServer()
    // ... base types
    .AddTypeExtension<DeviceApplicationQueries>()
    .AddTypeExtension<DeviceApplicationMutations>()
    // Types with resolvers
    .AddType<DeviceApplicationType>()
    .AddType<FunctionGroupType>()
    .AddType<FunctionType>()
    .AddType<FunctionBlockType>()
    .AddType<SignalType>()
    .AddType<SubsignalType>()
    .AddType<CdcConversionType>()
    .AddType<RoutingType>()
    // DataLoaders
    .AddDataLoader<FunctionGroupsByDeviceDataLoader>()
    .AddDataLoader<FunctionBlocksByDeviceDataLoader>()
    .AddDataLoader<FunctionsByFunctionGroupDataLoader>()
    .AddDataLoader<SignalsByFunctionBlockDataLoader>()
    .AddDataLoader<SubsignalsBySignalDataLoader>()
    .AddDataLoader<CdcConversionsBySignalDataLoader>()
    .AddDataLoader<RoutingsByParentPathDataLoader>();
```

## Best Practices

1. **Always use DataLoaders for relationships**
   - Don't manually load in queries
   - Let field resolvers handle it

2. **Keep DataLoaders focused**
   - One DataLoader per relationship type
   - Use `GroupedDataLoader` for one-to-many

3. **Type safety**
   - Define explicit GraphQL types
   - Use strong typing in resolvers

4. **Batch efficiently**
   - Use path-based queries with `StartsWith`
   - Return `ILookup` for grouping

5. **Document resolvers**
   - XML comments on field resolvers
   - Explain batching strategy

## Testing DataLoaders

```csharp
// DataLoader batches requests
var device1 = GetFunctionGroups("IED_001", dataLoader);
var device2 = GetFunctionGroups("IED_002", dataLoader);
var device3 = GetFunctionGroups("IED_003", dataLoader);

// Only fires ONE database query for all three!
await Task.WhenAll(device1, device2, device3);
```

## Comparison with LoadRelations()

| Aspect | LoadRelations() | DataLoaders |
|--------|----------------|-------------|
| **Loading** | Eager (all relationships) | Lazy (only requested) |
| **Batching** | No | Yes |
| **Caching** | No | Yes (per request) |
| **Queries** | Many (N+1) | Few (batched) |
| **Flexibility** | Fixed structure | Query-driven |
| **GraphQL Pattern** | Anti-pattern | Best practice |

## Migration Path

The `DeviceApplicationExtensions.cs` file with `LoadRelations()` is kept for:
- Backward compatibility
- REST API endpoints (if needed)
- Internal service calls

But GraphQL queries should **not** use it anymore. The field resolvers handle everything!

## Summary

✅ **Proper GraphQL architecture**  
✅ **Massive performance improvement**  
✅ **Flexible, query-driven loading**  
✅ **Production-ready pattern**  
✅ **Scalable solution**  

This is the **recommended way** to implement GraphQL relationships! 🎉
