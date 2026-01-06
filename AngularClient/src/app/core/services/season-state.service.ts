import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Season } from '../../models';
import { environment } from '../../../environments/environment';

/**
 * SeasonStateService - zarządza stanem sezonów
 * Odpowiednik ISeasonState z Blazora
 * 
 * Używa Angular Signals (Angular 17+) zamiast BehaviorSubject
 * dla lepszej wydajności i prostszego kodu
 */
@Injectable({
    providedIn: 'root'
})
export class SeasonStateService {
    private readonly API_URL = `${environment.apiUrl}/seasons`;

    // Signals - reaktywny stan (Angular 17+ best practice)
    private _allSeasons = signal<Season[]>([]);
    private _currentSeason = signal<Season | null>(null);
    private _isInitialized = signal(false);

    // Readonly signals dla komponentów
    readonly allSeasons = this._allSeasons.asReadonly();
    readonly currentSeason = this._currentSeason.asReadonly();
    readonly isInitialized = this._isInitialized.asReadonly();

    // Computed signal - czy filtrowanie jest aktywne
    readonly isFilteringEnabled = computed(() => this._currentSeason() !== null);

    constructor(private http: HttpClient) { }

    /**
     * Inicjalizuje sezony z API
     */
    initialize(): Observable<Season[]> {
        return this.http.get<Season[]>(this.API_URL).pipe(
            tap(seasons => {
                this._allSeasons.set(seasons);

                // Sprawdź zapisany sezon w localStorage
                const savedSeasonId = localStorage.getItem('season_id');
                if (savedSeasonId) {
                    const season = seasons.find(s => s.id === parseInt(savedSeasonId));
                    if (season) {
                        this._currentSeason.set(season);
                    }
                }

                this._isInitialized.set(true);
            })
        );
    }

    /**
     * Ustawia aktywny sezon do filtrowania
     */
    setCurrentSeason(season: Season | null): void {
        this._currentSeason.set(season);

        if (season) {
            localStorage.setItem('season_id', season.id.toString());
        } else {
            localStorage.removeItem('season_id');
        }
    }

    /**
     * Czyści filtr sezonu
     */
    clearSeasonFilter(): void {
        this.setCurrentSeason(null);
    }

    /**
     * Zwraca daty aktualnego sezonu (do filtrowania API)
     */
    getCurrentSeasonDates(): { startDate: Date | null, endDate: Date | null } {
        const season = this._currentSeason();
        if (!season) {
            return { startDate: null, endDate: null };
        }
        return {
            startDate: new Date(season.startDate),
            endDate: season.endDate ? new Date(season.endDate) : null
        };
    }
}
