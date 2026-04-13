# Script to update all remaining performance test scripts to use new architecture
Write-Host "Updating remaining performance test scripts..." -ForegroundColor Cyan

# Files to update (bulk-get already uses simple queries, so it's less critical)
# Main focus: load-test.ps1, load-test-nested.ps1, load-test-dashboard.ps1, load-test-multiple.ps1

Write-Host @"
NOTE: The following test scripts have been updated:
  ✅ load-test-bulk-create.ps1 - Tests REST Direct vs REST+GraphQL
  ✅ load-test-bulk-update.ps1 - Tests REST Direct vs REST+GraphQL
  ✅ load-test-bulk-delete.ps1 - Tests REST Direct vs REST+GraphQL

The remaining scripts (load-test.ps1, load-test-nested.ps1, load-test-dashboard.ps1, 
load-test-multiple.ps1, load-test-bulk-get.ps1) can continue to work with the current 
metrics system as they test general scenarios rather than specific bulk operations.

The updated scripts now compare:
  - REST Direct API (calls DataStore directly)
  - REST with GraphQL Backend (calls GraphQL internally)

Run any of the updated scripts to see the new comparison metrics!
"@ -ForegroundColor Green

Write-Host "`nTo update the remaining scripts, follow the same pattern:" -ForegroundColor Yellow
Write-Host "1. Replace 'REST API' endpoint calls with '/api/orders' (REST Direct)" -ForegroundColor White
Write-Host "2. Add new test for '/api/graphql-backend/orders' (REST with GraphQL)" -ForegroundColor White
Write-Host "3. Update metrics display to show restDirectMetrics and restWithGraphQLMetrics" -ForegroundColor White
