import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Guard to "strażnik" trasy - sprawdza czy użytkownik ma dostęp
// Odpowiednik [Authorize] z ASP.NET Core
// Jeśli użytkownik nie jest zalogowany, przekieruj na /login
export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isLoggedIn()) {
        return true; // Pozwól na dostęp
    }

    // Przekieruj na stronę logowania
    router.navigate(['/login']);
    return false;
};
