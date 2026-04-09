import { Component, Input, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Product } from '../../models/product.model';
import { ProductImagePipe } from '../../../../shared/pipes/product-image.pipe';
import { CartService } from '../../../../core/services/cart.service';

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
  adding = false;

  onAddToCart(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.adding) return;
    
    this.adding = true;
    this.cartService.addToCart({ productId: this.product.id, quantity: 1 }).subscribe({
      next: () => {
        this.adding = false;
      },
      error: () => {
        this.adding = false;
      },
      complete: () => {
        this.adding = false;
      }
    });
  }
}
