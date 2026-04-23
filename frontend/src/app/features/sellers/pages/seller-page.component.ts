import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SellersService, Seller } from '../../../core/services/sellers.service';
import { Product } from '../../products/models/product.model';
import { ProductCardComponent } from '../../products/components/product-card/product-card.component';

@Component({
  selector: 'app-seller-page',
  standalone: true,
  imports: [CommonModule, DatePipe, ProductCardComponent],
  template: `
    <div class="seller-page">
      @if (loading()) {
        <div class="loading">Loading seller...</div>
      } @else if (error()) {
        <div class="error">{{ error() }}</div>
      } @else if (seller()) {
        <header class="seller-header">
          @if (seller()!.logoUrl) {
            <img [src]="seller()!.logoUrl" [alt]="seller()!.businessName" class="seller-logo">
          }
          <div class="seller-info">
            <h1>{{ seller()!.businessName }}</h1>
            @if (seller()!.description) {
              <p class="description">{{ seller()!.description }}</p>
            }
            <p class="meta">
              <span>{{ seller()!.productsCount }} products</span>
              @if (seller()!.createdAt) {
                <span> · Joined {{ seller()!.createdAt | date:'mediumDate' }}</span>
              }
            </p>
          </div>
        </header>

        <section class="seller-products">
          <h2>Products</h2>
          @if (products().length > 0) {
            <div class="products-grid">
              @for (product of products(); track product.id) {
                <app-product-card [product]="product" />
              }
            </div>
          } @else {
            <p class="empty">No products available</p>
          }
        </section>
      }
    </div>
  `,
  styleUrl: './seller-page.scss'
})
export class SellerPage implements OnInit {
  private route = inject(ActivatedRoute);
  private sellersService = inject(SellersService);

  seller = signal<Seller | null>(null);
  products = signal<Product[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const sellerId = this.route.snapshot.paramMap.get('id');
    if (!sellerId) {
      this.error.set('Invalid seller ID');
      this.loading.set(false);
      return;
    }

    this.sellersService.getSeller(sellerId).subscribe({
      next: (seller) => {
        this.seller.set(seller);
        this.loadProducts(sellerId);
      },
      error: () => {
        this.error.set('Seller not found');
        this.loading.set(false);
      }
    });
  }

  private loadProducts(sellerId: string): void {
    this.sellersService.getSellerProducts(sellerId).subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}