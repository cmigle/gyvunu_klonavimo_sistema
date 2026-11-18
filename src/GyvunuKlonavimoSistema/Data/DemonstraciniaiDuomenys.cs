using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;
using Microsoft.AspNetCore.Identity;

namespace GyvunuKlonavimoSistema.Data;

// Tie patys demonstraciniai duomenys kaip GUI prototipe (prototype/js/duomenys.js)
public static class DemonstraciniaiDuomenys
{
    public const string Slaptazodis = "demo1234";

    private static DateOnly D(string data) => DateOnly.Parse(data);
    private static DateTime Dl(string data) => DateTime.Parse(data);

    public static async Task SukurtiAsync(KlonavimoDbContext db, IPasswordHasher<object> maisa)
    {
        var slaptazodis = maisa.HashPassword(new object(), Slaptazodis);

        var klientai = new[]
        {
            Klientas(1, "Jonas", "Petrauskas", "jonas@pavyzdys.lt", "+37061234567", 38901151234, "2026-08-18 10:42"),
            Klientas(2, "Rūta", "Kazlauskienė", "ruta@pavyzdys.lt", "+37062345678", 48705152345, null),
            Klientas(3, "Tomas", "Žukauskas", "tomas@pavyzdys.lt", "+37063456789", 39203034567, "2026-05-28 14:05"),
            Klientas(4, "Ieva", "Mikalauskaitė", "ieva@pavyzdys.lt", "+37064567890", 49511214321, "2026-09-02 09:17"),
            Klientas(5, "Andrius", "Paulauskas", "andrius@pavyzdys.lt", "+37065678901", 38407306543, "2026-09-30 16:31")
        };
        foreach (var k in klientai) k.Slaptazodis = slaptazodis;
        db.Klientai.AddRange(klientai);

        db.Augintiniai.AddRange(
            Augintinis(1, 1, "Reksas", "Vokiečių aviganis", 9, GyvunoTipas.Šuo, Lytys.Patinas, 34.5, "440110000123456"),
            Augintinis(2, 1, "Murka", "Meinkūnas", 12, GyvunoTipas.Katė, Lytys.Patele, 6.8, "440110000234567"),
            Augintinis(3, 1, "Žaibas", "Lietuvos sunkusis", 15, GyvunoTipas.Žirgas, Lytys.Patinas, 640, "440110000345678"),
            Augintinis(4, 2, "Pifas", "Britų trumpaplaukė", 10, GyvunoTipas.Katė, Lytys.Patinas, 5.6, "440110000456789"),
            Augintinis(5, 3, "Bella", "Labradoro retriveris", 11, GyvunoTipas.Šuo, Lytys.Patele, 29, "440110000567890"),
            Augintinis(6, 3, "Baltukė", "Romanovų", 7, GyvunoTipas.Avis, Lytys.Patele, 55, "440110000678901"),
            Augintinis(7, 3, "Pūkis", "Olandų nykštukinis", 6, GyvunoTipas.Triušis, Lytys.Patinas, 1.2, "440110000789012"),
            Augintinis(8, 4, "Rokis", "Biglis", 8, GyvunoTipas.Šuo, Lytys.Patinas, 14.2, "440110000890123"),
            Augintinis(9, 4, "Snieguolė", "Sibiro", 13, GyvunoTipas.Katė, Lytys.Patele, 4.9, "440110000901234"),
            Augintinis(10, 4, "Kaštonas", "Trakėnų", 18, GyvunoTipas.Žirgas, Lytys.Patinas, 580, "440110001012345"),
            Augintinis(11, 4, "Kiki", "Geltonkuodis kakadu", 28, GyvunoTipas.Papūga, Lytys.Patele, 0.9, "440110001123456"),
            Augintinis(12, 5, "Bekonas", "Vietnamo kiaulė", 5, GyvunoTipas.Kiaulė, Lytys.Patinas, 68, "440110001234567"),
            Augintinis(13, 5, "Ūsas", "Bengalų", 9, GyvunoTipas.Katė, Lytys.Patinas, 5.3, "440110001345678"),
            Augintinis(14, 5, "Rudis", "Taksas", 4, GyvunoTipas.Šuo, Lytys.Patinas, 9.1, "440110001456789"));

        // Žaibas (3) ir Kiki (11) dokumentų neturi – rodomas pranešimas „Dokumentų nėra“
        db.Dokumentai.AddRange(
            Dokumentas(1, 1, "reksas-kilmes-sertifikatas.pdf", "Kilmės sertifikatas", "2026-08-20", "2031-08-20"),
            Dokumentas(2, 1, "sutartis-UZ-0001.pdf", "Sutartis", "2026-08-22", "2027-08-22"),
            Dokumentas(3, 1, "reksas-pasas.jpg", "Veterinarinis pasas", "2026-08-20", "2027-01-10"),
            Dokumentas(4, 2, "murka-sertifikatas.png", "Kilmės sertifikatas", "2026-10-10", "2031-10-10"),
            Dokumentas(5, 4, "pifas-pasas.pdf", "Veterinarinis pasas", "2025-03-01", "2026-03-01"),
            Dokumentas(6, 5, "bella-sertifikatas.pdf", "Kilmės sertifikatas", "2026-05-29", "2031-05-29"),
            Dokumentas(7, 5, "sutartis-UZ-0003.pdf", "Sutartis", "2026-05-30", "2027-05-30"),
            Dokumentas(8, 6, "baltuke-pasas.jpg", "Veterinarinis pasas", "2026-09-30", "2028-09-30"),
            Dokumentas(9, 7, "pukis-sertifikatas.pdf", "Kilmės sertifikatas", "2026-10-28", "2031-10-28"),
            Dokumentas(10, 8, "rokis-sertifikatas.pdf", "Kilmės sertifikatas", "2026-09-05", "2031-09-05"),
            Dokumentas(11, 9, "snieguole-sertifikatas.png", "Kilmės sertifikatas", "2026-09-19", "2031-09-19"),
            Dokumentas(12, 10, "kastonas-sertifikatas.pdf", "Kilmės sertifikatas", "2026-02-12", "2031-02-12"),
            Dokumentas(13, 12, "bekonas-pasas.pdf", "Veterinarinis pasas", "2026-09-25", "2028-09-25"),
            Dokumentas(14, 13, "usas-sertifikatas.jpg", "Kilmės sertifikatas", "2026-10-04", "2031-10-04"));

        var uzsakymai = new[]
        {
            Uzsakymas(1, 1, 50000, "2026-08-22", "2026-09-05"),
            Uzsakymas(2, 2, 40000, "2026-10-12", null),
            Uzsakymas(3, 5, 50000, "2026-05-30", "2026-06-10"),
            Uzsakymas(4, 6, 20000, "2026-10-02", null),
            Uzsakymas(5, 8, 50000, "2026-09-08", "2026-09-18"),
            Uzsakymas(6, 9, 40000, "2026-09-21", "2026-10-01"),
            Uzsakymas(7, 10, 85000, "2026-02-16", "2026-03-02"),
            Uzsakymas(8, 12, 25000, "2026-09-29", "2026-10-10"),
            Uzsakymas(9, 13, 40000, "2026-10-06", "2026-10-15"),
            Uzsakymas(10, 7, 15000, "2026-10-30", "2026-11-04"),
            Uzsakymas(11, 11, 12000, "2026-11-02", "2026-11-05"),
            Uzsakymas(12, 14, 50000, "2026-11-06", null)
        };
        db.Uzsakymai.AddRange(uzsakymai);

        var t = Tinkamumai.Tinkamas;
        var dnr = new[]
        {
            Dnr(1, 1, "2026-08-25", 152.4, 0.2, 1.86, 8.6, 94.5, t, "2026-09-01"),
            // Rezultatai įvesti, bet tinkamumas dar nenustatytas: nustačius – UZ-0002 patvirtinamas automatiškai
            Dnr(2, 2, "2026-10-14", 98.1, 0.15, 1.91, 8.1, 91, null, null),
            Dnr(3, 5, "2026-06-02", 134, 0.18, 1.88, 8.9, 95.2, t, "2026-06-06"),
            Dnr(4, 6, "2026-10-05", 31.5, 0.12, 1.62, 5.9, 72.4, Tinkamumai.Netinkamas, "2026-10-09"),
            Dnr(5, 7, "2026-11-01", 87.6, 0.1, 1.84, 7.6, 88.3, t, "2026-11-03"),
            Dnr(6, 11, "2026-11-02", 64.2, 0.08, 1.83, 7.4, 86.1, t, "2026-11-04"),
            Dnr(7, 8, "2026-09-10", 120.7, 0.2, 1.9, 8.4, 93.1, t, "2026-09-15"),
            Dnr(8, 9, "2026-09-23", 101.3, 0.16, 1.87, 8, 90.6, t, "2026-09-28"),
            Dnr(9, 10, "2026-02-18", 76.2, 0.25, 1.82, 7.3, 84, t, "2026-02-25"),
            Dnr(10, 12, "2026-10-01", 143.9, 0.22, 1.93, 8.8, 92.7, t, "2026-10-07"),
            Dnr(11, 13, "2026-10-08", 110.5, 0.17, 1.89, 8.2, 91.8, t, "2026-10-13"),
            // Pifo pavyzdys: rezultatai įvesti, užsakymo dar nėra
            Dnr(12, 4, "2026-11-04", 71.8, 0.12, 1.85, 7.2, 82.5, null, null),
            // Rudžio pavyzdys: rezultatai dar neįvesti
            new DnrPavyzdys { Id = 13, AugintinisId = 14, PaemimoData = D("2026-11-06"), SaugojimoTemperatura = -80 }
        };
        db.DnrPavyzdziai.AddRange(dnr);

        var inkubatoriai = new[]
        {
            Inkubatorius(1, "Thermo Fisher Heracell VIOS 160i", "HV160-22-0147", 38.5, 165, 5, true, 7.3, 5),
            Inkubatorius(2, "Eppendorf CellXpert C170i", "CX170-21-0458", 38.6, 170, 5, false, 7.35, 5),
            Inkubatorius(3, "Binder CB 170", "CB170-20-1123", 38.8, 170, 5, true, 7.3, 5),
            Inkubatorius(4, "PHCbi MCO-170AICUV", "MC170-23-3390", 37, 165, 5, false, 7.4, 20),
            Inkubatorius(5, "Esco CelCulture CCL-170B", "CC170-22-0821", 38.5, 170, 5, false, 7.3, 5),
            Inkubatorius(6, "Memmert ICO150med", "IC150-19-0042", 37.5, 150, 0.5, false, 7.7, 20),
            Inkubatorius(7, "Thermo Fisher Forma Steri-Cycle i160", "FS160-21-7781", 38.5, 160, 5, true, 7.3, 5)
        };
        db.Inkubatoriai.AddRange(inkubatoriai);

        // Paskutinių šešių dienų matavimai kiekvienam inkubatoriui
        double[] nuokrypiai = [0.1, -0.1, 0, 0.2, -0.2, 0.1];
        double[] dregmes = [93.4, 94.1, 92.8, 95, 93.6, 94.3];
        foreach (var ink in inkubatoriai)
            for (var i = 0; i < nuokrypiai.Length; i++)
                db.InkubatoriuMatavimai.Add(new InkubatoriausMatavimas
                {
                    InkubatoriusId = ink.Id,
                    MatavimoData = new DateTime(2026, 11, 2 + i, 8, 0, 0),
                    Temperatura = Math.Round(ink.TikslineTemperatura + nuokrypiai[i] * (ink.Id % 2 == 1 ? 1 : -1), 1),
                    Dregme = dregmes[(i + ink.Id) % dregmes.Length]
                });

        db.LaboratorijosSpecialistai.AddRange(
            Specialistas(1, "Laura", "Vaitkutė", "+37061111222", "laura@klonavimas.lt", 1, "LS-2019-0142", true, slaptazodis),
            Specialistas(2, "Mantas", "Jankauskas", "+37061111333", "mantas@klonavimas.lt", 0.75, "LS-2020-0217", true, slaptazodis),
            Specialistas(3, "Greta", "Stankevičiūtė", "+37061111444", "greta@klonavimas.lt", 1, "LS-2023-0388", true, slaptazodis),
            Specialistas(4, "Darius", "Mockus", "+37061111555", "darius@klonavimas.lt", 0.5, "LS-2017-0091", false, slaptazodis));
        db.Darbuotojai.Add(new Darbuotojas
        {
            Id = 5, Vardas = "Asta", Pavarde = "Kairienė", TelefonoNumeris = "+37061111666",
            ElPastas = "asta@klonavimas.lt", Etatas = 1, Slaptazodis = slaptazodis
        });

        db.Klonavimai.AddRange(
            Klonavimas(1, 1, 1, 1, 1, "2026-09-08", "2026-11-10", "Embrionai vystosi normaliai", true, true),
            Klonavimas(2, 3, 3, 2, null, "2026-06-15", "2026-08-17", "Sėkmingas, gimė sveikas šuniukas", true, false),
            Klonavimas(3, 5, 7, 1, null, "2026-09-22", "2026-11-24", "Embrionas perkeltas surogatinei motinai", true, true),
            Klonavimas(4, 6, 8, 1, 7, "2026-10-03", "2026-12-07", "Vyksta embriono inkubacija", false, true),
            Klonavimas(5, 7, 9, 2, null, "2026-03-10", "2026-04-02", "Nesėkmingas, embrionas nustojo vystytis", true, false),
            Klonavimas(6, 8, 10, 2, 3, "2026-10-14", "2027-02-05", "Vyksta embriono inkubacija", false, true),
            Klonavimas(7, 9, 11, 3, null, "2026-10-20", "2026-12-24", "Embrionas perkeltas surogatinei motinai", true, true));

        await db.SaveChangesAsync();

        // Patvirtintiems užsakymams – išsiųsti pranešimai pagal laiško šabloną
        foreach (var u in uzsakymai.Where(u => u.Busena))
        {
            var augintinis = db.Augintiniai.Local.First(a => a.Id == u.AugintinisId);
            var klientas = klientai.First(k => k.Id == augintinis.KlientasId);
            var pavyzdys = dnr.FirstOrDefault(x => x.AugintinisId == augintinis.Id);
            db.Pranesimai.Add(new Pranesimas
            {
                UzsakymasId = u.Id,
                Tekstas = LaiskoSablonas.UzsakymoPatvirtinimas(u, augintinis, klientas, pavyzdys).Tekstas,
                SiuntimoData = u.PatvirtinimoData!.Value.ToDateTime(new TimeOnly(9, 15)),
                Busena = true
            });
        }
        await db.SaveChangesAsync();
    }

