// Model meczu - odpowiednik MatchEntry z C#
// Zawiera wszystkie pola z Shared/Models/MatchEntry.cs

export interface Match {
    id: string;
    gameId?: number;

    // Podstawowe informacje o grze
    champion: string;
    role: string;
    laneAlly: string;
    laneEnemy: string;
    laneEnemyAlly: string;

    // Statystyki walki (K/D/A)
    kills: number;
    deaths: number;
    assists: number;
    cs: number;
    gameLengthMinutes: number;
    win: boolean;
    date: Date;

    // Informacje o rankingu
    currentTier: string;
    currentDivision: number;
    currentLp: number;
    gameMode: string;
    queueId: number;
    notes?: string;
    profileId?: string;

    // Rozszerzone statystyki (opcjonalne)
    damageDealtToChampions?: number;
    goldEarned?: number;
    visionScore?: number;
    wardsPlaced?: number;

    // Obliczane pola (computed)
    kdaDisplay?: string;
    performanceScore?: number;
    performanceRating?: string;
}

// Odpowiedź z paginacją
export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}
