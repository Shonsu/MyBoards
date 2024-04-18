using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    /// <inheritdoc />
    public partial class AdditionMorkItemStateSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "WorkItemStates", column: "State", value: "On Hold");
            migrationBuilder.InsertData(
                table: "WorkItemStates",
                column: "State",
                value: "Rejected"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "WorkItemStates",
                keyColumn: "State",
                keyValue: "On Hold"
            );
            migrationBuilder.DeleteData(
                table: "WorkItemStates",
                keyColumn: "State",
                keyValue: "Rejected"
            );
        }
    }
}
