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

// «control» Istorijos patikrinimo valdiklis (3.1.16 pav.): per «boundary» CourtListener API sąsają
// ieško teistumo įrašų ir atnaujina esybę „Klientas“
[Authorize(Roles = Roles.Klientas)]
public class IstorijosPatikrinimasController(KlonavimoDbContext db, ICourtListenerSasaja courtListener,
    TimeProvider laikas, ILogger<IstorijosPatikrinimasController> log) : ValdiklisBase
{
    private const string RezultatoRaktas = "IstorijosRezultatas";

    [HttpGet]
    public async Task<IActionResult> Index(string? grizti)
    {
        var k = await db.Klientai.FirstAsync(x => x.Id == NaudotojoId);
        return View(new IstorijosForma
        {
            Vardas = k.Vardas, Pavarde = k.Pavarde, AsmensKodas = k.AsmensKodas.ToString(), Grizti = grizti
        });
    }

    // 2.9 pav. PA „Patikrinti kliento istoriją“
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IstorijosForma forma)
    {
        if (!ModelState.IsValid) return View(forma);
        var k = await db.Klientai.FirstAsync(x => x.Id == NaudotojoId);
        if (forma.AsmensKodas != k.AsmensKodas.ToString())
        {
            KlaidaLaukui(nameof(forma.AsmensKodas), "Asmens kodas nesutampa su profilio duomenimis.");
            return View(forma);
        }

        IstorijosPaieska paieska;
        try
        {
            paieska = await courtListener.IeskotiAsync(forma.Vardas, forma.Pavarde, HttpContext.RequestAborted);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            log.LogWarning(e, "CourtListener API užklausa nepavyko");
            ModelState.AddModelError("", "CourtListener API šiuo metu nepasiekiamas. Bandykite vėliau.");
            return View(forma);
        }

        k.IstorijosBusena = paieska.Irasai.Count > 0 ? IstorijosBusenos.Netinkamas : IstorijosBusenos.Patikrintas;
        k.PatikrinimoData = laikas.GetLocalNow().DateTime;
        await db.SaveChangesAsync();

        TempData[RezultatoRaktas] = JsonSerializer.Serialize(paieska);
        if (paieska.Irasai.Count > 0) Klaida("Rasta teistumo įrašų, todėl klientas netinkamas.");
        else Sekme("Patikrinimas sėkmingas: teistumo įrašų nerasta. Esate pažymėti kaip patikrintas klientas.");
        return RedirectToAction(nameof(Rezultatas), new { grizti = forma.Grizti });
    }

    public async Task<IActionResult> Rezultatas(string? grizti)
    {
        var k = await db.Klientai.FirstAsync(x => x.Id == NaudotojoId);
        if (k.PatikrinimoData is null)
            return Nera("Teistumo istorija dar netikrinta.", Url.Action(nameof(Index), new { grizti })!, "Patikrinti istoriją");

        // Atnaujinus langą paieškos rezultatų nebėra – rodomi paskutinio patikrinimo duomenys
        var paieska = TempData[RezultatoRaktas] is string json ? JsonSerializer.Deserialize<IstorijosPaieska>(json) : null;
        return View(new IstorijosRezultatoModelis
        {
            Klientas = k,
            Asmuo = paieska?.Asmuo ?? k.VardasPavarde,
            Uzklausa = paieska?.Uzklausa ?? "GET " + CourtListener.Adresas + CourtListener.Uzklausa(k.VardasPavarde),
            RastaIrasu = paieska?.Irasai.Count ?? (k.IstorijosBusena == IstorijosBusenos.Patikrintas ? 0 : null),
            Irasai = paieska?.Irasai ?? [],
            Grizti = grizti
        });
    }
}
