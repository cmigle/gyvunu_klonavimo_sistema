using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.Tests;

public class DnrTinkamumoAnalizeTests
{
    private static DnrPavyzdys Pavyzdys(double? koncentracija, double? turis, double? tarsa, double? din, double? gyvybingumas, double temperatura = -80) => new()
    {
        SaugojimoTemperatura = temperatura, DnrKoncentracija = koncentracija, TirpaloTuris = turis,
        BaltymuTarsa = tarsa, DinVerte = din, LasteliuGyvybingumas = gyvybingumas
    };

    [Fact]
    public void Visi_parametrai_normose_tinkamas_klonuoti()
    {
        var rezultatas = DnrTinkamumoAnalize.Analizuoti(Pavyzdys(71.8, 0.12, 1.85, 7.2, 82.5));

        Assert.True(rezultatas.GalimaNustatyti);
        Assert.Empty(rezultatas.Neatitikimai);
        Assert.Equal(Tinkamumai.Tinkamas, rezultatas.Statusas);
    }

    [Fact]
    public void Kiekvienas_neatitikimas_iskaiciuojamas_ir_pavyzdys_netinkamas()
    {
        // DNR-0004: maža koncentracija, baltymų tarša, DIN ir gyvybingumas žemiau normų
        var rezultatas = DnrTinkamumoAnalize.Analizuoti(Pavyzdys(31.5, 0.12, 1.62, 5.9, 72.4));

        Assert.Equal(Tinkamumai.Netinkamas, rezultatas.Statusas);
        Assert.Equal(["DNR koncentracija", "Baltymų tarša (A260/280)", "DIN vertė", "Ląstelių gyvybingumas"],
            rezultatas.Neatitikimai.Select(n => n.Norma.Pavadinimas));
    }

    [Fact]
    public void Per_silta_saugojimo_temperatura_yra_neatitikimas()
    {
        var rezultatas = DnrTinkamumoAnalize.Analizuoti(Pavyzdys(100, 0.2, 1.9, 8, 90, temperatura: -10));

        Assert.Equal(Tinkamumai.Netinkamas, rezultatas.Statusas);
        Assert.Equal("Saugojimo temperatūra", Assert.Single(rezultatas.Neatitikimai).Norma.Pavadinimas);
    }

    [Theory]
    [InlineData(1.8, true)]
    [InlineData(2.0, true)]
    [InlineData(1.79, false)]
    [InlineData(2.01, false)]
    public void Normos_ribos_imtinai(double tarsa, bool tinkamas)
    {
        var rezultatas = DnrTinkamumoAnalize.Analizuoti(Pavyzdys(100, 0.2, tarsa, 8, 90));

        Assert.Equal(tinkamas ? Tinkamumai.Tinkamas : Tinkamumai.Netinkamas, rezultatas.Statusas);
    }

    [Fact]
    public void Neivedus_rezultatu_tinkamumo_nustatyti_negalima()
    {
        var rezultatas = DnrTinkamumoAnalize.Analizuoti(Pavyzdys(100, null, 1.9, null, 90));

        Assert.False(rezultatas.GalimaNustatyti);
        Assert.Null(rezultatas.Statusas);
        Assert.Equal(["Tirpalo tūris", "DIN vertė"], rezultatas.Truksta);
    }
}
