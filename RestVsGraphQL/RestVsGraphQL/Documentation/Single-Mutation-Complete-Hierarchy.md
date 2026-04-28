# Single Mutation - Complete Device Hierarchy Creation

## Scenario 18: Create Complete DeviceApplication in One Call

This guide demonstrates how to create an entire DeviceApplication with all nested hierarchical data using **a single GraphQL mutation**.

---

## ✅ New Mutation: `createDeviceApplicationWithHierarchy`

### Features
- ✅ **Single API call** - Create entire device hierarchy at once
- ✅ **Nested input types** - Declare all relationships inline
- ✅ **Automatic path generation** - PTN paths calculated automatically
- ✅ **Transactional** - All-or-nothing creation (if one fails, none are created)
- ✅ **Field resolvers still work** - Query only what you need after creation

---

## Complete Example: Create IED with Full Hierarchy

```graphql
mutation CreateCompleteDevice {
  createDeviceApplicationWithHierarchy(
    input: {
      publicTechnicalName: "IED_COMPLETE_001"
      displayText: "Complete Protection IED"
      typeName: "IED"
      lastModifiedBy: "admin"
      dddVersion: "1.0.0"
      comDddVersion: "1.0.0"

      # Nested FunctionGroups
      functionGroups: [
        {
          publicTechnicalName: "CTRL"
          displayText: "Control Functions"
          typeName: "FunctionGroup"
          isDeletable: true

          # Nested Functions within CTRL
          functions: [
            {
              publicTechnicalName: "CSWI1"
              displayText: "Switch Controller 1"
              typeName: "Function"
              isDeletable: true

              # Nested FunctionBlocks within CSWI1
              functionBlocks: [
                {
                  publicTechnicalName: "Pos"
                  displayText: "Position"
                  typeName: "FunctionBlock"
                  originalName: "Pos"
                  isDeletable: true

                  # Nested Signals within Pos
                  signals: [
                    {
                      publicTechnicalName: "stVal"
                      displayText: "Position State Value"
                      typeName: "Signal"
                      cdcType: "DPC"
                      type: "Status"
                      isDeletable: true

                      # Nested CdcConversions
                      cdcConversions: [
                        {
                          publicTechnicalName: "DPC_to_SPS"
                          displayText: "Convert DPC to SPS"
                          sourceCdc: "DPC"
                          targetCdc: "SPS"

                          # Routings under CdcConversion
                          routings: [
                            {
                              isReadonly: true
                              value: "Internal_Mapping"
                              options: ["Internal_Mapping"]
                            }
                          ]
                        }
                      ]

                      # Direct Routings under Signal
                      routings: [
                        {
                          isReadonly: false
                          value: "GOOSE_CB1"
                          options: ["GOOSE_CB1", "GOOSE_CB2", "MMS_Direct"]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ]
        },

        {
          publicTechnicalName: "PROT"
          displayText: "Protection Functions"
          typeName: "FunctionGroup"
          isDeletable: true

          functions: [
            {
              publicTechnicalName: "PDIS1"
              displayText: "Distance Protection"
              typeName: "Function"
              isDeletable: true

              functionBlocks: [
                {
                  publicTechnicalName: "Op"
                  displayText: "Operation"
                  typeName: "FunctionBlock"
                  originalName: "Op"
                  isDeletable: true

                  signals: [
                    {
                      publicTechnicalName: "general"
                      displayText: "General Operation"
                      typeName: "Signal"
                      cdcType: "ACT"
                      type: "Status"
                      isDeletable: true

                      routings: [
                        {
                          isReadonly: false
                          value: "GOOSE_TRIP"
                          options: ["GOOSE_TRIP", "GOOSE_ALARM", "Local"]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ]
        },

        {
          publicTechnicalName: "MEAS"
          displayText: "Measurement Functions"
          typeName: "FunctionGroup"
          isDeletable: true

          functions: [
            {
              publicTechnicalName: "MMXU1"
              displayText: "Measurement Unit"
              typeName: "Function"
              isDeletable: true

              functionBlocks: [
                {
                  publicTechnicalName: "TotW"
                  displayText: "Total Watts"
                  typeName: "FunctionBlock"
                  originalName: "TotW"
                  isDeletable: true

                  signals: [
                    {
                      publicTechnicalName: "mag"
                      displayText: "Total Watts Magnitude"
                      typeName: "Signal"
                      cdcType: "MV"
                      type: "Analog"
                      isDeletable: true

                      subsignals: [
                        {
                          publicTechnicalName: "f"
                          displayText: "Frequency"
                          typeName: "Subsignal"
                          cdcType: "AnalogValue"
                          isDeletable: true

                          routings: [
                            {
                              isReadonly: false
                              value: "Modbus_Reg_100"
                              options: ["Modbus_Reg_100", "Modbus_Reg_200", "DNP3_Point_50"]
                            }
                          ]
                        }
                      ]

                      cdcConversions: [
                        {
                          publicTechnicalName: "MV_to_ASG"
                          displayText: "Convert MV to ASG"
                          sourceCdc: "MV"
                          targetCdc: "ASG"
                        }
                      ]
                    }
                  ]
                },

                {
                  publicTechnicalName: "TotVAr"
                  displayText: "Total VARs"
                  typeName: "FunctionBlock"
                  originalName: "TotVAr"
                  isDeletable: true

                  signals: [
                    {
                      publicTechnicalName: "mag"
                      displayText: "Total VARs Magnitude"
                      typeName: "Signal"
                      cdcType: "MV"
                      type: "Analog"
                      isDeletable: true

                      subsignals: [
                        {
                          publicTechnicalName: "f"
                          displayText: "Frequency"
                          typeName: "Subsignal"
                          cdcType: "AnalogValue"
                          isDeletable: true
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  ) {
    publicTechnicalName
    displayText
    typeName
    lastUpdatedAt

    # Query statistics (computed via DataLoaders)
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
  }
}
```

