# PowerShell Scripts Reference

This document provides a complete reference for all PowerShell scripts in the project.

## 📁 Script Organization

### Root Directory Scripts

| Script | Purpose | Usage |
|--------|---------|-------|
| **launch-tests.ps1** | Interactive test launcher | `.\\launch-tests.ps1` |
| **start-api.ps1** | Start the API with build check | `.\\start-api.ps1` |

### PerformanceTests Folder Scripts

| Script | Purpose | Usage |
|--------|---------|-------|
| **load-test.ps1** | Main comprehensive load test | `.\\load-test.ps1 -Iterations 100` |
| **load-test-bulk.ps1** | Bulk operations focused test | `.\\load-test-bulk.ps1 -Iterations 50` |
| **quick-test.ps1** | Quick validation (10 iterations) | `.\\quick-test.ps1` |

---

## 🚀 Quick Start Guide

### Option 1: Use the Interactive Launcher (Recommended)

```powershell
.\launch-tests.ps1
```

**Features:**
- Interactive menu with numbered options
- Pre-configured test scenarios
- View metrics directly
- Reset metrics
- No need to remember parameters

**Menu Options:**
1. Quick Test (10 iterations, ~1 minute)
2. Standard Load Test (100 iterations, ~5 minutes)
3. Heavy Load Test (400 iterations, ~15 minutes)
4. Bulk Operations Test (50 iterations)
5. Heavy Bulk Test (200 iterations, 15 orders/bulk)
6. Custom Load Test
7. View Current Metrics
8. Reset Metrics
9. Open Performance Tests Folder
0. Exit

### Option 2: Use Command-Line Arguments

```powershell
# Quick validation
.\launch-tests.ps1 -TestType quick

# Standard load test
.\launch-tests.ps1 -TestType standard

# Heavy load test
.\launch-tests.ps1 -TestType heavy

# Bulk operations
.\launch-tests.ps1 -TestType bulk
```

### Option 3: Run Scripts Directly

```powershell
cd PerformanceTests

# Quick test
.\quick-test.ps1

# Standard test with default 100 iterations
.\load-test.ps1

# Custom iterations
.\load-test.ps1 -Iterations 200

# Bulk operations
.\load-test-bulk.ps1

# Heavy bulk with custom parameters
.\load-test-bulk.ps1 -Iterations 100 -OrdersPerBulk 15
```

---

## 📖 Detailed Script Documentation

### launch-tests.ps1

**Location:** Root directory  
**Purpose:** Interactive menu for launching all performance tests

**Parameters:**
- `-TestType` (string, optional): Test type to run
  - `quick` - Quick test (10 iterations)
  - `standard` - Standard test (100 iterations)
  - `heavy` - Heavy test (400 iterations)
  - `bulk` - Bulk operations test
  - `menu` - Show interactive menu (default)

**Examples:**
```powershell
# Interactive mode
.\launch-tests.ps1

# Direct execution
.\launch-tests.ps1 -TestType quick
.\launch-tests.ps1 -TestType standard
```

**What it does:**
- Provides user-friendly menu interface
- Routes to appropriate test scripts
- Handles parameter passing
- Provides feedback and progress indication

---

### start-api.ps1

**Location:** Root directory  
**Purpose:** Build and start the REST vs GraphQL API

**Parameters:** None

**Examples:**
```powershell
.\start-api.ps1
```

**What it does:**
- Runs `dotnet build` to compile the project
- Checks for build errors
- Starts the API with `dotnet run`
- Displays endpoint information
- Keeps running until Ctrl+C

**Output:**
- REST endpoints at: `http://localhost:5072/api/*`
- GraphQL endpoint at: `http://localhost:5072/graphql`
- Swagger UI at: `http://localhost:5072/swagger`

---

### load-test.ps1

**Location:** PerformanceTests folder  
**Purpose:** Main comprehensive load test covering all scenarios

**Parameters:**
- `-BaseUrl` (string, optional): API base URL (default: `http://localhost:5072`)
- `-Iterations` (int, optional): Number of iterations per test (default: 100)

**Examples:**
```powershell
# Default: 100 iterations
.\load-test.ps1

# Light load
.\load-test.ps1 -Iterations 50

# Medium load
.\load-test.ps1 -Iterations 200

# Heavy load
.\load-test.ps1 -Iterations 400

# Custom API URL
.\load-test.ps1 -BaseUrl http://localhost:8080 -Iterations 100
```

**Test Scenarios Covered:**
1. ✅ Simple GET requests (basic customer queries)
2. ✅ Nested data queries (3-level hierarchy: Order → OrderItem → OrderItemNote)
3. ✅ Dashboard aggregations (complex composite views)
4. ✅ Multiple resources (1 GraphQL call vs 3 REST calls)

**Metrics Collected:**
- Response Time (Average, P50, P95, P99)
- Payload Size (Request & Response)
- Throughput (Requests per second)
- Success Rate (Percentage)
- Memory Usage
- Bandwidth Usage

**Duration:** ~5-10 minutes for 100 iterations

---

### load-test-bulk.ps1

**Location:** PerformanceTests folder  
**Purpose:** Focused testing of bulk create/update operations

**Parameters:**
- `-BaseUrl` (string, optional): API base URL (default: `http://localhost:5072`)
- `-Iterations` (int, optional): Number of bulk operations (default: 50)
- `-OrdersPerBulk` (int, optional): Orders per bulk request (default: 10)

