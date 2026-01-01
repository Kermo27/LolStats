using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRankMilestones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RankMilestones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RankMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AchievedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Division = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tier = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankMilestones_UserProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RankMilestones_ProfileId",
                table: "RankMilestones",
                column: "ProfileId");
        }
    }
}
