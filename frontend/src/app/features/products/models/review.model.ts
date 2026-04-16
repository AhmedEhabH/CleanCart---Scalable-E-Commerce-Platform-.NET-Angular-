export interface Review {
  id: string;
  productId: string;
  userId: string;
  userName: string;
  rating: number;
  title: string;
  comment: string | null;
  isVerifiedPurchase: boolean;
  createdAt: string;
}

export interface CreateReviewRequest {
  rating: number;
  title: string;
  comment: string | null;
}

export interface UpdateReviewRequest {
  rating: number;
  title: string;
  comment: string | null;
}

export interface ReviewSummary {
  totalReviews: number;
  averageRating: number;
  oneStar: number;
  twoStars: number;
  threeStars: number;
  fourStars: number;
  fiveStars: number;
}
