import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product } from '../../features/products/models/product.model';
import { ApiResponse } from '../models';

const STORAGE_KEY = 'wishlist_items';

interface WishlistState {
  items: Product[];
  loading: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly platformId = inject(PLATFORM_ID);

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

  private loadFromStorage(): void {
    const ids = this.getStoredIds();
    if (ids.length === 0) {
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
    if (ids.length === 0) {
      return of([]);
    }

    return this.http.post<ApiResponse<Product[]>>(`${this.baseUrl}/products/by-ids`, { ids }).pipe(
      map(response => response.data || []),
      catchError(() => of([] as Product[]))
    );
  }

  isInWishlist(productId: string): boolean {
    return this.wishlistStateSubject.value.items.some(p => p.id === productId);
  }

  toggleWishlist(product: Product): void {
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