import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DuoPartner {
    summonerName: string;
    games: number;
    winrate: number;
    avgKda: number;
}

@Component({
    selector: 'app-best-duo-card',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="card glass-card">
      <div class="card-header">
        <span class="card-icon">👥</span>
        <h3>Best Duo Partners</h3>
      </div>
      
      @if (duos.length === 0) {
        <div class="no-data">No duo data available</div>
      } @else {
        <div class="duo-list">
          @for (duo of duos.slice(0, 3); track duo.summonerName; let i = $index) {
            <div class="duo-item" [class.first]="i === 0">
              <div class="rank-badge">{{ i + 1 }}</div>
              <div class="duo-info">
                <span class="summoner-name">{{ duo.summonerName }}</span>
                <span class="games-count">{{ duo.games }} games</span>
              </div>
              <div class="duo-stats">
                <span class="winrate" [class.positive]="duo.winrate >= 50" [class.negative]="duo.winrate < 50">
                  {{ duo.winrate.toFixed(0) }}%
                </span>
                <span class="kda">{{ duo.avgKda.toFixed(1) }} KDA</span>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
    styles: [`
    .card {
      padding: 24px;
      border-radius: 20px;
      background: rgba(255, 255, 255, 0.03);
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
    
    .no-data {
      text-align: center;
      color: #666;
      padding: 24px;
    }
    
    .duo-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }
    
    .duo-item {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 14px;
      background: rgba(255, 255, 255, 0.03);
      border-radius: 12px;
      border: 1px solid rgba(255, 255, 255, 0.05);
      transition: all 0.2s ease;
    }
    
    .duo-item:hover {
      background: rgba(255, 255, 255, 0.05);
    }
    
    .duo-item.first {
      background: linear-gradient(135deg, rgba(251, 191, 36, 0.1), rgba(245, 158, 11, 0.05));
      border-color: rgba(251, 191, 36, 0.2);
    }
    
    .rank-badge {
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      color: #fff;
      font-weight: 700;
      font-size: 14px;
    }
    
    .first .rank-badge {
      background: linear-gradient(135deg, #fbbf24, #f59e0b);
      color: #000;
    }
    
    .duo-info {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    
    .summoner-name {
      color: #fff;
      font-weight: 600;
      font-size: 14px;
    }
    
    .games-count {
      color: #888;
      font-size: 12px;
    }
    
    .duo-stats {
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
  `]
})
export class BestDuoCardComponent {
    @Input() duos: DuoPartner[] = [];
}
