import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { signal } from '@angular/core';
import { UserCredential } from '../../model/user-credential';
import { LoaderService } from '../../service/shared/loader.service';
import { ToastNotificationService } from '../../service/shared/toast-notification.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly loaderService = inject(LoaderService);
  private readonly toastService = inject(ToastNotificationService);

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
    setTimeout(() => {
    this.loaderService.hide();
    this.toastService.showWarning(
      'Invalid email or password.'
    );

    console.log(userCredential);
  }, 1500);
  }
}
