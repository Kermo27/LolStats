import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ProfileStateService } from '../../core/services/profile-state.service';
import { UserProfile } from '../../models';

/**
 * Komponent wyboru profilu - odpowiednik ProfileSelector.razor
 * Wyświetla dropdown z listą profili użytkownika
 */
@Component({
  selector: 'app-profile-selector',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="profile-selector">
      <button class="profile-button" (click)="toggleDropdown()">
        <span class="profile-icon">👤</span>
        @if (currentProfile) {
          <span class="profile-name">{{ currentProfile.name }}</span>
          <span class="profile-tag">#{{ currentProfile.tag }}</span>
        } @else {
          <span class="profile-name">Select Profile</span>
        }
        <span class="dropdown-arrow">▼</span>
      </button>
      
      @if (isOpen) {
        <div class="dropdown-menu">
          <div class="dropdown-header">Switch account</div>
          
          @for (profile of allProfiles; track profile.id) {
            <div 
              class="dropdown-item" 
              [class.active]="profile.id === currentProfile?.id"
              (click)="selectProfile(profile)">
              <span class="item-name">{{ profile.name }}</span>
              <span class="item-tag">#{{ profile.tag }}</span>
              @if (profile.id === currentProfile?.id) {
                <span class="check-icon">✓</span>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .profile-selector {
      position: relative;
    }
    
    .profile-button {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 8px;
      color: #fff;
      cursor: pointer;
      transition: all 0.2s;
    }
    
    .profile-button:hover {
      background: rgba(255, 255, 255, 0.15);
      border-color: #00d9ff;
    }
    
    .profile-icon {
      font-size: 16px;
    }
    
    .profile-name {
      font-weight: 500;
    }
    
    .profile-tag {
      opacity: 0.7;
      font-size: 12px;
    }
    
    .dropdown-arrow {
      font-size: 10px;
      opacity: 0.7;
      margin-left: 4px;
    }
    
    .dropdown-menu {
      position: absolute;
      top: 100%;
      right: 0;
      margin-top: 8px;
      min-width: 220px;
      background: #1a1a2e;
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
      overflow: hidden;
      z-index: 1000;
    }
    
    .dropdown-header {
      padding: 12px 16px;
      font-size: 12px;
      color: #888;
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    }
    
    .dropdown-item {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      cursor: pointer;
      transition: background 0.2s;
    }
    
    .dropdown-item:hover {
      background: rgba(255, 255, 255, 0.1);
    }
    
    .dropdown-item.active {
      background: rgba(0, 217, 255, 0.1);
    }
    
    .item-name {
      flex: 1;
    }
    
    .item-tag {
      opacity: 0.6;
      font-size: 12px;
    }
    
    .check-icon {
      color: #00d9ff;
    }
  `]
})
export class ProfileSelectorComponent implements OnInit, OnDestroy {
  currentProfile: UserProfile | null = null;
  allProfiles: UserProfile[] = [];
  isOpen = false;

  private subscriptions: Subscription[] = [];

  constructor(
    private profileState: ProfileStateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Subskrybuj zmiany profilu
    this.subscriptions.push(
      this.profileState.currentProfile$.subscribe(profile => {
        this.currentProfile = profile;
        this.cdr.markForCheck();
      }),
      this.profileState.allProfiles$.subscribe(profiles => {
        this.allProfiles = profiles;
        this.cdr.markForCheck();
      })
    );
  }

  ngOnDestroy(): void {
    // Wyczyść subskrypcje przy niszczeniu komponentu
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  selectProfile(profile: UserProfile): void {
    this.profileState.setCurrentProfile(profile);
    this.isOpen = false;

    // Przeładuj stronę żeby zaktualizować dane
    // W przyszłości można to zrobić reaktywnie bez przeładowania
    window.location.reload();
  }
}

