using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Dokumentų valdiklis (3.1.12–3.1.15 pav.). Klientas valdo savo augintinių dokumentus,
// laboratorijos specialistas (užsakymų valdytojas) juos tik peržiūri per užsakymus.
[Authorize(Roles = Roles.Klientas + "," + Roles.Specialistas)]
public class DokumentaiController(KlonavimoDbContext db, DokumentuSaugykla saugykla, TimeProvider laikas) : ValdiklisBase
{
    private bool EsuKlientas => User.Role() == Roles.Klientas;
    private DateOnly Siandien => DateOnly.FromDateTime(laikas.GetLocalNow().DateTime);

    // Klientui – savi augintiniai, specialistui – augintiniai, turintys užsakymą
    private IQueryable<Augintinis> Pasiekiami => EsuKlientas
        ? db.Augintiniai.Where(a => a.KlientasId == NaudotojoId)
        : db.Augintiniai.Where(a => a.Uzsakymas != null);

    private IActionResult NeraAugintinio() => EsuKlientas
        ? Nera("Augintinis nerastas. Pirmiausia sukurkite augintinį.", "/augintiniai", "Grįžti")
        : Nera("Augintinis nerastas.", "/uzsakymai", "Grįžti");

    // 2.5 pav. PA „Peržiūrėti dokumentus“
    public async Task<IActionResult> Index(int augintinisId)
    {
        var augintiniai = await Pasiekiami.Include(a => a.Klientas).Include(a => a.Uzsakymas).OrderBy(a => a.Id).ToListAsync();
        var a = augintinisId == 0 ? augintiniai.FirstOrDefault() : augintiniai.FirstOrDefault(x => x.Id == augintinisId);
        if (a is null) return NeraAugintinio();
        return View(new DokumentuSarasoModelis
        {
            Augintinis = a,
            Augintiniai = augintiniai,
            Dokumentai = await db.Dokumentai.Where(d => d.AugintinisId == a.Id).OrderBy(d => d.Id).ToListAsync(),
            Klientas = EsuKlientas,
            Siandien = Siandien
        });
    }

