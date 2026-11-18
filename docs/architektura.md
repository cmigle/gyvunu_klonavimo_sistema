# Sistemos architektūra

Sistema realizuota ASP.NET Core MVC karkasu. Reikalavimų analizės modelio klasės ([3.1 skyrius](analize.md)) tiesiogiai tapo MVC dalimis:

| Analizės stereotipas | MVC dalis | Realizacija |
|---|---|---|
| «boundary» – naudotojo sąsajos langas | View | Razor rodinys `Views/<Valdiklis>/<Veiksmas>.cshtml`, pavadintas kaip langas navigavimo plane |
| «boundary» – išorinė sąsaja | – | `ICourtListenerSasaja` (CourtListener API), `LaiskoSablonas` + `IElPastoServeris` (el. pašto serveris) |
| «control» – valdiklis | Controller | `Controllers/<Pavadinimas>Controller.cs`, vienas veiksmas – vienas panaudojimo atvejo žingsnis |
| «control» – automatinė funkcija | Controller pagalbinė klasė | `Services/` – DNR tinkamumo analizė, inkubatoriaus priskyrimas, darbo krūvio analizė, užsakymo patvirtinimas |
| «entity» – esybė | Model | `Models/` klasės, per EF Core susietos su [duomenų bazės schemos](duomenu-baze.md) lentelėmis |

## Sluoksniai

```mermaid
flowchart LR
    naudotojas(["Naršyklė"])
    subgraph View["View – «boundary»"]
        rodiniai["Razor rodiniai<br/>Views/*/*.cshtml"]
    end
    subgraph Controller["Controller – «control»"]
        valdikliai["Valdikliai<br/>Controllers/*Controller.cs"]
        funkcijos["Automatinės funkcijos<br/>Services/*"]
    end
    subgraph Model["Model – «entity»"]
        esybes["Esybių klasės<br/>Models/*"]
        kontekstas["KlonavimoDbContext<br/>(EF Core)"]
    end
    db[("MySQL 8.4<br/>gyvunu_klonavimas")]
    court(["CourtListener API"])
    smtp(["El. pašto serveris"])

    naudotojas -- "HTTP užklausa" --> valdikliai
    valdikliai -- "modelis" --> rodiniai
    rodiniai -- "HTML" --> naudotojas
    valdikliai --> funkcijos
    valdikliai --> kontekstas
    funkcijos --> kontekstas
    kontekstas --> esybes
    kontekstas -- "SQL" --> db
    funkcijos -- "HTTPS" --> court
    funkcijos -- "SMTP" --> smtp
```

- **Užklausos eiga**: naršyklė kreipiasi į valdiklio veiksmą (`/uzsakymai/redaguoti/5` → `UzsakymaiController.Redaguoti(5)`), valdiklis per `KlonavimoDbContext` nuskaito ar keičia esybes, prireikus kviečia automatinę funkciją ir grąžina rodinį su modeliu arba nukreipia į kitą langą su pranešimu (*Post/Redirect/Get*).
- **Prieiga pagal rolę**: slapukų autentifikacija, rolės `klientas`, `specialistas`, `administratorius`; valdikliai pažymėti `[Authorize(Roles = …)]`. Slaptažodžiai saugomi kaip PBKDF2 maišos.
- **Duomenų bazė kuriama iš schemos**: paleidžiant programą, jei duomenų bazės nėra, vykdomas tas pats [`database/schema.sql`](../database/schema.sql), kuris pateiktas dokumentacijoje, todėl naudojama DB visada atitinka suprojektuotą schemą. EF Core naudojamas tik duomenims skaityti ir rašyti.
- **Automatinės funkcijos** parašytos kaip grynos funkcijos (be DB), todėl jas tikrina vienetų testai (`tests/`).
- **Išorinės sąsajos** paslėptos už sąsajų: be CourtListener rakto naudojama demonstracinė paieška, be SMTP serverio laiškai įrašomi kaip `.eml` failai.

## Klasės pagal posistemes

Kiekvienoje diagramoje: kairėje – langai («boundary»), viduryje – valdiklis ir jo veiksmai («control»), dešinėje – esybės («entity»). Rodyklės rodo, kurie langai kreipiasi į valdiklį ir kurias esybes valdiklis naudoja, kaip analizės diagramose.

### Bendri langai, prisijungimas ir registracija

