# Front-End Demo Guide

## Overview
Interactive web-based demo showing REST vs GraphQL API comparisons with live examples.

**URL**: `http://localhost:5072/` (default page)

---

## 🎯 Features

### 1. **Side-by-Side Comparison**
- REST API on the left
- GraphQL API on the right
- Real-time performance metrics for both

### 2. **Bulk Operations Demo**

#### **Bulk Create Orders**
- Configure number of orders (1-100)
- Automatically generates sample data
- Shows response time, request count, data size
- Works for both REST and GraphQL

#### **Bulk Delete Orders**
- Enter comma-separated order IDs
- Validates and deletes multiple orders
- Returns success/failure counts
- Shows deleted IDs

### 3. **GraphQL Query Examples**

Pre-built query templates:
- **Simple Query**: List customers with basic fields
- **Nested Query**: 4-level deep object graph (Order → Items → Product → Category)
- **Dashboard**: Aggregations with top products and recent orders
- **Multiple Resources**: Query customers, products, and orders in one request

### 4. **Custom Query Editor**
- Write your own GraphQL queries
- Execute directly from the browser
- See formatted JSON responses
- Test any query from the schema

---

## 📊 Performance Metrics

Each API call displays:
- **Response Time**: Milliseconds (ms)
- **Requests Made**: Number of HTTP calls
- **Data Size**: Response payload size (B/KB/MB)

---

## 🚀 Usage

### Starting the Demo

```powershell
# 1. Start the API server
.\start-api.ps1

# 2. Open browser
# Navigate to: http://localhost:5072/
```

### Example Workflow

1. **Try Bulk Create**
   - Set number of orders (e.g., 10)
   - Click "Create Orders (REST)" or "Create Orders (GraphQL)"
   - Compare response times and data sizes

2. **Try Bulk Delete**
   - Get order IDs from create response (e.g., `101,102,103,104,105`)
   - Paste into delete field
   - Click "Delete Orders (REST)" or "Delete Orders (GraphQL)"
   - View deleted IDs in response

3. **Explore GraphQL Queries**
   - Click "Nested Query" to load a complex query template
   - Click "Execute Query" to run it
   - See complete 4-level nested data in one request

4. **Compare Performance**
   - Scroll down to "Performance Comparison" table
   - See request count reductions for different scenarios
   - Notice 67-75% improvement in multi-resource queries

---

## 🎨 UI Components

### REST Card (Pink/Purple Gradient)
- Bulk create input and button
- Bulk delete input and button
- Response viewer
- Performance stats (time, requests, size)

### GraphQL Card (Blue Gradient)
- Identical operations to REST
- Same metrics for direct comparison
- Shows power of single-request operations

### Query Examples Card
- Template buttons for quick queries
- Custom query editor (textarea)
- Execute button
- Response viewer

### Performance Comparison Table
- Shows all 5 scenarios
- REST vs GraphQL request counts
- Percentage improvements
- Highlights GraphQL advantages

---

## 💡 Demo Tips

### For Business Stakeholders
1. Focus on the **Performance Comparison** table
2. Demonstrate **Bulk Create** side-by-side
3. Show **Nested Query** reducing 4 calls to 1
4. Highlight **response time improvements**

### For Technical Audiences
1. Show the **custom query editor**
2. Demonstrate **flexible field selection**
3. Explain **no over-fetching** (smaller payloads)
4. Show **GraphQL schema introspection** at `/graphql`

### For Demos
1. Pre-populate delete IDs before demo
2. Use Quick Bulk Test (5-10 orders) for fast results
3. Keep browser DevTools open to show network activity
4. Have Banana Cake Pop IDE ready for schema exploration

---

## 🔧 Technical Details

### Frontend Stack
- **Pure HTML/CSS/JavaScript** (no frameworks)
- **Fetch API** for HTTP requests
- **Responsive design** (mobile-friendly)
- **No build process required**

### API Integration
- REST: `http://localhost:5072/api/orders/bulk`
- GraphQL: `http://localhost:5072/graphql`
- CORS enabled for local development

### Performance Measurement
- Uses `performance.now()` for accurate timing
- Measures from request start to response parsing
- Calculates payload size from JSON stringification

---

## 🎯 Use Cases

### 1. Client Presentations
- Show live API calls
- Demonstrate real performance gains
- Interactive Q&A with custom queries

### 2. Stakeholder Reviews
- Visual comparison of technologies
- Quantifiable metrics (%, ms, KB)
- Professional gradient UI design

### 3. Developer Onboarding
- Learn GraphQL query syntax
- See REST vs GraphQL patterns
- Understand bulk operations

### 4. Testing & Validation
- Quick manual testing of bulk operations
- Verify API responses
- Explore schema capabilities

---

## 🐛 Troubleshooting

### Demo Page Not Loading
```powershell
# Ensure API is running
.\start-api.ps1

# Verify at: http://localhost:5072/
```

### CORS Errors
- Already configured in `Program.cs`
- Restart API if changes were made

### API Not Responding
```powershell
# Check if port 5072 is in use
netstat -ano | findstr :5072

# Kill process if needed
taskkill /PID <PID> /F
```

### GraphQL Errors
- Check query syntax in editor
- View errors in response panel
- Use `/graphql` IDE for schema reference

---

## 📚 Related Documentation

- **[GRAPHQL_SCHEMA.md](GRAPHQL_SCHEMA.md)** - Complete GraphQL API reference
- **[PERFORMANCE_TESTING.md](PERFORMANCE_TESTING.md)** - Automated testing guide
- **[README.md](../README.md)** - Project overview and quick start

---

## ✅ Quick Reference

| Feature | URL | Description |
|---------|-----|-------------|
| **Demo Page** | http://localhost:5072/ | Main interactive demo |
| **GraphQL IDE** | http://localhost:5072/graphql | Banana Cake Pop IDE |
| **Swagger API** | http://localhost:5072/swagger | REST API documentation |
| **Performance Report** | http://localhost:5072/api/metrics/report | Automated test results |

---

**Last Updated**: December 2024  
**Version**: 1.0  
**Status**: ✅ Complete
