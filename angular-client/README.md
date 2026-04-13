# Angular REST vs GraphQL Bulk Operations Client

## 🚀 Overview

This Angular application provides a comprehensive client-side interface for testing and comparing REST vs GraphQL bulk operations with detailed KPI reporting.

## 📋 Features

- **Bulk Operations Tabs**
  - ✅ Bulk Create Orders
  - 🔄 Bulk Update Orders
  - ❌ Bulk Delete Orders
  - 📋 Get All Orders / Get by IDs

- **Real-time Metrics**
  - Client-side performance tracking
  - Response time measurements
  - Data transfer analytics
  - Success rate monitoring

- **KPI Dashboard**
  - REST vs GraphQL comparison charts
  - Operation type breakdown
  - P95/P99 latency percentiles
  - Throughput metrics
  - Visual charts and graphs

## 🛠️ Installation

```bash
cd C:\Repo\GraphQL\angular-client

# Install dependencies
npm install

# Additional packages needed
npm install chart.js ng2-charts @angular/material @angular/cdk
```

## 📦 Project Structure

```
angular-client/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── services/
│   │   │   │   ├── rest.service.ts           # REST API service
│   │   │   │   ├── graphql.service.ts        # GraphQL service
│   │   │   │   └── metrics.service.ts        # Client-side metrics tracking
│   │   │   └── models/
│   │   │       └── order.models.ts           # TypeScript interfaces
│   │   ├── features/
│   │   │   ├── bulk-operations/              # Main operations component
│   │   │   └── kpi-dashboard/                # KPI reporting dashboard
│   │   └── app.component.ts                  # Root component with tabs
│   └── environments/
│       └── environment.ts                    # API configuration
```

## 🔧 Configuration

Update `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5072',
  graphqlEndpoint: 'http://localhost:5072/graphql'
};
```

## 🎯 Usage

### 1. Start the .NET Backend

```bash
cd C:\Repo\GraphQL\RestVsGraphQL
dotnet run
```

### 2. Start the Angular App

```bash
cd C:\Repo\GraphQL\angular-client
ng serve
```

Navigate to `http://localhost:4200`

## 📊 Components to Create

### Main App Component (`src/app/app.component.ts`)

```typescript
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <div class="app-container">
      <header>
        <h1>🚀 REST vs GraphQL - Bulk Operations Client</h1>
        <p>Client-side Performance Comparison Dashboard</p>
      </header>

      <mat-tab-group>
        <mat-tab label="📊 KPI Dashboard">
          <app-kpi-dashboard></app-kpi-dashboard>
        </mat-tab>
        
        <mat-tab label="✅ Bulk Create">
          <app-bulk-create></app-bulk-create>
        </mat-tab>
        
        <mat-tab label="🔄 Bulk Update">
          <app-bulk-update></app-bulk-update>
        </mat-tab>
        
        <mat-tab label="❌ Bulk Delete">
          <app-bulk-delete></app-bulk-delete>
        </mat-tab>
        
        <mat-tab label="📋 Get Orders">
          <app-bulk-get></app-bulk-get>
        </mat-tab>
      </mat-tab-group>
    </div>
  `,
  styles: [`
    .app-container {
      padding: 20px;
      max-width: 1400px;
      margin: 0 auto;
    }
    
    header {
      text-align: center;
      margin-bottom: 30px;
    }
    
    h1 {
      color: #667eea;
      margin-bottom: 10px;
    }
  `]
})
export class AppComponent {
  title = 'REST vs GraphQL Client';
}
```

### KPI Dashboard Component (`src/app/features/kpi-dashboard/kpi-dashboard.component.ts`)

```typescript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MetricsService } from '../../core/services/metrics.service';
import { KpiMetrics } from '../../core/models/order.models';
import { ChartConfiguration } from 'chart.js';