    private static Klientas Klientas(int id, string vardas, string pavarde, string elPastas, string telefonas, long asmensKodas, string? patikrinta) => new()
    {
        Id = id, Vardas = vardas, Pavarde = pavarde, ElPastas = elPastas, TelefonoNumeris = telefonas, AsmensKodas = asmensKodas,
        IstorijosBusena = patikrinta is null ? IstorijosBusenos.Nepatikrintas : IstorijosBusenos.Patikrintas,
        PatikrinimoData = patikrinta is null ? null : Dl(patikrinta)
    };

    private static Augintinis Augintinis(int id, int klientas, string vardas, string veisle, int amzius, GyvunoTipas tipas, string lytis, double svoris, string mikroschema) => new()
    {
        Id = id, KlientasId = klientas, Vardas = vardas, Veisle = veisle, Amzius = amzius, Tipas = tipas,
        Lytis = lytis, Svoris = svoris, MikroschemosNumeris = mikroschema
    };

    private static Dokumentas Dokumentas(int id, int augintinis, string failas, string tipas, string ikelta, string galioja) => new()
    {
        Id = id, AugintinisId = augintinis, FailoPavadinimas = failas, DokumentoTipas = tipas,
        IkelimoData = D(ikelta), GaliojimoData = D(galioja),
        FailoFormatas = FailuFormatai.PagalPletini[Path.GetExtension(failas)]
    };

