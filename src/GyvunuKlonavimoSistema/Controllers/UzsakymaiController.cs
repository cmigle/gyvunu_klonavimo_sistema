using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Užsakymų valdymo valdiklis: klientas valdo savo užsakymus, specialistas peržiūri visus
[Authorize(Roles = $"{Roles.Klientas},{Roles.Specialistas}")]
public class UzsakymaiController(KlonavimoDbContext db, UzsakymoPatvirtinimas patvirtinimas, TimeProvider laikas) : ValdiklisBase
{
    private bool EsuKlientas => User.Role() == Roles.Klientas;
    private DateOnly Siandien => DateOnly.FromDateTime(laikas.GetLocalNow().DateTime);

    private IQueryable<Uzsakymas> Pasiekiami() => EsuKlientas
        ? db.Uzsakymai.Where(u => u.Augintinis.KlientasId == NaudotojoId)
        : db.Uzsakymai;

    public async Task<IActionResult> Index() => View(await Pasiekiami()
        .Include(u => u.Augintinis).ThenInclude(a => a.Klientas)
        .OrderByDescending(u => u.SukurimoData).ThenByDescending(u => u.Id)
        .ToListAsync());

    // 2.11 pav. PA „Peržiūrėti užsakymą“
    public async Task<IActionResult> Perziura(int id)
    {
        var uzsakymas = await Pasiekiami()
            .Include(u => u.Augintinis).ThenInclude(a => a.Klientas)
            .Include(u => u.Augintinis).ThenInclude(a => a.DnrPavyzdys)
            .Include(u => u.Klonavimas).ThenInclude(k => k!.Inkubatorius)
            .Include(u => u.Pranesimas)
            .FirstOrDefaultAsync(u => u.Id == id);
        return uzsakymas is null ? Nera("Užsakymas nerastas.", "/uzsakymai", "Grįžti į sąrašą") : View(uzsakymas);
    }

