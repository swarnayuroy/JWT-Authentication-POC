import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { signal } from '@angular/core';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email: string = "";
  password: string = "";
  
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

    // API authentication will be implemented separately.
  }
}
