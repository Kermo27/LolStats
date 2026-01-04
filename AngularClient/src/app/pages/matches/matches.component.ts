import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { MatchService } from '../../core/services/match.service';
import { ProfileStateService } from '../../core/services/profile-state.service';
import { Match, UserProfile } from '../../models';

/**
 * Matches Page - lista wszystkich meczy z paginacją
 * Odpowiednik Matches.razor z Blazora
 */
@Component({
  selector: 'app-matches',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="matches-page">
      <header class="page-header">
        <div>
          <h1>Historia Meczy</h1>
          <p class="subtitle">Przeglądaj swoją historię jako: {{ currentProfile?.name || '...' }}</p>
        </div>
        
        <div class="filters">
          <select [(ngModel)]="selectedGameMode" (ngModelChange)="loadMatches()" class="filter-select">
            <option value="">Wszystkie tryby</option>
            <option value="Ranked Solo">Ranked Solo</option>
            <option value="Ranked Flex">Ranked Flex</option>
            <option value="Normal">Normal</option>
            <option value="ARAM">ARAM</option>
          </select>
        </div>
      </header>

      @if (isLoading) {
        <div class="loading">
          <div class="spinner"></div>
          <p>Ładowanie meczy...</p>
        </div>
      } @else {
        <div class="matches-table-container">
          <table class="matches-table">
            <thead>
              <tr>
                <th>Champion</th>
                <th>Wynik</th>
                <th>Tryb</th>
                <th>KDA</th>
                <th>CS</th>
                <th>Rola</th>
                <th>Data</th>
              </tr>
            </thead>
            <tbody>
              @for (match of matches; track match.id) {
                <tr [class.win]="match.win" [class.loss]="!match.win">
                  <td class="champion-cell">
                    <span class="champion-name">{{ match.champion }}</span>
                  </td>
                  <td>
                    <span class="result-badge" [class.win]="match.win" [class.loss]="!match.win">
                      {{ match.win ? 'WIN' : 'LOSS' }}
                    </span>
                  </td>
                  <td>{{ match.gameMode }}</td>
                  <td>{{ match.kills }}/{{ match.deaths }}/{{ match.assists }}</td>
                  <td>{{ match.cs }}</td>
                  <td>{{ match.role }}</td>
                  <td>{{ formatDate(match.date) }}</td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="7" class="no-data">Brak meczy do wyświetlenia</td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Paginacja -->
        <div class="pagination">
          <button 
            class="page-btn" 
            [disabled]="currentPage === 1"
            (click)="goToPage(currentPage - 1)">
            ← Poprzednia
          </button>
          <span class="page-info">Strona {{ currentPage }} z {{ totalPages }}</span>
          <button 
            class="page-btn" 
            [disabled]="currentPage >= totalPages"
            (click)="goToPage(currentPage + 1)">
            Następna →
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .matches-page {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      flex-wrap: wrap;
      gap: 16px;
    }

    h1 {
      color: #fff;
      font-size: 28px;
      margin: 0;
    }

    .subtitle {
      color: #888;
      margin: 4px 0 0;
    }

    .filter-select {
      padding: 10px 16px;
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 8px;
      color: #fff;
      font-size: 14px;
      min-width: 150px;
    }

    .filter-select option {
      background: #1a1a2e;
      color: #fff;
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 64px;
      color: #888;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid rgba(0, 217, 255, 0.2);
      border-top-color: #00d9ff;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin-bottom: 16px;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .matches-table-container {
      background: rgba(255, 255, 255, 0.03);
      border-radius: 12px;
      border: 1px solid rgba(255, 255, 255, 0.1);
      overflow: hidden;
    }

    .matches-table {
      width: 100%;
      border-collapse: collapse;
    }

    .matches-table th {
      padding: 16px;
      text-align: left;
      background: rgba(255, 255, 255, 0.05);
      color: #888;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .matches-table td {
      padding: 16px;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
      color: #fff;
    }

    .matches-table tr:hover td {
      background: rgba(255, 255, 255, 0.03);
    }

    .matches-table tr.win {
      border-left: 3px solid #4caf50;
    }

    .matches-table tr.loss {
      border-left: 3px solid #ff5252;
    }

    .champion-name {
      font-weight: 600;
    }

    .result-badge {
      padding: 4px 12px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 600;
    }

    .result-badge.win {
      background: rgba(76, 175, 80, 0.2);
      color: #4caf50;
    }

    .result-badge.loss {
      background: rgba(255, 82, 82, 0.2);
      color: #ff5252;
    }

    .no-data {
      text-align: center;
      color: #666;
      padding: 48px !important;
    }

    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 16px;
      margin-top: 24px;
    }

    .page-btn {
      padding: 10px 20px;
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 8px;
      color: #fff;
      cursor: pointer;
      transition: all 0.2s;
    }

    .page-btn:hover:not(:disabled) {
      background: rgba(255, 255, 255, 0.15);
      border-color: #00d9ff;
    }

    .page-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .page-info {
      color: #888;
    }
  `]
})
export class MatchesComponent implements OnInit, OnDestroy {
  currentProfile: UserProfile | null = null;
  matches: Match[] = [];
  isLoading = true;

  selectedGameMode = '';
  currentPage = 1;
  pageSize = 20;
  totalPages = 1;

  private subscriptions: Subscription[] = [];

  constructor(
    private matchService: MatchService,
    private profileState: ProfileStateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.subscriptions.push(
      this.profileState.currentProfile$.subscribe(profile => {
        this.currentProfile = profile;
      })
    );

    // Inicjalizuj profile i załaduj dane po zakończeniu
    this.profileState.initialize().subscribe({
      next: () => this.loadMatches(),
      error: (err) => {
        console.error('Błąd ładowania profili:', err);
        this.isLoading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadMatches(): void {
    this.isLoading = true;
    const gameMode = this.selectedGameMode || undefined;

    this.matchService.getRecent(100, gameMode).subscribe({
      next: (matches) => {
        this.matches = matches;
        this.totalPages = Math.ceil(matches.length / this.pageSize);
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Błąd ładowania meczy:', err);
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  formatDate(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('pl-PL', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
