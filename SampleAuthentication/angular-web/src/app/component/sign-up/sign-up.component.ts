import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { signal } from '@angular/core';

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

  submitted: boolean = false;
  showPassword= signal(false);

  togglePassword(): void {
    this.showPassword.update(value => !value);
  }

  onSignUp(form: NgForm): void {
    this.submitted = true;

    if (form.invalid) {
      return;
    }

    // Registration API will be implemented separately.
  }
}
