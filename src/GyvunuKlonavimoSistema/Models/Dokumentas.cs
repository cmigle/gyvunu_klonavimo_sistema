namespace GyvunuKlonavimoSistema.Models;

public class Dokumentas
{
    public int Id { get; set; }
    public string FailoPavadinimas { get; set; } = "";
    public string DokumentoTipas { get; set; } = "";
    public DateOnly IkelimoData { get; set; }
    public string FailoFormatas { get; set; } = "";
    public DateOnly GaliojimoData { get; set; }

    public int AugintinisId { get; set; }
    public Augintinis Augintinis { get; set; } = null!;

    public bool Galioja(DateOnly siandien) => GaliojimoData >= siandien;
}

public static class DokumentuTipai
{
    public static readonly string[] Visi = ["Kilmės sertifikatas", "Sutartis", "Veterinarinis pasas", "Kita"];
}

public static class FailuFormatai
{
    // Plėtinys -> failo formatas, saugomas DB
    public static readonly Dictionary<string, string> PagalPletini = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "PDF",
        [".jpg"] = "JPG",
        [".jpeg"] = "JPG",
        [".png"] = "PNG"
    };

    public static string TurinioTipas(string formatas) => formatas switch
    {
        "PDF" => "application/pdf",
        "JPG" => "image/jpeg",
        "PNG" => "image/png",
        _ => "application/octet-stream"
    };
}
