using Microsoft.EntityFrameworkCore.Migrations;

namespace GimmeMillions.SQLDataAccess.Migrations
{
    public partial class UpdateRecommendationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PredictedPriceTarget",
                table: "StockRecommendations");

            migrationBuilder.AddColumn<decimal>(
                name: "LowPrediction",
                table: "StockRecommendations",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Sentiment",
                table: "StockRecommendations",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LowPrediction",
                table: "StockRecommendations");

            migrationBuilder.DropColumn(
                name: "Sentiment",
                table: "StockRecommendations");

            migrationBuilder.AddColumn<decimal>(
                name: "PredictedPriceTarget",
                table: "StockRecommendations",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
