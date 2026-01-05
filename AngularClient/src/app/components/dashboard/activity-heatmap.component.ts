import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ActivityData {
    date: string;
    games: number;
    wins: number;
}

@Component({
    selector: 'app-activity-heatmap',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="card glass-card">
      <div class="card-header">
        <span class="card-icon">🗓️</span>
        <h3>Activity Heatmap</h3>
        <span class="total-games">{{ totalGames }} games in {{ weeks }} weeks</span>
      </div>

      <div class="heatmap-container">
        <div class="weekdays">
          <span>Mon</span>
          <span>Wed</span>
          <span>Fri</span>
          <span>Sun</span>
        </div>

        <div class="heatmap-grid">
          @for (week of weeksData; track $index) {
            <div class="week-column">
              @for (day of week; track $index) {
                <div
                  class="day-cell"
                  [style.background]="getCellColor(day.games)"
                  [title]="day.date + ': ' + day.games + ' games'"
                ></div>
              }
            </div>
          }
        </div>
      </div>

      <div class="legend">
        <span class="label">Less</span>
        <div class="legend-cells">
          <div class="legend-cell" [style.background]="getCellColor(0)"></div>
          <div class="legend-cell" [style.background]="getCellColor(1)"></div>
          <div class="legend-cell" [style.background]="getCellColor(3)"></div>
          <div class="legend-cell" [style.background]="getCellColor(5)"></div>
          <div class="legend-cell" [style.background]="getCellColor(8)"></div>
        </div>
        <span class="label">More</span>
      </div>
    </div>
  `,
    styles: [`
    .card {
      padding: 24px;
      border-radius: 20px;
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.08);
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
      flex: 1;
    }

    .total-games {
      color: #888;
      font-size: 13px;
    }

    .heatmap-container {
      display: flex;
      gap: 8px;
      overflow-x: auto;
      padding-bottom: 8px;
    }

    .weekdays {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      padding: 2px 0;
      color: #666;
      font-size: 10px;
      min-width: 28px;
    }

    .heatmap-grid {
      display: flex;
      gap: 3px;
    }

    .week-column {
      display: flex;
      flex-direction: column;
      gap: 3px;
    }

    .day-cell {
      width: 14px;
      height: 14px;
      border-radius: 3px;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .day-cell:hover {
      transform: scale(1.3);
      outline: 2px solid rgba(255, 255, 255, 0.3);
    }

    .legend {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 8px;
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
    }

    .legend .label {
      color: #666;
      font-size: 11px;
    }

    .legend-cells {
      display: flex;
      gap: 3px;
    }

    .legend-cell {
      width: 12px;
      height: 12px;
      border-radius: 2px;
    }
  `]
})
export class ActivityHeatmapComponent {
    @Input() data: ActivityData[] = [];
    @Input() maxGames: number = 10;

    get weeksData(): ActivityData[][] {
        const weeks: ActivityData[][] = [];
        const today = new Date();

        for (let w = 11; w >= 0; w--) {
            const week: ActivityData[] = [];
            for (let d = 0; d < 7; d++) {
                const date = new Date(today);
                date.setDate(date.getDate() - (w * 7 + (6 - d)));

                const dateStr = date.toISOString().split('T')[0];
                const existingData = this.data.find(a => a.date === dateStr);

                week.push(existingData || { date: dateStr, games: 0, wins: 0 });
            }
            weeks.push(week);
        }

        return weeks;
    }

    get weeks(): number {
        return 12;
    }

    get totalGames(): number {
        return this.data.reduce((sum, d) => sum + d.games, 0);
    }

    getCellColor(games: number): string {
        if (games === 0) return 'rgba(255, 255, 255, 0.05)';

        const intensity = Math.min(games / this.maxGames, 1);
        const alpha = 0.2 + (intensity * 0.8);

        return `rgba(0, 217, 255, ${alpha})`;
    }
}
