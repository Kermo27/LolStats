import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

// Komponent karty statystyk - wielokrotnego użytku
// @Input() to odpowiednik [Parameter] w Blazor
@Component({
    selector: 'app-stat-card',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="stat-card">
      <div class="stat-icon" [style.background]="iconBg">
        {{ icon }}
      </div>
      <div class="stat-content">
        <span class="stat-label">{{ label }}</span>
        <span class="stat-value" [style.color]="valueColor">{{ value }}</span>
        @if (subtitle) {
          <span class="stat-subtitle">{{ subtitle }}</span>
        }
      </div>
    </div>
  `,
    styles: [`
    .stat-card {
      background: rgba(255, 255, 255, 0.05);
      border-radius: 12px;
      padding: 20px;
      display: flex;
      align-items: center;
      gap: 16px;
      border: 1px solid rgba(255, 255, 255, 0.1);
      transition: transform 0.2s, box-shadow 0.2s;
    }
    
    .stat-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
    }
    
    .stat-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 24px;
    }
    
    .stat-content {
      display: flex;
      flex-direction: column;
    }
    
    .stat-label {
      color: #888;
      font-size: 13px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    
    .stat-value {
      font-size: 24px;
      font-weight: 700;
      color: #fff;
    }
    
    .stat-subtitle {
      color: #666;
      font-size: 12px;
    }
  `]
})
export class StatCardComponent {
    // Dane wejściowe komponentu (parametry)
    @Input() label = '';
    @Input() value = '';
    @Input() subtitle = '';
    @Input() icon = '📊';
    @Input() iconBg = 'rgba(0, 217, 255, 0.2)';
    @Input() valueColor = '#fff';
}
