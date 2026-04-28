# ECO - GraphQL Migration Rough Upfront Design (RUD)

## Purpose

Evaluate GraphQL as an alternative API technology for ECO to address performance challenges and improve developer experience, particularly for bulk operations and complex data retrieval patterns.

### Current Pain Points with REST

- **Over-fetching/Under-fetching**: REST endpoints return fixed data structures, often requiring multiple round-trips to fetch related data (e.g., applications with their function groups, functions, and building blocks)
- **Bulk Operations Performance**: Current REST implementation requires multiple sequential API calls for bulk create, update, and delete operations
- **API Versioning Complexity**: REST versioning (`/api/v1/protection`) creates maintenance overhead
- **N+1 Query Problem**: Fetching collections with nested relationships results in multiple database queries
- **Client Complexity**: Frontend needs to orchestrate multiple REST calls and handle data aggregation

### Expected Benefits of GraphQL

- **Single Request for Complex Data**: Retrieve entire device hierarchy (application → function groups → functions → building blocks) in one query
- **Reduced Network Overhead**: Clients request only needed fields, reducing payload size
- **Batch Operations**: Native support for mutations that operate on multiple entities
- **Strong Typing**: Schema-first approach with compile-time validation
- **Better Developer Experience**: Self-documenting API with introspection, GraphQL Playground
- **Flexible Queries**: Frontend can adapt queries without backend changes

## Scope

### In-Scope

1. **Protection Domain REST APIs**: Focus on `ApplicationController` endpoints as proof-of-concept
2. **Critical Use Cases**:
   - Bulk CRUD operations (create, read, update, delete multiple devices/function groups)
   - Complex hierarchical queries (station → applications → function groups → functions)
   - Nested mutations (create application with function groups in single request)
3. **Migration Strategy**: Coexistence model where REST and GraphQL run side-by-side
4. **Documentation**: How-to guides, schema design, migration approach

## GraphQL Fundamentals - How-to GraphQL

### What is GraphQL?

GraphQL is a **query language for APIs** and a **runtime for executing those queries** with your existing data. Developed by Facebook in 2012 and open-sourced in 2015, it provides a complete and understandable description of the data in your API, gives clients the power to ask for exactly what they need, and makes APIs easier to evolve over time.

**Core Philosophy:**
- **Client-Driven**: Clients specify exactly what data they need
- **Strongly Typed**: Every field and argument is validated against a schema
- **Hierarchical**: Queries mirror the shape of the data they return
- **Introspective**: Clients can query the schema for documentation

### Key Concepts

#### 1. Schema Definition Language (SDL)

The schema is the contract between client and server. It defines:
- **Types**: What data is available
- **Queries**: How to read data
- **Mutations**: How to modify data
- **Subscriptions**: How to receive real-time updates

**Example Schema:**
```graphql
type DeviceApplication {
  id: ID!                           # Non-nullable ID
  publicTechnicalName: String!      # Required string
  deviceName: String                # Optional string
  orderNumber: String!
  functionGroups: [FunctionGroup!]! # Non-nullable array of non-nullable items
  createdAt: DateTime!              # Custom scalar type
}

type Query {
  # Get single device
  application(stationName: String!, devicePtn: String): DeviceApplication

  # Get multiple devices (with optional filter)
  applications(stationName: String!, filter: ApplicationFilter): [DeviceApplication!]!
}

type Mutation {
  # Create devices (returns response with data and errors)
  createApplications(input: [CreateApplicationInput!]!): ApplicationResponse!

  # Delete devices
  deleteApplications(stationName: String!, devicePtns: [String!]!): DeleteResponse!
}
```

**Type Modifiers:**
- `String`: Nullable string (can be `null`)
- `String!`: Non-nullable string (required)
- `[String]`: Nullable array of nullable strings
- `[String]!`: Non-nullable array of nullable strings
- `[String!]!`: Non-nullable array of non-nullable strings

#### 2. Queries - Reading Data

Queries are how clients fetch data. They're hierarchical and declarative.

**Basic Query - GetDeviceInformation Example:**

Current REST endpoint:
```csharp
// REST: GET /api/v1/protection/hardware/{stationName}/{devicePtn}
[HttpGet("hardware/{stationName}/{devicePtn}")]
public async Task<ActionResult> GetDeviceInformation(string stationName, string devicePtn)
{
    _validationService.ValidateStationNameAndDevicePtn(stationName, devicePtn);
    APIResponse<DeviceInformationDto> deviceInformation = 
        await _deviceApplicationService.GetDeviceInformation(stationName, devicePtn);
    return Ok(deviceInformation);
}
```

GraphQL equivalent:
```graphql
query GetDeviceInformation {
  deviceInformation(stationName: "Station1", devicePtn: "Device1") {
    ptn                           
    displayText                   
    tags                          
    description                   
    mlfb                          
    configurationVersion          
    communicationConfigVersion    
    cpuType                       
    type                          
    lastUpdatedAt                 
    lastModifiedBy                
  }
}
```

**Comparison - REST vs GraphQL:**

REST (Fixed response):
```http
GET /api/v1/protection/hardware/Station1/Device1
Authorization: Bearer {token}

# Response: ALL fields returned (even if not needed)
{
  "data": {
    "ptn": "Device1",
    "displayText": "SIPROTEC 5 Protection Device",
    "tags": ["PRIMARY", "OVERCURRENT"],
    "description": "Primary protection device for Station1",
    "mlfb": "7SX8001-1AA00",
    "configurationVersion": "V8.00",
    "communicationConfigVersion": "V2.1",
    "cpuType": "ARM Cortex-A9",
    "type": "SIPROTEC5",
    "lastUpdatedAt": "2025-01-15T10:30:00Z",
    "lastModifiedBy": "admin@siemens.com"
  }
}
```

GraphQL (Client-specified fields):
```graphql
# Client requests ONLY needed fields
query GetDeviceInfo {
  deviceInformation(stationName: "Station1", devicePtn: "Device1") {
    displayText
    configurationVersion
    type
  }
}

# Response: ONLY requested fields returned
{
  "data": {
    "deviceInformation": {
      "displayText": "SIPROTEC 5 Protection Device",
      "configurationVersion": "V8.00",
      "type": "SIPROTEC5"
    }
  }
}
```

**Query with Variables:**
```graphql
query GetDeviceInfo($station: String!, $device: String!) {
  deviceInformation(stationName: $station, devicePtn: $device) {
    displayText
    mlfb
    configurationVersion
    communicationConfigVersion
    cpuType
    type
  }
}

# Variables (sent separately - type-safe):
{
  "station": "Station1",
  "device": "Device1"
}
```

**Field Selection Benefits:**
```graphql
# Scenario 1: Dashboard (only needs basic info)
query Dashboard {
  deviceInformation(stationName: "Station1", devicePtn: "Device1") {
    displayText           # Only 3 fields
    configurationVersion
    type
  }
}

# Scenario 2: Details page (needs all info)
query DeviceDetails {
  deviceInformation(stationName: "Station1", devicePtn: "Device1") {
    ptn
    displayText
    tags
    description
    mlfb
    configurationVersion
    communicationConfigVersion
    cpuType
    type
    lastUpdatedAt
    lastModifiedBy
  }
}
```

#### 3. Mutations - Modifying Data

Mutations are how clients create, update, or delete data. By convention, mutations that modify data should return the modified data.

**Current REST Endpoint:**
```csharp
// REST: POST /api/v1/protection/applications
[HttpPost("applications")]
public async Task<IActionResult> CreateApplicationsAsync(
    [FromBody] IReadOnlyCollection<CreateDeviceDto> createApplicationDtos,
    [FromHeader(Name = "X-Restore-Device")] string isRestoreOperation = "false")
{
    _validationService.ValidateCreateApplications(createApplicationDtos);

    APIResponse<IReadOnlyCollection<DeviceApplicationDto>> applicationDtos = 
        await _deviceApplicationService.CreateApplicationsAsync(createApplicationDtos);

    return StatusCode(201, applicationDtos);
}
```

