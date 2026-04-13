export interface OrderItem {
  productId: number;
  quantity: number;
  discount: number;
  notes: string[];
}

export interface OrderCreate {
  customerId: number;
  status: string;
  items: OrderItem[];
}

export interface BulkCreateRequest {
  orders: OrderCreate[];
}

export interface BulkCreateResponse {
  successCount: number;
  failureCount: number;
  errors: string[];
  createdIds: number[];
}

export interface BulkUpdateRequest {
  orders: OrderUpdate[];
}

export interface OrderUpdate {
  id: number;
  status?: string;
  items?: OrderItemUpdate[];
}

export interface OrderItemUpdate {
  id?: number;
  productId: number;
  quantity: number;
  discount: number;
}

export interface BulkUpdateResponse {
  successCount: number;
  failureCount: number;
  errors: string[];
}

export interface BulkDeleteRequest {
  orderIds: number[];
}

export interface BulkDeleteResponse {
  successCount: number;
  failureCount: number;
  errors: string[];
  deletedIds: number[];
}

export interface Order {
  id: number;
  customerId: number;
  orderDate: string;
  status: string;
  totalAmount: number;
  customer?: Customer;
  items?: OrderItemDetails[];
}

export interface Customer {
  id: number;
  name: string;
  email: string;
}

export interface OrderItemDetails {
  id: number;
  quantity: number;
  unitPrice: number;
  discount: number;
  product?: Product;
}

export interface Product {
  id: number;
  name: string;
  price: number;
}

export interface OperationMetrics {
  operationType: 'create' | 'update' | 'delete' | 'get';
  apiType: 'REST' | 'GraphQL';
  startTime: number;
  endTime: number;
  duration: number;
  requestSize: number;
  responseSize: number;
  success: boolean;
  itemCount: number;
  error?: string;
}

export interface KpiMetrics {
  totalOperations: number;
  successRate: number;
  averageResponseTime: number;
  p95ResponseTime: number;
  p99ResponseTime: number;
  totalDataTransferred: number;
  operationsPerSecond: number;
  restVsGraphql: {
    rest: ApiMetrics;
    graphql: ApiMetrics;
  };
  byOperation: {
    [key: string]: OperationTypeMetrics;
  };
}

export interface ApiMetrics {
  totalRequests: number;
  successCount: number;
  failureCount: number;
  averageResponseTime: number;
  p95ResponseTime: number;
  averageRequestSize: number;
  averageResponseSize: number;
}

export interface OperationTypeMetrics {
  count: number;
  averageTime: number;
  successRate: number;
}