    [HttpGet, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Ikelti(int augintinisId)
    {
        var a = await Pasiekiami.FirstOrDefaultAsync(x => x.Id == augintinisId);
        if (a is null) return NeraAugintinio();
        ViewData["Augintinis"] = a;
        return View(new DokumentoIkelimoForma());
    }

    // 2.6 pav. PA „Įkelti dokumentą“: tikrinamas failo formatas ir duomenys
    [HttpPost, Authorize(Roles = Roles.Klientas), ValidateAntiForgeryToken, RequestSizeLimit(DokumentuSaugykla.DidziausiasDydis + 1024 * 1024)]
    public async Task<IActionResult> Ikelti(int augintinisId, DokumentoIkelimoForma forma)
    {
        var a = await Pasiekiami.FirstOrDefaultAsync(x => x.Id == augintinisId);
        if (a is null) return NeraAugintinio();
        var formatas = Tikrinti(forma, forma.Failas, nameof(forma.Failas));
        if (!ModelState.IsValid)
        {
            ViewData["Augintinis"] = a;
            return View(forma);
        }

        var d = new Dokumentas
        {
            AugintinisId = a.Id,
            FailoPavadinimas = forma.FailoPavadinimas.Trim(),
            DokumentoTipas = forma.DokumentoTipas,
            GaliojimoData = forma.GaliojimoData!.Value,
            IkelimoData = Siandien,
            FailoFormatas = formatas!
        };
        db.Dokumentai.Add(d);
        await db.SaveChangesAsync();
        await saugykla.IssaugotiAsync(d, forma.Failas!);
        Sekme("Dokumentas sėkmingai įkeltas.");
        return RedirectToAction(nameof(Index), new { augintinisId = a.Id });
    }

    public async Task<IActionResult> Perziura(int id)
    {
        var d = await RastiDokumenta(id);
        if (d is null) return Nera("Dokumentas nerastas.", "/dokumentai", "Grįžti į sąrašą");
        return View(new DokumentoPerziurosModelis { Dokumentas = d, Klientas = EsuKlientas, FailasYra = saugykla.Yra(d), Siandien = Siandien });
    }

    // Įkelto failo turinys rodomas naršyklėje
    public async Task<IActionResult> Failas(int id)
    {
        var d = await RastiDokumenta(id);
        if (d is null || !saugykla.Yra(d)) return NotFound();
        var pavadinimas = d.FailoPavadinimas.EndsWith(DokumentuSaugykla.Pletinys(d.FailoFormatas), StringComparison.OrdinalIgnoreCase)
            ? d.FailoPavadinimas : d.FailoPavadinimas + DokumentuSaugykla.Pletinys(d.FailoFormatas);
        Response.Headers.ContentDisposition = $"inline; filename*=UTF-8''{Uri.EscapeDataString(pavadinimas)}";
        return PhysicalFile(saugykla.Kelias(d), FailuFormatai.TurinioTipas(d.FailoFormatas));
    }

    [HttpGet, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Redaguoti(int id)
    {
        var d = await RastiDokumenta(id);
        if (d is null) return Nera("Dokumentas nerastas.", "/dokumentai", "Grįžti į sąrašą");
        ViewData["Dokumentas"] = d;
        return View(new DokumentoRedagavimoForma
        {
            FailoPavadinimas = d.FailoPavadinimas, DokumentoTipas = d.DokumentoTipas, GaliojimoData = d.GaliojimoData
        });
    }

    // 2.7 pav. PA „Redaguoti dokumentą“
    [HttpPost, Authorize(Roles = Roles.Klientas), ValidateAntiForgeryToken, RequestSizeLimit(DokumentuSaugykla.DidziausiasDydis + 1024 * 1024)]
    public async Task<IActionResult> Redaguoti(int id, DokumentoRedagavimoForma forma)
    {
        var d = await RastiDokumenta(id);
        if (d is null) return Nera("Dokumentas nerastas.", "/dokumentai", "Grįžti į sąrašą");
        var formatas = Tikrinti(forma, forma.Failas, nameof(forma.Failas));
        if (!ModelState.IsValid)
        {
            ViewData["Dokumentas"] = d;
            return View(forma);
        }

        d.FailoPavadinimas = forma.FailoPavadinimas.Trim();
        d.DokumentoTipas = forma.DokumentoTipas;
        d.GaliojimoData = forma.GaliojimoData!.Value;
        if (forma.Failas is not null)
        {
            d.FailoFormatas = formatas!;
            d.IkelimoData = Siandien;
            await saugykla.IssaugotiAsync(d, forma.Failas);
        }
        await db.SaveChangesAsync();
        Sekme("Dokumentas atnaujintas.");
        return RedirectToAction(nameof(Perziura), new { id });
    }

    // 2.8 pav. PA „Ištrinti dokumentą“
    [HttpPost, Authorize(Roles = Roles.Klientas), ValidateAntiForgeryToken]
    public async Task<IActionResult> Istrinti(int id)
    {
        var d = await RastiDokumenta(id);
        if (d is null) return Nera("Dokumentas nerastas.", "/dokumentai", "Grįžti į sąrašą");
        db.Dokumentai.Remove(d);
        await db.SaveChangesAsync();
        saugykla.Istrinti(d.Id);
        Sekme("Dokumentas sėkmingai ištrintas.");
        return RedirectToAction(nameof(Index), new { augintinisId = d.AugintinisId });
    }

    private Task<Dokumentas?> RastiDokumenta(int id)
    {
        var pasiekiami = Pasiekiami;
        return db.Dokumentai.Include(d => d.Augintinis).ThenInclude(a => a.Uzsakymas)
            .Where(d => pasiekiami.Any(a => a.Id == d.AugintinisId))
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    // Grąžina failo formatą (PDF, JPG, PNG), jei failas pateiktas ir tinkamas
    private string? Tikrinti(DokumentoForma forma, IFormFile? failas, string failoLaukas)
    {
        if (!string.IsNullOrEmpty(forma.DokumentoTipas) && !DokumentuTipai.Visi.Contains(forma.DokumentoTipas))
            ModelState.AddModelError(nameof(forma.DokumentoTipas), "");
        if (failas is null || !ModelState.IsValid) return null;
        if (!FailuFormatai.PagalPletini.TryGetValue(Path.GetExtension(failas.FileName), out var formatas))
            KlaidaLaukui(failoLaukas, "Netinkamas failo formatas. Leidžiami formatai: PDF, JPG, PNG.");
        else if (failas.Length > DokumentuSaugykla.DidziausiasDydis)
            KlaidaLaukui(failoLaukas, "Failas per didelis. Leidžiama iki 10 MB.");
        else if (failas.Length == 0)
            KlaidaLaukui(failoLaukas, "Failas tuščias.");
        return formatas;
    }
}
