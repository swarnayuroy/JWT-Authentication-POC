import { CommonModule } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { signal } from '@angular/core';
import { finalize } from 'rxjs';

import { AuthService } from '../../service/auth.service';
import { LoaderService } from '../../service/shared/loader.service';
import { ToastNotificationService } from '../../service/shared/toast-notification.service';
import { UserCredential } from '../../model/user-credential';
import { ApiDataResponse } from 'src/app/model/api-response';
import { ErrorDetailResponse } from 'src/app/model/api-response';
import { SystemMessage } from 'src/app/utils/system-message';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly loaderService = inject(LoaderService);
  private readonly toastService = inject(ToastNotificationService);

  private readonly authService = inject(AuthService);

  submitted: boolean = false;
  showPassword = signal(false);

  togglePassword(): void {
    this.showPassword.update(value => !value);
  }

  onLogin(form: NgForm): void {
    this.submitted = true;

    if (form.invalid) {
      return;
    }

    const userCredential: UserCredential = {
      email: form.value.email,
      password: form.value.password
    };

    this.loaderService.show();

    // API authentication will be implemented separately.
    this.authService.login(userCredential)
      .pipe(
        finalize(() => this.loaderService.hide())
      )
      .subscribe({
        next: (response) => {
          if (response.status == HttpStatusCode.Ok && response.body?.status && response.body?.data) {
            this.toastService.showSuccess(
              'Login successful.'
            );

            // JWT handling and dashboard navigation
            // will be implemented in the next step.
            console.log('Login successful');
          } else {
            const errorDetail = new ErrorDetailResponse();
            errorDetail.statusCode = response.status;
            errorDetail.message = response.body?.message || SystemMessage.LOGIN_FAILED;

            this.toastService.showWarning(errorDetail);
          }
        },
        error: (error: HttpErrorResponse) => {
          const errorDetail = new ErrorDetailResponse();
          errorDetail.statusCode = error.status as HttpStatusCode;
          errorDetail.message = typeof error.error === 'string' ? error.error : SystemMessage.GENERAL_ERROR;

          this.toastService.showWarning(errorDetail);
        }
      });
  }
}
