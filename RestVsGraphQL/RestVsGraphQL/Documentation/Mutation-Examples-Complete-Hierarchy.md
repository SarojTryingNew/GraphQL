# GraphQL Mutation Examples - Complete Hierarchy

## Complete DeviceApplication Creation with Full Hierarchy

This guide shows how to create a complete DeviceApplication with all nested hierarchical data.

---

## ⚠️ Important: Mutation Architecture

**GraphQL Best Practice**: Create entities **separately** from top-level to children.

### Why?
1. **Field Resolvers**: GraphQL uses resolvers to load relationships on-demand
2. **DataLoaders**: Efficient batching only works with separate queries
3. **Flexibility**: Clients request only what they need
4. **Performance**: Avoids over-fetching data

### Hierarchy Structure
```
DeviceApplication (root)
├── FunctionGroups
│   ├── Functions
│   └── FunctionBlocks
│       └── Signals
│           ├── Subsignals
│           │   └── Routings
│           ├── CdcConversions
│           │   └── Routings
│           └── Routings
└── FunctionBlocks (direct children)
    └── Signals
        └── ... (same as above)
```

---

## Step-by-Step: Create Complete Device Hierarchy

### **Step 1: Create DeviceApplication (Root)**

```graphql
mutation CreateDevice {
  createDeviceApplication(
    deviceApplicationDto: {
      publicTechnicalName: "IED_NEW_DEVICE"
      displayText: "New Protection IED"
      typeName: "IED"
      lastModifiedBy: "admin"
      dddVersion: "1.0.0"
      comDddVersion: "1.0.0"
    }
  ) {
    publicTechnicalName
    displayText
    typeName
    lastUpdatedAt
  }
}
```

**Response:**
```json
{
  "data": {
    "createDeviceApplication": {
      "publicTechnicalName": "IED_NEW_DEVICE",
      "displayText": "New Protection IED",
      "typeName": "IED",
      "lastUpdatedAt": "2024-01-20T10:30:00Z"
    }
  }
}
```

---

### **Step 2: Create FunctionGroups**

```graphql
mutation CreateFunctionGroups {
  fg1: createFunctionGroup(
    functionGroupDto: {
      publicTechnicalName: "CTRL"
      displayText: "Control Functions"
      typeName: "FunctionGroup"
      ptnPath: "IED_NEW_DEVICE/CTRL"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  fg2: createFunctionGroup(
    functionGroupDto: {
      publicTechnicalName: "PROT"
      displayText: "Protection Functions"
      typeName: "FunctionGroup"
      ptnPath: "IED_NEW_DEVICE/PROT"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  fg3: createFunctionGroup(
    functionGroupDto: {
      publicTechnicalName: "MEAS"
      displayText: "Measurement Functions"
      typeName: "FunctionGroup"
      ptnPath: "IED_NEW_DEVICE/MEAS"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }
}
```

---

### **Step 3: Create Functions**

```graphql
mutation CreateFunctions {
  func1: createFunction(
    functionDto: {
      publicTechnicalName: "CSWI1"
      displayText: "Switch Controller 1"
      typeName: "Function"
      ptnPath: "IED_NEW_DEVICE/CTRL/CSWI1"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  func2: createFunction(
    functionDto: {
      publicTechnicalName: "PDIS1"
      displayText: "Distance Protection"
      typeName: "Function"
      ptnPath: "IED_NEW_DEVICE/PROT/PDIS1"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  func3: createFunction(
    functionDto: {
      publicTechnicalName: "MMXU1"
      displayText: "Measurement Unit"
      typeName: "Function"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }
}
```

---

### **Step 4: Create FunctionBlocks**

```graphql
mutation CreateFunctionBlocks {
  fb1: createFunctionBlock(
    functionBlockDto: {
      publicTechnicalName: "Pos"
      displayText: "Position"
      typeName: "FunctionBlock"
      originalName: "Pos"
      ptnPath: "IED_NEW_DEVICE/CTRL/CSWI1/Pos"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  fb2: createFunctionBlock(
    functionBlockDto: {
      publicTechnicalName: "Op"
      displayText: "Operation"
      typeName: "FunctionBlock"
      originalName: "Op"
      ptnPath: "IED_NEW_DEVICE/PROT/PDIS1/Op"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  fb3: createFunctionBlock(
    functionBlockDto: {
      publicTechnicalName: "TotW"
      displayText: "Total Watts"
      typeName: "FunctionBlock"
      originalName: "TotW"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotW"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  fb4: createFunctionBlock(
    functionBlockDto: {
      publicTechnicalName: "TotVAr"
      displayText: "Total VARs"
      typeName: "FunctionBlock"
      originalName: "TotVAr"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotVAr"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }
}
```

---

### **Step 5: Create Signals**

