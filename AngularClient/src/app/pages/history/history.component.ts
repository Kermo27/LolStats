import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatchService } from '../../core/services/match.service';
import { Match, PaginatedResponse } from '../../models';

// Strona historii meczy z paginacją
@Component({
    selector: 'app-history',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="history-page">
      <header class="page-header">
        <div>
          <a routerLink="/" class="back-link">← Wróć do Dashboard</a>
          <h1>Historia Meczy</h1>
        </div>
      </header>

      @if (isLoading) {
        <div class="loading">Ładowanie meczy...</div>
      } @else {
        <!-- Tabela meczy -->
        <div class="matches-table">
          <div class="table-header">
            <span>Champion</span>
            <span>Rola</span>
            <span>K/D/A</span>
            <span>CS</span>
            <span>Wynik</span>
            <span>Data</span>
          </div>
          
          @for (match of matches; track match.id) {
            <div class="table-row" [class.win]="match.win" [class.loss]="!match.win">
              <span class="champion">{{ match.champion }}</span>
              <span class="role">{{ match.role }}</span>
              <span class="kda">{{ match.kills }}/{{ match.deaths }}/{{ match.assists }}</span>
              <span>{{ match.cs }}</span>
              <span class="result">
                @if (match.win) {
                  <span class="badge win">Wygrana</span>
                } @else {
                  <span class="badge loss">Przegrana</span>
                }
              </span>
              <span class="date">{{ formatDate(match.date) }}</span>
            </div>
          } @empty {
            <div class="no-data">Brak meczy do wyświetlenia</div>
          }
        </div>

        <!-- Paginacja -->
        @if (totalPages > 1) {
          <div class="pagination">
            <button 
              [disabled]="currentPage === 1" 
              (click)="goToPage(currentPage - 1)"
              class="page-btn">
              ← Poprzednia
            </button>
            
            <span class="page-info">
              Strona {{ currentPage }} z {{ totalPages }}
            </span>
            
            <button 
              [disabled]="currentPage >= totalPages" 
              (click)="goToPage(currentPage + 1)"
              class="page-btn">
              Następna →
            </button>
          </div>
        }
      }
    </div>
  `,
    styles: [`
    .history-page {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .page-header {
      margin-bottom: 32px;
    }
    
    .back-link {
      color: #00d9ff;
      text-decoration: none;
      font-size: 14px;
    }
    
    .back-link:hover {
      text-decoration: underline;
    }
    
    h1 {
      color: #fff;
      font-size: 32px;
      margin: 8px 0 0;
    }
    
    .loading, .no-data {
      text-align: center;
      padding: 48px;
      color: #888;
    }
    
    .matches-table {
      background: rgba(255, 255, 255, 0.03);
      border-radius: 16px;
      overflow: hidden;
      border: 1px solid rgba(255, 255, 255, 0.1);
    }
    
    .table-header, .table-row {
      display: grid;
      grid-template-columns: 2fr 1fr 1.5fr 1fr 1.5fr 1.5fr;
      padding: 14px 20px;
      align-items: center;
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
      transition: background 0.2s;
    }
    
    .table-row:hover {
      background: rgba(255, 255, 255, 0.03);
    }
    
    .table-row.win {
      border-left: 3px solid #4caf50;
    }
    
    .table-row.loss {
      border-left: 3px solid #ff5252;
    }
    
    .champion {
      font-weight: 600;
    }
    
    .role {
      color: #888;
    }
    
    .kda {
      font-family: monospace;
      font-size: 14px;
    }
    
    .badge {
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
    }
    
    .badge.win {
      background: rgba(76, 175, 80, 0.2);
      color: #4caf50;
    }
    
    .badge.loss {
      background: rgba(255, 82, 82, 0.2);
      color: #ff5252;
    }
    
    .date {
      color: #888;
      font-size: 13px;
    }
    
    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 20px;
      margin-top: 24px;
    }
    
    .page-btn {
      padding: 10px 20px;
      background: rgba(0, 217, 255, 0.1);
      border: 1px solid rgba(0, 217, 255, 0.3);
      border-radius: 8px;
      color: #00d9ff;
      cursor: pointer;
      transition: background 0.2s;
    }
    
    .page-btn:hover:not(:disabled) {
      background: rgba(0, 217, 255, 0.2);
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
export class HistoryComponent implements OnInit {
    matches: Match[] = [];
    isLoading = true;
    currentPage = 1;
    pageSize = 20;
    totalPages = 1;
    totalCount = 0;

    constructor(private matchService: MatchService) { }

    ngOnInit(): void {
        this.loadMatches();
    }

    loadMatches(): void {
        this.isLoading = true;

        this.matchService.getPaginated(this.currentPage, this.pageSize).subscribe({
            next: (response: PaginatedResponse<Match>) => {
                this.matches = response.items;
                this.totalCount = response.totalCount;
                this.totalPages = response.totalPages;
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Błąd ładowania meczy:', err);
                this.isLoading = false;
            }
        });
    }

    goToPage(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
            this.loadMatches();
        }
    }

    formatDate(date: Date): string {
        return new Date(date).toLocaleDateString('pl-PL', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    }
}
