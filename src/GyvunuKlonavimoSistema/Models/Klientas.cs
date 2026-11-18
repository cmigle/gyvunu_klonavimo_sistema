namespace GyvunuKlonavimoSistema.Models;

public class Klientas
{
    public int Id { get; set; }
    public string Vardas { get; set; } = "";
    public string Pavarde { get; set; } = "";
    public string ElPastas { get; set; } = "";
    public string TelefonoNumeris { get; set; } = "";
    public long AsmensKodas { get; set; }
    public string Slaptazodis { get; set; } = "";
    public string IstorijosBusena { get; set; } = IstorijosBusenos.Nepatikrintas;
    public DateTime? PatikrinimoData { get; set; }

    public List<Augintinis> Augintiniai { get; set; } = [];

    public string VardasPavarde => $"{Vardas} {Pavarde}";
}

public static class IstorijosBusenos
{
    public const string Nepatikrintas = "Nepatikrintas";
    public const string Patikrintas = "Patikrintas";
    public const string Netinkamas = "Netinkamas";
}
