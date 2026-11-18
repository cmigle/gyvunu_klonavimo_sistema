using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.Services;

public record DnrNorma(string Pavadinimas, string Vienetai, double? Min, double? Maks, Func<DnrPavyzdys, double?> Reiksme)
{
    public bool Atitinka(double verte) => (Min is null || verte >= Min) && (Maks is null || verte <= Maks);
}

public record ParametroPatikrinimas(DnrNorma Norma, double Verte)
{
    public bool Atitinka => Norma.Atitinka(Verte);
}

public record TinkamumoRezultatas(IReadOnlyList<string> Truksta, IReadOnlyList<ParametroPatikrinimas> Patikrinimai)
{
    public bool GalimaNustatyti => Truksta.Count == 0;
    public IReadOnlyList<ParametroPatikrinimas> Neatitikimai => Patikrinimai.Where(p => !p.Atitinka).ToList();
    public string? Statusas => !GalimaNustatyti ? null : Neatitikimai.Count == 0 ? Tinkamumai.Tinkamas : Tinkamumai.Netinkamas;
}

// PA „Nustatyti DNR pavyzdžio tinkamumą“ (2.20 pav.): kiekvienas biologinis parametras
// tikrinamas pagal normą, suskaičiuojami neatitikimai ir priskiriamas statusas
public static class DnrTinkamumoAnalize
{
    public static readonly IReadOnlyList<DnrNorma> Normos =
    [
        new("Saugojimo temperatūra", "°C", null, -20, p => p.SaugojimoTemperatura),
        new("DNR koncentracija", "ng/µl", 50, null, p => p.DnrKoncentracija),
        new("Tirpalo tūris", "ml", 0.02, null, p => p.TirpaloTuris),
        new("Baltymų tarša (A260/280)", "", 1.8, 2.0, p => p.BaltymuTarsa),
        new("DIN vertė", "", 7, null, p => p.DinVerte),
        new("Ląstelių gyvybingumas", "%", 80, null, p => p.LasteliuGyvybingumas)
    ];

    public static TinkamumoRezultatas Analizuoti(DnrPavyzdys pavyzdys)
    {
        var truksta = Normos.Where(n => n.Reiksme(pavyzdys) is null).Select(n => n.Pavadinimas).ToList();
        if (truksta.Count > 0) return new(truksta, []);
        var patikrinimai = Normos.Select(n => new ParametroPatikrinimas(n, n.Reiksme(pavyzdys)!.Value)).ToList();
        return new([], patikrinimai);
    }
}
