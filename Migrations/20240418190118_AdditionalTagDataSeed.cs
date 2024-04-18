using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalTagDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "Tags", column: "Value", value: "Web");
            migrationBuilder.InsertData(table: "Tags", column: "Value", value: "UI");
            migrationBuilder.InsertData(table: "Tags", column: "Value", value: "Desktop");
            migrationBuilder.InsertData(table: "Tags", column: "Value", value: "API");
            migrationBuilder.InsertData(table: "Tags", column: "Value", value: "Service");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Tags", keyColumn: "Value", keyValue: "Web");
            migrationBuilder.DeleteData(table: "Tags", keyColumn: "Value", keyValue: "UI");
            migrationBuilder.DeleteData(table: "Tags", keyColumn: "Value", keyValue: "Desktop");
            migrationBuilder.DeleteData(table: "Tags", keyColumn: "Value", keyValue: "API");
            migrationBuilder.DeleteData(table: "Tags", keyColumn: "Value", keyValue: "Service");
        }
    }
}
