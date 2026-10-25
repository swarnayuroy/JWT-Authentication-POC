import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { signal } from '@angular/core';
import { UserRegistration } from '../../model/user-registration';
import { LoaderService } from '../../service/shared/loader.service';
import { ToastNotificationService } from '../../service/shared/toast-notification.service';

@Component({
  selector: 'app-sign-up',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.css'
})
export class SignUpComponent {
  private readonly loaderService = inject(LoaderService);
  private readonly toastService = inject(ToastNotificationService);

  submitted: boolean = false;
  showPassword = signal(false);

  togglePassword(): void {
    this.showPassword.update(value => !value);
  }

  onSignUp(form: NgForm): void {
    this.submitted = true;

    if (form.invalid) {
      return;
    }

    const userRegistration: UserRegistration = {
      name: form.value.name,
      email: form.value.email,
      password: form.value.password
    };

    this.loaderService.show();
    // Registration API will be implemented separately.
    setTimeout(() => {
      this.loaderService.hide();
      this.toastService.showSuccess(
        'Registration successful.'
      );

      console.log(userRegistration);
    }, 1500);
  }
}
