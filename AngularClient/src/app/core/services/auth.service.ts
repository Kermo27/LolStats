import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginRequest, RegisterRequest, AuthResponse, UserInfo } from '../../models';
import { environment } from '../../../environments/environment';

// Serwis do obsługi autoryzacji
// Podobnie jak w C# używamy Dependency Injection (HttpClient)
@Injectable({
    providedIn: 'root' // "Singleton" - jedna instancja dla całej aplikacji
})
export class AuthService {
    // Adres API - w produkcji byłby w environment.ts
    private readonly API_URL = `${environment.apiUrl}/auth`;

    // BehaviorSubject przechowuje aktualny stan użytkownika
    // Każdy komponent może "subskrybować" zmiany
    private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);

    // Observable pozwala komponentom nasłuchiwać zmian użytkownika
    currentUser$ = this.currentUserSubject.asObservable();

    constructor(private http: HttpClient) {
        // Przy starcie sprawdź czy użytkownik był zalogowany
        this.loadUserFromStorage();
    }

    // Logowanie - wysyła dane do API i zapisuje token
    login(credentials: LoginRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.API_URL}/login`, credentials)
            .pipe(
                // tap() wykonuje "efekt uboczny" - zapisuje dane bez zmiany odpowiedzi
                tap(response => this.handleAuthSuccess(response))
            );
    }

    // Rejestracja nowego użytkownika
    register(data: RegisterRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.API_URL}/register`, data)
            .pipe(
                tap(response => this.handleAuthSuccess(response))
            );
    }

    // Wylogowanie
    logout(): void {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('user');
        this.currentUserSubject.next(null);
    }

    // Pobiera token JWT (używany przez interceptor)
    getAccessToken(): string | null {
        return localStorage.getItem('access_token');
    }

    // Sprawdza czy użytkownik jest zalogowany
    isLoggedIn(): boolean {
        return !!this.getAccessToken();
    }

    // Pomocnicza metoda - obsługuje sukces logowania/rejestracji
    private handleAuthSuccess(response: AuthResponse): void {
        localStorage.setItem('access_token', response.accessToken);
        localStorage.setItem('refresh_token', response.refreshToken);
        localStorage.setItem('user', JSON.stringify(response.user));
        this.currentUserSubject.next(response.user);
    }

    // Wczytuje dane użytkownika z localStorage przy starcie
    private loadUserFromStorage(): void {
        const userJson = localStorage.getItem('user');
        if (userJson) {
            try {
                const user = JSON.parse(userJson) as UserInfo;
                this.currentUserSubject.next(user);
            } catch {
                // Jeśli JSON jest niepoprawny, wyczyść dane
                this.logout();
            }
        }
    }
}
