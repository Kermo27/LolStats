import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface StatsOverviewData {
    winrate: number;
    totalGames: number;
    wins: number;
    losses: number;
}

@Component({
    selector: 'app-stats-overview-card',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="card glass-card">
      <div class="card-header">
        <span class="card-icon">📊</span>
        <h3>Overview</h3>
      </div>
      
      <div class="main-stat">
        <span class="winrate" [class.positive]="stats.winrate >= 50" [class.negative]="stats.winrate < 50">
          {{ stats.winrate.toFixed(1) }}%
        </span>
        <span class="label">Winrate</span>
      </div>
      
      <div class="winrate-bar-container">
        <div class="winrate-bar">
          <div 
            class="winrate-fill" 
            [style.width.%]="stats.winrate"
            [class.positive]="stats.winrate >= 50"
            [class.negative]="stats.winrate < 50"
          ></div>
        </div>
      </div>
      
      <div class="stats-row">
        <div class="stat-item">
          <span class="value">{{ stats.totalGames }}</span>
          <span class="label">Games</span>
        </div>
        <div class="stat-item">
          <span class="value wins">{{ stats.wins }}</span>
          <span class="label">Wins</span>
        </div>
        <div class="stat-item">
          <span class="value losses">{{ stats.losses }}</span>
          <span class="label">Losses</span>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .card {
      padding: 24px;
      border-radius: 20px;
      background: linear-gradient(135deg, rgba(0, 217, 255, 0.05), rgba(139, 92, 246, 0.05));
      border: 1px solid rgba(255, 255, 255, 0.08);
      height: 100%;
    }
    
    .card-header {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 20px;
    }
    
    .card-icon {
      font-size: 20px;
    }
    
    h3 {
      color: #fff;
      font-size: 16px;
      font-weight: 600;
      margin: 0;
    }
    
    .main-stat {
      text-align: center;
      margin-bottom: 16px;
    }
    
    .winrate {
      font-size: 48px;
      font-weight: 700;
      display: block;
      line-height: 1;
    }
    
    .main-stat .label {
      color: #888;
      font-size: 14px;
      margin-top: 4px;
      display: block;
    }
    
    .positive { color: #4ade80; }
    .negative { color: #f87171; }
    
    .winrate-bar-container {
      margin-bottom: 20px;
    }
    
    .winrate-bar {
      height: 8px;
      background: rgba(255, 255, 255, 0.1);
      border-radius: 4px;
      overflow: hidden;
    }
    
    .winrate-fill {
      height: 100%;
      border-radius: 4px;
      transition: width 0.5s ease;
    }
    
    .winrate-fill.positive { background: linear-gradient(90deg, #22c55e, #4ade80); }
    .winrate-fill.negative { background: linear-gradient(90deg, #ef4444, #f87171); }
    
    .stats-row {
      display: flex;
      justify-content: space-around;
      padding-top: 16px;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
    }
    
    .stat-item {
      text-align: center;
    }
    
    .stat-item .value {
      display: block;
      font-size: 24px;
      font-weight: 700;
      color: #fff;
    }
    
    .stat-item .label {
      color: #888;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    
    .wins { color: #4ade80; }
    .losses { color: #f87171; }
  `]
})
export class StatsOverviewCardComponent {
    @Input() stats: StatsOverviewData = {
        winrate: 0,
        totalGames: 0,
        wins: 0,
        losses: 0
    };
}
