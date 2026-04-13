import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
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
export class GraphqlService {
  private graphqlUrl = environment.graphqlEndpoint;

  constructor(private http: HttpClient) {}

  bulkCreateOrders(request: BulkCreateRequest): Observable<BulkCreateResponse> {
    const mutation = `
      mutation BulkCreateOrders($orders: [OrderCreateDtoInput!]!) {
        bulkCreateOrders(request: { orders: $orders }) {
          successCount
          failureCount
          errors
          createdIds
        }
      }
    `;

    return this.executeGraphQL<{ bulkCreateOrders: BulkCreateResponse }>(mutation, { orders: request.orders })
      .pipe(map(response => response.bulkCreateOrders));
  }

  bulkUpdateOrders(request: BulkUpdateRequest): Observable<BulkUpdateResponse> {
    const mutation = `
      mutation BulkUpdateOrders($orders: [OrderUpdateDtoInput!]!) {
        bulkUpdateOrders(request: { orders: $orders }) {
          successCount
          failureCount
          errors
        }
      }
    `;

    return this.executeGraphQL<{ bulkUpdateOrders: BulkUpdateResponse }>(mutation, { orders: request.orders })
      .pipe(map(response => response.bulkUpdateOrders));
  }

  bulkDeleteOrders(request: BulkDeleteRequest): Observable<BulkDeleteResponse> {
    const mutation = `
      mutation BulkDeleteOrders($orderIds: [Int!]!) {
        bulkDeleteOrders(request: { orderIds: $orderIds }) {
          successCount
          failureCount
          errors
          deletedIds
        }
      }
    `;

    return this.executeGraphQL<{ bulkDeleteOrders: BulkDeleteResponse }>(mutation, { orderIds: request.orderIds })
      .pipe(map(response => response.bulkDeleteOrders));
  }

  getAllOrders(): Observable<Order[]> {
    const query = `
      query GetAllOrders {
        orders {
          id
          customerId
          orderDate
          status
          totalAmount
          customer {
            id
            name
            email
          }
          items {
            id
            quantity
            unitPrice
            discount
            product {
              id
              name
              price
            }
          }
        }
      }
    `;

    return this.executeGraphQL<{ orders: Order[] }>(query)
      .pipe(map(response => response.orders));
  }

  getOrdersByIds(ids: number[]): Observable<Order[]> {
    const query = `
      query GetOrdersByIds($ids: [Int!]!) {
        ordersByIds(ids: $ids) {
          id
          customerId
          orderDate
          status
          totalAmount
          customer {
            id
            name
            email
          }
          items {
            id
            quantity
            unitPrice
            discount
            product {
              id
              name
              price
            }
          }
        }
      }
    `;

    return this.executeGraphQL<{ ordersByIds: Order[] }>(query, { ids })
      .pipe(map(response => response.ordersByIds));
  }

  private executeGraphQL<T>(query: string, variables?: any): Observable<T> {
    return this.http.post<{ data: T }>(this.graphqlUrl, { query, variables })
      .pipe(map(response => response.data));
  }
}
