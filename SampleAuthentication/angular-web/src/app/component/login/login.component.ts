import { CommonModule } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { signal } from '@angular/core';
import { finalize } from 'rxjs';

import { AuthStateService } from '../../service/auth-state.service';
import { AuthService } from '../../service/auth.service';
import { LoaderService } from '../../service/shared/loader.service';
import { ToastNotificationService } from '../../service/shared/toast-notification.service';
import { UserCredential } from '../../model/user-credential';
import {
  SuccessDetailResponse,
  ErrorDetailResponse,
} from 'src/app/model/api-response';
import { SystemMessage } from 'src/app/utils/system-message';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly loaderService = inject(LoaderService);
  private readonly toastService = inject(ToastNotificationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly authState = inject(AuthStateService);
  private readonly authService = inject(AuthService);

  submitted: boolean = false;
  showPassword = signal(false);

  ngOnInit(): void {
    const handoff = this.route.snapshot.queryParamMap.get('handOff');

    if (handoff) {
      this.completeHandoff(handoff);
    }
  }

  togglePassword(): void {
    this.showPassword.update((value) => !value);
  }

  private completeHandoff(handoff: string): void {
    this.loaderService.show();

    this.authService
      .completeHandoff(handoff)
      .pipe(finalize(() => this.loaderService.hide()))
      .subscribe({
        next: (response) => {
          if (response.body?.status && response.body?.data) {
            const userId = response.body.data;

            this.authState.setUserId(userId);
            this.authState.setAuthenticated(response.body.message);

            // dashboard navigation
            this.router.navigate(['/dashboard']);
          } else {
            const errorDetail = new ErrorDetailResponse();
            errorDetail.statusCode = response.status;
            errorDetail.message =
              response.body?.message || SystemMessage.LOGIN_FAILED;

            this.toastService.showWarning(errorDetail);
          }
        },

        error: (error: HttpErrorResponse) => {
          const errorDetail = new ErrorDetailResponse();
          errorDetail.statusCode = error.status as HttpStatusCode;
          errorDetail.message =
            typeof error.error === 'string'
              ? error.error
              : SystemMessage.GENERAL_ERROR;

          this.toastService.showWarning(errorDetail);
        },
      });
  }

  onLogin(form: NgForm): void {
    this.submitted = true;

    if (form.invalid) {
      return;
    }

    const userCredential: UserCredential = {
      email: form.value.email,
      password: form.value.password,
      client: 'angular',
    };

    this.loaderService.show();

    // API authentication will be implemented separately.
    this.authService
      .login(userCredential)
      .pipe(finalize(() => this.loaderService.hide()))
      .subscribe({
        next: (response) => {
          if (response.body?.status && response.body?.data) {
            const userId = response.body.data;

            this.authState.setUserId(userId);
            this.authState.setAuthenticated(response.body.message);

            // dashboard navigation
            this.router.navigate(['/dashboard']);
          } else {
            const errorDetail = new ErrorDetailResponse();
            errorDetail.statusCode = response.status;
            errorDetail.message =
              response.body?.message || SystemMessage.LOGIN_FAILED;

            this.toastService.showWarning(errorDetail);
          }
        },
        error: (error: HttpErrorResponse) => {
          const errorDetail = new ErrorDetailResponse();
          errorDetail.statusCode = error.status as HttpStatusCode;
          errorDetail.message =
            typeof error.error === 'string'
              ? error.error
              : SystemMessage.GENERAL_ERROR;

          this.toastService.showWarning(errorDetail);
        },
      });
  }
}
