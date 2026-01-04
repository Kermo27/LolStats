import { Component, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeasonStateService } from '../../core/services/season-state.service';
import { Season } from '../../models';

/**
 * SeasonSelector - wybór sezonu do filtrowania danych
 * Odpowiednik SeasonSelector.razor z Blazora
 */
@Component({
    selector: 'app-season-selector',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="season-selector">
      <select 
        class="season-select" 
        [value]="currentSeasonId()" 
        (change)="onSeasonChange($event)">
        <option value="">Wszystkie sezony</option>
        @for (season of seasons(); track season.id) {
          <option [value]="season.id">
            {{ season.name }}
            @if (isCurrentSeason(season)) {
              (Aktualny)
            }
          </option>
        }
      </select>
    </div>
  `,
    styles: [`
    .season-selector {
      display: flex;
      align-items: center;
    }
    
    .season-select {
      padding: 8px 12px;
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 6px;
      color: #fff;
      font-size: 14px;
      cursor: pointer;
      min-width: 160px;
    }
    
    .season-select:focus {
      outline: none;
      border-color: #00d9ff;
    }
    
    .season-select option {
      background: #1a1a2e;
      color: #fff;
    }
  `]
})
export class SeasonSelectorComponent implements OnInit {
    // Computed signal - automatycznie reaguje na zmiany
    seasons = computed(() => this.seasonState.allSeasons());
    currentSeasonId = computed(() => this.seasonState.currentSeason()?.id.toString() || '');

    constructor(private seasonState: SeasonStateService) { }

    ngOnInit(): void {
        if (!this.seasonState.isInitialized()) {
            this.seasonState.initialize().subscribe();
        }
    }

    onSeasonChange(event: Event): void {
        const select = event.target as HTMLSelectElement;
        const seasonId = select.value;

        if (!seasonId) {
            this.seasonState.clearSeasonFilter();
        } else {
            const season = this.seasons().find(s => s.id === parseInt(seasonId));
            if (season) {
                this.seasonState.setCurrentSeason(season);
            }
        }

        // Przeładuj stronę żeby dane się zaktualizowały
        window.location.reload();
    }

    isCurrentSeason(season: Season): boolean {
        const now = new Date();
        const start = new Date(season.startDate);
        const end = season.endDate ? new Date(season.endDate) : null;

        return now >= start && (!end || now <= end);
    }
}

