import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

// Interceptor to funkcja przechwytująca KAŻDY request HTTP
// Odpowiednik DelegatingHandler w C# lub AuthorizationMessageHandler w Blazor
// Automatycznie dodaje token JWT do nagłówka Authorization
// oraz X-Profile-Id dla identyfikacji profilu
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.getAccessToken();
    const profileId = localStorage.getItem('profile_id');

    // Jeśli mamy token, dodaj nagłówki autoryzacji
    if (token) {
        // clone() tworzy kopię requestu z nowymi nagłówkami
        // W Angular requesty są niemutowalne (immutable)
        const headers: { [key: string]: string } = {
            'Authorization': `Bearer ${token}`
        };

        // Dodaj X-Profile-Id jeśli istnieje
        if (profileId) {
            headers['X-Profile-Id'] = profileId;
        }

        const clonedReq = req.clone({ setHeaders: headers });
        return next(clonedReq);
    }

    // Bez tokena - wyślij oryginalny request
    return next(req);
};

