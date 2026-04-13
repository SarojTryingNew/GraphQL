import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { RestService } from '../core/services/rest.service';
import { GraphqlService } from '../core/services/graphql.service';
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
  restValue: string;
  graphqlValue: string;
  winner: 'REST' | 'GraphQL' | 'Tie';
}

interface OperationResult {
  rest: SingleResult;
  graphql: SingleResult;
  winner: 'REST' | 'GraphQL' | 'Tie';
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
      description: 'Create multiple orders in a single request',
      defaultCount: 5,
      minCount: 1,
      maxCount: 100,
      helpText: 'Each order will have random customer and product assignments'
    },
    update: {
      label: 'Bulk Update Orders',
      icon: '🔄',
      description: 'Update multiple existing orders simultaneously',
      defaultCount: 5,
      minCount: 1,
      maxCount: 50,
      helpText: 'Uses IDs from previous CREATE operation (run CREATE first for best results)'
    },
    delete: {
      label: 'Bulk Delete Orders',
      icon: '🗑️',
      description: 'Delete multiple orders at once',
      defaultCount: 5,
      minCount: 1,
      maxCount: 50,
      helpText: 'Uses IDs from previous CREATE operation (run CREATE first for best results)'
    },
    get: {
      label: 'Bulk Get Orders',
      icon: '📋',
      description: 'Retrieve all orders with full nested data',
      defaultCount: 0,
      minCount: 0,
      maxCount: 0,
      helpText: 'Fetches all orders including customer and product details'
    }
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private restService: RestService,
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
      let restResult!: SingleResult;
      let graphqlResult!: SingleResult;

      if (this.operationType === 'get') {
        restResult = await this.runMeasured(null, () => firstValueFrom(this.restService.getAllOrders()));
        graphqlResult = await this.runMeasured(null, () => firstValueFrom(this.graphqlService.getAllOrders()));

      } else if (this.operationType === 'create') {
        const restReq = this.buildCreateRequest(this.itemCount);
        const gqlReq = this.buildCreateRequest(this.itemCount);
        restResult = await this.runMeasured(restReq, () => firstValueFrom(this.restService.bulkCreateOrders(restReq)));
        graphqlResult = await this.runMeasured(gqlReq, () => firstValueFrom(this.graphqlService.bulkCreateOrders(gqlReq)));
        
        // Store created IDs for future update/delete operations
        const restIds = restResult.responsePayload?.createdIds || [];
        const gqlIds = graphqlResult.responsePayload?.createdIds || [];
        this.createdOrderIds = [...restIds, ...gqlIds];
        console.log(`[Create] Stored ${this.createdOrderIds.length} order IDs for future operations:`, this.createdOrderIds);

      } else if (this.operationType === 'update') {
        // UPDATE OPERATION
        const totalNeeded = this.itemCount * 2;
        let ids: number[];
        
        if (this.createdOrderIds.length >= totalNeeded) {
          // Use stored IDs from previous CREATE operation
          ids = this.createdOrderIds.slice(0, totalNeeded);
          console.log(`[Update] Using ${ids.length} stored order IDs from previous create:`, ids);
        } else {
          // Fallback: assume sequential IDs
          ids = Array.from({ length: totalNeeded }, (_, i) => i + 1);
          console.log(`[Update] No stored IDs available. Using sequential IDs 1-${totalNeeded}. Run CREATE first for better accuracy.`);
        }
        
        const half = Math.ceil(ids.length / 2);
        const restReq: BulkUpdateRequest = {
          orders: ids.slice(0, half).map(id => ({ id: id, status: 'Processing' }))
        };
        const gqlReq: BulkUpdateRequest = {
          orders: ids.slice(half).map(id => ({ id: id, status: 'Shipped' }))
        };
        restResult = await this.runMeasured(restReq, () => firstValueFrom(this.restService.bulkUpdateOrders(restReq)));
        graphqlResult = await this.runMeasured(gqlReq, () => firstValueFrom(this.graphqlService.bulkUpdateOrders(gqlReq)));

      } else {
        // DELETE OPERATION
        const totalNeeded = this.itemCount * 2;
        let ids: number[];
        
        if (this.createdOrderIds.length >= totalNeeded) {
          // Use stored IDs from previous CREATE operation
          ids = this.createdOrderIds.splice(0, totalNeeded); // Remove used IDs
          console.log(`[Delete] Using ${ids.length} stored order IDs from previous create:`, ids);
          console.log(`[Delete] Remaining stored IDs: ${this.createdOrderIds.length}`);
        } else {
          // Fallback: assume sequential IDs
          ids = Array.from({ length: totalNeeded }, (_, i) => i + 1);
          console.log(`[Delete] No stored IDs available. Using sequential IDs 1-${totalNeeded}. Run CREATE first for better accuracy.`);
        }
        
        const half = Math.ceil(ids.length / 2);
        const restReq: BulkDeleteRequest = { orderIds: ids.slice(0, half) };
        const gqlReq: BulkDeleteRequest = { orderIds: ids.slice(half) };
        restResult = await this.runMeasured(restReq, () => firstValueFrom(this.restService.bulkDeleteOrders(restReq)));
        graphqlResult = await this.runMeasured(gqlReq, () => firstValueFrom(this.graphqlService.bulkDeleteOrders(gqlReq)));
      }

      const now = Date.now();
      this.metricsService.recordOperation({
        operationType: this.operationType,
        apiType: 'REST',
        startTime: now - restResult.duration,
        endTime: now,
        duration: restResult.duration,
        requestSize: restResult.requestSize,
        responseSize: restResult.responseSize,
        success: restResult.success,
        itemCount: restResult.itemCount,
        error: restResult.error
      });
      this.metricsService.recordOperation({
        operationType: this.operationType,
        apiType: 'GraphQL',
        startTime: now,
        endTime: now + graphqlResult.duration,
        duration: graphqlResult.duration,
        requestSize: graphqlResult.requestSize,
        responseSize: graphqlResult.responseSize,
        success: graphqlResult.success,
        itemCount: graphqlResult.itemCount,
        error: graphqlResult.error
      });

      this.result = this.buildResult(restResult, graphqlResult);

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

  private buildResult(rest: SingleResult, graphql: SingleResult): OperationResult {
    const metrics: ComparisonMetric[] = [
      {
        label: 'Response Time',
        restValue: `${rest.duration} ms`,
        graphqlValue: `${graphql.duration} ms`,
        winner: rest.duration < graphql.duration ? 'REST' : graphql.duration < rest.duration ? 'GraphQL' : 'Tie'
      },
      {
        label: 'Request Size',
        restValue: this.fmtBytes(rest.requestSize),
        graphqlValue: this.fmtBytes(graphql.requestSize),
        winner: rest.requestSize < graphql.requestSize ? 'REST' : graphql.requestSize < rest.requestSize ? 'GraphQL' : 'Tie'
      },
      {
        label: 'Response Size',
        restValue: this.fmtBytes(rest.responseSize),
        graphqlValue: this.fmtBytes(graphql.responseSize),
        winner: rest.responseSize < graphql.responseSize ? 'REST' : graphql.responseSize < rest.responseSize ? 'GraphQL' : 'Tie'
      },
      {
        label: 'Items Processed',
        restValue: `${rest.itemCount}`,
        graphqlValue: `${graphql.itemCount}`,
        winner: 'Tie'
      }
    ];

    const timeScore = rest.duration < graphql.duration ? 2 : graphql.duration < rest.duration ? -2 : 0;
    const sizeScore = rest.responseSize < graphql.responseSize ? 1 : graphql.responseSize < rest.responseSize ? -1 : 0;
    const total = timeScore + sizeScore;

    let winner: 'REST' | 'GraphQL' | 'Tie';
    let winnerReason: string;

    if (!rest.success && graphql.success) {
      winner = 'GraphQL';
      winnerReason = 'REST operation failed';
    } else if (rest.success && !graphql.success) {
      winner = 'REST';
      winnerReason = 'GraphQL operation failed';
    } else if (total > 0) {
      winner = 'REST';
      const pct = graphql.duration > 0 ? Math.round(((graphql.duration - rest.duration) / graphql.duration) * 100) : 0;
      winnerReason = `REST was ${pct}% faster (${rest.duration} ms vs ${graphql.duration} ms)`;
    } else if (total < 0) {
      winner = 'GraphQL';
      const pct = rest.duration > 0 ? Math.round(((rest.duration - graphql.duration) / rest.duration) * 100) : 0;
      winnerReason = `GraphQL was ${pct}% faster (${graphql.duration} ms vs ${rest.duration} ms)`;
    } else {
      winner = 'Tie';
      winnerReason = 'Both APIs performed equally well';
    }

    return { rest, graphql, winner, winnerReason, metrics };
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
