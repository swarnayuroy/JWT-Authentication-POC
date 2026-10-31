import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';

import { AuthStateService } from './service/auth-state.service';
import { LoaderComponent } from './component/shared/loader/loader.component';
import { ToastNotificationComponent } from './component/shared/toast-notification/toast-notification.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    LoaderComponent,
    ToastNotificationComponent,
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  headerTitle: string = 'Angular';
  isDashboard: boolean = false;

  constructor(
    private router: Router,
    private authState: AuthStateService,
  ) {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.isDashboard = event.urlAfterRedirects === '/dashboard';
      });
  }

  onLogout(): void {
    this.authState.clearAuthentication();
    this.router.navigate(['/login']);
  }
}
