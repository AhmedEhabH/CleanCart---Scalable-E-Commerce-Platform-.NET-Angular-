import { Component, inject, signal, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiChatService, ChatHistoryItem } from '../../services/ai-chat.service';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

@Component({
  selector: 'app-shopping-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="assistant" [class.assistant--open]="isOpen()">
      @if (isOpen()) {
        <div class="assistant__panel" @panelAnimation>
          <div class="assistant__header">
            <div class="assistant__title">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 2a4 4 0 0 1 4 4v2a4 4 0 0 1-8 0V6a4 4 0 0 1 4-4z"></path>
                <path d="M16 14H8a4 4 0 0 0-4 4v2h16v-2a4 4 0 0 0-4-4z"></path>
                <circle cx="9" cy="7" r="1" fill="currentColor"></circle>
                <circle cx="15" cy="7" r="1" fill="currentColor"></circle>
              </svg>
              <span>AI Shopping Assistant</span>
            </div>
            <button class="assistant__close" (click)="toggle()" aria-label="Close assistant">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18"></line>
                <line x1="6" y1="6" x2="18" y2="18"></line>
              </svg>
            </button>
          </div>

          <div class="assistant__chat" #chatContainer>
            @for (msg of messages(); track msg.timestamp) {
              <div class="assistant__message" [class.assistant__message--user]="msg.role === 'user'" [class.assistant__message--assistant]="msg.role === 'assistant'">
                <div class="assistant__message-bubble">
                  <p>{{ msg.content }}</p>
                </div>
              </div>
            }
            @if (isLoading()) {
              <div class="assistant__message assistant__message--assistant">
                <div class="assistant__message-bubble assistant__message-bubble--loading">
                  <div class="typing-indicator">
                    <span></span>
                    <span></span>
                    <span></span>
                  </div>
                </div>
              </div>
            }
          </div>

          @if (messages().length === 0) {
            <div class="assistant__suggestions">
              @for (sug of suggestions; track sug) {
                <button class="assistant__chip" (click)="onSuggestion(sug)">{{ sug }}</button>
              }
            </div>
          }

          <div class="assistant__input-row">
            <input 
              type="text" 
              class="assistant__input"
              placeholder="Ask me anything about products..."
              [(ngModel)]="inputMessage"
              (keydown.enter)="sendMessage()"
              [disabled]="isLoading()"
            />
            <button class="assistant__send" (click)="sendMessage()" [disabled]="!inputMessage.trim() || isLoading()">
              @if (isLoading()) {
                <svg class="spinner" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M21 12a9 9 0 11-6.219-8.56"></path>
                </svg>
              } @else {
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <line x1="22" y1="2" x2="11" y2="13"></line>
                  <polygon points="22 2 15 22 11 13 2 9 22 2"></polygon>
                </svg>
              }
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
            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
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
      transition: transform 0.2s ease, background-color 0.2s ease;
    }

    .assistant__fab:hover {
      transform: scale(1.05);
      background: var(--color-primary-dark);
    }

    .assistant__panel {
      position: absolute;
      bottom: 70px;
      right: 0;
      width: 380px;
      max-height: 520px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: 16px;
      box-shadow: 0 12px 32px rgba(0,0,0,0.15);
      display: flex;
      flex-direction: column;
      overflow: hidden;
      animation: slideUp 0.25s ease-out;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY(10px) scale(0.95);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }

    .assistant__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 14px 16px;
      background: var(--color-primary);
      color: white;
    }

    .assistant__title {
      display: flex;
      align-items: center;
      gap: 10px;
      font-weight: 600;
      font-size: 0.95rem;
    }

    .assistant__close {
      background: none;
      border: none;
      cursor: pointer;
      color: rgba(255,255,255,0.8);
      padding: 4px;
      border-radius: 4px;
      transition: color 0.2s, background-color 0.2s;
    }

    .assistant__close:hover {
      color: white;
      background: rgba(255,255,255,0.1);
    }

    .assistant__chat {
      flex: 1;
      overflow-y: auto;
      padding: 16px;
      display: flex;
      flex-direction: column;
      gap: 12px;
      min-height: 280px;
      max-height: 340px;
      scroll-behavior: smooth;
    }

    .assistant__message {
      display: flex;
      animation: fadeIn 0.2s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(4px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .assistant__message--user {
      justify-content: flex-end;
    }

    .assistant__message--assistant {
      justify-content: flex-start;
    }

    .assistant__message-bubble {
      max-width: 85%;
      padding: 10px 14px;
      border-radius: 12px;
      font-size: 0.875rem;
      line-height: 1.5;
    }

    .assistant__message--user .assistant__message-bubble {
      background: var(--color-primary);
      color: white;
      border-bottom-right-radius: 4px;
    }

    .assistant__message--assistant .assistant__message-bubble {
      background: var(--color-bg-secondary);
      color: var(--color-text);
      border-bottom-left-radius: 4px;
    }

    .assistant__message-bubble p {
      margin: 0;
      white-space: pre-wrap;
    }

    .assistant__message-bubble--loading {
      padding: 12px 16px;
    }

    .typing-indicator {
      display: flex;
      gap: 4px;
      align-items: center;
    }

    .typing-indicator span {
      width: 6px;
      height: 6px;
      background: var(--color-text-muted);
      border-radius: 50%;
      animation: bounce 1.4s infinite ease-in-out;
    }

    .typing-indicator span:nth-child(1) { animation-delay: -0.32s; }
    .typing-indicator span:nth-child(2) { animation-delay: -0.16s; }

    @keyframes bounce {
      0%, 80%, 100% { transform: scale(0); }
      40% { transform: scale(1); }
    }

    .assistant__suggestions {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      padding: 12px 16px;
      border-top: 1px solid var(--color-border);
      background: var(--color-bg-secondary);
    }

    .assistant__chip {
      padding: 6px 12px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: 16px;
      font-size: 0.8rem;
      cursor: pointer;
      color: var(--color-text-secondary);
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
      border-top: 1px solid var(--color-border);
      background: var(--color-surface);
    }

    .assistant__input {
      flex: 1;
      padding: 10px 14px;
      border: 1px solid var(--color-border);
      border-radius: 20px;
      font-size: 0.875rem;
      background: var(--color-bg-secondary);
      color: var(--color-text);
      transition: border-color 0.2s, box-shadow 0.2s;
    }

    .assistant__input:focus {
      outline: none;
      border-color: var(--color-primary);
      box-shadow: 0 0 0 2px var(--color-primary-light);
    }

    .assistant__input:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .assistant__send {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: var(--color-primary);
      color: white;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background-color 0.2s, transform 0.1s;
    }

    .assistant__send:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .assistant__send:not(:disabled):hover {
      background: var(--color-primary-dark);
    }

    .assistant__send:not(:disabled):active {
      transform: scale(0.95);
    }

    .spinner {
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }

    @media (max-width: 480px) {
      .assistant { bottom: 16px; right: 16px; }
      .assistant__panel { 
        width: calc(100vw - 32px); 
        right: -8px;
        max-height: 70vh;
      }
    }
  `]
})
export class ShoppingAssistantComponent implements AfterViewInit {
  private aiChatService = inject(AiChatService);
  
  @ViewChild('chatContainer') private chatContainer!: ElementRef;

  isOpen = signal(false);
  messages = signal<ChatMessage[]>([]);
  isLoading = signal(false);
  inputMessage = '';
  
  suggestions = [
    'Find budget laptops',
    'Best wireless headphones',
    'Gift ideas under $50',
    'Compare smartphones'
  ];

  private chatHistory: ChatHistoryItem[] = [];

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  toggle(): void {
    this.isOpen.update(v => !v);
    if (this.isOpen() && this.messages().length === 0) {
      this.addMessage('assistant', 'Hi! I\'m your AI shopping assistant. How can I help you today?');
    }
  }

  async sendMessage(): Promise<void> {
    const message = this.inputMessage.trim();
    if (!message || this.isLoading()) return;

    this.addMessage('user', message);
    this.inputMessage = '';
    this.isLoading.set(true);

    this.chatHistory.push({ role: 'user', content: message });

    try {
      const response = await this.aiChatService.sendMessage({
        message,
        history: this.chatHistory
      }).toPromise();

      const reply = response?.data?.reply ?? 'Sorry, I couldn\'t process that request. Please try again.';
      this.addMessage('assistant', reply);
      this.chatHistory.push({ role: 'assistant', content: reply });
    } catch {
      this.addMessage('assistant', 'I\'m having trouble connecting right now. Please try again in a moment.');
    } finally {
      this.isLoading.set(false);
    }
  }

  onSuggestion(suggestion: string): void {
    this.inputMessage = suggestion;
    this.sendMessage();
  }

  private addMessage(role: 'user' | 'assistant', content: string): void {
    this.messages.update(msgs => [...msgs, { role, content, timestamp: new Date() }]);
    setTimeout(() => this.scrollToBottom(), 50);
  }

  private scrollToBottom(): void {
    if (this.chatContainer) {
      this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
    }
  }
}
