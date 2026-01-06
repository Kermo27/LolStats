import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

// Konfiguracja aplikacji Angular
// Podobna do ConfigureServices w ASP.NET Core
export const appConfig: ApplicationConfig = {
  providers: [
    // Włącza routing (nawigację między stronami)
    provideRouter(routes),

    // Włącza HttpClient z interceptorem autoryzacji
    // Interceptor automatycznie dodaje token do każdego requestu
    provideHttpClient(
      withInterceptors([authInterceptor])
    )
  ]
};
