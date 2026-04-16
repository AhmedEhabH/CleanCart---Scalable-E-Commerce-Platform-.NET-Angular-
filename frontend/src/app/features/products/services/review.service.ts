import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Review, CreateReviewRequest, UpdateReviewRequest, ReviewSummary } from '../models/review.model';
import { ApiResponse } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = environment.apiBaseUrl;

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    return headers;
  }

  getProductReviews(productId: string): Observable<ApiResponse<Review[]>> {
    return this.http.get<ApiResponse<Review[]>>(`${this.baseUrl}/reviews/product/${productId}`);
  }

  getProductReviewSummary(productId: string): Observable<ApiResponse<ReviewSummary>> {
    return this.http.get<ApiResponse<ReviewSummary>>(`${this.baseUrl}/reviews/product/${productId}/summary`);
  }

  getReviewById(reviewId: string): Observable<ApiResponse<Review>> {
    return this.http.get<ApiResponse<Review>>(`${this.baseUrl}/reviews/${reviewId}`);
  }

  hasUserPurchasedProduct(productId: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/reviews/product/${productId}/has-purchased`);
  }

  createReview(productId: string, request: CreateReviewRequest): Observable<ApiResponse<Review>> {
    return this.http.post<ApiResponse<Review>>(
      `${this.baseUrl}/reviews/product/${productId}`,
      request,
      { headers: this.getAuthHeaders() }
    );
  }

  updateReview(reviewId: string, request: UpdateReviewRequest): Observable<ApiResponse<Review>> {
    return this.http.put<ApiResponse<Review>>(
      `${this.baseUrl}/reviews/${reviewId}`,
      request,
      { headers: this.getAuthHeaders() }
    );
  }

  deleteReview(reviewId: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(
      `${this.baseUrl}/reviews/${reviewId}`,
      { headers: this.getAuthHeaders() }
    );
  }
}
