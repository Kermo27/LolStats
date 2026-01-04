import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './components/navbar/navbar.component';
import { AuthService } from './core/services/auth.service';

// Główny komponent aplikacji
// RouterOutlet wyświetla aktualną stronę na podstawie URL
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NavbarComponent],
  template: `
    <div class="app-container">
      <!-- Navbar widoczny tylko dla zalogowanych -->
      @if (authService.isLoggedIn()) {
        <app-navbar />
      }
      
      <!-- Tutaj renderowane są wszystkie strony -->
      <main [class.with-navbar]="authService.isLoggedIn()">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
    }
    
    main.with-navbar {
      padding-top: 0;
    }
  `]
})
export class App {
  title = 'LoL Stats Tracker';

  constructor(public authService: AuthService) { }
}

