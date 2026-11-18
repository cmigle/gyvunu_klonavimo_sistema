using System.ComponentModel.DataAnnotations;
using GyvunuKlonavimoSistema.Infrastructure;
using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.ViewModels;

// 2.21 pav. PA „Sukurti darbuotoją“ ir 2.23 pav. PA „Redaguoti darbuotoją“
public class DarbuotojoForma
{
    [Display(Name = "Vardas"), Required, StringLength(50)]
    public string Vardas { get; set; } = "";

    [Display(Name = "Pavardė"), Required, StringLength(50)]
    public string Pavarde { get; set; } = "";

    [Display(Name = "Telefono numeris"), Required, RegularExpression(@"\+?[0-9 ]{8,15}")]
    public string TelefonoNumeris { get; set; } = "";

    [Display(Name = "El. paštas"), Required, EmailAddress, StringLength(100)]
    public string ElPastas { get; set; } = "";

    [Display(Name = "Etatas"), Required, Range(0.25, 1)]
    public double? Etatas { get; set; }

    [Display(Name = "Rolė (prieigos teisės)")]
    public string? Role { get; set; }

    // Licencija ir būsena – tik laboratorijos specialistui
    [Display(Name = "Licencijos numeris"), StringLength(20)]
    public string? LicencijosNumeris { get; set; }

    [Display(Name = "Būsena")]
    public bool Busena { get; set; } = true;

    // Kuriant – pradinis slaptažodis (privalomas), redaguojant – naujas (neprivalomas)
    [Display(Name = "Slaptažodis"), MinLength(8), DataType(DataType.Password)]
    public string? Slaptazodis { get; set; }

    public bool Specialistas => Role == Roles.Specialistas;

    public static DarbuotojoForma Is(Darbuotojas d) => new()
    {
        Vardas = d.Vardas, Pavarde = d.Pavarde, TelefonoNumeris = d.TelefonoNumeris, ElPastas = d.ElPastas,
        Etatas = d.Etatas, Role = DarbuotojuRoles.Role(d),
        LicencijosNumeris = (d as LaboratorijosSpecialistas)?.LicencijosNumeris,
        Busena = (d as LaboratorijosSpecialistas)?.Busena ?? true
    };
}

public static class DarbuotojuRoles
{
    public static string Role(Darbuotojas d) => d is LaboratorijosSpecialistas ? Roles.Specialistas : Roles.Administratorius;
    public static string Pavadinimas(Darbuotojas d) => Roles.Pavadinimas(Role(d));
}

public class DarbuotojuSarasoModelis
{
    public List<Rodiklis> Rodikliai { get; init; } = [];
    public List<Darbuotojas> Darbuotojai { get; init; } = [];
}

public class DarbuotojoPerziurosModelis
{
    public required Darbuotojas Darbuotojas { get; init; }
    public SpecialistoKruvis? Kruvis { get; init; }
}

public class DarbuotojoTyrimuModelis
{
    public required Darbuotojas Darbuotojas { get; init; }
    public List<LaboratorijosSpecialistas> Specialistai { get; init; } = [];
    public List<Klonavimas> Tyrimai { get; init; } = [];
}

public class KruvioAnalizesModelis
{
    public List<Rodiklis> Rodikliai { get; init; } = [];

    public static string Formule =>
        $"Darbo krūvis (%) = Σ aktyvių klonavimų trukmė (d.) ÷ (etatas × {KlonavimoNormos.KruvioNorma} d.) × 100";
}

public class KruvioSuvestinesModelis
{
    public DateTime Suformuota { get; init; }
    public List<Rodiklis> Rodikliai { get; init; } = [];
    public List<SpecialistoKruvis> Kruviai { get; init; } = [];
}
