namespace GyvunuKlonavimoSistema.Models;

public class Uzsakymas
{
    public int Id { get; set; }
    public decimal Kaina { get; set; }
    public DateOnly SukurimoData { get; set; }
    public bool Busena { get; set; }
    public DateOnly? PatvirtinimoData { get; set; }

    public int AugintinisId { get; set; }
    public Augintinis Augintinis { get; set; } = null!;
    public Pranesimas? Pranesimas { get; set; }
    public Klonavimas? Klonavimas { get; set; }

    public string Numeris => $"UZ-{Id:D4}";
    public string BusenosPavadinimas => Busena ? "Patvirtintas" : "Nepatvirtintas";
}
