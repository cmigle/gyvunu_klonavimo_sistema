using System.Text.Json;
using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Inkubatorių valdiklis
[Authorize(Roles = Roles.Specialistas)]
public class InkubatoriaiController(KlonavimoDbContext db, TimeProvider laikas) : ValdiklisBase
{
    private const string PriskyrimoRaktas = "Priskyrimas";

    public async Task<IActionResult> Index(int? dnrPavyzdysId) => View(await SarasoModelis(dnrPavyzdysId));

    // PA „Sukurti inkubatorių“
    [HttpGet]
    public IActionResult Kurti() => View(new InkubatoriausKurimoForma());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Kurti(InkubatoriausKurimoForma forma)
    {
        if (!ModelState.IsValid) return View(forma);
        var numeris = forma.SerijinisNumeris.Trim();
        if (await db.Inkubatoriai.AnyAsync(i => i.SerijinisNumeris == numeris))
        {
            KlaidaLaukui(nameof(forma.SerijinisNumeris), "Inkubatorius su tokiu serijiniu numeriu jau užregistruotas.");
            return View(forma);
        }

        var inkubatorius = new Inkubatorius { Modelis = forma.Modelis.Trim(), SerijinisNumeris = numeris, Talpa = forma.Talpa!.Value };
        forma.Perkelti(inkubatorius);
        db.Inkubatoriai.Add(inkubatorius);
        await db.SaveChangesAsync();
        Sekme($"Inkubatorius {inkubatorius.Modelis} sukurtas.");
        return RedirectToAction(nameof(Index));
    }

    // PA „Peržiūrėti inkubatoriaus parametrus“
    public async Task<IActionResult> Parametrai(int id)
    {
        var inkubatorius = await db.Inkubatoriai.FindAsync(id);
        if (inkubatorius is null) return InkubatoriausNera();
        return View(new InkubatoriausParametruModelis
        {
            Inkubatorius = inkubatorius,
            Klonavimas = await AktyvusKlonavimas(id)
                .Include(k => k.Uzsakymas).ThenInclude(u => u.Augintinis)
                .Include(k => k.DnrPavyzdys)
                .Include(k => k.LaboratorijosSpecialistas)
                .FirstOrDefaultAsync(),
            Matavimai = await db.InkubatoriuMatavimai.Where(m => m.InkubatoriusId == id)
                .OrderByDescending(m => m.MatavimoData).ToListAsync()
        });
    }

