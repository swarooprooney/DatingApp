import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private alertifyService: AlertifyService,
              private routerService: Router) {}
  canActivate(): boolean {
    if (this.authService.loggedIn()) {
    return true;
  }
    this.alertifyService.error('Please login to access this page');
    this.routerService.navigate(['home']);
  }

}
