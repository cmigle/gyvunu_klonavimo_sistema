using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Models;
using Microsoft.EntityFrameworkCore;

namespace GyvunuKlonavimoSistema.Services;

// Užsakymas patvirtinamas automatiškai, kai augintinio DNR pavyzdys įvertinamas kaip tinkamas
// klonuoti; klientui per el. pašto serverį išsiunčiamas pranešimas (3.1.17 pav.)
public class UzsakymoPatvirtinimas(KlonavimoDbContext db, IElPastoServeris pastas, TimeProvider laikas)
{
    public async Task<Uzsakymas?> PatvirtintiJeiTinkaAsync(int augintinisId)
    {
        var augintinis = await db.Augintiniai
            .Include(a => a.Klientas)
            .Include(a => a.Uzsakymas)
            .Include(a => a.DnrPavyzdys)
            .FirstOrDefaultAsync(a => a.Id == augintinisId);
        var uzsakymas = augintinis?.Uzsakymas;
        if (augintinis is null || uzsakymas is null || uzsakymas.Busena) return null;
        if (augintinis.DnrPavyzdys?.Tinkamumas != Tinkamumai.Tinkamas) return null;

        var dabar = laikas.GetLocalNow().DateTime;
        uzsakymas.Busena = true;
        uzsakymas.PatvirtinimoData = DateOnly.FromDateTime(dabar);
        var laiskas = LaiskoSablonas.UzsakymoPatvirtinimas(uzsakymas, augintinis, augintinis.Klientas, augintinis.DnrPavyzdys);
        db.Pranesimai.Add(new Pranesimas
        {
            Uzsakymas = uzsakymas,
            Tekstas = laiskas.Tekstas,
            SiuntimoData = dabar,
            Busena = pastas.Siusti(laiskas)
        });
        await db.SaveChangesAsync();
        return uzsakymas;
    }
}
