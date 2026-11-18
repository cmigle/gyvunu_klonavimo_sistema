using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GyvunuKlonavimoSistema.Infrastructure;

// Būsenos žyma: @Html.Zyma("Patvirtintas") -> <span class="zyma sekme">Patvirtintas</span>
public static class Zymos
{
    private static readonly Dictionary<string, string[]> Tipai = new()
    {
        ["sekme"] = ["Patvirtintas", Tinkamumai.Tinkamas, "Laisvas", "Aktyvus", IstorijosBusenos.Patikrintas, "Išsiųstas",
            "Galioja", DarboKruvioAnalize.Normalus, "Tinka", "Atitinka", "Taip"],
        ["ispejimas"] = ["Nepatvirtintas", Tinkamumai.Neivertintas, IstorijosBusenos.Nepatikrintas],
        ["klaida"] = [Tinkamumai.Netinkamas, "Nebegalioja", DarboKruvioAnalize.Didelis, "Netinka", "Neatitinka", "Neišsiųstas"],
        ["info"] = ["Užimtas", DarboKruvioAnalize.Mazas, "Laboratorijos specialistas", "Vykdomas"]
    };

    public static string Tipas(string tekstas) =>
        Tipai.FirstOrDefault(t => t.Value.Contains(tekstas)).Key ?? "neutrali";

    public static IHtmlContent Zyma(this IHtmlHelper html, string tekstas, string? tipas = null)
    {
        var span = new TagBuilder("span");
        span.AddCssClass("zyma");
        span.AddCssClass(tipas ?? Tipas(tekstas));
        span.InnerHtml.Append(tekstas);
        return span;
    }
}
