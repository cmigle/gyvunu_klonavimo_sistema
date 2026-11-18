using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Augintinio valdiklis (3.1.8–3.1.11 pav.): klientas valdo tik savo augintinius
[Authorize(Roles = Roles.Klientas)]
public class AugintiniaiController(KlonavimoDbContext db, DokumentuSaugykla saugykla) : ValdiklisBase
{
    private IQueryable<Augintinis> Mano => db.Augintiniai.Where(a => a.KlientasId == NaudotojoId);

    public async Task<IActionResult> Index()
    {
        var augintiniai = await Mano.Include(a => a.Dokumentai).Include(a => a.Uzsakymas).Include(a => a.DnrPavyzdys)
            .OrderBy(a => a.Id).ToListAsync();
        // Kliento augintinių informacijos suvestinė
        return View(new AugintiniuSarasoModelis
        {
            Augintiniai = augintiniai,
            Rodikliai =
            [
                new("Augintiniai", augintiniai.Count),
                new("Dokumentai", augintiniai.Sum(a => a.Dokumentai.Count)),
                new("Su užsakymu", augintiniai.Count(a => a.Uzsakymas is not null)),
                new("Tinkami DNR pavyzdžiai", augintiniai.Count(a => a.DnrPavyzdys?.Tinkamumas == Tinkamumai.Tinkamas))
            ]
        });
    }

    [HttpGet]
    public IActionResult Kurti() => View(new AugintinioForma());

    // 2.10 pav. PA „Sukurti gyvūną“
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Kurti(AugintinioForma forma)
    {
        await Tikrinti(forma, null);
        if (!ModelState.IsValid) return View(forma);
        var a = new Augintinis { KlientasId = NaudotojoId };
        forma.Perkelti(a);
        db.Augintiniai.Add(a);
        await db.SaveChangesAsync();
        Sekme($"Augintinis „{a.Vardas}“ sukurtas.");
        return RedirectToAction(nameof(Perziura), new { id = a.Id });
    }

    public async Task<IActionResult> Perziura(int id)
    {
        var a = await Mano.Include(x => x.Klientas).Include(x => x.Dokumentai).Include(x => x.DnrPavyzdys)
            .Include(x => x.Uzsakymas).ThenInclude(u => u!.Klonavimas)
            .FirstOrDefaultAsync(x => x.Id == id);
        return a is null ? Nera("Augintinis nerastas.", Url.Action(nameof(Index))!, "Grįžti į sąrašą") : View(a);
    }

    [HttpGet]
    public async Task<IActionResult> Redaguoti(int id)
    {
        var a = await Mano.FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return Nera("Augintinis nerastas.", Url.Action(nameof(Index))!, "Grįžti į sąrašą");
        ViewData["Augintinis"] = a;
        return View(AugintinioForma.Is(a));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Redaguoti(int id, AugintinioForma forma)
    {
        var a = await Mano.Include(x => x.Uzsakymas).FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return Nera("Augintinis nerastas.", Url.Action(nameof(Index))!, "Grįžti į sąrašą");
        await Tikrinti(forma, a.Id);
        if (!ModelState.IsValid)
        {
            ViewData["Augintinis"] = a;
            return View(forma);
        }
        forma.Perkelti(a);
        // Nepatvirtinto užsakymo kaina perskaičiuojama pagal gyvūno tipą
        if (a.Uzsakymas is { Busena: false } u) u.Kaina = KlonavimoNormos.Kainos[a.Tipas];
        await db.SaveChangesAsync();
        Sekme("Augintinio duomenys atnaujinti.");
        return RedirectToAction(nameof(Perziura), new { id });
    }

    // 3.1.11 pav.: ištrinant augintinį pašalinami ir jo dokumentai
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Istrinti(int id)
    {
        var a = await Mano.Include(x => x.Uzsakymas).Include(x => x.Dokumentai).FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return Nera("Augintinis nerastas.", Url.Action(nameof(Index))!, "Grįžti į sąrašą");
        if (a.Uzsakymas is { Busena: true } u)
        {
            Klaida($"Augintinio „{a.Vardas}“ ištrinti negalima, nes jo užsakymas {u.Numeris} jau patvirtintas.");
            return RedirectToAction(nameof(Index));
        }
        // Dokumentai, nepatvirtintas užsakymas ir DNR pavyzdys ištrinami kaskadiškai
        foreach (var d in a.Dokumentai) saugykla.Istrinti(d.Id);
        await db.Augintiniai.Where(x => x.Id == a.Id).ExecuteDeleteAsync();
        Sekme($"Augintinis „{a.Vardas}“ ištrintas.");
        return RedirectToAction(nameof(Index));
    }

    private async Task Tikrinti(AugintinioForma forma, int? augintinisId)
    {
        if (!ModelState.IsValid) return;
        if (!Lytys.Visos.Contains(forma.Lytis))
            ModelState.AddModelError(nameof(forma.Lytis), "");
        else if (await db.Augintiniai.AnyAsync(a => a.MikroschemosNumeris == forma.MikroschemosNumeris && a.Id != augintinisId))
            KlaidaLaukui(nameof(forma.MikroschemosNumeris), "Augintinis su tokiu mikroschemos numeriu jau užregistruotas.");
    }
}
