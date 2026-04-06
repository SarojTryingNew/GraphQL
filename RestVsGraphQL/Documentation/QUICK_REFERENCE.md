# 🚀 REST vs GraphQL - Quick Reference Card

## 📦 What You Have

A complete .NET 9 Web API project comparing REST and GraphQL with:
- ✅ Both APIs fully implemented
- ✅ Performance testing infrastructure
- ✅ YAML-based testing framework
- ✅ Comprehensive documentation
- ✅ Helper scripts for easy testing

## ⚡ Quick Start (30 seconds)

1. **Start API:**
   ```powershell
   cd RestVsGraphQL
   dotnet run
   ```

2. **Test REST:** Open browser → `http://localhost:5072/api/customers`

3. **Test GraphQL:** Open browser → `http://localhost:5072/graphql`

## 🎯 4 Key Scenarios Implemented

| # | Scenario | REST Endpoint | GraphQL Query |
|---|----------|---------------|---------------|
| 1 | **Bulk Operations** | `POST /api/orders/bulk` | `mutation { bulkCreateOrders(...) }` |
| 2 | **Nested Objects** | `GET /api/orders/1/nested` | `{ order(id:1) { items { notes }}}` |
| 3 | **Dashboard** | `GET /api/dashboard` | `{ dashboard { topProducts }}` |
| 4 | **Multiple Calls** | 3 endpoints | 1 query with multiple roots |

## 📊 Run Tests

### Option 1: PowerShell Script (Recommended)
```powershell
.\run-tests.ps1
# Select option 7 for complete comparison
```

### Option 2: YAML Test Runner
```powershell
# Run from TestRunner/Program.cs
# Or use the YamlTestRunner class directly
```

### Option 3: Manual Testing
```powershell
# REST
curl http://localhost:5072/api/dashboard

# GraphQL
curl -X POST http://localhost:5072/graphql `
  -H "Content-Type: application/json" `
  -d '{"query":"{ customers { id name } }"}'
```

## 🔗 Important URLs

| Service | URL | Purpose |
|---------|-----|---------|
| **REST API** | `http://localhost:5072/api/*` | All REST endpoints |
| **GraphQL** | `http://localhost:5072/graphql` | GraphQL endpoint |
| **GraphQL IDE** | `http://localhost:5072/graphql` | Interactive query builder |

## 📄 Key Files

| File | Purpose |
|------|---------|
| `README.md` | Complete documentation |
| `QUICKSTART.md` | Step-by-step guide |
| `ARCHITECTURE.md` | Visual diagrams & comparisons |
| `PROJECT_SUMMARY.md` | Executive summary |
| `IMPLEMENTATION_CHECKLIST.md` | Verification checklist |
| `start-api.ps1` | Start the API |
| `run-tests.ps1` | Run all tests |

## 🧪 Sample GraphQL Queries

### Simple Query
```graphql
{
  customer(id: 1) {
    name
    email
  }
}
```

### Nested Query (3 Levels)
```graphql
{
  order(id: 1) {
    customer { name }
    items {
      product { 
        name
        category { name }
      }
      notes { content }
    }
  }
}
```

### Dashboard
```graphql
{
  dashboard {
    totalOrders
    totalRevenue
    topProducts {
      productName
      revenue
    }
  }
}
```

### Multiple Resources (1 call!)
```graphql
{
  customer(id: 1) {
    name
    orders { totalAmount }
  }
  products {
    name
    price
  }
}
```

## 📈 What to Measure

| Metric | How to Check |
|--------|--------------|
| **Response Time** | Stopwatch in PowerShell script |
| **Payload Size** | Network tab or content length |
| **# of Requests** | Count API calls needed |
| **Over-fetching** | Compare returned vs needed data |
| **Flexibility** | Try different field combinations |

## 🎓 Expected Winners

| Scenario | Winner | Why |
|----------|--------|-----|
| Single record | **Tie** | Similar performance |
| Nested data | **GraphQL** | Less over-fetching |
| Multiple resources | **GraphQL** | 1 call vs 3+ calls |
| Dashboard | **Depends** | Fixed needs → Tie, Variable → GraphQL |
| Bulk ops | **Tie** | Similar performance |
| Caching | **REST** | Better HTTP caching |

## 🏆 Decision Guide

**Choose GraphQL if:**
- ✓ Complex UI with varied data needs
- ✓ Mobile app (bandwidth matters)
- ✓ Multiple client types
- ✓ Reducing roundtrips is critical

**Choose REST if:**
- ✓ Simple CRUD operations
- ✓ Heavy caching needed
- ✓ File uploads/downloads
- ✓ Team lacks GraphQL experience

**Use Both if:**
- ✓ Large organization
- ✓ Different client needs
- ✓ Gradual migration

## ⚠️ Troubleshooting

| Problem | Solution |
|---------|----------|
| Port 5000 in use | `netstat -ano \| findstr :5000` then kill process |
| Build fails | `dotnet restore && dotnet build` |
| GraphQL errors | Check syntax in Banana Cake Pop IDE |
| Tests fail | Ensure API is running first |

## 📞 Need Help?

1. Check `README.md` for detailed docs
2. Review `QUICKSTART.md` for step-by-step
3. Examine test files in `TestSuites/`
4. Look at controller code for examples

## 🎯 Your Next Steps

1. [ ] Start the API (`.\start-api.ps1`)
2. [ ] Test REST endpoints in browser
3. [ ] Explore GraphQL in Banana Cake Pop
4. [ ] Run automated tests (`.\run-tests.ps1`)
5. [ ] Compare results
6. [ ] Make your decision: REST, GraphQL, or both?

---

**Status:** ✅ READY TO RUN

**Project:** Complete & Fully Functional

**Time to results:** ~15 minutes

Good luck with your comparison! 🚀