---

## Response Example

```json
{
  "data": {
    "createDeviceApplicationWithHierarchy": {
      "publicTechnicalName": "IED_COMPLETE_001",
      "displayText": "Complete Protection IED",
      "typeName": "IED",
      "lastUpdatedAt": "2024-01-20T15:45:30Z",
      "functionGroupCount": 3,
      "functionBlockCount": 4,
      "functionCount": 3,
      "signalCount": 4,
      "analogSignalCount": 2,
      "statusSignalCount": 2,
      "subsignalCount": 2,
      "cdcConversionCount": 2,
      "routingCount": 5,
      "hasConfiguration": true,
      "configuredRoutingCount": 5,
      "editableRoutingCount": 4
    }
  }
}
```

---

## What Gets Created

The mutation above creates **19 entities** in a single call:

| Entity Type | Count | Examples |
|-------------|-------|----------|
| DeviceApplication | 1 | IED_COMPLETE_001 |
| FunctionGroups | 3 | CTRL, PROT, MEAS |
| Functions | 3 | CSWI1, PDIS1, MMXU1 |
| FunctionBlocks | 4 | Pos, Op, TotW, TotVAr |
| Signals | 4 | stVal, general, mag (2x) |
| Subsignals | 2 | f (frequency subsignals) |
| CdcConversions | 2 | DPC_to_SPS, MV_to_ASG |
| Routings | 5 | Various routing configurations |

**Total: 19 entities** created with proper PTN path hierarchy automatically!

---

## Automatic PTN Path Generation

The mutation **automatically calculates** PTN paths based on nesting:

