import { HttpStatusCode } from '@angular/common/http';

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