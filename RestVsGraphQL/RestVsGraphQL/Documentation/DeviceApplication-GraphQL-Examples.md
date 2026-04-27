# DeviceApplication GraphQL Examples

This document provides example GraphQL queries and mutations for the DeviceApplication DTOs.

## Queries

### 1. Get All Device Applications

```graphql
query {
  deviceApplications {
    publicTechnicalName
    displayText
    typeName
    lastUpdatedAt
    lastModifiedBy
    dddVersion
    comDddVersion
    functionGroups {
      publicTechnicalName
      displayText
      typeName
      ptnPath
      isDeletable
    }
    functionBlocks {
      publicTechnicalName
      displayText
      typeName
      originalName
      ptnPath
      isDeletable
    }
  }
}
```

### 2. Get Single Device Application by PublicTechnicalName

```graphql
query {
  deviceApplication(publicTechnicalName: "Device001") {
    publicTechnicalName
    displayText
    typeName
    lastUpdatedAt
    functionGroups {
      publicTechnicalName
      displayText
      functions {
        publicTechnicalName
        displayText
      }
    }
  }
}
```

### 3. Get Function Groups for a Device

```graphql
query {
  functionGroups(devicePublicTechnicalName: "Device001") {
    publicTechnicalName
    displayText
    typeName
    ptnPath
    isDeletable
    functionBlocks {
      publicTechnicalName
      displayText
    }
    functions {
      publicTechnicalName
      displayText
    }
  }
}
```

### 4. Get a Specific Function Group by PTN Path

```graphql
query {
  functionGroup(ptnPath: "Device001/FG001") {
    publicTechnicalName
    displayText
    typeName
    ptnPath
    isDeletable
    functionBlocks {
      publicTechnicalName
      signals {
        publicTechnicalName
        displayText
      }
    }
  }
}
```

### 5. Get Function Blocks for a Device

```graphql
query {
  functionBlocks(devicePublicTechnicalName: "Device001") {
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
      typeName
      cdcType
    }
  }
}
```

### 6. Get Signals for a Function Block

```graphql
query {
  signals(functionBlockPtnPath: "Device001/FB001") {
    publicTechnicalName
    displayText
    type
    typeName
    cdcType
    isDeletable
    ptnPath
    cdcConversions {
      publicTechnicalName
      sourceCdc
      targetCdc
    }
    subSignals {
      publicTechnicalName
      displayText
      typeName
      cdcType
    }
    routings {
      ptnPath
      isReadonly
      value
      options
    }
  }
}
```

## Mutations

### 1. Create Device Application

```graphql
mutation {
  createDeviceApplication(
    deviceApplicationDto: {
      publicTechnicalName: "Device001"
      displayText: "Main Device"
      typeName: "DeviceType1"
      dddVersion: "1.0.0"
      comDddVersion: "1.0.0"
    }
  ) {
    publicTechnicalName
    displayText
    lastUpdatedAt
  }
}
```

### 2. Update Device Application

```graphql
mutation {
  updateDeviceApplication(
    publicTechnicalName: "Device001"
    deviceApplicationDto: {
      publicTechnicalName: "Device001"
      displayText: "Updated Main Device"
      typeName: "DeviceType1"
      dddVersion: "1.1.0"
      comDddVersion: "1.1.0"
      lastModifiedBy: "admin"
    }
  ) {
    publicTechnicalName
    displayText
    lastUpdatedAt
    lastModifiedBy
  }
}
```

### 3. Delete Device Application

```graphql
mutation {
  deleteDeviceApplication(publicTechnicalName: "Device001")
}
```

### 4. Add Function Group to Device

```graphql
mutation {
  addFunctionGroup(
    devicePublicTechnicalName: "Device001"
    functionGroupDto: {
      publicTechnicalName: "FG001"
      displayText: "Function Group 1"
      typeName: "FGType1"
      ptnPath: "Device001/FG001"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }
}
```

### 5. Add Function Block to Device

```graphql
mutation {
  addFunctionBlock(
    devicePublicTechnicalName: "Device001"
    functionBlockDto: {
      publicTechnicalName: "FB001"
      displayText: "Function Block 1"
      typeName: "FBType1"
      originalName: "OriginalFB001"
      ptnPath: "Device001/FB001"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
    originalName
  }
}
```

### 6. Add Signal to Function Block

```graphql
mutation {
  addSignal(
    functionBlockPtnPath: "Device001/FB001"
    signalDto: {
      publicTechnicalName: "Signal001"
      displayText: "Temperature Signal"
      ptnPath: "Device001/FB001/Signal001"
      type: "Analog"
      typeName: "TemperatureType"
      cdcType: "MV"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    type
    cdcType
    ptnPath
  }
}
```

### 7. Complex Mutation - Create Device with Nested Data

```graphql
mutation {
  createDeviceApplication(
    deviceApplicationDto: {
      publicTechnicalName: "Device002"
      displayText: "Complex Device"
      typeName: "AdvancedType"
      dddVersion: "2.0.0"
      comDddVersion: "2.0.0"
      functionGroups: [
        {
          publicTechnicalName: "FG001"
          displayText: "Protection"
          typeName: "ProtectionType"
          ptnPath: "Device002/FG001"
          isDeletable: true
        }
      ]
      functionBlocks: [
        {
          publicTechnicalName: "FB001"
          displayText: "Measurement Block"
          typeName: "MeasurementType"
          originalName: "MeasBlock"
          ptnPath: "Device002/FB001"
          isDeletable: true
          signals: [
            {
              publicTechnicalName: "Voltage"
              displayText: "Voltage Measurement"
              ptnPath: "Device002/FB001/Voltage"
              type: "Analog"
              typeName: "VoltageType"
              cdcType: "MV"
              isDeletable: true
            }
          ]
        }
      ]
    }
  ) {
    publicTechnicalName
    displayText
    functionGroups {
      publicTechnicalName
      displayText
    }
    functionBlocks {
      publicTechnicalName
      signals {
        publicTechnicalName
        displayText
      }
    }
    lastUpdatedAt
  }
}
```

## Notes

- All mutations automatically set the `LastUpdatedAt` timestamp when creating or updating device applications.
- The `PublicTechnicalName` must be unique for device applications.
- PTN (Public Technical Name) paths follow a hierarchical structure: `Device/FunctionGroup/FunctionBlock/Signal`
- Use GraphQL introspection to explore the full schema and available fields.
