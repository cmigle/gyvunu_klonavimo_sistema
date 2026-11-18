using System.Security.Claims;
using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Kliento valdiklis: registracija (3.1.27 pav.), atsijungimas (3.1.28 pav.) ir kliento profilis
public class KlientasController(KlonavimoDbContext db, IPasswordHasher<object> maisa, DokumentuSaugykla saugykla) : ValdiklisBase
{
    [Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Profilis()
    {
        var k = await db.Klientai.Include(x => x.Augintiniai).ThenInclude(a => a.Uzsakymas)
            .Include(x => x.Augintiniai).ThenInclude(a => a.Dokumentai)
            .FirstAsync(x => x.Id == NaudotojoId);
        return View(new ProfilioModelis
        {
            Klientas = k,
            GalimaIstrinti = !k.Augintiniai.Any(a => a.Uzsakymas?.Busena == true),
            Rodikliai =
            [
                new("Augintiniai", k.Augintiniai.Count),
                new("Užsakymai", k.Augintiniai.Count(a => a.Uzsakymas is not null)),
                new("Dokumentai", k.Augintiniai.Sum(a => a.Dokumentai.Count))
            ]
        });
    }

    [HttpGet, Authorize(Roles = Roles.Klientas)]
    public async Task<IActionResult> Redaguoti()
    {
        var k = await db.Klientai.FirstAsync(x => x.Id == NaudotojoId);
        return View(new ProfilioForma
        {
            Vardas = k.Vardas, Pavarde = k.Pavarde, ElPastas = k.ElPastas,
            TelefonoNumeris = k.TelefonoNumeris, AsmensKodas = k.AsmensKodas.ToString()
        });
    }

    [HttpPost, Authorize(Roles = Roles.Klientas), ValidateAntiForgeryToken]
    public async Task<IActionResult> Redaguoti(ProfilioForma forma)
    {
        if (!ModelState.IsValid) return View(forma);
        var k = await db.Klientai.FirstAsync(x => x.Id == NaudotojoId);
        var elPastas = forma.ElPastas.Trim();
        var asmensKodas = long.Parse(forma.AsmensKodas);
        if (await ElPastasUzimtas(elPastas, k.Id))
            KlaidaLaukui(nameof(forma.ElPastas), "Toks el. paštas jau naudojamas kitos paskyros.");
        else if (await db.Klientai.AnyAsync(x => x.AsmensKodas == asmensKodas && x.Id != k.Id))
            KlaidaLaukui(nameof(forma.AsmensKodas), "Klientas su tokiu asmens kodu jau užregistruotas.");
        if (!ModelState.IsValid) return View(forma);

        // Pasikeitus asmens duomenims, ankstesnis istorijos patikrinimas nebegalioja
        var pasikeite = (forma.Vardas.Trim() != k.Vardas || forma.Pavarde.Trim() != k.Pavarde || asmensKodas != k.AsmensKodas)
            && k.IstorijosBusena != IstorijosBusenos.Nepatikrintas;
        k.Vardas = forma.Vardas.Trim();
        k.Pavarde = forma.Pavarde.Trim();
        k.ElPastas = elPastas;
        k.TelefonoNumeris = forma.TelefonoNumeris.Trim();
        k.AsmensKodas = asmensKodas;
        if (pasikeite)
        {
            k.IstorijosBusena = IstorijosBusenos.Nepatikrintas;
            k.PatikrinimoData = null;
        }
        await db.SaveChangesAsync();
        await AtnaujintiVarda(k.VardasPavarde);

        Sekme(pasikeite
            ? "Profilio duomenys atnaujinti. Pasikeitus asmens duomenims, teistumo istoriją reikia patikrinti iš naujo."
            : "Profilio duomenys atnaujinti.");
        return RedirectToAction(nameof(Profilis));
    }

    [HttpPost, Authorize(Roles = Roles.Klientas), ValidateAntiForgeryToken]
    public async Task<IActionResult> Istrinti()
    {
        var k = await db.Klientai.Include(x => x.Augintiniai).ThenInclude(a => a.Uzsakymas)
            .Include(x => x.Augintiniai).ThenInclude(a => a.Dokumentai)
            .FirstAsync(x => x.Id == NaudotojoId);
        if (k.Augintiniai.Any(a => a.Uzsakymas?.Busena == true))
        {
            Klaida("Paskyros ištrinti negalima, nes turite patvirtintų užsakymų.");
            return RedirectToAction(nameof(Profilis));
        }

        // Augintiniai, jų dokumentai, nepatvirtinti užsakymai ir DNR pavyzdžiai ištrinami kaskadiškai
        foreach (var d in k.Augintiniai.SelectMany(a => a.Dokumentai)) saugykla.Istrinti(d.Id);
        await db.Klientai.Where(x => x.Id == k.Id).ExecuteDeleteAsync();
        await HttpContext.SignOutAsync();
        Sekme("Paskyra ištrinta.");
        return RedirectToAction("Index", "Prisijungimas");
    }

    // Pakeitus vardą ar pavardę, atnaujinamas meniu rodomas prisijungusio naudotojo vardas
    private async Task AtnaujintiVarda(string vardasPavarde)
    {
        if (User.Identity?.Name == vardasPavarde) return;
        var tapatybe = new ClaimsIdentity(
            User.Claims.Where(c => c.Type != ClaimTypes.Name).Append(new Claim(ClaimTypes.Name, vardasPavarde)),
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(tapatybe));
    }

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
