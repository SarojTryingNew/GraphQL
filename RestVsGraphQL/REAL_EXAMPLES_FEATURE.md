# ✅ Real Request/Response Data in Reports - DONE!

## 🎯 What Changed

Instead of showing **hardcoded examples**, the metrics report now shows **actual requests and responses** captured from your tests!

---

## 📊 How It Works

### 1. **Middleware Captures Samples**
- When you make API calls, the middleware now captures:
  - Request body (for POST requests)
  - Response body
- Only captures for specific endpoints (to avoid memory overhead)
- Limits to 5000 characters per sample

### 2. **Samples Are Stored**
- First successful request to each endpoint is saved
- Stored in `MetricsCollector`
- Available via `GetSampleRequestsResponses()` method

### 3. **Report Shows Real Data**
- Report retrieves actual samples
- Formats them nicely (pretty JSON)
- Displays side-by-side REST vs GraphQL

---

## 🎨 What You'll See

### Before Running Tests:
```
┌──────────────────────────────────────────────────────┐
│  Request & Response Examples                         │
├──────────────────────────────────────────────────────┤
│                                                      │
│  No sample data available yet.                      │
│                                                      │
│  Sample requests and responses will appear here     │
│  after you run some tests.                          │
│                                                      │
└──────────────────────────────────────────────────────┘
```

### After Running Tests:
```
┌──────────────────────────────────────────────────────┐
│  Simple Customer Retrieval                           │
├─────────────────────────┬────────────────────────────┤
│  🔴 REST API            │  🟣 GraphQL API            │
├─────────────────────────┼────────────────────────────┤
│  Request:               │  Request:                  │
│  GET /api/customers/1   │  POST /graphql             │
│                         │  {                         │
│  [YOUR ACTUAL REQUEST]  │    "query": "..."          │
│                         │  }                         │
│                         │  [YOUR ACTUAL REQUEST]     │
├─────────────────────────┼────────────────────────────┤
│  Response:              │  Response:                 │
│  {                      │  {                         │
│    "id": 1,             │    "data": {               │
│    "name": "John Doe"   │      "customer": {         │
│    ...                  │        "id": 1,            │
│  }                      │        ...                 │
│  [YOUR ACTUAL DATA]     │      }                     │
│                         │    }                       │
│                         │  }                         │
│                         │  [YOUR ACTUAL DATA]        │
└─────────────────────────┴────────────────────────────┘
```

**Shows REAL data from YOUR tests!** 🎯

---

## 🔧 Technical Changes

### Files Modified:

#### 1. **`MetricsCollector.cs`**
- Added `RequestBody` and `ResponseBody` fields to `ApiMetric`
- Added `GetSampleRequestsResponses()` method
- Returns dictionary of captured samples

#### 2. **`MetricsMiddleware.cs`**
- Added `ShouldCaptureSample()` method
- Added `ReadRequestBodyAsync()` method
- Captures request/response for sample endpoints
- Passes capture flag to ResponseCapturingStream

#### 3. **`ResponseCapturingStream.cs`**
- Added optional response capture capability
- New `CapturedResponse` property
- Buffers response only when capturing samples
- No performance impact for non-sample requests

#### 4. **`MetricsController.cs`**
- **Removed** all hardcoded examples
- **Added** `AddActualScenarioExample()` method
- **Added** `FormatActualRequest()` method
- **Added** `FormatActualResponse()` method
- **Added** `TryFormatJson()` method
- **Added** scenario grouping logic

---

## 📍 Endpoints That Capture Samples

Sample data is captured for these endpoints (to avoid memory overhead):

- ✅ `/api/customers/1`
- ✅ `/api/orders/1/nested`
- ✅ `/api/dashboard`
- ✅ `/api/products`
- ✅ `/api/customers/1/orders`
- ✅ `/api/orders/bulk`
- ✅ `/graphql` (all GraphQL queries)

**Why limited?** To avoid memory overhead - only first request to each endpoint is captured.

---

## 🎯 How to Use

### Step 1: Run Your Tests
```powershell
# Run quick test
.\launch-tests.ps1
# Choose: 1 (Quick Test)

# Or manually make API calls
curl http://localhost:5072/api/customers/1
curl http://localhost:5072/api/orders/1/nested
curl -X POST http://localhost:5072/graphql -d '{"query":"..."}'
```

### Step 2: View Report
```
http://localhost:5072/api/metrics/report
```

### Step 3: See Real Examples
- Scroll to "Request & Response Examples" section
- See your actual API calls and responses
- Compare REST vs GraphQL side-by-side

