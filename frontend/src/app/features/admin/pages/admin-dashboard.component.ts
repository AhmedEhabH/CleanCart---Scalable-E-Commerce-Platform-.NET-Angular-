import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, ChartData, ChartType, registerables } from 'chart.js';
import { AdminService, DashboardSummary } from '../../../core/services/admin.service';

Chart.register(...registerables);
import { AnalyticsService } from '../../../core/services/analytics.service';
import { AnalyticsSummary, TopProduct, SalesTrend } from '../../../core/models/analytics.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, BaseChartDirective],
  template: `
    <div class="dashboard">
      <h1>Analytics Dashboard</h1>
      
      @if (loading()) {
        <p class="loading">Loading analytics data...</p>
      } @else {
        <div class="kpi-cards">
          <div class="kpi-card">
            <span class="kpi-label">Total Revenue</span>
            <span class="kpi-value">{{ analyticsSummary()?.totalRevenue | currency }}</span>
          </div>
          <div class="kpi-card">
            <span class="kpi-label">Total Orders</span>
            <span class="kpi-value">{{ analyticsSummary()?.totalOrders || 0 }}</span>
          </div>
          <div class="kpi-card">
            <span class="kpi-label">Total Users</span>
            <span class="kpi-value">{{ analyticsSummary()?.totalUsers || 0 }}</span>
          </div>
          <div class="kpi-card">
            <span class="kpi-label">Avg Order Value</span>
            <span class="kpi-value">{{ analyticsSummary()?.averageOrderValue | currency }}</span>
          </div>
        </div>

        <div class="chart-section">
          <div class="chart-card">
            <h2>Monthly Sales Trend</h2>
            @if (salesTrendChartData()) {
              <canvas baseChart
                [data]="salesTrendChartData()!"
                [options]="lineChartOptions"
                [type]="lineChartType">
              </canvas>
            } @else {
              <p class="empty">No sales data available</p>
            }
          </div>
        </div>

        <div class="dashboard-sections">
          <section>
            <h2>Top Selling Products</h2>
            @if (topProducts().length) {
              <table class="data-table">
                <thead>
                  <tr><th>Product</th><th>Sold</th><th>Revenue</th></tr>
                </thead>
                <tbody>
                  @for (product of topProducts(); track product.productId) {
                    <tr>
                      <td>{{ product.productName }}</td>
                      <td>{{ product.totalSold }}</td>
                      <td>{{ product.totalRevenue | currency }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else {
              <p class="empty">No top products data</p>
            }
          </section>

          <section>
            <h2>Recent Orders</h2>
            @if (dashboardSummary()?.recentOrders?.length) {
              <table class="data-table">
                <thead>
                  <tr><th>Order</th><th>Total</th><th>Status</th><th>Date</th></tr>
                </thead>
                <tbody>
                  @for (order of dashboardSummary()?.recentOrders; track order.id) {
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
        </div>
      }
    </div>
  `,
  styles: [`
    .dashboard { max-width: 1400px; padding: 1.5rem; }
    h1 { margin: 0 0 1.5rem; font-size: 1.75rem; }
    h2 { font-size: 1.1rem; margin: 0 0 1rem; }
    .loading { color: var(--text-secondary); }
    
    .kpi-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1rem;
      margin-bottom: 2rem;
    }
    .kpi-card {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 12px;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
    }
    .kpi-card:nth-child(2) { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); box-shadow: 0 4px 15px rgba(245, 87, 108, 0.3); }
    .kpi-card:nth-child(3) { background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); box-shadow: 0 4px 15px rgba(79, 172, 254, 0.3); }
    .kpi-card:nth-child(4) { background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%); box-shadow: 0 4px 15px rgba(67, 233, 123, 0.3); }
    .kpi-label { color: rgba(255,255,255,0.9); font-size: 0.875rem; font-weight: 500; }
    .kpi-value { font-size: 2rem; font-weight: 700; margin-top: 0.5rem; color: white; }
    
    .chart-section { margin-bottom: 2rem; }
    .chart-card {
      background: var(--card-bg);
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    }
    .chart-card canvas { max-height: 300px; }
    
    .dashboard-sections {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 1.5rem;
    }
    section {
      background: var(--card-bg);
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    }
    .empty { color: var(--text-secondary); font-size: 0.875rem; }
    
    .data-table { width: 100%; border-collapse: collapse; }
    .data-table th, .data-table td { padding: 0.75rem; text-align: left; border-bottom: 1px solid var(--border-color); }
    .data-table th { font-size: 0.75rem; color: var(--text-secondary); text-transform: uppercase; }
    .warning { color: var(--warning); font-weight: 600; }
    .btn-link { color: var(--primary); text-decoration: none; }
    .btn-link:hover { text-decoration: underline; }
    
    @media (max-width: 768px) {
      .kpi-cards, .dashboard-sections { grid-template-columns: 1fr; }
      .kpi-value { font-size: 1.5rem; }
    }
  `]
})
export class AdminDashboardComponent implements OnInit {
  private adminService = inject(AdminService);
  private analyticsService = inject(AnalyticsService);
  
  dashboardSummary = signal<DashboardSummary | null>(null);
  analyticsSummary = signal<AnalyticsSummary | null>(null);
  topProducts = signal<TopProduct[]>([]);
  salesTrend = signal<SalesTrend[]>([]);
  loading = signal(true);

  salesTrendChartData = signal<ChartData<'line'> | null>(null);
  
  public lineChartType: ChartType = 'line';
  public lineChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: true, position: 'top' }
    },
    scales: {
      y: { beginAtZero: true }
    }
  };

  ngOnInit(): void {
    this.loadDashboard();
    this.loadAnalytics();
  }

  private loadDashboard(): void {
    this.adminService.getDashboard().subscribe({
      next: (data) => this.dashboardSummary.set(data),
      error: () => {}
    });
  }

  private loadAnalytics(): void {
    this.analyticsService.getAnalytics().subscribe({
      next: (response) => {
        if (response.data) {
          this.analyticsSummary.set(response.data.summary);
          this.topProducts.set(response.data.topProducts);
          this.salesTrend.set(response.data.salesTrend);
          this.setupChart();
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private setupChart(): void {
    const trend = this.salesTrend();
    if (!trend.length) return;

    this.salesTrendChartData.set({
      labels: trend.map(t => t.monthName),
      datasets: [
        {
          data: trend.map(t => t.revenue),
          label: 'Revenue',
          fill: true,
          tension: 0.4,
          borderColor: '#667eea',
          backgroundColor: 'rgba(102, 126, 234, 0.1)'
        },
        {
          data: trend.map(t => t.orderCount),
          label: 'Orders',
          fill: true,
          tension: 0.4,
          borderColor: '#f5576c',
          backgroundColor: 'rgba(245, 87, 108, 0.1)'
        }
      ]
    });
  }
}