```mermaid
classDiagram
    direction LR
    class PradziosLangas["Pradžios / Pagrindinis langas"] {
        <<boundary>>
        Pagrindinis/Index
    }
    class PrisijungimoLangas["Prisijungimo langas"] {
        <<boundary>>
        Prisijungimas/Index
    }
    class RegistracijosLangas["Registracijos langas"] {
        <<boundary>>
        Klientas/Registracija
    }
    class AtsijungimoLangas["Atsijungimo langas"] {
        <<boundary>>
        Klientas/Atsijungimas
    }
    class PagrindinisController {
        <<control>>
        +Index()
    }
    class PrisijungimasController["PrisijungimasController (Prisijungimo valdiklis)"] {
        <<control>>
        +Index(PrisijungimoForma)
        +Draudziama()
    }
    class KlientasController["KlientasController (Kliento valdiklis)"] {
        <<control>>
        +Registracija(RegistracijosForma)
        +Atsijungimas()
    }
    class Klientas { <<entity>> }
    class Darbuotojas { <<entity>> }
    class LaboratorijosSpecialistas { <<entity>> }

    PradziosLangas --> PagrindinisController
    PrisijungimoLangas --> PrisijungimasController
    RegistracijosLangas --> KlientasController
    AtsijungimoLangas --> KlientasController
    PrisijungimasController --> Klientas
    PrisijungimasController --> Darbuotojas
    KlientasController --> Klientas
    Darbuotojas <|-- LaboratorijosSpecialistas
```

### Kliento ir augintinio valdymas

```mermaid
classDiagram
    direction LR
    class KlientoProfilioLangas["Kliento profilio langas"] { <<boundary>> }
    class ProfilioRedagavimoLangas["Kliento profilio redagavimo langas"] { <<boundary>> }
    class IstorijosPatikrinimoLangas["Kliento istorijos patikrinimo langas"] { <<boundary>> }
    class PatikrinimoRezultatoLangas["Patikrinimo rezultato langas"] { <<boundary>> }
    class CourtListenerSasaja["CourtListener API sąsaja"] {
        <<boundary>>
        +IeskotiAsync(vardas, pavarde)
    }
    class AugintiniuSarasoLangas["Augintinių sąrašo langas"] { <<boundary>> }
    class AugintinioLangai["Augintinio kūrimo / peržiūros / redagavimo langai"] { <<boundary>> }
    class DokumentuSarasoLangas["Dokumentų sąrašo langas"] { <<boundary>> }
    class DokumentoLangai["Dokumento įkėlimo / peržiūros / redagavimo langai"] { <<boundary>> }
    class IstrynimoPatvirtinimoLangas["Ištrynimo patvirtinimo langas"] { <<boundary>> }

    class KlientasController["KlientasController (Kliento valdiklis)"] {
        <<control>>
        +Profilis()
        +Redaguoti(forma)
        +Istrinti()
    }
    class IstorijosPatikrinimasController["IstorijosPatikrinimasController (Istorijos patikrinimo valdiklis)"] {
        <<control>>
        +Index(forma)
        +Rezultatas()
    }
    class AugintiniaiController["AugintiniaiController (Augintinio valdiklis)"] {
        <<control>>
        +Index()
        +Kurti(forma)
        +Perziura(id)
        +Redaguoti(id, forma)
        +Istrinti(id)
    }
    class DokumentaiController["DokumentaiController (Dokumentų valdiklis)"] {
        <<control>>
        +Index(augintinisId)
        +Ikelti(augintinisId, forma)
        +Perziura(id)
        +Redaguoti(id, forma)
        +Istrinti(id)
        +Failas(id)
    }
    class Klientas { <<entity>> }
    class Augintinis { <<entity>> }
    class Dokumentas { <<entity>> }

    KlientoProfilioLangas --> KlientasController
    ProfilioRedagavimoLangas --> KlientasController
    IstorijosPatikrinimoLangas --> IstorijosPatikrinimasController
    PatikrinimoRezultatoLangas --> IstorijosPatikrinimasController
    IstorijosPatikrinimasController --> CourtListenerSasaja
    AugintiniuSarasoLangas --> AugintiniaiController
    AugintinioLangai --> AugintiniaiController
    DokumentuSarasoLangas --> DokumentaiController
    DokumentoLangai --> DokumentaiController
    IstrynimoPatvirtinimoLangas --> AugintiniaiController
    IstrynimoPatvirtinimoLangas --> DokumentaiController
    KlientasController --> Klientas
    IstorijosPatikrinimasController --> Klientas
    AugintiniaiController --> Klientas
    AugintiniaiController --> Augintinis
    DokumentaiController --> Augintinis
    DokumentaiController --> Dokumentas
```

### Užsakymo valdymas

