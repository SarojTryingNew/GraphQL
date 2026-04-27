# GraphQL Query Examples - Complete Guide

This document provides **complete examples** for calling every GraphQL query in the DeviceApplication domain. All queries can be executed at the `/graphql` endpoint.

---

## 📍 **GraphQL Endpoint**

**URL:** `https://localhost:7XXX/graphql`  
**Tool:** Banana Cake Pop (built-in GraphQL IDE)

**Access:** Navigate to `/graphql` in your browser to open the interactive GraphQL IDE.

---

## 🎯 **Query Structure Overview**

### **Available Query Root Fields:**

```graphql
type Query {
  # Core Queries (DeviceApplicationQueries.cs)
  deviceApplications(where: DeviceApplicationFilterInput): [DeviceApplication!]!
  deviceApplication(publicTechnicalName: String!): DeviceApplication

  # Extended Queries (DeviceApplicationExtendedQueries.cs)
  functionGroups(devicePublicTechnicalName: String!): [FunctionGroup!]!
  functionGroup(ptnPath: String!): FunctionGroup
  functionBlocks(devicePublicTechnicalName: String!): [FunctionBlock!]!
  functionBlock(ptnPath: String!): FunctionBlock
  signals(functionBlockPtnPath: String!): [Signal!]!
  allPtnPaths(devicePublicTechnicalName: String!): [String!]!
  functions: [Function!]!
  function(ptnPath: String!): Function
  subsignals(signalPtnPath: String!): [Subsignal!]!
  cdcConversions(signalPtnPath: String!): [CdcConversion!]!
  routings(ptnPath: String!): [Routing!]!

  # Type-Based Queries
  deviceApplicationsByType(typeName: String!): [DeviceApplication!]!
  deviceApplicationsByTypes(typeNames: [String!]!): [DeviceApplication!]!
  deviceTypes: [String!]!
}
```

---

## 📋 **Scenario-by-Scenario Query Examples**

---

### **Scenario 1: Get All Devices**

**Method:** `GetDeviceApplications()`  
**File:** `DeviceApplicationQueries.cs` (Line 21)

#### **Basic Query:**
```graphql
query GetAllDevices {
  deviceApplications {
    publicTechnicalName
    displayText
    typeName
    dddVersion
    comDddVersion
    lastUpdatedAt
    lastModifiedBy
  }
}
```

#### **Response:**
```json
{
  "data": {
    "deviceApplications": [
      {
        "publicTechnicalName": "IED_001",
        "displayText": "Protection IED 1",
        "typeName": "IED",
        "dddVersion": "2.1.0",
        "comDddVersion": "1.5.0",
        "lastUpdatedAt": "2025-01-15T10:30:00Z",
        "lastModifiedBy": "admin"
      },
      {
        "publicTechnicalName": "IED_002",
        "displayText": "Protection IED 2",
        "typeName": "IED",
        "dddVersion": "2.0.0",
        "comDddVersion": null,
        "lastUpdatedAt": "2025-01-14T09:20:00Z",
        "lastModifiedBy": "user1"
      }
    ]
  }
}
```

#### **With Nested Relationships:**
```graphql
query GetAllDevicesWithRelationships {
  deviceApplications {
    publicTechnicalName
    displayText
    functionGroups {
      publicTechnicalName
      displayText
    }
    functionBlocks {
      publicTechnicalName
      displayText
    }
  }
}
```

#### **With Statistics (Scenario 7):**
```graphql
query GetAllDevicesWithStats {
  deviceApplications {
    publicTechnicalName
    displayText
    typeName
    functionGroupCount
    functionBlockCount
    signalCount
    analogSignalCount
    statusSignalCount
    hasConfiguration
  }
}
```

---

### **Scenario 2: Get Single Device**

**Method:** `GetDeviceApplication(publicTechnicalName)`  
**File:** `DeviceApplicationQueries.cs` (Line 28)

#### **Query:**
```graphql
query GetSingleDevice {
  deviceApplication(publicTechnicalName: "IED_001") {
    publicTechnicalName
    displayText
    typeName
    dddVersion
    comDddVersion
    lastUpdatedAt
    lastModifiedBy
  }
}
```

