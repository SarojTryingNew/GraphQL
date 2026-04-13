# Performance Testing - Bulk Operations Categorization

## Updated Menu Structure

The bulk operations have been separated into **three distinct categories** for focused performance testing:

### Main Menu Options:

```
COMPREHENSIVE TESTS:
  1. Quick Test (10 iterations + 100 bulk orders)
  2. Standard Load Test (100 iterations + 500 bulk orders)

INDIVIDUAL SCENARIO TESTS:
  3. Bulk Create Operations Test        ← Category 1 (Green)
  4. Bulk Update Operations Test        ← Category 2 (Yellow)
  5. Bulk Delete Operations Test        ← Category 3 (Red)
  6. Nested Object Graph Test
  7. Dashboard Aggregation Test
  8. Multiple Dependent Calls Test

  0. Exit
```

---

## Category 1: Bulk CREATE Operations (Option 3)

### Test Scripts:
- **File**: `PerformanceTests\load-test-bulk-create.ps1`
- **Color**: Green

### Configuration Options:
| Option | Description | Iterations | Orders/Bulk | Total Orders |
|--------|------------|------------|-------------|--------------|
| 1 | Quick Test | 10 | 10 | 100 |
| 2 | Standard Test | 50 | 10 | 500 |
| 3 | Heavy Load Test | 50 | 20 | 1,000 |
| 4 | Stress Test | 200 | 50 | 10,000 |
| 5 | Custom | User-defined | User-defined | User-defined |

### Actions Performed:
- ✅ Validate customers (rotating IDs 1-5)
- ✅ Validate products (rotating IDs 1-8)
- ✅ Create orders with status "Pending"
- ✅ Create order items (random qty 1-5, discount 0-20%)
- ✅ Create 2 notes per item
- ✅ Calculate order totals

### Test Scenarios:
1. REST: `POST /api/orders/bulk`
2. GraphQL: `mutation { bulkCreateOrders }`

---

## Category 2: Bulk UPDATE Operations (Option 4)

### Test Scripts:
- **File**: `PerformanceTests\load-test-bulk-update.ps1`
- **Color**: Yellow

### Configuration Options:
| Option | Description | Iterations | Orders/Bulk | Total Orders |
|--------|------------|------------|-------------|--------------|
| 1 | Quick Test | 10 | 10 | 100 |
| 2 | Standard Test | 25 | 10 | 250 |
| 3 | Heavy Load Test | 50 | 20 | 1,000 |
| 4 | Custom | User-defined | User-defined | User-defined |

### Actions Performed:
- ✅ Query recent orders
- ✅ Update status (random: Pending/Processing/Shipped/Completed)
- ✅ Recalculate order totals

### Test Scenarios:
1. REST: `PUT /api/orders/bulk`
2. GraphQL: `mutation { bulkUpdateOrders }`

---

## Category 3: Bulk DELETE Operations (Option 5)

### Test Scripts:
- **File**: `PerformanceTests\load-test-bulk-delete.ps1`
- **Color**: Red

### Configuration Options:
| Option | Description | Iterations | Orders/Bulk | Total Orders |
|--------|------------|------------|-------------|--------------|
| 1 | Quick Test | 10 | 10 | 100 |
| 2 | Standard Test | 16 | 10 | 160 |
| 3 | Heavy Load Test | 25 | 20 | 500 |
| 4 | Custom | User-defined | User-defined | User-defined |

### Actions Performed:
- ✅ Query oldest orders
- ✅ **Cascade delete** OrderItemNotes
- ✅ **Cascade delete** OrderItems
- ✅ Delete Orders
- ✅ Track deleted IDs and errors

### Test Scenarios:
1. REST: `DELETE /api/orders/bulk`
2. GraphQL: `mutation { bulkDeleteOrders }`

---

## Complete Performance Testing Matrix

| **Category** | **Menu Option** | **Test Script** | **Focus** | **Default Iterations** | **Default Orders/Bulk** |
|--------------|----------------|-----------------|-----------|----------------------|------------------------|
| **Comprehensive** | 1 | quick-test.ps1 | All scenarios (quick) | 10 | 10 |
| **Comprehensive** | 2 | load-test.ps1 + load-test-bulk.ps1 | All scenarios (full) | 100 | 50 |
| **Bulk CREATE** | 3 | load-test-bulk-create.ps1 | Order creation only | 50 | 10 |
| **Bulk UPDATE** | 4 | load-test-bulk-update.ps1 | Order updates only | 25 | 10 |
| **Bulk DELETE** | 5 | load-test-bulk-delete.ps1 | Order deletion only | 16 | 10 |
| **Nested Graph** | 6 | load-test-nested.ps1 | 4-level nesting | 100 | N/A |
| **Dashboard** | 7 | load-test-dashboard.ps1 | Aggregations | 100 | N/A |
| **Multiple Calls** | 8 | load-test-multiple.ps1 | REST 3× vs GraphQL 1× | 100 | N/A |

---

## Benefits of Categorization

### 1. **Focused Testing**
- Test specific CRUD operations independently
- Isolate performance characteristics of each operation
- Easier to identify bottlenecks

### 2. **Better Metrics**
- Each category generates separate KPI reports
- Clear comparison: REST vs GraphQL per operation type
- Metrics include: Response time, Payload size, Throughput, Efficiency

### 3. **Flexible Load Testing**
- Run only the operations you need to test
- Different iteration counts for different operations
- Avoids data pollution (e.g., deleting test data prematurely)

### 4. **Clear Organization**
- Color-coded menu (Green=Create, Yellow=Update, Red=Delete)
- Logical separation of concerns
- Easy to understand test scenarios

---

## Metrics Collected Per Category

All three categories measure:
- ✅ **Response Time** (Average & P95)
- ✅ **Payload Size** (Request & Response bytes)
- ✅ **Throughput** (Requests/second)
- ✅ **Success Rate** (% successful)
- ✅ **Bulk Efficiency** (Orders processed per request)
- ✅ **Winners** (REST vs GraphQL)
- ✅ **Improvements** (% faster/smaller)

HTML reports available at: `http://localhost:5072/api/metrics/report`
