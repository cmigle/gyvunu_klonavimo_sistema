using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» DNR pavyzdžių valdiklis: nuskaito, kuria, keičia ir šalina esybę „DNR pavyzdys“
[Authorize(Roles = Roles.Specialistas)]
public class DnrPavyzdziaiController(KlonavimoDbContext db, UzsakymoPatvirtinimas patvirtinimas, TimeProvider laikas) : ValdiklisBase
{
    private DateOnly Siandien => DateOnly.FromDateTime(laikas.GetLocalNow().DateTime);

    // 3.1.1 pav.: DNR pavyzdžių puslapis; po kūrimo – sugeneruota etiketė (2.16 pav.)
    public async Task<IActionResult> Index(int? etikete)
    {
        var pavyzdziai = await db.DnrPavyzdziai
            .Include(d => d.Augintinis).ThenInclude(a => a.Klientas)
            .OrderByDescending(d => d.PaemimoData).ThenByDescending(d => d.Id)
            .ToListAsync();
        return View(new DnrSarasas(pavyzdziai, pavyzdziai.FirstOrDefault(d => d.Id == etikete)));
    }

    // 2.16 pav. PA „Sukurti DNR pavyzdį“
    [HttpGet]
    public async Task<IActionResult> Kurti()
    {
        var forma = new DnrPavyzdzioForma { PaemimoData = Siandien };
        forma.Augintiniai = await AugintiniaiBePavyzdzio(null);
        return View(forma);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Kurti(DnrPavyzdzioForma forma)
    {
        forma.Augintiniai = await AugintiniaiBePavyzdzio(null);
        Tikrinti(forma);
        if (!ModelState.IsValid) return View(forma);

        var pavyzdys = new DnrPavyzdys
        {
            AugintinisId = forma.AugintinisId!.Value,
            PaemimoData = forma.PaemimoData!.Value,
            SaugojimoTemperatura = forma.SaugojimoTemperatura!.Value
        };
        db.DnrPavyzdziai.Add(pavyzdys);
        await db.SaveChangesAsync();
        Sekme($"DNR pavyzdys {pavyzdys.Kodas} sukurtas.");
        return RedirectToAction(nameof(Index), new { etikete = forma.SpausdintiEtikete ? pavyzdys.Id : (int?)null });
    }

    // 2.17 pav. PA „Peržiūrėti DNR pavyzdį“
    public async Task<IActionResult> Perziura(int id)
    {
        var pavyzdys = await db.DnrPavyzdziai
            .Include(d => d.Augintinis).ThenInclude(a => a.Klientas)
            .Include(d => d.Augintinis).ThenInclude(a => a.Uzsakymas)
            .Include(d => d.Klonavimas).ThenInclude(k => k!.Inkubatorius)
            .FirstOrDefaultAsync(d => d.Id == id);
        return pavyzdys is null ? NeraPavyzdzio() : View(pavyzdys);
    }

    // 2.19 pav. PA „Redaguoti DNR pavyzdį“ (čia įvedami ir analizės rezultatai)
    [HttpGet]
    public async Task<IActionResult> Redaguoti(int id)
    {
        var pavyzdys = await db.DnrPavyzdziai.Include(d => d.Klonavimas).FirstOrDefaultAsync(d => d.Id == id);
        if (pavyzdys is null) return NeraPavyzdzio();
        var forma = new DnrPavyzdzioForma
        {
            AugintinisId = pavyzdys.AugintinisId,
            PaemimoData = pavyzdys.PaemimoData,
            SaugojimoTemperatura = pavyzdys.SaugojimoTemperatura,
            DnrKoncentracija = pavyzdys.DnrKoncentracija,
            TirpaloTuris = pavyzdys.TirpaloTuris,
            BaltymuTarsa = pavyzdys.BaltymuTarsa,
            DinVerte = pavyzdys.DinVerte,
            LasteliuGyvybingumas = pavyzdys.LasteliuGyvybingumas
        };
        return View(await RedagavimoForma(pavyzdys, forma));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Redaguoti(int id, DnrPavyzdzioForma forma)
    {
        var pavyzdys = await db.DnrPavyzdziai.Include(d => d.Klonavimas).FirstOrDefaultAsync(d => d.Id == id);
        if (pavyzdys is null) return NeraPavyzdzio();
        await RedagavimoForma(pavyzdys, forma);
        if (forma.AugintinisUzrakintas)
        {
            forma.AugintinisId = pavyzdys.AugintinisId;
            ModelState.Remove(nameof(forma.AugintinisId));
        }
        Tikrinti(forma);
        if (!ModelState.IsValid) return View(forma);

        // Pakeitus saugojimo temperatūrą ar rezultatus, tinkamumą reikia nustatyti iš naujo
        var pasikeite = pavyzdys.SaugojimoTemperatura != forma.SaugojimoTemperatura
            || pavyzdys.DnrKoncentracija != forma.DnrKoncentracija || pavyzdys.TirpaloTuris != forma.TirpaloTuris
            || pavyzdys.BaltymuTarsa != forma.BaltymuTarsa || pavyzdys.DinVerte != forma.DinVerte
            || pavyzdys.LasteliuGyvybingumas != forma.LasteliuGyvybingumas;
        var buvoIvertintas = pavyzdys.Tinkamumas is not null;

        pavyzdys.AugintinisId = forma.AugintinisId!.Value;
        pavyzdys.PaemimoData = forma.PaemimoData!.Value;
        pavyzdys.SaugojimoTemperatura = forma.SaugojimoTemperatura!.Value;
        pavyzdys.DnrKoncentracija = forma.DnrKoncentracija;
        pavyzdys.TirpaloTuris = forma.TirpaloTuris;
        pavyzdys.BaltymuTarsa = forma.BaltymuTarsa;
        pavyzdys.DinVerte = forma.DinVerte;
        pavyzdys.LasteliuGyvybingumas = forma.LasteliuGyvybingumas;
        if (pasikeite)
        {
            pavyzdys.Tinkamumas = null;
            pavyzdys.TinkamumoData = null;
        }
        await db.SaveChangesAsync();
        Sekme($"DNR pavyzdžio {pavyzdys.Kodas} duomenys atnaujinti.{(pasikeite && buvoIvertintas ? " Tinkamumą reikia nustatyti iš naujo." : "")}");
        return RedirectToAction(nameof(Index));
    }

    // 2.18 pav. PA „Ištrinti DNR pavyzdį“
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Istrinti(int id)
    {
        var pavyzdys = await db.DnrPavyzdziai.Include(d => d.Klonavimas).FirstOrDefaultAsync(d => d.Id == id);
        if (pavyzdys is null) return NeraPavyzdzio();
        if (pavyzdys.Klonavimas is not null)
        {
            Klaida($"DNR pavyzdžio {pavyzdys.Kodas} ištrinti negalima, nes jis naudojamas klonavime {pavyzdys.Klonavimas.Numeris}.");
            return RedirectToAction(nameof(Index));
        }
        db.DnrPavyzdziai.Remove(pavyzdys);
        await db.SaveChangesAsync();
        Sekme($"DNR pavyzdys {pavyzdys.Kodas} ištrintas.");
        return RedirectToAction(nameof(Index));
    }

    // 2.20 pav. PA „Nustatyti DNR pavyzdžio tinkamumą“ (inicijuojama iš DNR pavyzdžių puslapio, 3.1.3 pav.)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NustatytiTinkamuma(int id)
    {
        var pavyzdys = await db.DnrPavyzdziai.FindAsync(id);
        if (pavyzdys is null) return NeraPavyzdzio();
        var analize = DnrTinkamumoAnalize.Analizuoti(pavyzdys);
        if (!analize.GalimaNustatyti)
        {
            Klaida($"Pavyzdžio {pavyzdys.Kodas} tinkamumo nustatyti negalima, nes neįvesti analizės rezultatai: {string.Join(", ", analize.Truksta)}.");
            return RedirectToAction(nameof(Index));
        }

        pavyzdys.Tinkamumas = analize.Statusas;
        pavyzdys.TinkamumoData = Siandien;
        await db.SaveChangesAsync();

        if (analize.Statusas == Tinkamumai.Tinkamas)
        {
            // Tinkamas pavyzdys – augintinio užsakymas patvirtinamas ir klientui siunčiamas laiškas (3.1.17 pav.)
            var uzsakymas = await patvirtinimas.PatvirtintiJeiTinkaAsync(pavyzdys.AugintinisId);
            var apieUzsakyma = uzsakymas is null ? ""
                : uzsakymas.Pranesimas?.Busena == true
                    ? $" Užsakymas {uzsakymas.Numeris} automatiškai patvirtintas, klientui išsiųstas patvirtinimo laiškas."
                    : $" Užsakymas {uzsakymas.Numeris} automatiškai patvirtintas, bet patvirtinimo laiško išsiųsti nepavyko.";
            Sekme($"Analizė atlikta: pavyzdys {pavyzdys.Kodas} tinkamas klonuoti. Sugeneruotas sėkmingos analizės sertifikatas.{apieUzsakyma}");
        }
        else
        {
            Klaida($"Analizė atlikta: pavyzdys {pavyzdys.Kodas} netinkamas, neatitikimų: {analize.Neatitikimai.Count}.");
        }
        return RedirectToAction(nameof(Ataskaita), new { id });
    }

    // DNR pavyzdžio ataskaitos langas: sertifikatas arba atmetimo priežasčių suvestinė su detalia ataskaita
    public async Task<IActionResult> Ataskaita(int id)
    {
        var pavyzdys = await db.DnrPavyzdziai.Include(d => d.Augintinis).FirstOrDefaultAsync(d => d.Id == id);
        if (pavyzdys is null) return NeraPavyzdzio();
        var ataskaita = new DnrAtaskaita(pavyzdys, DnrTinkamumoAnalize.Analizuoti(pavyzdys));
        if (pavyzdys.Tinkamumas is null || !ataskaita.Analize.GalimaNustatyti)
            Info($"Pavyzdžio {pavyzdys.Kodas} tinkamumas dar nenustatytas. Analizę inicijuokite DNR pavyzdžių puslapyje.");
        return View(ataskaita);
    }

    private IActionResult NeraPavyzdzio() => Nera("DNR pavyzdys nerastas.", "/dnrpavyzdziai", "Grįžti į sąrašą");

    // Rodomi augintiniai, kurie dar neturi DNR pavyzdžio (redaguojant – ir dabartinis)
    private Task<List<Augintinis>> AugintiniaiBePavyzdzio(int? dabartinis) => db.Augintiniai
        .Include(a => a.Klientas)
        .Where(a => a.Id == dabartinis || a.DnrPavyzdys == null)
        .OrderBy(a => a.Id).ToListAsync();

    private async Task<DnrPavyzdzioForma> RedagavimoForma(DnrPavyzdys pavyzdys, DnrPavyzdzioForma forma)
    {
        forma.Pavyzdys = pavyzdys;
        forma.AugintinisUzrakintas = pavyzdys.Klonavimas is not null;
        forma.Augintiniai = await AugintiniaiBePavyzdzio(pavyzdys.AugintinisId);
        return forma;
    }

    // Augintinis turi būti iš galimų sąrašo, paėmimo data – ne vėlesnė nei šiandien
    private void Tikrinti(DnrPavyzdzioForma forma)
    {
        if (forma.AugintinisId is not null && forma.Augintiniai.All(a => a.Id != forma.AugintinisId))
            ModelState.AddModelError(nameof(forma.AugintinisId), "");
        if (forma.PaemimoData > Siandien)
            ModelState.AddModelError(nameof(forma.PaemimoData), "");
    }
}
