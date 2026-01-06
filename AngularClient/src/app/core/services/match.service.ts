
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Match, PaginatedResponse } from '../../models';
import { environment } from '../../../environments/environment';

// Serwis do pobierania danych o meczach z API
// Odpowiednik serwisu Blazor MatchService
@Injectable({
    providedIn: 'root'
})
export class MatchService {
    private readonly API_URL = `${environment.apiUrl}/matches`;

    constructor(private http: HttpClient) { }

    // Pobiera wszystkie mecze
    // GET /api/matches
    getAll(): Observable<Match[]> {
        return this.http.get<Match[]>(this.API_URL);
    }

    // Pobiera ostatnie mecze
    // GET /api/matches/recent?count=20
    getRecent(count: number = 20, gameMode?: string): Observable<Match[]> {
        let params = new HttpParams().set('count', count.toString());

        if (gameMode) {
            params = params.set('gameMode', gameMode);
        }

        return this.http.get<Match[]>(`${this.API_URL}/recent`, { params });
    }

    // Pobiera mecze z paginacją
    // GET /api/matches/paginated?page=1&pageSize=20
    getPaginated(
        page: number = 1,
        pageSize: number = 20,
        gameMode?: string
    ): Observable<PaginatedResponse<Match>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (gameMode) {
            params = params.set('gameMode', gameMode);
        }

        return this.http.get<PaginatedResponse<Match>>(`${this.API_URL}/paginated`, { params });
    }

    // Pobiera pojedynczy mecz po ID
    // GET /api/matches/{id}
    getById(id: string): Observable<Match> {
        return this.http.get<Match>(`${this.API_URL}/${id}`);
    }
}
