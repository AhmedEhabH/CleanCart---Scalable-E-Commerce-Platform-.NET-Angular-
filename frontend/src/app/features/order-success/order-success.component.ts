import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { OrderService } from '../../core/services/order.service';
import { OrderDto } from '../../core/models/order.model';

@Component({
  selector: 'app-order-success',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyPipe],
  template: `
    <div class="page">
      <div class="container container--narrow">
        @if (order) {
          <div class="success-card">
            <div class="success-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                <polyline points="22 4 12 14.01 9 11.01"></polyline>
              </svg>
            </div>
            <h1>Order Placed Successfully!</h1>
            <p class="success-subtitle">Thank you for your purchase.</p>
            
            <div class="order-info">
              <div class="order-info__row">
                <span class="order-info__label">Order Number</span>
                <span class="order-info__value">{{ order.orderNumber }}</span>
              </div>
              <div class="order-info__row">
                <span class="order-info__label">Status</span>
                <span class="order-info__value order-info__value--status">{{ order.status }}</span>
              </div>
              <div class="order-info__row">
                <span class="order-info__label">Total Amount</span>
                <span class="order-info__value">{{ order.totalAmount | currency }}</span>
              </div>
              <div class="order-info__row">
                <span class="order-info__label">Items</span>
                <span class="order-info__value">{{ order.totalItems }} item(s)</span>
              </div>
            </div>

            <div class="success-actions">
              <a routerLink="/products" class="btn">Continue Shopping</a>
              <button class="btn btn--secondary" (click)="clearAndGoHome()">Back to Home</button>
            </div>
          </div>
        } @else {
          <div class="error-card">
            <div class="error-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"></circle>
                <line x1="12" y1="8" x2="12" y2="12"></line>
                <line x1="12" y1="16" x2="12.01" y2="16"></line>
              </svg>
            </div>
            <h1>Order Not Found</h1>
            <p class="error-subtitle">We couldn't find the order details.</p>
            <div class="success-actions">
              <a routerLink="/" class="btn">Back to Home</a>
              <a routerLink="/products" class="btn btn--secondary">Browse Products</a>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .success-card,
    .error-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      padding: var(--space-8);
      text-align: center;
    }

    .success-icon {
      color: var(--color-success);
      margin-bottom: var(--space-4);
    }

    .error-icon {
      color: var(--color-text-muted);
      margin-bottom: var(--space-4);
    }

    h1 {
      margin-bottom: var(--space-2);
    }

    .success-subtitle,
    .error-subtitle {
      color: var(--color-text-secondary);
      margin-bottom: var(--space-6);
    }

    .order-info {
      background: var(--color-bg);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      padding: var(--space-4);
      margin-bottom: var(--space-6);
      text-align: left;

      &__row {
        display: flex;
        justify-content: space-between;
        padding: var(--space-2) 0;

        &:not(:last-child) {
          border-bottom: 1px solid var(--color-border);
        }
      }

      &__label {
        color: var(--color-text-secondary);
      }

      &__value {
        font-weight: 600;

        &--status {
          color: var(--color-primary);
          text-transform: capitalize;
        }
      }
    }

    .success-actions {
      display: flex;
      gap: var(--space-3);
      justify-content: center;

      .btn {
        min-width: 160px;
      }

      .btn--secondary {
        background: var(--color-surface-elevated);
        color: var(--color-text);
        border: 1px solid var(--color-border);
      }
    }

    @media (max-width: 480px) {
      .success-actions {
        flex-direction: column;

        .btn {
          width: 100%;
        }
      }
    }
  `]
})
export class OrderSuccessComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);

  order: OrderDto | null = null;

  ngOnInit(): void {
    const orderId = this.route.snapshot.queryParamMap.get('orderId');
    this.order = this.orderService.getLastOrder();
    
    if (orderId && this.order && this.order.id !== orderId) {
      this.order = null;
    }
  }

  clearAndGoHome(): void {
    this.orderService.clearLastOrder();
    this.router.navigate(['/']);
  }
}