    // 2.12 pav. PA „Sukurti užsakymą“, kuris «include» PA „Patikrinti kliento istoriją“
    [HttpGet, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Kurti(int? augintinisId) =>
        View(await KurimoForma(new UzsakymoForma { AugintinisId = augintinisId }));

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Kurti(UzsakymoForma forma, string? veiksmas)
    {
        await KurimoForma(forma);
        if (forma.Kliutis is not null) return View(forma);
        if (veiksmas == "grizti")
        {
            ModelState.Clear();
            return View(forma);
        }

        // Sistema patikrina užsakymo duomenis
        var augintinis = forma.Augintiniai.FirstOrDefault(a => a.Id == forma.AugintinisId);
        if (ModelState.IsValid && augintinis is null)
            KlaidaLaukui(nameof(forma.AugintinisId), "Užsakymo duomenys neteisingi: pasirinktas augintinis neegzistuoja arba jau turi užsakymą.");
        if (!ModelState.IsValid) return View(forma);

        // Pirmiausia pateikiama užpildytos formos peržiūra
        if (veiksmas != "pateikti")
        {
            forma.Perziura = augintinis;
            return View(forma);
        }

        var uzsakymas = new Uzsakymas
        {
            AugintinisId = augintinis!.Id,
            Kaina = KlonavimoNormos.Kainos[augintinis.Tipas],
            SukurimoData = Siandien
        };
        db.Uzsakymai.Add(uzsakymas);
        await db.SaveChangesAsync();

        // Jei augintinio DNR pavyzdys jau tinkamas klonuoti, užsakymas patvirtinamas iš karto
        var patvirtintas = await patvirtinimas.PatvirtintiJeiTinkaAsync(augintinis.Id);
        Sekme(patvirtintas is null
            ? $"Užsakymas {uzsakymas.Numeris} sukurtas. Būsena: „Nepatvirtintas“."
            : $"Užsakymas {uzsakymas.Numeris} sukurtas ir automatiškai patvirtintas, nes augintinio DNR pavyzdys tinkamas klonuoti. {LaiskoBusena(patvirtintas)}");
        return RedirectToAction(nameof(Index));
    }

    // 2.13 pav. PA „Redaguoti užsakymą“: redaguoti galima tik nepatvirtintą užsakymą
    [HttpGet, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Redaguoti(int id)
    {
        var uzsakymas = await Pasiekiami().FirstOrDefaultAsync(u => u.Id == id);
        if (uzsakymas is null) return Nera("Užsakymas nerastas.", "/uzsakymai", "Grįžti į sąrašą");
        if (uzsakymas.Busena) return NegalimaRedaguoti(uzsakymas);
        return View(await RedagavimoForma(uzsakymas, new UzsakymoForma { AugintinisId = uzsakymas.AugintinisId }));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Redaguoti(int id, UzsakymoForma forma)
    {
        var uzsakymas = await Pasiekiami().FirstOrDefaultAsync(u => u.Id == id);
        if (uzsakymas is null) return Nera("Užsakymas nerastas.", "/uzsakymai", "Grįžti į sąrašą");
        if (uzsakymas.Busena) return NegalimaRedaguoti(uzsakymas);

        // Patikrinami pakeisti duomenys; klaidingi laukai nurodomi redagavimo formoje
        await RedagavimoForma(uzsakymas, forma);
        var augintinis = forma.Augintiniai.FirstOrDefault(a => a.Id == forma.AugintinisId);
        if (ModelState.IsValid && augintinis is null) ModelState.AddModelError(nameof(forma.AugintinisId), "");
        if (!ModelState.IsValid) return View(forma);

        uzsakymas.AugintinisId = augintinis!.Id;
        uzsakymas.Kaina = KlonavimoNormos.Kainos[augintinis.Tipas];
        await db.SaveChangesAsync();
        var patvirtintas = await patvirtinimas.PatvirtintiJeiTinkaAsync(augintinis.Id);
        Sekme(patvirtintas is null
            ? $"Užsakymas {uzsakymas.Numeris} sėkmingai atnaujintas."
            : $"Užsakymas {uzsakymas.Numeris} atnaujintas ir automatiškai patvirtintas, nes augintinio DNR pavyzdys tinkamas klonuoti. {LaiskoBusena(patvirtintas)}");
        return RedirectToAction(nameof(Index));
    }
    // 2.14 pav. PA „Ištrinti užsakymą“: ištrinti galima tik nepatvirtintą užsakymą
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Istrinti(int id)
    {
        var uzsakymas = await Pasiekiami().FirstOrDefaultAsync(u => u.Id == id);
        if (uzsakymas is null) return Nera("Užsakymas nerastas.", "/uzsakymai", "Grįžti į sąrašą");
        if (uzsakymas.Busena)
        {
            Klaida($"Užsakymo {uzsakymas.Numeris} ištrinti negalima, nes jis jau patvirtintas.");
            return RedirectToAction(nameof(Index));
        }
        db.Uzsakymai.Remove(uzsakymas);
        await db.SaveChangesAsync();
        Sekme($"Užsakymas {uzsakymas.Numeris} ištrintas.");
        return RedirectToAction(nameof(Index));
    }

    private IActionResult NegalimaRedaguoti(Uzsakymas uzsakymas)
    {
        Klaida($"Užsakymas {uzsakymas.Numeris} negali būti redaguojamas, nes jis jau patvirtintas.");
        return RedirectToAction(nameof(Index));
    }

    // Užsakyti galima tik patikrinus kliento istoriją ir tik augintiniams, kurie dar neturi užsakymo
    private async Task<UzsakymoForma> KurimoForma(UzsakymoForma forma)
    {
        var klientas = await db.Klientai.FindAsync(NaudotojoId);
        forma.Kliutis = klientas?.IstorijosBusena switch
        {
            IstorijosBusenos.Patikrintas => null,
            IstorijosBusenos.Netinkamas => Kliutis.KlientasNetinkamas,
            _ => Kliutis.IstorijaNepatikrinta
        };
        if (forma.Kliutis is not null) return forma;
        forma.Augintiniai = await db.Augintiniai
            .Where(a => a.KlientasId == NaudotojoId && a.Uzsakymas == null)
            .OrderBy(a => a.Id).ToListAsync();
        if (forma.Augintiniai.Count == 0) forma.Kliutis = Kliutis.NeraAugintiniu;
        return forma;
    }

    // Galima pasirinkti to paties užsakymo augintinį arba augintinį be užsakymo
    private async Task<UzsakymoForma> RedagavimoForma(Uzsakymas uzsakymas, UzsakymoForma forma)
    {
        forma.Uzsakymas = uzsakymas;
        forma.Augintiniai = await db.Augintiniai
            .Where(a => a.KlientasId == NaudotojoId && (a.Id == uzsakymas.AugintinisId || a.Uzsakymas == null))
            .OrderBy(a => a.Id).ToListAsync();
        return forma;
    }

    private static string LaiskoBusena(Uzsakymas uzsakymas) => uzsakymas.Pranesimas?.Busena == true
        ? "Patvirtinimo laiškas išsiųstas el. paštu."
        : "Patvirtinimo laiško išsiųsti nepavyko.";
}
