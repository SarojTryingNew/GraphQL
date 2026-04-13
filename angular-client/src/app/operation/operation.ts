import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, forkJoin } from 'rxjs';
import { RestService } from '../core/services/rest.service';
import { GraphqlService } from '../core/services/graphql.service';
import { RestGraphQLBackendService } from '../core/services/rest-graphql-backend.service';
import { MetricsService } from '../core/services/metrics.service';
import { BulkCreateRequest, BulkUpdateRequest, BulkDeleteRequest } from '../core/models/order.models';

interface SingleResult {
  duration: number;
  requestSize: number;
  responseSize: number;
  success: boolean;
  itemCount: number;
  error?: string;
  requestPayload?: any;
  responsePayload?: any;
}

interface ComparisonMetric {
  label: string;
  restDirectValue: string;
  restGraphQLValue: string;
  winner: 'REST Direct' | 'REST+GraphQL' | 'Tie';
}

interface OperationResult {
  restDirect: SingleResult;
  restWithGraphQL: SingleResult;
  winner: 'REST Direct' | 'REST+GraphQL' | 'Tie';
  winnerReason: string;
  metrics: ComparisonMetric[];
}

@Component({
  selector: 'app-operation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operation.html',
  styleUrl: './operation.scss'
})
export class OperationComponent implements OnInit {
  operationType: 'create' | 'update' | 'delete' | 'get' = 'create';
  operationConfig: any;
  itemCount = 5;
  running = false;
  result: OperationResult | null = null;
  error: string | null = null;
  
  // Store created order IDs for reuse in update/delete
  private createdOrderIds: number[] = [];

  readonly configs: Record<string, any> = {
    create: {
      label: 'Bulk Create Orders',
      icon: '✅',
      description: 'Create multiple orders - comparing REST Direct vs REST with GraphQL backend',
      defaultCount: 5,
      minCount: 1,
      maxCount: 100,
      helpText: 'Each order will have random customer and product assignments. Tests both REST approaches.'
    },
    update: {
      label: 'Bulk Update Orders',
      icon: '🔄',
      description: 'Update multiple existing orders - comparing REST Direct vs REST with GraphQL backend',
      defaultCount: 5,
      minCount: 1,
      maxCount: 50,
      helpText: 'Uses IDs from previous CREATE operation (run CREATE first for best results)'
    },
    delete: {
      label: 'Bulk Delete Orders',
      icon: '🗑️',
      description: 'Delete multiple orders - comparing REST Direct vs REST with GraphQL backend',
      defaultCount: 5,
      minCount: 1,
      maxCount: 50,
      helpText: 'Uses IDs from previous CREATE operation (run CREATE first for best results)'
    },
    get: {
      label: 'Bulk Get Orders',
      icon: '📋',
      description: 'Retrieve all orders - comparing REST Direct vs REST with GraphQL backend',
      defaultCount: 0,
      minCount: 0,
      maxCount: 0,
      helpText: 'Fetches all orders including customer and product details using both approaches'
    }
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private restService: RestService,
    private restGraphQLBackend: RestGraphQLBackendService,
    private graphqlService: GraphqlService,
    private metricsService: MetricsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const type = this.route.snapshot.paramMap.get('type') as any;
    if (type && this.configs[type]) {
      this.operationType = type;
      this.operationConfig = this.configs[type];
      this.itemCount = this.operationConfig.defaultCount;
    } else {
      this.router.navigate(['/']);
    }
  }

