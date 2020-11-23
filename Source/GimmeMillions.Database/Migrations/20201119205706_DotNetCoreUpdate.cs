using Microsoft.EntityFrameworkCore.Migrations;

namespace GimmeMillions.SQLDataAccess.Migrations
{
    public partial class DotNetCoreUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "Period",
            //    table: "StockHistories",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Period",
            //    table: "StockHistories");
        }
    }
}
