# Getting Started Guide

**Quick start guide to set up and run the REST vs GraphQL Performance Comparison project.**

---

## 📦 Prerequisites

- **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PowerShell 7+** - For running test scripts
- **Visual Studio 2022+** or **VS Code** - Optional but recommended

---

## 🚀 Quick Start (3 Steps)

### Step 1: Start the API

```powershell
# Navigate to project root
cd C:\Repo\GraphQL\RestVsGraphQL

# Start the API server
.\start-api.ps1
```

**What this does:**
- Builds the .NET project
- Starts the API on `http://localhost:5072`
- Opens your browser automatically

**Available Endpoints:**
- `http://localhost:5072/` - **Interactive front-end demo** ⭐
- `http://localhost:5072/graphql` - GraphQL IDE (Banana Cake Pop)
- `http://localhost:5072/swagger` - REST API documentation
- `http://localhost:5072/api/metrics/report` - Performance report

### Step 2: Run Performance Tests

```powershell
# Interactive test launcher
.\launch-tests.ps1
```

**Test Options:**
1. Quick Test (10 iterations, ~30 seconds)
2. Standard Load Test (100 iterations, ~5 minutes)
3. Bulk Operations (presets: Quick/Standard/Heavy/Stress/Custom)
4. Individual Scenarios (Nested/Dashboard/Multiple Calls)
5. All Tests (runs everything, ~15-20 minutes)
6. Exit

### Step 3: View Results

After tests complete, the browser automatically opens the performance report at:
```
http://localhost:5072/api/metrics/report
```

**Report Sections:**
- Executive Summary (winner comparison)
- Detailed KPI metrics
- Request/Response examples
- Performance charts

---

## 🎨 Try the Interactive Demo

**Best for:** Live presentations, stakeholder demos, learning GraphQL

```powershell
# 1. Start API (if not already running)
.\start-api.ps1

# 2. Open browser to:
http://localhost:5072/
```

**What you can do:**
- ✅ Side-by-side REST vs GraphQL comparison
- ✅ Bulk create orders (configurable count)
- ✅ Bulk delete orders (comma-separated IDs)
- ✅ Execute custom GraphQL queries
- ✅ Try pre-built query templates
- ✅ See real-time performance metrics

**Pre-built Query Templates:**
- **Simple**: List customers
- **Nested**: 4-level object graph
- **Dashboard**: Aggregations with top products
- **Multiple**: Query multiple resources in one request

---

## 📊 Understanding Test Results

### Executive Summary
Shows which API performed better for each metric:
- Response Time (lower is better)
- Throughput (higher is better)
- HTTP Calls (lower is better)
- Payload Size (lower is better)

### Key Metrics

| Metric | What It Means | GraphQL Advantage |
|--------|---------------|-------------------|
| **Response Time** | How fast the API responds | 60-80% faster for complex queries |
| **HTTP Calls** | Number of requests needed | 67-75% reduction for multi-resource queries |
| **Payload Size** | Amount of data transferred | 90-96% smaller (no over-fetching) |
| **Throughput** | Requests per second | Higher for bulk operations |

### Performance Highlights

**Scenario (a) - Bulk Operations:**
- Both REST and GraphQL: **1 request** (equal)
- Best for creating/updating/deleting multiple records

**Scenario (b) - Nested Object Graphs:**
- REST: **4 requests** (Order → Customer → Product → Category)
- GraphQL: **1 request** (all data at once)
- **75% reduction** in HTTP calls

**Scenario (c) - Dashboard Aggregation:**
- REST: **4 requests** (separate endpoints)
- GraphQL: **1 request** (combined query)
- **75% reduction** in HTTP calls

**Scenario (d) - Multiple Dependent Calls:**
- REST: **3 requests** (customers, orders, products)
- GraphQL: **1 request** (combined query)
- **67% reduction** in HTTP calls

---

## 🛠️ Project Structure

