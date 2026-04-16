import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { OrderService } from '../../core/services/order.service';
import { OrderDto } from '../../core/models/order.model';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyPipe, DatePipe],
  template: `
    <div class="page">
      <div class="container">
        <div class="page-header">
          <h1>My Orders</h1>
          <a routerLink="/products" class="btn">Continue Shopping</a>
        </div>

        @if (loading) {
          <div class="loading-state">
            <p>Loading orders...</p>
          </div>
        } @else if (error) {
          <div class="error-state">
            <p>{{ error }}</p>
            <button class="btn" (click)="loadOrders()">Try Again</button>
          </div>
        } @else if (orders.length === 0) {
          <div class="empty-state">
            <div class="empty-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"></path>
                <line x1="3" y1="6" x2="21" y2="6"></line>
                <path d="M16 10a4 4 0 0 1-8 0"></path>
              </svg>
            </div>
            <h2>No Orders Yet</h2>
            <p>You haven't placed any orders yet. Start shopping to see your orders here.</p>
            <a routerLink="/products" class="btn">Browse Products</a>
          </div>
        } @else {
          <div class="orders-list">
            @for (order of orders; track order.id) {
              <div class="order-card">
                <div class="order-card__header">
                  <div class="order-card__info">
                    <span class="order-card__number">Order #{{ order.orderNumber }}</span>
                    <span class="order-card__date">{{ order.createdAt | date:'medium' }}</span>
                  </div>
                  <span class="order-card__status order-card__status--{{ order.status.toLowerCase() }}">
                    {{ order.status }}
                  </span>
                </div>
                <div class="order-card__body">
                  <div class="order-card__details">
                    <div class="order-card__detail">
                      <span class="order-card__label">Items</span>
                      <span class="order-card__value">{{ order.totalItems }} item(s)</span>
                    </div>
                    <div class="order-card__detail">
                      <span class="order-card__label">Total</span>
                      <span class="order-card__value order-card__value--total">{{ order.totalAmount | currency }}</span>
                    </div>
                    @if (order.shippingAddress) {
                      <div class="order-card__detail">
                        <span class="order-card__label">Shipped To</span>
                        <span class="order-card__value">{{ order.shippingAddress.city }}, {{ order.shippingAddress.country }}</span>
                      </div>
                    }
                  </div>
                </div>
              </div>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-6);
      padding-bottom: var(--space-4);
      border-bottom: 1px solid var(--color-border);

      h1 {
        margin: 0;
      }
    }

    .loading-state,
    .error-state,
    .empty-state {
      text-align: center;
      padding: var(--space-12) var(--space-4);
    }

    .empty-icon {
      color: var(--color-text-muted);
      margin-bottom: var(--space-4);
    }

    .empty-state {
      h2 {
        margin-bottom: var(--space-2);
      }

      p {
        color: var(--color-text-secondary);
        margin-bottom: var(--space-6);
        max-width: 400px;
        margin-left: auto;
        margin-right: auto;
      }
    }

    .error-state {
      p {
        color: var(--color-error);
        margin-bottom: var(--space-4);
      }
    }

    .orders-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }

    .order-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      overflow: hidden;
      transition: box-shadow 0.2s ease;

      &:hover {
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      }

      &__header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: var(--space-4);
        background: var(--color-surface-elevated);
        border-bottom: 1px solid var(--color-border);
      }

      &__info {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
      }

      &__number {
        font-weight: 600;
        font-size: 1rem;
      }

      &__date {
        font-size: 0.875rem;
        color: var(--color-text-secondary);
      }

      &__status {
        padding: var(--space-1) var(--space-3);
        border-radius: var(--radius-full);
        font-size: 0.75rem;
        font-weight: 600;
        text-transform: uppercase;

        &--pending {
          background: rgba(245, 158, 11, 0.1);
          color: #d97706;
        }

        &--processing {
          background: rgba(59, 130, 246, 0.1);
          color: #2563eb;
        }

        &--shipped {
          background: rgba(139, 92, 246, 0.1);
          color: #7c3aed;
        }

        &--delivered {
          background: rgba(34, 197, 94, 0.1);
          color: #16a34a;
        }

        &--cancelled {
          background: rgba(239, 68, 68, 0.1);
          color: #dc2626;
        }

        &--completed {
          background: rgba(34, 197, 94, 0.1);
          color: #16a34a;
        }
      }

      &__body {
        padding: var(--space-4);
      }

      &__details {
        display: flex;
        gap: var(--space-6);
        flex-wrap: wrap;
      }

      &__detail {
        display: flex;
        flex-direction: column;
        gap: var(--space-1);
      }

      &__label {
        font-size: 0.75rem;
        color: var(--color-text-muted);
        text-transform: uppercase;
      }

      &__value {
        font-weight: 500;

        &--total {
          font-size: 1.125rem;
          font-weight: 700;
          color: var(--color-primary);
        }
      }
    }

    @media (max-width: 480px) {
      .page-header {
        flex-direction: column;
        gap: var(--space-3);
        text-align: center;

        h1 {
          margin: 0;
        }
      }

      .order-card {
        &__header {
          flex-direction: column;
          align-items: flex-start;
          gap: var(--space-2);
        }

        &__details {
          flex-direction: column;
          gap: var(--space-3);
        }
      }
    }
  `]
})
export class OrdersComponent implements OnInit {
  private orderService = inject(OrderService);
  private cdr = inject(ChangeDetectorRef);

  orders: OrderDto[] = [];
  loading = true;
  error: string | null = null;

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = null;

    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to load orders. Please try again.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