```mermaid
classDiagram
    direction LR
    class UzsakymuSarasoLangas["Užsakymų sąrašo langas"] { <<boundary>> }
    class UzsakymoKurimoLangas["Užsakymo kūrimo langas"] { <<boundary>> }
    class UzsakymoPerziurosLangas["Užsakymo peržiūros langas"] { <<boundary>> }
    class UzsakymoRedagavimoLangas["Užsakymo redagavimo langas"] { <<boundary>> }
    class LaiskoSablonas["Laiško šablonas"] {
        <<boundary>>
        +UzsakymoPatvirtinimas(uzsakymas, augintinis, klientas, pavyzdys)
    }
    class ElPastoServeris["IElPastoServeris (El. pašto serveris)"] {
        <<boundary>>
        +Siusti(laiskas)
    }
    class UzsakymaiController["UzsakymaiController (Užsakymų valdymo valdiklis)"] {
        <<control>>
        +Index()
        +Kurti(forma)
        +Perziura(id)
        +Redaguoti(id, forma)
        +Istrinti(id)
    }
    class UzsakymoPatvirtinimas {
        <<control>>
        +PatvirtintiJeiTinkaAsync(augintinisId)
    }
    class Uzsakymas { <<entity>> }
    class Pranesimas { <<entity>> }
    class Augintinis { <<entity>> }
    class Klientas { <<entity>> }

    UzsakymuSarasoLangas --> UzsakymaiController
    UzsakymoKurimoLangas --> UzsakymaiController
    UzsakymoPerziurosLangas --> UzsakymaiController
    UzsakymoRedagavimoLangas --> UzsakymaiController
    UzsakymaiController --> UzsakymoPatvirtinimas
    UzsakymoPatvirtinimas --> LaiskoSablonas
    LaiskoSablonas --> ElPastoServeris
    UzsakymaiController --> Uzsakymas
    UzsakymaiController --> Augintinis
    UzsakymaiController --> Klientas
    UzsakymoPatvirtinimas --> Uzsakymas
    UzsakymoPatvirtinimas --> Pranesimas
```

### DNR pavyzdžio valdymas

```mermaid
classDiagram
    direction LR
    class DnrPavyzdziuPuslapis["DNR pavyzdžių puslapis"] { <<boundary>> }
    class NaujoDnrIvedimoLangas["Naujo DNR pavyzdžio įvedimo langas"] { <<boundary>> }
    class DnrPerziurosLangas["DNR pavyzdžio peržiūros langas"] { <<boundary>> }
    class DnrRedagavimoForma["DNR pavyzdžio redagavimo forma"] { <<boundary>> }
    class DnrAtaskaitosLangas["DNR pavyzdžio ataskaitos langas"] { <<boundary>> }
    class DnrPavyzdziaiController["DnrPavyzdziaiController (DNR pavyzdžių valdiklis)"] {
        <<control>>
        +Index()
        +Kurti(forma)
        +Perziura(id)
        +Redaguoti(id, forma)
        +Istrinti(id)
        +NustatytiTinkamuma(id)
        +Ataskaita(id)
    }
    class DnrTinkamumoAnalize {
        <<control>>
        +Analizuoti(pavyzdys) TinkamumoRezultatas
    }
    class UzsakymoPatvirtinimas { <<control>> }
    class DnrPavyzdys { <<entity>> }
    class Augintinis { <<entity>> }

    DnrPavyzdziuPuslapis --> DnrPavyzdziaiController
    NaujoDnrIvedimoLangas --> DnrPavyzdziaiController
    DnrPerziurosLangas --> DnrPavyzdziaiController
    DnrRedagavimoForma --> DnrPavyzdziaiController
    DnrAtaskaitosLangas --> DnrPavyzdziaiController
    DnrPavyzdziaiController --> DnrTinkamumoAnalize
    DnrPavyzdziaiController --> UzsakymoPatvirtinimas
    DnrPavyzdziaiController --> DnrPavyzdys
    DnrPavyzdziaiController --> Augintinis
```

### Inkubatorių valdymas

```mermaid
classDiagram
    direction LR
    class InkubatoriuLangas["Inkubatorių langas"] { <<boundary>> }
    class NaujoInkubatoriausLangas["Naujo inkubatoriaus kūrimo langas"] { <<boundary>> }
    class ParametruLangas["Inkubatoriaus parametrų langas"] { <<boundary>> }
    class BusenosLangas["Inkubatoriaus būsenos redagavimo langas"] { <<boundary>> }
    class PriskyrimoRezultatoLangas["Inkubatoriaus priskyrimo rezultato langas"] { <<boundary>> }
    class InkubatoriaiController["InkubatoriaiController (Inkubatorių valdiklis)"] {
        <<control>>
        +Index()
        +Kurti(forma)
        +Parametrai(id)
        +Busena(id, forma)
        +Istrinti(id)
        +Priskirti(dnrPavyzdysId)
        +PriskyrimoRezultatas(dnrPavyzdysId)
    }
    class InkubatoriausPriskyrimas {
        <<control>>
        +Ivertinti(tipas, inkubatoriai) PriskyrimoRezultatas
    }
    class Inkubatorius { <<entity>> }
    class InkubatoriausMatavimas { <<entity>> }
    class Klonavimas { <<entity>> }
    class DnrPavyzdys { <<entity>> }

    InkubatoriuLangas --> InkubatoriaiController
    NaujoInkubatoriausLangas --> InkubatoriaiController
    ParametruLangas --> InkubatoriaiController
    BusenosLangas --> InkubatoriaiController
    PriskyrimoRezultatoLangas --> InkubatoriaiController
    InkubatoriaiController --> InkubatoriausPriskyrimas
    InkubatoriaiController --> Inkubatorius
    InkubatoriaiController --> InkubatoriausMatavimas
    InkubatoriaiController --> Klonavimas
    InkubatoriaiController --> DnrPavyzdys
```

