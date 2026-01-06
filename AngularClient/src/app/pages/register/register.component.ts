import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink],
    template: `
    <div class="register-page">
      <div class="register-card glass-card">
        <div class="card-header">
          <div class="icon-badge">👤</div>
          <h1>Create Account</h1>
          <p class="subtitle">Join LolStats Tracker</p>
        </div>

        @if (errorMessage) {
          <div class="alert alert-error">
            <span class="alert-icon">⚠️</span>
            {{ errorMessage }}
          </div>
        }

        <form (ngSubmit)="handleRegister()" class="register-form">
          <div class="form-group">
            <label for="username">Username</label>
            <div class="input-wrapper">
              <span class="input-icon">👤</span>
              <input
                type="text"
                id="username"
                [(ngModel)]="username"
                name="username"
                placeholder="Choose a username"
                [disabled]="isLoading"
                required
              />
            </div>
            <span class="helper-text">Choose a unique username</span>
          </div>

          <div class="form-group">
            <label for="email">Email (optional)</label>
            <div class="input-wrapper">
              <span class="input-icon">📧</span>
              <input
                type="email"
                id="email"
                [(ngModel)]="email"
                name="email"
                placeholder="your@email.com"
                [disabled]="isLoading"
              />
            </div>
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <div class="input-wrapper">
              <span class="input-icon">🔒</span>
              <input
                [type]="showPassword ? 'text' : 'password'"
                id="password"
                [(ngModel)]="password"
                name="password"
                placeholder="Enter password"
                [disabled]="isLoading"
                required
              />
              <button 
                type="button" 
                class="toggle-password"
                (click)="showPassword = !showPassword"
              >
                {{ showPassword ? '🙈' : '👁️' }}
              </button>
            </div>
            <span class="helper-text">Minimum 6 characters</span>
          </div>

          <div class="form-group">
            <label for="confirmPassword">Confirm Password</label>
            <div class="input-wrapper">
              <span class="input-icon">🔒</span>
              <input
                [type]="showPassword ? 'text' : 'password'"
                id="confirmPassword"
                [(ngModel)]="confirmPassword"
                name="confirmPassword"
                placeholder="Confirm password"
                [disabled]="isLoading"
                required
              />
            </div>
          </div>

          <button 
            type="submit" 
            class="btn-primary"
            [disabled]="isLoading"
          >
            @if (isLoading) {
              <span class="spinner"></span>
              Creating account...
            } @else {
              Create Account
            }
          </button>
        </form>

        <div class="divider">
          <span>or</span>
        </div>

        <p class="login-link">
          Already have an account?
          <a routerLink="/login">Login here</a>
        </p>
      </div>
    </div>
  `,
    styles: [`
    .register-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
    }

    .register-card {
      width: 100%;
      max-width: 440px;
      padding: 40px;
      border-radius: 24px;
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.08);
      backdrop-filter: blur(20px);
    }

    .card-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .icon-badge {
      width: 64px;
      height: 64px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 32px;
      background: linear-gradient(135deg, rgba(0, 217, 255, 0.2), rgba(139, 92, 246, 0.2));
      border-radius: 20px;
      margin: 0 auto 16px;
      border: 1px solid rgba(255, 255, 255, 0.1);
    }

    h1 {
      color: #fff;
      font-size: 28px;
      margin: 0 0 8px;
      font-weight: 700;
    }

    .subtitle {
      color: #888;
      margin: 0;
      font-size: 14px;
    }

    .alert {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 14px 16px;
      border-radius: 12px;
      margin-bottom: 24px;
      font-size: 14px;
    }

    .alert-error {
      background: rgba(239, 68, 68, 0.15);
      border: 1px solid rgba(239, 68, 68, 0.3);
      color: #f87171;
    }

    .register-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    label {
      color: #fff;
      font-size: 14px;
      font-weight: 500;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-icon {
      position: absolute;
      left: 14px;
      font-size: 16px;
      opacity: 0.5;
    }

    input {
      width: 100%;
      padding: 14px 16px 14px 44px;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 12px;
      color: #fff;
      font-size: 15px;
      transition: all 0.2s ease;
    }

    input::placeholder {
      color: rgba(255, 255, 255, 0.4);
    }

    input:focus {
      outline: none;
      border-color: #00d9ff;
      box-shadow: 0 0 0 3px rgba(0, 217, 255, 0.15);
    }

    input:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .toggle-password {
      position: absolute;
      right: 12px;
      background: none;
      border: none;
      cursor: pointer;
      font-size: 16px;
      padding: 4px;
      opacity: 0.6;
      transition: opacity 0.2s;
    }

    .toggle-password:hover {
      opacity: 1;
    }

    .helper-text {
      color: #666;
      font-size: 12px;
    }

    .btn-primary {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
      width: 100%;
      padding: 16px;
      background: linear-gradient(135deg, #00d9ff, #8b5cf6);
      border: none;
      border-radius: 12px;
      color: #fff;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s ease;
      margin-top: 8px;
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0, 217, 255, 0.3);
    }

    .btn-primary:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    .spinner {
      width: 18px;
      height: 18px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: #fff;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .divider {
      display: flex;
      align-items: center;
      gap: 16px;
      margin: 28px 0;
    }

    .divider::before,
    .divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background: rgba(255, 255, 255, 0.1);
    }

    .divider span {
      color: #666;
      font-size: 13px;
    }

    .login-link {
      text-align: center;
      color: #888;
      font-size: 14px;
      margin: 0;
    }

    .login-link a {
      color: #00d9ff;
      text-decoration: none;
      font-weight: 500;
    }

    .login-link a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
    username = '';
    email = '';
    password = '';
    confirmPassword = '';
    errorMessage = '';
    isLoading = false;
    showPassword = false;

    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    handleRegister(): void {
        // Validation
        if (!this.username.trim() || !this.password.trim()) {
            this.errorMessage = 'Username and password are required';
            return;
        }

        if (this.password.length < 6) {
            this.errorMessage = 'Password must be at least 6 characters';
            return;
        }

        if (this.password !== this.confirmPassword) {
            this.errorMessage = 'Passwords do not match';
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        this.authService.register({
            username: this.username,
            password: this.password,
            email: this.email || undefined
        }).subscribe({
            next: () => {
                this.router.navigate(['/']);
            },
            error: (err) => {
                this.isLoading = false;
                this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
            }
        });
    }
}