  async runComparison(): Promise<void> {
    this.running = true;
    this.result = null;
    this.error = null;
    this.cdr.detectChanges();

    try {
      let restDirectResult!: SingleResult;
      let restGraphQLResult!: SingleResult;

      if (this.operationType === 'get') {
        // Run both in parallel
        [restDirectResult, restGraphQLResult] = await Promise.all([
          this.runMeasured(null, () => firstValueFrom(this.restService.getAllOrders())),
          this.runMeasured(null, () => firstValueFrom(this.restGraphQLBackend.getAllOrders()))
        ]);

      } else if (this.operationType === 'create') {
        const restReq = this.buildCreateRequest(this.itemCount);
        const restGqlReq = this.buildCreateRequest(this.itemCount);

        // Run both in parallel
        [restDirectResult, restGraphQLResult] = await Promise.all([
          this.runMeasured(restReq, () => firstValueFrom(this.restService.bulkCreateOrders(restReq))),
          this.runMeasured(restGqlReq, () => firstValueFrom(this.restGraphQLBackend.bulkCreateOrders(restGqlReq)))
        ]);

        // Store created IDs for future update/delete operations
        const restIds = restDirectResult.responsePayload?.createdIds || [];
        const gqlIds = restGraphQLResult.responsePayload?.createdIds || [];
        this.createdOrderIds = [...restIds, ...gqlIds];
        console.log(`[Create] Stored ${this.createdOrderIds.length} order IDs for future operations:`, this.createdOrderIds);

      } else if (this.operationType === 'update') {
        // UPDATE OPERATION
        const totalNeeded = this.itemCount * 2;
        let ids: number[];

        if (this.createdOrderIds.length >= totalNeeded) {
          ids = this.createdOrderIds.slice(0, totalNeeded);
          console.log(`[Update] Using ${ids.length} stored order IDs from previous create:`, ids);
        } else {
          ids = Array.from({ length: totalNeeded }, (_, i) => i + 1);
          console.log(`[Update] No stored IDs available. Using sequential IDs 1-${totalNeeded}. Run CREATE first for better accuracy.`);
        }

        const half = Math.ceil(ids.length / 2);
        const restReq: BulkUpdateRequest = {
          orders: ids.slice(0, half).map(id => ({ id: id, status: 'Shipped' }))
        };
        const restGqlReq: BulkUpdateRequest = {
          orders: ids.slice(half).map(id => ({ id: id, status: 'Shipped' }))
        };

        // Run both in parallel
        [restDirectResult, restGraphQLResult] = await Promise.all([
          this.runMeasured(restReq, () => firstValueFrom(this.restService.bulkUpdateOrders(restReq))),
          this.runMeasured(restGqlReq, () => firstValueFrom(this.restGraphQLBackend.bulkUpdateOrders(restGqlReq)))
        ]);

      } else {
        // DELETE OPERATION
        const totalNeeded = this.itemCount * 2;
        let ids: number[];

        if (this.createdOrderIds.length >= totalNeeded) {
          ids = this.createdOrderIds.splice(0, totalNeeded);
          console.log(`[Delete] Using ${ids.length} stored order IDs from previous create:`, ids);
          console.log(`[Delete] Remaining stored IDs: ${this.createdOrderIds.length}`);
        } else {
          ids = Array.from({ length: totalNeeded }, (_, i) => i + 1);
          console.log(`[Delete] No stored IDs available. Using sequential IDs 1-${totalNeeded}. Run CREATE first for better accuracy.`);
        }

        const half = Math.ceil(ids.length / 2);
        const restReq: BulkDeleteRequest = { orderIds: ids.slice(0, half) };
        const restGqlReq: BulkDeleteRequest = { orderIds: ids.slice(half) };

        // Run both in parallel
        [restDirectResult, restGraphQLResult] = await Promise.all([
          this.runMeasured(restReq, () => firstValueFrom(this.restService.bulkDeleteOrders(restReq))),
          this.runMeasured(restGqlReq, () => firstValueFrom(this.restGraphQLBackend.bulkDeleteOrders(restGqlReq)))
        ]);
      }

      const now = Date.now();
      this.metricsService.recordOperation({
        operationType: this.operationType,
        apiType: 'REST Direct',
        startTime: now - restDirectResult.duration,
        endTime: now,
        duration: restDirectResult.duration,
        requestSize: restDirectResult.requestSize,
        responseSize: restDirectResult.responseSize,
        success: restDirectResult.success,
        itemCount: restDirectResult.itemCount,
        error: restDirectResult.error
      });
      this.metricsService.recordOperation({
        operationType: this.operationType,
        apiType: 'REST+GraphQL',
        startTime: now,
        endTime: now + restGraphQLResult.duration,
        duration: restGraphQLResult.duration,
        requestSize: restGraphQLResult.requestSize,
        responseSize: restGraphQLResult.responseSize,
        success: restGraphQLResult.success,
        itemCount: restGraphQLResult.itemCount,
        error: restGraphQLResult.error
      });

      this.result = this.buildResult(restDirectResult, restGraphQLResult);

    } catch (e: any) {
      this.error = e?.message || 'Unexpected error occurred';
    } finally {
      this.running = false;
      this.cdr.detectChanges();
    }
  }

