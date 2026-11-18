using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Kliento valdiklis: registracija (3.1.27 pav.) ir atsijungimas (3.1.28 pav.)
public class KlientasController(KlonavimoDbContext db, IPasswordHasher<object> maisa) : ValdiklisBase
{
    [HttpGet]
    public IActionResult Registracija() => View(new RegistracijosForma());

    // 2.2 pav. PA „Registracija“
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registracija(RegistracijosForma forma)
    {
        if (!ModelState.IsValid) return View(forma);

        var elPastas = forma.ElPastas.Trim();
        var asmensKodas = long.Parse(forma.AsmensKodas);
        if (await ElPastasUzimtas(elPastas))
            KlaidaLaukui(nameof(forma.ElPastas), "Toks el. paštas jau užregistruotas.");
        else if (await db.Klientai.AnyAsync(k => k.AsmensKodas == asmensKodas))
            KlaidaLaukui(nameof(forma.AsmensKodas), "Klientas su tokiu asmens kodu jau užregistruotas.");
        else if (forma.Slaptazodis != forma.Slaptazodis2)
            KlaidaLaukui(nameof(forma.Slaptazodis2), "Slaptažodžiai nesutampa.");
        if (!ModelState.IsValid) return View(forma);

        db.Klientai.Add(new Klientas
        {
            Vardas = forma.Vardas.Trim(),
            Pavarde = forma.Pavarde.Trim(),
            ElPastas = elPastas,
            TelefonoNumeris = forma.TelefonoNumeris.Trim(),
            AsmensKodas = asmensKodas,
            Slaptazodis = maisa.HashPassword(new object(), forma.Slaptazodis)
        });
        await db.SaveChangesAsync();

        Sekme("Paskyra sukurta. Galite prisijungti.");
        return RedirectToAction("Index", "Prisijungimas", new { elPastas });
    }

    // 2.4 pav. PA „Atsijungimas“
    [HttpGet, Authorize]
    public IActionResult Atsijungimas() => View();

    [HttpPost, Authorize, ValidateAntiForgeryToken, ActionName("Atsijungimas")]
    public async Task<IActionResult> Atsijungti()
    {
        await HttpContext.SignOutAsync();
        Sekme("Sėkmingai atsijungta.");
        return RedirectToAction("Index", "Prisijungimas");
    }

    // El. paštas unikalus tarp klientų ir darbuotojų, nes pagal jį prisijungiama
    private async Task<bool> ElPastasUzimtas(string elPastas, int? isskyrusKlienta = null) =>
        await db.Klientai.AnyAsync(k => k.ElPastas == elPastas && k.Id != isskyrusKlienta)
        || await db.Darbuotojai.AnyAsync(d => d.ElPastas == elPastas);
}
