import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Apollo, gql } from 'apollo-angular';
import {
  BulkCreateRequest,
  BulkCreateResponse,
  BulkUpdateRequest,
  BulkUpdateResponse,
  BulkDeleteRequest,
  BulkDeleteResponse,
  Order
} from '../models/order.models';

// GraphQL Mutations
const BULK_CREATE_ORDERS = gql`
  mutation BulkCreateOrders($orders: [OrderCreateDtoInput!]!) {
    bulkCreateOrders(request: { orders: $orders }) {
      successCount
      failureCount
      errors
      createdIds
    }
  }
`;

const BULK_UPDATE_ORDERS = gql`
  mutation BulkUpdateOrders($orders: [OrderUpdateDtoInput!]!) {
    bulkUpdateOrders(request: { orders: $orders }) {
      successCount
      failureCount
      errors
    }
  }
`;

const BULK_DELETE_ORDERS = gql`
  mutation BulkDeleteOrders($orderIds: [Int!]!) {
    bulkDeleteOrders(request: { orderIds: $orderIds }) {
      successCount
      failureCount
      errors
      deletedIds
    }
  }
`;

const GET_ALL_ORDERS = gql`
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

const GET_ORDERS_BY_IDS = gql`
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

@Injectable({
  providedIn: 'root'
})
export class GraphqlService {
  constructor(private apollo: Apollo) {}

  bulkCreateOrders(request: BulkCreateRequest): Observable<BulkCreateResponse> {
    return this.apollo
      .mutate<{ bulkCreateOrders: BulkCreateResponse }>({
        mutation: BULK_CREATE_ORDERS,
        variables: { orders: request.orders }
      })
      .pipe(
        map(result => {
          if (result.error) {
            throw result.error;
          }
          if (!result.data) {
            throw new Error('No data returned from bulkCreateOrders mutation');
          }
          return result.data.bulkCreateOrders;
        })
      );
  }

  bulkUpdateOrders(request: BulkUpdateRequest): Observable<BulkUpdateResponse> {
    return this.apollo
      .mutate<{ bulkUpdateOrders: BulkUpdateResponse }>({
        mutation: BULK_UPDATE_ORDERS,
        variables: { orders: request.orders }
      })
      .pipe(
        map(result => {
          if (result.error) {
            throw result.error;
          }
          if (!result.data) {
            throw new Error('No data returned from bulkUpdateOrders mutation');
          }
          return result.data.bulkUpdateOrders;
        })
      );
  }

  bulkDeleteOrders(request: BulkDeleteRequest): Observable<BulkDeleteResponse> {
    return this.apollo
      .mutate<{ bulkDeleteOrders: BulkDeleteResponse }>({
        mutation: BULK_DELETE_ORDERS,
        variables: { orderIds: request.orderIds }
      })
      .pipe(
        map(result => {
          if (result.error) {
            throw result.error;
          }
          if (!result.data) {
            throw new Error('No data returned from bulkDeleteOrders mutation');
          }
          return result.data.bulkDeleteOrders;
        })
      );
  }

  getAllOrders(): Observable<Order[]> {
    return this.apollo
      .query<{ orders: Order[] }>({
        query: GET_ALL_ORDERS
      })
      .pipe(
        map(result => {
          if (result.error) {
            throw result.error;
          }
          if (!result.data) {
            throw new Error('No data returned from getAllOrders query');
          }
          return result.data.orders;
        })
      );
  }

  getOrdersByIds(ids: number[]): Observable<Order[]> {
    return this.apollo
      .query<{ ordersByIds: Order[] }>({
        query: GET_ORDERS_BY_IDS,
        variables: { ids }
      })
      .pipe(
        map(result => {
          if (result.error) {
            throw result.error;
          }
          if (!result.data) {
            throw new Error('No data returned from getOrdersByIds query');
          }
          return result.data.ordersByIds;
        })
      );
  }
}

