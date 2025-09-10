import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessageService, UserMessage } from '../services/message.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-message',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (message) {
      <div [ngClass]="'msg-' + message.type" class="user-message">
        {{ message.text }}
      </div>
    }
  `,
  styles: [`
    .user-message {
      position: fixed;
      top: 1.5rem;
      right: 1.5rem;
      z-index: 9999;
      padding: 1rem 2rem;
      border-radius: 4px;
      font-weight: 500;
      box-shadow: 0 2px 8px rgba(0,0,0,0.15);
      min-width: 200px;
      text-align: center;
      opacity: 0.95;
      transition: opacity 0.3s;
    }
    .msg-success { background: #e6ffed; color: #237804; border: 1px solid #b7eb8f; }
    .msg-error { background: #fff1f0; color: #a8071a; border: 1px solid #ffa39e; }
    .msg-info { background: #e6f7ff; color: #0050b3; border: 1px solid #91d5ff; }
    .msg-warning { background: #fffbe6; color: #ad6800; border: 1px solid #ffe58f; }
  `]
})
export class MessageComponent implements OnDestroy {
  message: UserMessage | null = null;
  private sub: Subscription;

  constructor(private messageService: MessageService) {
    this.sub = this.messageService.message$.subscribe(msg => {
      this.message = msg;
    });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}