#### **With Variables:**
```graphql
query GetSingleDevice($deviceName: String!) {
  deviceApplication(publicTechnicalName: $deviceName) {
    publicTechnicalName
    displayText
    typeName
    functionGroups {
      publicTechnicalName
      displayText
    }
  }
}

# Variables:
{
  "deviceName": "IED_001"
}
```

#### **Full Device Tree:**
```graphql
query GetFullDeviceTree {
  deviceApplication(publicTechnicalName: "IED_001") {
    publicTechnicalName
    displayText
    typeName
    dddVersion

    functionGroups {
      publicTechnicalName
      displayText
      functions {
        publicTechnicalName
        displayText
      }
    }

    functionBlocks {
      publicTechnicalName
      displayText
      signals {
        publicTechnicalName
        displayText
        type
        cdcType
      }
    }
  }
}
```

---

### **Scenario 3: Get Function Groups by Device**

**Method:** `GetFunctionGroups(devicePublicTechnicalName)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 16)

#### **Query:**
```graphql
query GetFunctionGroups {
  functionGroups(devicePublicTechnicalName: "IED_001") {
    publicTechnicalName
    displayText
    typeName
    ptnPath
    isDeletable
  }
}
```

#### **With Variables:**
```graphql
query GetFunctionGroups($device: String!) {
  functionGroups(devicePublicTechnicalName: $device) {
    publicTechnicalName
    displayText
    ptnPath
    functions {
      publicTechnicalName
      displayText
    }
  }
}

# Variables:
{
  "device": "IED_001"
}
```

---

### **Scenario 4: Get Single Function Group**

**Method:** `GetFunctionGroup(ptnPath)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 24)

#### **Query:**
```graphql
query GetSingleFunctionGroup {
  functionGroup(ptnPath: "IED_001/PROTECTION") {
    publicTechnicalName
    displayText
    typeName
    ptnPath
    isDeletable
    functions {
      publicTechnicalName
      displayText
    }
    functionBlocks {
      publicTechnicalName
      displayText
    }
  }
}
```

---

### **Scenario 5: Get Function Blocks by Device**

**Method:** `GetFunctionBlocks(devicePublicTechnicalName)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 32)

#### **Query:**
```graphql
query GetFunctionBlocks {
  functionBlocks(devicePublicTechnicalName: "IED_001") {
    publicTechnicalName
    displayText
    typeName
    originalName
    ptnPath
    isDeletable
  }
}
```

#### **With Nested Signals:**
```graphql
query GetFunctionBlocksWithSignals {
  functionBlocks(devicePublicTechnicalName: "IED_001") {
    publicTechnicalName
    displayText
    ptnPath
    signals {
      publicTechnicalName
      displayText
      type
      cdcType
    }
  }
}
```

---

### **Scenario 6: Get Single Function Block**

**Method:** `GetFunctionBlock(ptnPath)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 40)

#### **Query:**
```graphql
query GetSingleFunctionBlock {
  functionBlock(ptnPath: "IED_001/MMXU1") {
    publicTechnicalName
    displayText
    typeName
    originalName
    ptnPath
    isDeletable
    signals {
      publicTechnicalName
      displayText
      type
      cdcType
    }
  }
}
```

---

### **Scenario 7: Get Signals by Function Block**

**Method:** `GetSignals(functionBlockPtnPath)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 48)

#### **Query:**
```graphql
query GetSignals {
  signals(functionBlockPtnPath: "IED_001/MMXU1") {
    publicTechnicalName
    displayText
    type
    cdcType
    ptnPath
    isDeletable
  }
}
```

#### **With Subsignals and Routings:**
```graphql
query GetSignalsWithDetails {
  signals(functionBlockPtnPath: "IED_001/MMXU1") {
    publicTechnicalName
    displayText
    type
    cdcType
    subSignals {
      publicTechnicalName
      displayText
    }
    cdcConversions {
      sourceCdc
      targetCdc
    }
    routings {
      ptnPath
      value
      isReadonly
      options
    }
  }
}
```

---

### **Scenario 8: Get All PTN Paths for a Device**

**Method:** `GetAllPtnPaths(devicePublicTechnicalName)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 56)

#### **Query:**
```graphql
query GetAllPtnPaths {
  allPtnPaths(devicePublicTechnicalName: "IED_001")
}
```