```
RestVsGraphQL/
│
├── README.md                      # Main project overview
├── start-api.ps1                  # Quick API startup script
├── launch-tests.ps1               # Interactive test menu
│
├── Documentation/                 # 📚 All documentation
│   ├── GETTING_STARTED.md         # This file - quick start guide
│   ├── USER_GUIDE.md              # Complete usage guide
│   └── DEVELOPER_GUIDE.md         # Technical reference
│
├── PerformanceTests/              # 🧪 Test scripts
│   ├── quick-test.ps1
│   ├── load-test.ps1
│   ├── load-test-bulk.ps1
│   ├── load-test-nested.ps1
│   ├── load-test-dashboard.ps1
│   └── load-test-multiple.ps1
│
└── RestVsGraphQL/                 # 🎯 Main application
    ├── wwwroot/index.html         # Front-end demo
    ├── Controllers/               # REST endpoints
    ├── GraphQL/                   # GraphQL schema
    └── Metrics/                   # Performance tracking
```

---

## 🎯 Common Use Cases

### 1. Quick Validation
**Goal:** Verify everything works
```powershell
.\start-api.ps1
.\PerformanceTests\quick-test.ps1
```

### 2. Demo for Stakeholders
**Goal:** Show GraphQL benefits visually
```powershell
.\start-api.ps1
# Open http://localhost:5072/ in browser
# Try bulk create/delete side-by-side
```

### 3. Performance Benchmarking
**Goal:** Get detailed metrics
```powershell
.\start-api.ps1
.\launch-tests.ps1
# Choose option 2 (Standard Load Test)
```

### 4. Explore GraphQL Schema
**Goal:** Learn GraphQL syntax
```powershell
.\start-api.ps1
# Open http://localhost:5072/graphql
# Use Banana Cake Pop IDE
```

---

## 🐛 Troubleshooting

### API Won't Start

**Problem:** Port 5072 already in use
```powershell
# Find process using port
netstat -ano | findstr :5072

# Kill process
taskkill /PID <PID> /F
```

**Problem:** .NET SDK not found
```powershell
# Check .NET version
dotnet --version

# Should show 9.0.x or higher
# If not, download from: https://dotnet.microsoft.com/download/dotnet/9.0
```

### Tests Failing

**Problem:** API not running
```powershell
# Ensure API is running first
.\start-api.ps1

# Then run tests in new terminal
.\launch-tests.ps1
```

**Problem:** YAML parsing errors
```powershell
# Ensure YamlDotNet package is installed
dotnet restore RestVsGraphQL\RestVsGraphQL.csproj
```

### Front-End Demo Issues

**Problem:** Page not loading
```powershell
# Verify API is running
curl http://localhost:5072/

# Check browser console for errors
```

**Problem:** CORS errors
- Already configured, restart API if needed

**Problem:** Bulk create failures
- Demo uses customer IDs 1-5 and product IDs 1-8
- These are pre-seeded in the DataStore

---

## 📚 Next Steps

Once you're comfortable with the basics:

1. **Read the User Guide** - Learn all features in detail
   - `Documentation/USER_GUIDE.md`

2. **Explore GraphQL Schema** - Understand available queries/mutations
   - Open http://localhost:5072/graphql
   - View schema documentation in IDE

3. **Run Custom Tests** - Create your own test scenarios
   - Copy existing test scripts
   - Modify iterations and parameters

4. **Review Architecture** - Understand implementation
   - `Documentation/DEVELOPER_GUIDE.md`

---

## ✅ Success Checklist

After following this guide, you should be able to:

- [ ] Start the API with `.\start-api.ps1`
- [ ] Access the front-end demo at `http://localhost:5072/`
- [ ] Run quick tests with `.\PerformanceTests\quick-test.ps1`
- [ ] View performance reports at `/api/metrics/report`
- [ ] Execute GraphQL queries in Banana Cake Pop
- [ ] Understand the 4 core scenarios (a, b, c, d)
- [ ] Interpret test results and metrics

---

## 🆘 Getting Help

- **Documentation:** Check `USER_GUIDE.md` for detailed usage
- **Technical Details:** See `DEVELOPER_GUIDE.md` for architecture
- **Issues:** Review troubleshooting section above
- **Questions:** Contact project maintainers

---

**Last Updated:** December 2024  
**Version:** 2.0 (Consolidated)
