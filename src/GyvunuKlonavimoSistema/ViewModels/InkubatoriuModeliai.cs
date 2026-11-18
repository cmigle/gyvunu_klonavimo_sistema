using System.ComponentModel.DataAnnotations;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.ViewModels;

// Inkubavimo parametrai – bendri kūrimo ir būsenos redagavimo formoms
public abstract class InkubavimoParametrai
{
    [Display(Name = "Tikslinė temperatūra (°C)"), Required, Range(20, 45)]
    public double? TikslineTemperatura { get; set; }

    [Display(Name = "CO₂ (%)"), Required, Range(0, 20)]
    public double? Co2Procentas { get; set; }

    [Display(Name = "O₂ (%)"), Required, Range(1, 21)]
    public double? O2Procentas { get; set; }

    [Display(Name = "pH lygis"), Required, Range(6, 8.5)]
    public double? PhLygis { get; set; }

    public void Perkelti(Inkubatorius i)
    {
        i.TikslineTemperatura = TikslineTemperatura!.Value;
        i.Co2Procentas = Co2Procentas!.Value;
        i.O2Procentas = O2Procentas!.Value;
        i.PhLygis = PhLygis!.Value;
    }
}

public class InkubatoriausKurimoForma : InkubavimoParametrai
{
    [Display(Name = "Modelis"), Required, StringLength(100)]
    public string Modelis { get; set; } = "";

    [Display(Name = "Serijinis numeris"), Required, StringLength(50)]
    public string SerijinisNumeris { get; set; } = "";

    [Display(Name = "Talpa (l)"), Required, Range(1, 1000)]
    public double? Talpa { get; set; }
}

public class InkubatoriausBusenosForma : InkubavimoParametrai
{
    [Display(Name = "Būsena")]
    public bool Busena { get; set; }

    public static InkubatoriausBusenosForma Is(Inkubatorius i) => new()
    {
        Busena = i.Busena, TikslineTemperatura = i.TikslineTemperatura, Co2Procentas = i.Co2Procentas,
        O2Procentas = i.O2Procentas, PhLygis = i.PhLygis
    };
}

public class InkubatoriuSarasoModelis
{
    public List<Rodiklis> Rodikliai { get; init; } = [];
    // Inkubatorius ir jame vykdomas klonavimas
    public List<(Inkubatorius Inkubatorius, Klonavimas? Klonavimas)> Inkubatoriai { get; init; } = [];
    public List<DnrPavyzdys> LaukiaInkubatoriaus { get; init; } = [];

    [Display(Name = "DNR pavyzdys")]
    public int? DnrPavyzdysId { get; set; }
}

public class InkubatoriausParametruModelis
{
    public required Inkubatorius Inkubatorius { get; init; }
    public Klonavimas? Klonavimas { get; init; }
    public List<InkubatoriausMatavimas> Matavimai { get; init; } = [];
}

// Priskyrimo metu užfiksuoti inkubatorių patikrinimai (būsena – prieš priskyrimą)
public record PatikrinimoEilute(int Id, string Modelis, string SerijinisNumeris, bool Busena, double TikslineTemperatura,
    double Co2Procentas, double O2Procentas, double PhLygis, List<string> Priezastys, double Ivertis)
{
    public bool Tinka => Priezastys.Count == 0;
}

public record PriskyrimoIrasas(int DnrPavyzdysId, DateTime Data, int? ParinktasId, int? KlonavimasId, List<PatikrinimoEilute> Patikrinimai);

public class PriskyrimoRezultatoModelis
{
    public required PriskyrimoIrasas Irasas { get; init; }
    public required DnrPavyzdys Pavyzdys { get; init; }
    public required InkubavimoSalygos Salygos { get; init; }
    public Klonavimas? Klonavimas { get; init; }
    public HashSet<int> EsamiInkubatoriai { get; init; } = [];

    public PatikrinimoEilute? Parinktas => Irasas.Patikrinimai.FirstOrDefault(p => p.Id == Irasas.ParinktasId);

    // Laisvas inkubatorius, kurio parametrai arčiausiai reikalingų – siūloma jį redaguoti
    public PatikrinimoEilute? Artimiausias => Irasas.Patikrinimai
        .Where(p => !p.Busena && EsamiInkubatoriai.Contains(p.Id)).OrderBy(p => p.Ivertis).FirstOrDefault();
}