**Examples:**
```powershell
# Default: 50 iterations, 10 orders per bulk
.\load-test-bulk.ps1

# More iterations
.\load-test-bulk.ps1 -Iterations 100

# More orders per request
.\load-test-bulk.ps1 -OrdersPerBulk 15

# Heavy bulk load
.\load-test-bulk.ps1 -Iterations 200 -OrdersPerBulk 20

# Custom API URL
.\load-test-bulk.ps1 -BaseUrl http://localhost:8080
```

**Test Scenarios Covered:**
1. ✅ Bulk CREATE operations (10-15 orders per request)
2. ✅ Bulk UPDATE operations (batch modifications)
3. ✅ Efficiency metrics (orders processed per request)

**Key Metrics:**
- Orders created/updated per request
- Average processing time per order
- Total payload size comparison
- Efficiency ratio (GraphQL vs REST)

**Duration:** ~3-5 minutes for 50 iterations

---

### quick-test.ps1

**Location:** PerformanceTests folder  
**Purpose:** Quick validation test with minimal iterations

**Parameters:** None (hardcoded to 10 iterations)

**Examples:**
```powershell
.\quick-test.ps1
```

**Test Scenarios Covered:**
- All scenarios from `load-test.ps1` but with only 10 iterations

**Use Cases:**
- ✅ First-time validation
- ✅ Quick sanity check after code changes
- ✅ CI/CD pipeline integration
- ✅ Verifying API is running correctly

**Duration:** ~1 minute

---

## 🎯 Common Workflows

### First Time Setup and Testing

```powershell
# 1. Start the API
.\start-api.ps1

# 2. In another terminal, run quick test
cd PerformanceTests
.\quick-test.ps1

# 3. View results
Start-Process http://localhost:5072/api/metrics/report
```

### Comprehensive Performance Analysis

```powershell
# Use the interactive launcher
.\launch-tests.ps1

# Select:
# Option 2: Standard Load Test (100 iterations)
# Option 4: Bulk Operations Test
# Option 7: View Metrics Report
```

### Custom Performance Testing

```powershell
cd PerformanceTests

# Reset metrics
Invoke-RestMethod -Uri http://localhost:5072/api/metrics/reset -Method Post

# Run light load
.\load-test.ps1 -Iterations 50

# Run heavy bulk
.\load-test-bulk.ps1 -Iterations 200 -OrdersPerBulk 15

# View results
Start-Process http://localhost:5072/api/metrics/report
```

### Production Simulation

```powershell
cd PerformanceTests

# Heavy sustained load
.\load-test.ps1 -Iterations 500

# Heavy bulk operations
.\load-test-bulk.ps1 -Iterations 200 -OrdersPerBulk 15

# View metrics
Invoke-RestMethod http://localhost:5072/api/metrics/comparison | ConvertTo-Json -Depth 10
```

---

## 🔧 Troubleshooting

### "Cannot connect to API"

**Problem:** Scripts can't reach the API  
**Solution:**
```powershell
# Make sure API is running
.\start-api.ps1

# Or manually:
cd RestVsGraphQL
dotnet run

# Wait for: "Now listening on: http://localhost:5072"
```

### "Script execution policy error"

**Problem:** PowerShell won't run scripts  
**Solution:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### "Metrics reset failed"

**Problem:** API not fully started yet  
**Solution:** Wait a few seconds after starting the API before running tests

### Scripts not found

**Problem:** Running from wrong directory  
**Solution:**
```powershell
# For launcher
cd C:\Users\z004ev4m\source\repos\RestVsGraphQL
.\launch-tests.ps1

# For direct execution
cd C:\Users\z004ev4m\source\repos\RestVsGraphQL\PerformanceTests
.\load-test.ps1
```

---

## 📊 Understanding Test Results

### Metrics Report

Access the comprehensive HTML report at:
```
http://localhost:5072/api/metrics/report
```

**Includes:**
- Summary statistics (total requests, success rate)
- Response time percentiles (P50, P95, P99)
- Payload size comparison (GraphQL vs REST)
- Interactive charts and graphs
- Detailed breakdowns per scenario

### API Endpoints for Metrics

```powershell
# Full comparison report
Invoke-RestMethod http://localhost:5072/api/metrics/comparison

# Summary only
Invoke-RestMethod http://localhost:5072/api/metrics/summary

# REST metrics only
Invoke-RestMethod http://localhost:5072/api/metrics/rest

# GraphQL metrics only
Invoke-RestMethod http://localhost:5072/api/metrics/graphql

# Reset all metrics
Invoke-RestMethod -Uri http://localhost:5072/api/metrics/reset -Method Post
```

---

## 📚 Related Documentation

- [PerformanceTests/README.md](PerformanceTests/README.md) - Detailed testing guide
- [Documentation/PERFORMANCE_TESTS.md](Documentation/PERFORMANCE_TESTS.md) - Performance testing methodology
- [Documentation/KPI_NFR_GUIDE.md](Documentation/KPI_NFR_GUIDE.md) - KPI & NFR measurement guide
- [Documentation/QUICKSTART.md](Documentation/QUICKSTART.md) - Getting started guide

---

**Need Help?** Check the main [README.md](README.md) for complete project documentation.
