import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { Routes } from '@angular/router';

const routes: Routes = [
  { path: 'login', loadComponent: () =>
      import('./component/login/login.component').then(m => m.LoginComponent) },
  { path: 'sign-up', loadComponent: () =>
      import('./component/sign-up/sign-up.component').then(m => m.SignUpComponent) },
      
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' }
];

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimations()
  ]
};
