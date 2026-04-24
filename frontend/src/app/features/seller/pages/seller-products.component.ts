import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Product } from '../../products/models/product.model';
import { ProductImagePipe } from '../../../shared/pipes/product-image.pipe';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models';

interface ProductListItem {
  id: string;
  name: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  mainImageUrl: string | null;
  slug: string;
  categoryId: string;
  createdAt: string;
}

@Component({
  selector: 'app-seller-products',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, RouterLink, ProductImagePipe],
  template: `
    <div class="seller-products-page">
      <div class="page-header">
        <h1>My Products</h1>
        <a routerLink="/seller/products/new" class="btn-primary">Add Product</a>
      </div>

      @if (loading()) {
        <p class="loading">Loading...</p>
      } @else if (products().length === 0) {
        <div class="empty">
          <p>You haven't added any products yet.</p>
          <a routerLink="/seller/products/new" class="btn-primary">Add your first product</a>
        </div>
      } @else {
        <div class="products-grid">
          @for (product of products(); track product.id) {
            <div class="product-card">
              <a [routerLink]="['/products', product.id]" class="product-card__image">
                <img [src]="product.mainImageUrl | productImage: product.slug" [alt]="product.name" />
              </a>
              <div class="product-card__info">
                <h3>{{ product.name }}</h3>
                <p class="price">{{ product.price | currency }}</p>
                <p class="stock" [class.low]="product.stockQuantity < 10">
                  Stock: {{ product.stockQuantity }}
                </p>
                <span class="status" [class.active]="product.isActive">
                  {{ product.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
              <div class="product-card__actions">
                <a [routerLink]="['/seller/products', product.id, 'edit']" class="btn-link">Edit</a>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .seller-products-page { max-width: 1200px; }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }
    .page-header h1 { margin: 0; }
    
    .btn-primary {
      background: var(--primary);
      color: white;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      text-decoration: none;
    }
    .loading, .empty { text-align: center; padding: 3rem; color: var(--text-secondary); }
    .empty a { margin-top: 1rem; display: inline-block; }
    
    .products-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 1.5rem;
    }
    .product-card {
      background: var(--card-bg);
      border-radius: 8px;
      overflow: hidden;
      border: 1px solid var(--border-color);
    }
    .product-card__image {
      display: block;
      aspect-ratio: 1;
      overflow: hidden;
      background: var(--color-bg-secondary);
    }
    .product-card__image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    .product-card__info { padding: 1rem; }
    .product-card__info h3 {
      margin: 0 0 0.5rem;
      font-size: 1rem;
    }
    .price { color: var(--primary); font-weight: 700; margin: 0.25rem 0; }
    .stock { margin: 0.25rem 0; color: var(--text-secondary); font-size: 0.875rem; }
    .stock.low { color: var(--warning); }
    .status {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.75rem;
      background: var(--text-muted);
      color: white;
      margin-top: 0.5rem;
    }
    .status.active { background: var(--success); }
    .product-card__actions {
      padding: 0.5rem 1rem 1rem;
      display: flex;
      gap: 1rem;
    }
    .btn-link {
      color: var(--primary);
      text-decoration: none;
      font-size: 0.875rem;
    }
    .btn-link:hover { text-decoration: underline; }
  `]
})
export class SellerProductsComponent implements OnInit {
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl;

  products = signal<ProductListItem[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.loadProducts();
  }

  private loadProducts(): void {
    this.loading.set(true);
    const user = this.authService.currentUser;
    if (!user) {
      this.loading.set(false);
      this.toastService.error('Please log in to view your products');
      return;
    }

    this.http.get<ApiResponse<any>>(`${this.baseUrl}/products?vendorId=${user.id}&pageSize=100`).subscribe({
      next: (response) => {
        const data = response?.data;
        this.products.set(data?.items || []);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.products.set([]);
      }
    });
  }
}