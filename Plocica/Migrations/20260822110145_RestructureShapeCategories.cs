using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plocica.Migrations
{
    /// <inheritdoc />
    public partial class RestructureShapeCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Finish",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "LayoutScheme",
                table: "Shapes");

            migrationBuilder.CreateTable(
                name: "ShapeGalleryImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShapeId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShapeGalleryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShapeGalleryImages_Shapes_ShapeId",
                        column: x => x.ShapeId,
                        principalTable: "Shapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShapeGalleryImages_ShapeId",
                table: "ShapeGalleryImages",
                column: "ShapeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShapeGalleryImages");

            migrationBuilder.AddColumn<string>(
                name: "Finish",
                table: "Shapes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LayoutScheme",
                table: "Shapes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
