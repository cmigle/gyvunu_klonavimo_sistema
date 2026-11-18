using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.Services;

public record SpecialistoKruvis(LaboratorijosSpecialistas Specialistas, int AktyviuKiekis, int Trukme, double Kruvis)
{
    public string Lygis => DarboKruvioAnalize.Lygis(Kruvis);
}

// PA „Analizuoti darbuotojų krūvį“ (2.26 pav.)
public static class DarboKruvioAnalize
{
    public const string Mazas = "Mažas darbo krūvis";
    public const string Normalus = "Normalus darbo krūvis";
    public const string Didelis = "Didelis darbo krūvis";

    public static string Lygis(double procentai) => procentai < 70 ? Mazas : procentai <= 100 ? Normalus : Didelis;

    // Specialisto klonavimai turi būti įkelti (Include)
    public static SpecialistoKruvis Apskaiciuoti(LaboratorijosSpecialistas specialistas)
    {
        var aktyvus = specialistas.Klonavimai.Where(k => k.Busena).ToList();
        var trukme = aktyvus.Sum(k => k.TrukmeDienomis);
        var kruvis = specialistas.Etatas > 0 ? trukme / (specialistas.Etatas * KlonavimoNormos.KruvioNorma) * 100 : 0;
        return new(specialistas, aktyvus.Count, trukme, kruvis);
    }

    public static List<SpecialistoKruvis> Analizuoti(IEnumerable<LaboratorijosSpecialistas> specialistai) =>
        specialistai.Select(Apskaiciuoti).ToList();
}
