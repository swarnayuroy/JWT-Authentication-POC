import { CommonModule } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
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

  private readonly authState = inject(AuthStateService);
  private readonly authService = inject(AuthService);

  submitted: boolean = false;
  showPassword = signal(false);

  togglePassword(): void {
    this.showPassword.update((value) => !value);
  }

  private decodeJwtPayload(token: string): any {
    const payload = token.split('.')[1];

    if (!payload) {
      throw new Error('Invalid JWT token.');
    }

    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');

    const decoded = atob(base64);
    return JSON.parse(decoded);
  }

  onLogin(form: NgForm): void {
    this.submitted = true;

    if (form.invalid) {
      return;
    }

    const userCredential: UserCredential = {
      email: form.value.email,
      password: form.value.password,
    };

    this.loaderService.show();

    // API authentication will be implemented separately.
    this.authService
      .login(userCredential)
      .pipe(finalize(() => this.loaderService.hide()))
      .subscribe({
        next: (response) => {
          if (response.body?.status && response.body?.data) {
            const token: string = response.body.data.toString();
            const role: string =
              this.decodeJwtPayload(token).role?.toLowerCase();

            if (role !== 'user') {
              const errorDetail = new ErrorDetailResponse();
              errorDetail.statusCode = HttpStatusCode.Unauthorized;
              errorDetail.message = 'Unauthorized user.';
              this.toastService.showWarning(errorDetail);
              return;
            }
            // Store authentication details.
            this.authState.setAuthentication(token, role);

            const successDetail = new SuccessDetailResponse();
            successDetail.statusCode = response.status;
            successDetail.message =
              response.body?.message || SystemMessage.LOGIN_SUCCESS;
            successDetail.data = response.body?.data;

            this.toastService.showSuccess(successDetail);

            // JWT handling and dashboard navigation
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