#### **Response:**
```json
{
  "data": {
    "allPtnPaths": [
      "IED_001",
      "IED_001/AUTOMATION",
      "IED_001/AUTOMATION/AutoClose",
      "IED_001/MMXU1",
      "IED_001/MMXU1/TotW",
      "IED_001/PROTECTION",
      "IED_001/PROTECTION/OverCurrent",
      "IED_001/XCBR1",
      "IED_001/XCBR1/Pos"
    ]
  }
}
```

---

### **Scenario 9: Get All Functions**

**Method:** `GetFunctions()`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 91)

#### **Query:**
```graphql
query GetAllFunctions {
  functions {
    publicTechnicalName
    displayText
    typeName
    ptnPath
    isDeletable
  }
}
```

#### **With Nested Data:**
```graphql
query GetAllFunctionsWithRelations {
  functions {
    publicTechnicalName
    displayText
    ptnPath
    functionBlocks {
      publicTechnicalName
      displayText
    }
    signals {
      publicTechnicalName
      type
    }
  }
}
```

---

### **Scenario 10: Get Single Function**

**Method:** `GetFunction(ptnPath)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 99)

#### **Query:**
```graphql
query GetSingleFunction {
  function(ptnPath: "IED_001/PROTECTION/OverCurrent") {
    publicTechnicalName
    displayText
    typeName
    ptnPath
    isDeletable
    functionBlocks {
      publicTechnicalName
      displayText
    }
  }
}
```

---

### **Scenario 11: Get Subsignals by Signal**

**Method:** `GetSubsignals(signalPtnPath)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 107)

#### **Query:**
```graphql
query GetSubsignals {
  subsignals(signalPtnPath: "IED_001/MMXU1/TotW") {
    publicTechnicalName
    displayText
    typeName
    cdcType
    ptnPath
    isDeletable
  }
}
```

#### **With Routings:**
```graphql
query GetSubsignalsWithRoutings {
  subsignals(signalPtnPath: "IED_001/MMXU1/TotW") {
    publicTechnicalName
    displayText
    ptnPath
    routings {
      ptnPath
      value
      isReadonly
    }
  }
}
```

---

### **Scenario 12: Get CDC Conversions by Signal**

**Method:** `GetCdcConversions(signalPtnPath)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 115)

#### **Query:**
```graphql
query GetCdcConversions {
  cdcConversions(signalPtnPath: "IED_001/MMXU1/TotW") {
    publicTechnicalName
    displayText
    sourceCdc
    targetCdc
    ptnPath
  }
}
```

#### **With Routings:**
```graphql
query GetCdcConversionsWithRoutings {
  cdcConversions(signalPtnPath: "IED_001/MMXU1/TotW") {
    publicTechnicalName
    sourceCdc
    targetCdc
    routings {
      ptnPath
      value
    }
  }
}
```

---

### **Scenario 13: Get Routings by PTN Path**

**Method:** `GetRoutings(ptnPath)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 123)

#### **Query:**
```graphql
query GetRoutings {
  routings(ptnPath: "IED_001/MMXU1/TotW") {
    ptnPath
    value
    isReadonly
    options
  }
}
```

#### **Response:**
```json
{
  "data": {
    "routings": [
      {
        "ptnPath": "IED_001/MMXU1/TotW/Route1",
        "value": "Local",
        "isReadonly": true,
        "options": ["Local", "Remote", "Disabled"]
      }
    ]
  }
}
```

---

### **Scenario 14: Type-Based Device Grouping (Single Type)**

**Method:** `GetDeviceApplicationsByType(typeName)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 133)

#### **Query:**
```graphql
query GetDevicesByType {
  deviceApplicationsByType(typeName: "IED") {
    publicTechnicalName
    displayText
    typeName
    dddVersion
  }
}
```

#### **With Statistics:**
```graphql
query GetDevicesByTypeWithStats {
  deviceApplicationsByType(typeName: "IED") {
    publicTechnicalName
    displayText
    functionGroupCount
    functionBlockCount
    signalCount
  }
}
```

---

### **Scenario 15: Type-Based Device Grouping (Multiple Types)**

**Method:** `GetDeviceApplicationsByTypes(typeNames)`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 143)

#### **Query:**
```graphql
query GetDevicesByMultipleTypes {
  deviceApplicationsByTypes(typeNames: ["IED", "ProtectionRelay", "ControlUnit"]) {
    publicTechnicalName
    displayText
    typeName
    dddVersion
  }
}
```

#### **With Variables:**
```graphql
query GetDevicesByMultipleTypes($types: [String!]!) {
  deviceApplicationsByTypes(typeNames: $types) {
    publicTechnicalName
    displayText
    typeName
    functionGroupCount
    signalCount
  }
}

