namespace GyvunuKlonavimoSistema.Models;

public class InkubatoriausMatavimas
{
    public int Id { get; set; }
    public DateTime MatavimoData { get; set; }
    public double Temperatura { get; set; }
    public double Dregme { get; set; }

    public int InkubatoriusId { get; set; }
    public Inkubatorius Inkubatorius { get; set; } = null!;
}
