import { Component, inject } from '@angular/core';
import { ToastNotificationService } from 'src/app/service/shared/toast-notification.service';

@Component({
  selector: 'app-toast-notification',
  standalone: true,
  templateUrl: './toast-notification.component.html',
  styleUrl: './toast-notification.component.css'
})
export class ToastNotificationComponent {
  readonly toastService = inject(ToastNotificationService);
}
