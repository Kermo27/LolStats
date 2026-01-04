import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ProfileStateService } from '../../core/services/profile-state.service';
import { LoginRequest } from '../../models';

// Komponent strony logowania
// Component = odpowiednik Razor Component w Blazor
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule], // Moduły potrzebne do formularzy
  template: `
    <div class="login-container">
      <div class="login-card">
        <h1>LoL Stats Tracker</h1>
        <p class="subtitle">Zaloguj się, aby zobaczyć swoje statystyki</p>
        
        <!-- Formularz logowania -->
        <form (ngSubmit)="login()" class="login-form">
          <div class="form-group">
            <label for="username">Nazwa użytkownika</label>
            <input 
              type="text" 
              id="username" 
              [(ngModel)]="credentials.username"
              name="username"
              placeholder="Wpisz nazwę użytkownika"
              required>
          </div>
          
          <div class="form-group">
            <label for="password">Hasło</label>
            <input 
              type="password" 
              id="password" 
              [(ngModel)]="credentials.password"
              name="password"
              placeholder="Wpisz hasło"
              required>
          </div>
          
          <!-- Komunikat błędu -->
          @if (errorMessage) {
            <div class="error-message">{{ errorMessage }}</div>
          }
          
          <button type="submit" [disabled]="isLoading" class="btn-login">
            @if (isLoading) {
              <span>Logowanie...</span>
            } @else {
              <span>Zaloguj się</span>
            }
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
    }
    
    .login-card {
      background: rgba(255, 255, 255, 0.05);
      backdrop-filter: blur(10px);
      border-radius: 16px;
      padding: 48px;
      width: 100%;
      max-width: 400px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
      border: 1px solid rgba(255, 255, 255, 0.1);
    }
    
    h1 {
      color: #00d9ff;
      font-size: 28px;
      margin-bottom: 8px;
      text-align: center;
    }
    
    .subtitle {
      color: #888;
      text-align: center;
      margin-bottom: 32px;
    }
    
    .form-group {
      margin-bottom: 20px;
    }
    
    label {
      display: block;
      color: #ccc;
      margin-bottom: 8px;
      font-size: 14px;
    }
    
    input {
      width: 100%;
      padding: 12px 16px;
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 8px;
      color: #fff;
      font-size: 16px;
      transition: border-color 0.2s;
      box-sizing: border-box;
    }
    
    input:focus {
      outline: none;
      border-color: #00d9ff;
    }
    
    input::placeholder {
      color: #666;
    }
    
    .btn-login {
      width: 100%;
      padding: 14px;
      background: linear-gradient(135deg, #00d9ff 0%, #0099ff 100%);
      border: none;
      border-radius: 8px;
      color: #fff;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
    }
    
    .btn-login:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 4px 20px rgba(0, 217, 255, 0.4);
    }
    
    .btn-login:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }
    
    .error-message {
      background: rgba(255, 82, 82, 0.2);
      border: 1px solid #ff5252;
      color: #ff5252;
      padding: 12px;
      border-radius: 8px;
      margin-bottom: 16px;
      text-align: center;
    }
  `]
})
export class LoginComponent {
  // Dane formularza - odpowiednik @bind w Blazor
  credentials: LoginRequest = {
    username: '',
    password: ''
  };

  isLoading = false;
  errorMessage = '';

  // Dependency Injection - podobnie jak w C#
  constructor(
    private authService: AuthService,
    private router: Router,
    private profileState: ProfileStateService
  ) { }

  // Obsługa logowania
  login(): void {
    if (!this.credentials.username || !this.credentials.password) {
      this.errorMessage = 'Wypełnij wszystkie pola';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // subscribe() to odpowiednik async/await w Observable
    this.authService.login(this.credentials).subscribe({
      next: () => {
        // Po zalogowaniu pobierz profile i ustaw pierwszy jako aktywny
        this.profileState.initialize().subscribe({
          next: () => this.router.navigate(['/']),
          error: () => this.router.navigate(['/'])
        });
      },
      error: (err) => {
        // Błąd - pokaż komunikat
        this.errorMessage = err.error || 'Błąd logowania. Sprawdź dane.';
        this.isLoading = false;
      }
    });
  }
}
