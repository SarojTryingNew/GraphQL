import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { OperationMetrics, KpiMetrics, ApiMetrics, OperationTypeMetrics } from '../models/order.models';

@Injectable({
  providedIn: 'root'
})
export class MetricsService {
  private metrics: OperationMetrics[] = [];
  private metricsSubject = new BehaviorSubject<OperationMetrics[]>([]);
  
  metrics$ = this.metricsSubject.asObservable();

  recordOperation(metric: OperationMetrics): void {
    this.metrics.push(metric);
    this.metricsSubject.next([...this.metrics]);
  }

  clearMetrics(): void {
    this.metrics = [];
    this.metricsSubject.next([]);
  }

  getKpiMetrics(): KpiMetrics {
    if (this.metrics.length === 0) {
      return this.getEmptyKpi();
    }

    const successfulOps = this.metrics.filter(m => m.success);
    const durations = this.metrics.map(m => m.duration).sort((a, b) => a - b);
    
    const restMetrics = this.metrics.filter(m => m.apiType === 'REST');
    const graphqlMetrics = this.metrics.filter(m => m.apiType === 'GraphQL');

    const timeRange = this.metrics.length > 0 
      ? (this.metrics[this.metrics.length - 1].endTime - this.metrics[0].startTime) / 1000
      : 1;

    return {
      totalOperations: this.metrics.length,
      successRate: (successfulOps.length / this.metrics.length) * 100,
      averageResponseTime: this.average(durations),
      p95ResponseTime: this.percentile(durations, 95),
      p99ResponseTime: this.percentile(durations, 99),
      totalDataTransferred: this.metrics.reduce((sum, m) => sum + m.requestSize + m.responseSize, 0),
      operationsPerSecond: this.metrics.length / timeRange,
      restVsGraphql: {
        rest: this.calculateApiMetrics(restMetrics),
        graphql: this.calculateApiMetrics(graphqlMetrics)
      },
      byOperation: {
        create: this.calculateOperationMetrics('create'),
        update: this.calculateOperationMetrics('update'),
        delete: this.calculateOperationMetrics('delete'),
        get: this.calculateOperationMetrics('get')
      }
    };
  }

  private calculateApiMetrics(metrics: OperationMetrics[]): ApiMetrics {
    if (metrics.length === 0) {
      return {
        totalRequests: 0,
        successCount: 0,
        failureCount: 0,
        averageResponseTime: 0,
        p95ResponseTime: 0,
        averageRequestSize: 0,
        averageResponseSize: 0
      };
    }

    const durations = metrics.map(m => m.duration).sort((a, b) => a - b);
    const successCount = metrics.filter(m => m.success).length;

    return {
      totalRequests: metrics.length,
      successCount,
      failureCount: metrics.length - successCount,
      averageResponseTime: this.average(durations),
      p95ResponseTime: this.percentile(durations, 95),
      averageRequestSize: this.average(metrics.map(m => m.requestSize)),
      averageResponseSize: this.average(metrics.map(m => m.responseSize))
    };
  }

  private calculateOperationMetrics(operationType: string): OperationTypeMetrics {
    const ops = this.metrics.filter(m => m.operationType === operationType);
    
    if (ops.length === 0) {
      return {
        count: 0,
        averageTime: 0,
        successRate: 0
      };
    }

    const successCount = ops.filter(m => m.success).length;

    return {
      count: ops.length,
      averageTime: this.average(ops.map(m => m.duration)),
      successRate: (successCount / ops.length) * 100
    };
  }

  private average(numbers: number[]): number {
    if (numbers.length === 0) return 0;
    return numbers.reduce((sum, n) => sum + n, 0) / numbers.length;
  }

  private percentile(sortedNumbers: number[], percentile: number): number {
    if (sortedNumbers.length === 0) return 0;
    const index = Math.ceil((percentile / 100) * sortedNumbers.length) - 1;
    return sortedNumbers[Math.max(0, index)];
  }

  private getEmptyKpi(): KpiMetrics {
    return {
      totalOperations: 0,
      successRate: 0,
      averageResponseTime: 0,
      p95ResponseTime: 0,
      p99ResponseTime: 0,
      totalDataTransferred: 0,
      operationsPerSecond: 0,
      restVsGraphql: {
        rest: {
          totalRequests: 0,
          successCount: 0,
          failureCount: 0,
          averageResponseTime: 0,
          p95ResponseTime: 0,
          averageRequestSize: 0,
          averageResponseSize: 0
        },
        graphql: {
          totalRequests: 0,
          successCount: 0,
          failureCount: 0,
          averageResponseTime: 0,
          p95ResponseTime: 0,
          averageRequestSize: 0,
          averageResponseSize: 0
        }
      },
      byOperation: {
        create: { count: 0, averageTime: 0, successRate: 0 },
        update: { count: 0, averageTime: 0, successRate: 0 },
        delete: { count: 0, averageTime: 0, successRate: 0 },
        get: { count: 0, averageTime: 0, successRate: 0 }
      }
    };
  }

  exportMetrics(): string {
    return JSON.stringify(this.metrics, null, 2);
  }

  getComparisonData(): any {
    const kpi = this.getKpiMetrics();
    
    return {
      responseTime: {
        rest: kpi.restVsGraphql.rest.averageResponseTime,
        graphql: kpi.restVsGraphql.graphql.averageResponseTime,
        winner: kpi.restVsGraphql.rest.averageResponseTime < kpi.restVsGraphql.graphql.averageResponseTime ? 'REST' : 'GraphQL'
      },
      requestSize: {
        rest: kpi.restVsGraphql.rest.averageRequestSize,
        graphql: kpi.restVsGraphql.graphql.averageRequestSize,
        winner: kpi.restVsGraphql.rest.averageRequestSize < kpi.restVsGraphql.graphql.averageRequestSize ? 'REST' : 'GraphQL'
      },
      responseSize: {
        rest: kpi.restVsGraphql.rest.averageResponseSize,
        graphql: kpi.restVsGraphql.graphql.averageResponseSize,
        winner: kpi.restVsGraphql.rest.averageResponseSize < kpi.restVsGraphql.graphql.averageResponseSize ? 'REST' : 'GraphQL'
      },
      successRate: {
        rest: kpi.restVsGraphql.rest.totalRequests > 0 
          ? (kpi.restVsGraphql.rest.successCount / kpi.restVsGraphql.rest.totalRequests) * 100 
          : 0,
        graphql: kpi.restVsGraphql.graphql.totalRequests > 0 
          ? (kpi.restVsGraphql.graphql.successCount / kpi.restVsGraphql.graphql.totalRequests) * 100 
          : 0
      }
    };
  }
}
