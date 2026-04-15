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
4. **Performance Validation**: Compare REST vs GraphQL for bulk operations
5. **Documentation**: How-to guides, schema design, migration approach

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

**Resolver Signature (C# with HotChocolate):**
```csharp
public async Task<DeviceApplication?> GetApplication(
    [Service] IDeviceApplicationService service,  // Dependency injection
    string stationName,                           // Arguments
    string devicePtny)          
{
    return await service.GetApplicationAsync(stationName, devicePtn);
}
```

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
```

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
# ✅ Good: Specific, clear purpose
query GetDeviceForEditing($station: String!, $device: String!) {
  application(stationName: $station, devicePtn: $device) {
    id
    deviceName
    orderNumber
  }
}

# ❌ Avoid: Generic, unclear intent
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
# ✅ Good: Non-nullable for critical fields
type DeviceApplication {
  id: ID!                      # Always present
  publicTechnicalName: String! # Required
  deviceName: String           # Optional (can be null)
}

# ❌ Avoid: Making everything non-nullable
# (If a field can fail to resolve, make it nullable to avoid breaking entire query)
```

#### 5. DataLoaders for N+1 Problems
```csharp
// ❌ Bad: N+1 queries
public async Task<IEnumerable<FunctionGroup>> GetFunctionGroups(
    [Parent] DeviceApplication device,
    [Service] ProtectionDbContext dbContext)
{
    // This executes a separate query for EACH device!
    return await dbContext.FunctionGroups
        .Where(fg => fg.DevicePtn == device.PublicTechnicalName)
        .ToListAsync();
}

// ✅ Good: Batched with DataLoader
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

**Recommended: HotChocolate 13.x** ⭐

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

✅ **Use HotChocolate 13.x**

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
- ✅ **Zero Breaking Changes**: Existing clients unaffected
- ✅ **Internal Optimization**: Backend can leverage GraphQL features (DataLoaders, batching)
- ✅ **Hidden Complexity**: GraphQL complexity hidden from frontend
- ✅ **Gradual Backend Refactor**: Can optimize backend without frontend coordination
- ✅ **Testable Architecture**: Can test GraphQL layer independently

**Cons:**
- ❌ **Extra Translation Layer**: REST → GraphQL → Services (performance overhead)
- ❌ **Lost GraphQL Benefits for Clients**: Frontend can't customize queries, no field selection
- ❌ **Increased Code Complexity**: more code to maintain (REST + GraphQL + adapter)
- ❌ **Dual Error Handling**: Must translate GraphQL errors to REST responses
- ❌ **Serialization Overhead**: Object → JSON → Object conversions
- ❌ **Complex Debugging**: 3 layers to troubleshoot (REST → GraphQL Client → GraphQL Resolvers)
- ❌ **No Clear Business Value**: Adds complexity without delivering GraphQL benefits to consumers

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
- ✅ **Risk Mitigation**: Incremental rollout, issues isolated to specific features
- ✅ **Early Value**: High-impact endpoints migrated first (bulk operations)
- ✅ **Real-world Validation**: POC with production traffic validates approach
- ✅ **Flexible Approach**: Spread effort over multiple phases
- ✅ **Client Flexibility**: Clients migrate when ready, no forced coordination
- ✅ **Easy Rollback**: Fallback to REST per endpoint if issues arise
- ✅ **Team Learning**: Team learns GraphQL incrementally, less overwhelming
- ✅ **Budget-Friendly**: Effort spread over multiple quarters

**Cons:**
- ⚠️ **Dual Maintenance**: Both APIs maintained during transition
- ⚠️ **Complexity**: API Gateway routes to both REST and GraphQL
- ⚠️ **Longer Overall Process**: Complete migration requires multiple phases

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
- ✅ **Best Tool for Job**: REST for files, GraphQL for data
- ✅ **No Breaking Changes**: Existing REST clients never forced to migrate
- ✅ **Flexibility**: New features can use either API
- ✅ **Backward Compatibility**: Legacy integrations continue working
- ✅ **Gradual Adoption**: Teams adopt GraphQL at own pace, no pressure

**Cons:**
- ❌ **Permanent Maintenance**: Two APIs to maintain, test, document, monitor forever
- ❌ **Increased Complexity**: API Gateway routes to both, infrastructure overhead
- ❌ **Inconsistent Client Experience**: Some clients use REST, some GraphQL
- ❌ **Higher Operational Costs**: Double monitoring, logging, security reviews

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

#### ✅ Excellent Candidates for GraphQL (11 endpoints)

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

#### ❌ Keep as REST (2 endpoints)

| # | Endpoint | Method | Route | Keep REST | Reason |
|---|----------|--------|-------|-----------|--------|
| 12 | `ExportTcfAsync` | POST | `/exportTcf` | **YES** | Binary file download (TCF format) - GraphQL not optimized for large binaries |
| 13 | `ExportIidAsync` | POST | `/exportIid` | **YES** | Binary file download (IID format) - GraphQL not optimized for large binaries |


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

### Core Types

> **Note**: This section shows representative schema types. For detailed explanations of GraphQL type system, type modifiers, and usage examples, see the **"GraphQL Fundamentals - How-to GraphQL"** section above.

```graphql
# Scalars & Enums
scalar DateTime
scalar JSON

enum DeviceType {
  SIPROTEC5_COMPACT
  SIPROTEC5
  SICAM8
}

enum ValidationSeverity {
  ERROR
  WARNING
  INFO
}

# Example: Core Domain Type with Relationships
type DeviceApplication {
  id: ID!
  publicTechnicalName: String!
  deviceName: String!
  stationName: String!
  orderNumber: String!
  deviceType: DeviceType!

  # Relationships (see GraphQL Fundamentals section for nested query examples)
  deviceInformation: DeviceInformation
  functionGroups: [FunctionGroup!]!
  station: Station!

  # Metadata
  createdAt: DateTime!
  updatedAt: DateTime!
}

# Example: API Response Wrapper Pattern
type ApplicationResponse {
  data: [DeviceApplication!]
  errors: [ValidationError!]
  warnings: [ValidationError!]
}

type ValidationError {
  code: String!
  message: String!
  severity: ValidationSeverity!
  path: String
}

# ... (Additional types: Station, DeviceInformation, FunctionGroup, ProtectionFunction, etc.)
# Full schema viewable at /graphql endpoint via Banana Cake Pop UI
```

### Query Definitions

> **Note**: For detailed query examples with variables, field selection, and nested data fetching, see the **"Queries - Reading Data"** section in GraphQL Fundamentals above.

```graphql
type Query {
  applications(
    stationName: String!
    devicePtns: [String!]
  ): ApplicationResponse!

  # Example: Single entity query
  deviceInformation(stationName: String!, devicePtn: String!): DeviceInformation

  # ... (Additional queries: station, functionGroups, deviceHeaders, etc.)
  # Full schema viewable at /graphql endpoint via Banana Cake Pop UI
}
```

### Mutation Definitions

> **Note**: For detailed mutation examples with variables, nested creation, and bulk operations, see the **"Mutations - Modifying Data"** section in GraphQL Fundamentals above.

```graphql
type Mutation {
  # Example: Standard CRUD mutation with batch support
  createApplications(input: [CreateApplicationInput!]!): ApplicationResponse!

  # Example: Bulk operation with partial success handling
  bulkCreateApplicationsWithFunctionGroups(
    input: [BulkCreateApplicationInput!]!
  ): BulkApplicationResponse!

  # ... (Additional mutations: update, delete, move, restore, etc.)
  # Full schema viewable at /graphql endpoint via Banana Cake Pop UI
}

# Input types for mutations
input CreateApplicationInput {
  stationName: String!
  publicTechnicalName: String!
  orderNumber: String!
  deviceName: String

  # Nested creation (see Mutations section in GraphQL Fundamentals for examples)
  deviceInformation: DeviceInformationInput
  functionGroups: [CreateFunctionGroupInput!]
}
```

### Frontend Integration

**Apollo Client (TypeScript/Angular)**

```typescript
// Install
npm install @apollo/client graphql

// Configure Apollo Client
import { ApolloClient, InMemoryCache, HttpLink } from '@apollo/client';

const client = new ApolloClient({
  link: new HttpLink({
    uri: 'https://localhost:5001/graphql',
    headers: {
      authorization: `Bearer ${token}`
    }
  }),
  cache: new InMemoryCache()
});

// Query example
const GET_APPLICATIONS = gql`
  query GetApplications($stationName: String!, $devicePtns: [String!]) {
    applications(stationName: $stationName, devicePtns: $devicePtns) {
      data {
        publicTechnicalName
        deviceName
        functionGroups {
          name
          functions {
            name
            type
          }
        }
      }
      errors {
        message
        code
      }
    }
  }
`;

// Usage in Angular service
export class ApplicationService {
  getApplications(stationName: string, devicePtns?: string[]) {
    return this.apollo.query({
      query: GET_APPLICATIONS,
      variables: { stationName, devicePtns }
    });
  }
}
```

