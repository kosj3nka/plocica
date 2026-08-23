using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plocica.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeededDemoProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [Projects] WHERE [Title] IN (N'Kuća Heinzel', N'Crkva sv. Mirka');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