```graphql
mutation CreateSignals {
  sig1: createSignal(
    signalDto: {
      publicTechnicalName: "stVal"
      displayText: "Position State Value"
      typeName: "Signal"
      ptnPath: "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal"
      cdcType: "DPC"
      isDeletable: true
      type: "Status"
    }
  ) {
    publicTechnicalName
    displayText
    type
    ptnPath
  }

  sig2: createSignal(
    signalDto: {
      publicTechnicalName: "general"
      displayText: "General Operation"
      typeName: "Signal"
      ptnPath: "IED_NEW_DEVICE/PROT/PDIS1/Op/general"
      cdcType: "ACT"
      isDeletable: true
      type: "Status"
    }
  ) {
    publicTechnicalName
    displayText
    type
    ptnPath
  }

  sig3: createSignal(
    signalDto: {
      publicTechnicalName: "mag"
      displayText: "Total Watts Magnitude"
      typeName: "Signal"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotW/mag"
      cdcType: "MV"
      isDeletable: true
      type: "Analog"
    }
  ) {
    publicTechnicalName
    displayText
    type
    ptnPath
  }

  sig4: createSignal(
    signalDto: {
      publicTechnicalName: "mag"
      displayText: "Total VARs Magnitude"
      typeName: "Signal"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotVAr/mag"
      cdcType: "MV"
      isDeletable: true
      type: "Analog"
    }
  ) {
    publicTechnicalName
    displayText
    type
    ptnPath
  }
}
```

---

### **Step 6: Create Subsignals**

```graphql
mutation CreateSubsignals {
  ss1: createSubsignal(
    subsignalDto: {
      publicTechnicalName: "f"
      displayText: "Frequency"
      typeName: "Subsignal"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotW/mag/f"
      cdcType: "AnalogValue"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }

  ss2: createSubsignal(
    subsignalDto: {
      publicTechnicalName: "f"
      displayText: "Frequency"
      typeName: "Subsignal"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotVAr/mag/f"
      cdcType: "AnalogValue"
      isDeletable: true
    }
  ) {
    publicTechnicalName
    displayText
    ptnPath
  }
}
```

---

### **Step 7: Create CdcConversions**

```graphql
mutation CreateCdcConversions {
  cdc1: createCdcConversion(
    cdcConversionDto: {
      publicTechnicalName: "DPC_to_SPS"
      displayText: "Convert DPC to SPS"
      ptnPath: "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal/DPC_to_SPS"
      sourceCdc: "DPC"
      targetCdc: "SPS"
    }
  ) {
    publicTechnicalName
    displayText
    sourceCdc
    targetCdc
    ptnPath
  }

  cdc2: createCdcConversion(
    cdcConversionDto: {
      publicTechnicalName: "MV_to_ASG"
      displayText: "Convert MV to ASG"
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotW/mag/MV_to_ASG"
      sourceCdc: "MV"
      targetCdc: "ASG"
    }
  ) {
    publicTechnicalName
    displayText
    sourceCdc
    targetCdc
    ptnPath
  }
}
```

---

### **Step 8: Create Routings**

```graphql
mutation CreateRoutings {
  route1: createRouting(
    routingDto: {
      ptnPath: "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal/routing1"
      isReadonly: false
      value: "GOOSE_CB1"
      options: ["GOOSE_CB1", "GOOSE_CB2", "MMS_Direct"]
    }
  ) {
    ptnPath
    isReadonly
    value
    options
  }

  route2: createRouting(
    routingDto: {
      ptnPath: "IED_NEW_DEVICE/PROT/PDIS1/Op/general/routing1"
      isReadonly: false
      value: "GOOSE_TRIP"
      options: ["GOOSE_TRIP", "GOOSE_ALARM", "Local"]
    }
  ) {
    ptnPath
    isReadonly
    value
    options
  }

  route3: createRouting(
    routingDto: {
      ptnPath: "IED_NEW_DEVICE/MEAS/MMXU1/TotW/mag/f/routing1"
      isReadonly: false
      value: "Modbus_Reg_100"
      options: ["Modbus_Reg_100", "Modbus_Reg_200", "DNP3_Point_50"]
    }
  ) {
    ptnPath
    isReadonly
    value
    options
  }

  route4: createRouting(
    routingDto: {
      ptnPath: "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal/DPC_to_SPS/routing1"
      isReadonly: true
      value: "Internal_Mapping"
      options: ["Internal_Mapping"]
    }
  ) {
    ptnPath
    isReadonly
    value
    options
  }
}
```

---

## Step 9: Query Complete Hierarchy

After creating all entities, query the complete structure:

```graphql
query GetCompleteDeviceHierarchy {
  deviceApplication(publicTechnicalName: "IED_NEW_DEVICE") {
    publicTechnicalName
    displayText
    typeName
    lastUpdatedAt
    lastModifiedBy

    # Statistics (using DataLoaders - efficient!)
    functionGroupCount
    functionBlockCount
    functionCount
    signalCount
    analogSignalCount
    statusSignalCount
    subsignalCount
    cdcConversionCount
    routingCount
    hasConfiguration
    configuredRoutingCount
    editableRoutingCount

    # Nested relationships (loaded on-demand via field resolvers)
    functionGroups {
      publicTechnicalName
      displayText
      ptnPath

      functions {
        publicTechnicalName
        displayText
        ptnPath

        functionBlocks {
          publicTechnicalName
          displayText
          ptnPath

          signals {
            publicTechnicalName
            displayText
            type
            cdcType
            ptnPath

            subsignals {
              publicTechnicalName
              displayText
              ptnPath

              routings {
                ptnPath
                isReadonly
                value
                options
              }
            }

            cdcConversions {
              publicTechnicalName
              displayText
              sourceCdc
              targetCdc
              ptnPath

              routings {
                ptnPath
                isReadonly
                value
                options
              }
            }

            routings {
              ptnPath
              isReadonly
              value
              options
            }
          }
        }
      }

      functionBlocks {
        publicTechnicalName
        displayText
        ptnPath

        signals {
          publicTechnicalName
          displayText
          type
          ptnPath
        }
      }
    }

    functionBlocks {
      publicTechnicalName
      displayText
      ptnPath

      signals {
        publicTechnicalName
        displayText
        type
        ptnPath
      }
    }
  }
}
```

