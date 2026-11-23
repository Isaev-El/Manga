using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_active_readers",
                columns: table => new
                {
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_active_readers", x => new { x.Day, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "daily_reading_stats",
                columns: table => new
                {
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    ChaptersRead = table.Column<int>(type: "integer", nullable: false),
                    ActiveReaders = table.Column<int>(type: "integer", nullable: false),
                    TitlesAdded = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_reading_stats", x => x.Day);
                });

            migrationBuilder.CreateTable(
                name: "manga_completed_readers",
                columns: table => new
                {
                    MangaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manga_completed_readers", x => new { x.MangaId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "manga_reading_stats",
                columns: table => new
                {
                    MangaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChaptersRead = table.Column<int>(type: "integer", nullable: false),
                    InLists = table.Column<int>(type: "integer", nullable: false),
                    Completed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manga_reading_stats", x => x.MangaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_events_OccurredOnUtc",
                table: "events",
                column: "OccurredOnUtc")
                .Annotation("Npgsql:IndexMethod", "brin");

            migrationBuilder.CreateIndex(
                name: "IX_manga_reading_stats_ChaptersRead",
                table: "manga_reading_stats",
                column: "ChaptersRead");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_active_readers");

            migrationBuilder.DropTable(
                name: "daily_reading_stats");

            migrationBuilder.DropTable(
                name: "manga_completed_readers");

            migrationBuilder.DropTable(
                name: "manga_reading_stats");

            migrationBuilder.DropIndex(
                name: "IX_events_OccurredOnUtc",
                table: "events");
        }
    }
}
