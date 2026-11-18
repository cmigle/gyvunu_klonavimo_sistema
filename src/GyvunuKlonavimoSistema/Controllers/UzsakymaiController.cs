using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Užsakymų valdymo valdiklis: klientas valdo savo užsakymus, specialistas peržiūri visus
[Authorize(Roles = $"{Roles.Klientas},{Roles.Specialistas}")]
public class UzsakymaiController(KlonavimoDbContext db) : ValdiklisBase
{
    private bool EsuKlientas => User.Role() == Roles.Klientas;

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
}
