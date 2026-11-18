using GyvunuKlonavimoSistema.Models;

namespace GyvunuKlonavimoSistema.Services;

public record InkubatoriausPatikrinimas(Inkubatorius Inkubatorius, IReadOnlyList<string> Priezastys, double Ivertis)
{
    public bool Tinka => Priezastys.Count == 0;
}

public record PriskyrimoRezultatas(GyvunoTipas Tipas, InkubavimoSalygos Salygos, IReadOnlyList<InkubatoriausPatikrinimas> Patikrinimai)
{
    // Tinkamiausias – laisvas inkubatorius, kurio parametrai arčiausiai embriono poreikių
    public Inkubatorius? Parinktas => Patikrinimai.Where(p => p.Tinka).OrderBy(p => p.Ivertis).FirstOrDefault()?.Inkubatorius;
}

// PA „Priskirti inkubatorių“: ciklas per visus inkubatorius, tinkami įtraukiami į sąrašą
public static class InkubatoriausPriskyrimas
{
    private const double Paklaida = 1e-9;

    public static PriskyrimoRezultatas Ivertinti(GyvunoTipas tipas, IEnumerable<Inkubatorius> inkubatoriai)
    {
        var salygos = KlonavimoNormos.InkubavimoSalygos[tipas];
        return new(tipas, salygos, inkubatoriai.Select(i => Patikrinti(i, salygos)).ToList());
    }

    public static InkubatoriausPatikrinimas Patikrinti(Inkubatorius ink, InkubavimoSalygos salygos)
    {
        var temperatura = Math.Abs(ink.TikslineTemperatura - salygos.Temperatura);
        var co2 = Math.Abs(ink.Co2Procentas - salygos.Co2);
        var o2 = Math.Abs(ink.O2Procentas - salygos.O2);

        var priezastys = new List<string>();
        if (ink.Busena) priezastys.Add("užimtas");
        if (temperatura > KlonavimoNormos.LeidziamasTemperaturosNuokrypis + Paklaida) priezastys.Add("temperatūra");
        if (co2 > KlonavimoNormos.LeidziamasCo2Nuokrypis + Paklaida) priezastys.Add("CO₂");
        if (o2 > KlonavimoNormos.LeidziamasO2Nuokrypis + Paklaida) priezastys.Add("O₂");
        if (ink.PhLygis < salygos.PhMin || ink.PhLygis > salygos.PhMax) priezastys.Add("pH");

        var ivertis = temperatura / KlonavimoNormos.LeidziamasTemperaturosNuokrypis
            + co2 / KlonavimoNormos.LeidziamasCo2Nuokrypis
            + o2 / KlonavimoNormos.LeidziamasO2Nuokrypis
            + Math.Abs(ink.PhLygis - salygos.PhVidurys) / ((salygos.PhMax - salygos.PhMin) / 2);
        return new(ink, priezastys, ivertis);
    }
}
