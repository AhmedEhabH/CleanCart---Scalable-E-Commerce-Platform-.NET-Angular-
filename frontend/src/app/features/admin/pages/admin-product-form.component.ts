import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-admin-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="product-form">
      <h1>{{ isEdit() ? 'Edit' : 'Add' }} Product</h1>
      
      <form (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label for="name">Name *</label>
          <input id="name" [(ngModel)]="product.name" name="name" required />
        </div>

        <div class="form-group">
          <label for="slug">Slug *</label>
          <input id="slug" [(ngModel)]="product.slug" name="slug" required />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="price">Price *</label>
            <input id="price" type="number" [(ngModel)]="product.price" name="price" step="0.01" required />
          </div>
          <div class="form-group">
            <label for="sku">SKU *</label>
            <input id="sku" [(ngModel)]="product.sku" name="sku" required />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="stockQuantity">Stock *</label>
            <input id="stockQuantity" type="number" [(ngModel)]="product.stockQuantity" name="stockQuantity" required />
          </div>
          <div class="form-group">
            <label for="categoryId">Category *</label>
            <select id="categoryId" [(ngModel)]="product.categoryId" name="categoryId" required>
              <option value="">Select category</option>
              @for (cat of categories(); track cat.id) {
                <option [value]="cat.id">{{ cat.name }}</option>
              }
            </select>
          </div>
        </div>

        <div class="form-group">
          <label for="description">Description</label>
          <textarea id="description" [(ngModel)]="product.description" name="description" rows="4"></textarea>
        </div>

        <div class="form-group checkbox">
          <input id="isFeatured" type="checkbox" [(ngModel)]="product.isFeatured" name="isFeatured" />
          <label for="isFeatured">Featured product</label>
        </div>

        <div class="form-group checkbox">
          <input id="isActive" type="checkbox" [(ngModel)]="product.isActive" name="isActive" />
          <label for="isActive">Active</label>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-primary" [disabled]="saving()">
            {{ saving() ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .product-form { max-width: 600px; }
    h1 { margin: 0 0 1.5rem; }
    
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.5rem; font-weight: 500; }
    .form-group input, .form-group select, .form-group textarea {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid var(--border-color);
      border-radius: 6px;
      background: var(--input-bg);
      color: var(--text-primary);
      font-size: 1rem;
    }
    .form-group select {
      appearance: none;
      background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23888' d='M6 8L1 3h10z'/%3E%3C/svg%3E");
      background-repeat: no-repeat;
      background-position: right 0.75rem center;
      padding-right: 2.5rem;
      cursor: pointer;
      color: var(--color-text, var(--text-primary));
      background-color: var(--color-surface, var(--card-bg));
    }
    .form-group select option {
      background: var(--color-surface, #ffffff);
      color: var(--color-text, #111827);
    }
    
    .form-group.checkbox {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .form-group.checkbox label { margin: 0; }
    .form-group.checkbox input { width: auto; }
    
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .form-actions { display: flex; gap: 1rem; margin-top: 1.5rem; }
    
    .btn-primary, .btn-secondary {
      padding: 0.75rem 1.5rem;
      border-radius: 6px;
      font-size: 1rem;
      cursor: pointer;
      border: none;
    }
    .btn-primary { background: var(--primary); color: white; }
    .btn-primary:disabled { opacity: 0.5; }
    .btn-secondary { background: var(--text-muted); color: white; }
  `]
})
export class AdminProductFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private adminService = inject(AdminService);
  private toastService = inject(ToastService);
  
  product: any = this.getEmptyProduct();
  categories = signal<any[]>([]);
  loading = signal(true);
  saving = signal(false);
  isEdit = signal(false);
  private productId = '';

  private getEmptyProduct() {
    return {
      name: '',
      slug: '',
      price: 0,
      sku: '',
      stockQuantity: 0,
      categoryId: '',
      description: null as string | null,
      isFeatured: false,
      isActive: true
    };
  }

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id') || '';
    this.isEdit.set(!!this.productId);
    this.loadCategories();
    
    if (this.productId) {
      this.loadProduct();
    } else {
      this.loading.set(false);
    }
  }

  private loadCategories(): void {
    this.adminService.getCategories().subscribe({
      next: (response: any) => this.categories.set(response?.data || response || []),
      error: () => this.categories.set([])
    });
  }

  private loadProduct(): void {
    this.adminService.getProduct(this.productId).subscribe({
      next: (response: any) => {
        const p = response?.data || response;
        this.product = {
          name: p.name,
          slug: p.slug,
          price: p.price,
          sku: p.sku,
          stockQuantity: p.stockQuantity,
          categoryId: p.categoryId,
          description: p.description || null,
          isFeatured: p.isFeatured,
          isActive: p.isActive
        };
        this.loading.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load product');
        this.router.navigate(['/admin/products']);
      }
    });
  }

private buildRequestPayload(): any {
    const payload = {
      CategoryId: this.product.categoryId,
      Name: this.product.name,
      Slug: this.product.slug,
      Price: Number(this.product.price) || 0,
      SKU: this.product.sku,
      StockQuantity: Number(this.product.stockQuantity) || 0,
      Description: this.product.description || null,
      CompareAtPrice: this.product.compareAtPrice ? Number(this.product.compareAtPrice) : null,
      LowStockThreshold: 10,
      IsFeatured: Boolean(this.product.isFeatured)
    };
    return payload;
  }

onSubmit(): void {
    if (!this.product.name || !this.product.slug || !this.product.categoryId || !this.product.sku || !this.product.price || !this.product.stockQuantity) {
      this.toastService.error('Please fill in all required fields');
      return;
    }

    if (!this.product.categoryId || this.product.categoryId === '') {
      this.toastService.error('Please select a category');
      return;
    }

    this.saving.set(true);
    const payload = this.buildRequestPayload();

    if (this.isEdit()) {
      this.adminService.updateProduct(this.productId, payload).subscribe({
        next: () => {
          this.toastService.success('Product updated successfully');
          this.router.navigate(['/admin/products']);
        },
        error: (err: any) => {
          this.saving.set(false);
          const msg = err?.error?.message || 'Failed to update product';
          this.toastService.error(msg);
        }
      });
    } else {
      this.adminService.createProduct(payload).subscribe({
        next: () => {
          this.toastService.success('Product created successfully');
          this.router.navigate(['/admin/products']);
        },
        error: (err: any) => {
          this.saving.set(false);
          this.toastService.error(this.extractErrorMessage(err));
        }
      });
    }
  }

  private extractErrorMessage(error: any): string {
    if (!error) return 'Failed to save product. Please verify the form and try again.';
    const errorData = error.error;
    if (errorData?.message) {
      const msg = errorData.message.toLowerCase();
      if (msg.includes('sku')) return 'A product with this SKU already exists';
      if (msg.includes('slug')) return 'A product with this slug already exists';
      if (msg.includes('category')) return 'Invalid category selected';
      return errorData.message;
    }
    if (errorData?.errors) {
      return Object.values(errorData.errors).flat().join(', ') || 'Please check your form input';
    }
    return 'Failed to save product. Please verify the form and try again.';
  }

  cancel(): void {
    this.router.navigate(['/admin/products']);
  }
}