**Request DTO (CreateDeviceDto):**
```csharp
public record CreateDeviceDto
{
    public string PublicTechnicalName { get; set; } = string.Empty;
    public required string TemplateName { get; set; }
    public required string DddFileName { get; set; }
    public required string DddVersion { get; set; }
    public required string? ComDddFileName { get; set; }
    public required string? ComDddVersion { get; set; }
    public required string StationName { get; set; }
    public string? DeviceFolderPath { get; set; }
}
```

**Response DTO (DeviceApplicationDto):**
```csharp
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
```

**Simple Mutation (GraphQL):**
```graphql
mutation CreateApplications {
  createApplications(input: [
    {
      publicTechnicalName: "Device1"
      templateName: "SIPROTEC5_Template"
      dddFileName: "7SX8001.ddd"
      dddVersion: "V8.00"
      comDddFileName: "7SX8001_Com.ddd"
      comDddVersion: "V2.1"
      stationName: "Station1"
      deviceFolderPath: "/Devices"
    }
  ]) {
    data {
      publicTechnicalName
      displayText
      typeName
      dddVersion
      comDddVersion
      lastUpdatedAt
    }
    errors {
      message
      code
      severity
    }
  }
}
```

**REST Equivalent:**
```http
POST /api/v1/protection/applications
Content-Type: application/json
Authorization: Bearer {token}

[
  {
    "publicTechnicalName": "Device1",
    "templateName": "SIPROTEC5_Template",
    "dddFileName": "7SX8001.ddd",
    "dddVersion": "V8.00",
    "comDddFileName": "7SX8001_Com.ddd",
    "comDddVersion": "V2.1",
    "stationName": "Station1",
    "deviceFolderPath": "/Devices"
  }
]

# Response: 201 Created
{
  "data": [
    {
      "publicTechnicalName": "Device1",
      "displayText": "SIPROTEC 5 Device 1",
      "typeName": "SIPROTEC5",
      "functionGroups": null,
      "functionBlocks": null,
      "lastUpdatedAt": "2025-01-15T10:30:00Z",
      "lastModifiedBy": "admin@siemens.com",
      "dddVersion": "V8.00",
      "comDddVersion": "V2.1"
    }
  ],
  "errors": null
}
```

**Mutation with Variables:**
```graphql
mutation CreateApplications($input: [CreateApplicationInput!]!) {
  createApplications(input: $input) {
    data {
      publicTechnicalName
      displayText
      typeName
      functionGroups {
        name
        publicTechnicalName
      }
    }
    errors {
      message
      path
      code
    }
  }
}

# Variables (sent separately):
{
  "input": [
    {
      "publicTechnicalName": "Device1",
      "templateName": "SIPROTEC5_Template",
      "dddFileName": "7SX8001.ddd",
      "dddVersion": "V8.00",
      "comDddFileName": "7SX8001_Com.ddd",
      "comDddVersion": "V2.1",
      "stationName": "Station1",
      "deviceFolderPath": "/Devices"
    }
  ]
}

# Response:
{
  "data": {
    "createApplications": {
      "data": [
        {
          "publicTechnicalName": "Device1",
          "displayText": "SIPROTEC 5 Device 1",
          "typeName": "SIPROTEC5",
          "functionGroups": [
            {
              "name": "FG_Protection",
              "publicTechnicalName": "Station1/Device1/FG_Protection"
            }
          ]
        }
      ],
      "errors": null
    }
  }
}
```

**Bulk Create (Multiple Devices):**
```graphql
mutation BulkCreateApplications {
  createApplications(input: [
    {
      publicTechnicalName: "Device1"
      templateName: "SIPROTEC5_Template"
      dddFileName: "7SX8001.ddd"
      dddVersion: "V8.00"
      comDddFileName: null
      comDddVersion: null
      stationName: "Station1"
    },
    {
      publicTechnicalName: "Device2"
      templateName: "SIPROTEC5_Compact_Template"
      dddFileName: "7SX8002.ddd"
      dddVersion: "V8.00"
      comDddFileName: null
      comDddVersion: null
      stationName: "Station1"
    },
    {
      publicTechnicalName: "Device3"
      templateName: "SICAM8_Template"
      dddFileName: "8MU8000.ddd"
      dddVersion: "V1.5"
      comDddFileName: null
      comDddVersion: null
      stationName: "Station1"
    }
  ]) {
    data {
      publicTechnicalName
      displayText
      typeName
    }
    errors {
      message
      path
    }
  }
}
```

#### 4. Resolvers - The Implementation

Resolvers are functions that fetch/compute data for each field in the schema. They form the connection between GraphQL and your business logic.

**What are Resolvers?**

In GraphQL, every field in your schema can have a resolver - a function that knows how to fetch the data for that specific field. Resolvers are the backbone of GraphQL's "execute as you need" philosophy.

**Key Benefits of Resolvers:**

1. **On-Demand Execution (Lazy Loading)**
   - Resolvers only execute when a field is requested in the query
   - If a client doesn't request a field, its resolver never runs
   - Prevents unnecessary database queries and computations

2. **Field-Level Granularity**
   - Each field can have its own resolver with custom logic
   - Different data sources can be combined in a single type
   - Complex transformations can be isolated per field

3. **Separation of Concerns**
   - Business logic separated from data access
   - Resolvers act as adapters between GraphQL schema and backend services
   - Easy to mock and test individual resolvers

4. **Declarative Data Fetching**
   - Client declares what it needs, resolvers figure out how to get it
   - No need to create custom endpoints for different data shapes
   - Single schema supports infinite query combinations

