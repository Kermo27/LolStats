using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    CustomName = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    RiotPuuid = table.Column<string>(type: "text", nullable: true),
                    ProfileIconId = table.Column<int>(type: "integer", nullable: true),
                    SoloTier = table.Column<string>(type: "text", nullable: true),
                    SoloRank = table.Column<string>(type: "text", nullable: true),
                    SoloLP = table.Column<int>(type: "integer", nullable: true),
                    FlexTier = table.Column<string>(type: "text", nullable: true),
                    FlexRank = table.Column<string>(type: "text", nullable: true),
                    FlexLP = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<long>(type: "bigint", nullable: true),
                    Champion = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    LaneAlly = table.Column<string>(type: "text", nullable: false),
                    LaneEnemy = table.Column<string>(type: "text", nullable: false),
                    LaneEnemyAlly = table.Column<string>(type: "text", nullable: false),
                    Kills = table.Column<int>(type: "integer", nullable: false),
                    Deaths = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    Cs = table.Column<int>(type: "integer", nullable: false),
                    GameLengthMinutes = table.Column<int>(type: "integer", nullable: false),
                    Win = table.Column<bool>(type: "boolean", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentTier = table.Column<string>(type: "text", nullable: false),
                    CurrentDivision = table.Column<int>(type: "integer", nullable: false),
                    CurrentLp = table.Column<int>(type: "integer", nullable: false),
                    GameMode = table.Column<string>(type: "text", nullable: false),
                    QueueId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalDamageDealt = table.Column<int>(type: "integer", nullable: true),
                    DamageDealtToChampions = table.Column<int>(type: "integer", nullable: true),
                    PhysicalDamageDealt = table.Column<int>(type: "integer", nullable: true),
                    PhysicalDamageToChampions = table.Column<int>(type: "integer", nullable: true),
                    MagicDamageDealt = table.Column<int>(type: "integer", nullable: true),
                    MagicDamageToChampions = table.Column<int>(type: "integer", nullable: true),
                    TrueDamageDealt = table.Column<int>(type: "integer", nullable: true),
                    TrueDamageToChampions = table.Column<int>(type: "integer", nullable: true),
                    DamageToBuildings = table.Column<int>(type: "integer", nullable: true),
                    DamageToObjectives = table.Column<int>(type: "integer", nullable: true),
                    DamageToTurrets = table.Column<int>(type: "integer", nullable: true),
                    TotalDamageTaken = table.Column<int>(type: "integer", nullable: true),
                    PhysicalDamageTaken = table.Column<int>(type: "integer", nullable: true),
                    MagicDamageTaken = table.Column<int>(type: "integer", nullable: true),
                    TrueDamageTaken = table.Column<int>(type: "integer", nullable: true),
                    DamageSelfMitigated = table.Column<int>(type: "integer", nullable: true),
                    GoldEarned = table.Column<int>(type: "integer", nullable: true),
                    GoldSpent = table.Column<int>(type: "integer", nullable: true),
                    DoubleKills = table.Column<int>(type: "integer", nullable: true),
                    TripleKills = table.Column<int>(type: "integer", nullable: true),
                    QuadraKills = table.Column<int>(type: "integer", nullable: true),
                    PentaKills = table.Column<int>(type: "integer", nullable: true),
                    LargestKillingSpree = table.Column<int>(type: "integer", nullable: true),
                    LargestMultiKill = table.Column<int>(type: "integer", nullable: true),
                    KillingSprees = table.Column<int>(type: "integer", nullable: true),
                    TurretsKilled = table.Column<int>(type: "integer", nullable: true),
                    InhibitorsKilled = table.Column<int>(type: "integer", nullable: true),
                    FirstBloodKill = table.Column<bool>(type: "boolean", nullable: true),
                    FirstBloodAssist = table.Column<bool>(type: "boolean", nullable: true),
                    VisionScore = table.Column<int>(type: "integer", nullable: true),
                    WardsPlaced = table.Column<int>(type: "integer", nullable: true),
                    WardsKilled = table.Column<int>(type: "integer", nullable: true),
                    VisionWardsBought = table.Column<int>(type: "integer", nullable: true),
                    TotalHeal = table.Column<int>(type: "integer", nullable: true),
                    HealOnTeammates = table.Column<int>(type: "integer", nullable: true),
                    UnitsHealed = table.Column<int>(type: "integer", nullable: true),
                    DamageShieldedOnTeammates = table.Column<int>(type: "integer", nullable: true),
                    TotalTimeCCDealt = table.Column<int>(type: "integer", nullable: true),
                    TimeCCingOthers = table.Column<int>(type: "integer", nullable: true),
                    TimeSpentDead = table.Column<int>(type: "integer", nullable: true),
                    LongestTimeSpentLiving = table.Column<int>(type: "integer", nullable: true),
                    LargestCriticalStrike = table.Column<int>(type: "integer", nullable: true),
                    CombatPlayerScore = table.Column<int>(type: "integer", nullable: true),
                    TotalPlayerScore = table.Column<int>(type: "integer", nullable: true),
                    ChampionLevel = table.Column<int>(type: "integer", nullable: true),
                    Spell1Id = table.Column<int>(type: "integer", nullable: true),
                    Spell2Id = table.Column<int>(type: "integer", nullable: true),
                    Spell1Casts = table.Column<int>(type: "integer", nullable: true),
                    Spell2Casts = table.Column<int>(type: "integer", nullable: true),
                    Spell3Casts = table.Column<int>(type: "integer", nullable: true),
                    Spell4Casts = table.Column<int>(type: "integer", nullable: true),
                    ItemsBuild = table.Column<string>(type: "text", nullable: true),
                    PerkPrimaryStyle = table.Column<int>(type: "integer", nullable: true),
                    PerkSubStyle = table.Column<int>(type: "integer", nullable: true),
                    Perks = table.Column<string>(type: "text", nullable: true),
                    Augment1 = table.Column<int>(type: "integer", nullable: true),
                    Augment2 = table.Column<int>(type: "integer", nullable: true),
                    Augment3 = table.Column<int>(type: "integer", nullable: true),
                    Augment4 = table.Column<int>(type: "integer", nullable: true),
                    GameEndedInSurrender = table.Column<bool>(type: "boolean", nullable: true),
                    GameEndedInEarlySurrender = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_UserProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Seasons",
                columns: new[] { "Id", "CustomName", "EndDate", "Number", "StartDate" },
                values: new object[] { 1, null, new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), 15, new DateTime(2025, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Date",
                table: "Matches",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ProfileId",
                table: "Matches",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ProfileId_Champion",
                table: "Matches",
                columns: new[] { "ProfileId", "Champion" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ProfileId_Date",
                table: "Matches",
                columns: new[] { "ProfileId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ProfileId_Date_GameMode",
                table: "Matches",
                columns: new[] { "ProfileId", "Date", "GameMode" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ProfileId_Role",
                table: "Matches",
                columns: new[] { "ProfileId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
