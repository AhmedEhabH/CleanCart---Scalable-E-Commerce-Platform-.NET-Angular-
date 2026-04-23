import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink],
  template: `
    <div class="products-page">
      <div class="page-header">
        <h1>Products</h1>
        <a routerLink="/admin/products/new" class="btn-primary">Add Product</a>
      </div>

      @if (loading()) {
        <p class="loading">Loading...</p>
      } @else if (products().length === 0) {
        <div class="empty">
          <p>No products yet.</p>
          <a routerLink="/admin/products/new" class="btn-primary">Add your first product</a>
        </div>
      } @else {
        <div class="products-table-wrapper">
          <table class="data-table">
            <thead>
              <tr><th>Name</th><th>Price</th><th>Stock</th><th>Status</th><th>Created</th><th>Actions</th></tr>
            </thead>
            <tbody>
              @for (product of products(); track product.id) {
                <tr>
                  <td>{{ product.name }}</td>
                  <td>{{ product.price | currency }}</td>
                  <td [class.warning]="product.stockQuantity < 10">{{ product.stockQuantity }}</td>
                  <td>
                    <span class="status" [class.active]="product.isActive">
                      {{ product.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td>{{ product.createdAt | date:'short' }}</td>
                  <td class="actions">
                    <a [routerLink]="['/admin/products', product.id, 'edit']" class="btn-link">Edit</a>
                    <button (click)="deleteProduct(product.id)" class="btn-link danger">Delete</button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
  styles: [`
    .products-page { max-width: 1200px; }
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
    
    .products-table-wrapper { overflow-x: auto; }
    .data-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--card-bg);
      border-radius: 8px;
    }
    .data-table th, .data-table td {
      padding: 1rem;
      text-align: left;
      border-bottom: 1px solid var(--border-color);
    }
    .data-table th { font-size: 0.75rem; color: var(--text-secondary); text-transform: uppercase; }
    .warning { color: var(--warning); font-weight: 600; }
    .status { 
      padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.75rem;
      background: var(--text-muted); color: white;
    }
    .status.active { background: var(--success); }
    .actions { display: flex; gap: 1rem; }
    .btn-link { color: var(--primary); text-decoration: none; background: none; border: none; cursor: pointer; font: inherit; }
    .btn-link:hover { text-decoration: underline; }
    .btn-link.danger { color: var(--error); }
  `]
})
export class AdminProductsComponent implements OnInit {
  private adminService = inject(AdminService);
  
  products = signal<any[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.loadProducts();
  }

  private loadProducts(): void {
    this.adminService.getProducts().subscribe({
      next: (response: any) => {
        const data = response?.data || response;
        this.products.set(data?.items || []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  deleteProduct(id: string): void {
    if (!confirm('Are you sure you want to delete this product?')) return;
    
    this.adminService.deleteProduct(id).subscribe({
      next: () => this.loadProducts(),
      error: () => alert('Failed to delete product')
    });
  }
}