namespace GyvunuKlonavimoSistema.Models;

public class Inkubatorius
{
    public int Id { get; set; }
    public string Modelis { get; set; } = "";
    public string SerijinisNumeris { get; set; } = "";
    public double TikslineTemperatura { get; set; }
    public double Talpa { get; set; }
    public double Co2Procentas { get; set; }
    public bool Busena { get; set; }
    public double PhLygis { get; set; }
    public double O2Procentas { get; set; }

    public List<InkubatoriausMatavimas> Matavimai { get; set; } = [];
    public List<Klonavimas> Klonavimai { get; set; } = [];

    public string BusenosPavadinimas => Busena ? "Užimtas" : "Laisvas";
}
