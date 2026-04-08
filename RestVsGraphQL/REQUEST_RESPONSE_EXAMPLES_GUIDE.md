# Request & Response Examples - New Feature ✨

## 🎯 What's New?

The metrics report now includes a **new section** showing **sample requests and responses** for each test scenario!

This helps you understand exactly what data is being exchanged between REST and GraphQL APIs.

---

## 📍 Where to See It

1. **Start your application** (F5 in Visual Studio)
2. **Run some tests** (make API calls or run launch-tests.ps1)
3. **Open the report**: `http://localhost:5072/api/metrics/report`
4. **Scroll down** to the new section: **"Request & Response Examples"**

---

## 📊 What You'll See

The new section shows **5 test scenarios** with side-by-side comparisons:

### Scenario 1: Simple Customer Retrieval
- **REST**: `GET /api/customers/1`
- **GraphQL**: Query for customer by ID
- Shows: Basic data fetching

### Scenario 2: Order with Nested Data (3 Levels Deep)
- **REST**: `GET /api/orders/1/nested`
- **GraphQL**: Nested query for order with customer, items, products, categories
- Shows: Complex nested object retrieval

### Scenario 3: Dashboard Aggregation
- **REST**: `GET /api/dashboard`
- **GraphQL**: Dashboard query with all statistics
- Shows: Aggregated data and statistics

### Scenario 4: Multiple Resources (Customer + Orders + Products)
- **REST**: 3 separate GET calls
- **GraphQL**: 1 combined query
- Shows: The key difference - REST needs multiple calls, GraphQL combines them

### Scenario 5: Bulk Create Orders
- **REST**: `POST /api/orders/bulk`
- **GraphQL**: Mutation with variables
- Shows: Bulk operations and mutations

---

## 🎨 Visual Layout

Each scenario shows **4 boxes**:

```
┌─────────────────────────────────────────────────────────────┐
│                   Scenario 1: Simple Customer Retrieval     │
├─────────────────────────┬───────────────────────────────────┤
│  🔴 REST API            │  🟣 GraphQL API                   │
├─────────────────────────┼───────────────────────────────────┤
│  Request:               │  Request:                         │
│  GET /api/customers/1   │  POST /graphql                    │
│  [formatted code]       │  { query: "{ customer ... }" }    │
│                         │  [formatted code]                 │
├─────────────────────────┼───────────────────────────────────┤
│  Response:              │  Response:                        │
│  { "id": 1,             │  { "data": {                      │
│    "name": "John Doe",  │    "customer": {                  │
│    ... }                │      "id": 1, ... }}}             │
│  [formatted JSON]       │  [formatted JSON]                 │
└─────────────────────────┴───────────────────────────────────┘
```

---

## 💡 Why This is Useful

### 1. **Learn by Example**
- See exactly how to call each API
- Understand the request format
- See what data you get back

### 2. **Spot Differences**
- Compare REST and GraphQL syntax side-by-side
- See GraphQL's `data` wrapper
- Notice REST's multiple calls vs GraphQL's single call

### 3. **Documentation**
- Use as reference when building your own apps
- Copy-paste examples to test
- Share with team members

### 4. **Training Tool**
- Great for teaching REST vs GraphQL
- Visual comparison helps understanding
- Real examples from your application

---

## 📋 Example: What You'll See for Scenario 4

### REST (3 Calls):
```http
# Call 1: Get Customer
GET /api/customers/1 HTTP/1.1

# Call 2: Get Customer's Orders
GET /api/customers/1/orders HTTP/1.1

# Call 3: Get All Products
GET /api/products HTTP/1.1
```

### GraphQL (1 Call):
```http
POST /graphql HTTP/1.1
Content-Type: application/json

{
  "query": "{ 
    customer(id: 1) { 
      name 
      orders { id totalAmount }
    }
    products { id name price }
  }"
}
```

**Visual Impact**: Clearly shows GraphQL's advantage for fetching related data!

---

## 🎓 How to Use This Feature

### For Learning:
1. Open the report
2. Read through each scenario
3. Try copying the requests and sending them yourself
4. Compare the responses

