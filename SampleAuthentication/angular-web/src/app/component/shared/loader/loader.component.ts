import { Component, inject } from '@angular/core';
import { LoaderService } from 'src/app/service/shared/loader.service';

@Component({
  selector: 'app-loader',
  standalone: true,
  templateUrl: './loader.component.html',
  styleUrl: './loader.component.css'
})
export class LoaderComponent {
  readonly loaderService = inject(LoaderService);
}
