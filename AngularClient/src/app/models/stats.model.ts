// Modele statystyk - odpowiedniki DTOs z C#
// Odwzorowują Shared/DTOs/StatsDtos.cs

// Odpowiednik OverviewDto
export interface Overview {
    winrate: number;
    mostPlayedChampion: string;
    mostPlayedChampionGames: number;
    favoriteSupport: string;
    favoriteSupportGames: number;
}

// Odpowiednik ChampionStatsDto
export interface ChampionStats {
    championName: string;
    role: string;
    games: number;
    wins: number;
    losses: number;
    winrate: number;
    avgKda: number;
    avgCsm: number;
    avgVisionScore: number;
}

// Odpowiednik EnemyStatsDto
export interface EnemyStats {
    championName: string;
    games: number;
    winrateAgainst: number;
}

// Odpowiednik ActivityDayDto
export interface ActivityDay {
    date: string;  // DateOnly w C# -> string w TS
    gamesPlayed: number;
}

// Odpowiednik DuoSummary
export interface DuoSummary {
    champion: string;
    support: string;
    count: number;
    winRate: number;
    avgKda: number;
}

// Odpowiednik EnchanterUsageSummary
export interface EnchanterUsage {
    myEnchanterGames: number;
    myPercentage: number;
    enemyEnchanterGames: number;
    enemyPercentage: number;
    myTopEnchanter: string;
    enemyTopEnchanter: string;
}

// Odpowiednik StreakDto
export interface Streak {
    currentStreak: number;
    isWinStreak: boolean;
    longestWinStreak: number;
    longestLossStreak: number;
}

// Pełne podsumowanie statystyk
export interface StatsSummary {
    overview: Overview;
    championStats: ChampionStats[];
    enemyBotStats: EnemyStats[];
    enemySupportStats: EnemyStats[];
    activity: ActivityDay[];
    enchanterUsage: EnchanterUsage;
    bestDuos: DuoSummary[];
    worstEnemyDuos: DuoSummary[];
    streak: Streak;
}
