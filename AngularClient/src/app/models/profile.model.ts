// Model profilu użytkownika - odpowiednik UserProfile z C#
export interface UserProfile {
    id: string;
    name: string;
    tag?: string;
    userId: string;
    profileIconId?: number;
    soloTier?: string;
    soloRank?: string;
    soloLP?: number;
}
