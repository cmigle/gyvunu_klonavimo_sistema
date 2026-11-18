using System.ComponentModel.DataAnnotations;
using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.ViewModels;

public class DokumentuSarasoModelis
{
    public required Augintinis Augintinis { get; init; }
    // Augintiniai, kurių dokumentus galima perjungti: klientui – savi, specialistui – su užsakymu
    public List<Augintinis> Augintiniai { get; init; } = [];
    public List<Dokumentas> Dokumentai { get; init; } = [];
    public bool Klientas { get; init; }
    public DateOnly Siandien { get; init; }
}

public class DokumentoPerziurosModelis
{
    public required Dokumentas Dokumentas { get; init; }
    public bool Klientas { get; init; }
    public bool FailasYra { get; init; }
    public DateOnly Siandien { get; init; }
}

public class DokumentoForma
{
    [Display(Name = "Pavadinimas"), Required, StringLength(255)]
    public string FailoPavadinimas { get; set; } = "";

    [Display(Name = "Dokumento tipas"), Required]
    public string DokumentoTipas { get; set; } = "";

    [Display(Name = "Galiojimo data"), Required]
    public DateOnly? GaliojimoData { get; set; }
}

public class DokumentoIkelimoForma : DokumentoForma
{
    [Display(Name = "Failas"), Required]
    public IFormFile? Failas { get; set; }
}

public class DokumentoRedagavimoForma : DokumentoForma
{
    [Display(Name = "Naujas failas (neprivaloma)")]
    public IFormFile? Failas { get; set; }
}
