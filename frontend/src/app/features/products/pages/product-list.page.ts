import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductGridComponent } from '../components/product-grid/product-grid.component';
import { ProductsService } from '../services/products.service';
import { CategoriesService } from '../services/categories.service';
import { Product } from '../models/product.model';
import { CategorySimple } from '../models/category.model';
import { PaginatedResult } from '../models/pagination.model';

type SortOption = {
  value: string;
  label: string;
  sortBy: string;
  descending: boolean;
};

@Component({
  selector: 'app-product-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ProductGridComponent],
  templateUrl: './product-list.page.html',
  styleUrl: './product-list.page.scss'
})
export class ProductListPage implements OnInit {
  private productsService = inject(ProductsService);
  private categoriesService = inject(CategoriesService);

  products = signal<Product[]>([]);
  pagination = signal<PaginatedResult<Product> | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  currentPage = signal(1);

  categories = signal<CategorySimple[]>([]);
  searchTerm = signal('');
  selectedCategory = signal<string>('');
  inStockOnly = signal(false);
  featuredOnly = signal(false);
  sortBy = signal<string>('featured');
  sortDescending = signal(false);

  searchInput = '';
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  sortOptions: SortOption[] = [
    { value: 'featured', label: 'Featured', sortBy: 'featured', descending: false },
    { value: 'price-asc', label: 'Price: Low to High', sortBy: 'price', descending: false },
    { value: 'price-desc', label: 'Price: High to Low', sortBy: 'price', descending: true },
    { value: 'name-asc', label: 'Name: A-Z', sortBy: 'name', descending: false },
    { value: 'name-desc', label: 'Name: Z-A', sortBy: 'name', descending: true },
  ];

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.categoriesService.getCategoryOptions().subscribe({
      next: (categories) => {
        this.categories.set(categories);
      }
    });
  }

  loadProducts(page: number = 1): void {
    this.loading.set(true);
    this.error.set(null);

    const query: any = { page, pageSize: 12 };

    if (this.searchTerm()) {
      query.searchTerm = this.searchTerm();
    }
    if (this.selectedCategory()) {
      query.categoryId = this.selectedCategory();
    }
    if (this.inStockOnly()) {
      query.isInStock = true;
    }
    if (this.featuredOnly()) {
      query.isFeatured = true;
    }
    if (this.sortBy()) {
      query.sortBy = this.sortBy();
      query.sortDescending = this.sortDescending();
    }

    this.productsService.getProducts(query).subscribe({
      next: (response) => {
        if (response.data) {
          this.products.set(response.data.items);
          this.pagination.set(response.data);
          this.currentPage.set(response.data.page);
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load products. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  onSearchInput(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.searchTerm.set(this.searchInput);
      this.loadProducts(1);
    }, 300);
  }

  onSearchKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      if (this.searchTimeout) {
        clearTimeout(this.searchTimeout);
      }
      this.searchTerm.set(this.searchInput);
      this.loadProducts(1);
    }
  }

  onCategoryChange(): void {
    this.loadProducts(1);
  }

  onInStockToggle(): void {
    this.loadProducts(1);
  }

  onFeaturedToggle(): void {
    this.loadProducts(1);
  }

  onSortChange(): void {
    const option = this.sortOptions.find(o => o.value === this.sortBy());
    if (option) {
      this.sortBy.set(option.sortBy);
      this.sortDescending.set(option.descending);
    }
    this.loadProducts(1);
  }

  clearFilters(): void {
    this.searchInput = '';
    this.searchTerm.set('');
    this.selectedCategory.set('');
    this.inStockOnly.set(false);
    this.featuredOnly.set(false);
    this.sortBy.set('featured');
    this.sortDescending.set(false);
    this.loadProducts(1);
  }

  get hasActiveFilters(): boolean {
    return !!this.searchTerm() || !!this.selectedCategory() || this.inStockOnly() || this.featuredOnly();
  }

  goToPage(page: number): void {
    this.loadProducts(page);
  }

  nextPage(): void {
    const pag = this.pagination();
    if (pag?.hasNextPage) {
      this.loadProducts(this.currentPage() + 1);
    }
  }

  prevPage(): void {
    const pag = this.pagination();
    if (pag?.hasPreviousPage) {
      this.loadProducts(this.currentPage() - 1);
    }
  }
}
