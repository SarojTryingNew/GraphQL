import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BulkCreateRequest,
  BulkCreateResponse,
  BulkUpdateRequest,
  BulkUpdateResponse,
  BulkDeleteRequest,
  BulkDeleteResponse,
  Order
} from '../models/order.models';

/**
 * REST API Service that uses GraphQL as an internal backend layer
 * Architecture: Angular -> REST API -> GraphQL (internal) -> DataStore
 */
@Injectable({
  providedIn: 'root'
})
export class RestGraphQLBackendService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  bulkCreateOrders(request: BulkCreateRequest): Observable<BulkCreateResponse> {
    return this.http.post<BulkCreateResponse>(`${this.apiUrl}/api/graphql-backend/orders/bulk`, request);
  }

  bulkUpdateOrders(request: BulkUpdateRequest): Observable<BulkUpdateResponse> {
    return this.http.put<BulkUpdateResponse>(`${this.apiUrl}/api/graphql-backend/orders/bulk`, request);
  }

  bulkDeleteOrders(request: BulkDeleteRequest): Observable<BulkDeleteResponse> {
    return this.http.request<BulkDeleteResponse>('delete', `${this.apiUrl}/api/graphql-backend/orders/bulk`, {
      body: request
    });
  }

  getAllOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/api/graphql-backend/orders`);
  }

  getOrdersByIds(ids: number[]): Observable<Order[]> {
    const idsParam = ids.join(',');
    return this.http.get<Order[]>(`${this.apiUrl}/api/graphql-backend/orders/bulk?ids=${idsParam}`);
  }
}
