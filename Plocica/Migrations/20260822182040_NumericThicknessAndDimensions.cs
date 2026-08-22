using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plocica.Migrations
{
    /// <inheritdoc />
    public partial class NumericThicknessAndDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dimensions",
                table: "Shapes");

            // Existing Thickness values are free text (e.g. "0,8 cm") and cannot be
            // cast directly to decimal — clear them first so the column-type change
            // below cannot fail against a database with prior data. Per plan: no
            // data preservation, values are re-entered via the admin form.
            migrationBuilder.Sql("UPDATE [Shapes] SET [Thickness] = NULL;");

            migrationBuilder.AlterColumn<decimal>(
                name: "Thickness",
                table: "Shapes",
                type: "decimal(6,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DimensionHeight",
                table: "Shapes",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DimensionWidth",
                table: "Shapes",
                type: "decimal(6,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DimensionHeight",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "DimensionWidth",
                table: "Shapes");

            migrationBuilder.AlterColumn<string>(
                name: "Thickness",
                table: "Shapes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "Shapes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
