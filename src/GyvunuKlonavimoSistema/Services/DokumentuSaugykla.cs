using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.Services;

// Dokumentų failai saugomi kataloge (Dokumentai:Katalogas) pavadinti įrašo id: „15.pdf“
public class DokumentuSaugykla(IConfiguration konfiguracija, IWebHostEnvironment aplinka)
{
    public const long DidziausiasDydis = 10 * 1024 * 1024;

    private readonly string katalogas = Path.Combine(aplinka.ContentRootPath,
        konfiguracija["Dokumentai:Katalogas"] ?? "uploads/dokumentai");

    public static string Pletinys(string formatas) => "." + formatas.ToLowerInvariant();

    public string Kelias(Dokumentas d) => Path.Combine(katalogas, d.Id + Pletinys(d.FailoFormatas));

    public bool Yra(Dokumentas d) => File.Exists(Kelias(d));

    public async Task IssaugotiAsync(Dokumentas d, IFormFile failas)
    {
        Directory.CreateDirectory(katalogas);
        Istrinti(d.Id);
        await using var srautas = File.Create(Kelias(d));
        await failas.CopyToAsync(srautas);
    }

    public void Istrinti(int dokumentoId)
    {
        if (!Directory.Exists(katalogas)) return;
        foreach (var failas in Directory.GetFiles(katalogas, $"{dokumentoId}.*"))
            File.Delete(failas);
    }
}
