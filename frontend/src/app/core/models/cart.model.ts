export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSlug: string;
  productImageUrl?: string;
  quantity: number;
  unitPrice: number;
  total: number;
  isInStock: boolean;
}

export interface CartResponse {
  success: boolean;
  message?: string | null;
  data?: {
    id: string;
    items: CartItem[];
    totalItems: number;
    subTotal: number;
    isEmpty: boolean;
  };
  errors?: string[] | null;
}

export interface AddToCartRequest {
  productId: string;
  quantity?: number;
}