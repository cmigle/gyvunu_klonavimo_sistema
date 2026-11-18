using System.Globalization;
using System.Text;
using System.Text.Json;

namespace GyvunuKlonavimoSistema.Services;

public record TeistumoIrasas(string Numeris, string Byla, string Teismas, string Data);

public record IstorijosPaieska(string Asmuo, string Uzklausa, IReadOnlyList<TeistumoIrasas> Irasai);

// «boundary» CourtListener API sąsaja (3.1.16 pav.): teistumo istorijos paieška pagal vardą ir pavardę
public interface ICourtListenerSasaja
{
    Task<IstorijosPaieska> IeskotiAsync(string vardas, string pavarde, CancellationToken ct = default);
}

public static class CourtListener
{
    public const string Adresas = "https://www.courtlistener.com/api/rest/v4/search/";

    public static string Uzklausa(string asmuo) => $"?type=r&q={Uri.EscapeDataString($"\"{asmuo}\"")}";

    public static string Asmuo(string vardas, string pavarde) => $"{vardas.Trim()} {pavarde.Trim()}";
}

// Naudojama, kai nenurodytas CourtListener:Token: grąžina išgalvotus demonstracinius įrašus
public class DemoCourtListenerSasaja : ICourtListenerSasaja
{
    private static readonly (string Asmuo, TeistumoIrasas Irasas)[] Irasai =
    [
        ("Petras Petraitis", new("1-241/2019", "Sukčiavimas", "Demonstracinis apygardos teismas", "2019-04-12")),
        ("Petras Petraitis", new("1-1187/2021", "Žiaurus elgesys su gyvūnais", "Demonstracinis apylinkės teismas", "2021-09-30")),
        ("Antanas Kazlauskas", new("1-86/2017", "Kontrabanda", "Demonstracinis apygardos teismas", "2017-02-08"))
    ];

    public Task<IstorijosPaieska> IeskotiAsync(string vardas, string pavarde, CancellationToken ct = default)
    {
        var asmuo = CourtListener.Asmuo(vardas, pavarde);
        var rasti = Irasai.Where(x => Normalizuoti(x.Asmuo) == Normalizuoti(asmuo)).Select(x => x.Irasas).ToList();
        return Task.FromResult(new IstorijosPaieska(asmuo, "GET " + CourtListener.Adresas + CourtListener.Uzklausa(asmuo), rasti));
    }

    // „Pétras  PETRAITIS“ -> „petras petraitis“
    private static string Normalizuoti(string s)
    {
        var be = new string(s.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        return string.Join(' ', be.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

// Tikra REST API užklausa (v4 paieška tarp RECAP bylų), kai nurodytas CourtListener:Token
public class CourtListenerApiSasaja(HttpClient http) : ICourtListenerSasaja
{
    public async Task<IstorijosPaieska> IeskotiAsync(string vardas, string pavarde, CancellationToken ct = default)
    {
        var asmuo = CourtListener.Asmuo(vardas, pavarde);
        var uzklausa = CourtListener.Uzklausa(asmuo);
        using var atsakymas = await http.GetAsync(uzklausa, ct);
        atsakymas.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await atsakymas.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        var irasai = new List<TeistumoIrasas>();
        if (json.RootElement.TryGetProperty("results", out var rezultatai))
            foreach (var r in rezultatai.EnumerateArray().Take(20))
                irasai.Add(new(Tekstas(r, "docketNumber"), Tekstas(r, "caseName"), Tekstas(r, "court"),
                    Tekstas(r, "dateFiled") is { Length: >= 10 } data ? data[..10] : Tekstas(r, "dateFiled")));
        return new(asmuo, "GET " + CourtListener.Adresas + uzklausa, irasai);
    }

    private static string Tekstas(JsonElement e, string laukas) =>
        e.TryGetProperty(laukas, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
}
