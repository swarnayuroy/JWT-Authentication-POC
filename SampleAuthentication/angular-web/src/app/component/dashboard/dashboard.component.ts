import { Component, OnInit } from '@angular/core';

import { UserDetail } from 'src/app/model/user-detail.model';
import { AuthStateService } from 'src/app/service/auth-state.service';
import { UserService } from 'src/app/service/user.service';

@Component({
  selector: 'app-dashboard',
  imports: [],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  user: UserDetail | null = null;

  constructor(
    private authState: AuthStateService,
    private userService: UserService,
  ) {}

  ngOnInit(): void {
    const userId = this.authState.userId();

    if (!userId) {
      return;
    }

    this.userService.getById(userId).subscribe({
      next: (response) => {
        if (response.body?.status && response.body.data) {
          this.user = response.body.data;
        }
      },
      error: (error) => {
        console.error('Failed to load user details.', error);
      },
    });
  }

  get firstName(): string {
    return this.user?.name?.trim().split(/\s+/)[0] ?? '';
  }
}