# Variables:
{
  "types": ["IED", "ProtectionRelay"]
}
```

---

### **Scenario 16: Get All Unique Device Types**

**Method:** `GetDeviceTypes()`  
**File:** `DeviceApplicationExtendedQueries.cs` (Line 152)

#### **Query:**
```graphql
query GetDeviceTypes {
  deviceTypes
}
```

#### **Response:**
```json
{
  "data": {
    "deviceTypes": [
      "ControlUnit",
      "IED",
      "ProtectionRelay",
      "SIPROTEC5"
    ]
  }
}
```

---

## 🎯 **Scenario 5: Type-Based Device Grouping with [UseFiltering]**

**Method:** `GetDeviceApplications()` with WHERE clause  
**File:** `DeviceApplicationQueries.cs` (Line 21)

This scenario uses HotChocolate's filtering feature.

### **Filter by Single Type:**
```graphql
query DevicesByType {
  deviceApplications(where: { typeName: { eq: "IED" } }) {
    publicTechnicalName
    displayText
    typeName
  }
}
```

### **Filter by Multiple Types:**
```graphql
query DevicesByMultipleTypes {
  deviceApplications(where: { typeName: { in: ["IED", "ProtectionRelay"] } }) {
    publicTechnicalName
    displayText
    typeName
  }
}
```

### **Complex Filter (Type + Display Text):**
```graphql
query ComplexFilter {
  deviceApplications(
    where: { 
      and: [
        { typeName: { eq: "IED" } }
        { displayText: { contains: "Protection" } }
      ]
    }
  ) {
    publicTechnicalName
    displayText
    typeName
  }
}
```

### **Filter with Statistics:**
```graphql
query FilteredDevicesWithStats {
  deviceApplications(where: { typeName: { eq: "IED" } }) {
    publicTechnicalName
    displayText
    typeName
    functionGroupCount
    functionBlockCount
    signalCount
    analogSignalCount
    statusSignalCount
  }
}
```

### **Multiple Type Groups in One Query:**
```graphql
query DashboardByType {
  protectionRelays: deviceApplications(where: { typeName: { eq: "ProtectionRelay" } }) {
    publicTechnicalName
    displayText
    functionGroupCount
    signalCount
  }

  controlUnits: deviceApplications(where: { typeName: { eq: "ControlUnit" } }) {
    publicTechnicalName
    displayText
    functionGroupCount
    signalCount
  }

  iedDevices: deviceApplications(where: { typeName: { eq: "IED" } }) {
    publicTechnicalName
    displayText
    functionGroupCount
    signalCount
  }
}
```

---

## 📊 **Scenario 7: Device Statistics Dashboard**

**Computed Fields Available:**  
All statistics are computed fields on the `DeviceApplication` type.

### **Basic Statistics:**
```graphql
query DeviceStatistics {
  deviceApplications {
    publicTechnicalName
    displayText
    typeName

    # Structure counts
    functionGroupCount
    functionBlockCount
    functionCount

    # Signal counts
    signalCount
    analogSignalCount
    statusSignalCount
    subsignalCount

    # Configuration status
    hasConfiguration
    routingCount
    configuredRoutingCount
    editableRoutingCount
    cdcConversionCount
  }
}
```

### **Dashboard Summary:**
```graphql
query DashboardSummary {
  deviceApplications {
    publicTechnicalName
    displayText
    typeName
    dddVersion
    lastUpdatedAt

    functionGroupCount
    functionBlockCount
    signalCount
    hasConfiguration
  }
}
```

### **Configuration Coverage Report:**
```graphql
query ConfigurationCoverage {
  deviceApplications {
    publicTechnicalName
    displayText
    signalCount
    routingCount
    configuredRoutingCount
    editableRoutingCount
    hasConfiguration

    # Frontend calculates:
    # coveragePercentage = (configuredRoutingCount / routingCount) * 100
  }
}
```

### **Type-Based Statistics:**
```graphql
query StatsByDeviceType {
  deviceApplications {
    publicTechnicalName
    displayText
    typeName
    functionGroupCount
    functionBlockCount
    signalCount
    analogSignalCount
    statusSignalCount
  }
}
```

---

## 🔗 **Combined Scenarios**

### **Example 1: Device Comparison**
```graphql
query CompareDevices {
  device1: deviceApplication(publicTechnicalName: "IED_001") {
    publicTechnicalName
    displayText
    typeName
    dddVersion
    functionGroupCount
    functionBlockCount
    signalCount
  }

  device2: deviceApplication(publicTechnicalName: "IED_002") {
    publicTechnicalName
    displayText
    typeName
    dddVersion
    functionGroupCount
    functionBlockCount
    signalCount
  }

  device3: deviceApplication(publicTechnicalName: "IED_003") {
    publicTechnicalName
    displayText
    typeName
    dddVersion
    functionGroupCount
    functionBlockCount
    signalCount
  }
}
```

### **Example 2: Multi-Level Data Fetch**
```graphql
query MultiLevelData {
  deviceApplications(where: { typeName: { eq: "IED" } }) {
    publicTechnicalName
    displayText
    typeName
    functionGroupCount

    functionGroups {
      publicTechnicalName
      displayText
      functions {
        publicTechnicalName
        displayText
      }
    }

    functionBlocks {
      publicTechnicalName
      displayText
      signals {
        publicTechnicalName
        type
        cdcType
      }
    }
  }
}
```

### **Example 3: Dashboard with Type Filters**
```graphql
query DashboardWithFilters {
  # All available types for dropdown
  allTypes: deviceTypes

  # Get IED devices with stats
  iedDevices: deviceApplications(where: { typeName: { eq: "IED" } }) {
    publicTechnicalName
    displayText
    functionGroupCount
    signalCount
    hasConfiguration
  }

  # Get specific device details
  specificDevice: deviceApplication(publicTechnicalName: "IED_001") {
    publicTechnicalName
    displayText
    functionGroups {
      publicTechnicalName
    }
  }
}
```

---

## 🧪 **Testing in Banana Cake Pop**

### **Access the GraphQL IDE:**

1. Start your application
2. Navigate to: `https://localhost:7XXX/graphql`
3. The Banana Cake Pop IDE will open

