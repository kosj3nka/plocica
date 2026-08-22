using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plocica.Migrations
{
    /// <inheritdoc />
    public partial class SeedCustomProjectImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO [ProjectImages] ([ProjectId], [Url], [SortOrder])
SELECT [Id], v.[Url], v.[SortOrder]
FROM [Projects]
CROSS APPLY (VALUES
    (N'/img/radovi/heinzelStubiste.jpg', 0),
    (N'/img/radovi/heinzelBordura.jpg', 1),
    (N'/img/radovi/heinzelHodnik.jpg', 2)
) AS v([Url], [SortOrder])
WHERE [Title] = N'Kuća Heinzel';

INSERT INTO [ProjectImages] ([ProjectId], [Url], [SortOrder])
SELECT [Id], v.[Url], v.[SortOrder]
FROM [Projects]
CROSS APPLY (VALUES
    (N'/img/radovi/mirkaIzrada.jpg', 0),
    (N'/img/radovi/mirkaKalup.jpg', 1)
) AS v([Url], [SortOrder])
WHERE [Title] = N'Crkva sv. Mirka';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [ProjectImages] WHERE [Url] IN (
    N'/img/radovi/heinzelStubiste.jpg',
    N'/img/radovi/heinzelBordura.jpg',
    N'/img/radovi/heinzelHodnik.jpg',
    N'/img/radovi/mirkaIzrada.jpg',
    N'/img/radovi/mirkaKalup.jpg'
);
");
        }
    }
}