### For Development:
1. Use examples as templates for your own apps
2. Modify the queries to get different data
3. Test with tools like Postman or curl

### For Decision Making:
1. See the actual data payload sizes
2. Compare request complexity
3. Understand which approach fits your needs

---

## 🔍 Technical Details

### Color Coding:
- 🔴 **Red border** = REST API examples
- 🟣 **Purple border** = GraphQL API examples

### Formatting:
- Syntax-highlighted code blocks
- Proper JSON formatting
- HTTP headers included
- Comments for multi-call scenarios

### Content:
- **Request**: Shows HTTP method, endpoint, headers, body
- **Response**: Shows actual JSON response structure
- **Real data**: Uses actual data from your application

---

## 📖 Reading the Examples

### REST Format:
```http
GET /api/endpoint HTTP/1.1          ← HTTP Method & Path
Host: localhost:5072                 ← Server
Accept: application/json             ← Headers

{                                    ← Response Body (JSON)
  "field": "value"
}
```

### GraphQL Format:
```http
POST /graphql HTTP/1.1               ← Always POST
Content-Type: application/json       ← Always JSON

{
  "query": "{ field }"               ← GraphQL Query
}

{                                    ← Response wrapped in "data"
  "data": {
    "field": "value"
  }
}
```

---

## 💡 Key Insights from Examples

### Insight 1: GraphQL Field Selection
**Notice**: GraphQL only returns fields you ask for

```graphql
# Ask for: id, name
{ customer(id: 1) { id name } }

# Get back: Only id and name (not email, phone, etc.)
```

**Benefit**: Smaller payloads, less bandwidth!

### Insight 2: REST Over-fetching
**Notice**: REST returns all fields whether you need them or not

```http
GET /api/customers/1

# Get back: id, name, email, phone, address, createdAt, updatedAt...
# (Even if you only needed name!)
```

**Drawback**: Larger payloads, wasted bandwidth

### Insight 3: Multiple Calls vs Single Call
**Scenario 4 clearly shows**:
- REST: 3 network round-trips (wait 3 times)
- GraphQL: 1 network round-trip (wait once)

**Impact**: Faster for mobile users, better performance

### Insight 4: GraphQL Nesting
**Notice**: GraphQL naturally handles nested data

```graphql
{
  order {
    customer { name }
    items {
      product {
        category { name }
      }
    }
  }
}
```

**Benefit**: Get related data in one query, no joins needed

---

## 🎯 Tips for Using This Section

### Tip 1: Compare Payload Sizes
Look at the response sizes:
- Scenario 1: REST ~150 bytes, GraphQL ~170 bytes (similar)
- Scenario 2: REST might return more data than GraphQL (if fields differ)
- Scenario 4: REST sends 3 responses, GraphQL sends 1

### Tip 2: Notice the Patterns
- REST: Always GET/POST/PUT/DELETE + endpoint
- GraphQL: Always POST /graphql + query/mutation

### Tip 3: Try the Examples
Copy-paste into:
- **Postman**: Import as HTTP request
- **curl**: Run from command line
- **Browser DevTools**: Fetch API

### Tip 4: Modify and Experiment
Change the examples:
- Different customer IDs
- Different fields
- Different filters

---

## 📚 Related Documentation

- `LAUNCH_TESTS_FAIRNESS_ANALYSIS.md` - Explains test fairness
- `TEST_CASES_EXPLAINED_SIMPLE.md` - Non-technical test guide
- `HOW_TO_VIEW_RESULTS.md` - How to run tests and view reports

---

## ✅ Summary

**New Feature**: Request & Response Examples section in metrics report

**Shows**: 5 test scenarios with REST and GraphQL side-by-side

**Benefits**:
- ✅ Learn by example
- ✅ Visual comparison
- ✅ Real data from your app
- ✅ Copy-paste ready
- ✅ Great for teaching/training

**Access**: `http://localhost:5072/api/metrics/report` → Scroll to bottom

**Perfect for**: Understanding differences, learning APIs, making decisions!

---

**Enjoy exploring your API comparisons with real examples!** 🎉
