# 📊 KPI & NFR Quick Reference

## ⚡ Quick Start (30 seconds)

```powershell
# Terminal 1: Start API
cd RestVsGraphQL
dotnet run

# Terminal 2: Run tests
.\load-test.ps1

# Browser: View results
http://localhost:5072/api/metrics/report
```

## 🎯 Metrics Endpoints

| Endpoint | Purpose |
|----------|---------|
| `/api/metrics/report` | 📄 Beautiful HTML report |
| `/api/metrics/comparison` | 📊 JSON comparison data |
| `/api/metrics/summary` | 📈 Quick summary |
| `/api/metrics/rest` | 🔴 REST-only metrics |
| `/api/metrics/graphql` | 🟣 GraphQL-only metrics |
| `/api/metrics/reset` POST | 🔄 Reset metrics |

## 📊 KPIs Measured

- ✅ Response Time (P50, P95, P99)
- ✅ Throughput (req/s)
- ✅ Success Rate (%)
- ✅ Payload Size (bytes)
- ✅ Error Rate (%)
- ✅ Bandwidth Usage

## ✅ NFRs Measured

- ✅ Performance (latency, throughput)
- ✅ Scalability (concurrent requests)
- ✅ Reliability (success rate)
- ✅ Efficiency (payload size, bandwidth)
- ✅ Maintainability (code complexity)
- ✅ Usability (developer experience)

## 🧪 Load Test Options

```powershell
# Default (100 iterations, 10 concurrent)
.\load-test.ps1

# Custom load
.\load-test.ps1 -Iterations 500 -ConcurrentRequests 50

# Heavy load
.\load-test.ps1 -Iterations 1000 -ConcurrentRequests 100
```

## 📈 What Gets Tested

1. Simple queries (customers)
2. Nested data (3 levels deep)
3. Dashboard aggregations
4. Multiple resources
5. Bulk operations

## 🏆 Check Winners

```powershell
$results = Invoke-RestMethod http://localhost:5072/api/metrics/comparison
Write-Host "Response Time: $($results.responseTimeWinner)"
Write-Host "Payload Size: $($results.payloadSizeWinner)"
Write-Host "Throughput: $($results.throughputWinner)"
Write-Host "Reliability: $($results.reliabilityWinner)"
```

## 📄 View Full Report

**Browser:** `http://localhost:5072/api/metrics/report`

Shows:
- Executive summary
- KPI comparison cards
- Response time analysis
- Bandwidth efficiency
- Per-endpoint breakdown
- NFR assessment

## 🔄 Reset & Retest

```powershell
# Reset metrics
Invoke-RestMethod -Uri http://localhost:5072/api/metrics/reset -Method Post

# Run new test
.\load-test.ps1 -Iterations 200
```

## 📖 Full Documentation

- `KPI_NFR_GUIDE.md` - Complete guide
- `KPI_NFR_IMPLEMENTATION.md` - What was implemented
- `README.md` - Project overview

---

**Status:** ✅ Ready to measure and compare!
