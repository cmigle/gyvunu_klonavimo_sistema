namespace GyvunuKlonavimoSistema.Models;

public class Augintinis
{
    public int Id { get; set; }
    public string Vardas { get; set; } = "";
    public string Veisle { get; set; } = "";
    public int Amzius { get; set; }
    public GyvunoTipas Tipas { get; set; }
    public string Lytis { get; set; } = "";
    public double Svoris { get; set; }
    public string MikroschemosNumeris { get; set; } = "";

    public int KlientasId { get; set; }
    public Klientas Klientas { get; set; } = null!;
    public List<Dokumentas> Dokumentai { get; set; } = [];
    public Uzsakymas? Uzsakymas { get; set; }
    public DnrPavyzdys? DnrPavyzdys { get; set; }

    // „Reksas (šuo)“
    public string Pavadinimas => $"{Vardas} ({Tipas.ToString().ToLowerInvariant()})";
}

public static class Lytys
{
    public const string Patinas = "Patinas";
    public const string Patele = "Patelė";
    public static readonly string[] Visos = [Patinas, Patele];
}
