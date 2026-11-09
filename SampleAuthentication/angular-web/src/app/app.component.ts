import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';

import { AuthStateService } from './service/auth-state.service';
import { AuthService } from './service/auth.service';
import { LoaderComponent } from './component/shared/loader/loader.component';
import { ToastNotificationComponent } from './component/shared/toast-notification/toast-notification.component';
import { LoaderService } from './service/shared/loader.service';

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
  isLoggingOut = signal(false);

  constructor(
    private router: Router,
    private authState: AuthStateService,
    private authService: AuthService,
    private loaderService: LoaderService,
  ) {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.isDashboard = event.urlAfterRedirects === '/dashboard';
      });
  }

  onLogout(): void {
    if (this.isLoggingOut()) {
      return;
    }

    this.isLoggingOut.set(true);
    this.loaderService.show();

    setTimeout(() => {
      this.authService.logout().subscribe({
        next: () => {
          this.authState.clearAuthentication();

          this.router.navigate(['/login']).then(() => {
            this.loaderService.hide();
            this.isLoggingOut.set(false);
          });
        },
        error: () => {
          this.loaderService.hide();
          this.isLoggingOut.set(false);
        },
      });
    }, 400);
  }
}
