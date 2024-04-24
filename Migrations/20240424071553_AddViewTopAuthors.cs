using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    /// <inheritdoc />
    public partial class AddViewTopAuthors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE VIEW View_TopAuthors AS SELECT u.FullName as fullname, count(*) as WorkItemsCreated 
                FROM myboards.Users as u 
                JOIN WorkItems as wi on wi.AuthorId = u.Id GROUP BY u.Id, u.FullName
                ORDER BY WorkItemsCreated DESC LIMIT 5"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { 
            migrationBuilder.Sql(
                @"DROP VIEW View_TopAuthors"
            );

        }
    }
}
