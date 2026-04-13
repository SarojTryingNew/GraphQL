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

@Injectable({
  providedIn: 'root'
})
export class RestService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  bulkCreateOrders(request: BulkCreateRequest): Observable<BulkCreateResponse> {
    return this.http.post<BulkCreateResponse>(`${this.apiUrl}/api/orders/bulk`, request);
  }

  bulkUpdateOrders(request: BulkUpdateRequest): Observable<BulkUpdateResponse> {
    return this.http.put<BulkUpdateResponse>(`${this.apiUrl}/api/orders/bulk`, request);
  }

  bulkDeleteOrders(request: BulkDeleteRequest): Observable<BulkDeleteResponse> {
    return this.http.request<BulkDeleteResponse>('delete', `${this.apiUrl}/api/orders/bulk`, {
      body: request
    });
  }

  getAllOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/api/orders`);
  }

  getOrdersByIds(ids: number[]): Observable<Order[]> {
    const idsParam = ids.join(',');
    return this.http.get<Order[]>(`${this.apiUrl}/api/orders/bulk?ids=${idsParam}`);
  }
}
