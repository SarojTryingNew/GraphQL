# ❌ PORT CONFIGURATION MISMATCH DETECTED!

## 🚨 Critical Issue Found

Your application has **MISMATCHED PORTS** that will cause the Gateway pattern to **FAIL**!

---

## 🔍 Current Port Configuration

### ✅ What's Correct:

| Component | Configured Port | Status |
|-----------|----------------|--------|
| **.NET API Server** | `http://localhost:5072` | ✅ Correct |
| **Angular App** | `http://localhost:5072` | ✅ Correct |
| **launch-tests.ps1** | `http://localhost:5072` | ✅ Correct |
| **Performance Tests** | `http://localhost:5072` | ✅ Correct |

### ❌ What's WRONG:

| Component | Current Value | Should Be | Impact |
|-----------|--------------|-----------|---------|
| **RestApiClient (Gateway)** | `https://localhost:7000/api` | `http://localhost:5072/api` | 🔴 **CRITICAL** |
| **appsettings.json** | `"BaseUrl": "https://localhost:7000/api"` | `"BaseUrl": "http://localhost:5072/api"` | 🔴 **CRITICAL** |

---

## 💥 The Problem

### Current Flow (BROKEN):

```
Client (Angular on port ???)
    ↓
GraphQL Gateway (http://localhost:5072/graphql)
    ↓
RestApiClient tries to call:
    ↓
❌ https://localhost:7000/api/customers  ← NOTHING LISTENING HERE!
    ↓
💥 CONNECTION REFUSED / TIMEOUT
```

### What Should Happen:

```
Client (Angular)
    ↓
GraphQL Gateway (http://localhost:5072/graphql)
    ↓
RestApiClient calls:
    ↓
✅ http://localhost:5072/api/customers  ← SAME SERVER!
    ↓
REST API (on same server)
    ↓
DataStore
```

---

## 🔧 Fix Required

### File: `RestVsGraphQL/appsettings.json`

**Change:**
```json
{
  "RestApi": {
    "BaseUrl": "https://localhost:7000/api",  // ❌ WRONG PORT
    "Timeout": 30
  }
}
```

**To:**
```json
{
  "RestApi": {
    "BaseUrl": "http://localhost:5072/api",  // ✅ CORRECT PORT
    "Timeout": 30
  }
}
```

---

## 📊 Complete Port Audit

### Files Checked:

#### ✅ Correct Configuration:

1. **launchSettings.json**
   ```json
   "applicationUrl": "http://localhost:5072"
   ```
   Status: ✅ Correct

2. **Angular environment.ts**
   ```typescript
   apiBaseUrl: 'http://localhost:5072',
   graphqlEndpoint: 'http://localhost:5072/graphql'
   ```
   Status: ✅ Correct

3. **launch-tests.ps1**
   ```powershell
   http://localhost:5072/api/metrics/reset
   ```
   Status: ✅ Correct

4. **All Performance Test Scripts**
   ```powershell
   [string]$BaseUrl = "http://localhost:5072"
   ```
   Status: ✅ Correct (10 files checked)

#### ❌ Incorrect Configuration:

5. **appsettings.json**
   ```json
   "BaseUrl": "https://localhost:7000/api"
   ```
   Status: ❌ **WRONG** - Should be `http://localhost:5072/api`

6. **RestApiClient.cs (fallback)**
   ```csharp
   _baseUrl = _configuration["RestApi:BaseUrl"] ?? "https://localhost:7000/api";
   ```
   Status: ⚠️ Fallback is wrong (but config takes precedence)

---

## 🎯 Why This Matters

### For Gateway Pattern:

In your Gateway architecture, the GraphQL layer and REST layer **run on the SAME server**:

```
Single .NET Application (localhost:5072)
├── /graphql         ← GraphQL Gateway endpoint
├── /api/customers   ← REST API endpoint
├── /api/orders      ← REST API endpoint
└── /api/products    ← REST API endpoint
```

The **RestApiClient** is configured to make **internal HTTP calls** to the same server's REST endpoints.

### Current Issue:

