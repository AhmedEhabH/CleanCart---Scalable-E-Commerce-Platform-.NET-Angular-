import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CurrencyPipe } from '@angular/common';
import { ProductImagePipe } from '../../shared/pipes/product-image.pipe';
import { CartService } from '../../core/services/cart.service';
import { CartResponse, CartItem } from '../../core/models/cart.model';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyPipe, ProductImagePipe],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit {
  private cartService = inject(CartService);
  private cdr = inject(ChangeDetectorRef);
  
  cartItems: CartItem[] = [];
  loading = true;
  error = null as string | null;
  totalItems = 0;
  subtotal = 0;
  updatingItems = new Set<string>();

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading = true;
    this.error = null;
    
    this.cartService.getCart().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.cartItems = response.data.items || [];
          this.totalItems = response.data.totalItems || 0;
          this.subtotal = response.data.subTotal || 0;
        } else {
          this.error = response.message || 'Failed to load cart';
          this.cartItems = [];
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load cart. Please try again later.';
        this.loading = false;
        this.cartItems = [];
        this.cdr.detectChanges();
      }
    });
  }

  removeItem(itemId: string): void {
    if (this.updatingItems.has(itemId)) return;
    
    this.updatingItems.add(itemId);
    this.cartService.removeCartItem(itemId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.cartItems = response.data.items || [];
          this.totalItems = response.data.totalItems || 0;
          this.subtotal = response.data.subTotal || 0;
        }
        this.updatingItems.delete(itemId);
        this.cdr.detectChanges();
      },
      error: () => {
        this.updatingItems.delete(itemId);
        this.cdr.detectChanges();
      }
    });
  }

  updateQuantity(itemId: string, newQuantity: number): void {
    if (newQuantity < 1 || this.updatingItems.has(itemId)) return;
    
    this.updatingItems.add(itemId);
    this.cartService.updateCartItem(itemId, newQuantity).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.cartItems = response.data.items || [];
          this.totalItems = response.data.totalItems || 0;
          this.subtotal = response.data.subTotal || 0;
        }
        this.updatingItems.delete(itemId);
        this.cdr.detectChanges();
      },
      error: () => {
        this.updatingItems.delete(itemId);
        this.cdr.detectChanges();
      }
    });
  }

  isItemUpdating(itemId: string): boolean {
    return this.updatingItems.has(itemId);
  }
}
