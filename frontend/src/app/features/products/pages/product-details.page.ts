import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductsService } from '../services/products.service';
import { ReviewService } from '../services/review.service';
import { Product } from '../models/product.model';
import { Review, ReviewSummary } from '../models/review.model';
import { ProductImagePipe } from '../../../shared/pipes/product-image.pipe';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { StarRatingComponent } from '../../../shared/components/star-rating/star-rating.component';

@Component({
  selector: 'app-product-details-page',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, FormsModule, RouterLink, ProductImagePipe, StarRatingComponent],
  templateUrl: './product-details.page.html',
  styleUrl: './product-details.page.scss'
})
export class ProductDetailsPage implements OnInit {
  private productsService = inject(ProductsService);
  private reviewService = inject(ReviewService);
  private route = inject(ActivatedRoute);
  private cartService = inject(CartService);
  private wishlistService = inject(WishlistService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  product = signal<Product | null>(null);
  reviews = signal<Review[]>([]);
  reviewSummary = signal<ReviewSummary | null>(null);
  loading = signal(true);
  loadingReviews = signal(false);
  error = signal<string | null>(null);
  adding = signal(false);
  quantity = signal(1);
  showReviewForm = signal(false);
  submittingReview = signal(false);
  newReviewRating = signal(0);
  newReviewTitle = signal('');
  newReviewComment = signal('');
  userHasReviewed = signal(false);

  get isFavorite(): boolean {
    const p = this.product();
    return p ? this.wishlistService.isInWishlist(p.id) : false;
  }

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated;
  }

  get currentUserId(): string | null {
    return this.authService.currentUser?.email || null;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProduct(id);
      this.loadReviews(id);
    } else {
      this.error.set('Product ID is required.');
      this.loading.set(false);
    }
  }

  loadProduct(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.quantity.set(1);

    this.productsService.getProductById(id).subscribe({
      next: (response) => {
        if (response.data) {
          this.product.set(response.data);
        } else {
          this.error.set(response.message || 'Product not found.');
        }
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load product. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  loadReviews(productId: string): void {
    this.loadingReviews.set(true);
    this.reviewService.getProductReviews(productId).subscribe({
      next: (response) => {
        if (response.data) {
          this.reviews.set(response.data);
          if (this.currentUserId) {
            this.userHasReviewed.set(response.data.some(r => r.userId === this.currentUserId));
          }
        }
        this.loadingReviews.set(false);
      },
      error: () => {
        this.loadingReviews.set(false);
      }
    });

    this.reviewService.getProductReviewSummary(productId).subscribe({
      next: (response) => {
        if (response.data) {
          this.reviewSummary.set(response.data);
        }
      }
    });
  }

  incrementQuantity(): void {
    const current = this.quantity();
    const max = this.product()?.stockQuantity || 1;
    if (current < max) {
      this.quantity.set(current + 1);
    }
  }

  decrementQuantity(): void {
    const current = this.quantity();
    if (current > 1) {
      this.quantity.set(current - 1);
    }
  }

  onAddToCart(): void {
    const currentProduct = this.product();
    if (!currentProduct || this.adding()) return;

    this.adding.set(true);
    this.cartService.addToCart({ productId: currentProduct.id, quantity: this.quantity() }).subscribe({
      next: () => {
        this.toastService.success(`${currentProduct.name} added to cart`);
      },
      error: () => {
        this.toastService.error('Failed to add item to cart');
        this.adding.set(false);
      },
      complete: () => {
        this.adding.set(false);
      }
    });
  }

  onToggleWishlist(): void {
    const currentProduct = this.product();
    if (!currentProduct) return;

    this.wishlistService.toggleWishlist(currentProduct);
    if (this.isFavorite) {
      this.toastService.success(`${currentProduct.name} removed from wishlist`);
    } else {
      this.toastService.success(`${currentProduct.name} added to wishlist`);
    }
  }

  toggleReviewForm(): void {
    this.showReviewForm.set(!this.showReviewForm());
  }

  onRatingChange(rating: number): void {
    this.newReviewRating.set(rating);
  }

  submitReview(): void {
    const currentProduct = this.product();
    if (!currentProduct) return;

    if (!this.isAuthenticated) {
      this.toastService.error('Please log in to submit a review.');
      return;
    }

    if (this.newReviewRating() === 0) {
      this.toastService.error('Please select a rating');
      return;
    }

    if (!this.newReviewTitle().trim()) {
      this.toastService.error('Please enter a review title');
      return;
    }

    this.submittingReview.set(true);

    this.reviewService.createReview(currentProduct.id, {
      rating: this.newReviewRating(),
      title: this.newReviewTitle().trim(),
      comment: this.newReviewComment().trim() || null
    }).subscribe({
      next: (response) => {
        if (response.data) {
          this.reviews.update(reviews => [response.data!, ...reviews]);
          this.userHasReviewed.set(true);
          this.showReviewForm.set(false);
          this.newReviewRating.set(0);
          this.newReviewTitle.set('');
          this.newReviewComment.set('');
          this.toastService.success('Review submitted successfully');
          this.loadReviews(currentProduct.id);
        } else {
          this.toastService.error(response.message || 'Failed to submit review');
        }
        this.submittingReview.set(false);
      },
      error: (err) => {
        if (err?.status === 401) {
          this.toastService.error('Please log in to submit a review.');
        } else {
          this.toastService.error(err?.error?.message || 'Failed to submit review');
        }
        this.submittingReview.set(false);
      }
    });
  }

  deleteReview(reviewId: string): void {
    if (!confirm('Are you sure you want to delete your review?')) return;

    this.reviewService.deleteReview(reviewId).subscribe({
      next: (response) => {
        if (response.success) {
          this.reviews.update(reviews => reviews.filter(r => r.id !== reviewId));
          this.userHasReviewed.set(false);
          this.toastService.success('Review deleted successfully');
          const currentProduct = this.product();
          if (currentProduct) {
            this.loadReviews(currentProduct.id);
          }
        }
      },
      error: (err) => {
        if (err?.status === 401) {
          this.toastService.error('Please log in to manage your review.');
        } else {
          this.toastService.error('Failed to delete review');
        }
      }
    });
  }

  getUserReview(): Review | undefined {
    const userId = this.currentUserId;
    if (!userId) return undefined;
    return this.reviews().find(r => r.userId === userId);
  }

  getRatingPercentage(count: number): number {
    const summary = this.reviewSummary();
    if (!summary || summary.totalReviews === 0) return 0;
    return (count / summary.totalReviews) * 100;
  }
}
