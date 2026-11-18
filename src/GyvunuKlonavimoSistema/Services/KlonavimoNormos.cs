using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.Services;

// Embriono vystymosi poreikiai – pagal juos parenkamas inkubatorius
public record InkubavimoSalygos(double Temperatura, double Co2, double O2, double PhMin, double PhMax)
{
    public double PhVidurys => (PhMin + PhMax) / 2;
}

public static class KlonavimoNormos
{
    // Klonavimo kaina pagal gyvūno tipą, €
    public static readonly IReadOnlyDictionary<GyvunoTipas, decimal> Kainos = new Dictionary<GyvunoTipas, decimal>
    {
        [GyvunoTipas.Šuo] = 50000, [GyvunoTipas.Katė] = 40000, [GyvunoTipas.Triušis] = 15000,
        [GyvunoTipas.Žirgas] = 85000, [GyvunoTipas.Avis] = 20000, [GyvunoTipas.Kiaulė] = 25000,
        [GyvunoTipas.Papūga] = 12000
    };

    // Klonavimo trukmė (vaikingumo ar inkubacijos laikotarpis), d.
    public static readonly IReadOnlyDictionary<GyvunoTipas, int> TrukmeDienomis = new Dictionary<GyvunoTipas, int>
    {
        [GyvunoTipas.Šuo] = 63, [GyvunoTipas.Katė] = 65, [GyvunoTipas.Triušis] = 31,
        [GyvunoTipas.Žirgas] = 340, [GyvunoTipas.Avis] = 147, [GyvunoTipas.Kiaulė] = 114,
        [GyvunoTipas.Papūga] = 28
    };

    public static readonly IReadOnlyDictionary<GyvunoTipas, InkubavimoSalygos> InkubavimoSalygos = new Dictionary<GyvunoTipas, InkubavimoSalygos>
    {
        [GyvunoTipas.Šuo] = new(38.5, 5.0, 5.0, 7.2, 7.4),
        [GyvunoTipas.Katė] = new(38.5, 5.0, 5.0, 7.2, 7.4),
        [GyvunoTipas.Triušis] = new(38.0, 5.0, 5.0, 7.3, 7.5),
        [GyvunoTipas.Žirgas] = new(38.2, 6.0, 5.0, 7.2, 7.4),
        [GyvunoTipas.Avis] = new(38.8, 6.0, 5.0, 7.2, 7.4),
        [GyvunoTipas.Kiaulė] = new(38.8, 5.0, 5.0, 7.2, 7.4),
        [GyvunoTipas.Papūga] = new(37.5, 0.5, 20.0, 7.6, 7.8)
    };

    public const double LeidziamasTemperaturosNuokrypis = 0.3;
    public const double LeidziamasCo2Nuokrypis = 0.5;
    public const double LeidziamasO2Nuokrypis = 1.0;

    // Darbo krūvis (%) = aktyvių klonavimų trukmių suma ÷ (etatas × norma) × 100
    public const double KruvioNorma = 180;
}
