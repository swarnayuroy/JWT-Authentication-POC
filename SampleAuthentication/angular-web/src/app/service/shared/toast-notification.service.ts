import { Injectable, signal } from '@angular/core';
import { HttpStatusCode } from '@angular/common/http';
import { ToastNotification } from '../../model/toast-notification';
import { SuccessDetailResponse, ErrorDetailResponse } from 'src/app/model/api-response';

@Injectable({
  providedIn: 'root'
})

export class ToastNotificationService {
  private timeoutId?: ReturnType<typeof setTimeout>;
  notification = signal<ToastNotification | null>(null);
  
  constructor() { }

  private clearTimeout(): void {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
      this.timeoutId = undefined;
    }
  }

  show(toast: ToastNotification): void {
    this.clearTimeout();

    toast.isEnable = true;
    this.notification.set(toast);

    this.timeoutId = setTimeout(() => {
      this.hide();
    }, toast.timeOut);
  }

  showSuccess(successDetail: SuccessDetailResponse): void {
    console.log(successDetail);

    const toast = new ToastNotification();
    toast.type = successDetail.statusCode;
    toast.statusIcon = ToastNotification.SUCCESS_ICON;
    toast.message = successDetail.message;

    this.show(toast);
  }

  showWarning(errorDetail: ErrorDetailResponse): void {
    console.log(errorDetail);

    const toast = new ToastNotification();
    toast.type = errorDetail.statusCode;
    toast.statusIcon = ToastNotification.WARNING_ICON;
    toast.message = errorDetail.message;
    
    this.show(toast);
  }

  hide(): void {
    this.clearTimeout();
    this.notification.set(null);
  }  
}
