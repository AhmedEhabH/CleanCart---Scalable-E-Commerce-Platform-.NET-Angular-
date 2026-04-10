import { Component, inject, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { ProductImagePipe } from '../../shared/pipes/product-image.pipe';
import { CartService, CartState } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { CartItem } from '../../core/models/cart.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, CurrencyPipe, ProductImagePipe],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private cdr = inject(ChangeDetectorRef);
  
  private cartSub?: Subscription;

  cartItems: CartItem[] = [];
  totalItems = 0;
  subtotal = 0;
  loading = true;
  submitting = false;
  error: string | null = null;

  checkoutForm: FormGroup;

  constructor() {
    this.checkoutForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      street: ['', [Validators.required]],
      city: ['', [Validators.required]],
      state: ['', []],
      postalCode: ['', []],
      country: ['', [Validators.required]],
      notes: ['', [Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    this.loadCart();
  }

  ngOnDestroy(): void {
    this.cartSub?.unsubscribe();
  }

  loadCart(): void {
    this.loading = true;
    this.error = null;

    this.cartSub?.unsubscribe();
    this.cartSub = this.cartService.cartState$.subscribe({
      next: (state: CartState) => {
        this.cartItems = state.items as CartItem[];
        this.totalItems = state.totalItems;
        this.subtotal = state.subTotal;
        this.loading = false;

        if (this.cartItems.length === 0) {
          this.router.navigate(['/cart']);
        }
        
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load cart. Please try again.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get formControls() {
    return this.checkoutForm.controls;
  }

  hasError(field: string, errorType?: string): boolean {
    const control = this.checkoutForm.get(field);
    if (!control || !control.touched) return false;
    if (errorType) {
      return control.hasError(errorType);
    }
    return control.invalid;
  }

  placeOrder(): void {
    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    if (this.cartItems.length === 0) {
      this.error = 'Your cart is empty. Please add items before placing an order.';
      return;
    }

    this.submitting = true;
    this.error = null;

    const formValue = this.checkoutForm.value;
    const request = {
      shippingAddress: {
        street: formValue.street,
        city: formValue.city,
        state: formValue.state || '',
        postalCode: formValue.postalCode || '',
        country: formValue.country
      },
      notes: formValue.notes || undefined
    };

    this.orderService.placeOrder(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.router.navigate(['/order-success'], {
            queryParams: { orderId: response.data.id }
          });
        } else {
          this.error = response.message || 'Failed to place order. Please try again.';
          this.submitting = false;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.error = err.error?.message || 'An error occurred. Please try again.';
        this.submitting = false;
        this.cdr.detectChanges();
      }
    });
  }
}