---

## 💡 Benefits

### 1. **Real Data**
- Not fake examples
- Shows what your API actually returns
- Current data from your database

### 2. **Learning Tool**
- See exact request format
- Understand response structure
- Copy examples to test manually

### 3. **Documentation**
- Automatic API documentation
- Always up-to-date
- Real examples for team

### 4. **Debugging**
- See what was actually sent/received
- Verify data structure
- Troubleshoot issues

---

## 📋 Example Scenarios Shown

The report groups samples into scenarios:

### Scenario 1: Simple Customer Retrieval
- REST: `GET /api/customers/1`
- GraphQL: Query for customer

### Scenario 2: Order with Nested Data
- REST: `GET /api/orders/1/nested`
- GraphQL: Nested order query

### Scenario 3: Dashboard Aggregation
- REST: `GET /api/dashboard`
- GraphQL: Dashboard query

### Scenario 4: Multiple Resources
- REST: `GET /api/customers/1`, `/orders`, `/products`
- GraphQL: Combined query

### Scenario 5: Bulk Operations
- REST: `POST /api/orders/bulk`
- GraphQL: Bulk mutation

---

## ⚙️ Performance Impact

### Minimal Overhead:
- ✅ Samples captured only for specific endpoints
- ✅ Only first request per endpoint stored
- ✅ Response truncated to 5000 characters
- ✅ Most requests bypass capture logic

### When Capturing:
- Slightly slower (buffering response)
- Only affects first request to each endpoint
- Subsequent requests are fast (no capture)

**Net impact: Negligible for testing purposes!**

---

## 🎓 Advanced Usage

### Manually Trigger Sample Capture

```csharp
// Your custom code
var samples = _metricsCollector.GetSampleRequestsResponses();

foreach (var sample in samples)
{
    Console.WriteLine($"Endpoint: {sample.Key}");
    Console.WriteLine($"Request: {sample.Value.RequestBody}");
    Console.WriteLine($"Response: {sample.Value.ResponseBody}");
}
```

### Clear Samples (Reset)

```bash
# Reset all metrics (includes samples)
curl -X POST http://localhost:5072/api/metrics/reset
```

### Check If Samples Exist

```bash
# Get comparison report
$report = Invoke-RestMethod http://localhost:5072/api/metrics/comparison

# Check samples (they're not in the JSON yet, only in HTML report)
# View HTML report instead:
start http://localhost:5072/api/metrics/report
```

---

## ✅ Testing the Feature

### Test 1: No Data Yet
```powershell
# Start app
dotnet run

# Open report (before making any calls)
start http://localhost:5072/api/metrics/report

# Should see:
"No sample data available yet..."
```

### Test 2: After REST Call
```powershell
# Make REST call
curl http://localhost:5072/api/customers/1

# Refresh report
# Should see REST example for "Simple Customer Retrieval"
```

### Test 3: After GraphQL Call
```powershell
# Make GraphQL call
curl -X POST http://localhost:5072/graphql -d '{\"query\":\"{customers{name}}\"}'

# Refresh report
# Should see GraphQL example
```

### Test 4: After Full Test Run
```powershell
# Run comprehensive test
.\launch-tests.ps1
# Choose: 2 (Standard Load Test)

# View report
# Should see all 5 scenarios with real data!
```

---

## 📝 Notes

### Captured Data Includes:
- ✅ HTTP method (GET, POST, etc.)
- ✅ Endpoint path
- ✅ Request body (for POST/PUT)
- ✅ Response body
- ✅ Status code
- ✅ Timestamp

### Not Included:
- ❌ Request headers (except Content-Type implied)
- ❌ Response headers
- ❌ Authentication tokens
- ❌ Cookies

### JSON Formatting:
- Automatically pretty-prints JSON
- If not JSON, shows as-is
- Truncates at 5000 characters

---

## 🎯 Summary

**Before:**
- Hardcoded fake examples
- Same data every time
- Not representative of your API

**After:**
- Real requests from your tests
- Actual data from your database
- True representation of your API behavior

**Result:** Much more useful and accurate examples! 🎉

---

## 🆘 Troubleshooting

### "No sample data available"
**Fix:** Make some API calls first, then refresh report

### Samples don't appear
**Fix:** Check if you're calling the right endpoints (see list above)

### Truncated responses
**Expected:** Responses over 5000 characters are truncated to avoid huge HTML pages

### Old samples persist
**Fix:** Reset metrics: `POST /api/metrics/reset`

---

**Enjoy your real, live API examples!** 🚀
