using System.Security.Claims;
using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Prisijungimo valdiklis (3.1.29 pav.): tikrina duomenis pagal esybes „Klientas“ ir „Darbuotojas“
public class PrisijungimasController(KlonavimoDbContext db, IPasswordHasher<object> maisa) : ValdiklisBase
{
    [HttpGet]
    public IActionResult Index(string? elPastas) => View(new PrisijungimoForma { ElPastas = elPastas ?? "" });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PrisijungimoForma forma, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(forma);

        var paskyra = await RastiPaskyra(forma.ElPastas.Trim(), forma.Slaptazodis);
        if (paskyra is null)
        {
            KlaidaLaukui(nameof(forma.ElPastas), "Neteisingi prisijungimo duomenys.");
            ModelState.AddModelError(nameof(forma.Slaptazodis), "");
            return View(forma);
        }
        if (paskyra is { Role: Roles.Specialistas, Aktyvus: false })
        {
            KlaidaLaukui(nameof(forma.ElPastas), "Paskyra neaktyvi. Kreipkitės į laboratorijos administratorių.");
            return View(forma);
        }

        var tapatybe = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, paskyra.Id.ToString()),
            new Claim(ClaimTypes.Name, paskyra.VardasPavarde),
            new Claim(ClaimTypes.Role, paskyra.Role)
        ], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(tapatybe));

        Sekme("Sėkmingai prisijungta.");
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Pagrindinis");
    }

    // Langas, skirtas kitai rolei
    public IActionResult Draudziama()
    {
        ViewData["Title"] = "Prieiga negalima";
        return View();
    }

    private record Paskyra(int Id, string VardasPavarde, string Role, bool Aktyvus);

    private async Task<Paskyra?> RastiPaskyra(string elPastas, string slaptazodis)
    {
        var klientas = await db.Klientai.FirstOrDefaultAsync(k => k.ElPastas == elPastas);
        if (klientas is not null)
            return Tinka(klientas.Slaptazodis, slaptazodis) ? new(klientas.Id, klientas.VardasPavarde, Roles.Klientas, true) : null;

        var darbuotojas = await db.Darbuotojai.FirstOrDefaultAsync(d => d.ElPastas == elPastas);
        if (darbuotojas is null || !Tinka(darbuotojas.Slaptazodis, slaptazodis)) return null;
        return darbuotojas is LaboratorijosSpecialistas s
            ? new(s.Id, s.VardasPavarde, Roles.Specialistas, s.Busena)
            : new(darbuotojas.Id, darbuotojas.VardasPavarde, Roles.Administratorius, true);
    }

    private bool Tinka(string maisosReiksme, string slaptazodis) =>
        maisa.VerifyHashedPassword(new object(), maisosReiksme, slaptazodis) != PasswordVerificationResult.Failed;
}
