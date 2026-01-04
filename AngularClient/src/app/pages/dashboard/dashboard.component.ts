import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { StatsService } from '../../core/services/stats.service';
import { MatchService } from '../../core/services/match.service';
import { ProfileStateService } from '../../core/services/profile-state.service';
import { StatCardComponent } from '../../components/stat-card/stat-card.component';
import { Overview, ChampionStats, Match, UserProfile } from '../../models';

// Główna strona Dashboard
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, StatCardComponent],
  template: `
    <div class="dashboard">
      <!-- Nagłówek z filtrami -->
      <header class="dashboard-header">
        <div>
          <h1>Dashboard</h1>
          <p class="welcome">Statystyki dla: {{ currentProfile?.name || 'Brak profilu' }}</p>
        </div>
        
        <!-- Filtry Role i GameMode -->
        <div class="filters">
          <select [(ngModel)]="selectedRole" (ngModelChange)="onRoleChange()" class="filter-select">
            <option value="All">Wszystkie role</option>
            <option value="Top">Top</option>
            <option value="Jungle">Jungle</option>
            <option value="Mid">Mid</option>
            <option value="ADC">ADC</option>
            <option value="Support">Support</option>
          </select>
          
          <select [(ngModel)]="selectedGameMode" (ngModelChange)="onGameModeChange()" class="filter-select">
            <option value="All">Wszystkie tryby</option>
            <option value="Ranked Solo">Ranked Solo</option>
            <option value="Ranked Flex">Ranked Flex</option>
            <option value="Normal">Normal</option>
            <option value="ARAM">ARAM</option>
          </select>
        </div>
      </header>

      <!-- Karty statystyk -->
      @if (isLoading) {
        <div class="loading">Ładowanie statystyk...</div>
      } @else {
        <section class="stats-grid">
          <app-stat-card
            label="Winrate"
            [value]="winrateDisplay"
            [valueColor]="winrateColor"
            icon="🏆"
            iconBg="rgba(0, 217, 255, 0.2)">
          </app-stat-card>
          
          <app-stat-card
            label="Najczęstszy Champion"
            [value]="overview?.mostPlayedChampion || '-'"
            [subtitle]="overview ? overview.mostPlayedChampionGames + ' gier' : ''"
            icon="⚔️"
            iconBg="rgba(255, 193, 7, 0.2)">
          </app-stat-card>
          
          <app-stat-card
            label="Ulubiony Support"
            [value]="overview?.favoriteSupport || '-'"
            [subtitle]="overview ? overview.favoriteSupportGames + ' gier' : ''"
            icon="🛡️"
            iconBg="rgba(76, 175, 80, 0.2)">
          </app-stat-card>
          
          <app-stat-card
            label="Ostatnie mecze"
            [value]="recentMatches.length.toString()"
            subtitle="W historii"
            icon="📊"
            iconBg="rgba(156, 39, 176, 0.2)">
          </app-stat-card>
        </section>

        <!-- Tabela najczęstszych championów -->
        <section class="champions-section">
          <div class="section-header">
            <h2>Twoi Championowie</h2>
            <a routerLink="/history" class="view-all">Zobacz wszystkie mecze →</a>
          </div>
          
          <div class="champions-table">
            <div class="table-header">
              <span>Champion</span>
              <span>Gry</span>
              <span>Winrate</span>
              <span>KDA</span>
            </div>
            @for (champ of filteredChampions; track champ.championName) {
              <div class="table-row">
                <span class="champion-name">{{ champ.championName }}</span>
                <span>{{ champ.games }}</span>
                <span [class.positive]="champ.winrate >= 50" 
                      [class.negative]="champ.winrate < 50">
                  {{ champ.winrate.toFixed(1) }}%
                </span>
                <span>{{ champ.avgKda.toFixed(2) }}</span>
              </div>
            } @empty {
              <div class="no-data">Brak danych o championach</div>
            }
          </div>
        </section>
      }
    </div>
  `,
  styles: [`
    .dashboard {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      flex-wrap: wrap;
      gap: 16px;
    }
    
    h1 {
      color: #fff;
      font-size: 32px;
      margin: 0;
    }
    
    .welcome {
      color: #888;
      margin: 4px 0 0;
    }
    
    .filters {
      display: flex;
      gap: 12px;
    }
    
    .filter-select {
      padding: 10px 16px;
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 8px;
      color: #fff;
      font-size: 14px;
      cursor: pointer;
      min-width: 150px;
    }
    
    .filter-select:focus {
      outline: none;
      border-color: #00d9ff;
    }
    
    .filter-select option {
      background: #1a1a2e;
      color: #fff;
    }
    
    .loading {
      text-align: center;
      padding: 48px;
      color: #888;
    }
    
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 20px;
      margin-bottom: 40px;
    }
    
    .champions-section {
      background: rgba(255, 255, 255, 0.03);
      border-radius: 16px;
      padding: 24px;
      border: 1px solid rgba(255, 255, 255, 0.1);
    }
    
    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }
    
    h2 {
      color: #fff;
      font-size: 20px;
      margin: 0;
    }
    
    .view-all {
      color: #00d9ff;
      text-decoration: none;
      font-size: 14px;
    }
    
    .view-all:hover {
      text-decoration: underline;
    }
    
    .champions-table {
      width: 100%;
    }
    
    .table-header, .table-row {
      display: grid;
      grid-template-columns: 2fr 1fr 1fr 1fr;
      padding: 12px 16px;
      border-radius: 8px;
    }
    
    .table-header {
      background: rgba(255, 255, 255, 0.05);
      color: #888;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    
    .table-row {
      color: #fff;
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
    }
    
    .table-row:hover {
      background: rgba(255, 255, 255, 0.03);
    }
    
    .champion-name {
      font-weight: 600;
    }
    
    .positive { color: #4caf50; }
    .negative { color: #ff5252; }
    
    .no-data {
      text-align: center;
      padding: 24px;
      color: #666;
    }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  currentProfile: UserProfile | null = null;
  isLoading = true;
  overview: Overview | null = null;
  allChampions: ChampionStats[] = [];
  recentMatches: Match[] = [];

  // Filtry
  selectedRole = 'All';
  selectedGameMode = 'Ranked Solo';

  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthService,
    private statsService: StatsService,
    private matchService: MatchService,
    private profileState: ProfileStateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Subskrybuj zmiany profilu dla aktualizacji UI
    this.subscriptions.push(
      this.profileState.currentProfile$.subscribe(profile => {
        this.currentProfile = profile;
      })
    );

    // Inicjalizuj profile i załaduj dane po zakończeniu
    this.profileState.initialize().subscribe({
      next: () => {
        // Profile zostały załadowane, teraz załaduj dane
        this.loadData();
      },
      error: (err) => {
        console.error('Błąd ładowania profili:', err);
        this.isLoading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadData(): void {
    this.isLoading = true;

    // Pobierz overview
    this.statsService.getOverview().subscribe({
      next: (data) => {
        this.overview = data;
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Błąd ładowania overview:', err)
    });

    // Pobierz statystyki championów
    this.statsService.getChampionStats().subscribe({
      next: (data) => {
        this.allChampions = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Błąd ładowania championów:', err);
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });

    // Pobierz ostatnie mecze
    this.matchService.getRecent(10).subscribe({
      next: (data) => {
        this.recentMatches = data;
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Błąd ładowania meczy:', err)
    });
  }

  onRoleChange(): void {
    // W przyszłości można tutaj dodać filtrowanie po stronie API
    // Na razie filtrujemy lokalnie
  }

  onGameModeChange(): void {
    // Przeładuj dane z nowym filtrem
    this.loadData();
  }

  // Filtrowane championiony na podstawie wybranej roli
  get filteredChampions(): ChampionStats[] {
    let filtered = this.allChampions;

    if (this.selectedRole !== 'All') {
      filtered = filtered.filter(c => c.role === this.selectedRole);
    }

    return filtered.slice(0, 5);
  }

  // Computed properties
  get winrateDisplay(): string {
    return this.overview ? `${this.overview.winrate.toFixed(1)}%` : '-';
  }

  get winrateColor(): string {
    if (!this.overview) return '#fff';
    return this.overview.winrate >= 50 ? '#4caf50' : '#ff5252';
  }
}

