import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface UserMessage {
  type: 'success' | 'error' | 'info' | 'warning';
  text: string;
}

@Injectable({ providedIn: 'root' })
export class MessageService {
  private messageSubject = new BehaviorSubject<UserMessage | null>(null);
  message$ = this.messageSubject.asObservable();
  private timeoutId: any;

  showMessage(message: UserMessage, duration: number = 4000) {
    this.messageSubject.next(message);
    if (this.timeoutId) clearTimeout(this.timeoutId);
    this.timeoutId = setTimeout(() => {
      this.clear();
    }, duration);
  }

  clear() {
    this.messageSubject.next(null);
  }
}
