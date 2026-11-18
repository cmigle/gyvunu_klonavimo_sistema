using System.ComponentModel.DataAnnotations;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.ViewModels;

public class ProfilioModelis
{
    public required Klientas Klientas { get; init; }
    public List<Rodiklis> Rodikliai { get; init; } = [];
    // Paskyros ištrinti negalima, kai yra patvirtintų užsakymų
    public bool GalimaIstrinti { get; init; }
}

public class ProfilioForma
{
    [Display(Name = "Vardas"), Required, StringLength(50)]
    public string Vardas { get; set; } = "";

    [Display(Name = "Pavardė"), Required, StringLength(50)]
    public string Pavarde { get; set; } = "";

    [Display(Name = "El. paštas"), Required, EmailAddress, StringLength(100)]
    public string ElPastas { get; set; } = "";

    [Display(Name = "Telefono numeris"), Required, RegularExpression(@"\+?[0-9 ]{8,15}")]
    public string TelefonoNumeris { get; set; } = "";

    [Display(Name = "Asmens kodas"), Required, RegularExpression("[0-9]{11}")]
    public string AsmensKodas { get; set; } = "";
}

public class IstorijosForma
{
    [Display(Name = "Vardas"), Required, StringLength(50)]
    public string Vardas { get; set; } = "";

    [Display(Name = "Pavardė"), Required, StringLength(50)]
    public string Pavarde { get; set; } = "";

    [Display(Name = "Asmens kodas"), Required, RegularExpression("[0-9]{11}")]
    public string AsmensKodas { get; set; } = "";

    // „uzsakymas“, kai patikrinimas pradėtas kuriant užsakymą («include»)
    public string? Grizti { get; set; }
}

public class IstorijosRezultatoModelis
{
    public required Klientas Klientas { get; init; }
    public required string Asmuo { get; init; }
    public required string Uzklausa { get; init; }
    public int? RastaIrasu { get; init; }
    public IReadOnlyList<TeistumoIrasas> Irasai { get; init; } = [];
    public string? Grizti { get; init; }

    public bool Patikrintas => Klientas.IstorijosBusena == IstorijosBusenos.Patikrintas;
}
