import { Injectable, signal } from '@angular/core';
import { HttpStatusCode } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})

export class ToastNotification {
  static readonly SUCCESS_ICON: string = 'fa-solid fa-circle-check';
  static readonly WARNING_ICON: string = 'fa fa-triangle-exclamation';
  readonly timeOut: number = 8000;
  readonly closeToastIcon: string = 'fa-solid fa-circle-xmark';

  isEnable: boolean = false;
  type: HttpStatusCode = HttpStatusCode.Ok;
  message: string = ''; 

  statusIcon:
    | typeof ToastNotification.SUCCESS_ICON
    | typeof ToastNotification.WARNING_ICON =
      ToastNotification.SUCCESS_ICON;
}

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

  showSuccess(message: string): void {
    const toast = new ToastNotification();

    toast.type = HttpStatusCode.Ok;
    toast.statusIcon = ToastNotification.SUCCESS_ICON;
    toast.message = message;

    this.show(toast);
  }

  showWarning(message: string): void {
    const toast = new ToastNotification();

    toast.type = HttpStatusCode.BadRequest;
    toast.statusIcon = ToastNotification.WARNING_ICON;
    toast.message = message;

    this.show(toast);
  }

  hide(): void {
    this.clearTimeout();
    this.notification.set(null);
  }  
}
