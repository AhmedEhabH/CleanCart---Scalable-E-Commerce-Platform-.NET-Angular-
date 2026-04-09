import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductsService } from '../services/products.service';
import { Product } from '../models/product.model';
import { ProductImagePipe } from '../../../shared/pipes/product-image.pipe';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-product-details-page',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, RouterLink, ProductImagePipe],
  templateUrl: './product-details.page.html',
  styleUrl: './product-details.page.scss'
})
export class ProductDetailsPage implements OnInit {
  private productsService = inject(ProductsService);
  private route = inject(ActivatedRoute);
  private cartService = inject(CartService);

  product = signal<Product | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  adding = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProduct(id);
    } else {
      this.error.set('Product ID is required.');
      this.loading.set(false);
    }
  }

  loadProduct(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.productsService.getProductById(id).subscribe({
      next: (response) => {
        if (response.data) {
          this.product.set(response.data);
        } else {
          this.error.set(response.message || 'Product not found.');
        }
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load product. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  onAddToCart(): void {
    const currentProduct = this.product();
    if (!currentProduct || this.adding()) return;

    this.adding.set(true);
    this.cartService.addToCart({ productId: currentProduct.id, quantity: 1 }).subscribe({
      next: () => {
        this.adding.set(false);
      },
      error: () => {
        this.adding.set(false);
      },
      complete: () => {
        this.adding.set(false);
      }
    });
  }
}
