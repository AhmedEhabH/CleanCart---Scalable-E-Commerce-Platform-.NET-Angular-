import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CartResponse, AddToCartRequest } from '../../core/models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private cartCountSubject = new BehaviorSubject<number>(0);
  cartCount$ = this.cartCountSubject.asObservable();

  constructor() {
    this.loadInitialCart();
  }

  private loadInitialCart(): void {
    this.http.get<CartResponse>(`${this.baseUrl}/Cart`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.cartCountSubject.next(response.data.totalItems || 0);
        }
      },
      error: () => {
        this.cartCountSubject.next(0);
      }
    });
  }

  getCart(): Observable<CartResponse> {
    return this.http.get<CartResponse>(`${this.baseUrl}/Cart`).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.cartCountSubject.next(response.data.totalItems || 0);
        }
      })
    );
  }

  addToCart(request: AddToCartRequest): Observable<CartResponse> {
    return this.http.post<CartResponse>(`${this.baseUrl}/Cart/items`, request).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.cartCountSubject.next(response.data.totalItems || 0);
        }
      })
    );
  }

  updateCartItem(itemId: string, quantity: number): Observable<CartResponse> {
    return this.http.put<CartResponse>(`${this.baseUrl}/Cart/items/${itemId}`, { quantity });
  }

  removeCartItem(itemId: string): Observable<CartResponse> {
    return this.http.delete<CartResponse>(`${this.baseUrl}/Cart/items/${itemId}`);
  }
}