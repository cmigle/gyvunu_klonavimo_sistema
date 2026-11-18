using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GyvunuKlonavimoSistema.Controllers;

// Bendri visų valdiklių veiksmai: pranešimai kitame lange ir klaidingų laukų žymėjimas
public abstract class ValdiklisBase : Controller
{
    protected int NaudotojoId => User.Id();

    protected void Sekme(string tekstas) => TempData["Sekme"] = tekstas;
    protected void Klaida(string tekstas) => TempData["Klaida"] = tekstas;
    protected void Info(string tekstas) => TempData["Info"] = tekstas;

    // Laukas pažymimas formoje, o pranešime rodomas nurodytas tekstas
    protected void KlaidaLaukui(string laukas, string tekstas)
    {
        ModelState.AddModelError(laukas, tekstas);
        ModelState.AddModelError("", tekstas);
    }

    // Įrašo nėra arba jis nepasiekiamas prisijungusiam naudotojui
    protected IActionResult Nera(string tekstas, string atgal, string atgalTekstas)
    {
        ViewData["Title"] = "Įrašas nerastas";
        return View("Nera", new NeraModelis(tekstas, atgal, atgalTekstas));
    }
}