@Component({
  selector: 'app-kpi-dashboard',
  template: `
    <div class="dashboard-container">
      <div class="controls">
        <button mat-raised-button color="warn" (click)="clearMetrics()">
          🗑️ Clear Metrics
        </button>
        <button mat-raised-button color="primary" (click)="exportMetrics()">
          📥 Export Data
        </button>
      </div>

      <div class="metrics-grid">
        <!-- Summary Cards -->
        <mat-card class="metric-card">
          <mat-card-header>
            <mat-card-title>Total Operations</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="metric-value">{{ kpi.totalOperations }}</div>
          </mat-card-content>
        </mat-card>

        <mat-card class="metric-card">
          <mat-card-header>
            <mat-card-title>Success Rate</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="metric-value">{{ kpi.successRate | number:'1.2-2' }}%</div>
          </mat-card-content>
        </mat-card>

        <mat-card class="metric-card">
          <mat-card-header>
            <mat-card-title>Avg Response Time</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="metric-value">{{ kpi.averageResponseTime | number:'1.2-2' }}ms</div>
          </mat-card-content>
        </mat-card>

        <mat-card class="metric-card">
          <mat-card-header>
            <mat-card-title>Throughput</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="metric-value">{{ kpi.operationsPerSecond | number:'1.2-2' }} ops/sec</div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- REST vs GraphQL Comparison -->
      <div class="comparison-section">
        <h2>REST vs GraphQL Comparison</h2>
        
        <div class="charts-grid">
          <mat-card>
            <mat-card-header>
              <mat-card-title>Response Time Comparison</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <canvas baseChart
                [data]="responseTimeChartData"
                [options]="chartOptions"
                [type]="'bar'">
              </canvas>
            </mat-card-content>
          </mat-card>

          <mat-card>
            <mat-card-header>
              <mat-card-title>Request Count</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <canvas baseChart
                [data]="requestCountChartData"
                [options]="chartOptions"
                [type]="'bar'">
              </canvas>
            </mat-card-content>
          </mat-card>

          <mat-card>
            <mat-card-header>
              <mat-card-title>Data Transfer (bytes)</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <canvas baseChart
                [data]="dataTransferChartData"
                [options]="chartOptions"
                [type]="'bar'">
              </canvas>
            </mat-card-content>
          </mat-card>

          <mat-card>
            <mat-card-header>
              <mat-card-title>Operations Breakdown</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <canvas baseChart
                [data]="operationsChartData"
                [type]="'pie'">
              </canvas>
            </mat-card-content>
          </mat-card>
        </div>
      </div>

      <!-- Detailed Metrics Tables -->
      <div class="tables-section">
        <h2>Detailed Metrics</h2>
        
        <table mat-table [dataSource]="comparisonTableData" class="comparison-table">
          <ng-container matColumnDef="metric">
            <th mat-header-cell *matHeaderCellDef>Metric</th>
            <td mat-cell *matCellDef="let row">{{ row.metric }}</td>
          </ng-container>

          <ng-container matColumnDef="rest">
            <th mat-header-cell *matHeaderCellDef>REST</th>
            <td mat-cell *matCellDef="let row">{{ row.rest }}</td>
          </ng-container>

          <ng-container matColumnDef="graphql">
            <th mat-header-cell *matHeaderCellDef>GraphQL</th>
            <td mat-cell *matCellDef="let row">{{ row.graphql }}</td>
          </ng-container>

          <ng-container matColumnDef="winner">
            <th mat-header-cell *matHeaderCellDef>Winner</th>
            <td mat-cell *matCellDef="let row" [class.winner]="true">{{ row.winner }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 20px;
    }

    .controls {
      display: flex;
      gap: 10px;
      margin-bottom: 20px;
    }

    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 20px;
      margin-bottom: 30px;
    }

    .metric-card {
      text-align: center;
    }

    .metric-value {
      font-size: 2.5em;
      font-weight: bold;
      color: #667eea;
      margin-top: 10px;
    }

    .comparison-section {
      margin-bottom: 30px;
    }

    .charts-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 20px;
      margin-top: 20px;
    }

    .comparison-table {
      width: 100%;
      margin-top: 20px;
    }

    .winner {
      color: #4caf50;
      font-weight: bold;
    }

    h2 {
      color: #667eea;
      margin: 20px 0;
    }
  `]
})
export class KpiDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  kpi: KpiMetrics;
  displayedColumns = ['metric', 'rest', 'graphql', 'winner'];
  comparisonTableData: any[] = [];

  responseTimeChartData: ChartConfiguration['data'] = {
    labels: ['REST', 'GraphQL'],
    datasets: [
      {
        data: [0, 0],
        label: 'Avg Response Time (ms)',
        backgroundColor: ['#f093fb', '#4facfe']
      }
    ]
  };

  requestCountChartData: ChartConfiguration['data'] = {
    labels: ['REST', 'GraphQL'],
    datasets: [
      {
        data: [0, 0],
        label: 'Total Requests',
        backgroundColor: ['#f093fb', '#4facfe']
      }
    ]
  };

  dataTransferChartData: ChartConfiguration['data'] = {
    labels: ['REST', 'GraphQL'],
    datasets: [
      {
        data: [0, 0],
        label: 'Avg Data Transfer (bytes)',
        backgroundColor: ['#f093fb', '#4facfe']
      }
    ]
  };

  operationsChartData: ChartConfiguration['data'] = {
    labels: ['Create', 'Update', 'Delete', 'Get'],
    datasets: [
      {
        data: [0, 0, 0, 0],
        backgroundColor: ['#667eea', '#764ba2', '#f093fb', '#4facfe']
      }
    ]
  };

  chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false
  };

  constructor(private metricsService: MetricsService) {
    this.kpi = this.metricsService.getKpiMetrics();
  }

  ngOnInit(): void {
    this.metricsService.metrics$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateDashboard();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  updateDashboard(): void {
    this.kpi = this.metricsService.getKpiMetrics();
    this.updateCharts();
    this.updateComparisonTable();
  }

  updateCharts(): void {
    // Response Time Chart
    this.responseTimeChartData.datasets[0].data = [
      this.kpi.restVsGraphql.rest.averageResponseTime,
      this.kpi.restVsGraphql.graphql.averageResponseTime
    ];

    // Request Count Chart
    this.requestCountChartData.datasets[0].data = [
      this.kpi.restVsGraphql.rest.totalRequests,
      this.kpi.restVsGraphql.graphql.totalRequests
    ];

    // Data Transfer Chart
    this.dataTransferChartData.datasets[0].data = [
      this.kpi.restVsGraphql.rest.averageRequestSize + this.kpi.restVsGraphql.rest.averageResponseSize,
      this.kpi.restVsGraphql.graphql.averageRequestSize + this.kpi.restVsGraphql.graphql.averageResponseSize
    ];

    // Operations Chart
    this.operationsChartData.datasets[0].data = [
      this.kpi.byOperation.create.count,
      this.kpi.byOperation.update.count,
      this.kpi.byOperation.delete.count,
      this.kpi.byOperation.get.count
    ];
  }

  updateComparisonTable(): void {
    const comparison = this.metricsService.getComparisonData();
    
    this.comparisonTableData = [
      {
        metric: 'Avg Response Time',
        rest: `${this.kpi.restVsGraphql.rest.averageResponseTime.toFixed(2)}ms`,
        graphql: `${this.kpi.restVsGraphql.graphql.averageResponseTime.toFixed(2)}ms`,
        winner: comparison.responseTime.winner
      },
      {
        metric: 'P95 Response Time',
        rest: `${this.kpi.restVsGraphql.rest.p95ResponseTime.toFixed(2)}ms`,
        graphql: `${this.kpi.restVsGraphql.graphql.p95ResponseTime.toFixed(2)}ms`,
        winner: this.kpi.restVsGraphql.rest.p95ResponseTime < this.kpi.restVsGraphql.graphql.p95ResponseTime ? 'REST' : 'GraphQL'
      },
      {
        metric: 'Success Rate',
        rest: `${comparison.successRate.rest.toFixed(2)}%`,
        graphql: `${comparison.successRate.graphql.toFixed(2)}%`,
        winner: comparison.successRate.rest > comparison.successRate.graphql ? 'REST' : 'GraphQL'
      },
      {
        metric: 'Total Requests',
        rest: this.kpi.restVsGraphql.rest.totalRequests.toString(),
        graphql: this.kpi.restVsGraphql.graphql.totalRequests.toString(),
        winner: '-'
      }
    ];
  }

  clearMetrics(): void {
    if (confirm('Are you sure you want to clear all metrics?')) {
      this.metricsService.clearMetrics();
    }
  }

  exportMetrics(): void {
    const data = this.metricsService.exportMetrics();
    const blob = new Blob([data], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `metrics-${new Date().toISOString()}.json`;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
```

### Bulk Create Component Example

Create this for each operation type (create, update, delete, get).

## 📈 KPI Metrics Tracked

### Client-Side Metrics:
- ⏱️ **Response Time**: Measured using `performance.now()`
- 📦 **Request/Response Size**: JSON serialization size
- ✅ **Success Rate**: Percentage of successful operations
- 🚀 **Throughput**: Operations per second
- 📊 **Percentiles**: P95, P99 latency

### Comparison Metrics:
- REST vs GraphQL response times
- REST vs GraphQL data transfer
- Operation type breakdown
- Real-time visual charts

## 🎨 Features

1. **Tab-based Navigation**: Easy switching between operations
2. **Side-by-Side Execution**: Run REST and GraphQL simultaneously
3. **Real-time Charts**: Live updating visualizations
4. **Export Capability**: Download metrics as JSON
5. **Detailed Reporting**: Comprehensive KPI dashboard

## 🚦 Running the Full Stack

```bash
# Terminal 1: Backend
cd C:\Repo\GraphQL\RestVsGraphQL
dotnet run

# Terminal 2: Angular Frontend
cd C:\Repo\GraphQL\angular-client
ng serve

# Open browser
http://localhost:4200
```

## 📝 Next Steps

1. Create individual components for each bulk operation
2. Add form validation
3. Implement error handling UI
4. Add loading indicators
5. Create responsive design for mobile

Would you like me to create the complete component files for all bulk operations?