### Darbuotojų valdymas

```mermaid
classDiagram
    direction LR
    class DarbuotojuSarasoLangas["Darbuotojų sąrašo langas"] { <<boundary>> }
    class DarbuotojoLangai["Darbuotojo sukūrimo / peržiūros / redagavimo langai"] { <<boundary>> }
    class TyrimuSarasoLangas["Darbuotojo tyrimų sąrašo langas"] { <<boundary>> }
    class TyrimoPerziurosLangas["Tyrimo peržiūros langas"] { <<boundary>> }
    class KruvioAnalizesLangas["Darbo krūvio analizės langas"] { <<boundary>> }
    class KruvioSuvestinesLangas["Darbo krūvio suvestinės langas"] { <<boundary>> }
    class DarbuotojaiController["DarbuotojaiController (Darbuotojo valdiklis)"] {
        <<control>>
        +Index()
        +Kurti(forma)
        +Perziura(id)
        +Redaguoti(id, forma)
        +Istrinti(id)
        +Tyrimai(id)
        +Tyrimas(id)
        +KruvioAnalize()
        +AtliktiKruvioAnalize()
        +KruvioSuvestine()
    }
    class DarboKruvioAnalize {
        <<control>>
        +Analizuoti(specialistai) List~SpecialistoKruvis~
    }
    class Darbuotojas { <<entity>> }
    class LaboratorijosSpecialistas { <<entity>> }
    class Klonavimas { <<entity>> }

    DarbuotojuSarasoLangas --> DarbuotojaiController
    DarbuotojoLangai --> DarbuotojaiController
    TyrimuSarasoLangas --> DarbuotojaiController
    TyrimoPerziurosLangas --> DarbuotojaiController
    KruvioAnalizesLangas --> DarbuotojaiController
    KruvioSuvestinesLangas --> DarbuotojaiController
    DarbuotojaiController --> DarboKruvioAnalize
    DarbuotojaiController --> Darbuotojas
    DarbuotojaiController --> LaboratorijosSpecialistas
    DarbuotojaiController --> Klonavimas
    Darbuotojas <|-- LaboratorijosSpecialistas
```

## Esybės («entity»)

Modelio klasės sutampa su [esybių klasių diagrama](reikalavimai.md#23-dalykinės-srities-esybių-klasės): `Klientas`, `Augintinis`, `GyvunoTipas`, `Dokumentas`, `Uzsakymas`, `Pranesimas`, `DnrPavyzdys`, `Klonavimas`, `Inkubatorius`, `InkubatoriausMatavimas`, `Darbuotojas`, `LaboratorijosSpecialistas`. `KlonavimoDbContext` jas susieja su lentelėmis: savybė `DnrPavyzdysId` → stulpelis `dnr_pavyzdys_id`, `GyvunoTipas` → `ENUM`, paveldėjimas → atskira lentelė `laboratorijos_specialistas` (*table per type*).

## Projekto struktūra

```text
database/schema.sql               duomenų bazės schema (MySQL 8.4)
src/GyvunuKlonavimoSistema/
├── Controllers/                  «control»: valdikliai
├── Services/                     «control»: automatinės funkcijos; «boundary»: išorinės sąsajos
├── Models/                       «entity»: esybių klasės
├── Data/                         EF Core kontekstas, DB kūrimas iš schemos, demonstraciniai duomenys
├── ViewModels/                   formų ir langų modeliai su tikrinimo taisyklėmis
├── Views/                        «boundary»: langai (Razor rodiniai)
├── Infrastructure/               rolės, meniu, formatavimas, būsenų žymos
└── wwwroot/                      stilius ir naršyklės skriptas
tests/GyvunuKlonavimoSistema.Tests/   automatinių funkcijų vienetų testai
prototype/                        GUI prototipas (3 lab. darbas)
docs/                             UML modeliai ir šis aprašas
```

## Technologijos

| Sritis | Technologija |
|---|---|
| Kalba ir karkasas | C# 14, .NET 10, ASP.NET Core MVC (Razor) |
| Duomenų bazė | MySQL 8.4, EF Core 10 su `MySql.EntityFrameworkCore` tiekėju |
| Testai | xUnit |
| CI | GitHub Actions: kompiliavimas, testai ir paleidimas su MySQL konteineriu |
| Modeliavimas | Magic Systems of Systems Architect |
