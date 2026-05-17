export interface AnalyticsSummary {
  totalUsers: number;
  totalOrders: number;
  totalRevenue: number;
  totalProducts: number;
  averageOrderValue: number;
}

export interface TopProduct {
  productId: string;
  productName: string;
  totalSold: number;
  totalRevenue: number;
}

export interface SalesTrend {
  month: number;
  monthName: string;
  orderCount: number;
  revenue: number;
}

export interface AnalyticsData {
  summary: AnalyticsSummary;
  topProducts: TopProduct[];
  salesTrend: SalesTrend[];
}