import { Injectable, inject, PLATFORM_ID, Injector } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map, catchError, of, firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product } from '../../features/products/models/product.model';
import { ApiResponse } from '../models';
import { AuthService } from './auth.service';

const STORAGE_KEY = 'wishlist_items';

interface WishlistState {
  items: Product[];
  loading: boolean;
}

interface WishlistItemDto {
  id: string;
  productId: string;
  productName: string;
  price: number;
  mainImageUrl: string | null;
  addedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly platformId = inject(PLATFORM_ID);
  private readonly injector = inject(Injector);

  private wishlistStateSubject = new BehaviorSubject<WishlistState>({
    items: [],
    loading: false
  });
  wishlistState$ = this.wishlistStateSubject.asObservable();

  wishlistItems$: Observable<Product[]> = this.wishlistState$.pipe(
    map(state => state.items)
  );

  wishlistCount$: Observable<number> = this.wishlistState$.pipe(
    map(state => state.items.length)
  );

  private get authService(): AuthService {
    return this.injector.get(AuthService);
  }

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadFromStorage();
    }
  }

  private getStoredIds(): string[] {
    if (!isPlatformBrowser(this.platformId)) {
      return [];
    }
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        const ids: string[] = JSON.parse(stored);
        const validIds = this.filterValidGuids(ids);
        if (validIds.length !== ids.length) {
          this.persistIds(validIds);
        }
        return validIds;
      } catch {
        localStorage.removeItem(STORAGE_KEY);
        return [];
      }
    }
    return [];
  }

  private filterValidGuids(ids: string[]): string[] {
    return ids.filter(id => {
      if (!id || typeof id !== 'string') return false;
      const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
      return guidRegex.test(id);
    });
  }

  private persistIds(ids: string[]): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(ids));
    }
  }

  private clearLocalStorage(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(STORAGE_KEY);
    }
  }

  private loadFromStorage(): void {
    const ids = this.getStoredIds();
    if (!ids || ids.length === 0) {
      this.wishlistStateSubject.next({ items: [], loading: false });
      return;
    }

    this.wishlistStateSubject.next({ items: [], loading: true });
    
    this.fetchProductsByIds(ids).subscribe({
      next: (products) => {
        this.wishlistStateSubject.next({ items: products, loading: false });
      },
      error: () => {
        this.wishlistStateSubject.next({ items: [], loading: false });
      }
    });
  }

  private fetchProductsByIds(ids: string[]): Observable<Product[]> {
    if (!ids || ids.length === 0) {
      return of([]);
    }

    return this.http.post<ApiResponse<Product[]>>(`${this.baseUrl}/products/by-ids`, ids).pipe(
      map(response => response.data || []),
      catchError(() => of([] as Product[]))
    );
  }

  isInWishlist(productId: string): boolean {
    return this.wishlistStateSubject.value.items.some(p => p.id === productId);
  }

  async syncWithServer(): Promise<void> {
    if (!this.authService.isAuthenticated) {
      return;
    }

    const localIds = this.getStoredIds();
    if (localIds.length === 0) {
      return;
    }

    try {
      const response = await firstValueFrom(
        this.http.post<ApiResponse<WishlistItemDto[]>>(`${this.baseUrl}/wishlist/sync`, localIds)
      );

      if (response.data && response.data.length > 0) {
        const products: Product[] = response.data.map(item => ({
          id: item.productId,
          name: item.productName,
          price: item.price,
          mainImageUrl: item.mainImageUrl
        } as Product));

        this.clearLocalStorage();
        this.wishlistStateSubject.next({ items: products, loading: false });
      } else {
        this.clearLocalStorage();
        this.wishlistStateSubject.next({ items: [], loading: false });
      }
    } catch (error) {
      console.error('Failed to sync wishlist with server:', error);
    }
  }

  toggleWishlist(product: Product): void {
    if (this.authService.isAuthenticated) {
      this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/wishlist/toggle/${product.id}`, {}).subscribe({
        next: (response) => {
          if (response.data !== undefined) {
            const current = this.wishlistStateSubject.value;
            const newItems = response.data
              ? [...current.items, product]
              : current.items.filter(p => p.id !== product.id);
            this.wishlistStateSubject.next({ ...current, items: newItems });
          }
        },
        error: (err) => {
          console.error('Failed to toggle wishlist item:', err);
        }
      });
    } else {
      const current = this.wishlistStateSubject.value;
      const exists = current.items.some(p => p.id === product.id);

      let newItems: Product[];
      let newIds: string[];

      if (exists) {
        newItems = current.items.filter(p => p.id !== product.id);
        newIds = newItems.map(p => p.id);
      } else {
        newItems = [...current.items, product];
        newIds = newItems.map(p => p.id);
      }

      this.persistIds(newIds);
      this.wishlistStateSubject.next({ ...current, items: newItems });
    }
  }

  removeFromWishlist(productId: string): void {
    const current = this.wishlistStateSubject.value;
    const newItems = current.items.filter(p => p.id !== productId);
    const newIds = newItems.map(p => p.id);

    this.persistIds(newIds);
    this.wishlistStateSubject.next({ ...current, items: newItems });
  }

  refreshWishlist(): void {
    const ids = this.getStoredIds();
    if (ids.length === 0) {
      this.wishlistStateSubject.next({ items: [], loading: false });
      return;
    }

    this.wishlistStateSubject.next({ ...this.wishlistStateSubject.value, loading: true });

    this.fetchProductsByIds(ids).subscribe({
      next: (products) => {
        this.wishlistStateSubject.next({ items: products, loading: false });
      },
      error: () => {
        this.wishlistStateSubject.next({ items: [], loading: false });
      }
    });
  }
}