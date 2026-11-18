using System.ComponentModel.DataAnnotations;
using System.Text;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GyvunuKlonavimoSistema.ViewModels;

// Naujo DNR pavyzdžio įvedimo langas ir DNR pavyzdžio redagavimo forma
public class DnrPavyzdzioForma
{
    [Display(Name = "Augintinis"), Required]
    public int? AugintinisId { get; set; }

    [Display(Name = "Paėmimo data"), Required]
    public DateOnly? PaemimoData { get; set; }

    [Display(Name = "Saugojimo temperatūra (°C)"), Required, Range(-200, 10)]
    public double? SaugojimoTemperatura { get; set; } = -80;

    [Display(Name = "Sugeneruoti ir spausdinti pavyzdžio etiketę")]
    public bool SpausdintiEtikete { get; set; } = true;

    // Analizės rezultatai įvedami redagavimo formoje
    [Display(Name = "DNR koncentracija (ng/µl)"), Range(0, double.MaxValue)]
    public double? DnrKoncentracija { get; set; }

    [Display(Name = "Tirpalo tūris (ml)"), Range(0, double.MaxValue)]
    public double? TirpaloTuris { get; set; }

    [Display(Name = "Baltymų tarša (A260/280)"), Range(0, double.MaxValue)]
    public double? BaltymuTarsa { get; set; }

    [Display(Name = "DIN vertė"), Range(0, double.MaxValue)]
    public double? DinVerte { get; set; }

    [Display(Name = "Ląstelių gyvybingumas (%)"), Range(0, 100)]
    public double? LasteliuGyvybingumas { get; set; }

    // Užpildo valdiklis
    [BindNever, ValidateNever] public List<Augintinis> Augintiniai { get; set; } = [];
    [BindNever, ValidateNever] public DnrPavyzdys? Pavyzdys { get; set; }
    // Klonavime panaudoto pavyzdžio augintinio keisti negalima
    [BindNever, ValidateNever] public bool AugintinisUzrakintas { get; set; }
}

public record DnrSarasas(List<DnrPavyzdys> Pavyzdziai, DnrPavyzdys? Etikete);

public record DnrAtaskaita(DnrPavyzdys Pavyzdys, TinkamumoRezultatas Analize);

public static class DnrEtikete
{
    // Demonstracinis brūkšninis kodas (dekoratyvus) pagal pavyzdžio kodą, kaip prototipe
    public static string Barkodas(string tekstas)
    {
        var x = 0;
        var juostos = new StringBuilder();
        for (var i = 0; i < tekstas.Length; i++)
        {
            var kodas = tekstas[i] * 7 + i * 13;
            for (var b = 0; b < 6; b++)
            {
                var plotis = 1 + ((kodas >> b) & 1) + ((kodas >> (b + 3)) & 1);
                if (b % 2 == 0) juostos.Append($"<rect x=\"{x}\" width=\"{plotis}\" height=\"46\"/>");
                x += plotis;
            }
        }
        return $"<svg viewBox=\"0 0 {x} 46\" preserveAspectRatio=\"none\" fill=\"currentColor\">{juostos}</svg>";
    }
}
