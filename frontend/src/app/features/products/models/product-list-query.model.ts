export interface ProductListQuery {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: string;
  sortBy?: string;
  sortDescending?: boolean;
  isFeatured?: boolean;
  isInStock?: boolean;
}
