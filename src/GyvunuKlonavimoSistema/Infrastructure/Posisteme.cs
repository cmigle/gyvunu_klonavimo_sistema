namespace GyvunuKlonavimoSistema.Infrastructure;

// Lango antraštėje rodoma posistemė; spalva – pagal komandos spalvinę legendą
public enum Posisteme
{
    Sistema,
    KlientaiIrAugintiniai,
    Uzsakymai,
    DnrPavyzdziai,
    Inkubatoriai,
    Darbuotojai
}

public static class Posistemes
{
    public static string Pavadinimas(Posisteme p) => p switch
    {
        Posisteme.KlientaiIrAugintiniai => "Kliento ir augintinio valdymas",
        Posisteme.Uzsakymai => "Užsakymo valdymas",
        Posisteme.DnrPavyzdziai => "DNR pavyzdžio valdymas",
        Posisteme.Inkubatoriai => "Inkubatorių valdymas",
        Posisteme.Darbuotojai => "Darbuotojų valdymas",
        _ => "Gyvūnų klonavimo sistema"
    };

    public static string Spalva(Posisteme p) => p switch
    {
        Posisteme.KlientaiIrAugintiniai => "var(--migle)",
        Posisteme.Uzsakymai => "var(--vytaute)",
        Posisteme.DnrPavyzdziai => "var(--evelina)",
        Posisteme.Inkubatoriai => "var(--pijus)",
        Posisteme.Darbuotojai => "var(--ugne)",
        _ => "var(--sistema)"
    };
}

// Naršymo kelio dalis; paskutinė (be nuorodos) – dabartinis langas
public record KelioDalis(string Pavadinimas, string? Nuoroda = null);
