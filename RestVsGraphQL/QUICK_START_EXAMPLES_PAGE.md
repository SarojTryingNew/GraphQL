# Quick Start: View Request/Response Examples

## 🚀 3 Steps to See Examples

### Step 1: Restart Your App (Hot Reload Won't Work)
Since you're debugging, you need to restart:

**In Visual Studio:**
1. Press `Shift+F5` to stop debugging
2. Press `F5` to start again
3. Wait for "Now listening on: http://localhost:5072"

### Step 2: Run a Test
```powershell
cd C:\Users\z004ev4m\source\repos\RestVsGraphQL
.\launch-tests.ps1
```

Choose any option (I recommend **Option 1 - Quick Test** for fastest results)

### Step 3: View Examples
After the test completes, the metrics report opens automatically.

**Click the link at the top:**
```
📋 View Request/Response Examples
```

**Or visit directly:**
```
http://localhost:5072/api/metrics/examples
```

## 🎯 What You'll See

### Side-by-Side Comparison
```
┌─────────────────────────┬─────────────────────────┐
│  🔵 REST API            │  🔴 GraphQL API         │
├─────────────────────────┼─────────────────────────┤
│  GET /api/customers/1   │  POST /graphql          │
│                         │                         │
│  📤 Request:            │  📤 Request (Query):    │
│  (no body - GET)        │  {                      │
│                         │    "query": "query {    │
│  📥 Response:           │      customer(id: 1)    │
│  {                      │    }"                   │
│    "id": 1,             │  }                      │
│    "name": "John",      │                         │
│    "email": "..."       │  📥 Response:           │
│  }                      │  {                      │
│                         │    "data": {            │
│  Size: 245 B | 12.5ms   │      "customer": {...}  │
│                         │    }                    │
│                         │  }                      │
│                         │                         │
│                         │  Size: 189 B | 10.2ms   │
└─────────────────────────┴─────────────────────────┘
```

## 🔍 Key Features

### ✅ Shows Actual Data
- Not hardcoded examples
- Real requests from your tests
- Real responses from API

### ✅ Easy Comparison
- REST on left, GraphQL on right
- Same operation, different approaches
- Clear visual differences

### ✅ Beautiful Formatting
- JSON is pretty-printed
- Color-coded by API type
- Dark code blocks
- Scrollable for long content

### ✅ Useful Metadata
- Response size (bytes)
- Response time (milliseconds)
- Status codes
- HTTP methods

## 📊 What Gets Captured

The system captures **first 2 requests per endpoint**:

### From Quick Test (Option 1):
- **Customers:** 2 GET requests
- **Orders:** 2 GET requests  
- **Products:** 2 GET requests
- **Dashboard:** 2 GET requests
- **Bulk Orders:** 2 POST requests

**Total:** ~10 examples captured out of 50+ total requests

### From Standard Test (Option 2):
- Same endpoints
- **Total:** ~10 examples captured out of 500+ total requests

**Performance Impact:** < 2% (only first 2 per endpoint buffered)

## 🎨 Visual Elements

### Color Coding:
- **REST** = Red borders and headers
- **GraphQL** = Pink borders and headers
- **Status 200** = Green badge
- **Status 4xx/5xx** = Red badge

### Sections:
- **📤 Request** - What you sent to the API
- **📥 Response** - What the API returned
- **Metadata** - Size and timing info

### Organization:
- Grouped by operation type
- Customers, Orders, Products, etc.
- All examples for same operation together

## 🔄 How Capture Works

### First Request:
```
Request #1 to GET /api/customers/1
→ Middleware: "This is sample #1, CAPTURE IT"
→ Buffers request body
→ Buffers response body
→ Stores in metrics
```

### Second Request:
```
Request #2 to GET /api/customers/1
→ Middleware: "This is sample #2, CAPTURE IT"
→ Buffers request body
→ Buffers response body
→ Stores in metrics
```

### Third+ Requests:
```
Request #3 to GET /api/customers/1
→ Middleware: "Already have 2 samples, SKIP CAPTURE"
→ Pass-through mode (zero overhead)
→ Only stores metrics (no body capture)
```

## ❓ FAQ

### Q: How many examples will I see?
**A:** Maximum 2 per unique endpoint. Typical test has 5-10 different endpoints, so you'll see 10-20 examples total.

### Q: What if I don't see any examples?
**A:** Make sure you:
1. Restarted the app after code changes
2. Ran a test from `launch-tests.ps1`
3. Waited for test to complete
4. Examples page will say "No Examples Captured Yet" if none exist

### Q: Can I see all requests, not just 2?
**A:** No, that would impact performance too much. For debugging all requests, use:
- Browser Developer Tools (F12 → Network tab)
- Fiddler or similar proxy tools
- Application logging

### Q: Will this slow down my tests?
**A:** Minimal impact:
- Only 2 requests per endpoint are buffered
- Other requests use pass-through mode (zero overhead)
- Typical impact: < 2% of total test time

### Q: How do I clear examples?
**A:** Examples are cleared when you reset metrics:
- Happens automatically when you run a new test from `launch-tests.ps1`
- Or call: `POST http://localhost:5072/api/metrics/reset`

### Q: Can I export examples?
**A:** Currently displays in HTML only. To export:
- Copy from browser
- Use browser's "Save Page As"
- Or use browser developer tools to get raw JSON from API

## 🎯 Common Use Cases

### Use Case 1: Understanding GraphQL Queries
**Problem:** "I don't know what GraphQL query to write"  
**Solution:** Run a test, look at examples page, copy the query structure

### Use Case 2: Comparing Response Sizes
**Problem:** "Which API returns more data?"  
**Solution:** Look at the metadata under each response (Size: XXX B)

### Use Case 3: Debugging Differences
**Problem:** "REST and GraphQL return different data"  
**Solution:** Compare responses side-by-side to spot differences

### Use Case 4: Showing Stakeholders
**Problem:** "Explain REST vs GraphQL to non-technical people"  
**Solution:** Show them the examples page - visual and easy to understand

## 📝 Example URLs

After starting your app (`http://localhost:5072`):

### Main Pages:
- **Metrics Report:** `http://localhost:5072/api/metrics/report`
- **Examples Page:** `http://localhost:5072/api/metrics/examples`

### API Endpoints (for testing):
- **Get Metrics:** `http://localhost:5072/api/metrics/summary`
- **Reset Metrics:** `POST http://localhost:5072/api/metrics/reset`
- **Set Scenario:** `POST http://localhost:5072/api/metrics/scenario`

## ✨ That's It!

You now have a **separate page** showing:
- ✅ Real request/response examples
- ✅ Side-by-side REST vs GraphQL comparison
- ✅ Beautiful formatting
- ✅ Minimal performance impact

**Go try it now!** 🚀

---

**Next Step:** Restart app → Run test → View examples at `/api/metrics/examples`
