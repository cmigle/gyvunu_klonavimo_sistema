using System.ComponentModel.DataAnnotations;
using GyvunuKlonavimoSistema.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GyvunuKlonavimoSistema.ViewModels;

// Užsakymo kūrimo ir redagavimo langų forma
public class UzsakymoForma
{
    [Display(Name = "Augintinis"), Required]
    public int? AugintinisId { get; set; }

    // Užpildo valdiklis
    [BindNever, ValidateNever] public List<Augintinis> Augintiniai { get; set; } = [];
    [BindNever, ValidateNever] public Kliutis? Kliutis { get; set; }
    [BindNever, ValidateNever] public Augintinis? Perziura { get; set; }
    [BindNever, ValidateNever] public Uzsakymas? Uzsakymas { get; set; }
}

// Kodėl užsakymo sukurti negalima («include» PA „Patikrinti kliento istoriją“)
public enum Kliutis
{
    IstorijaNepatikrinta,
    KlientasNetinkamas,
    NeraAugintiniu
}
