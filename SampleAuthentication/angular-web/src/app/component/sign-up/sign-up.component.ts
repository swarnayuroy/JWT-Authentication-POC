import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { signal } from '@angular/core';
import { UserRegistration} from '../../model/user-registration';

@Component({
  selector: 'app-sign-up',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.css'
})
export class SignUpComponent {
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

    const userRegistration: UserRegistration = {
      name: form.value.name,
      email: form.value.email,
      password: form.value.password
    };    
    // Registration API will be implemented separately.
    console.log(userRegistration);
  }
}
