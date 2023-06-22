import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ScrollToBottomDirective } from 'src/app/_directives/scroll-to-bottom.directive';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild(ScrollToBottomDirective)
  scroll?: ScrollToBottomDirective;
  @Input() username?: string;
  @Input() gender?: string;
  messageContent = '';

  constructor(public messageService: MessageService,
    private _el: ElementRef) { }

  ngOnInit(): void {}

  sendMessage() {
    if (!this.username) return;
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      this.messageForm?.reset();
    })
  }
}