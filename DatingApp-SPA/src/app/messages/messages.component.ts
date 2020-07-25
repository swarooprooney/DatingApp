import { Component, OnInit } from '@angular/core';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Message } from '../_models/message';
import { Pagination, PaginatedResult } from '../_models/Pagination';
import { AuthService } from '../_services/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
messages: Message[];
pagination: Pagination;
messageContainer = 'Unread';

  constructor(private userService: UserService,
              private alertify: AlertifyService,
              private authService: AuthService,
              private route: ActivatedRoute
    ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data.messages.result;
      this.pagination = data.messages.pagination;
    });
  }

  loadMessages() {
    this.userService.getMessages(this.authService.decodedToken.nameid, this.pagination.pageNumber,
      this.pagination.itemsPerPage, this.messageContainer)
      .subscribe((res: PaginatedResult<Message[]>) => {
      this.messages = res.result;
      this.pagination = res.pagination;
      }, error => {
        this.alertify.error(error);
      });
  }

  deleteMessage(id: number) {
    this.alertify.confirm('Are you sure you want to delete the message?', () => {
      this.userService.deleteMessage(id, this.authService.decodedToken.nameid).subscribe(() => {
      this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
      this.alertify.success('Message Deleted Successfully');
      }, error => {
          this.alertify.error(error);
        });
    });
  }

  pageChanged(event: any): void {
    this.pagination.pageNumber = event.page;
    this.loadMessages();
  }
}
