using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolStatsTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChampionPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChampionPools");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChampionPools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Champion = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tier = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionPools_UserProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionPools_ProfileId",
                table: "ChampionPools",
                column: "ProfileId");
        }
    }
}
