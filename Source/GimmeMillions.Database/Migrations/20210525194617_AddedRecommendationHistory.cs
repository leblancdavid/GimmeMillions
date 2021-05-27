using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GimmeMillions.Database.Migrations
{
    public partial class AddedRecommendationHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureVectors");

            migrationBuilder.DropTable(
                name: "StockHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockRecommendations",
                table: "StockRecommendations");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "StockRecommendations");

            migrationBuilder.RenameTable(
                name: "StockRecommendations",
                newName: "LastRecommendations");

            migrationBuilder.RenameColumn(
                name: "PreviousClose",
                table: "LastRecommendations",
                newName: "DateUpdated");

            migrationBuilder.AddColumn<int>(
                name: "LastDataId",
                table: "LastRecommendations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LastRecommendations",
                table: "LastRecommendations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "RecommendationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SystemId = table.Column<string>(type: "TEXT", nullable: true),
                    Symbol = table.Column<string>(type: "TEXT", nullable: true),
                    HistoricalDataStr = table.Column<string>(type: "TEXT", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Symbol = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Period = table.Column<int>(type: "INTEGER", nullable: false),
                    Open = table.Column<decimal>(type: "TEXT", nullable: false),
                    High = table.Column<decimal>(type: "TEXT", nullable: false),
                    Low = table.Column<decimal>(type: "TEXT", nullable: false),
                    Close = table.Column<decimal>(type: "TEXT", nullable: false),
                    AdjustedClose = table.Column<decimal>(type: "TEXT", nullable: false),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: false),
                    AveragePercentPeriodRange = table.Column<decimal>(type: "TEXT", nullable: false),
                    PreviousClose = table.Column<decimal>(type: "TEXT", nullable: false),
                    Signal = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LastRecommendations_LastDataId",
                table: "LastRecommendations",
                column: "LastDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_LastRecommendations_StockData_LastDataId",
                table: "LastRecommendations",
                column: "LastDataId",
                principalTable: "StockData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LastRecommendations_StockData_LastDataId",
                table: "LastRecommendations");

            migrationBuilder.DropTable(
                name: "RecommendationHistories");

            migrationBuilder.DropTable(
                name: "StockData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LastRecommendations",
                table: "LastRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_LastRecommendations_LastDataId",
                table: "LastRecommendations");

            migrationBuilder.DropColumn(
                name: "LastDataId",
                table: "LastRecommendations");

            migrationBuilder.RenameTable(
                name: "LastRecommendations",
                newName: "StockRecommendations");

            migrationBuilder.RenameColumn(
                name: "DateUpdated",
                table: "StockRecommendations",
                newName: "PreviousClose");

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "StockRecommendations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockRecommendations",
                table: "StockRecommendations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FeatureVectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataStr = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Encoding = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureVectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HistoricalDataStr = table.Column<string>(type: "TEXT", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StockPeriod = table.Column<int>(type: "INTEGER", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockHistories", x => x.Id);
                });
        }
    }
}
