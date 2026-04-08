# ✅ Test Scenario Names Fixed

## Problem

When running bulk tests (or other tests), the report was showing incorrect scenario names like "Quick Test (10 iterations)" instead of the actual test being run.

## Root Cause

**Timing issue**: The scenario name from a previous test was persisting and showing in the report because:

1. User runs "Quick Test" → Scenario set to "Quick Test (10 iterations)"
2. User runs "Bulk Test" → Test script tries to set new scenario
3. User views report → Sometimes sees old scenario name before new one is set

The individual test scripts (`load-test-bulk.ps1`, etc.) were setting the scenario name, but **AFTER** being called, causing a race condition.

---

## Solution

**Set scenario name BEFORE calling test scripts** in `launch-tests.ps1`.

Now the flow is:
1. User selects test from menu
2. `launch-tests.ps1` **immediately resets metrics and sets scenario name**
3. Then calls the test script
4. Opens report with correct scenario name

---

## Changes Made

### File: `launch-tests.ps1`

Updated all test options (1-6) to set scenario name before running:

#### Option 1: Quick Test
```powershell
# Set scenario BEFORE running test
$scenarioName = "Quick Test (10 iterations + 100 bulk orders)"
Invoke-RestMethod -Uri ".../api/metrics/reset" ...
Invoke-RestMethod -Uri ".../api/metrics/scenario" ...

# Then run test
& "quick-test.ps1"
```

#### Option 2: Standard Load Test
```powershell
$scenarioName = "Standard Load Test (100 iterations + 500 bulk orders)"
# Reset and set scenario first
# Then run tests
```

#### Option 3: Bulk Operations Test
```powershell
$scenarioName = "Bulk Operations Test ($iterations iterations × $ordersPerBulk orders)"
# Reset and set scenario first
# Then run test
```

#### Option 4: Nested Object Graph Test
```powershell
$scenarioName = "Nested Object Graph Test ($iterations iterations)"
# Reset and set scenario first
# Then run test
```

#### Option 5: Dashboard Aggregation Test
```powershell
$scenarioName = "Dashboard Aggregation Test ($iterations iterations)"
# Reset and set scenario first
# Then run test
```

#### Option 6: Multiple Dependent Calls Test
```powershell
$scenarioName = "Multiple Dependent Calls Test ($iterations iterations)"
# Reset and set scenario first
# Then run test
```

---

## What You'll See Now

### Before (Wrong):
```
Test Scenario: Quick Test (10 iterations)
(even though you ran bulk test!)
```

### After (Correct):
```
Test Scenario: Bulk Operations Test (50 iterations × 10 orders)
(shows the actual test you ran!)
```

---

## Benefits

✅ **Correct scenario names** always show in report  
✅ **No race conditions** - scenario set before test runs  
✅ **Metrics reset** before each test (fresh start)  
✅ **Clear feedback** - see "Test scenario set: ..." message  
✅ **Dynamic names** - include iteration counts and configuration  

---

## Test Scenario Names

Each test now has a clear, descriptive name:

| Test Option | Scenario Name |
|-------------|---------------|
| Quick Test | "Quick Test (10 iterations + 100 bulk orders)" |
| Standard Load Test | "Standard Load Test (100 iterations + 500 bulk orders)" |
| Bulk Operations (Quick) | "Bulk Operations Test (10 iterations × 10 orders)" |
| Bulk Operations (Standard) | "Bulk Operations Test (50 iterations × 10 orders)" |
| Bulk Operations (Heavy) | "Bulk Operations Test (50 iterations × 20 orders)" |
| Bulk Operations (Stress) | "Bulk Operations Test (200 iterations × 50 orders)" |
| Bulk Operations (Custom) | "Bulk Operations Test (X iterations × Y orders)" |
| Nested Graph | "Nested Object Graph Test (100 iterations)" |
| Dashboard | "Dashboard Aggregation Test (100 iterations)" |
| Multiple Calls | "Multiple Dependent Calls Test (100 iterations)" |

---

## How to Test

### Try it now:

1. **Stop and restart** launch-tests.ps1:
   ```powershell
   .\launch-tests.ps1
   ```

2. **Choose option 3** (Bulk Operations)

3. **Select any bulk test** (e.g., "1" for Quick Bulk)

4. **Watch for**:
   ```
   Preparing test...
   Test scenario set: Bulk Operations Test (10 iterations × 10 orders)
   ```

5. **View report** - should show correct scenario name!

6. **Try another test** (e.g., option 4 - Nested Graph)

7. **View report again** - should show new scenario name!

---

## Error Handling

If scenario setting fails:
```powershell
Warning: Could not set scenario
```

The test will still run, but scenario name might be "General" or from previous test. This usually happens if API isn't running.

---

## Additional Improvements

### User Feedback

Now shows clear messages:
```
Preparing test...
Test scenario set: Bulk Operations Test (50 iterations × 10 orders)
```

### Metrics Reset

Each test now automatically resets metrics before setting scenario:
- Fresh start for each test
- No carryover from previous tests
- Clean comparison data

### Silent Errors

Uses `-ErrorAction SilentlyContinue` for scenario setting:
- Won't crash if API is slow
- Shows warning if it fails
- Continues with test anyway

---

## Summary

**Problem:** Incorrect scenario names in report  
**Cause:** Race condition - scenario set too late  
**Solution:** Set scenario BEFORE calling test scripts  
**Result:** Always shows correct test scenario name! ✅

---

**Try running a bulk test now - the scenario name should be correct!** 🎉
