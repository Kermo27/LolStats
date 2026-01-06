import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile } from '../../models';

// Serwis do pobierania profili użytkownika
@Injectable({
    providedIn: 'root'
})
export class ProfileService {
    private readonly API_URL = 'http://localhost:5031/api/profiles';

    constructor(private http: HttpClient) { }

    // Pobiera wszystkie profile użytkownika
    // GET /api/profiles
    getAll(): Observable<UserProfile[]> {
        return this.http.get<UserProfile[]>(this.API_URL);
    }

    // Pobiera pojedynczy profil
    getById(id: string): Observable<UserProfile> {
        return this.http.get<UserProfile>(`${this.API_URL}/${id}`);
    }
}
