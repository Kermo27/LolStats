import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

// Konfiguracja tras - odpowiednik @page w Blazor
// Każda trasa mapuje ścieżkę URL do komponentu
export const routes: Routes = [
    // Strona logowania - dostępna dla wszystkich
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login.component')
            .then(m => m.LoginComponent)
    },

    // Dashboard - chroniony (wymaga zalogowania)
    {
        path: '',
        loadComponent: () => import('./pages/dashboard/dashboard.component')
            .then(m => m.DashboardComponent),
        canActivate: [authGuard]
    },

    // Matches - lista meczy z paginacją
    {
        path: 'matches',
        loadComponent: () => import('./pages/matches/matches.component')
            .then(m => m.MatchesComponent),
        canActivate: [authGuard]
    },

    // History - stara strona (do usunięcia w przyszłości)
    {
        path: 'history',
        loadComponent: () => import('./pages/history/history.component')
            .then(m => m.HistoryComponent),
        canActivate: [authGuard]
    },

    // Przekierowanie nieznanych ścieżek na dashboard
    {
        path: '**',
        redirectTo: ''
    }
];

