using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.Services;

public record Laiskas(string Gavejas, string Tema, string Tekstas);

// «boundary» Laiško šablonas (3.1.17 pav.): pranešimas apie užsakymo patvirtinimą
public static class LaiskoSablonas
{
    public static Laiskas UzsakymoPatvirtinimas(Uzsakymas uzsakymas, Augintinis augintinis, Klientas klientas, DnrPavyzdys? pavyzdys) => new(
        klientas.ElPastas,
        $"Užsakymas {uzsakymas.Numeris} patvirtintas",
        string.Join("\n",
            $"Gerb. {klientas.VardasPavarde},",
            "",
            $"Jūsų klonavimo užsakymas {uzsakymas.Numeris} augintiniui „{augintinis.Vardas}“ ({augintinis.Tipas.ToString().ToLowerInvariant()}) patvirtintas {Formatai.Data(uzsakymas.PatvirtinimoData)}.",
            $"DNR pavyzdys {(pavyzdys is null ? "" : pavyzdys.Kodas + " ")}įvertintas kaip tinkamas klonuoti.",
            $"Užsakymo kaina: {Formatai.Pinigai(uzsakymas.Kaina)}.",
            "",
            "Apie tolesnius klonavimo etapus informuosime atskiru laišku.",
            "",
            "Pagarbiai",
            "Gyvūnų klonavimo sistema"));
}
