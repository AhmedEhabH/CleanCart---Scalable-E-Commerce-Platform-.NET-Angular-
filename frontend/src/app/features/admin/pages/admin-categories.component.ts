import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

interface CategoryItem {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  displayOrder: number;
  isActive: boolean;
  parentId: string | null;
  productCount: number;
}

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="categories-page">
      <div class="page-header">
        <h1>Categories</h1>
        <a routerLink="/admin/categories/new" class="btn-primary">Add Category</a>
      </div>

      @if (loading()) {
        <div class="loading">Loading...</div>
      } @else if (categories().length === 0) {
        <div class="empty-state">
          <p>No categories found. Create your first category to get started.</p>
          <a routerLink="/admin/categories/new" class="btn-primary">Add Category</a>
        </div>
      } @else {
        <div class="categories-table">
          <table>
            <thead>
              <tr>
                <th>Order</th>
                <th>Name</th>
                <th>Slug</th>
                <th>Products</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (category of categories(); track category.id) {
                <tr>
                  <td>{{ category.displayOrder }}</td>
                  <td>
                    <a [routerLink]="['/admin/categories', category.id, 'edit']" class="category-name">
                      {{ category.name }}
                    </a>
                  </td>
                  <td><code>{{ category.slug }}</code></td>
                  <td>{{ category.productCount }}</td>
                  <td>
                    <span class="status-badge" [class.active]="category.isActive">
                      {{ category.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="actions">
                    <a [routerLink]="['/admin/categories', category.id, 'edit']" class="btn-edit">Edit</a>
                    @if (!category.parentId) {
                      <button class="btn-delete" (click)="deleteCategory(category)" [disabled]="category.productCount > 0">Delete</button>
                    }
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
    .categories-page { max-width: 1000px; }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    .page-header h1 { margin: 0; }
    
    .loading, .empty-state {
      text-align: center;
      padding: 3rem;
      color: var(--text-secondary);
    }
    .empty-state p { margin-bottom: 1rem; }
    
    .categories-table {
      background: var(--card-bg);
      border-radius: 8px;
      border: 1px solid var(--border-color);
      overflow: hidden;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th, td {
      padding: 1rem;
      text-align: left;
      border-bottom: 1px solid var(--border-color);
    }
    th {
      background: var(--hover-bg);
      font-weight: 600;
      color: var(--text-secondary);
      font-size: 0.875rem;
    }
    tr:last-child td { border-bottom: none; }
    tr:hover { background: var(--hover-bg); }
    
    .category-name {
      color: var(--text-primary);
      text-decoration: none;
      font-weight: 500;
    }
    .category-name:hover { color: var(--primary); }
    
    code {
      background: var(--hover-bg);
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.875rem;
    }
    
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.75rem;
      font-weight: 500;
      background: var(--text-muted);
      color: white;
    }
    .status-badge.active {
      background: #22c55e;
    }
    
    .actions {
      display: flex;
      gap: 0.5rem;
    }
    .btn-edit, .btn-delete {
      padding: 0.5rem 0.75rem;
      border-radius: 4px;
      font-size: 0.875rem;
      text-decoration: none;
      border: none;
      cursor: pointer;
    }
    .btn-edit {
      background: var(--primary);
      color: white;
    }
    .btn-delete {
      background: #ef4444;
      color: white;
    }
    .btn-delete:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    
    .btn-primary {
      padding: 0.75rem 1.5rem;
      border-radius: 6px;
      font-size: 1rem;
      font-weight: 500;
      background: var(--primary);
      color: white;
      text-decoration: none;
      border: none;
      cursor: pointer;
    }
  `]
})
export class AdminCategoriesComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastService = inject(ToastService);

  categories = signal<CategoryItem[]>([]);
  loading = signal(true);
  deleting = signal<string | null>(null);

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.adminService.getCategories().subscribe({
      next: (response: any) => {
        const data = response?.data || response || [];
        const flat = this.flattenCategories(data);
        this.categories.set(flat);
        this.loading.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load categories');
        this.loading.set(false);
      }
    });
  }

  private flattenCategories(categories: any[], result: CategoryItem[] = [], parentId: string | null = null): CategoryItem[] {
    for (const category of categories) {
      result.push({
        id: category.id,
        name: category.name,
        slug: category.slug,
        description: category.description,
        displayOrder: category.displayOrder,
        isActive: category.isActive,
        parentId: parentId,
        productCount: category.productCount || 0
      });
      if (category.children && category.children.length > 0) {
        this.flattenCategories(category.children, result, category.id);
      }
    }
    return result;
  }

  deleteCategory(category: CategoryItem): void {
    if (category.productCount > 0 || category.parentId) {
      this.deactivateCategory(category);
      return;
    }

    if (!confirm(`Are you sure you want to delete "${category.name}"? This action cannot be undone.`)) {
      return;
    }

    this.deleting.set(category.id);
    this.adminService.deleteCategory(category.id).subscribe({
      next: () => {
        this.toastService.success('Category deleted successfully');
        this.deleting.set(null);
        this.loadCategories();
      },
      error: (err: any) => {
        this.deleting.set(null);
        const msg = err?.error?.message || 'Failed to delete category';
        this.toastService.error(msg);
      }
    });
  }

  deactivateCategory(category: CategoryItem): void {
    if (!confirm(`Are you sure you want to deactivate "${category.name}"? Products in this category will remain but the category will be hidden.`)) {
      return;
    }

    this.deleting.set(category.id);
    this.adminService.deactivateCategory(category.id).subscribe({
      next: () => {
        this.toastService.success('Category deactivated successfully');
        this.deleting.set(null);
        this.loadCategories();
      },
      error: (err: any) => {
        this.deleting.set(null);
        const msg = err?.error?.message || 'Failed to deactivate category';
        this.toastService.error(msg);
      }
    });
  }
}