import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductsService } from '../services/products.service';
import { Product } from '../models/product.model';
import { ProductImagePipe } from '../../../shared/pipes/product-image.pipe';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

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
  private wishlistService = inject(WishlistService);
  private toastService = inject(ToastService);

  product = signal<Product | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  adding = signal(false);
  quantity = signal(1);

  get isFavorite(): boolean {
    const p = this.product();
    return p ? this.wishlistService.isInWishlist(p.id) : false;
  }

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
    this.quantity.set(1);

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

  incrementQuantity(): void {
    const current = this.quantity();
    const max = this.product()?.stockQuantity || 1;
    if (current < max) {
      this.quantity.set(current + 1);
    }
  }

  decrementQuantity(): void {
    const current = this.quantity();
    if (current > 1) {
      this.quantity.set(current - 1);
    }
  }

  onAddToCart(): void {
    const currentProduct = this.product();
    if (!currentProduct || this.adding()) return;

    this.adding.set(true);
    this.cartService.addToCart({ productId: currentProduct.id, quantity: this.quantity() }).subscribe({
      next: () => {
        this.toastService.success(`${currentProduct.name} added to cart`);
      },
      error: () => {
        this.toastService.error('Failed to add item to cart');
        this.adding.set(false);
      },
      complete: () => {
        this.adding.set(false);
      }
    });
  }

  onToggleWishlist(): void {
    const currentProduct = this.product();
    if (!currentProduct) return;

    this.wishlistService.toggleWishlist(currentProduct);
    if (this.isFavorite) {
      this.toastService.success(`${currentProduct.name} removed from wishlist`);
    } else {
      this.toastService.success(`${currentProduct.name} added to wishlist`);
    }
  }
}
