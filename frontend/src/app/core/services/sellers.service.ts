import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Seller {
  id: string;
  businessName: string;
  description: string | null;
  logoUrl: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  isApproved: boolean;
  productsCount: number;
  createdAt: string;
  approvedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class SellersService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl + '/sellers';

  getSellers(): Observable<Seller[]> {
    return this.http.get<Seller[]>(this.baseUrl);
  }

  getSeller(id: string): Observable<Seller> {
    return this.http.get<Seller>(`${this.baseUrl}/${id}`);
  }

  getSellerProducts(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${id}/products`);
  }
}