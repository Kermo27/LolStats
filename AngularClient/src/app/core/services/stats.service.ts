
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
    Overview,
    ChampionStats,
    EnemyStats,
    ActivityDay,
    DuoSummary,
    StatsSummary
} from '../../models';
import { environment } from '../../../environments/environment';

// Serwis do pobierania statystyk z API
// Odpowiednik StatsService z backendu C#
@Injectable({
    providedIn: 'root'
})
export class StatsService {
    private readonly API_URL = `${environment.apiUrl}/stats`;

    constructor(private http: HttpClient) { }

    // Pobiera podstawowy przegląd (winrate, najczęstszy champion)
    // GET /api/stats/overview
    getOverview(seasonId?: number, gameMode?: string): Observable<Overview> {
        const params = this.buildParams(seasonId, gameMode);
        return this.http.get<Overview>(`${this.API_URL}/overview`, { params });
    }

    // Pobiera statystyki championów
    // GET /api/stats/champions
    getChampionStats(seasonId?: number, gameMode?: string): Observable<ChampionStats[]> {
        const params = this.buildParams(seasonId, gameMode);
        return this.http.get<ChampionStats[]>(`${this.API_URL}/champions`, { params });
    }

    // Pobiera najtrudniejszych przeciwników
    // GET /api/stats/hardest-enemies
    getHardestEnemies(role?: string, seasonId?: number, gameMode?: string): Observable<EnemyStats[]> {
        let params = this.buildParams(seasonId, gameMode);
        if (role) {
            params = params.set('playerRole', role);
        }
        return this.http.get<EnemyStats[]>(`${this.API_URL}/hardest-enemies`, { params });
    }

    // Pobiera aktywność (ile gier dziennie)
    // GET /api/stats/activity?months=6
    getActivity(months: number = 6, seasonId?: number, gameMode?: string): Observable<ActivityDay[]> {
        let params = this.buildParams(seasonId, gameMode);
        params = params.set('months', months.toString());
        return this.http.get<ActivityDay[]>(`${this.API_URL}/activity`, { params });
    }

    // Pobiera najlepsze duo
    // GET /api/stats/best-duos
    getBestDuos(role?: string, seasonId?: number, gameMode?: string): Observable<DuoSummary> {
        let params = this.buildParams(seasonId, gameMode);
        if (role) {
            params = params.set('playerRole', role);
        }
        return this.http.get<DuoSummary>(`${this.API_URL}/best-duos`, { params });
    }

    // Pobiera pełne podsumowanie (wszystkie statystyki naraz)
    // GET /api/stats/summary
    getSummary(months: number = 6, seasonId?: number, gameMode?: string): Observable<StatsSummary> {
        let params = this.buildParams(seasonId, gameMode);
        params = params.set('months', months.toString());
        return this.http.get<StatsSummary>(`${this.API_URL}/summary`, { params });
    }

    // Pomocnicza metoda do budowania parametrów URL
    private buildParams(seasonId?: number, gameMode?: string): HttpParams {
        let params = new HttpParams();

        if (seasonId) {
            params = params.set('seasonId', seasonId.toString());
        }
        if (gameMode) {
            params = params.set('gameMode', gameMode);
        }

        return params;
    }
}
