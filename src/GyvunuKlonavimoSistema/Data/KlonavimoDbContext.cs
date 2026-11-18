using System.Text;
using GyvunuKlonavimoSistema.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GyvunuKlonavimoSistema.Data;

// Esybės susiejamos su database/schema.sql lentelėmis: klasė KlientasId -> stulpelis klientas_id
public class KlonavimoDbContext(DbContextOptions<KlonavimoDbContext> options) : DbContext(options)
{
    public DbSet<Klientas> Klientai => Set<Klientas>();
    public DbSet<Augintinis> Augintiniai => Set<Augintinis>();
    public DbSet<Dokumentas> Dokumentai => Set<Dokumentas>();
    public DbSet<Uzsakymas> Uzsakymai => Set<Uzsakymas>();
    public DbSet<Pranesimas> Pranesimai => Set<Pranesimas>();
    public DbSet<DnrPavyzdys> DnrPavyzdziai => Set<DnrPavyzdys>();
    public DbSet<Klonavimas> Klonavimai => Set<Klonavimas>();
    public DbSet<Inkubatorius> Inkubatoriai => Set<Inkubatorius>();
    public DbSet<InkubatoriausMatavimas> InkubatoriuMatavimai => Set<InkubatoriausMatavimas>();
    public DbSet<Darbuotojas> Darbuotojai => Set<Darbuotojas>();
    public DbSet<LaboratorijosSpecialistas> LaboratorijosSpecialistai => Set<LaboratorijosSpecialistas>();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<DateOnly>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
        b.Properties<bool>().HaveColumnType("tinyint(1)");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Augintinis>().Property(a => a.Tipas).HasConversion<string>();

        // Ryšiai 1 : 0..1
        b.Entity<Augintinis>().HasOne(a => a.Uzsakymas).WithOne(u => u.Augintinis)
            .HasForeignKey<Uzsakymas>(u => u.AugintinisId);
        b.Entity<Augintinis>().HasOne(a => a.DnrPavyzdys).WithOne(d => d.Augintinis)
            .HasForeignKey<DnrPavyzdys>(d => d.AugintinisId);
        b.Entity<Uzsakymas>().HasOne(u => u.Pranesimas).WithOne(p => p.Uzsakymas)
            .HasForeignKey<Pranesimas>(p => p.UzsakymasId);
        b.Entity<Uzsakymas>().HasOne(u => u.Klonavimas).WithOne(k => k.Uzsakymas)
            .HasForeignKey<Klonavimas>(k => k.UzsakymasId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DnrPavyzdys>().HasOne(d => d.Klonavimas).WithOne(k => k.DnrPavyzdys)
            .HasForeignKey<Klonavimas>(k => k.DnrPavyzdysId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Klonavimas>().HasOne(k => k.LaboratorijosSpecialistas).WithMany(s => s.Klonavimai)
            .HasForeignKey(k => k.LaboratorijosSpecialistasId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Klonavimas>().HasOne(k => k.Inkubatorius).WithMany(i => i.Klonavimai)
            .HasForeignKey(k => k.InkubatoriusId).OnDelete(DeleteBehavior.SetNull);

        foreach (var esybe in b.Model.GetEntityTypes())
        {
            esybe.SetTableName(SnakeCase(esybe.ClrType.Name));
            foreach (var savybe in esybe.GetDeclaredProperties())
                savybe.SetColumnName(SnakeCase(savybe.Name));
        }

        // Paveldėjimas (TPT): specialisto lentelės pirminis raktas vadinasi darbuotojas_id
        b.Entity<LaboratorijosSpecialistas>().ToTable("laboratorijos_specialistas",
            t => t.Property(s => s.Id).HasColumnName("darbuotojas_id"));
    }

    // „DnrPavyzdysId“ -> „dnr_pavyzdys_id“, „Co2Procentas“ -> „co2_procentas“
    private static string SnakeCase(string pavadinimas)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < pavadinimas.Length; i++)
        {
            var c = pavadinimas[i];
            if (char.IsUpper(c) && i > 0 && !char.IsUpper(pavadinimas[i - 1])) sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    private sealed class DateOnlyConverter() : ValueConverter<DateOnly, DateTime>(
        d => d.ToDateTime(TimeOnly.MinValue), d => DateOnly.FromDateTime(d));
}
