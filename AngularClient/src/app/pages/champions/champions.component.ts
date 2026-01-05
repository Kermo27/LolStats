import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StatsService } from '../../core/services/stats.service';
import { ChampionStats } from '../../models';

@Component({
    selector: 'app-champions',
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
    <div class="champions-page">
      <header class="page-header">
        <div class="header-info">
          <div class="icon-badge">👥</div>
          <div>
            <h1>Champion Stats</h1>
            <p class="subtitle">Performance breakdown by champion</p>
          </div>
        </div>
      </header>

      <!-- Filters & Search -->
      <div class="toolbar">
        <div class="filters">
          <select [(ngModel)]="selectedRole" (ngModelChange)="applyFilters()" class="filter-select">
            <option value="All">All Roles</option>
            <option value="Top">Top</option>
            <option value="Jungle">Jungle</option>
            <option value="Mid">Mid</option>
            <option value="ADC">ADC</option>
            <option value="Support">Support</option>
          </select>

          <select [(ngModel)]="selectedGameMode" (ngModelChange)="loadData()" class="filter-select">
            <option value="All">All Modes</option>
            <option value="Ranked Solo">Ranked Solo</option>
            <option value="Ranked Flex">Ranked Flex</option>
            <option value="Normal">Normal</option>
            <option value="ARAM">ARAM</option>
          </select>
        </div>

        <div class="search-box">
          <span class="search-icon">🔍</span>
          <input 
            type="text" 
            [(ngModel)]="searchQuery" 
            (ngModelChange)="applyFilters()"
            placeholder="Search champion..."
            class="search-input"
          />
        </div>
      </div>

      <!-- Loading State -->
      @if (isLoading) {
        <div class="loading-container">
          <div class="loading-spinner"></div>
          <p>Loading champion stats...</p>
        </div>
      } @else {
        <!-- Champions Table -->
        <div class="table-container glass-card">
          <table class="champions-table">
            <thead>
              <tr>
                <th (click)="sortBy('championName')" class="sortable">
                  Champion
                  <span class="sort-indicator">{{ getSortIndicator('championName') }}</span>
                </th>
                <th (click)="sortBy('role')" class="sortable">
                  Role
                  <span class="sort-indicator">{{ getSortIndicator('role') }}</span>
                </th>
                <th (click)="sortBy('games')" class="sortable">
                  Games
                  <span class="sort-indicator">{{ getSortIndicator('games') }}</span>
                </th>
                <th (click)="sortBy('winrate')" class="sortable">
                  Winrate
                  <span class="sort-indicator">{{ getSortIndicator('winrate') }}</span>
                </th>
                <th>W / L</th>
                <th (click)="sortBy('avgKda')" class="sortable">
                  KDA
                  <span class="sort-indicator">{{ getSortIndicator('avgKda') }}</span>
                </th>
                <th (click)="sortBy('avgCsm')" class="sortable">
                  CS/min
                  <span class="sort-indicator">{{ getSortIndicator('avgCsm') }}</span>
                </th>
                <th>Performance</th>
              </tr>
            </thead>
            <tbody>
              @for (champ of filteredChampions; track champ.championName) {
                <tr class="champion-row">
                  <td>
                    <div class="champion-info">
                      <img 
                        [src]="getChampionIcon(champ.championName)" 
                        [alt]="champ.championName"
                        class="champion-icon"
                        (error)="onImageError($event)"
                      />
                      <span class="champion-name">{{ champ.championName }}</span>
                    </div>
                  </td>
                  <td>
                    <span class="role-chip" [class]="'role-' + champ.role.toLowerCase()">
                      {{ champ.role }}
                    </span>
                  </td>
                  <td>{{ champ.games }}</td>
                  <td>
                    <div class="winrate-cell">
                      <span [class.positive]="champ.winrate >= 50" [class.negative]="champ.winrate < 50">
                        {{ champ.winrate.toFixed(1) }}%
                      </span>
                      <div class="winrate-bar">
                        <div 
                          class="winrate-fill" 
                          [style.width.%]="champ.winrate"
                          [class.positive]="champ.winrate >= 50"
                          [class.negative]="champ.winrate < 50"
                        ></div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span class="wins">{{ champ.wins }}W</span> / 
                    <span class="losses">{{ champ.losses }}L</span>
                  </td>
                  <td [class]="getKdaClass(champ.avgKda)">
                    {{ champ.avgKda.toFixed(2) }}
                  </td>
                  <td>{{ champ.avgCsm?.toFixed(1) || '-' }}</td>
                  <td>
                    @if (champ.games >= 5 && champ.winrate >= 60) {
                      <span class="badge badge-carry">⭐ CARRY</span>
                    } @else if (champ.games >= 5 && champ.winrate <= 40) {
                      <span class="badge badge-inting">📉 INTING</span>
                    } @else if (champ.games < 3) {
                      <span class="badge badge-new">New</span>
                    }
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="8" class="no-data">No champion data found</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
    styles: [`
    .champions-page {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .page-header {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 32px;
    }

    .header-info {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .icon-badge {
      width: 56px;
      height: 56px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 28px;
      background: linear-gradient(135deg, rgba(0, 217, 255, 0.2), rgba(139, 92, 246, 0.2));
      border-radius: 16px;
      border: 1px solid rgba(255, 255, 255, 0.1);
    }

    h1 {
      color: #fff;
      font-size: 28px;
      margin: 0;
      font-weight: 700;
    }

    .subtitle {
      color: #888;
      margin: 4px 0 0;
      font-size: 14px;
    }

    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      flex-wrap: wrap;
      gap: 16px;
    }

    .filters {
      display: flex;
      gap: 12px;
    }

    .filter-select {
      padding: 10px 16px;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 10px;
      color: #fff;
      font-size: 14px;
      cursor: pointer;
      min-width: 140px;
      transition: all 0.2s ease;
    }

    .filter-select:hover {
      background: rgba(255, 255, 255, 0.08);
      border-color: rgba(255, 255, 255, 0.2);
    }

    .filter-select:focus {
      outline: none;
      border-color: #00d9ff;
      box-shadow: 0 0 0 3px rgba(0, 217, 255, 0.1);
    }

    .filter-select option {
      background: #1a1a2e;
      color: #fff;
    }

    .search-box {
      position: relative;
      display: flex;
      align-items: center;
    }

    .search-icon {
      position: absolute;
      left: 14px;
      font-size: 14px;
      opacity: 0.5;
    }

    .search-input {
      padding: 10px 16px 10px 40px;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 10px;
      color: #fff;
      font-size: 14px;
      width: 250px;
      transition: all 0.2s ease;
    }

    .search-input::placeholder {
      color: rgba(255, 255, 255, 0.4);
    }

    .search-input:focus {
      outline: none;
      border-color: #00d9ff;
      box-shadow: 0 0 0 3px rgba(0, 217, 255, 0.1);
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 64px;
      color: #888;
    }

    .loading-spinner {
      width: 48px;
      height: 48px;
      border: 3px solid rgba(255, 255, 255, 0.1);
      border-top-color: #00d9ff;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin-bottom: 16px;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .glass-card {
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 16px;
      backdrop-filter: blur(10px);
      overflow: hidden;
    }

    .table-container {
      overflow-x: auto;
    }

    .champions-table {
      width: 100%;
      border-collapse: collapse;
    }

    .champions-table th,
    .champions-table td {
      padding: 14px 16px;
      text-align: left;
    }

    .champions-table th {
      background: rgba(255, 255, 255, 0.05);
      color: #888;
      font-size: 12px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      white-space: nowrap;
    }

    .sortable {
      cursor: pointer;
      user-select: none;
      transition: color 0.2s ease;
    }

    .sortable:hover {
      color: #00d9ff;
    }

    .sort-indicator {
      margin-left: 4px;
      opacity: 0.5;
    }

    .champion-row {
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
      color: #fff;
      transition: background 0.2s ease;
    }

    .champion-row:hover {
      background: rgba(255, 255, 255, 0.03);
    }

    .champion-info {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .champion-icon {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      object-fit: cover;
      border: 2px solid rgba(255, 255, 255, 0.1);
    }

    .champion-name {
      font-weight: 600;
    }

    .role-chip {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 6px;
      font-size: 12px;
      font-weight: 600;
      text-transform: uppercase;
    }

    .role-top { background: rgba(239, 68, 68, 0.2); color: #ef4444; }
    .role-jungle { background: rgba(34, 197, 94, 0.2); color: #22c55e; }
    .role-mid { background: rgba(59, 130, 246, 0.2); color: #3b82f6; }
    .role-adc { background: rgba(249, 115, 22, 0.2); color: #f97316; }
    .role-support { background: rgba(168, 85, 247, 0.2); color: #a855f7; }

    .winrate-cell {
      display: flex;
      flex-direction: column;
      gap: 4px;
      min-width: 100px;
    }

    .winrate-bar {
      height: 4px;
      background: rgba(255, 255, 255, 0.1);
      border-radius: 2px;
      overflow: hidden;
    }

    .winrate-fill {
      height: 100%;
      border-radius: 2px;
      transition: width 0.3s ease;
    }

    .winrate-fill.positive { background: linear-gradient(90deg, #22c55e, #4ade80); }
    .winrate-fill.negative { background: linear-gradient(90deg, #ef4444, #f87171); }

    .positive { color: #4ade80; }
    .negative { color: #f87171; }

    .wins { color: #4ade80; }
    .losses { color: #f87171; }

    .kda-excellent { color: #fbbf24; font-weight: 600; }
    .kda-great { color: #22c55e; }
    .kda-good { color: #60a5fa; }
    .kda-average { color: #9ca3af; }
    .kda-poor { color: #f87171; }

    .badge {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 6px;
      font-size: 11px;
      font-weight: 700;
    }

    .badge-carry {
      background: linear-gradient(135deg, rgba(251, 191, 36, 0.2), rgba(245, 158, 11, 0.2));
      color: #fbbf24;
      border: 1px solid rgba(251, 191, 36, 0.3);
    }

    .badge-inting {
      background: rgba(239, 68, 68, 0.2);
      color: #ef4444;
      border: 1px solid rgba(239, 68, 68, 0.3);
    }

    .badge-new {
      background: rgba(255, 255, 255, 0.1);
      color: #9ca3af;
      border: 1px solid rgba(255, 255, 255, 0.1);
    }

    .no-data {
      text-align: center;
      padding: 48px !important;
      color: #666;
    }

    @media (max-width: 768px) {
      .toolbar {
        flex-direction: column;
        align-items: stretch;
      }

      .filters {
        flex-wrap: wrap;
      }

      .search-input {
        width: 100%;
      }
    }
  `]
})
export class ChampionsComponent implements OnInit {
    allChampions: ChampionStats[] = [];
    filteredChampions: ChampionStats[] = [];
    isLoading = true;

    selectedRole = 'All';
    selectedGameMode = 'All';
    searchQuery = '';

    sortColumn = 'games';
    sortDirection: 'asc' | 'desc' = 'desc';

    private readonly DDRAGON_VERSION = '14.24.1';

    constructor(private statsService: StatsService) { }

    ngOnInit(): void {
        this.loadData();
    }

    loadData(): void {
        this.isLoading = true;
        const gameMode = this.selectedGameMode === 'All' ? undefined : this.selectedGameMode;

        this.statsService.getChampionStats(undefined, gameMode).subscribe({
            next: (data) => {
                this.allChampions = data;
                this.applyFilters();
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading champions:', err);
                this.isLoading = false;
            }
        });
    }

    applyFilters(): void {
        let filtered = [...this.allChampions];

        // Filter by role
        if (this.selectedRole !== 'All') {
            filtered = filtered.filter(c => c.role === this.selectedRole);
        }

        // Filter by search query
        if (this.searchQuery.trim()) {
            const query = this.searchQuery.toLowerCase();
            filtered = filtered.filter(c =>
                c.championName.toLowerCase().includes(query)
            );
        }

        // Sort
        filtered.sort((a, b) => {
            const aVal = (a as any)[this.sortColumn];
            const bVal = (b as any)[this.sortColumn];

            if (typeof aVal === 'string') {
                return this.sortDirection === 'asc'
                    ? aVal.localeCompare(bVal)
                    : bVal.localeCompare(aVal);
            }

            return this.sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
        });

        this.filteredChampions = filtered;
    }

    sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = 'desc';
        }
        this.applyFilters();
    }

    getSortIndicator(column: string): string {
        if (this.sortColumn !== column) return '';
        return this.sortDirection === 'asc' ? '↑' : '↓';
    }

    getChampionIcon(name: string): string {
        const formatted = name.replace(/['\s]/g, '').replace(/&/g, '');
        return `https://ddragon.leagueoflegends.com/cdn/${this.DDRAGON_VERSION}/img/champion/${formatted}.png`;
    }

    onImageError(event: Event): void {
        (event.target as HTMLImageElement).src = 'https://ddragon.leagueoflegends.com/cdn/14.24.1/img/champion/Aatrox.png';
    }

    getKdaClass(kda: number): string {
        if (kda >= 5) return 'kda-excellent';
        if (kda >= 4) return 'kda-great';
        if (kda >= 3) return 'kda-good';
        if (kda >= 2) return 'kda-average';
        return 'kda-poor';
    }
}
