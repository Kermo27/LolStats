// Model sezonu - odpowiednik Season z C#
export interface Season {
    id: number;
    name: string;
    startDate: Date;
    endDate?: Date;
}
