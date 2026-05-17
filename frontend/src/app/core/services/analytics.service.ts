import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AnalyticsData, AnalyticsSummary, TopProduct, SalesTrend } from '../models/analytics.model';
import { ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getAnalytics(year: number = new Date().getFullYear()): Observable<ApiResponse<AnalyticsData>> {
    const params = new HttpParams().set('year', year.toString());
    return this.http.get<ApiResponse<AnalyticsData>>(`${this.baseUrl}/analytics`, { params });
  }

  getSummary(): Observable<ApiResponse<AnalyticsSummary>> {
    return this.http.get<ApiResponse<AnalyticsSummary>>(`${this.baseUrl}/analytics/summary`);
  }

  getTopProducts(count: number = 5): Observable<ApiResponse<TopProduct[]>> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<ApiResponse<TopProduct[]>>(`${this.baseUrl}/analytics/top-products`, { params });
  }

  getSalesTrend(year: number = new Date().getFullYear()): Observable<ApiResponse<SalesTrend[]>> {
    const params = new HttpParams().set('year', year.toString());
    return this.http.get<ApiResponse<SalesTrend[]>>(`${this.baseUrl}/analytics/sales-trend`, { params });
  }
}