**Resolver Signature (C# with HotChocolate):**
```csharp
// Simple resolver
public async Task<DeviceApplication?> GetApplication(
    [Service] IDeviceApplicationService service,  // Dependency injection
    string stationName,                           // Arguments
    string devicePtn)          
{
    return await service.GetApplicationAsync(stationName, devicePtn);
}

// Field resolver (nested data)
public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
    [Parent] DeviceApplication device,            // Parent object
    [Service] IFunctionGroupService service)      // Service dependency
{
    // Only executes if client requests functionGroups field!
    return await service.GetFunctionGroupsAsync(device.PublicTechnicalName);
}
```

**Execute as You Need - Practical Example**

This demonstrates how GraphQL resolvers execute only what's requested:

```csharp
// GraphQL Schema Definition
public class DeviceApplicationType : ObjectType<DeviceApplication>
{
    protected override void Configure(IObjectTypeDescriptor<DeviceApplication> descriptor)
    {
        // Basic fields (always resolved)
        descriptor.Field(d => d.PublicTechnicalName);
        descriptor.Field(d => d.DeviceName);

        // Complex field with custom resolver (only when requested)
        descriptor
            .Field("functionGroups")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetFunctionGroups(default!, default!));

        descriptor
            .Field("deviceInformation")
            .ResolveWith<DeviceApplicationResolvers>(r => r.GetDeviceInformation(default!, default!));
    }
}

// Resolvers class
public class DeviceApplicationResolvers
{
    // This resolver ONLY executes if client requests 'functionGroups' field
    public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
        [Parent] DeviceApplication device,
        [Service] IFunctionGroupService service)
    {
        Console.WriteLine($"Fetching function groups for {device.PublicTechnicalName}");
        return await service.GetFunctionGroupsAsync(device.PublicTechnicalName);
    }

    // This resolver ONLY executes if client requests 'deviceInformation' field
    public async Task<DeviceInformation> GetDeviceInformation(
        [Parent] DeviceApplication device,
        [Service] IDeviceInformationService service)
    {
        Console.WriteLine($"Fetching device info for {device.PublicTechnicalName}");
        return await service.GetDeviceInformationAsync(device.PublicTechnicalName);
    }
}
```

**Client Query Examples:**

```graphql
# Query 1: Only basic fields - No resolver methods called
query MinimalData {
  application(stationName: "Station1", devicePtn: "Device1") {
    publicTechnicalName  # Direct field access
    deviceName           # Direct field access
  }
}

# Execution:
# 1. GetApplication() executes → fetches DeviceApplication from DB
# 2. Returns publicTechnicalName and deviceName
# 3. GetFunctionGroups() NOT called
# 4. GetDeviceInformation() NOT called

# Query 2: With function groups - GetFunctionGroups() resolver executes
query WithFunctionGroups {
  application(stationName: "Station1", devicePtn: "Device1") {
    publicTechnicalName
    functionGroups {      # Triggers GetFunctionGroups() resolver!
      name
      publicTechnicalName
    }
  }
}

# Execution:
# 1. GetApplication() executes
# 2. Returns publicTechnicalName
# 3. GetFunctionGroups() EXECUTES (fetches from DB)
# 4. GetDeviceInformation() NOT called

# Query 3: Everything - All resolvers execute
query FullData {
  application(stationName: "Station1", devicePtn: "Device1") {
    publicTechnicalName
    deviceName
    functionGroups {           # Triggers GetFunctionGroups()
      name
    }
    deviceInformation {        # Triggers GetDeviceInformation()
      firmwareVersion
      ipAddress
    }
  }
}

# Execution:
# 1. GetApplication() executes
# 2. GetFunctionGroups() EXECUTES
# 3. GetDeviceInformation() EXECUTES
```

**Performance Comparison:**

```
REST Approach (Fixed Response):
GET /api/applications/Device1
→ Always fetches:
  - Device data
  - Function groups (JOIN)
  - Device information (JOIN)
  - Total: 3 database queries/JOINs

GraphQL with Resolvers (On-Demand):
Query 1 (minimal): 1 query  (device only)
Query 2 (with FGs): 2 queries (device + function groups)
Query 3 (full): 3 queries (device + FGs + device info)

Benefit: 66% reduction in database load when only basic data needed!
```

#### 4a. DataLoaders - Solving the N+1 Problem

**What is the N+1 Problem?**

The N+1 problem occurs when fetching a collection of entities and then making a separate query for each entity's relationships.

**N+1 Problem Example (Without DataLoader):**

```csharp
// BAD: This creates N+1 queries
public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
    [Parent] DeviceApplication device,
    [Service] ProtectionDbContext db)
{
    // For 10 devices, this executes 10 separate queries!
    return await db.FunctionGroups
        .Where(fg => fg.DevicePtn == device.PublicTechnicalName)
        .ToListAsync();
}
```

**Query:**
```graphql
query {
  applications(stationName: "Station1") {  # 1 query
    publicTechnicalName
    functionGroups {  # N queries (one per device!)
      name
    }
  }
}
```

**Database Execution:**
```sql
-- Query 1: Get devices
SELECT * FROM DeviceApplications WHERE StationName = 'Station1';
-- Returns: Device1, Device2, Device3 (3 devices)

-- Query 2: Get function groups for Device1
SELECT * FROM FunctionGroups WHERE DevicePtn = 'Device1';

-- Query 3: Get function groups for Device2
SELECT * FROM FunctionGroups WHERE DevicePtn = 'Device2';

-- Query 4: Get function groups for Device3
SELECT * FROM FunctionGroups WHERE DevicePtn = 'Device3';

-- Total: 4 queries (1 + 3 = N+1 problem!)
```

**Solution: DataLoader**

DataLoaders batch and cache data requests within a single GraphQL query execution.

**Key Benefits of DataLoaders:**

1. **Automatic Batching**
   - Collects all data requests during a single GraphQL execution
   - Executes them as a single database query
   - Dramatically reduces database round-trips

2. **Request-Level Caching**
   - Caches results within a single GraphQL request
   - Same entity requested multiple times = fetched once
   - Automatic deduplication of requests

3. **Transparent to Resolvers**
   - Resolvers use DataLoaders like normal async calls
   - Batching happens automatically in the background
   - No special handling needed in resolver code

4. **Performance Optimization**
   - Converts N+1 queries into 2 queries (1 + 1 batched)
   - Reduces database load by 70-90% for hierarchical data
   - Faster response times for complex queries

**DataLoader Implementation:**

```csharp
// 1. Define DataLoader
public class FunctionGroupDataLoader : GroupedDataLoader<string, FunctionGroup>
{
    private readonly ProtectionDbContext _db;

    public FunctionGroupDataLoader(
        ProtectionDbContext db,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _db = db;
    }

    // This method receives ALL device PTNs collected during query execution
    protected override async Task<ILookup<string, FunctionGroup>> LoadGroupedBatchAsync(
        IReadOnlyList<string> devicePtns,  // ["Device1", "Device2", "Device3"]
        CancellationToken cancellationToken)
    {
        // Single query fetches function groups for ALL devices!
        var functionGroups = await _db.FunctionGroups
            .Where(fg => devicePtns.Contains(fg.DevicePtn))
            .ToListAsync(cancellationToken);

        // Group by device PTN for return
        return functionGroups.ToLookup(fg => fg.DevicePtn);
    }
}

// 2. Use DataLoader in resolver
public class DeviceApplicationResolvers
{
    // GOOD: Uses DataLoader - Batches requests
    public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
        [Parent] DeviceApplication device,
        FunctionGroupDataLoader dataLoader)  // DataLoader injected
    {
        // This looks like a single request, but DataLoader batches it!
        return await dataLoader.LoadAsync(device.PublicTechnicalName);
    }
}

// 3. Register DataLoader
services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddDataLoader<FunctionGroupDataLoader>();  // Register the DataLoader
```

**How DataLoader Works (Execution Flow):**

```graphql
query {
  applications(stationName: "Station1") {  # Returns 3 devices
    publicTechnicalName
    functionGroups {  # Resolver called 3 times
      name
    }
  }
}
```

**Execution Timeline:**
```
Time    Event
----    -----
T0      Execute root query: GetApplications()
        → Returns: [Device1, Device2, Device3]

T1      Resolve field: Device1.functionGroups
        → Calls: dataLoader.LoadAsync("Device1")
        → DataLoader: Queues request (doesn't execute yet)

T2      Resolve field: Device2.functionGroups
        → Calls: dataLoader.LoadAsync("Device2")
        → DataLoader: Queues request

T3      Resolve field: Device3.functionGroups
        → Calls: dataLoader.LoadAsync("Device3")
        → DataLoader: Queues request

T4      All fields processed → DataLoader triggers batch execution
        → Executes: LoadGroupedBatchAsync(["Device1", "Device2", "Device3"])
        → Single SQL query:
          SELECT * FROM FunctionGroups 
          WHERE DevicePtn IN ('Device1', 'Device2', 'Device3')

T5      DataLoader returns results to each resolver
        → Device1 gets its function groups
        → Device2 gets its function groups
        → Device3 gets its function groups
```

**Database Queries Comparison:**

```
WITHOUT DataLoader:
Query 1: SELECT * FROM DeviceApplications WHERE StationName = 'Station1';
Query 2: SELECT * FROM FunctionGroups WHERE DevicePtn = 'Device1';
Query 3: SELECT * FROM FunctionGroups WHERE DevicePtn = 'Device2';
Query 4: SELECT * FROM FunctionGroups WHERE DevicePtn = 'Device3';
Total: 4 queries

WITH DataLoader:
Query 1: SELECT * FROM DeviceApplications WHERE StationName = 'Station1';
Query 2: SELECT * FROM FunctionGroups WHERE DevicePtn IN ('Device1', 'Device2', 'Device3');
Total: 2 queries (50% reduction!)

For 100 devices:
Without DataLoader: 101 queries
With DataLoader: 2 queries (98% reduction!)
```

**Complete Example - Nested DataLoaders:**

```csharp
// Schema with multiple levels of nesting
public class Query
{
    public async Task<IEnumerable<DeviceApplication>> GetApplications(
        [Service] ProtectionDbContext db,
        string stationName)
    {
        return await db.DeviceApplications
            .Where(a => a.StationName == stationName)
            .ToListAsync();
    }
}

// Device → FunctionGroups resolver
public class DeviceApplicationResolvers
{
    public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
        [Parent] DeviceApplication device,
        FunctionGroupDataLoader dataLoader)
    {
        return await dataLoader.LoadAsync(device.PublicTechnicalName);
    }
}

// FunctionGroup → Functions resolver
public class FunctionGroupResolvers
{
    public async Task<IEnumerable<ProtectionFunction>> GetFunctions(
        [Parent] FunctionGroup group,
        FunctionDataLoader dataLoader)  // Another DataLoader!
    {
        return await dataLoader.LoadAsync(group.PublicTechnicalName);
    }
}

// Client query with 3 levels of nesting
query {
  applications(stationName: "Station1") {     # Level 1
    publicTechnicalName
    functionGroups {                           # Level 2 (batched!)
      name
      functions {                              # Level 3 (batched!)
        name
        type
      }
    }
  }
}

/*
Database Execution:
1. Query applications: 1 query
2. Query all function groups: 1 batched query
3. Query all functions: 1 batched query
Total: 3 queries (instead of 1 + N + N*M queries!)
*/
```

**DataLoader Caching Example:**

```graphql
query {
  device1: application(devicePtn: "Device1") {
    functionGroups { name }  # Fetches function groups
  }
  device2: application(devicePtn: "Device1") {  # Same device!
    functionGroups { name }  # Uses cached result!
  }
}

# DataLoader caches Device1's function groups
# Second request doesn't hit database
```

<!-- **Best Practices:**

1. **Always use DataLoaders for relationships**
   ```csharp
   //   Bad
   public async Task<List<FunctionGroup>> GetGroups([Parent] Device device, [Service] DbContext db)
       => await db.FunctionGroups.Where(fg => fg.DevicePtn == device.Ptn).ToListAsync();

   //   Good
   public async Task<IEnumerable<FunctionGroup>> GetGroups([Parent] Device device, FunctionGroupDataLoader loader)
       => await loader.LoadAsync(device.Ptn);
   ```

2. **Use GroupedDataLoader for one-to-many relationships**
   ```csharp
   public class FunctionGroupDataLoader : GroupedDataLoader<string, FunctionGroup> { }
   ```

3. **Use BatchDataLoader for one-to-one relationships**
   ```csharp
   public class DeviceInfoDataLoader : BatchDataLoader<string, DeviceInformation> { }
   ```

4. **Register all DataLoaders at startup**
   ```csharp
   services
       .AddGraphQLServer()
       .AddDataLoader<FunctionGroupDataLoader>()
       .AddDataLoader<FunctionDataLoader>()
       .AddDataLoader<DeviceInfoDataLoader>();
   ``` -->

**Summary: Resolvers + DataLoaders = Optimal Performance**

| Feature | Benefit | Impact |
|---------|---------|--------|
| **Resolvers** | Execute only requested fields | 50-90% fewer computations |
| **DataLoaders** | Batch database queries | 90-98% fewer DB queries |
| **Caching** | Deduplicate requests | Eliminates redundant fetches |
| **Combined** | Optimal "execute as you need" | 10-100x performance improvement |

#### 4b. Projections - Database Column-Level Optimization

**What is `[UseProjection]`?**

`[UseProjection]` is a HotChocolate attribute (from `HotChocolate.Data` package) that automatically translates GraphQL field selections into optimized LINQ projections. This ensures **only requested columns are fetched from the database**, not entire entity objects.

**Why Projections Matter:**

While resolvers + DataLoaders optimize **which relationships to load**, projections optimize **which fields to fetch** from each entity. This completes the "execute as you need" philosophy at all layers:

| Optimization Layer | Technology | What It Optimizes |
|-------------------|------------|-------------------|
| **Relationship-Level** | Resolvers | Which related entities to load |
| **Query-Level** | DataLoaders | Batch N+1 queries into single query |
| **Column-Level** | Projections | Which columns to SELECT from database |
| **Network-Level** | GraphQL | Which fields to serialize to JSON |

**The Problem Without Projections:**

```csharp
// Query without [UseProjection]
public IEnumerable<DeviceApplication> GetApplications([Service] ProtectionDbContext db)
{
    return db.DeviceApplications; // Fetches ALL columns
}
```

**Database Execution:**
```sql
-- ❌ Fetches all 15 columns even if client only needs 3
SELECT 
    PublicTechnicalName,
    DisplayText,
    DeviceName,
    TypeName,
    OrderNumber,
    StationName,
    ParentPtnPath,
    DeviceFolderPath,
    DddFileName,
    DddVersion,
    ComDddFileName,
    ComDddVersion,
    LastUpdatedAt,
    LastModifiedBy,
    CreatedAt
FROM DeviceApplications
WHERE StationName = 'Station1';
```

**Client Query (Only Needs 3 Fields):**
```graphql
query {
  applications(stationName: "Station1") {
    publicTechnicalName
    displayText
    typeName
  }
}
```

**Result:**
- ❌ Database: Fetched 15 columns
- ❌ Memory: Full entity objects with 15 properties
- ✅ Network: Only 3 fields sent to client (GraphQL optimization)
- **Wasted**: 80% of database I/O and memory

---

**The Solution With Projections:**

```csharp
[UseProjection]  // ✅ Enable column-level optimization
public IQueryable<DeviceApplication> GetApplications([Service] ProtectionDbContext db)
{
    return db.DeviceApplications.AsQueryable();  // Must return IQueryable
}
```

**Database Execution:**
```sql
-- ✅ Only fetches 3 requested columns!
SELECT 
    PublicTechnicalName,
    DisplayText,
    TypeName
FROM DeviceApplications
WHERE StationName = 'Station1';
```

**Result:**
- ✅ Database: Fetched only 3 columns (80% reduction)
- ✅ Memory: Partial objects with only 3 properties populated
- ✅ Network: Only 3 fields sent to client
- **Benefit**: Optimal at every layer!

---

**How `[UseProjection]` Works:**

1. **HotChocolate intercepts the query**: Analyzes which fields the client requested
2. **Builds LINQ projection**: Creates `Select()` expression with only those fields
3. **Entity Framework translates**: Converts LINQ projection to optimized SQL
4. **Database executes**: Fetches only requested columns
5. **Memory efficiency**: Creates partial objects, not full entities

**Execution Flow:**

```
┌─────────────────────────────────────────────────────┐
│ Client Query                                        │
│ query { applications { publicTechnicalName } }      │
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│ HotChocolate Projection Middleware                  │
│ - Analyzes selection set                            │
│ - Builds: .Select(d => new { d.PublicTechnicalName })│
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│ Entity Framework Core                               │
│ - Translates LINQ to SQL                            │
│ - Generates: SELECT PublicTechnicalName FROM ...    │
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│ SQL Server                                          │
│ - Executes optimized query                          │
│ - Returns only requested columns                    │
└─────────────────────────────────────────────────────┘
```

---

**ECO-Specific Example:**

**Scenario**: Frontend dashboard needs device list for dropdown selection.

**REST Approach (Current):**
```csharp
// REST Controller
[HttpGet("applications/{stationName}")]
public async Task<ActionResult> GetApplications(string stationName)
{
    // Fetches EVERYTHING
    var devices = await _dbContext.DeviceApplications
        .Where(d => d.StationName == stationName)
        .ToListAsync();

    return Ok(devices);  // Returns all 15 fields
}
```

**REST Response:**
```json
// ❌ 500 KB payload for 100 devices (all fields)
[
  {
    "publicTechnicalName": "IED_001",
    "displayText": "SIPROTEC 5 Device 1",
    "deviceName": "7SX8001",
    "typeName": "SIPROTEC5",
    "orderNumber": "ORD-12345",
    "stationName": "Station1",
    "parentPtnPath": "Station1",
    "deviceFolderPath": "/Devices/IED_001",
    "dddFileName": "7SX8001.ddd",
    "dddVersion": "V8.00",
    "comDddFileName": "7SX8001_Com.ddd",
    "comDddVersion": "V2.1",
    "lastUpdatedAt": "2025-01-15T10:30:00Z",
    "lastModifiedBy": "admin@siemens.com",
    "createdAt": "2024-06-01T08:00:00Z"
  },
  ... 99 more devices
]
```

**Database Query:**
```sql
-- Fetches 15 columns × 100 rows = 1,500 values
SELECT * FROM DeviceApplications WHERE StationName = 'Station1';
```

---

**GraphQL with Projection:**

```csharp
// GraphQL Query
public class Query
{
    [UseProjection]  // ✅ Enable projection
    public IQueryable<DeviceApplication> GetApplications(
        [Service] ProtectionDbContext db,
        string stationName)
    {
        return db.DeviceApplications
            .Where(d => d.StationName == stationName);
    }
}
```

**Client Query:**
```graphql
query DeviceDropdown {
  applications(stationName: "Station1") {
    publicTechnicalName
    displayText
  }
}
```

**GraphQL Response:**
```json
// ✅ 50 KB payload for 100 devices (only 2 fields)
{
  "data": {
    "applications": [
      {
        "publicTechnicalName": "IED_001",
        "displayText": "SIPROTEC 5 Device 1"
      },
      ... 99 more devices
    ]
  }
}
```

**Generated SQL:**
```sql
-- ✅ Fetches only 2 columns × 100 rows = 200 values
SELECT 
    PublicTechnicalName,
    DisplayText
FROM DeviceApplications
WHERE StationName = 'Station1';
```

**Performance Comparison:**

| Metric | REST (All Fields) | GraphQL (Projection) | Improvement |
|--------|-------------------|----------------------|-------------|
| **Columns Fetched** | 15 | 2 | **86% reduction** |
| **Database I/O** | 1,500 values | 200 values | **86% faster** |
| **Memory Usage** | 500 KB | 50 KB | **90% reduction** |
| **Network Payload** | 500 KB | 50 KB | **90% reduction** |
| **Query Time** | 150ms | 20ms | **7.5x faster** |

<!-- ---

**Real-World ECO Use Cases:**

**1. Device List for Dropdown:**
```graphql
query DeviceDropdown {
  applications(stationName: "Station1") {
    publicTechnicalName  # Only 2 fields
    displayText
  }
}

# SQL: SELECT PublicTechnicalName, DisplayText FROM ...
# Benefit: 86% reduction in data fetched
```

**2. Device Details Page:**
```graphql
query DeviceDetails {
  application(stationName: "Station1", devicePtn: "IED_001") {
    publicTechnicalName
    displayText
    deviceName
    typeName
    orderNumber
    dddVersion
    comDddVersion
    lastUpdatedAt
    lastModifiedBy
  }
}

# SQL: SELECT PublicTechnicalName, DisplayText, DeviceName, ... (9 columns)
# Benefit: Still optimized - only needed columns fetched
```

**3. Dashboard Summary:**
```graphql
query DashboardSummary {
  applications(stationName: "Station1") {
    displayText        # Only 3 fields for dashboard cards
    typeName
    lastUpdatedAt
  }
}

# SQL: SELECT DisplayText, TypeName, LastUpdatedAt FROM ...
# Benefit: 80% reduction for dashboard performance
```
 -->
---

**Setup Requirements:**

**1. Install NuGet Package:**
```powershell
dotnet add package HotChocolate.Data --version 14.2.0
```

**2. Register Projections in `Program.cs`:**
```csharp
services
    .AddGraphQLServer()
    .AddProjections()  // ✅ Enable projection support
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();
```

**3. Use `[UseProjection]` Attribute:**
```csharp
public class Query
{
    [UseProjection]  // ✅ Add to queries returning IQueryable
    public IQueryable<DeviceApplication> GetApplications(
        [Service] ProtectionDbContext db,
        string stationName)
    {
        return db.DeviceApplications.Where(d => d.StationName == stationName);
    }

    [UseProjection]  // ✅ Works with single entity queries too
    public IQueryable<DeviceApplication> GetApplication(
        [Service] ProtectionDbContext db,
        string stationName,
        string devicePtn)
    {
        return db.DeviceApplications
            .Where(d => d.StationName == stationName && d.PublicTechnicalName == devicePtn);
    }
}
```

**Important:** 
- Must return `IQueryable<T>`, not `IEnumerable<T>` or `List<T>`
- Only works with Entity Framework Core `DbContext` queries
- Does NOT work with in-memory lists (`List<T>.AsQueryable()` won't optimize)

---

**Combining Projections with Other Optimizations:**

```csharp
[UseProjection]  // ✅ Column-level optimization
[UseFiltering]   // ✅ Allows client-side WHERE clauses
[UseSorting]     // ✅ Allows client-side ORDER BY
public IQueryable<DeviceApplication> GetApplications(
    [Service] ProtectionDbContext db,
    string stationName)
{
    return db.DeviceApplications.Where(d => d.StationName == stationName);
}
```

**Client Query with All Features:**
```graphql
query FilteredDevices {
  applications(
    stationName: "Station1",
    where: { typeName: { eq: "SIPROTEC5" } },  # Filtering
    order: { lastUpdatedAt: DESC }             # Sorting
  ) {
    publicTechnicalName  # Projection (only 3 fields)
    displayText
    lastUpdatedAt
  }
}
```

**Generated SQL:**
```sql
-- ✅ Single optimized query with all features
SELECT 
    PublicTechnicalName,
    DisplayText,
    LastUpdatedAt
FROM DeviceApplications
WHERE StationName = 'Station1' 
  AND TypeName = 'SIPROTEC5'
ORDER BY LastUpdatedAt DESC;
```

---

**Performance Impact:**

**Scenario**: 1,000 devices in Station1, dashboard needs 3 fields

| Technology | Columns | Rows | Total Values | Query Time | Memory |
|------------|---------|------|--------------|------------|--------|
| **REST** | 15 | 1,000 | 15,000 | 300ms | 2 MB |
| **GraphQL (No Projection)** | 15 | 1,000 | 15,000 | 300ms | 2 MB |
| **GraphQL (With Projection)** | 3 | 1,000 | 3,000 | 60ms | 400 KB |
| **Improvement** | - | - | **80% less** | **5x faster** | **80% less** |

---

**Best Practices:**

1. **Always use `[UseProjection]` on queries returning entities**
   ```csharp
   //   Good
   [UseProjection]
   public IQueryable<DeviceApplication> GetApplications([Service] ProtectionDbContext db)
       => db.DeviceApplications;

   //   Bad (no optimization)
   public IEnumerable<DeviceApplication> GetApplications([Service] ProtectionDbContext db)
       => db.DeviceApplications.ToList();
   ```

2. **Return `IQueryable`, not `IEnumerable` or `List`**
   ```csharp
   //   Good - Allows projection
   return db.DeviceApplications.Where(d => d.StationName == stationName);

   //   Bad - ToList() materializes query, prevents projection
   return db.DeviceApplications.Where(d => d.StationName == stationName).ToList();
   ```

3. **Don't use for in-memory collections**
   ```csharp
   //   Projection doesn't help here (data already in memory)
   [UseProjection]
   public IQueryable<DeviceApplication> GetApplications([Service] DataStore dataStore)
       => dataStore.DeviceApplications.AsQueryable();  // No benefit
   ```

4. **Combine with `[UseFiltering]` and `[UseSorting]`**
   ```csharp
   [UseProjection]
   [UseFiltering]
   [UseSorting]
   public IQueryable<DeviceApplication> GetApplications([Service] ProtectionDbContext db)
       => db.DeviceApplications;
   ```

---

**Summary: Complete Optimization Stack**

| Feature | Purpose | Benefit | ECO Impact |
|---------|---------|---------|------------|
| **Resolvers** | Load relationships on-demand | 50-90% fewer queries | FunctionGroups only loaded if requested |
| **DataLoaders** | Batch N+1 queries | 90-98% fewer DB calls | 100 devices → 2 queries instead of 101 |
| **Projections** | Fetch only needed columns | 80-90% less data | Dashboard: 3 columns instead of 15 |
| **Network** | Serialize only requested fields | 50-95% smaller payload | 50 KB instead of 500 KB |
| **Combined** | Full-stack optimization | **10-100x faster** | **Sub-second dashboard loads** |

**ECO Migration Benefit:**

When migrating ECO from REST to GraphQL with projections:
- **Dashboard queries:** 5-10x faster (fewer columns)
- **Bulk operations:** 10-50x faster (DataLoaders + projections)
- **Memory usage:** 80-90% reduction (partial entities)
- **Network bandwidth:** 50-95% reduction (field selection + projections)
- **Database load:** 70-95% reduction (optimized SQL)

#### 5. Type System - Strong Typing

**Note**: GraphQL's type system provides strong typing across the **entire stack**:
- **Backend**: HotChocolate generates GraphQL schema from C# classes (code-first) or C# types from schema (schema-first)
- **Frontend**: Code generation creates TypeScript types from GraphQL schema
- **Runtime**: GraphQL validates all queries against the schema before execution

The SDL in the document is just for documentation/planning, not for implementation!

GraphQL has a rich type system that ensures queries are valid before execution.

**Scalar Types (Built-in):**
- `Int`: Signed 32-bit integer
- `Float`: Signed double-precision floating-point
- `String`: UTF-8 character sequence
- `Boolean`: true or false
- `ID`: Unique identifier (serialized as string)

**Custom Scalar Types:**
```graphql
scalar DateTime
scalar JSON
scalar EmailAddress

type DeviceApplication {
  createdAt: DateTime!     # ISO 8601 datetime
  metadata: JSON           # Arbitrary JSON object
  contactEmail: EmailAddress
}
```

**Enums:**
```graphql
enum DeviceType {
  SIPROTEC5_COMPACT
  SIPROTEC5
  SICAM8
}

enum FunctionType {
  OVERCURRENT
  DISTANCE_PROTECTION
  DIFFERENTIAL_PROTECTION
}
```

**Object Types:**
```graphql
type DeviceApplication {
  id: ID!
  publicTechnicalName: String!
  deviceName: String
  deviceType: DeviceType!
  functionGroups: [FunctionGroup!]!
  station: Station!
}
```

**Input Types (for mutations):**
```graphql
input CreateApplicationInput {
  stationName: String!
  publicTechnicalName: String!
  orderNumber: String!
  deviceName: String
  functionGroups: [CreateFunctionGroupInput!]
}

input CreateFunctionGroupInput {
  name: String!
  parentPtnPath: String!
  functions: [CreateFunctionInput!]
}
```

**Interfaces:**
```graphql
interface Node {
  id: ID!
}

type DeviceApplication implements Node {
  id: ID!
  publicTechnicalName: String!
  # ... other fields
}

type FunctionGroup implements Node {
  id: ID!
  name: String!
  # ... other fields
}
```

**Unions:**
```graphql
union SearchResult = DeviceApplication | FunctionGroup | Station

type Query {
  search(query: String!): [SearchResult!]!
}
```

### Advanced GraphQL Features

#### Fragments - Reusable Field Sets

Fragments allow you to define reusable sets of fields:

```graphql
fragment DeviceBasicInfo on DeviceApplication {
  publicTechnicalName
  deviceName
  orderNumber
  createdAt
}

fragment DeviceWithGroups on DeviceApplication {
  ...DeviceBasicInfo
  functionGroups {
    name
    publicTechnicalName
  }
}

query GetDevices {
  device1: application(stationName: "Station1", devicePtn: "Device1") {
    ...DeviceWithGroups
  }
  device2: application(stationName: "Station1", devicePtn: "Device2") {
    ...DeviceBasicInfo
  }
}
```

#### Directives - Conditional Queries

Directives modify query execution behavior:

**@include Directive:**
```graphql
query GetDevice($includeGroups: Boolean!) {
  application(stationName: "Station1", devicePtn: "Device1") {
    publicTechnicalName
    deviceName
    functionGroups @include(if: $includeGroups) {
      name
    }
  }
}
```

**@skip Directive:**
```graphql
query GetDevice($skipInfo: Boolean!) {
  application(stationName: "Station1", devicePtn: "Device1") {
    publicTechnicalName
    deviceInformation @skip(if: $skipInfo) {
      firmwareVersion
      ipAddress
    }
  }
}
```
<!-- 
#### Pagination Strategies

**1. Offset-Based Pagination (Simple):**
```graphql
query GetApplications {
  applications(stationName: "Station1", page: 1, pageSize: 20) {
    data {
      publicTechnicalName
      deviceName
    }
    totalCount
  }
}
```

**2. Cursor-Based Pagination (Relay Spec):**
```graphql
query GetApplications($after: String) {
  applicationsPaginated(stationName: "Station1", first: 20, after: $after) {
    edges {
      cursor
      node {
        publicTechnicalName
        deviceName
      }
    }
    pageInfo {
      hasNextPage
      endCursor
    }
    totalCount
  }
}
``` -->

### Introspection - Self-Documenting APIs

GraphQL schemas are introspectable - clients can query the schema itself:

**Query for All Types:**
```graphql
query IntrospectionQuery {
  __schema {
    types {
      name
      kind
      description
    }
  }
}
```

**Query for Specific Type:**
```graphql
query GetDeviceApplicationType {
  __type(name: "DeviceApplication") {
    name
    description
    fields {
      name
      type {
        name
        kind
      }
      description
    }
  }
}
```

**Tooling Benefits:**
- **GraphQL Playground / Banana Cake Pop**: Auto-generated documentation
- **Code Generation**: Generate TypeScript types from schema

### Best Practices

#### 1. Naming Conventions
- **Types**: PascalCase (`DeviceApplication`)
- **Fields**: camelCase (`publicTechnicalName`)
- **Enums**: UPPER_SNAKE_CASE (`SIPROTEC5_COMPACT`)
- **Arguments**: camelCase (`stationName`)

#### 2. Query Design
```graphql
#   Good: Specific, clear purpose
query GetDeviceForEditing($station: String!, $device: String!) {
  application(stationName: $station, devicePtn: $device) {
    id
    deviceName
    orderNumber
  }
}

#   Avoid: Generic, unclear intent
query GetData {
  application(stationName: "Station1", devicePtn: "Device1") {
    id
    publicTechnicalName
    deviceName
    orderNumber
    deviceType
    parentPtnPath
    createdAt
    updatedAt
    # ... fetching everything "just in case"
  }
}
```

#### 3. Nullable vs Non-Nullable
```graphql
#   Good: Non-nullable for critical fields
type DeviceApplication {
  id: ID!                      # Always present
  publicTechnicalName: String! # Required
  deviceName: String           # Optional (can be null)
}

#   Avoid: Making everything non-nullable
# (If a field can fail to resolve, make it nullable to avoid breaking entire query)
```

#### 5. DataLoaders for N+1 Problems
```csharp
//   Bad: N+1 queries
public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
    [Parent] DeviceApplication device,
    [Service] ProtectionDbContext dbContext)
{
    // This executes a separate query for EACH device!
    return await dbContext.FunctionGroups
        .Where(fg => fg.DevicePtn == device.PublicTechnicalName)
        .ToListAsync();
}

//   Good: Batched with DataLoader
public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
    [Parent] DeviceApplication device,
    FunctionGroupDataLoader dataLoader)
{
    // DataLoader batches all device PTNs into single query
    return await dataLoader.LoadAsync(device.PublicTechnicalName);
}
```

### GraphQL vs REST

| Aspect | REST | GraphQL |
|--------|------|---------|
| **Endpoints** | Multiple endpoints (`/applications`, `/functionGroups`) | Single endpoint (`/graphql`) |
| **Data Fetching** | Fixed response structure | Client specifies exact fields needed |
| **Multiple Resources** | Multiple HTTP requests | Single request with nested queries |
| **Versioning** | URL versioning (`/api/v1/`, `/api/v2/`) | Schema evolution (deprecation) |
| **Documentation** | Swagger/OpenAPI | Self-documenting via introspection |
| **Caching** | HTTP caching (ETags, headers) | Client-side caching (Apollo, Relay) |

## Technology Stack for GraphQL Implementation

### Backend (.NET 9)

#### HotChocolate 13.x vs GraphQL.NET - Detailed Comparison

**Recommended: HotChocolate 13.x**  

**Why HotChocolate:**

**Architecture & Performance:**
- **Native .NET Implementation**: Built specifically for modern .NET (6/7/8/9), leverages latest C# features
- **Performance**: Significantly faster than GraphQL.NET (~2-3x in benchmarks)
  - Optimized query execution pipeline
  - Minimal allocations and memory pressure
- **Code-First & Schema-First**: Flexible approach - generate schema from C# classes OR define SDL and generate types
- **AOT-Friendly**: Compatible with .NET Native AOT compilation (important for cloud deployments)

**Developer Experience:**
- **Built-in DataLoader**: Automatic batching and N+1 query resolution (GraphQL.NET requires manual implementation)
- **EF Core Integration**: Deep integration with Entity Framework Core
  - `IQueryable` projections (only fetch requested fields from database)
  - Automatic JOIN optimization
  - Built-in `UseProjection()`, `UseFiltering()`, `UseSorting()` attributes
- **Filtering, Sorting, Pagination**: Out-of-the-box with conventions
  
  ```csharp
  [UseProjection]
  [UseFiltering]
  [UseSorting]
  public IQueryable<DeviceApplication> GetApplications([Service] ProtectionDbContext db)
      => db.DeviceApplications;
  ```
- **GraphQL Playground**: Banana Cake Pop UI (superior to GraphiQL) included
- **Schema Stitching & Fusion**: Built-in support for combining multiple GraphQL schemas (microservices)

**Enterprise Features:**
- **Persisted Queries**: Query whitelisting for security and performance
- **Automatic Persisted Queries (APQ)**: Client-side caching optimization
- **Error Filtering**: Sophisticated error handling pipeline
- **Authorization**: Built-in `[Authorize]` attributes, policy-based authorization
- **Validation**: Comprehensive query validation with custom rules

**Migration Path:**
- **From REST**: Smooth migration, can reuse existing services and DTOs
- **From GraphQL.NET**: Migration tools available, mostly straightforward

**Example - Simple Query with HotChocolate:**
```csharp
public class Query
{ 
    public IQueryable<DeviceApplication> GetApplications(
        [Service] ProtectionDbContext db,
        string stationName)
    {
        return db.DeviceApplications.Where(a => a.StationName == stationName);
    }
}

// Generated GraphQL:
// query {
//   applications(where: { deviceName: { contains: "SIPROTEC" } }, order: { createdAt: DESC }) {
//     publicTechnicalName
//     deviceName
//   }
// }
```

---

**Alternative: GraphQL.NET**

**When to Consider GraphQL.NET:**
- Legacy projects already using GraphQL.NET (migration cost high)
- Team has deep GraphQL.NET expertise
- Need for specific GraphQL.NET plugins not yet ported to HotChocolate

**GraphQL.NET Characteristics:**

**Pros:**
- **Maturity**: Older library, has been around since 2015
- **Stability**: Well-tested in production environments

**Cons:**
- **Less Feature-Rich**: Missing many modern GraphQL features
  - No built-in DataLoader (requires separate library: `GraphQL.DataLoader`)
  - Manual filtering/sorting/pagination implementation required
  - Limited EF Core integration (manual query building)
- **More Manual Configuration**: Requires significant boilerplate
  ```csharp
  // GraphQL.NET requires manual type registration
  public class DeviceApplicationType : ObjectGraphType<DeviceApplication>
  {
      public DeviceApplicationType()
      {
          Field(x => x.PublicTechnicalName);
          Field(x => x.DeviceName, nullable: true);
          Field(x => x.OrderNumber);
          // ... manual field definitions for every property
      }
  }
  ```
- **Performance**: Slower than HotChocolate (reflection-heavy, more allocations)
- **Community**: Smaller community, slower development pace
- **EF Core**: No automatic query projection - must manually handle field selection
- **Documentation**: Less comprehensive for modern .NET versions

**Recommendation:**

  **Use HotChocolate 13.x**

**Rationale:**
1. **Performance Critical**: Bulk operations and hierarchical queries benefit from HotChocolate's optimizations
3. **Team Learning Curve**: HotChocolate's code-first approach aligns with team's C# expertise
3. **Future-Proof**: Active development, modern .NET features, AOT support
4. **Maintenance**: Less boilerplate = less code to maintain


## Three Migration Approaches - Comparative Analysis

When migrating from REST to GraphQL, there are three primary strategic approaches to consider. Each has distinct advantages, trade-offs, and use cases.

#### Approach 1: REST-over-GraphQL (Internal GraphQL Layer)

**Description**: REST endpoints remain unchanged for clients, but internally delegate to a GraphQL layer. GraphQL sits between REST controllers and business services.

**Architecture:**
```
┌────────────────────────────┐
│  Frontend/API Clients      │
└────────────┬───────────────┘
             │ HTTP REST calls
             ▼
┌────────────────────────────┐
│  REST Controllers          │  ← ApplicationController.cs
│  (Facade/Adapter Layer)    │
└────────────┬───────────────┘
             │ Internal GraphQL calls
             ▼
┌────────────────────────────┐
│  GraphQL Client/Executor   │  ← ProtectionGraphQLClient
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│  GraphQL Resolvers         │  ← Query/Mutation types
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│  Service Layer             │  ← IDeviceApplicationService
└────────────────────────────┘
```

**Implementation:**
- Keep all existing REST endpoint signatures unchanged
- Create GraphQL layer (schema, resolvers, mutations)
- Build internal GraphQL client that REST controllers use
- GraphQL endpoint is internal-only (not exposed externally)
- Clients continue using REST with no changes

**Pros:**
-   **Zero Breaking Changes**: Existing clients unaffected
-   **Internal Optimization**: Backend can leverage GraphQL features (DataLoaders, batching)
-   **Hidden Complexity**: GraphQL complexity hidden from frontend
-   **Gradual Backend Refactor**: Can optimize backend without frontend coordination
-   **Testable Architecture**: Can test GraphQL layer independently

**Cons:**
-   **Extra Translation Layer**: REST → GraphQL → Services (performance overhead)
-   **Lost GraphQL Benefits for Clients**: Frontend can't customize queries, no field selection
-   **Increased Code Complexity**: more code to maintain (REST + GraphQL + adapter)
-   **Dual Error Handling**: Must translate GraphQL errors to REST responses
-   **Serialization Overhead**: Object → JSON → Object conversions
-   **Complex Debugging**: 3 layers to troubleshoot (REST → GraphQL Client → GraphQL Resolvers)
-   **No Clear Business Value**: Adds complexity without delivering GraphQL benefits to consumers

**Best For:**
- Internal backend optimization without client changes
- Testing GraphQL before external exposure
- GraphQL aggregates multiple microservices (BFF pattern)
- Transition phase before full GraphQL adoption

**Not Recommended For:**
- Simple pass-through scenarios (GraphQL adds no value)
- Performance-critical paths (extra layer adds latency)


---

#### Approach 2: Gradual Migration (Strangler Pattern)

**Description**: Run REST and GraphQL side-by-side. Migrate endpoints incrementally, starting with high-value use cases. Eventually deprecate and remove REST.

**Implementation:**
- Phase 1: Setup GraphQL infrastructure, migrate critical endpoints (bulk operations)
- Phase 2: Migrate standard CRUD endpoints gradually
- Phase 3: Deprecate REST, monitor usage, remove after grace period
- Clients migrate feature-by-feature at their own pace

**Pros:**
-   **Risk Mitigation**: Incremental rollout, issues isolated to specific features
-   **Early Value**: High-impact endpoints migrated first (bulk operations)
-   **Real-world Validation**: POC with production traffic validates approach
-   **Flexible Approach**: Spread effort over multiple phases
-   **Client Flexibility**: Clients migrate when ready, no forced coordination
-   **Easy Rollback**: Fallback to REST per endpoint if issues arise
-   **Team Learning**: Team learns GraphQL incrementally, less overwhelming
-   **Budget-Friendly**: Effort spread over multiple quarters

**Cons:**
-   **Dual Maintenance**: Both APIs maintained during transition
-   **Complexity**: API Gateway routes to both REST and GraphQL
-   **Longer Overall Process**: Complete migration requires multiple phases

**Best For:**
- Large production APIs
- Multiple API consumers with independent release cycles
- Teams new to GraphQL (learning curve)


---

#### Approach 3: Permanent Hybrid (Keep Both APIs)

**Description**: Maintain both REST and GraphQL APIs indefinitely. Use each for their strengths: REST for file operations/simple queries, GraphQL for complex data aggregation.

**Implementation:**
- Migrate data-oriented endpoints to GraphQL
- Keep REST for file operations, webhooks, simple CRUD
- Both APIs maintained long-term
- Clients choose API based on use case

**Pros:**
-   **Best Tool for Job**: REST for files, GraphQL for data
-   **No Breaking Changes**: Existing REST clients never forced to migrate
-   **Flexibility**: New features can use either API
-   **Backward Compatibility**: Legacy integrations continue working
-   **Gradual Adoption**: Teams adopt GraphQL at own pace, no pressure

**Cons:**
-   **Permanent Maintenance**: Two APIs to maintain, test, document, monitor forever
-   **Increased Complexity**: API Gateway routes to both, infrastructure overhead
-   **Inconsistent Client Experience**: Some clients use REST, some GraphQL
-   **Higher Operational Costs**: Double monitoring, logging, security reviews

**Best For:**
- APIs with distinct use cases (data vs files)
- Organizations with many external API consumers
- Microservices with different access patterns
- Long-term legacy support requirements

**Not Recommended For:**
- New projects (starts with technical debt)

**Use Cases Where Hybrid Makes Sense:**

| Operation Type | API Technology | Reason |
|----------------|---------------|---------|
| Bulk CRUD operations | GraphQL | Batch mutations, nested creation |
| Hierarchical queries | GraphQL | Single request for entire tree |
| Complex filtering/sorting | GraphQL | Client-driven field selection |
| File uploads/downloads | REST | HTTP streaming, browser download handling |
| Webhooks/callbacks | REST | Standard HTTP POST, no client state |
| Simple health checks | REST | Lightweight, no authentication needed |

---

## ApplicationController Endpoint Analysis

### Complete Endpoint Inventory

The `ApplicationController` contains **13 REST endpoints**. Below is the comprehensive analysis of each endpoint's suitability for GraphQL migration.

####  Excellent Candidates for GraphQL (11 endpoints)

| # | Endpoint | Method | Route | Reason |
|---|----------|--------|-------|--------|
| 1 | `GetApplicationFromStation` | GET | `/applications/{stationName}` | Perfect for GraphQL field selection and nested queries (device → FGs → functions) |
| 2 | `CreateApplicationsAsync` | POST | `/applications` | Bulk creation, supports nesting function groups - major pain point |
| 3 | `DeleteApplicationAsync` | DELETE | `/applications` | Bulk delete operation, already batch-oriented |
| 4 | `CreateBuildingBlockWithSelectedChildrenAsync` | POST | `/buildingBlocks` | Nested creation of FGs with functions - complex hierarchy |
| 5 | `DuplicateApplicationAsync` | POST | `/applications/duplicate/{devicePtnPath}` | Simple mutation, clear input/output |
| 6 | `UpdateDeviceInformationAsync` | PATCH | `/applications/{stationName}` | Partial updates align well with GraphQL mutations |
| 7 | `MoveDeviceAsync` | PATCH | `/applications/move` | Simple state change mutation |
| 8 | `GetDeviceInformation` | GET | `/hardware/{stationName}/{devicePtn}` | Can be nested under device queries, reduces round trips |
| 9 | `GetDeviceHeaders` | GET | `/applications/header/{stationName}` | Lightweight data, perfect for GraphQL projection |
| 10 | `GetStationData` | GET | `/stationId` | Part of station hierarchy, benefits from nesting |
| 11 | `GetDevicePathAsync` | GET | `/application/path` | Simple query, minimal benefit but can be included |

####  Keep as REST (2 endpoints)

| # | Endpoint | Method | Route | Keep REST | Reason |
|---|----------|--------|-------|-----------|--------|
| 12 | `ExportTcfAsync` | POST | `/exportTcf` | **YES** | Binary file download (TCF format) - GraphQL not optimized for large binaries |
| 13 | `ExportIidAsync` | POST | `/exportIid` | **YES** | Binary file download (IID format) - GraphQL not optimized for large binaries |

<!-- 
## GraphQL Schema Design

**Note**: This schema is written in GraphQL SDL (Schema Definition Language) for **documentation and design purposes**. When using HotChocolate **Code-First** approach, you'll implement these types as C# classes, and HotChocolate will automatically generate the GraphQL schema. This SDL serves as:
- Design blueprint for backend developers
- API contract for frontend team
- Communication tool for architecture reviews
- Reference for type generation and documentation

> The generated schema can be viewed at `/graphql` endpoint using Banana Cake Pop UI.
┌─────────────────────────────────────────────────────────┐
│ 1. DESIGN PHASE (Before Coding)                        │
│    - Write SDL in document (like what's shown)         │
│    - Review with team                                   │
│    - Get agreement on API contract                      │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 2. DEVELOPMENT PHASE (Implementation)                  │
│    - Write C# classes matching the design              │
│    - Add HotChocolate attributes                        │
│    - HotChocolate generates schema automatically        │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 3. RUNTIME                                              │
│    - Schema visible at /graphql endpoint                │
│    - Frontend generates TypeScript types from it        │
│    - No manual SDL maintenance needed                   │
└─────────────────────────────────────────────────────────┘
 -->
