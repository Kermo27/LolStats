import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ProfileSelectorComponent } from '../profile-selector/profile-selector.component';
import { SeasonSelectorComponent } from '../season-selector/season-selector.component';
import { AuthService } from '../../core/services/auth.service';
import { ProfileStateService } from '../../core/services/profile-state.service';
import { SeasonStateService } from '../../core/services/season-state.service';

/**
 * Navbar - odpowiednik MainLayout.razor z Blazora
 * Zawiera logo, linki, SeasonSelector, ProfileSelector i Logout
 */
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, ProfileSelectorComponent, SeasonSelectorComponent],
  template: `
    <nav class="navbar">
      <div class="navbar-left">
        <a routerLink="/" class="logo">
          <span class="logo-icon">📊</span>
          <span class="logo-text">LoL Stats Tracker</span>
        </a>
        
        <div class="nav-links">
          <a routerLink="/" class="nav-link">Dashboard</a>
          <a routerLink="/matches" class="nav-link">Matches</a>
        </div>
      </div>
      
      <div class="navbar-right">
        <app-season-selector />
        
        <div class="divider"></div>
        
        <app-profile-selector />
        
        <button class="logout-btn" (click)="logout()">
          <span>Logout</span>
        </button>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 24px;
      height: 64px;
      background: #111827;
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    }
    
    .navbar-left {
      display: flex;
      align-items: center;
      gap: 32px;
    }
    
    .logo {
      display: flex;
      align-items: center;
      gap: 10px;
      text-decoration: none;
      color: #fff;
    }
    
    .logo-icon {
      font-size: 24px;
    }
    
    .logo-text {
      font-size: 18px;
      font-weight: 600;
      color: #00d9ff;
    }
    
    .nav-links {
      display: flex;
      gap: 8px;
    }
    
    .nav-link {
      padding: 8px 16px;
      color: #888;
      text-decoration: none;
      border-radius: 6px;
      transition: all 0.2s;
    }
    
    .nav-link:hover {
      color: #fff;
      background: rgba(255, 255, 255, 0.1);
    }
    
    .navbar-right {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    
    .divider {
      width: 1px;
      height: 24px;
      background: rgba(255, 255, 255, 0.2);
    }
    
    .logout-btn {
      padding: 8px 16px;
      background: transparent;
      border: 1px solid rgba(255, 82, 82, 0.5);
      border-radius: 6px;
      color: #ff5252;
      cursor: pointer;
      transition: all 0.2s;
    }
    
    .logout-btn:hover {
      background: rgba(255, 82, 82, 0.1);
      border-color: #ff5252;
    }
  `]
})
export class NavbarComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private profileState: ProfileStateService,
    private seasonState: SeasonStateService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Inicjalizuj profile i sezony przy starcie
    this.profileState.initialize().subscribe();
    this.seasonState.initialize().subscribe();
  }

  logout(): void {
    this.authService.logout();
    localStorage.removeItem('profile_id');
    localStorage.removeItem('season_id');
    this.router.navigate(['/login']);
  }
}

