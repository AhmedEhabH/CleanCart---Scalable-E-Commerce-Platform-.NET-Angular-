import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../core/services/wishlist.service';
import { Product } from '../products/models/product.model';
import { ProductImagePipe } from '../../shared/pipes/product-image.pipe';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../shared/components/toast/toast.service';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyPipe, ProductImagePipe],
  template: `
    <div class="page">
      <div class="container">
        <div class="page-header">
          <h1>My Wishlist</h1>
          <a routerLink="/products" class="btn">Continue Shopping</a>
        </div>

        @if (loading()) {
          <div class="loading-state">
            <p>Loading wishlist...</p>
          </div>
        } @else if (items().length === 0) {
          <div class="empty-state">
            <div class="empty-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
              </svg>
            </div>
            <h2>No Saved Items</h2>
            <p>Your wishlist is empty. Browse products and save your favorites.</p>
            <a routerLink="/products" class="btn">Browse Products</a>
          </div>
        } @else {
          <div class="products-grid">
            @for (product of items(); track product.id) {
              <div class="product-card">
                <a [routerLink]="['/products', product.id]" class="product-card__link">
                  <div class="product-card__image">
                    <img [src]="product.mainImageUrl | productImage: product.slug" [alt]="product.name" />
                  </div>
                  <div class="product-card__info">
                    <h3 class="product-card__name">{{ product.name }}</h3>
                    <div class="product-card__price">
                      @if (product.hasDiscount && product.compareAtPrice) {
                        <span class="product-card__price-current">{{ product.price | currency }}</span>
                        <span class="product-card__price-original">{{ product.compareAtPrice | currency }}</span>
                      } @else {
                        <span class="product-card__price-current">{{ product.price | currency }}</span>
                      }
                    </div>
                  </div>
                </a>
                <div class="product-card__actions">
                  <button class="product-card__remove-btn" (click)="removeFromWishlist(product)">
                    Remove
                  </button>
                  <button class="product-card__add-btn" (click)="addToCart(product)" [disabled]="!product.isInStock">
                    Add to Cart
                  </button>
                </div>
              </div>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-6);
      padding-bottom: var(--space-4);
      border-bottom: 1px solid var(--color-border);
    }
    .page-header h1 { margin: 0; }
    .loading-state, .empty-state {
      text-align: center;
      padding: var(--space-12) var(--space-4);
    }
    .empty-icon {
      color: var(--color-text-muted);
      margin-bottom: var(--space-4);
    }
    .empty-state h2 { margin-bottom: var(--space-2); }
    .empty-state p {
      color: var(--color-text-secondary);
      margin-bottom: var(--space-6);
      max-width: 400px;
      margin-left: auto;
      margin-right: auto;
    }
    .products-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
      gap: var(--space-6);
    }
    .product-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      overflow: hidden;
    }
    .product-card__link { text-decoration: none; color: inherit; display: block; }
    .product-card__image { aspect-ratio: 1; overflow: hidden; background: var(--color-bg-secondary); }
    .product-card__image img { width: 100%; height: 100%; object-fit: cover; }
    .product-card__info { padding: var(--space-3); }
    .product-card__name {
      font-size: 0.95rem; font-weight: 600; margin: 0 0 var(--space-2);
      display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
    }
    .product-card__price { display: flex; align-items: center; gap: var(--space-2); }
    .product-card__price-current { font-size: 1.1rem; font-weight: 700; color: var(--color-primary); }
    .product-card__price-original { font-size: 0.85rem; color: var(--color-text-muted); text-decoration: line-through; }
    .product-card__actions { display: flex; gap: var(--space-2); padding: var(--space-3); padding-top: 0; }
    .product-card__remove-btn, .product-card__add-btn {
      flex: 1; padding: var(--space-2) var(--space-3); border-radius: var(--radius-sm);
      font-size: 0.875rem; font-weight: 500; cursor: pointer;
    }
    .product-card__remove-btn {
      background: transparent; border: 1px solid var(--color-border); color: var(--color-text-secondary);
    }
    .product-card__add-btn {
      background: var(--color-primary); border: 1px solid var(--color-primary); color: white;
    }
  `]
})
export class WishlistComponent implements OnInit {
  protected wishlistService = inject(WishlistService);
  private cartService = inject(CartService);
  private toastService = inject(ToastService);

  items = signal<Product[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.wishlistService.wishlistItems$.subscribe(products => {
      this.items.set(products);
      this.loading.set(false);
    });
  }

  removeFromWishlist(product: Product): void {
    this.wishlistService.removeFromWishlist(product.id);
    this.toastService.success(product.name + ' removed from wishlist');
  }

  addToCart(product: Product): void {
    this.cartService.addToCart({ productId: product.id, quantity: 1 }).subscribe({
      next: () => this.toastService.success(product.name + ' added to cart'),
      error: () => this.toastService.error('Failed to add item to cart')
    });
  }
}