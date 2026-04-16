import { Component, Input, Output, EventEmitter, HostListener, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface SortOption {
  value: string;
  label: string;
  sortBy: string;
  descending: boolean;
}

@Component({
  selector: 'app-sort-dropdown',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="sort-dropdown">
      <button 
        class="sort-dropdown__trigger"
        (click)="toggle()"
        [attr.aria-expanded]="isOpen"
        aria-haspopup="listbox"
      >
        <span class="sort-dropdown__label">{{ selectedLabel }}</span>
        <svg class="sort-dropdown__chevron" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="6 9 12 15 18 9"></polyline>
        </svg>
      </button>

      @if (isOpen) {
        <div class="sort-dropdown__menu" role="listbox">
          @for (option of options; track option.value) {
            <button
              class="sort-dropdown__option"
              [class.active]="option.value === selectedValue"
              (click)="selectOption(option)"
              role="option"
              [attr.aria-selected]="option.value === selectedValue"
            >
              {{ option.label }}
              @if (option.value === selectedValue) {
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="20 6 9 17 4 12"></polyline>
                </svg>
              }
            </button>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .sort-dropdown {
      position: relative;
      display: inline-block;

      &__trigger {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        padding: var(--space-2) var(--space-3);
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        font-size: 0.9rem;
        font-family: inherit;
        background-color: var(--color-surface);
        color: var(--color-text);
        cursor: pointer;
        min-width: 160px;
        transition: border-color var(--transition-fast), background-color var(--transition-fast);

        &:hover {
          border-color: var(--color-primary);
        }

        &:focus-visible {
          outline: none;
          border-color: var(--color-primary);
          box-shadow: 0 0 0 3px var(--color-primary-light);
        }
      }

      &__label {
        flex: 1;
        text-align: left;
      }

      &__chevron {
        flex-shrink: 0;
        color: var(--color-text-muted);
        transition: transform var(--transition-fast);
      }

      &__menu {
        position: absolute;
        top: calc(100% + 4px);
        left: 0;
        right: 0;
        background: var(--color-surface);
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-md);
        z-index: 100;
        overflow: hidden;
      }

      &__option {
        display: flex;
        align-items: center;
        justify-content: space-between;
        width: 100%;
        padding: var(--space-2) var(--space-3);
        border: none;
        font-size: 0.9rem;
        font-family: inherit;
        background: transparent;
        color: var(--color-text);
        cursor: pointer;
        text-align: left;
        transition: background-color var(--transition-fast);

        &:hover {
          background-color: var(--color-bg-secondary);
        }

        &.active {
          color: var(--color-primary);
          font-weight: 500;
        }

        svg {
          color: var(--color-primary);
        }
      }
    }
  `]
})
export class SortDropdownComponent {
  @Input() options: SortOption[] = [];
  @Input() selectedValue: string = '';
  @Output() selectionChange = new EventEmitter<SortOption>();

  isOpen = false;

  get selectedLabel(): string {
    const option = this.options.find(o => o.value === this.selectedValue);
    return option?.label || 'Sort by';
  }

  toggle(): void {
    this.isOpen = !this.isOpen;
  }

  selectOption(option: SortOption): void {
    this.selectionChange.emit(option);
    this.isOpen = false;
  }
}
