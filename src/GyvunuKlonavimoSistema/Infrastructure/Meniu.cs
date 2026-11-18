namespace GyvunuKlonavimoSistema.Infrastructure;

// Meniu punktas pažymimas, kai dabartinis langas priklauso jo skyriui (valdikliui)
public record MeniuPunktas(string Tekstas, string Nuoroda, string[] Valdikliai, string? Veiksmas = null)
{
    public bool Aktyvus(string valdiklis, string veiksmas) =>
        Valdikliai.Contains(valdiklis, StringComparer.OrdinalIgnoreCase)
        && (Veiksmas is null ? !veiksmas.StartsWith("Kruvio") : veiksmas.StartsWith(Veiksmas));
}

public static class Meniu
{
    public static IReadOnlyList<MeniuPunktas> Punktai(string? role) => role switch
    {
        Roles.Klientas =>
        [
            new("Pagrindinis", "/", ["Pagrindinis"]),
            new("Mano profilis", "/klientas/profilis", ["Klientas", "IstorijosPatikrinimas"]),
            new("Mano augintiniai", "/augintiniai", ["Augintiniai", "Dokumentai"]),
            new("Užsakymai", "/uzsakymai", ["Uzsakymai"])
        ],
        Roles.Specialistas =>
        [
            new("Pagrindinis", "/", ["Pagrindinis"]),
            new("Užsakymai", "/uzsakymai", ["Uzsakymai", "Dokumentai"]),
            new("DNR pavyzdžiai", "/dnrpavyzdziai", ["DnrPavyzdziai"]),
            new("Inkubatoriai", "/inkubatoriai", ["Inkubatoriai"])
        ],
        Roles.Administratorius =>
        [
            new("Pagrindinis", "/", ["Pagrindinis"]),
            new("Darbuotojai", "/darbuotojai", ["Darbuotojai"]),
            new("Krūvio analizė", "/darbuotojai/kruvioanalize", ["Darbuotojai"], "Kruvio")
        ],
        _ => [new("Pradžia", "/", ["Pagrindinis"])]
    };
}