### **Features Available:**

- ✅ **IntelliSense** - Auto-complete for queries
- ✅ **Schema Explorer** - Browse all available queries and types
- ✅ **Variable Editor** - Test queries with variables
- ✅ **Query History** - Previous queries saved
- ✅ **Response Formatting** - JSON formatted responses

### **Sample Test Flow:**

1. **Test Basic Query:**
   ```graphql
   query Test {
     deviceApplications {
       publicTechnicalName
       displayText
     }
   }
   ```

2. **Test Filtering:**
   ```graphql
   query TestFiltering {
     deviceApplications(where: { typeName: { eq: "IED" } }) {
       publicTechnicalName
     }
   }
   ```

3. **Test Statistics:**
   ```graphql
   query TestStats {
     deviceApplications {
       publicTechnicalName
       functionGroupCount
       signalCount
     }
   }
   ```

---

## 📝 **Filtering Capabilities (Scenario 5)**

### **Available Filter Operators:**

```graphql
# Equality
where: { typeName: { eq: "IED" } }

# Inequality
where: { typeName: { neq: "ProtectionRelay" } }

# In list
where: { typeName: { in: ["IED", "ProtectionRelay"] } }

# Not in list
where: { typeName: { nin: ["OldType"] } }

# Contains (string)
where: { displayText: { contains: "Protection" } }

# Starts with (string)
where: { displayText: { startsWith: "IED" } }

# Ends with (string)
where: { displayText: { endsWith: "001" } }

# AND logic
where: {
  and: [
    { typeName: { eq: "IED" } }
    { displayText: { contains: "Protection" } }
  ]
}

# OR logic
where: {
  or: [
    { typeName: { eq: "IED" } }
    { typeName: { eq: "ProtectionRelay" } }
  ]
}
```

---

## 🎯 **Performance Tips**

