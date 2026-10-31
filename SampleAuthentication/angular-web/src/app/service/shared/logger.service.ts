import { Injectable, isDevMode } from '@angular/core';
import { LogLevel } from '../../utils/logger-level';

@Injectable({
  providedIn: 'root'
})

export class LoggerService {

  logDetails(type: LogLevel, message: string): void {

    if (isDevMode()) {

      const timestamp: string = new Date().toISOString();
      const logMessage: string = `[${timestamp}] [${type}] ${message}`;

      switch (type) {
        case LogLevel.INFO:
          console.info(logMessage);
          break;

        case LogLevel.WARN:
          console.warn(logMessage);
          break;

        case LogLevel.ERROR:
          console.error(logMessage);
          break;
      }
    }
  }
}
