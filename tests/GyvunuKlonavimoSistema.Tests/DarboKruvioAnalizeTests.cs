using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.Tests;

public class DarboKruvioAnalizeTests
{
    private static Klonavimas Klonavimas(string pradzia, string pabaiga, bool aktyvus) => new()
    {
        PradziosData = DateOnly.Parse(pradzia), PabaigosData = DateOnly.Parse(pabaiga), Busena = aktyvus
    };

    [Theory]
    [InlineData(0, DarboKruvioAnalize.Mazas)]
    [InlineData(69.9, DarboKruvioAnalize.Mazas)]
    [InlineData(70, DarboKruvioAnalize.Normalus)]
    [InlineData(100, DarboKruvioAnalize.Normalus)]
    [InlineData(100.1, DarboKruvioAnalize.Didelis)]
    public void Lygis_priskiriamas_pagal_ribas(double procentai, string lygis) =>
        Assert.Equal(lygis, DarboKruvioAnalize.Lygis(procentai));

    [Fact]
    public void Skaiciuojami_tik_aktyvus_klonavimai_pagal_etata()
    {
        var specialistas = new LaboratorijosSpecialistas
        {
            Etatas = 0.5,
            Klonavimai =
            [
                Klonavimas("2026-10-01", "2026-12-03", aktyvus: true),  // 63 d.
                Klonavimas("2026-10-10", "2026-12-14", aktyvus: true),  // 65 d.
                Klonavimas("2026-01-01", "2026-12-31", aktyvus: false)  // baigtas – neskaičiuojamas
            ]
        };

        var kruvis = DarboKruvioAnalize.Apskaiciuoti(specialistas);

        Assert.Equal(2, kruvis.AktyviuKiekis);
        Assert.Equal(128, kruvis.Trukme);
        Assert.Equal(128 / (0.5 * 180) * 100, kruvis.Kruvis, 6);
        Assert.Equal(DarboKruvioAnalize.Didelis, kruvis.Lygis);
    }

    [Fact]
    public void Specialistas_be_klonavimu_turi_maza_kruvi()
    {
        var kruvis = DarboKruvioAnalize.Apskaiciuoti(new LaboratorijosSpecialistas { Etatas = 1 });

        Assert.Equal(0, kruvis.Kruvis);
        Assert.Equal(DarboKruvioAnalize.Mazas, kruvis.Lygis);
    }
}
