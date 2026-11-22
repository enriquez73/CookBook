import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  imports: [RouterOutlet, RouterLink, ButtonModule],
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {}
