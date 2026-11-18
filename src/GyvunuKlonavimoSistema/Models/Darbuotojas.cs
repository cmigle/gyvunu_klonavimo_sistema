namespace GyvunuKlonavimoSistema.Models;

// Darbuotojas be specialisto įrašo – administratorius
public class Darbuotojas
{
    public int Id { get; set; }
    public string Vardas { get; set; } = "";
    public string Pavarde { get; set; } = "";
    public string TelefonoNumeris { get; set; } = "";
    public string ElPastas { get; set; } = "";
    public double Etatas { get; set; }
    public string Slaptazodis { get; set; } = "";

    public string VardasPavarde => $"{Vardas} {Pavarde}";
}
