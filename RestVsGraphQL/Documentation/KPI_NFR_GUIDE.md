# KPI & NFR Measurement Guide

## 📊 Key Performance Indicators (KPIs)

### 1. **Response Time**
**What:** Time taken to process and return a response
**How to Measure:** Automated via MetricsMiddleware
**Targets:**
- P50 (Median): < 50ms
- P95: < 100ms  
- P99: < 200ms

### 2. **Throughput**
**What:** Number of requests processed per second
**How to Measure:** Total requests / uptime
**Targets:**
- Minimum: > 100 req/s
- Target: > 500 req/s

### 3. **Success Rate**
**What:** Percentage of successful requests
**How to Measure:** (Successful requests / Total requests) × 100
**Targets:**
- Minimum: 99%
- Target: 99.9%

### 4. **Payload Size**
**What:** Size of request and response bodies
**How to Measure:** Content-Length header
**Targets:**
- Minimize over-fetching
- GraphQL should be 30-50% smaller than REST for complex queries

### 5. **Error Rate**
**What:** Percentage of failed requests
**How to Measure:** (Failed requests / Total requests) × 100
**Targets:**
- Maximum: 1%
- Target: < 0.1%

## ✅ Non-Functional Requirements (NFRs)

### 1. **Performance**
**Description:** System responsiveness and speed

**Metrics:**
- Response Time (P50, P95, P99)
- Latency under load
- Time to First Byte (TTFB)

**How to Measure:**
```powershell
.\load-test.ps1 -Iterations 1000 -ConcurrentRequests 50
```

**Acceptance Criteria:**
- Average response time < 50ms
- P95 response time < 100ms
- No degradation up to 1000 concurrent users

### 2. **Scalability**
**Description:** Ability to handle increased load

**Metrics:**
- Requests per second at different loads
- Response time degradation curve
- Resource utilization vs load

**How to Measure:**
```powershell
# Run with increasing load
.\load-test.ps1 -Iterations 100 -ConcurrentRequests 10
.\load-test.ps1 -Iterations 100 -ConcurrentRequests 50
.\load-test.ps1 -Iterations 100 -ConcurrentRequests 100
```

**Acceptance Criteria:**
- Linear scalability up to 100 concurrent users
- Graceful degradation beyond capacity
- No request failures under normal load

### 3. **Reliability**
**Description:** System uptime and error handling

**Metrics:**
- Success rate
- Error rate
- Mean Time Between Failures (MTBF)

**How to Measure:**
- Monitored automatically by MetricsCollector
- View: `http://localhost:5072/api/metrics/comparison`

**Acceptance Criteria:**
- 99.9% success rate
- < 0.1% error rate
- Graceful error handling (no crashes)

### 4. **Efficiency**
**Description:** Resource utilization

**Metrics:**
- CPU usage
- Memory consumption
- Network bandwidth
- Payload sizes

**How to Measure:**
```powershell
# Check metrics endpoint
Invoke-RestMethod http://localhost:5072/api/metrics/comparison
```

**Acceptance Criteria:**
- Memory usage < 500MB under load
- CPU usage < 80% at peak
- Bandwidth optimization (GraphQL should use less)

### 5. **Maintainability**
**Description:** Ease of understanding and modifying code

**Metrics:**
- Lines of Code (LOC)
- Cyclomatic Complexity
- Code duplication
- Test coverage

**How to Measure:**
Manual code analysis:

| Aspect | REST | GraphQL |
|--------|------|---------|
| LOC (Controllers/Resolvers) | ~400 lines | ~250 lines |
| Endpoints/Queries | 11 endpoints | 1 endpoint |
| Schema Definition | Implicit (models) | Explicit (generated from types) |
| Type Safety | Compile-time | Runtime + compile-time |

**Acceptance Criteria:**
- Code complexity: Low to Medium
- DRY principle followed
- Clear separation of concerns

### 6. **Usability (Developer Experience)**
**Description:** Ease of use for API consumers

**Metrics:**
- Learning curve
- Documentation completeness
- Tooling support
- Error messages clarity

**How to Evaluate:**

**REST:**
- ✅ Familiar HTTP methods
- ✅ Swagger UI for testing
- ✅ Standard error codes
- ❌ Multiple endpoints to learn
- ❌ Over-fetching common

**GraphQL:**
- ✅ Single endpoint
- ✅ Banana Cake Pop IDE
- ✅ Self-documenting schema
- ✅ Precise data fetching
- ❌ Learning GraphQL query language
- ❌ More complex error handling

### 7. **Security**
**Description:** Protection against threats

**Metrics:**
- Vulnerability scan results
- Authentication/Authorization
- Rate limiting
- Input validation

**Current Implementation:**
- ✅ CORS configured
- ✅ HTTPS support
- ⚠️ No authentication (demo project)
- ⚠️ No rate limiting (demo project)

**Production Recommendations:**
- Add JWT authentication
- Implement rate limiting
- Add query depth limiting (GraphQL)
- Add query cost analysis (GraphQL)
- Input validation and sanitization

