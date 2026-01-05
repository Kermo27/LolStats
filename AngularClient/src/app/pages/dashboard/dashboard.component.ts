import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subscription, forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { StatsService } from '../../core/services/stats.service';
import { MatchService } from '../../core/services/match.service';
import { ProfileStateService } from '../../core/services/profile-state.service';
import { StatCardComponent } from '../../components/stat-card/stat-card.component';
import { StatsOverviewCardComponent, StatsOverviewData } from '../../components/dashboard/stats-overview-card.component';
import { StreakCardComponent, StreakData } from '../../components/dashboard/streak-card.component';
import { BestDuoCardComponent, DuoPartner } from '../../components/dashboard/best-duo-card.component';
import { LpTrendChartComponent, LpDataPoint } from '../../components/dashboard/lp-trend-chart.component';
import { ActivityHeatmapComponent, ActivityData } from '../../components/dashboard/activity-heatmap.component';
import { Overview, ChampionStats, Match, UserProfile } from '../../models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    StatCardComponent,
    StatsOverviewCardComponent,
    StreakCardComponent,
    BestDuoCardComponent,
    LpTrendChartComponent,
    ActivityHeatmapComponent
  ],
  template: `
    <div class="dashboard">
      <!-- Header -->
      <header class="dashboard-header">
        <div class="header-left">
          <div class="icon-badge">📊</div>
          <div>
            <h1>Dashboard</h1>
            <p class="subtitle">Stats for: {{ currentProfile?.name || 'No profile' }}</p>
          </div>
        </div>
        
        <div class="filters">
          <select [(ngModel)]="selectedRole" (ngModelChange)="onRoleChange()" class="filter-select">
            <option value="All">All Roles</option>
            <option value="Top">Top</option>
            <option value="Jungle">Jungle</option>
            <option value="Mid">Mid</option>
            <option value="ADC">ADC</option>
            <option value="Support">Support</option>
          </select>
          
          <select [(ngModel)]="selectedGameMode" (ngModelChange)="onGameModeChange()" class="filter-select">
            <option value="All">All Modes</option>
            <option value="Ranked Solo">Ranked Solo</option>
            <option value="Ranked Flex">Ranked Flex</option>
            <option value="Normal">Normal</option>
            <option value="ARAM">ARAM</option>
          </select>
        </div>
      </header>

      @if (isLoading) {
        <div class="loading-container">
          <div class="loading-spinner"></div>
          <p>Analyzing your stats...</p>
        </div>
      } @else {
        <!-- Main Stats Grid -->
        <section class="stats-grid">
          <div class="grid-item">
            <app-stats-overview-card [stats]="overviewData"></app-stats-overview-card>
          </div>
          
          <div class="grid-item">
            <app-streak-card [streak]="streakData"></app-streak-card>
          </div>
          
          <div class="grid-item">
            <app-best-duo-card [duos]="duoPartners"></app-best-duo-card>
          </div>
        </section>

        <!-- Quick Stats Row -->
        <section class="quick-stats">
          <app-stat-card
            label="Most Played"
            [value]="overview?.mostPlayedChampion || '-'"
            [subtitle]="overview ? overview.mostPlayedChampionGames + ' games' : ''"
            icon="⚔️"
            iconBg="rgba(251, 191, 36, 0.2)">
          </app-stat-card>
          
          <app-stat-card
            label="Favorite Support"
            [value]="overview?.favoriteSupport || '-'"
            [subtitle]="overview ? overview.favoriteSupportGames + ' games' : ''"
            icon="🛡️"
            iconBg="rgba(139, 92, 246, 0.2)">
          </app-stat-card>
          
          <app-stat-card
            label="Total Matches"
            [value]="recentMatches.length.toString()"
            subtitle="In history"
            icon="🎮"
            iconBg="rgba(34, 197, 94, 0.2)">
          </app-stat-card>
          
          <app-stat-card
            label="Avg KDA"
            [value]="avgKda"
            [valueColor]="kdaColor"
            icon="💀"
            iconBg="rgba(239, 68, 68, 0.2)">
          </app-stat-card>
        </section>

        <!-- Charts Row -->
        <section class="charts-row">
          <div class="chart-container wide">
            <app-lp-trend-chart [data]="lpData"></app-lp-trend-chart>
          </div>
        </section>

        <!-- Activity & Champions -->
        <section class="bottom-section">
          <div class="activity-container">
            <app-activity-heatmap [data]="activityData" [maxGames]="10"></app-activity-heatmap>
          </div>

          <!-- Top Champions -->
          <div class="champions-card glass-card">
            <div class="card-header">
              <span class="card-icon">👑</span>
              <h3>Top Champions</h3>
              <a routerLink="/champions" class="view-all">View all →</a>
            </div>
            
            <div class="champions-list">
              @for (champ of filteredChampions; track champ.championName) {
                <div class="champion-row">
                  <img 
                    [src]="getChampionIcon(champ.championName)" 
                    [alt]="champ.championName"
                    class="champion-icon"
                    (error)="onImageError($event)"
                  />
                  <div class="champion-info">
                    <span class="name">{{ champ.championName }}</span>
                    <span class="games">{{ champ.games }} games</span>
                  </div>
                  <div class="champion-stats">
                    <span class="winrate" [class.positive]="champ.winrate >= 50" [class.negative]="champ.winrate < 50">
                      {{ champ.winrate.toFixed(0) }}%
                    </span>
                    <span class="kda">{{ champ.avgKda.toFixed(1) }} KDA</span>
                  </div>
                </div>
              } @empty {
                <div class="no-data">No champion data</div>
              }
            </div>
          </div>
        </section>
      }
    </div>
  `,
  styles: [`
    .dashboard {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      flex-wrap: wrap;
      gap: 20px;
    }

    .header-left {
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
      font-size: 32px;
      margin: 0;
      font-weight: 700;
    }

    .subtitle {
      color: #888;
      margin: 4px 0 0;
    }

    .filters {
      display: flex;
      gap: 12px;
    }

    .filter-select {
      padding: 12px 16px;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 12px;
      color: #fff;
      font-size: 14px;
      cursor: pointer;
      min-width: 150px;
      transition: all 0.2s ease;
    }

    .filter-select:hover {
      background: rgba(255, 255, 255, 0.08);
    }

    .filter-select:focus {
      outline: none;
      border-color: #00d9ff;
    }

    .filter-select option {
      background: #1a1a2e;
      color: #fff;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 80px 20px;
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

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 20px;
      margin-bottom: 24px;
    }

    .quick-stats {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }

    .charts-row {
      display: flex;
      gap: 20px;
      margin-bottom: 24px;
    }

    .chart-container.wide {
      flex: 1;
    }

    .bottom-section {
      display: grid;
      grid-template-columns: 1.5fr 1fr;
      gap: 20px;
    }

    .glass-card {
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 20px;
      padding: 24px;
    }

    .champions-card .card-header {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 20px;
    }

    .card-icon {
      font-size: 20px;
    }

    .champions-card h3 {
      color: #fff;
      font-size: 16px;
      font-weight: 600;
      margin: 0;
      flex: 1;
    }

    .view-all {
      color: #00d9ff;
      text-decoration: none;
      font-size: 13px;
      transition: opacity 0.2s;
    }

    .view-all:hover {
      opacity: 0.8;
    }

    .champions-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .champion-row {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 12px;
      background: rgba(255, 255, 255, 0.03);
      border-radius: 12px;
      transition: all 0.2s ease;
    }

    .champion-row:hover {
      background: rgba(255, 255, 255, 0.06);
    }

    .champion-icon {
      width: 44px;
      height: 44px;
      border-radius: 12px;
      object-fit: cover;
      border: 2px solid rgba(255, 255, 255, 0.1);
    }

    .champion-info {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .champion-info .name {
      color: #fff;
      font-weight: 600;
      font-size: 14px;
    }

    .champion-info .games {
      color: #888;
      font-size: 12px;
    }

    .champion-stats {
      text-align: right;
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .winrate {
      font-weight: 700;
      font-size: 14px;
    }

    .positive { color: #4ade80; }
    .negative { color: #f87171; }

    .kda {
      color: #888;
      font-size: 12px;
    }

    .no-data {
      text-align: center;
      color: #666;
      padding: 24px;
    }

    @media (max-width: 1200px) {
      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }

      .quick-stats {
        grid-template-columns: repeat(2, 1fr);
      }

      .bottom-section {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 768px) {
      .stats-grid {
        grid-template-columns: 1fr;
      }

      .quick-stats {
        grid-template-columns: 1fr;
      }

      .dashboard-header {
        flex-direction: column;
        align-items: flex-start;
      }
    }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  currentProfile: UserProfile | null = null;
  isLoading = true;
  overview: Overview | null = null;
  allChampions: ChampionStats[] = [];
  recentMatches: Match[] = [];

  // Filters
  selectedRole = 'All';
  selectedGameMode = 'Ranked Solo';

  // New component data
  overviewData: StatsOverviewData = { winrate: 0, totalGames: 0, wins: 0, losses: 0 };
  streakData: StreakData = { type: 'none', count: 0, lastGames: [] };
  duoPartners: DuoPartner[] = [];
  lpData: LpDataPoint[] = [];
  activityData: ActivityData[] = [];

  private subscriptions: Subscription[] = [];
  private readonly DDRAGON_VERSION = '14.24.1';

  constructor(
    private authService: AuthService,
    private statsService: StatsService,
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

    this.profileState.initialize().subscribe({
      next: () => this.loadData(),
      error: (err) => {
        console.error('Error loading profiles:', err);
        this.isLoading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadData(): void {
    this.isLoading = true;
    const gameMode = this.selectedGameMode === 'All' ? undefined : this.selectedGameMode;

    forkJoin({
      overview: this.statsService.getOverview(undefined, gameMode),
      champions: this.statsService.getChampionStats(undefined, gameMode),
      recent: this.matchService.getRecent(20),
      duos: this.statsService.getBestDuos(undefined, undefined, gameMode),
      activity: this.statsService.getActivity(3, undefined, gameMode)
    }).subscribe({
      next: (data) => {
        this.overview = data.overview;
        this.allChampions = data.champions;
        this.recentMatches = data.recent;

        // Map to component data
        this.mapOverviewData();
        this.mapStreakData();
        this.mapDuoData(data.duos);
        this.mapActivityData(data.activity);
        this.generateLpData();

        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading data:', err);
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  private mapOverviewData(): void {
    if (!this.overview) return;

    const totalGames = this.recentMatches.length;
    const wins = this.recentMatches.filter(m => m.win).length;

    this.overviewData = {
      winrate: this.overview.winrate,
      totalGames,
      wins,
      losses: totalGames - wins
    };
  }

  private mapStreakData(): void {
    if (this.recentMatches.length === 0) {
      this.streakData = { type: 'none', count: 0, lastGames: [] };
      return;
    }

    const lastGames = this.recentMatches.slice(0, 5).map(m => m.win);
    const firstResult = this.recentMatches[0]?.win;
    let count = 0;

    for (const match of this.recentMatches) {
      if (match.win === firstResult) count++;
      else break;
    }

    this.streakData = {
      type: firstResult ? 'win' : 'loss',
      count,
      lastGames
    };
  }

  private mapDuoData(duos: any): void {
    if (duos?.partners) {
      this.duoPartners = duos.partners.slice(0, 3).map((p: any) => ({
        summonerName: p.summonerName || p.name,
        games: p.games,
        winrate: (p.winrate || 0) * 100,
        avgKda: p.avgKda || 0
      }));
    }
  }

  private mapActivityData(activity: any[]): void {
    if (Array.isArray(activity)) {
      this.activityData = activity.map(a => ({
        date: a.date,
        games: a.games || a.count || 0,
        wins: a.wins || 0
      }));
    }
  }

  private generateLpData(): void {
    // Generate sample LP data for chart demo
    // In production, this would come from API
    const data: LpDataPoint[] = [];
    let lp = 50;

    for (let i = 20; i >= 0; i--) {
      const date = new Date();
      date.setDate(date.getDate() - i);

      lp += Math.floor(Math.random() * 40) - 18;
      lp = Math.max(0, Math.min(100, lp));

      data.push({
        date: date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
        lp,
        rank: 'Gold II'
      });
    }

    this.lpData = data;
  }

  onRoleChange(): void {
    // Filter locally
    this.cdr.markForCheck();
  }

  onGameModeChange(): void {
    this.loadData();
  }

  get filteredChampions(): ChampionStats[] {
    let filtered = this.allChampions;

    if (this.selectedRole !== 'All') {
      filtered = filtered.filter(c => c.role === this.selectedRole);
    }

    return filtered.slice(0, 5);
  }

  get avgKda(): string {
    if (this.allChampions.length === 0) return '-';
    const avg = this.allChampions.reduce((sum, c) => sum + c.avgKda, 0) / this.allChampions.length;
    return avg.toFixed(2);
  }

  get kdaColor(): string {
    const kda = parseFloat(this.avgKda);
    if (isNaN(kda)) return '#fff';
    if (kda >= 4) return '#fbbf24';
    if (kda >= 3) return '#4ade80';
    if (kda >= 2) return '#60a5fa';
    return '#f87171';
  }

  getChampionIcon(name: string): string {
    const formatted = name.replace(/['\s]/g, '').replace(/&/g, '');
    return `https://ddragon.leagueoflegends.com/cdn/${this.DDRAGON_VERSION}/img/champion/${formatted}.png`;
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'https://ddragon.leagueoflegends.com/cdn/14.24.1/img/champion/Aatrox.png';
  }
}
