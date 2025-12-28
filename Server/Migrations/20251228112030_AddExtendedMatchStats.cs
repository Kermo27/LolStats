using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedMatchStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Augment1",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Augment2",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Augment3",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Augment4",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChampionLevel",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CombatPlayerScore",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageDealtToChampions",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageSelfMitigated",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageShieldedOnTeammates",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageToBuildings",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageToObjectives",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageToTurrets",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoubleKills",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FirstBloodAssist",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FirstBloodKill",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GameEndedInEarlySurrender",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GameEndedInSurrender",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GameId",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GoldEarned",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GoldSpent",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealOnTeammates",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InhibitorsKilled",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemsBuild",
                table: "Matches",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KillingSprees",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LargestCriticalStrike",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LargestKillingSpree",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LargestMultiKill",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LongestTimeSpentLiving",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MagicDamageDealt",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MagicDamageTaken",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MagicDamageToChampions",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PentaKills",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerkPrimaryStyle",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerkSubStyle",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Perks",
                table: "Matches",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhysicalDamageDealt",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhysicalDamageTaken",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhysicalDamageToChampions",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuadraKills",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Spell1Casts",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Spell1Id",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Spell2Casts",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Spell2Id",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeCCingOthers",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeSpentDead",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalDamageDealt",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalDamageTaken",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalHeal",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPlayerScore",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalTimeCCDealt",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripleKills",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrueDamageDealt",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrueDamageTaken",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrueDamageToChampions",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurretsKilled",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitsHealed",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisionScore",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisionWardsBought",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WardsKilled",
                table: "Matches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WardsPlaced",
                table: "Matches",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Augment1",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Augment2",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Augment3",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Augment4",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ChampionLevel",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CombatPlayerScore",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DamageDealtToChampions",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DamageSelfMitigated",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DamageShieldedOnTeammates",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DamageToBuildings",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DamageToObjectives",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DamageToTurrets",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DoubleKills",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "FirstBloodAssist",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "FirstBloodKill",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "GameEndedInEarlySurrender",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "GameEndedInSurrender",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "GoldEarned",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "GoldSpent",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HealOnTeammates",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "InhibitorsKilled",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ItemsBuild",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "KillingSprees",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LargestCriticalStrike",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LargestKillingSpree",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LargestMultiKill",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LongestTimeSpentLiving",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MagicDamageDealt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MagicDamageTaken",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MagicDamageToChampions",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PentaKills",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PerkPrimaryStyle",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PerkSubStyle",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Perks",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PhysicalDamageDealt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PhysicalDamageTaken",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PhysicalDamageToChampions",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "QuadraKills",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Spell1Casts",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Spell1Id",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Spell2Casts",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Spell2Id",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TimeCCingOthers",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TimeSpentDead",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TotalDamageDealt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TotalDamageTaken",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TotalHeal",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TotalPlayerScore",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TotalTimeCCDealt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TripleKills",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TrueDamageDealt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TrueDamageTaken",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TrueDamageToChampions",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TurretsKilled",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "UnitsHealed",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "VisionScore",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "VisionWardsBought",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WardsKilled",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WardsPlaced",
                table: "Matches");
        }
    }
}
