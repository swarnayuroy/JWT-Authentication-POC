import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { signal } from '@angular/core';
import { finalize } from 'rxjs';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';

import { AuthService } from '../../service/auth.service';
import { UserRegistration } from '../../model/user-registration';
import { LoaderService } from '../../service/shared/loader.service';
import { ToastNotificationService } from '../../service/shared/toast-notification.service';
import {
  SuccessDetailResponse,
  ErrorDetailResponse,
} from 'src/app/model/api-response';
import { SystemMessage } from 'src/app/utils/system-message';

@Component({
  selector: 'app-sign-up',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.css',
})
export class SignUpComponent {
  private readonly loaderService = inject(LoaderService);
  private readonly toastService = inject(ToastNotificationService);
  private readonly router = inject(Router);

  private readonly authService = inject(AuthService);

  submitted: boolean = false;
  showPassword = signal(false);

  togglePassword(): void {
    this.showPassword.update((value) => !value);
  }

  onSignUp(form: NgForm): void {
    this.submitted = true;

    if (form.invalid) {
      return;
    }

    const userRegistration: UserRegistration = {
      name: form.value.name,
      email: form.value.email,
      password: form.value.password,
    };

    this.loaderService.show();

    // Registration API will be implemented separately.
    this.authService
      .register(userRegistration)
      .pipe(finalize(() => this.loaderService.hide()))
      .subscribe({
        next: (response) => {
          if (response.body?.status) {
            // Show the success message returned by the API.
            const successDetail = new SuccessDetailResponse();
            successDetail.statusCode = response.status;
            successDetail.message =
              response.body?.message || SystemMessage.REGISTRATION_SUCCESS;
            this.toastService.showSuccess(successDetail);

            // Navigate to login after successful registration.
            this.router.navigate(['/login']);
          } else {
            const errorDetail = new ErrorDetailResponse();
            errorDetail.statusCode = response.status;
            errorDetail.message =
              response.body?.message || SystemMessage.REGISTRATION_FAILED;

            // Stay on the sign-up page if registration fails.
            this.toastService.showWarning(errorDetail);
          }
        },
        error: (error: HttpErrorResponse) => {
          // Handle HTTP errors, including server-side failures.
          const errorDetail = new ErrorDetailResponse();
          errorDetail.statusCode = error.status as HttpStatusCode;
          errorDetail.message =
            error.error?.message ||
            (typeof error.error === 'string' ? error.error : null) ||
            SystemMessage.GENERAL_ERROR;

          this.toastService.showWarning(errorDetail);
        },
      });
  }
}
