import { Component, Input, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Product } from '../../models/product.model';
import { ProductImagePipe } from '../../../../shared/pipes/product-image.pipe';
import { CartService } from '../../../../core/services/cart.service';
import { WishlistService } from '../../../../core/services/wishlist.service';
import { ToastService } from '../../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyPipe, ProductImagePipe],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss'
})
export class ProductCardComponent {
  @Input({ required: true }) product!: Product;

  private cartService = inject(CartService);
  private wishlistService = inject(WishlistService);
  private toastService = inject(ToastService);
  adding = false;

  get isFavorite(): boolean {
    return this.wishlistService.isInWishlist(this.product.id);
  }

  onAddToCart(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.adding) return;
    
    this.adding = true;
    this.cartService.addToCart({ productId: this.product.id, quantity: 1 }).subscribe({
      next: () => {
        this.toastService.success(`${this.product.name} added to cart`);
      },
      error: () => {
        this.toastService.error('Failed to add item to cart');
      },
      complete: () => {
        this.adding = false;
      }
    });
  }

  onToggleWishlist(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    
    this.wishlistService.toggleWishlist(this.product);
    if (this.isFavorite) {
      this.toastService.success(`${this.product.name} removed from wishlist`);
    } else {
      this.toastService.success(`${this.product.name} added to wishlist`);
    }
  }
}
