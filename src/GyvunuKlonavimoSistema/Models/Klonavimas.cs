namespace GyvunuKlonavimoSistema.Models;

public class Klonavimas
{
    public int Id { get; set; }
    public DateOnly PradziosData { get; set; }
    public DateOnly PabaigosData { get; set; }
    public string? Rezultatas { get; set; }
    public bool SpecialistoPatvirtinimas { get; set; }
    public bool Busena { get; set; } = true;

    public int UzsakymasId { get; set; }
    public Uzsakymas Uzsakymas { get; set; } = null!;
    public int DnrPavyzdysId { get; set; }
    public DnrPavyzdys DnrPavyzdys { get; set; } = null!;
    public int? LaboratorijosSpecialistasId { get; set; }
    public LaboratorijosSpecialistas? LaboratorijosSpecialistas { get; set; }
    public int? InkubatoriusId { get; set; }
    public Inkubatorius? Inkubatorius { get; set; }

    public string Numeris => $"KL-{Id:D4}";
    public int TrukmeDienomis => PabaigosData.DayNumber - PradziosData.DayNumber;
    public string BusenosPavadinimas => Busena ? "Aktyvus" : "Baigtas";
}
