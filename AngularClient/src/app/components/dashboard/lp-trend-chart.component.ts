import { Component, Input, OnChanges, SimpleChanges, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

export interface LpDataPoint {
    date: string;
    lp: number;
    rank: string;
}

@Component({
    selector: 'app-lp-trend-chart',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="card glass-card">
      <div class="card-header">
        <span class="card-icon">📈</span>
        <h3>LP Trend</h3>
        <div class="badge" [class.positive]="lpChange >= 0" [class.negative]="lpChange < 0">
          {{ lpChange >= 0 ? '+' : '' }}{{ lpChange }} LP
        </div>
      </div>
      
      <div class="chart-container">
        <canvas #chartCanvas></canvas>
      </div>
      
      @if (data.length > 0) {
        <div class="chart-footer">
          <div class="stat">
            <span class="label">Current</span>
            <span class="value">{{ data[data.length - 1]?.rank }}</span>
          </div>
          <div class="stat">
            <span class="label">Peak</span>
            <span class="value">{{ peakLp }} LP</span>
          </div>
          <div class="stat">
            <span class="label">Games</span>
            <span class="value">{{ data.length }}</span>
          </div>
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
    
    .badge {
      padding: 6px 12px;
      border-radius: 8px;
      font-size: 13px;
      font-weight: 600;
    }
    
    .badge.positive {
      background: rgba(34, 197, 94, 0.2);
      color: #4ade80;
    }
    
    .badge.negative {
      background: rgba(239, 68, 68, 0.2);
      color: #f87171;
    }
    
    .chart-container {
      height: 200px;
      position: relative;
    }
    
    .chart-footer {
      display: flex;
      justify-content: space-around;
      padding-top: 16px;
      margin-top: 16px;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
    }
    
    .stat {
      text-align: center;
    }
    
    .stat .label {
      display: block;
      color: #888;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 4px;
    }
    
    .stat .value {
      color: #fff;
      font-size: 14px;
      font-weight: 600;
    }
    
    .positive { color: #4ade80; }
    .negative { color: #f87171; }
  `]
})
export class LpTrendChartComponent implements AfterViewInit, OnChanges {
    @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
    @Input() data: LpDataPoint[] = [];

    private chart: Chart | null = null;

    get lpChange(): number {
        if (this.data.length < 2) return 0;
        return this.data[this.data.length - 1].lp - this.data[0].lp;
    }

    get peakLp(): number {
        if (this.data.length === 0) return 0;
        return Math.max(...this.data.map(d => d.lp));
    }

    ngAfterViewInit(): void {
        this.createChart();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['data'] && this.chart) {
            this.updateChart();
        }
    }

    private createChart(): void {
        if (!this.chartCanvas) return;

        const ctx = this.chartCanvas.nativeElement.getContext('2d');
        if (!ctx) return;

        const gradient = ctx.createLinearGradient(0, 0, 0, 200);
        gradient.addColorStop(0, 'rgba(0, 217, 255, 0.3)');
        gradient.addColorStop(1, 'rgba(0, 217, 255, 0)');

        this.chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: this.data.map(d => d.date),
                datasets: [{
                    data: this.data.map(d => d.lp),
                    borderColor: '#00d9ff',
                    backgroundColor: gradient,
                    fill: true,
                    tension: 0.4,
                    pointRadius: 0,
                    pointHoverRadius: 6,
                    pointHoverBackgroundColor: '#00d9ff',
                    pointHoverBorderColor: '#fff',
                    pointHoverBorderWidth: 2,
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: 'rgba(255, 255, 255, 0.1)',
                        borderWidth: 1,
                        padding: 12,
                        displayColors: false,
                        callbacks: {
                            label: (ctx) => `${ctx.raw} LP`
                        }
                    }
                },
                scales: {
                    x: {
                        display: false
                    },
                    y: {
                        display: false
                    }
                }
            }
        });
    }

    private updateChart(): void {
        if (!this.chart) return;

        this.chart.data.labels = this.data.map(d => d.date);
        this.chart.data.datasets[0].data = this.data.map(d => d.lp);
        this.chart.update();
    }
}