## 🧪 How to Run Complete KPI/NFR Tests

### Step 1: Start the Application
```powershell
cd RestVsGraphQL
dotnet run
```

### Step 2: Run Load Tests
```powershell
.\load-test.ps1 -Iterations 100 -ConcurrentRequests 10
```

### Step 3: View Results

**Option 1: HTML Report**
Open browser: `http://localhost:5072/api/metrics/report`

**Option 2: JSON API**
```powershell
# Full comparison
Invoke-RestMethod http://localhost:5072/api/metrics/comparison

# REST metrics only
Invoke-RestMethod http://localhost:5072/api/metrics/rest

# GraphQL metrics only
Invoke-RestMethod http://localhost:5072/api/metrics/graphql
```

**Option 3: Summary**
```powershell
Invoke-RestMethod http://localhost:5072/api/metrics/summary
```

### Step 4: Reset Metrics (for new test run)
```powershell
Invoke-RestMethod -Uri http://localhost:5072/api/metrics/reset -Method Post
```

## 📈 Interpreting Results

### Performance Comparison

**REST Wins When:**
- Simple CRUD operations
- HTTP caching is critical
- Single resource queries
- Standard tooling preferred

**GraphQL Wins When:**
- Complex nested data
- Multiple resources needed
- Mobile/bandwidth constrained
- Flexible field selection needed

### Sample Expected Results

```
╔════════════════════════════════════════════════════════════════╗
║                    EXPECTED OUTCOMES                            ║
╠════════════════════════════════════════════════════════════════╣
║ Scenario: Simple GET                                           ║
║   REST:    15-20ms avg response                                ║
║   GraphQL: 18-25ms avg response                                ║
║   Winner:  REST (slightly faster for simple queries)           ║
╠════════════════════════════════════════════════════════════════╣
║ Scenario: Nested Data (3 levels)                               ║
║   REST:    40-50ms, 5-8KB payload (over-fetching)              ║
║   GraphQL: 35-45ms, 2-3KB payload (exact data)                 ║
║   Winner:  GraphQL (smaller payload, similar speed)            ║
╠════════════════════════════════════════════════════════════════╣
║ Scenario: Multiple Resources                                   ║
║   REST:    3 requests, 150-200ms total, 3x bandwidth           ║
║   GraphQL: 1 request, 50-70ms total, 1x bandwidth              ║
║   Winner:  GraphQL (3x faster, 3x less bandwidth)              ║
╠════════════════════════════════════════════════════════════════╣
║ Scenario: Dashboard Aggregation                                ║
║   REST:    45-60ms (returns all data)                          ║
║   GraphQL: 40-55ms (returns only requested fields)             ║
║   Winner:  GraphQL (flexible field selection)                  ║
╠════════════════════════════════════════════════════════════════╣
║ Scenario: Bulk Operations                                      ║
║   REST:    Similar performance                                 ║
║   GraphQL: Similar performance                                 ║
║   Winner:  Tie (both handle efficiently)                       ║
╚════════════════════════════════════════════════════════════════╝
```

## 🎯 Decision Matrix

Based on your KPI/NFR measurements, use this decision matrix:

| Requirement | REST Score | GraphQL Score | Recommendation |
|-------------|-----------|---------------|----------------|
| **Simple CRUD** | 9/10 | 7/10 | REST |
| **Complex Queries** | 6/10 | 9/10 | GraphQL |
| **Mobile Apps** | 5/10 | 9/10 | GraphQL |
| **Caching** | 9/10 | 6/10 | REST |
| **Real-time** | 5/10 | 9/10 | GraphQL (subscriptions) |
| **Team Experience** | 9/10 | 5/10 | Depends on team |
| **Bandwidth Critical** | 6/10 | 9/10 | GraphQL |
| **Tooling/Ecosystem** | 9/10 | 7/10 | REST |
| **Type Safety** | 7/10 | 8/10 | GraphQL |
| **Flexibility** | 6/10 | 10/10 | GraphQL |

## 📋 Checklist for Complete NFR Assessment

- [ ] Run load tests with varying concurrency levels
- [ ] Measure response times (P50, P95, P99)
- [ ] Compare payload sizes
- [ ] Calculate throughput (req/s)
- [ ] Verify success rates > 99%
- [ ] Test error handling
- [ ] Analyze memory usage
- [ ] Check CPU utilization
- [ ] Measure bandwidth consumption
- [ ] Evaluate developer experience
- [ ] Review code maintainability
- [ ] Generate HTML report
- [ ] Document findings
- [ ] Make recommendation

## 🚀 Next Steps

1. Run baseline tests with current load
2. Identify bottlenecks
3. Optimize critical paths
4. Re-test and compare
5. Scale testing to production-like loads
6. Make informed decision: REST, GraphQL, or Both

---

**All metrics are automatically collected and available at:**
- `http://localhost:5072/api/metrics/report` (HTML)
- `http://localhost:5072/api/metrics/comparison` (JSON)
