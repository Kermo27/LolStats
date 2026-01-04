// Interfejsy odpowiadające C# DTOs z serwera
// Te modele TypeScript są odpowiednikami klas C# z Shared/DTOs

// === Autentykacja ===

// Odpowiednik LoginDto z C#
export interface LoginRequest {
    username: string;
    password: string;
}

// Odpowiednik RegisterDto z C#
export interface RegisterRequest {
    username: string;
    password: string;
    email?: string;
}

// Odpowiednik TokenResponseDto z C#
export interface AuthResponse {
    accessToken: string;
    refreshToken: string;
    expiresAt: Date;
    user: UserInfo;
}

// Odpowiednik UserInfoDto z C#
export interface UserInfo {
    id: string;
    username: string;
    email?: string;
}
