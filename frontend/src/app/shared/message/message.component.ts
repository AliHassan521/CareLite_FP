import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessageService, UserMessage } from '../../services/message.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-message',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message.component.html',
  styleUrls: ['./message.component.scss']
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