```
IED_COMPLETE_001                                              (DeviceApplication)
├── IED_COMPLETE_001/CTRL                                     (FunctionGroup)
│   └── IED_COMPLETE_001/CTRL/CSWI1                          (Function)
│       └── IED_COMPLETE_001/CTRL/CSWI1/Pos                  (FunctionBlock)
│           └── IED_COMPLETE_001/CTRL/CSWI1/Pos/stVal        (Signal)
│               ├── IED_COMPLETE_001/CTRL/CSWI1/Pos/stVal/DPC_to_SPS  (CdcConversion)
│               │   └── .../DPC_to_SPS/routing1              (Routing)
│               └── IED_COMPLETE_001/CTRL/CSWI1/Pos/stVal/routing1    (Routing)
├── IED_COMPLETE_001/PROT                                     (FunctionGroup)
│   └── IED_COMPLETE_001/PROT/PDIS1                          (Function)
│       └── IED_COMPLETE_001/PROT/PDIS1/Op                   (FunctionBlock)
│           └── IED_COMPLETE_001/PROT/PDIS1/Op/general       (Signal)
│               └── .../general/routing1                     (Routing)
└── IED_COMPLETE_001/MEAS                                     (FunctionGroup)
    └── IED_COMPLETE_001/MEAS/MMXU1                          (Function)
        ├── IED_COMPLETE_001/MEAS/MMXU1/TotW                 (FunctionBlock)
        │   └── IED_COMPLETE_001/MEAS/MMXU1/TotW/mag         (Signal)
        │       ├── .../TotW/mag/f                           (Subsignal)
        │       │   └── .../f/routing1                       (Routing)
        │       └── .../mag/MV_to_ASG                        (CdcConversion)
        └── IED_COMPLETE_001/MEAS/MMXU1/TotVAr               (FunctionBlock)
            └── IED_COMPLETE_001/MEAS/MMXU1/TotVAr/mag       (Signal)
                └── .../TotVAr/mag/f                         (Subsignal)
```

**No manual path construction needed!** ✅

---

## Query Complete Hierarchy After Creation

After creating the device, query the full structure:

```graphql
query GetCompleteDevice {
  deviceApplication(publicTechnicalName: "IED_COMPLETE_001") {
    publicTechnicalName
    displayText
    typeName

    # Statistics (DataLoaders batch efficiently)
    functionGroupCount
    signalCount
    routingCount

    # Full nested hierarchy (field resolvers load on-demand)
    functionGroups {
      publicTechnicalName
      displayText
      ptnPath

      functions {
        publicTechnicalName
        displayText

        functionBlocks {
          publicTechnicalName
          displayText

          signals {
            publicTechnicalName
            displayText
            type

            subsignals {
              publicTechnicalName
              displayText

              routings {
                ptnPath
                value
                options
              }
            }

            cdcConversions {
              publicTechnicalName
              sourceCdc
              targetCdc

              routings {
                ptnPath
                value
              }
            }

            routings {
              ptnPath
              value
              options
            }
          }
        }
      }
    }
  }
}
```

---

## Minimal Example (Simple Device)

Create a minimal device with just one signal:

```graphql
mutation CreateMinimalDevice {
  createDeviceApplicationWithHierarchy(
    input: {
      publicTechnicalName: "IED_MINIMAL"
      displayText: "Minimal IED"
      typeName: "IED"

      functionGroups: [
        {
          publicTechnicalName: "CTRL"
          displayText: "Control"
          typeName: "FunctionGroup"

          functions: [
            {
              publicTechnicalName: "CSWI1"
              displayText: "Switch"
              typeName: "Function"

              functionBlocks: [
                {
                  publicTechnicalName: "Pos"
                  displayText: "Position"
                  typeName: "FunctionBlock"
                  originalName: "Pos"

                  signals: [
                    {
                      publicTechnicalName: "stVal"
                      displayText: "State Value"
                      typeName: "Signal"
                      cdcType: "DPC"
                      type: "Status"
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  ) {
    publicTechnicalName
    displayText
    functionGroupCount
    signalCount
  }
}
```

**Response:**
```json
{
  "data": {
    "createDeviceApplicationWithHierarchy": {
      "publicTechnicalName": "IED_MINIMAL",
      "displayText": "Minimal IED",
      "functionGroupCount": 1,
      "signalCount": 1
    }
  }
}
```

---

## Comparison: Single vs Multi-Step Approach

### **Single Mutation Approach** (Scenario 18)

