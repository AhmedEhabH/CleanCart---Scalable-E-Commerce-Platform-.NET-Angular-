import { Component, inject, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShoppingAssistantService, AssistantFilter } from '../../services/shopping-assistant.service';

@Component({
  selector: 'app-shopping-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="assistant" [class.assistant--open]="isOpen()">
      @if (isOpen()) {
        <div class="assistant__panel">
          <div class="assistant__header">
            <div class="assistant__title">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 8V4H8"></path>
                <rect x="2" y="2" width="20" height="20" rx="5" ry="5"></rect>
                <path d="M2 12h4"></path>
                <path d="M2 16h4"></path>
                <path d="M2 8h4"></path>
                <path d="M18 12h4"></path>
                <path d="M18 16h4"></path>
                <path d="M18 8h4"></path>
              </svg>
              <span>Shop Assistant</span>
            </div>
            <button class="assistant__close" (click)="toggle()">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18"></line>
                <line x1="6" y1="6" x2="18" y2="18"></line>
              </svg>
            </button>
          </div>
          
          @if (lastMessage()) {
            <div class="assistant__message">
              <p>{{ lastMessage() }}</p>
            </div>
          }

          <div class="assistant__suggestions">
            @for (sug of suggestions; track sug) {
              <button class="assistant__chip" (click)="onSuggestion(sug)">{{ sug }}</button>
            }
          </div>

          <div class="assistant__input-row">
            <input 
              type="text" 
              class="assistant__input"
              placeholder="Try: cheap laptop"
              [(ngModel)]="query"
              (keydown.enter)="send()"
            />
            <button class="assistant__send" (click)="send()" [disabled]="!query.trim()">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="22" y1="2" x2="11" y2="13"></line>
                <polygon points="22 2 15 22 11 13 2 9 22 2"></polygon>
              </svg>
            </button>
          </div>
        </div>
      }
      
      <button class="assistant__fab" (click)="toggle()" [attr.aria-label]="isOpen() ? 'Close assistant' : 'Open shopping assistant'">
        @if (isOpen()) {
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        } @else {
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 8V4H8"></path>
            <rect x="2" y="2" width="20" height="20" rx="5" ry="5"></rect>
            <path d="M2 12h4"></path>
            <path d="M2 16h4"></path>
            <path d="M2 8h4"></path>
            <path d="M18 12h4"></path>
            <path d="M18 16h4"></path>
            <path d="M18 8h4"></path>
          </svg>
        }
      </button>
    </div>
  `,
  styles: [`
    .assistant {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 1000;
    }
    .assistant__fab {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: var(--color-primary);
      color: white;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 4px 12px rgba(0,0,0,0.2);
      transition: transform 0.2s, background-color 0.2s;
    }
    .assistant__fab:hover {
      transform: scale(1.05);
      background: var(--color-primary-dark);
    }
    .assistant__panel {
      position: absolute;
      bottom: 70px;
      right: 0;
      width: 320px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: 12px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.15);
      overflow: hidden;
    }
    .assistant__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      background: var(--color-surface-elevated);
      border-bottom: 1px solid var(--color-border);
    }
    .assistant__title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
      font-size: 0.95rem;
    }
    .assistant__close {
      background: none;
      border: none;
      cursor: pointer;
      color: var(--color-text-muted);
      padding: 4px;
    }
    .assistant__close:hover { color: var(--color-text); }
    .assistant__message {
      padding: 12px 16px;
      background: var(--color-primary-light);
      color: var(--color-primary);
      font-size: 0.9rem;
    }
    .assistant__message p { margin: 0; }
    .assistant__suggestions {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      padding: 12px 16px;
      border-bottom: 1px solid var(--color-border);
    }
    .assistant__chip {
      padding: 6px 12px;
      background: var(--color-bg-secondary);
      border: 1px solid var(--color-border);
      border-radius: 16px;
      font-size: 0.8rem;
      cursor: pointer;
      transition: all 0.2s;
    }
    .assistant__chip:hover {
      background: var(--color-primary-light);
      border-color: var(--color-primary);
      color: var(--color-primary);
    }
    .assistant__input-row {
      display: flex;
      gap: 8px;
      padding: 12px 16px;
    }
    .assistant__input {
      flex: 1;
      padding: 10px 12px;
      border: 1px solid var(--color-border);
      border-radius: 8px;
      font-size: 0.9rem;
      background: var(--color-surface);
      color: var(--color-text);
    }
    .assistant__input:focus {
      outline: none;
      border-color: var(--color-primary);
    }
    .assistant__send {
      width: 40px;
      height: 40px;
      border-radius: 8px;
      background: var(--color-primary);
      color: white;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .assistant__send:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .assistant__send:not(:disabled):hover {
      background: var(--color-primary-dark);
    }
    @media (max-width: 480px) {
      .assistant { bottom: 16px; right: 16px; }
      .assistant__panel { width: calc(100vw - 32px); right: -8px; }
    }
  `]
})
export class ShoppingAssistantComponent {
  private assistantService = inject(ShoppingAssistantService);
  
  @Output() filtersChange = new EventEmitter<AssistantFilter>();
  @Output() resetFilters = new EventEmitter<void>();

  isOpen = signal(false);
  lastMessage = signal<string | null>(null);
  query = '';
  suggestions = this.assistantService.getSuggestions();

  toggle(): void {
    this.isOpen.update(v => !v);
  }

  send(): void {
    if (!this.query.trim()) return;
    
    const response = this.assistantService.parseQuery(this.query);
    this.lastMessage.set(response.message);
    
    if (Object.keys(response.filters).length === 0) {
      this.resetFilters.emit();
    } else {
      this.filtersChange.emit(response.filters);
    }
    
    this.query = '';
  }

  onSuggestion(suggestion: string): void {
    this.query = suggestion;
    this.send();
  }
}
