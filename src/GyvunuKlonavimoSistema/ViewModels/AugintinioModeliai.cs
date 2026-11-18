using System.ComponentModel.DataAnnotations;
using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.ViewModels;

public class AugintiniuSarasoModelis
{
    public List<Augintinis> Augintiniai { get; init; } = [];
    public List<Rodiklis> Rodikliai { get; init; } = [];
}

public class AugintinioForma
{
    [Display(Name = "Vardas"), Required, StringLength(50)]
    public string Vardas { get; set; } = "";

    [Display(Name = "Veislė"), Required, StringLength(80)]
    public string Veisle { get; set; } = "";

    [Display(Name = "Tipas"), Required]
    public GyvunoTipas? Tipas { get; set; }

    [Display(Name = "Amžius (metai)"), Required, Range(0, 100)]
    public int? Amzius { get; set; }

    [Display(Name = "Lytis"), Required]
    public string Lytis { get; set; } = "";

    [Display(Name = "Svoris (kg)"), Required, Range(0.01, 10000)]
    public double? Svoris { get; set; }

    [Display(Name = "Mikroschemos numeris"), Required, RegularExpression("[0-9]{15}")]
    public string MikroschemosNumeris { get; set; } = "";

    public static AugintinioForma Is(Augintinis a) => new()
    {
        Vardas = a.Vardas, Veisle = a.Veisle, Tipas = a.Tipas, Amzius = a.Amzius,
        Lytis = a.Lytis, Svoris = a.Svoris, MikroschemosNumeris = a.MikroschemosNumeris
    };

    public void Perkelti(Augintinis a)
    {
        a.Vardas = Vardas.Trim();
        a.Veisle = Veisle.Trim();
        a.Tipas = Tipas!.Value;
        a.Amzius = Amzius!.Value;
        a.Lytis = Lytis;
        a.Svoris = Svoris!.Value;
        a.MikroschemosNumeris = MikroschemosNumeris;
    }
}
