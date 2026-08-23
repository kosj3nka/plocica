using Microsoft.EntityFrameworkCore;
using Plocica.Models;

namespace Plocica.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext db)
    {
        db.Database.Migrate();

        if (!db.Shapes.Any())
        {
            db.Shapes.AddRange(GetShapes());
        }

        if (!db.Colors.Any())
        {
            db.Colors.AddRange(GetColors());
        }

        db.SaveChanges();
    }

    private static IEnumerable<Shape> GetShapes()
    {
        const string obliciOtherInfo = "Zbog procesa ručne izrade moguće su manje varijacije u dimenzijama i nijansi.";
        const decimal thickness = 0.8m;

        return new List<Shape>
        {
            new()
            {
                Name = "Arabesque",
                Collection = "oblici",
                Thickness = thickness,
                DimensionHeight = 14.5m,
                DimensionWidth = 14.5m,
                AvailableColors = "Warm Sun, Aqua, Moss, Indigo Gold, Sour Cherry, Clay Rose",
                OtherInfo = obliciOtherInfo,
                Price = "300 € / m²",
                SortOrder = 1,
            },
            new()
            {
                Name = "Oval",
                Collection = "oblici",
                Thickness = thickness,
                DimensionHeight = 17.5m,
                DimensionWidth = 5.3m,
                AvailableColors = "White, Bisque, Beige, Bisque x Beige, Beige x Watergreen, Clay Rose x Moss",
                OtherInfo = obliciOtherInfo,
                Price = "315 € / m² jednobojne\n325 € / m² kombinacije dvije boje\nMoguća usluga ljepljenja na mrežice 30 € / m²",
                SortOrder = 2,
            },
            new()
            {
                Name = "Fish scale",
                Collection = "oblici",
                Thickness = thickness,
                DimensionHeight = 13m,
                DimensionWidth = 14.5m,
                AvailableColors = "Deep Green, Honey, White, Deep Green x Beige, Clay Rose x Cherry, Cherry x Beige, Aquamarine x Watergreen x Petrolej, Rose x Beige x Green, Cobalt x Sky blue x Bluegrey",
                OtherInfo = obliciOtherInfo,
                Price = "310 € / m² jednobojne\n325 € / m² kombinacije dvije boje\n340 € / m² kombinacije tri boje\nMoguća usluga ljepljenja na mrežice 30 € / m²",
                SortOrder = 3,
            },
            new()
            {
                Name = "Linea",
                Collection = "oblici",
                Thickness = thickness,
                DimensionHeight = 15.5m,
                DimensionWidth = 0.8m,
                AvailableColors = "Terra, Honey, White, Bisque, Warm Yellow, Deep Green",
                OtherInfo = obliciOtherInfo,
                Price = "340 € / m² jednobojne\nUsluga ljepljenja na mrežice uključena je u cijenu",
                SortOrder = 4,
            },
            new()
            {
                Name = "Wave",
                Collection = "oslikane",
                Thickness = thickness,
                DimensionHeight = 15.5m,
                DimensionWidth = 15.5m,
                AvailableColors = "Black x White, White x Red matte, Blue x White matte",
                OtherInfo = obliciOtherInfo,
                Price = "430 € / m² kombinacija dvije boje\n460 € / m² kombinacije tri do četiri boje\n500 € / m² kombinacije pet do šest boja",
                SortOrder = 5,
            },
            new()
            {
                Name = "Module",
                Collection = "oslikane",
                Thickness = thickness,
                DimensionHeight = 15.5m,
                DimensionWidth = 15.5m,
                AvailableColors = "White x Red, Blue sky x Brown, White x Cobalt blue",
                OtherInfo = obliciOtherInfo,
                Price = "430 € / m² kombinacija dvije boje",
                SortOrder = 6,
            },
            new()
            {
                Name = "Curve",
                Collection = "oslikane",
                Thickness = thickness,
                DimensionHeight = 20m,
                DimensionWidth = 10m,
                AvailableColors = "vidi Karta boja",
                OtherInfo = obliciOtherInfo,
                Price = "470 € / m² kombinacija dvije boje + dva motiva",
                SortOrder = 7,
            },
            new()
            {
                Name = "Reljefne iz kalupa",
                Collection = "reljefne",
                OtherInfo = "Reljefne pločice izrađuju se iz posebno razvijenih kalupa, dok ručna završna obrada i custom razvijene glazure naglašavaju njihovu dubinu, teksturu i karakter ručnog rada.",
                Price = "Kreće od 500 € / m²",
                SortOrder = 8,
            },
            new()
            {
                Name = "Ručno rezbarene",
                Collection = "reljefne",
                OtherInfo = "Svaka pločica ručno se rezbari prije prvog pečenja, stvarajući reljef kroz koji se glazure prirodno prelijevaju i zadržavaju u teksturi. Glazure razvijene s oksidima dodatno naglašavaju sjaj, slojevitost i karakter svake pločice.",
                Price = "Kreće od 500 € / m²",
                SortOrder = 9,
            },
        };
    }

    private static IEnumerable<ColorItem> GetColors()
    {
        for (var i = 1; i <= 28; i++)
        {
            yield return new ColorItem
            {
                Code = i.ToString("000"),
                SortOrder = i,
            };
        }
    }

}
