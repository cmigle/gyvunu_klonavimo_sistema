using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using GyvunuKlonavimoSistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Controllers;

// «control» Darbuotojo valdiklis
[Authorize(Roles = Roles.Administratorius)]
public class DarbuotojaiController(KlonavimoDbContext db, IPasswordHasher<object> maisa, TimeProvider laikas) : ValdiklisBase
{
    public async Task<IActionResult> Index()
    {
        var darbuotojai = await db.Darbuotojai.OrderBy(d => d.Id).ToListAsync();
        return View(new DarbuotojuSarasoModelis
        {
            Darbuotojai = darbuotojai,
            Rodikliai =
            [
                new("Darbuotojai", darbuotojai.Count),
                new("Laboratorijos specialistai", darbuotojai.Count(d => d is LaboratorijosSpecialistas)),
                new("Administratoriai", darbuotojai.Count(d => d is not LaboratorijosSpecialistas)),
                new("Aktyvūs klonavimai", await db.Klonavimai.CountAsync(k => k.Busena))
            ]
        });
    }

    // 2.21 pav. PA „Sukurti darbuotoją“
    [HttpGet]
    public IActionResult Kurti() => View(new DarbuotojoForma());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Kurti(DarbuotojoForma forma)
    {
        if (forma.Role is not (Roles.Specialistas or Roles.Administratorius)) ModelState.AddModelError(nameof(forma.Role), "");
        if (string.IsNullOrEmpty(forma.Slaptazodis)) ModelState.AddModelError(nameof(forma.Slaptazodis), "");
        if (!await Tikrinti(forma, null)) return View(forma);

        Darbuotojas darbuotojas = forma.Specialistas
            ? new LaboratorijosSpecialistas { LicencijosNumeris = forma.LicencijosNumeris!.Trim(), Busena = forma.Busena }
            : new Darbuotojas();
        Perkelti(forma, darbuotojas);
        db.Darbuotojai.Add(darbuotojas);
        await db.SaveChangesAsync();
        Sekme($"Darbuotojas {darbuotojas.VardasPavarde} sėkmingai sukurtas.");
        return RedirectToAction(nameof(Index));
    }

    // 2.22 pav. PA „Peržiūrėti darbuotoją“
    public async Task<IActionResult> Perziura(int id)
    {
        var darbuotojas = await db.Darbuotojai.FindAsync(id);
        if (darbuotojas is null) return DarbuotojoNera();
        SpecialistoKruvis? kruvis = null;
        if (darbuotojas is LaboratorijosSpecialistas s)
        {
            await db.Entry(s).Collection(x => x.Klonavimai).LoadAsync();
            kruvis = DarboKruvioAnalize.Apskaiciuoti(s);
        }
        return View(new DarbuotojoPerziurosModelis { Darbuotojas = darbuotojas, Kruvis = kruvis });
    }

