using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.Tests;

public class InkubatoriausPriskyrimasTests
{
    // Demonstraciniai laboratorijos inkubatoriai: 1, 3 ir 7 užimti
    private static readonly Inkubatorius[] Inkubatoriai =
    [
        Ink(1, 38.5, 5, true, 7.3, 5),
        Ink(2, 38.6, 5, false, 7.35, 5),
        Ink(3, 38.8, 5, true, 7.3, 5),
        Ink(4, 37, 5, false, 7.4, 20),
        Ink(5, 38.5, 5, false, 7.3, 5),
        Ink(6, 37.5, 0.5, false, 7.7, 20),
        Ink(7, 38.5, 5, true, 7.3, 5)
    ];

    private static Inkubatorius Ink(int id, double temperatura, double co2, bool uzimtas, double ph, double o2) => new()
    {
        Id = id, Modelis = $"Inkubatorius {id}", TikslineTemperatura = temperatura, Co2Procentas = co2,
        Busena = uzimtas, PhLygis = ph, O2Procentas = o2
    };

    [Fact]
    public void Papugai_parenkamas_vienintelis_atitinkantis_inkubatorius()
    {
        var rezultatas = InkubatoriausPriskyrimas.Ivertinti(GyvunoTipas.Papūga, Inkubatoriai);

        Assert.Equal(6, rezultatas.Parinktas?.Id);
    }

    [Fact]
    public void Kai_keli_tinka_parenkamas_arciausias_embriono_poreikiams()
    {
        // Šuniui tinka 2 (38,6 °C, pH 7,35) ir 5 (38,5 °C, pH 7,3); 5 atitinka tiksliai
        var rezultatas = InkubatoriausPriskyrimas.Ivertinti(GyvunoTipas.Šuo, Inkubatoriai);

        Assert.Equal([2, 5], rezultatas.Patikrinimai.Where(p => p.Tinka).Select(p => p.Inkubatorius.Id));
        Assert.Equal(5, rezultatas.Parinktas?.Id);
    }

    [Fact]
    public void Triusiui_tinkamo_inkubatoriaus_nera_ir_nurodomos_priezastys()
    {
        var rezultatas = InkubatoriausPriskyrimas.Ivertinti(GyvunoTipas.Triušis, Inkubatoriai);

        Assert.Null(rezultatas.Parinktas);
        var antras = rezultatas.Patikrinimai.Single(p => p.Inkubatorius.Id == 2);
        Assert.Equal(["temperatūra"], antras.Priezastys);
    }

    [Fact]
    public void Uzimtas_inkubatorius_netinka_net_jei_parametrai_atitinka()
    {
        var patikrinimas = InkubatoriausPriskyrimas.Patikrinti(Inkubatoriai[0], KlonavimoNormos.InkubavimoSalygos[GyvunoTipas.Šuo]);

        Assert.False(patikrinimas.Tinka);
        Assert.Equal(["užimtas"], patikrinimas.Priezastys);
    }
}