    private static Uzsakymas Uzsakymas(int id, int augintinis, decimal kaina, string sukurta, string? patvirtinta) => new()
    {
        Id = id, AugintinisId = augintinis, Kaina = kaina, SukurimoData = D(sukurta),
        Busena = patvirtinta is not null, PatvirtinimoData = patvirtinta is null ? null : D(patvirtinta)
    };

    private static DnrPavyzdys Dnr(int id, int augintinis, string paimta, double koncentracija, double turis, double tarsa,
        double din, double gyvybingumas, string? tinkamumas, string? nustatyta) => new()
    {
        Id = id, AugintinisId = augintinis, PaemimoData = D(paimta), SaugojimoTemperatura = -80,
        DnrKoncentracija = koncentracija, TirpaloTuris = turis, BaltymuTarsa = tarsa, DinVerte = din,
        LasteliuGyvybingumas = gyvybingumas, Tinkamumas = tinkamumas, TinkamumoData = nustatyta is null ? null : D(nustatyta)
    };

    private static Inkubatorius Inkubatorius(int id, string modelis, string numeris, double temperatura, double talpa, double co2, bool uzimtas, double ph, double o2) => new()
    {
        Id = id, Modelis = modelis, SerijinisNumeris = numeris, TikslineTemperatura = temperatura, Talpa = talpa,
        Co2Procentas = co2, Busena = uzimtas, PhLygis = ph, O2Procentas = o2
    };

    private static LaboratorijosSpecialistas Specialistas(int id, string vardas, string pavarde, string telefonas, string elPastas,
        double etatas, string licencija, bool aktyvus, string slaptazodis) => new()
    {
        Id = id, Vardas = vardas, Pavarde = pavarde, TelefonoNumeris = telefonas, ElPastas = elPastas, Etatas = etatas,
        LicencijosNumeris = licencija, Busena = aktyvus, Slaptazodis = slaptazodis
    };

    private static Klonavimas Klonavimas(int id, int uzsakymas, int dnr, int? specialistas, int? inkubatorius, string pradzia,
        string pabaiga, string rezultatas, bool patvirtinta, bool aktyvus) => new()
    {
        Id = id, UzsakymasId = uzsakymas, DnrPavyzdysId = dnr, LaboratorijosSpecialistasId = specialistas,
        InkubatoriusId = inkubatorius, PradziosData = D(pradzia), PabaigosData = D(pabaiga), Rezultatas = rezultatas,
        SpecialistoPatvirtinimas = patvirtinta, Busena = aktyvus
    };
}