  private async runMeasured(requestBody: any, fn: () => Promise<any>): Promise<SingleResult> {
    const requestSize = requestBody ? JSON.stringify(requestBody).length : 0;
    const start = performance.now();
    try {
      const response = await fn();
      const end = performance.now();
      return {
        duration: Math.round(end - start),
        requestSize,
        responseSize: JSON.stringify(response).length,
        success: true,
        itemCount: this.getItemCount(response),
        requestPayload: requestBody,
        responsePayload: response
      };
    } catch (err: any) {
      const end = performance.now();
      return {
        duration: Math.round(end - start),
        requestSize,
        responseSize: 0,
        success: false,
        itemCount: 0,
        error: err?.message || 'Error',
        requestPayload: requestBody,
        responsePayload: null
      };
    }
  }

  private getItemCount(response: any): number {
    if (Array.isArray(response)) return response.length;
    if (response?.successCount !== undefined) return response.successCount;
    return 0;
  }

  private buildCreateRequest(count: number): BulkCreateRequest {
    return {
      orders: Array.from({ length: count }, (_, i) => ({
        customerId: (i % 3) + 1,
        status: 'Pending',
        items: [
          {
            productId: (i % 3) + 1,
            quantity: Math.floor(Math.random() * 5) + 1,
            discount: Math.random() > 0.7 ? 5 : 0,
            notes: []
          }
        ]
      }))
    };
  }

  private buildResult(restDirect: SingleResult, restGraphQL: SingleResult): OperationResult {
    const metrics: ComparisonMetric[] = [
      {
        label: 'Response Time',
        restDirectValue: `${restDirect.duration} ms`,
        restGraphQLValue: `${restGraphQL.duration} ms`,
        winner: restDirect.duration < restGraphQL.duration ? 'REST Direct' : restGraphQL.duration < restDirect.duration ? 'REST+GraphQL' : 'Tie'
      },
      {
        label: 'Request Size',
        restDirectValue: this.fmtBytes(restDirect.requestSize),
        restGraphQLValue: this.fmtBytes(restGraphQL.requestSize),
        winner: restDirect.requestSize < restGraphQL.requestSize ? 'REST Direct' : restGraphQL.requestSize < restDirect.requestSize ? 'REST+GraphQL' : 'Tie'
      },
      {
        label: 'Response Size',
        restDirectValue: this.fmtBytes(restDirect.responseSize),
        restGraphQLValue: this.fmtBytes(restGraphQL.responseSize),
        winner: restDirect.responseSize < restGraphQL.responseSize ? 'REST Direct' : restGraphQL.responseSize < restDirect.responseSize ? 'REST+GraphQL' : 'Tie'
      },
      {
        label: 'Items Processed',
        restDirectValue: `${restDirect.itemCount}`,
        restGraphQLValue: `${restGraphQL.itemCount}`,
        winner: 'Tie'
      }
    ];

    const timeScore = restDirect.duration < restGraphQL.duration ? 2 : restGraphQL.duration < restDirect.duration ? -2 : 0;
    const sizeScore = restDirect.responseSize < restGraphQL.responseSize ? 1 : restGraphQL.responseSize < restDirect.responseSize ? -1 : 0;
    const total = timeScore + sizeScore;

    let winner: 'REST Direct' | 'REST+GraphQL' | 'Tie';
    let winnerReason: string;

    if (!restDirect.success && restGraphQL.success) {
      winner = 'REST+GraphQL';
      winnerReason = 'REST Direct operation failed';
    } else if (restDirect.success && !restGraphQL.success) {
      winner = 'REST Direct';
      winnerReason = 'REST+GraphQL operation failed';
    } else if (total > 0) {
      winner = 'REST Direct';
      const pct = restGraphQL.duration > 0 ? Math.round(((restGraphQL.duration - restDirect.duration) / restGraphQL.duration) * 100) : 0;
      winnerReason = `REST Direct was ${pct}% faster (${restDirect.duration} ms vs ${restGraphQL.duration} ms)`;
    } else if (total < 0) {
      winner = 'REST+GraphQL';
      const pct = restDirect.duration > 0 ? Math.round(((restDirect.duration - restGraphQL.duration) / restDirect.duration) * 100) : 0;
      winnerReason = `REST+GraphQL was ${pct}% faster (${restGraphQL.duration} ms vs ${restDirect.duration} ms)`;
    } else {
      winner = 'Tie';
      winnerReason = 'Both approaches performed equally well';
    }

    return { restDirect, restWithGraphQL: restGraphQL, winner, winnerReason, metrics };
  }

  private fmtBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    if (bytes < 1024) return `${bytes} B`;
    return `${(bytes / 1024).toFixed(1)} KB`;
  }

  get storedIdsCount(): number {
    return this.createdOrderIds.length;
  }

  get hasEnoughStoredIds(): boolean {
    const needed = this.itemCount * 2;
    return this.createdOrderIds.length >= needed;
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}