1. GraphQL Gateway receives request on `http://localhost:5072/graphql`
2. GatewayQuery tries to call RestApiClient
3. RestApiClient tries to connect to `https://localhost:7000/api/customers`
4. **Nothing is listening on port 7000!**
5. Request fails with "Connection refused" or timeout

---

## 🧪 How to Verify the Issue

### Test 1: Check Current Configuration

```powershell
# From RestVsGraphQL directory
dotnet run

# In another terminal, test direct REST call (should work)
curl http://localhost:5072/api/customers

# Test GraphQL Gateway (will FAIL)
curl -X POST http://localhost:5072/graphql `
  -H "Content-Type: application/json" `
  -d '{"query":"{ customers { name } }"}'
```

**Expected Error:**
```
Connection refused to https://localhost:7000
```

### Test 2: After Fix

After fixing appsettings.json:

```powershell
# Restart application
dotnet run

# Test GraphQL Gateway (should work now)
curl -X POST http://localhost:5072/graphql `
  -H "Content-Type: application/json" `
  -d '{"query":"{ customers { name } }"}'

# Check logs - should see:
# info: System.Net.Http.HttpClient.RestApiClient
#       GET http://localhost:5072/api/customers  ✅
```

---

## 📝 Additional Considerations

### Why Port 7000 in the Configuration?

The `7000` port was likely from:
- Initial template or example
- Different environment configuration
- Microservices architecture (separate services)

### Why Same Port (5072) is Correct for Your Setup:

In your **Gateway pattern**, you have a **monolithic approach** where:
- GraphQL and REST are in the **same application**
- Running on the **same server**
- Both accessible via **localhost:5072**

This is correct for:
- ✅ Development/testing
- ✅ Learning the Gateway pattern
- ✅ Single-server deployment

### For Microservices (Future):

If you later split into separate services:
```
GraphQL Gateway (port 5072)
    ↓ HTTP
REST Service 1 (port 5001)
REST Service 2 (port 5002)
REST Service 3 (port 5003)
```

Then you would configure different ports. But **not now**.

---

## 🚀 Action Items

### IMMEDIATE (Required):

1. **Fix appsettings.json**
   - Change `BaseUrl` from `https://localhost:7000/api` to `http://localhost:5072/api`
   
2. **Restart Application**
   ```powershell
   # Kill current instance (Ctrl+C)
   dotnet run
   ```

3. **Verify Fix**
   ```powershell
   # Test GraphQL Gateway
   curl -X POST http://localhost:5072/graphql `
     -H "Content-Type: application/json" `
     -d '{"query":"{ customers { name } }"}'
   ```

### OPTIONAL (Recommended):

4. **Update RestApiClient.cs fallback**
   ```csharp
   // Change this line:
   _baseUrl = _configuration["RestApi:BaseUrl"] ?? "http://localhost:5072/api";
   ```

5. **Add appsettings.Development.json override** (if needed)
   ```json
   {
     "RestApi": {
       "BaseUrl": "http://localhost:5072/api"
     }
   }
   ```

---

## ✅ Expected Behavior After Fix

### GraphQL Query:
```graphql
query {
  customers {
    name
    orders {
      orderDate
    }
  }
}
```

### Console Logs:
```
info: System.Net.Http.HttpClient.RestApiClient
      GET http://localhost:5072/api/customers
info: System.Net.Http.HttpClient.RestApiClient
      GET http://localhost:5072/api/orders/by-customers?customerIds=1,2,3
```

### Response:
```json
{
  "data": {
    "customers": [
      { "name": "John Doe", "orders": [...] }
    ]
  }
}
```

---

## 🎓 Summary

| Issue | Status | Fix |
|-------|--------|-----|
| **Angular App Port** | ✅ Correct (5072) | No change needed |
| **Test Scripts Port** | ✅ Correct (5072) | No change needed |
| **API Server Port** | ✅ Correct (5072) | No change needed |
| **Gateway REST Client** | ❌ **WRONG** (7000) | 🔧 **Update appsettings.json** |

**After fixing the port in appsettings.json, your entire Gateway architecture will work correctly!** 🚀
