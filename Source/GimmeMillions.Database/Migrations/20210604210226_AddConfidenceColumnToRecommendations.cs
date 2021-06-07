using Microsoft.EntityFrameworkCore.Migrations;

namespace GimmeMillions.Database.Migrations
{
    public partial class AddConfidenceColumnToRecommendations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Confidence",
                table: "LastRecommendations",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "LastRecommendations");
        }
    }
}
