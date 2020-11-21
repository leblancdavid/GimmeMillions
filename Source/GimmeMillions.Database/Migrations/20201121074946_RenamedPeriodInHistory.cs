using Microsoft.EntityFrameworkCore.Migrations;

namespace GimmeMillions.SQLDataAccess.Migrations
{
    public partial class RenamedPeriodInHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Period",
                table: "StockHistories",
                newName: "StockPeriod");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StockPeriod",
                table: "StockHistories",
                newName: "Period");
        }
    }
}
