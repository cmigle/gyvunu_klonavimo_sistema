using System.Globalization;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.Infrastructure;

// Rodomi skaičiai ir datos – pagal lietuvių kalbos taisykles. Užklausų kultūra paliekama
// nepriklausoma, kad formų skaičiai su tašku („38.5“) būtų nuskaitomi teisingai.
public static class Formatai
{
    public static readonly CultureInfo Lt = CultureInfo.GetCultureInfo("lt-LT");
    public const string Bruksnys = "–";

    public static string Pinigai(decimal suma) => suma.ToString("#,##0 €", Lt);

    public static string Skaicius(double? x, int? skaitmenys = null)
    {
        if (x is null) return Bruksnys;
        var formatas = skaitmenys is null ? "#,##0.##" : "#,##0." + new string('0', skaitmenys.Value);
        if (skaitmenys == 0) formatas = "#,##0";
        return x.Value.ToString(formatas, Lt);
    }

    public static string SuVienetais(double? x, string vienetai, int? skaitmenys = null) =>
        x is null ? Bruksnys : string.IsNullOrEmpty(vienetai) ? Skaicius(x, skaitmenys) : $"{Skaicius(x, skaitmenys)} {vienetai}";

    public static string Data(DateOnly? d) => d?.ToString("yyyy-MM-dd") ?? Bruksnys;
    public static string Data(DateTime? d) => d?.ToString("yyyy-MM-dd") ?? Bruksnys;
    public static string DataLaikas(DateTime? d) => d?.ToString("yyyy-MM-dd HH:mm") ?? Bruksnys;
    public static string Metai(int? n) => n is null ? Bruksnys : $"{n} m.";

    // DNR normos tekstas: „≥ 50 ng/µl“, „1,8–2,0“, „≤ -20 °C“
    public static string Norma(DnrNorma n)
    {
        var v = string.IsNullOrEmpty(n.Vienetai) ? "" : " " + n.Vienetai;
        if (n.Min is not null && n.Maks is not null) return $"{Skaicius(n.Min, 1)}–{Skaicius(n.Maks, 1)}{v}";
        return n.Min is not null ? $"≥ {Skaicius(n.Min)}{v}" : $"≤ {Skaicius(n.Maks)}{v}";
    }
}
