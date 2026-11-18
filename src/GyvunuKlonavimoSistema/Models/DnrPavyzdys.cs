namespace GyvunuKlonavimoSistema.Models;

public class DnrPavyzdys
{
    public int Id { get; set; }
    public DateOnly PaemimoData { get; set; }
    public double SaugojimoTemperatura { get; set; }
    public double? DnrKoncentracija { get; set; }
    public double? TirpaloTuris { get; set; }
    public double? BaltymuTarsa { get; set; }
    public double? DinVerte { get; set; }
    public double? LasteliuGyvybingumas { get; set; }
    public string? Tinkamumas { get; set; }
    public DateOnly? TinkamumoData { get; set; }

    public int AugintinisId { get; set; }
    public Augintinis Augintinis { get; set; } = null!;
    public Klonavimas? Klonavimas { get; set; }

    public string Kodas => $"DNR-{Id:D4}";
    public bool RezultataiIvesti => DnrKoncentracija is not null && TirpaloTuris is not null
        && BaltymuTarsa is not null && DinVerte is not null && LasteliuGyvybingumas is not null;
}

public static class Tinkamumai
{
    public const string Tinkamas = "Tinkamas klonuoti";
    public const string Netinkamas = "Netinkamas";
    public const string Neivertintas = "Neįvertintas";
}
