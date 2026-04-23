export interface ProductImage {
  id: string;
  imageUrl: string;
  altText: string | null;
  displayOrder: number;
}

export interface Product {
  id: string;
  vendorId: string;
  categoryId: string;
  name: string;
  slug: string;
  description: string | null;
  price: number;
  compareAtPrice: number | null;
  sku: string;
  stockQuantity: number;
  lowStockThreshold: number;
  isFeatured: boolean;
  isActive: boolean;
  reviewCount: number;
  averageRating: number;
  isInStock: boolean;
  isLowStock: boolean;
  hasDiscount: boolean;
  discountPercentage: number;
  mainImageUrl: string | null;
  images: ProductImage[];
  createdAt: string;
  updatedAt: string;
  sellerName?: string;
  sellerDescription?: string;
}

export interface SellerSummary {
  id: string;
  businessName: string;
  description: string | null;
  logoUrl: string | null;
  productsCount: number;
}
