import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface StreakData {
    type: 'win' | 'loss' | 'none';
    count: number;
    lastGames: boolean[];
}

@Component({
    selector: 'app-streak-card',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="card glass-card" [class.win-streak]="streak.type === 'win'" [class.loss-streak]="streak.type === 'loss'">
      <div class="card-header">
        <span class="card-icon">{{ streak.type === 'win' ? '🔥' : streak.type === 'loss' ? '💀' : '⚖️' }}</span>
        <h3>Current Streak</h3>
      </div>

      <div class="streak-display">
        <span class="streak-count" [class]="streak.type + '-text'">
          @if (streak.type === 'none') {
            -
          } @else {
            {{ streak.count }}
          }
        </span>
        <span class="streak-label">
          @if (streak.type === 'win') {
            Win Streak 🎯
          } @else if (streak.type === 'loss') {
            Loss Streak 😢
          } @else {
            No Active Streak
          }
        </span>
      </div>

      <div class="last-games">
        <span class="label">Last 5 games:</span>
        <div class="game-indicators">
          @for (win of streak.lastGames.slice(0, 5); track $index) {
            <span class="game-dot" [class.win]="win" [class.loss]="!win">
              {{ win ? 'W' : 'L' }}
            </span>
          }
        </div>
      </div>
    </div>
  `,
    styles: [`
    .card {
      padding: 24px;
      border-radius: 20px;
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.08);
      height: 100%;
      transition: all 0.3s ease;
    }

    .win-streak {
      background: linear-gradient(135deg, rgba(34, 197, 94, 0.1), rgba(74, 222, 128, 0.05));
      border-color: rgba(34, 197, 94, 0.2);
    }

    .loss-streak {
      background: linear-gradient(135deg, rgba(239, 68, 68, 0.1), rgba(248, 113, 113, 0.05));
      border-color: rgba(239, 68, 68, 0.2);
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

    .streak-display {
      text-align: center;
      margin-bottom: 20px;
    }

    .streak-count {
      font-size: 56px;
      font-weight: 800;
      display: block;
      line-height: 1;
    }

    .win-text { color: #4ade80; }
    .loss-text { color: #f87171; }
    .none-text { color: #888; }

    .streak-label {
      color: #aaa;
      font-size: 14px;
      margin-top: 8px;
      display: block;
    }

    .last-games {
      padding-top: 16px;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
    }

    .last-games .label {
      color: #888;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      display: block;
      margin-bottom: 10px;
    }

    .game-indicators {
      display: flex;
      gap: 8px;
      justify-content: center;
    }

    .game-dot {
      width: 36px;
      height: 36px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
      font-weight: 700;
    }

    .game-dot.win {
      background: rgba(34, 197, 94, 0.2);
      color: #4ade80;
      border: 1px solid rgba(34, 197, 94, 0.3);
    }

    .game-dot.loss {
      background: rgba(239, 68, 68, 0.2);
      color: #f87171;
      border: 1px solid rgba(239, 68, 68, 0.3);
    }
  `]
})
export class StreakCardComponent {
    @Input() streak: StreakData = {
        type: 'none',
        count: 0,
        lastGames: []
    };
}
