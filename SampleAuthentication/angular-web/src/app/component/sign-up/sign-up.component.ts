import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-sign-up',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.css'
})
export class SignUpComponent {
  name: string = "";
  email: string = "";
  password: string = "";
  showPassword: boolean = false;
  submitted: boolean = false;

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSignUp(): void {
    this.submitted = true;

    if (!this.name.trim() || !this.email.trim() || !this.password) {
      return;
    }

    // Registration API will be implemented separately.
  }
}
