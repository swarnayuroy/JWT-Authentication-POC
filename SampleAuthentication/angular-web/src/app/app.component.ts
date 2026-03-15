import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountComponent } from './account/account.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, AccountComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'angular-web';
}
