import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardSummary {
  totalOrders: number;
  totalSales: number;
  totalProducts: number;
  totalUsers: number;
  recentOrders: any[];
  topProducts: any[];
  lowStockProducts: any[];
}

export interface ProductListItem {
  id: string;
  name: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  categoryId: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl + '/admin';
  private jsonHeaders = new HttpHeaders({ 'Content-Type': 'application/json' });

  getDashboard(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.baseUrl}/dashboard`, { headers: this.jsonHeaders });
  }

  getProducts(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiBaseUrl}/products`, { headers: this.jsonHeaders });
  }

  getProduct(id: string): Observable<any> {
    return this.http.get<any>(`${environment.apiBaseUrl}/products/${id}`, { headers: this.jsonHeaders });
  }

  createProduct(product: any): Observable<any> {
    return this.http.post<any>(`${environment.apiBaseUrl}/products`, product, { headers: this.jsonHeaders });
  }

  updateProduct(id: string, product: any): Observable<any> {
    return this.http.put<any>(`${environment.apiBaseUrl}/products/${id}`, product, { headers: this.jsonHeaders });
  }

  deleteProduct(id: string): Observable<any> {
    return this.http.delete<any>(`${environment.apiBaseUrl}/products/${id}`, { headers: this.jsonHeaders });
  }

  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiBaseUrl}/categories`, { headers: this.jsonHeaders });
  }
}