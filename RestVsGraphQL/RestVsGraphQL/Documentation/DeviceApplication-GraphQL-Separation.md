# DeviceApplication GraphQL Separation

This document describes the separation of DeviceApplication-related GraphQL queries and mutations into dedicated files.

## File Structure

```
RestVsGraphQL/
└── GraphQL/
    ├── Query.cs                          # Base queries (Customers, Orders, Products)
    ├── Mutation.cs                       # Base mutations (Orders)
    ├── DeviceApplicationQueries.cs       # NEW: DeviceApplication queries
    └── DeviceApplicationMutations.cs     # NEW: DeviceApplication mutations
```

## DeviceApplicationQueries.cs

Contains all query operations for DeviceApplications:

### Queries Available:

1. **GetDeviceApplications** - Get all devices with relationships
2. **GetDeviceApplication** - Get single device by PublicTechnicalName
3. **GetFunctionGroups** - Get all function groups for a device
4. **GetFunctionGroup** - Get specific function group by PTN path
5. **GetFunctionBlocks** - Get all function blocks for a device
6. **GetFunctionBlock** - Get specific function block by PTN path
7. **GetSignals** - Get all signals for a function block
8. **GetAllPtnPaths** - Get all PTN paths from a device
9. **GetFunctions** - Get all functions across all devices
10. **GetFunction** - Get specific function by PTN path
11. **GetSubsignals** - Get all subsignals for a signal
12. **GetCdcConversions** - Get all CDC conversions for a signal
13. **GetRoutings** - Get all routings for a signal/subsignal

## DeviceApplicationMutations.cs

Contains all mutation operations for DeviceApplications:

### Mutations Available:

1. **CreateDeviceApplication** - Create new device
2. **UpdateDeviceApplication** - Update existing device
3. **DeleteDeviceApplication** - Delete device (cascade delete)
4. **AddFunctionGroup** - Add function group to device
5. **AddFunction** - Add function to function group
6. **AddFunctionBlock** - Add function block to device
7. **AddSignal** - Add signal to function block
8. **AddSubsignal** - Add subsignal to signal
9. **AddCdcConversion** - Add CDC conversion to signal
10. **AddRouting** - Add routing to signal/subsignal/CDC conversion
11. **UpdateRouting** - Update routing value

## HotChocolate Type Extension Pattern

Both files use the `[ExtendObjectType]` attribute to extend the base Query and Mutation types:

```csharp
[ExtendObjectType(typeof(Query))]
public class DeviceApplicationQueries
{
    // Queries...
}

[ExtendObjectType(typeof(Mutation))]
public class DeviceApplicationMutations
{
    // Mutations...
}
```

## Registration in Program.cs

The extensions are registered in the GraphQL configuration:

```csharp
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    // Add DeviceApplication extensions
    .AddTypeExtension<DeviceApplicationQueries>()
    .AddTypeExtension<DeviceApplicationMutations>()
    // ... other configuration
```

## Example GraphQL Queries

### Query Example:
```graphql
query {
  deviceApplications {
    publicTechnicalName
    displayText
    functionBlocks {
      publicTechnicalName
      ptnPath
      signals {
        publicTechnicalName
        ptnPath
        type
      }
    }
  }
}
```

### Mutation Example:
```graphql
mutation {
  addSignal(
    functionBlockPtnPath: "IED_001/MMXU1"
    signalDto: {
      publicTechnicalName: "NewSignal"
      displayText: "New Signal"
      ptnPath: "IED_001/MMXU1/NewSignal"
      type: "Analog"
      typeName: "MeasuredValue"
      cdcType: "MV"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    ptnPath
    type
  }
}
```

## Benefits of This Organization

1. **Separation of Concerns** - DeviceApplication logic is isolated
2. **Maintainability** - Easier to find and update DeviceApplication code
3. **Scalability** - Can add more domain-specific query/mutation files
4. **Testability** - Can test DeviceApplication operations independently
5. **Code Organization** - Clear structure for team collaboration

## Relationship with DataStore

Both files use the `DataStore` service and `DeviceApplicationExtensions` for:
- Loading relationships dynamically
- Path-based queries (foreign key simulation)
- Cascade deletes
- Validation

## Validation Features

The mutations include comprehensive validation:
- PTN path hierarchy validation
- Parent existence checks
- Read-only routing protection
- Options validation for routing values
- Cascade delete for device removal