    // 2.23 pav. PA „Redaguoti darbuotoją“: rolė nekeičiama, slaptažodis keičiamas tik įvedus naują
    [HttpGet]
    public async Task<IActionResult> Redaguoti(int id)
    {
        var darbuotojas = await db.Darbuotojai.FindAsync(id);
        if (darbuotojas is null) return DarbuotojoNera();
        ViewData["Aprasas"] = $"Redaguojami darbuotojo {darbuotojas.VardasPavarde} duomenys.";
        return View(DarbuotojoForma.Is(darbuotojas));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Redaguoti(int id, DarbuotojoForma forma)
    {
        var darbuotojas = await db.Darbuotojai.FindAsync(id);
        if (darbuotojas is null) return DarbuotojoNera();
        ViewData["Aprasas"] = $"Redaguojami darbuotojo {darbuotojas.VardasPavarde} duomenys.";
        forma.Role = DarbuotojuRoles.Role(darbuotojas);
        if (!await Tikrinti(forma, id)) return View(forma);

        Perkelti(forma, darbuotojas);
        if (darbuotojas is LaboratorijosSpecialistas s)
        {
            s.LicencijosNumeris = forma.LicencijosNumeris!.Trim();
            s.Busena = forma.Busena;
        }
        await db.SaveChangesAsync();
        Sekme($"Darbuotojo {darbuotojas.VardasPavarde} duomenys sėkmingai atnaujinti.");
        return RedirectToAction(nameof(Index));
    }

    // 2.24 pav. PA „Ištrinti darbuotoją“: ištrynus specialistą jo klonavimai lieka be specialisto
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Istrinti(int id)
    {
        var darbuotojas = await db.Darbuotojai.FindAsync(id);
        if (darbuotojas is null) return DarbuotojoNera();
        if (darbuotojas.Id == NaudotojoId)
        {
            Klaida("Negalite ištrinti savo paskyros.");
            return RedirectToAction(nameof(Index));
        }

        db.Darbuotojai.Remove(darbuotojas);
        await db.SaveChangesAsync();
        Sekme($"Darbuotojas {darbuotojas.VardasPavarde} sėkmingai ištrintas.");
        return RedirectToAction(nameof(Index));
    }

    // 2.25 pav. PA „Peržiūrėti darbuotojo tyrimus“
    public async Task<IActionResult> Tyrimai(int? id)
    {
        var specialistai = await db.LaboratorijosSpecialistai.OrderBy(s => s.Id).ToListAsync();
        var darbuotojas = id is null ? specialistai.FirstOrDefault() : await db.Darbuotojai.FindAsync(id);
        if (darbuotojas is null) return DarbuotojoNera();
        return View(new DarbuotojoTyrimuModelis
        {
            Darbuotojas = darbuotojas,
            Specialistai = specialistai,
            Tyrimai = await db.Klonavimai.Include(k => k.Uzsakymas).ThenInclude(u => u.Augintinis)
                .Where(k => k.LaboratorijosSpecialistasId == darbuotojas.Id)
                .OrderByDescending(k => k.PradziosData).ToListAsync()
        });
    }

    public async Task<IActionResult> Tyrimas(int id)
    {
        var klonavimas = await db.Klonavimai
            .Include(k => k.Uzsakymas).ThenInclude(u => u.Augintinis).ThenInclude(a => a.Klientas)
            .Include(k => k.DnrPavyzdys)
            .Include(k => k.Inkubatorius)
            .Include(k => k.LaboratorijosSpecialistas)
            .FirstOrDefaultAsync(k => k.Id == id);
        if (klonavimas is null)
        {
            ViewData["Posisteme"] = Posisteme.Darbuotojai;
            return Nera("Tyrimas nerastas.", "/darbuotojai", "Grįžti į darbuotojų sąrašą");
        }
        return View(klonavimas);
    }

    // 2.26 pav. PA „Analizuoti darbuotojų krūvį“
    [HttpGet]
    public async Task<IActionResult> KruvioAnalize()
    {
        var specialistai = await db.LaboratorijosSpecialistai.ToListAsync();
        return View(new KruvioAnalizesModelis
        {
            Rodikliai =
            [
                new("Laboratorijos specialistai", specialistai.Count),
                new("Aktyvūs specialistai", specialistai.Count(s => s.Busena)),
                new("Aktyvūs klonavimai", await db.Klonavimai.CountAsync(k => k.Busena))
            ]
        });
    }

    [HttpPost, ValidateAntiForgeryToken, ActionName(nameof(KruvioAnalize))]
    public IActionResult AtliktiKruvioAnalize()
    {
        Sekme("Darbo krūvio suvestinė suformuota.");
        return RedirectToAction(nameof(KruvioSuvestine));
    }

    public async Task<IActionResult> KruvioSuvestine()
    {
        var specialistai = await db.LaboratorijosSpecialistai.Include(s => s.Klonavimai).ToListAsync();
        var kruviai = DarboKruvioAnalize.Analizuoti(specialistai).OrderByDescending(k => k.Kruvis).ToList();
        var vidurkis = kruviai.Count > 0 ? kruviai.Average(k => k.Kruvis) : 0;
        return View(new KruvioSuvestinesModelis
        {
            Suformuota = laikas.GetLocalNow().DateTime,
            Kruviai = kruviai,
            Rodikliai =
            [
                new(DarboKruvioAnalize.Didelis, kruviai.Count(k => k.Lygis == DarboKruvioAnalize.Didelis)),
                new(DarboKruvioAnalize.Normalus, kruviai.Count(k => k.Lygis == DarboKruvioAnalize.Normalus)),
                new(DarboKruvioAnalize.Mazas, kruviai.Count(k => k.Lygis == DarboKruvioAnalize.Mazas)),
                new("Vidutinis darbo krūvis", $"{Formatai.Skaicius(vidurkis, 1)} %")
            ]
        });
    }

    // Neteisingai užpildyti laukai nurodomi formoje (2.21, 2.23 pav.)
    private async Task<bool> Tikrinti(DarbuotojoForma forma, int? id)
    {
        if (forma.Etatas is double etatas && etatas * 4 % 1 != 0) ModelState.AddModelError(nameof(forma.Etatas), "");
        if (forma.Specialistas && string.IsNullOrWhiteSpace(forma.LicencijosNumeris))
            ModelState.AddModelError(nameof(forma.LicencijosNumeris), "");
        if (!ModelState.IsValid) return false;

        var elPastas = forma.ElPastas.Trim();
        var licencija = forma.LicencijosNumeris?.Trim();
        if (await db.Darbuotojai.AnyAsync(d => d.ElPastas == elPastas && d.Id != id) || await db.Klientai.AnyAsync(k => k.ElPastas == elPastas))
            KlaidaLaukui(nameof(forma.ElPastas), "Toks el. paštas jau naudojamas kitos paskyros.");
        else if (forma.Specialistas && await db.LaboratorijosSpecialistai.AnyAsync(s => s.LicencijosNumeris == licencija && s.Id != id))
            KlaidaLaukui(nameof(forma.LicencijosNumeris), "Specialistas su tokiu licencijos numeriu jau užregistruotas.");
        return ModelState.IsValid;
    }

    private void Perkelti(DarbuotojoForma forma, Darbuotojas d)
    {
        d.Vardas = forma.Vardas.Trim();
        d.Pavarde = forma.Pavarde.Trim();
        d.TelefonoNumeris = forma.TelefonoNumeris.Trim();
        d.ElPastas = forma.ElPastas.Trim();
        d.Etatas = forma.Etatas!.Value;
        if (!string.IsNullOrEmpty(forma.Slaptazodis)) d.Slaptazodis = maisa.HashPassword(new object(), forma.Slaptazodis);
    }

    private IActionResult DarbuotojoNera()
    {
        ViewData["Posisteme"] = Posisteme.Darbuotojai;
        return Nera("Darbuotojas nerastas.", "/darbuotojai", "Grįžti į sąrašą");
    }
}
