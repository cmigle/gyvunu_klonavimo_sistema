using GyvunuKlonavimoSistema.Models;
using GyvunuKlonavimoSistema.Services;

namespace GyvunuKlonavimoSistema.ViewModels;

// Santrauka virš sąrašo: „Augintiniai: 3 | Dokumentai: 4“
public record Rodiklis(string Pavadinimas, object Reiksme);

// Įrašo nėra (ištrintas ar nepasiekiamas) – vietoj lango turinio rodomas pranešimas
public record NeraModelis(string Tekstas, string Atgal, string AtgalTekstas);

public class PagrindinisModelis
{
    public string? Vardas { get; init; }
    public List<Rodiklis> Rodikliai { get; init; } = [];
    public List<Uzsakymas> Uzsakymai { get; init; } = [];
    public List<DnrPavyzdys> NaujausiDnr { get; init; } = [];
    public List<(Darbuotojas Darbuotojas, SpecialistoKruvis? Kruvis)> Darbuotojai { get; init; } = [];
}
