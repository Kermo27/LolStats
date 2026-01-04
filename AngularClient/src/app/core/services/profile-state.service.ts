import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { UserProfile } from '../../models';
import { environment } from '../../../environments/environment';

/**
 * Serwis zarządzający stanem profili użytkownika
 * Odpowiednik IUserProfileState z Blazora
 * 
 * Używa BehaviorSubject do reaktywnego powiadamiania komponentów o zmianach
 */
@Injectable({
    providedIn: 'root'
})
export class ProfileStateService {
    private readonly API_URL = `${environment.apiUrl}/profiles`;

    // BehaviorSubject przechowuje aktualną wartość i emituje ją do nowych subskrybentów
    private allProfilesSubject = new BehaviorSubject<UserProfile[]>([]);
    private currentProfileSubject = new BehaviorSubject<UserProfile | null>(null);
    private isInitialized = false;

    // Publiczne Observable do subskrypcji w komponentach
    allProfiles$ = this.allProfilesSubject.asObservable();
    currentProfile$ = this.currentProfileSubject.asObservable();

    constructor(private http: HttpClient) { }

    // Getter do synchronicznego dostępu do aktualnego profilu
    get currentProfile(): UserProfile | null {
        return this.currentProfileSubject.value;
    }

    get allProfiles(): UserProfile[] {
        return this.allProfilesSubject.value;
    }

    /**
     * Inicjalizacja - pobiera profile i ustawia aktualny
     * Wywoływane przy starcie aplikacji lub po logowaniu
     */
    initialize(): Observable<UserProfile[]> {
        return this.http.get<UserProfile[]>(this.API_URL).pipe(
            tap(profiles => {
                this.allProfilesSubject.next(profiles);

                // Sprawdź czy mamy zapisany profil w localStorage
                const savedProfileId = localStorage.getItem('profile_id');
                let selectedProfile = profiles.find(p => p.id === savedProfileId);

                // Jeśli nie ma zapisanego lub nie istnieje, weź pierwszy
                if (!selectedProfile && profiles.length > 0) {
                    selectedProfile = profiles[0];
                }

                if (selectedProfile) {
                    this.setCurrentProfile(selectedProfile);
                }

                this.isInitialized = true;
            })
        );
    }

    /**
     * Ustawia aktualny profil
     * Zapisuje do localStorage i emituje zmianę do wszystkich subskrybentów
     */
    setCurrentProfile(profile: UserProfile): void {
        localStorage.setItem('profile_id', profile.id);
        this.currentProfileSubject.next(profile);
    }

    /**
     * Pobiera aktualne profile z API (refresh)
     */
    refreshProfiles(): Observable<UserProfile[]> {
        return this.initialize();
    }
}
