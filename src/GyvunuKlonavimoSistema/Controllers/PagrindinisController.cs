using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// Svečiui – „Pradžios langas“, prisijungusiam – „Pagrindinis langas“ su jo rolės dalimis
public class PagrindinisController(KlonavimoDbContext db) : ValdiklisBase
{
    public async Task<IActionResult> Index() => View(User.Role() switch
    {
        Roles.Klientas => await KlientoLangas(),
        Roles.Specialistas => await SpecialistoLangas(),
        Roles.Administratorius => await AdministratoriausLangas(),
        _ => new PagrindinisModelis()
    });

    public IActionResult Klaida()
    {
        ViewData["Title"] = "Įvyko klaida";
        return View();
    }

    private async Task<PagrindinisModelis> KlientoLangas()
    {
        var klientas = await db.Klientai.FindAsync(NaudotojoId);
        var uzsakymai = await db.Uzsakymai.Include(u => u.Augintinis)
            .Where(u => u.Augintinis.KlientasId == NaudotojoId)
            .OrderByDescending(u => u.SukurimoData).ToListAsync();
        return new()
        {
            Vardas = klientas?.Vardas,
            Uzsakymai = uzsakymai,
            Rodikliai =
            [
                new("Augintiniai", await db.Augintiniai.CountAsync(a => a.KlientasId == NaudotojoId)),
                new("Užsakymai", uzsakymai.Count),
                new("Laukia patvirtinimo", uzsakymai.Count(u => !u.Busena)),
                new("Teistumo istorijos patikrinimas", klientas?.IstorijosBusena ?? "–")
            ]
        };
    }

    private async Task<PagrindinisModelis> SpecialistoLangas()
    {
        var inkubatoriai = await db.Inkubatoriai.ToListAsync();
        return new()
        {
            Vardas = (await db.Darbuotojai.FindAsync(NaudotojoId))?.Vardas,
            NaujausiDnr = await db.DnrPavyzdziai.Include(d => d.Augintinis)
                .OrderByDescending(d => d.PaemimoData).ThenByDescending(d => d.Id).Take(5).ToListAsync(),
            Rodikliai =
            [
                new("DNR pavyzdžiai", await db.DnrPavyzdziai.CountAsync()),
                new("Laukia tinkamumo nustatymo", await db.DnrPavyzdziai.CountAsync(d => d.Tinkamumas == null)),
                new("Laukia inkubatoriaus", await db.DnrPavyzdziai.CountAsync(d =>
                    d.Tinkamumas == Tinkamumai.Tinkamas && d.Klonavimas == null
                    && d.Augintinis.Uzsakymas != null && d.Augintinis.Uzsakymas.Busena)),
                new("Laisvi inkubatoriai", $"{inkubatoriai.Count(i => !i.Busena)} / {inkubatoriai.Count}")
            ]
        };
    }

    private async Task<PagrindinisModelis> AdministratoriausLangas()
    {
        var darbuotojai = await db.Darbuotojai.OrderBy(d => d.Id).ToListAsync();
        var specialistai = await db.LaboratorijosSpecialistai.Include(s => s.Klonavimai).ToListAsync();
        var kruviai = DarboKruvioAnalize.Analizuoti(specialistai);
        return new()
        {
            Vardas = darbuotojai.FirstOrDefault(d => d.Id == NaudotojoId)?.Vardas,
            Darbuotojai = darbuotojai.Select(d => (d, kruviai.FirstOrDefault(k => k.Specialistas.Id == d.Id))).ToList(),
            Rodikliai =
            [
                new("Darbuotojai", darbuotojai.Count),
                new("Laboratorijos specialistai", specialistai.Count),
                new("Aktyvūs klonavimai", await db.Klonavimai.CountAsync(k => k.Busena)),
                new("Didelis darbo krūvis", kruviai.Count(k => k.Kruvis > 100))
            ]
        };
    }
}
