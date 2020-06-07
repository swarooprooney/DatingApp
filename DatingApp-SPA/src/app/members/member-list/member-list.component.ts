import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { PaginatedResult, Pagination } from 'src/app/_models/Pagination';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css'],
})
export class MemberListComponent implements OnInit {
  pagination: Pagination;
  user: User = JSON.parse(localStorage.getItem('user'));
  users: User[];
  userParams: any = {};
  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'Females' },
  ];

  constructor(
    private userService: UserService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.route.data.subscribe((data) => {
      this.users = data.users.result;
      this.pagination = data.users.pagination;
      console.log(this.pagination);
    });
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.gender = this.user.gender === 'male' ? 'female' : 'male';
    this.userParams.orderBy = 'lastActive';
  }

  pageChanged(event) {
    this.pagination.pageNumber = event.page;
    this.loadUsers();
  }
  resetFilters() {
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.gender = this.user.gender === 'male' ? 'female' : 'male';
    this.userParams.orderBy = 'lastActive';
    this.loadUsers();
  }
  loadUsers() {
    this.userService
      .getUsers(
        this.pagination.pageNumber,
        this.pagination.itemsPerPage,
        this.userParams
      )
      .subscribe(
        (res: PaginatedResult<User[]>) => {
          this.users = res.result;
          this.pagination = res.pagination;
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }
}
