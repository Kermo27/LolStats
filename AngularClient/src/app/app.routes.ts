import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login.component')
            .then(m => m.LoginComponent)
    },

    {
        path: '',
        loadComponent: () => import('./pages/dashboard/dashboard.component')
            .then(m => m.DashboardComponent),
        canActivate: [authGuard]
    },

    {
        path: 'matches',
        loadComponent: () => import('./pages/matches/matches.component')
            .then(m => m.MatchesComponent),
        canActivate: [authGuard]
    },

    {
        path: 'champions',
        loadComponent: () => import('./pages/champions/champions.component')
            .then(m => m.ChampionsComponent),
        canActivate: [authGuard]
    },

    {
        path: 'register',
        loadComponent: () => import('./pages/register/register.component')
            .then(m => m.RegisterComponent)
    },

    {
        path: 'history',
        loadComponent: () => import('./pages/history/history.component')
            .then(m => m.HistoryComponent),
        canActivate: [authGuard]
    },

    {
        path: '**',
        redirectTo: ''
    }
];

