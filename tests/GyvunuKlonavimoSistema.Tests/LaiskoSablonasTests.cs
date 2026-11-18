using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.Tests;

public class LaiskoSablonasTests
{
    [Fact]
    public void Patvirtinimo_laiske_nurodomas_uzsakymas_augintinis_ir_kaina()
    {
        var klientas = new Klientas { Vardas = "Jonas", Pavarde = "Petrauskas", ElPastas = "jonas@pavyzdys.lt" };
        var augintinis = new Augintinis { Vardas = "Murka", Tipas = GyvunoTipas.Katė };
        var uzsakymas = new Uzsakymas { Id = 2, Kaina = 40000, Busena = true, PatvirtinimoData = new DateOnly(2026, 11, 18) };
        var pavyzdys = new DnrPavyzdys { Id = 2 };

        var laiskas = LaiskoSablonas.UzsakymoPatvirtinimas(uzsakymas, augintinis, klientas, pavyzdys);

        Assert.Equal("jonas@pavyzdys.lt", laiskas.Gavejas);
        Assert.Equal("Užsakymas UZ-0002 patvirtintas", laiskas.Tema);
        Assert.Contains("Gerb. Jonas Petrauskas,", laiskas.Tekstas);
        Assert.Contains("augintiniui „Murka“ (katė) patvirtintas 2026-11-18", laiskas.Tekstas);
        Assert.Contains("DNR pavyzdys DNR-0002 įvertintas", laiskas.Tekstas);
        Assert.Contains($"Užsakymo kaina: {Formatai.Pinigai(40000)}.", laiskas.Tekstas);
    }

    [Fact]
    public void Kaina_rodoma_lietuvisku_formatu() =>
        Assert.Equal("40 000 €", Formatai.Pinigai(40000).Replace(' ', ' '));
}