### **1. Request Only Needed Fields:**
```graphql
# ❌ Bad - Over-fetching
query {
  deviceApplications {
    publicTechnicalName
    displayText
    typeName
    dddVersion
    comDddVersion
    lastUpdatedAt
    lastModifiedBy
    functionGroups {
      publicTechnicalName
      displayText
      # ... all fields
    }
  }
}

# ✅ Good - Minimal fields
query {
  deviceApplications {
    publicTechnicalName
    displayText
  }
}
```

### **2. Use Aliases for Multiple Queries:**
```graphql
query {
  ied001: deviceApplication(publicTechnicalName: "IED_001") {
    functionGroupCount
  }
  ied002: deviceApplication(publicTechnicalName: "IED_002") {
    functionGroupCount
  }
}
```

### **3. Leverage DataLoaders (Automatic):**
```graphql
# This query automatically batches database calls
query {
  deviceApplications {
    publicTechnicalName
    functionGroups {  # Batched via DataLoader
      publicTechnicalName
    }
  }
}
```

---

## 🔍 **Error Handling**

### **GraphQL Error Response:**
```json
{
  "errors": [
    {
      "message": "Device application with PublicTechnicalName 'INVALID' not found",
      "locations": [{ "line": 2, "column": 3 }],
      "path": ["deviceApplication"],
      "extensions": {
        "code": "NOT_FOUND"
      }
    }
  ],
  "data": {
    "deviceApplication": null
  }
}
```

### **Handling Nullable Returns:**
```graphql
query {
  deviceApplication(publicTechnicalName: "INVALID") {
    publicTechnicalName  # Returns null if not found
    displayText
  }
}
```

---

## 📚 **Complete Scenario Coverage Summary**

| Scenario | Query Method | Example Provided |
|----------|--------------|------------------|
| 1 | `deviceApplications` | ✅ Yes |
| 2 | `deviceApplication(publicTechnicalName)` | ✅ Yes |
| 3 | `functionGroups(devicePublicTechnicalName)` | ✅ Yes |
| 4 | `functionGroup(ptnPath)` | ✅ Yes |
| 5 | `functionBlocks(devicePublicTechnicalName)` | ✅ Yes |
| 6 | `functionBlock(ptnPath)` | ✅ Yes |
| 7 | `signals(functionBlockPtnPath)` | ✅ Yes |
| 8 | `allPtnPaths(devicePublicTechnicalName)` | ✅ Yes |
| 9 | `functions` | ✅ Yes |
| 10 | `function(ptnPath)` | ✅ Yes |
| 11 | `subsignals(signalPtnPath)` | ✅ Yes |
| 12 | `cdcConversions(signalPtnPath)` | ✅ Yes |
| 13 | `routings(ptnPath)` | ✅ Yes |
| 14 | `deviceApplicationsByType(typeName)` | ✅ Yes |
| 15 | `deviceApplicationsByTypes(typeNames)` | ✅ Yes |
| 16 | `deviceTypes` | ✅ Yes |
| **Scenario 5** | `deviceApplications(where: {...})` | ✅ Yes (Filtering) |
| **Scenario 7** | Computed statistics fields | ✅ Yes (12 fields) |

---

## 🚀 **Quick Start**

1. **Start Application:**
   ```powershell
   cd C:\Repo\GraphQL\RestVsGraphQL\RestVsGraphQL
   dotnet run
   ```

2. **Open GraphQL IDE:**
   - Navigate to `https://localhost:7XXX/graphql`

3. **Try First Query:**
   ```graphql
   query FirstQuery {
     deviceApplications {
       publicTechnicalName
       displayText
     }
   }
   ```

4. **Explore Schema:**
   - Click "Schema" tab in Banana Cake Pop
   - Browse all available queries and types

---

## 📖 **Additional Resources**

- **GraphQL Official Docs:** https://graphql.org/learn/
- **HotChocolate Docs:** https://chillicream.com/docs/hotchocolate/
- **Filtering Guide:** https://chillicream.com/docs/hotchocolate/v13/fetching-data/filtering
- **DataLoaders:** https://chillicream.com/docs/hotchocolate/v13/fetching-data/dataloader

---

**Document Version:** 1.0  
**Last Updated:** 2025-01-23  
**Total Scenarios Covered:** 18 (16 numbered + Scenario 5 filtering + Scenario 7 statistics)
