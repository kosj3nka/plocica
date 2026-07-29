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

        if (!db.Projects.Any())
        {
            db.Projects.AddRange(GetProjects());
        }

        db.SaveChanges();
    }

    private static IEnumerable<Shape> GetShapes()
    {
        const string obliciOtherInfo = "Zbog procesa ručne izrade moguće su manje varijacije u dimenzijama i nijansi.";
        const string thickness = "0,8 cm";
        const string finish = "mat i sjajno";

        return new List<Shape>
        {
            new()
            {
                Name = "Arabesque",
                Collection = "oblici",
                Thickness = thickness,
                Dimensions = "14,5 × 14,5 cm",
                Finish = finish,
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
                Dimensions = "17,5 × 5,3 cm",
                Finish = finish,
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
                Dimensions = "13 × 14,5 cm",
                Finish = finish,
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
                Dimensions = "15,5 × 0,8–1 cm",
                Finish = finish,
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
                Dimensions = "15,5 × 15,5 cm",
                Finish = finish,
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
                Dimensions = "15,5 × 15,5 cm",
                Finish = finish,
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
                Dimensions = "20 × 10 cm",
                Finish = finish,
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

    private static IEnumerable<Project> GetProjects()
    {
        return new List<Project>
        {
            new()
            {
                Title = "Kuća Heinzel",
                Location = "Zgrada Hrvatske izvještajne novinske agencije (HINA), Trg Marka Marulića 16, Zagreb",
                Text = "Rekonstrukcija originalnih pločica iz 1910. godine. Šest različitih tipova pločica, reljefne, jednobojne, oslikane te tri vrste bordura, zajedno tvore originalnu kompoziciju haustora. Pločice prikazane na fotografijama kombinacija su sačuvanih originala i naših ručno rekonstruiranih replika, izrađenih s ciljem očuvanja izvornog karaktera prostora.",
                SortOrder = 1,
            },
            new()
            {
                Title = "Crkva sv. Mirka",
                Location = "Šestinski vijenac 1, Zagreb",
                Text = "Rekonstrukcija originalnih keramičkih pločica uz precizno usklađivanje oblika, reljefa i boje. Projekt je uključivao izradu novih matrica i kalupa te brojna testiranja glazura kako bi se što vjernije rekonstruirali izvorni tonovi pločica.",
                SortOrder = 2,
            },
        };
    }
}
