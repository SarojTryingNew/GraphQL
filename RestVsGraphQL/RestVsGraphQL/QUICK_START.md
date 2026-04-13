# 🚀 QUICK START GUIDE

## Start Testing in 3 Steps!

### Step 1: Start the Backend API
```powershell
cd C:\Repo\GraphQL\RestVsGraphQL
dotnet run
```

Wait for: `Now listening on: http://localhost:5072`

---

### Step 2: Run Performance Tests
Open **new PowerShell terminal**:

```powershell
cd C:\Repo\GraphQL\RestVsGraphQL
.\launch-tests.ps1
```

**Recommended Test Options:**
- Press `3` - Bulk CREATE Operations (Quick: 10 iterations)
- Press `4` - Bulk UPDATE Operations  
- Press `5` - Bulk DELETE Operations

Each test will:
1. ✅ Run REST Direct API calls
2. ✅ Run REST+GraphQL Backend API calls
3. ✅ Show comparison metrics
4. ✅ Auto-open metrics report in browser

---

### Step 3: View Results

The browser will automatically open to:
**http://localhost:5072/api/metrics/report**

You'll see:
- 🏆 **Winner** - Which approach performed better
- 📊 **KPI Comparison** - Response time, payload size, throughput
- 📈 **Detailed Metrics** - Complete performance breakdown
- ✅ **Success Rates** - Reliability comparison

---

## Optional: Test with Angular Frontend

### Start Angular App
Open **new PowerShell terminal**:

```powershell
cd C:\Repo\GraphQL\angular-client
npm install  # Only first time
ng serve
```

### Open Browser
Navigate to: **http://localhost:4200**

### Run Comparison
1. Click any operation card (Create / Update / Delete / Get)
2. Set number of items
3. Click "🚀 Run Comparison"
4. See side-by-side results!

---

## What You'll See

### PowerShell Output Example:
```
================================================================
   Bulk CREATE Operations - REST Direct vs REST+GraphQL
================================================================

Testing: REST Direct - Bulk Create (10 orders per request)
  Completed: 10 successful bulk requests
  Orders created: 100

Testing: REST with GraphQL - Bulk Create (10 orders per request)
  Completed: 10 successful bulk requests
  Orders created: 100

================================================================
     BULK CREATE - KPI COMPARISON RESULTS
     REST Direct vs REST with GraphQL Backend
================================================================

WINNERS:
  Response Time:  RESTDirect
  Payload Size:   Tie
  Throughput:     RESTDirect

IMPROVEMENTS:
  Response Time:  12.50% faster
  Payload Size:   0.00% smaller

REST Direct API (DataStore) - BULK CREATE:
  Total Requests:     10
  Success Rate:       100.00%
  Avg Response Time:  45.20 ms
  P95 Response Time:  52.10 ms
  Avg Payload:        2341 bytes
  Throughput:         221.24 req/s

REST with GraphQL Backend - BULK CREATE:
  Total Requests:     10
  Success Rate:       100.00%
  Avg Response Time:  51.67 ms
  P95 Response Time:  58.30 ms
  Avg Payload:        2341 bytes
  Throughput:         193.54 req/s
```

### Angular UI Example:
```
🏆 Winner: REST Direct
REST Direct was 12% faster (45 ms vs 52 ms)

📊 Performance Metrics: REST Direct vs REST+GraphQL
┌─────────────────┬──────────────────┬────────────────────┬──────────────┐
│ Metric          │ REST Direct      │ REST + GraphQL     │ Winner       │
├─────────────────┼──────────────────┼────────────────────┼──────────────┤
│ Response Time   │ 45 ms            │ 52 ms              │ REST Direct✓ │
│ Request Size    │ 856 B            │ 856 B              │ Tie          │
│ Response Size   │ 2.3 KB           │ 2.3 KB             │ Tie          │
│ Items Processed │ 10               │ 10                 │ Tie          │
└─────────────────┴──────────────────┴────────────────────┴──────────────┘
```

---

## Tips for Best Results

✅ **Run CREATE first** - This generates order IDs for UPDATE/DELETE tests  
✅ **Reset metrics** - Each test automatically resets metrics before running  
✅ **Multiple iterations** - Run tests multiple times for average results  
✅ **Check HTML report** - More detailed metrics and trends

---

## Quick Commands Reference

### Reset Metrics
```powershell
curl -Method POST http://localhost:5072/api/metrics/reset
```

### View Metrics (JSON)
```powershell
curl http://localhost:5072/api/metrics/comparison
```

### View Metrics (HTML)
```
http://localhost:5072/api/metrics/report
```

---

## What to Expect

**REST Direct** should generally:
- ✅ Be faster (no GraphQL overhead)
- ✅ Use less memory
- ✅ Have higher throughput

**REST+GraphQL** will be:
- ⏱️ Slightly slower (GraphQL execution overhead)
- 📊 Same payload size (same data)
- 🏗️ More flexible architecture

**Difference**: Typically **10-20% slower** due to GraphQL abstraction layer

---

## Need Help?

- **Build errors?** Run: `dotnet clean; dotnet build`
- **Port in use?** Change port in `appsettings.json`
- **Angular errors?** Delete `node_modules`, run `npm install`

---

## Full Documentation

See `IMPLEMENTATION_COMPLETE.md` for complete details!

**Happy Testing! 🎉**
