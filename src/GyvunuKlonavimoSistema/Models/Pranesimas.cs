namespace GyvunuKlonavimoSistema.Models;

public class Pranesimas
{
    public int Id { get; set; }
    public string Tekstas { get; set; } = "";
    public DateTime SiuntimoData { get; set; }
    public bool Busena { get; set; }

    public int UzsakymasId { get; set; }
    public Uzsakymas Uzsakymas { get; set; } = null!;

    public string BusenosPavadinimas => Busena ? "Išsiųstas" : "Neišsiųstas";
}
