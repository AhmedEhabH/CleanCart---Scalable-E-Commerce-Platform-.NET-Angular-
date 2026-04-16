import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="star-rating" [class.interactive]="interactive">
      @for (star of stars; track star) {
        <button
          type="button"
          class="star"
          [class.filled]="star <= displayRating()"
          [class.half]="star === Math.ceil(displayRating()) && displayRating() % 1 !== 0"
          [disabled]="!interactive"
          (click)="interactive && selectRating(star)"
          (mouseenter)="interactive && previewRating.set(star)"
          (mouseleave)="interactive && previewRating.set(0)"
          [attr.aria-label]="star + ' star' + (star > 1 ? 's' : '')"
        >
          @if (interactive && star <= previewRating() && previewRating() > 0) {
            <svg viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/>
            </svg>
          } @else if (star <= displayRating()) {
            <svg viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/>
            </svg>
          } @else {
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/>
            </svg>
          }
        </button>
      }
      @if (showLabel && displayRating() > 0) {
        <span class="rating-label">{{ displayRating().toFixed(1) }}</span>
      }
    </div>
  `,
  styles: [`
    .star-rating {
      display: inline-flex;
      align-items: center;
      gap: 2px;
    }

    .star {
      background: none;
      border: none;
      padding: 2px;
      cursor: default;
      color: #d1d5db;
      transition: color 0.2s, transform 0.2s;
    }

    .interactive .star {
      cursor: pointer;
    }

    .interactive .star:hover {
      transform: scale(1.1);
    }

    .star.filled {
      color: #fbbf24;
    }

    .star svg {
      width: 20px;
      height: 20px;
    }

    @media (min-width: 768px) {
      .star svg {
        width: 24px;
        height: 24px;
      }
    }

    .rating-label {
      margin-left: 8px;
      font-size: 0.875rem;
      font-weight: 600;
      color: #374151;
    }

    :host-context(.dark) .star {
      color: #4b5563;
    }

    :host-context(.dark) .star.filled {
      color: #fbbf24;
    }

    :host-context(.dark) .rating-label {
      color: #d1d5db;
    }
  `]
})
export class StarRatingComponent {
  @Input() rating = 0;
  @Input() interactive = false;
  @Input() showLabel = false;
  @Output() ratingChange = new EventEmitter<number>();

  stars = [1, 2, 3, 4, 5];
  Math = Math;
  previewRating = signal(0);

  displayRating(): number {
    if (this.interactive && this.previewRating() > 0) {
      return this.previewRating();
    }
    return this.rating;
  }

  selectRating(star: number): void {
    this.rating = star;
    this.ratingChange.emit(star);
  }
}