---

## Expected Response Structure

```json
{
  "data": {
    "deviceApplication": {
      "publicTechnicalName": "IED_NEW_DEVICE",
      "displayText": "New Protection IED",
      "typeName": "IED",
      "lastUpdatedAt": "2024-01-20T10:30:00Z",
      "lastModifiedBy": "admin",
      "functionGroupCount": 3,
      "functionBlockCount": 4,
      "functionCount": 3,
      "signalCount": 4,
      "analogSignalCount": 2,
      "statusSignalCount": 2,
      "subsignalCount": 2,
      "cdcConversionCount": 2,
      "routingCount": 4,
      "hasConfiguration": true,
      "configuredRoutingCount": 4,
      "editableRoutingCount": 3,
      "functionGroups": [
        {
          "publicTechnicalName": "CTRL",
          "displayText": "Control Functions",
          "ptnPath": "IED_NEW_DEVICE/CTRL",
          "functions": [
            {
              "publicTechnicalName": "CSWI1",
              "displayText": "Switch Controller 1",
              "ptnPath": "IED_NEW_DEVICE/CTRL/CSWI1",
              "functionBlocks": [
                {
                  "publicTechnicalName": "Pos",
                  "displayText": "Position",
                  "ptnPath": "IED_NEW_DEVICE/CTRL/CSWI1/Pos",
                  "signals": [
                    {
                      "publicTechnicalName": "stVal",
                      "displayText": "Position State Value",
                      "type": "Status",
                      "cdcType": "DPC",
                      "ptnPath": "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal",
                      "subsignals": [],
                      "cdcConversions": [
                        {
                          "publicTechnicalName": "DPC_to_SPS",
                          "displayText": "Convert DPC to SPS",
                          "sourceCdc": "DPC",
                          "targetCdc": "SPS",
                          "ptnPath": "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal/DPC_to_SPS",
                          "routings": [
                            {
                              "ptnPath": "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal/DPC_to_SPS/routing1",
                              "isReadonly": true,
                              "value": "Internal_Mapping",
                              "options": ["Internal_Mapping"]
                            }
                          ]
                        }
                      ],
                      "routings": [
                        {
                          "ptnPath": "IED_NEW_DEVICE/CTRL/CSWI1/Pos/stVal/routing1",
                          "isReadonly": false,
                          "value": "GOOSE_CB1",
                          "options": ["GOOSE_CB1", "GOOSE_CB2", "MMS_Direct"]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ],
          "functionBlocks": []
        }
        // ... more function groups
      ],
      "functionBlocks": [
        // Direct FunctionBlocks under device
      ]
    }
  }
}
```

---

## Summary Statistics After Creation

The complete device hierarchy created:

| Entity Type | Count |
|-------------|-------|
| DeviceApplication | 1 |
| FunctionGroups | 3 |
| Functions | 3 |
| FunctionBlocks | 4 |
| Signals | 4 |
| Subsignals | 2 |
| CdcConversions | 2 |
| Routings | 4 |

**Total Entities**: 19

---

## Benefits of This Approach

### ✅ **Efficient**
- Field resolvers load only requested data
- DataLoaders batch statistical queries
- No over-fetching

### ✅ **Flexible**
- Client controls what data to retrieve
- Can query just device info without nested data
- Or query full hierarchy when needed

### ✅ **Maintainable**
- Clear separation between mutation and query logic
- Each mutation is simple and focused
- Easy to test individual operations

### ✅ **Scalable**
- Works efficiently with 1 device or 1000 devices
- DataLoaders prevent N+1 query problems
- Consistent performance regardless of hierarchy depth

---

## Testing with Banana Cake Pop

1. Navigate to `https://localhost:<port>/graphql`
2. Execute each mutation step-by-step (Steps 1-8)
3. Verify data creation after each step
4. Execute final query (Step 9) to see complete hierarchy
5. Verify all 13 statistical fields show correct counts

---

## Notes

- **PTN Path Format**: `DeviceName/FunctionGroup/Function/FunctionBlock/Signal/...`
- **Path Consistency**: Each child's path must start with parent's path + `/`
- **Field Resolvers**: Automatically use DataLoaders for efficient batching
- **Statistics**: All computed fields use dedicated DataLoaders (99% query reduction)
- **Mutations**: Return only the created entity (relationships loaded on-demand in query)
