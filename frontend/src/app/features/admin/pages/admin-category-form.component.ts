import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

interface CategoryForm {
  name: string;
  slug: string;
  description: string;
  displayOrder: number;
}

@Component({
  selector: 'app-admin-category-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="category-form">
      <h1>{{ isEdit() ? 'Edit' : 'Add' }} Category</h1>
      
      <form (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label for="name">Name *</label>
          <input id="name" [(ngModel)]="category.name" name="name" required />
        </div>

        <div class="form-group">
          <label for="slug">Slug *</label>
          <input id="slug" [(ngModel)]="category.slug" name="slug" required />
          <small>URL-friendly name (e.g., electronics, clothing)</small>
        </div>

        <div class="form-group">
          <label for="description">Description</label>
          <textarea id="description" [(ngModel)]="category.description" name="description" rows="3"></textarea>
        </div>

        <div class="form-group">
          <label for="displayOrder">Display Order</label>
          <input id="displayOrder" type="number" [(ngModel)]="category.displayOrder" name="displayOrder" />
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
    .category-form { max-width: 500px; }
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
    .form-group small {
      display: block;
      margin-top: 0.25rem;
      color: var(--text-muted);
      font-size: 0.875rem;
    }
    
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
export class AdminCategoryFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private adminService = inject(AdminService);
  private toastService = inject(ToastService);

  category: CategoryForm = this.getEmptyCategory();
  loading = signal(true);
  saving = signal(false);
  isEdit = signal(false);
  private categoryId = '';

  private getEmptyCategory(): CategoryForm {
    return {
      name: '',
      slug: '',
      description: '',
      displayOrder: 0
    };
  }

  ngOnInit(): void {
    this.categoryId = this.route.snapshot.paramMap.get('id') || '';
    this.isEdit.set(!!this.categoryId);
    
    if (this.categoryId) {
      this.loadCategory();
    } else {
      this.loading.set(false);
    }
  }

  private loadCategory(): void {
    this.adminService.getCategory(this.categoryId).subscribe({
      next: (response: any) => {
        const c = response?.data || response;
        this.category = {
          name: c.name,
          slug: c.slug,
          description: c.description || '',
          displayOrder: c.displayOrder || 0
        };
        this.loading.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load category');
        this.router.navigate(['/admin/categories']);
      }
    });
  }

  private buildRequestPayload(): any {
    return {
      Name: this.category.name,
      Slug: this.category.slug,
      Description: this.category.description || null,
      DisplayOrder: this.category.displayOrder || 0
    };
  }

  onSubmit(): void {
    if (!this.category.name || !this.category.slug) {
      this.toastService.error('Please fill in all required fields');
      return;
    }

    this.saving.set(true);
    const payload = this.buildRequestPayload();

    if (this.isEdit()) {
      this.adminService.updateCategory(this.categoryId, payload).subscribe({
        next: () => {
          this.toastService.success('Category updated successfully');
          this.router.navigate(['/admin/categories']);
        },
        error: (err: any) => {
          this.saving.set(false);
          const msg = err?.error?.message || 'Failed to update category';
          this.toastService.error(msg);
        }
      });
    } else {
      this.adminService.createCategory(payload).subscribe({
        next: () => {
          this.toastService.success('Category created successfully');
          this.router.navigate(['/admin/categories']);
        },
        error: (err: any) => {
          this.saving.set(false);
          const msg = err?.error?.message || 'Failed to create category';
          this.toastService.error(msg);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/admin/categories']);
  }
}