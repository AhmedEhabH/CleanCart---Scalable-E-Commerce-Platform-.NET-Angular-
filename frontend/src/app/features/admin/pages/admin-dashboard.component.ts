import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService, DashboardSummary } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink],
  template: `
    <div class="dashboard">
      <h1>Dashboard</h1>
      
      @if (loading()) {
        <p class="loading">Loading...</p>
      } @else {
        <div class="kpi-cards">
          <div class="kpi-card">
            <span class="kpi-label">Total Orders</span>
            <span class="kpi-value">{{ summary()?.totalOrders || 0 }}</span>
          </div>
          <div class="kpi-card">
            <span class="kpi-label">Total Sales</span>
            <span class="kpi-value">{{ summary()?.totalSales | currency }}</span>
          </div>
          <div class="kpi-card">
            <span class="kpi-label">Products</span>
            <span class="kpi-value">{{ summary()?.totalProducts || 0 }}</span>
          </div>
          <div class="kpi-card">
            <span class="kpi-label">Users</span>
            <span class="kpi-value">{{ summary()?.totalUsers || 0 }}</span>
          </div>
        </div>

        <div class="dashboard-sections">
          <section>
            <h2>Recent Orders</h2>
            @if (summary()?.recentOrders?.length) {
              <table class="data-table">
                <thead>
                  <tr><th>Order</th><th>Total</th><th>Status</th><th>Date</th></tr>
                </thead>
                <tbody>
                  @for (order of summary()?.recentOrders; track order.id) {
                    <tr>
                      <td>{{ order.orderNumber }}</td>
                      <td>{{ order.totalAmount | currency }}</td>
                      <td>{{ order.status }}</td>
                      <td>{{ order.createdAt | date:'short' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else {
              <p class="empty">No recent orders</p>
            }
          </section>

          <section>
            <h2>Low Stock Products</h2>
            @if (summary()?.lowStockProducts?.length) {
              <table class="data-table">
                <thead>
                  <tr><th>Product</th><th>Stock</th><th>Action</th></tr>
                </thead>
                <tbody>
                  @for (product of summary()?.lowStockProducts; track product.id) {
                    <tr>
                      <td>{{ product.name }}</td>
                      <td class="warning">{{ product.stockQuantity }}</td>
                      <td>
                        <a [routerLink]="['/admin/products', product.id, 'edit']" class="btn-link">Edit</a>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else {
              <p class="empty">No low stock products</p>
            }
          </section>
        </div>
      }
    </div>
  `,
  styles: [`
    .dashboard { max-width: 1200px; }
    h1 { margin: 0 0 1.5rem; }
    h2 { font-size: 1.1rem; margin: 0 0 1rem; }
    .loading { color: var(--text-secondary); }
    
    .kpi-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
      margin-bottom: 2rem;
    }
    .kpi-card {
      background: var(--card-bg);
      border-radius: 8px;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
    }
    .kpi-label { color: var(--text-secondary); font-size: 0.875rem; }
    .kpi-value { font-size: 1.75rem; font-weight: 600; margin-top: 0.5rem; }
    
    .dashboard-sections {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 1.5rem;
    }
    section {
      background: var(--card-bg);
      border-radius: 8px;
      padding: 1.5rem;
    }
    .empty { color: var(--text-secondary); font-size: 0.875rem; }
    
    .data-table {
      width: 100%;
      border-collapse: collapse;
    }
    .data-table th, .data-table td {
      padding: 0.75rem;
      text-align: left;
      border-bottom: 1px solid var(--border-color);
    }
    .data-table th { font-size: 0.75rem; color: var(--text-secondary); text-transform: uppercase; }
    .warning { color: var(--warning); font-weight: 600; }
    .btn-link { color: var(--primary); text-decoration: none; }
    .btn-link:hover { text-decoration: underline; }
    
    @media (max-width: 640px) {
      .kpi-cards, .dashboard-sections { grid-template-columns: 1fr; }
    }
  `]
})
export class AdminDashboardComponent implements OnInit {
  private adminService = inject(AdminService);
  
  summary = signal<DashboardSummary | null>(null);
  loading = signal(true);

  ngOnInit(): void {
    this.adminService.getDashboard().subscribe({
      next: (data) => { this.summary.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}