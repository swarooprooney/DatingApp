import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
  messages: Message[];
  @Input() recipientId: number;
  newMessage: any = {};
  constructor(
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    this.loadMessage();
  }
  loadMessage() {
    const currentUserId = + this.authService.decodedToken.nameid;
    this.userService
      .getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      .pipe(
        tap(messages => {
        for (let index = 0; index < messages.length; index++) {
          if (messages[index].isRead === false && messages[index].recipientId === currentUserId) {
            this.userService.markMessageRead(messages[index].id, currentUserId);
          }
        }
        })
      )
      .subscribe(
        (messages) => {
          this.messages = messages;
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService
      .sendMessage(this.authService.decodedToken.nameid, this.newMessage)
      .subscribe(
        (message: Message) => {
          this.messages.unshift(message);
          this.newMessage.content = '';
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }
}