✅ **Pros:**
- One API call
- Automatic path generation
- Transactional (all-or-nothing)
- Easier for bulk imports
- Less network overhead

❌ **Cons:**
- Large JSON payload
- Harder to debug errors
- Less flexible (must define entire structure upfront)
- Cannot reuse existing entities

### **Multi-Step Approach** (Original)

✅ **Pros:**
- Incremental building
- Easy to debug (clear error per step)
- Can reuse existing entities
- More flexible
- Smaller individual requests

❌ **Cons:**
- Multiple API calls
- Manual path construction
- Not transactional
- More network round trips

---

## When to Use Which Approach

| Use Case | Recommended Approach |
|----------|---------------------|
| **Bulk data import** (JSON files, migrations) | Single Mutation ✅ |
| **Creating complete device from template** | Single Mutation ✅ |
| **UI-driven creation** (user builds step-by-step) | Multi-Step ✅ |
| **Adding to existing device** | Multi-Step ✅ |
| **Testing/development** | Multi-Step ✅ (easier debugging) |
| **Production automation** | Single Mutation ✅ (atomic) |

---

## Error Handling

If **any part** of the hierarchy fails validation, **nothing is created**:

```graphql
mutation CreateInvalidDevice {
  createDeviceApplicationWithHierarchy(
    input: {
      publicTechnicalName: "IED_BAD"
      displayText: "Bad Device"
      typeName: "IED"

      functionGroups: [
        {
          publicTechnicalName: ""  # ⚠️ INVALID: Empty name
          displayText: "Bad FG"
          typeName: "FunctionGroup"
        }
      ]
    }
  ) {
    publicTechnicalName
  }
}
```

**Error Response:**
```json
{
  "errors": [
    {
      "message": "PublicTechnicalName cannot be empty",
      "path": ["createDeviceApplicationWithHierarchy"]
    }
  ]
}
```

**Result**: No device created, no partial data in database. ✅

---

## Performance Considerations

### **Creation Performance**
- ✅ Single transaction (faster than N separate calls)
- ✅ No network latency between steps
- ✅ Efficient for bulk operations

### **Query Performance After Creation**
- ✅ Field resolvers still work (lazy loading)
- ✅ DataLoaders batch statistical queries
- ✅ Client controls what data to retrieve

### **Memory Usage**
- ⚠️ Large nested input = larger memory footprint
- ⚠️ Consider chunking for very large devices (1000+ entities)

---

## Best Practices

1. ✅ **Use for complete devices**: When you have the full structure ready
2. ✅ **Validate input before mutation**: Check required fields client-side
3. ✅ **Handle errors gracefully**: Show clear error messages to users
4. ✅ **Use variables**: Don't hardcode large JSON in queries
5. ✅ **Test with small examples first**: Verify structure before bulk import

---

## Using GraphQL Variables

For cleaner code, use variables:

```graphql
mutation CreateDevice($input: CreateDeviceApplicationInput!) {
  createDeviceApplicationWithHierarchy(input: $input) {
    publicTechnicalName
    displayText
    functionGroupCount
    signalCount
  }
}
```

**Variables:**
```json
{
  "input": {
    "publicTechnicalName": "IED_VAR_EXAMPLE",
    "displayText": "Variable Example",
    "typeName": "IED",
    "functionGroups": [
      {
        "publicTechnicalName": "CTRL",
        "displayText": "Control",
        "typeName": "FunctionGroup",
        "functions": [...]
      }
    ]
  }
}
```

---

## Summary

✅ **New Mutation**: `createDeviceApplicationWithHierarchy`  
✅ **Single API Call**: Create entire device hierarchy at once  
✅ **Automatic Paths**: PTN paths generated automatically  
✅ **Transactional**: All-or-nothing creation  
✅ **Field Resolvers**: Still work after creation (lazy loading)  
✅ **DataLoaders**: Statistics still batched efficiently  
✅ **19 Entities**: Complete hierarchy in example (customizable)  

**Both approaches are available** - choose based on your use case!
