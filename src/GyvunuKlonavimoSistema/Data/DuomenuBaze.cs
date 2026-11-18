using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace GyvunuKlonavimoSistema.Data;

// Paleidžiant programą: jei duomenų bazės nėra, ji sukuriama iš database/schema.sql
// ir užpildoma demonstraciniais duomenimis
public static class DuomenuBaze
{
    private const string SchemosDuomenuBaze = "gyvunu_klonavimas";

    public static async Task ParuostiAsync(IServiceProvider paslaugos, bool atkurti = false)
    {
        var konfiguracija = paslaugos.GetRequiredService<IConfiguration>();
        var log = paslaugos.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DuomenuBaze));
        var jungtis = new MySqlConnectionStringBuilder(konfiguracija.GetConnectionString("KlonavimoDb"));
        var pavadinimas = jungtis.Database;
        jungtis.Database = "";

        await using (var serveris = new MySqlConnection(jungtis.ConnectionString))
        {
            await serveris.OpenAsync();
            if (atkurti)
            {
                await new MySqlCommand($"DROP DATABASE IF EXISTS `{pavadinimas}`", serveris).ExecuteNonQueryAsync();
                log.LogInformation("Duomenų bazė {Pavadinimas} ištrinta", pavadinimas);
            }
            var tikrinimas = new MySqlCommand("SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = @db", serveris);
            tikrinimas.Parameters.AddWithValue("@db", pavadinimas);
            if (Convert.ToInt32(await tikrinimas.ExecuteScalarAsync()) == 0)
            {
                var scenarijus = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Data", "schema.sql"));
                await new MySqlScript(serveris, scenarijus.Replace(SchemosDuomenuBaze, pavadinimas)).ExecuteAsync();
                log.LogInformation("Duomenų bazė {Pavadinimas} sukurta iš schema.sql", pavadinimas);
            }
        }

        using var sritis = paslaugos.CreateScope();
        var db = sritis.ServiceProvider.GetRequiredService<KlonavimoDbContext>();
        if (!await db.Klientai.AnyAsync() && !await db.Darbuotojai.AnyAsync())
        {
            var maisa = sritis.ServiceProvider.GetRequiredService<IPasswordHasher<object>>();
            await DemonstraciniaiDuomenys.SukurtiAsync(db, maisa);
            log.LogInformation("Įrašyti demonstraciniai duomenys");
        }
    }
}
