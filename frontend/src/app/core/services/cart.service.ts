import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CartResponse, AddToCartRequest } from '../../core/models/cart.model';
import { AuthService } from './auth.service';

export interface CartState {
  items: any[];
  totalItems: number;
  subTotal: number;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly authService = inject(AuthService);

  private cartCountSubject = new BehaviorSubject<number>(0);
  cartCount$ = this.cartCountSubject.asObservable();

  private cartStateSubject = new BehaviorSubject<CartState>({
    items: [],
    totalItems: 0,
    subTotal: 0
  });
  cartState$ = this.cartStateSubject.asObservable();

  constructor() {
    this.loadInitialCart();
  }

  private loadInitialCart(): void {
    if (!this.authService.isAuthenticated) {
      this.cartCountSubject.next(0);
      return;
    }

    this.http.get<CartResponse>(`${this.baseUrl}/Cart`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.updateState(response.data);
        }
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 401) {
          this.cartCountSubject.next(0);
        }
      }
    });
  }

  private updateState(data: { totalItems?: number; subTotal?: number; items?: any[] }): void {
    const totalItems = data.totalItems || 0;
    const subTotal = data.subTotal || 0;
    const items = data.items || [];
    this.cartCountSubject.next(totalItems);
    this.cartStateSubject.next({ items, totalItems, subTotal });
  }

  getCart(): Observable<CartResponse> {
    return this.http.get<CartResponse>(`${this.baseUrl}/Cart`).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.updateState(response.data);
        }
      })
    );
  }

  addToCart(request: AddToCartRequest): Observable<CartResponse> {
    return this.http.post<CartResponse>(`${this.baseUrl}/Cart/items`, request).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.updateState(response.data);
        }
      })
    );
  }

  updateCartItem(itemId: string, quantity: number): Observable<CartResponse> {
    return this.http.put<CartResponse>(`${this.baseUrl}/Cart/items/${itemId}`, { quantity }).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.updateState(response.data);
        } else if (response.success) {
          this.getCart().subscribe();
        }
      })
    );
  }

  removeCartItem(itemId: string): Observable<CartResponse> {
    return this.http.delete<CartResponse>(`${this.baseUrl}/Cart/items/${itemId}`).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.updateState(response.data);
        } else if (response.success) {
          this.getCart().subscribe();
        }
      })
    );
  }
}