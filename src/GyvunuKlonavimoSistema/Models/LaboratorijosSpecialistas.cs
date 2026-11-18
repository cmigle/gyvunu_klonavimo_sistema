namespace GyvunuKlonavimoSistema.Models;

// Paveldėjimas Darbuotojas ← Laboratorijos specialistas (atskira lentelė, bendras pirminis raktas)
public class LaboratorijosSpecialistas : Darbuotojas
{
    public bool Busena { get; set; } = true;
    public string LicencijosNumeris { get; set; } = "";

    public List<Klonavimas> Klonavimai { get; set; } = [];

    public string BusenosPavadinimas => Busena ? "Aktyvus" : "Neaktyvus";
}
