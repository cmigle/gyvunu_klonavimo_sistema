using System.Security.Claims;

namespace GyvunuKlonavimoSistema.Infrastructure;

public static class Roles
{
    public const string Klientas = "klientas";
    public const string Specialistas = "specialistas";
    public const string Administratorius = "administratorius";

    public static string Pavadinimas(string? role) => role switch
    {
        Klientas => "Klientas",
        Specialistas => "Laboratorijos specialistas",
        Administratorius => "Administratorius",
        _ => "Svečias"
    };
}

public static class PrisijungesNaudotojas
{
    // Kliento arba darbuotojo identifikatorius, priklausomai nuo rolės
    public static int Id(this ClaimsPrincipal naudotojas) =>
        int.Parse(naudotojas.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    public static string? Role(this ClaimsPrincipal naudotojas) => naudotojas.FindFirstValue(ClaimTypes.Role);

    public static bool Prisijunges(this ClaimsPrincipal naudotojas) => naudotojas.Identity?.IsAuthenticated == true;
}