    // PA „Redaguoti inkubatoriaus būseną“
    [HttpGet]
    public async Task<IActionResult> Busena(int id)
    {
        var inkubatorius = await db.Inkubatoriai.FindAsync(id);
        if (inkubatorius is null) return InkubatoriausNera();
        ViewData["Aprasas"] = $"{inkubatorius.Modelis} ({inkubatorius.SerijinisNumeris})";
        return View(InkubatoriausBusenosForma.Is(inkubatorius));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Busena(int id, InkubatoriausBusenosForma forma)
    {
        var inkubatorius = await db.Inkubatoriai.FindAsync(id);
        if (inkubatorius is null) return InkubatoriausNera();
        ViewData["Aprasas"] = $"{inkubatorius.Modelis} ({inkubatorius.SerijinisNumeris})";
        if (!ModelState.IsValid) return View(forma);

        var klonavimas = await AktyvusKlonavimas(id).FirstOrDefaultAsync();
        if (!forma.Busena && klonavimas is not null)
        {
            KlaidaLaukui(nameof(forma.Busena), $"Inkubatoriuje vykdomas klonavimas {klonavimas.Numeris}, todėl būsenos „Laisvas“ nustatyti negalima.");
            return View(forma);
        }

        inkubatorius.Busena = forma.Busena;
        forma.Perkelti(inkubatorius);
        await db.SaveChangesAsync();
        Sekme($"Inkubatoriaus {inkubatorius.Modelis} duomenys atnaujinti.");
        return RedirectToAction(nameof(Index));
    }

    // PA „Ištrinti inkubatorių“: tikrinama būsena – užimto inkubatoriaus ištrinti negalima
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Istrinti(int id)
    {
        var inkubatorius = await db.Inkubatoriai.FindAsync(id);
        if (inkubatorius is null) return InkubatoriausNera();
        var pavadinimas = $"{inkubatorius.Modelis} ({inkubatorius.SerijinisNumeris})";
        if (inkubatorius.Busena)
        {
            Klaida($"Inkubatorius {pavadinimas} užimtas, todėl jo ištrinti negalima.");
            return RedirectToAction(nameof(Index));
        }

        // Matavimai ištrinami kaskadiškai, baigtų klonavimų nuoroda tampa NULL
        db.Inkubatoriai.Remove(inkubatorius);
        await db.SaveChangesAsync();
        Sekme($"Inkubatorius {pavadinimas} ištrintas.");
        return RedirectToAction(nameof(Index));
    }

    // PA „Priskirti inkubatorių“: ciklas per inkubatorius, tinkamiausias priskiriamas DNR pavyzdžiui
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Priskirti(InkubatoriuSarasoModelis forma)
    {
        const string laukas = nameof(forma.DnrPavyzdysId);
        var dnrPavyzdysId = forma.DnrPavyzdysId;
        var pavyzdys = dnrPavyzdysId is null ? null : await db.DnrPavyzdziai
            .Include(d => d.Augintinis).ThenInclude(a => a.Uzsakymas)
            .Include(d => d.Klonavimas)
            .FirstOrDefaultAsync(d => d.Id == dnrPavyzdysId);
        if (pavyzdys is null)
            ModelState.AddModelError(laukas, "");
        else if (NegalimaPriskirti(pavyzdys) is string priezastis)
            KlaidaLaukui(laukas, $"Pavyzdžiui {pavyzdys.Kodas} inkubatoriaus priskirti negalima: {priezastis}.");
        if (!ModelState.IsValid) return View(nameof(Index), await SarasoModelis(dnrPavyzdysId));

        var tipas = pavyzdys!.Augintinis.Tipas;
        var rezultatas = InkubatoriausPriskyrimas.Ivertinti(tipas, await db.Inkubatoriai.OrderBy(i => i.Id).ToListAsync());
        var patikrinimai = rezultatas.Patikrinimai.Select(p => new PatikrinimoEilute(
            p.Inkubatorius.Id, p.Inkubatorius.Modelis, p.Inkubatorius.SerijinisNumeris, p.Inkubatorius.Busena,
            p.Inkubatorius.TikslineTemperatura, p.Inkubatorius.Co2Procentas, p.Inkubatorius.O2Procentas,
            p.Inkubatorius.PhLygis, p.Priezastys.ToList(), p.Ivertis)).ToList();

        var dabar = laikas.GetLocalNow().DateTime;
        var parinktas = rezultatas.Parinktas;
        Klonavimas? klonavimas = null;
        if (parinktas is not null)
        {
            var pradzia = DateOnly.FromDateTime(dabar);
            klonavimas = new Klonavimas
            {
                UzsakymasId = pavyzdys.Augintinis.Uzsakymas!.Id,
                DnrPavyzdysId = pavyzdys.Id,
                LaboratorijosSpecialistasId = NaudotojoId,
                InkubatoriusId = parinktas.Id,
                PradziosData = pradzia,
                PabaigosData = pradzia.AddDays(KlonavimoNormos.TrukmeDienomis[tipas]),
                Rezultatas = "Vyksta embriono inkubacija",
                Busena = true
            };
            db.Klonavimai.Add(klonavimas);
            parinktas.Busena = true;
            await db.SaveChangesAsync();
            Sekme($"Pavyzdžiui {pavyzdys.Kodas} priskirtas inkubatorius {parinktas.Modelis} ({parinktas.SerijinisNumeris}).");
        }
        else
        {
            Klaida($"Pavyzdžiui {pavyzdys.Kodas} tinkamų laisvų inkubatorių nerasta.");
        }

        TempData[PriskyrimoRaktas] = JsonSerializer.Serialize(
            new PriskyrimoIrasas(pavyzdys.Id, dabar, parinktas?.Id, klonavimas?.Id, patikrinimai));
        return RedirectToAction(nameof(PriskyrimoRezultatas), new { dnrPavyzdysId = pavyzdys.Id });
    }

    public async Task<IActionResult> PriskyrimoRezultatas(int? dnrPavyzdysId)
    {
        var irasas = TempData.Peek(PriskyrimoRaktas) is string json ? JsonSerializer.Deserialize<PriskyrimoIrasas>(json) : null;
        var pavyzdys = irasas is null || (dnrPavyzdysId is not null && dnrPavyzdysId != irasas.DnrPavyzdysId) ? null
            : await db.DnrPavyzdziai.Include(d => d.Augintinis).ThenInclude(a => a.Uzsakymas)
                .FirstOrDefaultAsync(d => d.Id == irasas.DnrPavyzdysId);
        if (pavyzdys is null)
        {
            ViewData["Posisteme"] = Posisteme.Inkubatoriai;
            return Nera("Priskyrimas dar neatliktas. Pasirinkite DNR pavyzdį inkubatorių lange.", "/inkubatoriai", "Į inkubatorių langą");
        }

        return View(new PriskyrimoRezultatoModelis
        {
            Irasas = irasas!,
            Pavyzdys = pavyzdys,
            Salygos = KlonavimoNormos.InkubavimoSalygos[pavyzdys.Augintinis.Tipas],
            Klonavimas = irasas!.KlonavimasId is int id ? await db.Klonavimai.FindAsync(id) : null,
            EsamiInkubatoriai = (await db.Inkubatoriai.Select(i => i.Id).ToListAsync()).ToHashSet()
        });
    }

    private IQueryable<Klonavimas> AktyvusKlonavimas(int inkubatoriausId) =>
        db.Klonavimai.Where(k => k.InkubatoriusId == inkubatoriausId && k.Busena);

    // Inkubatoriaus laukia tinkami klonuoti pavyzdžiai su patvirtintu užsakymu, kuriems dar nepradėtas klonavimas
    private IQueryable<DnrPavyzdys> LaukiaInkubatoriaus() => db.DnrPavyzdziai
        .Where(d => d.Tinkamumas == Tinkamumai.Tinkamas && d.Klonavimas == null
            && d.Augintinis.Uzsakymas != null && d.Augintinis.Uzsakymas.Busena);

    private static string? NegalimaPriskirti(DnrPavyzdys pavyzdys) =>
        pavyzdys.Klonavimas is not null ? $"jau pradėtas klonavimas {pavyzdys.Klonavimas.Numeris}"
        : pavyzdys.Tinkamumas != Tinkamumai.Tinkamas ? "pavyzdys neįvertintas kaip tinkamas klonuoti"
        : pavyzdys.Augintinis.Uzsakymas is not { Busena: true } ? "augintinio užsakymas nepatvirtintas"
        : null;

    private async Task<InkubatoriuSarasoModelis> SarasoModelis(int? dnrPavyzdysId)
    {
        var inkubatoriai = await db.Inkubatoriai
            .Include(i => i.Klonavimai.Where(k => k.Busena)).ThenInclude(k => k.Uzsakymas).ThenInclude(u => u.Augintinis)
            .OrderBy(i => i.Id).ToListAsync();
        var laukia = await LaukiaInkubatoriaus().Include(d => d.Augintinis).OrderBy(d => d.Id).ToListAsync();
        return new()
        {
            DnrPavyzdysId = dnrPavyzdysId,
            LaukiaInkubatoriaus = laukia,
            Inkubatoriai = inkubatoriai.Select(i => (i, i.Klonavimai.FirstOrDefault())).ToList(),
            Rodikliai =
            [
                new("Inkubatoriai", inkubatoriai.Count),
                new("Laisvi", inkubatoriai.Count(i => !i.Busena)),
                new("Užimti", inkubatoriai.Count(i => i.Busena)),
                new("DNR pavyzdžiai laukia inkubatoriaus", laukia.Count)
            ]
        };
    }

    private IActionResult InkubatoriausNera()
    {
        ViewData["Posisteme"] = Posisteme.Inkubatoriai;
        return Nera("Inkubatorius nerastas.", "/inkubatoriai", "Grįžti į sąrašą");
    }
}
