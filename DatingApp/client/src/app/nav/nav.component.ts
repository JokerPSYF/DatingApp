import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
})
export class NavComponent implements OnInit {
  model: any = {};
  constructor(
    public accountService: AccountService,
    public memberService: MembersService,
    private router: Router,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void { }

  login() {
    this.accountService.login(this.model).subscribe({
      next: (user: User | undefined) => {
        if (user) {
          this.memberService.updateUserAndParams(user);
        }
        this.router.navigateByUrl('/members')
      },
      error: (error) => {
        if (Object.prototype.toString.call(error) === '[object Array]') {
          this.toastr.error(error);
        }
      },
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
