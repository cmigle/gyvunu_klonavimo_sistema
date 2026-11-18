# Reikalavimų modelis

## 1. Sistemos paskirtis

Gyvūnų klonavimo sistema skaitmenizuoja visą klonavimo procesą: sujungia klientų, veterinarijos klinikų ir genetikos laboratorijos veiklą į vieną centralizuotą platformą. Sprendžiama problema – centralizuoto duomenų valdymo trūkumas, dėl kurio sunku sekti užsakymus, užtikrinti tyrimų atsekamumą ir paskirstyti laboratorijos resursus.

Sistemą sudaro penkios posistemės:

| Posistemė | Paskirtis |
|---|---|
| Kliento ir augintinio valdymas | Registracija, prisijungimas, kliento ir augintinių profiliai, dokumentai, teistumo istorijos patikrinimas per CourtListener API |
| Užsakymo valdymas | Klonavimo užsakymų kūrimas, peržiūra, redagavimas, trynimas ir patvirtinimas |
| DNR pavyzdžio valdymas | DNR pavyzdžių registravimas, analizės rezultatų įvedimas ir automatinis tinkamumo nustatymas |
| Inkubatorių valdymas | Inkubatorių parametrai, būsenos ir automatinis inkubatoriaus priskyrimas embrionui |
| Darbuotojų valdymas | Darbuotojų profiliai, jų atliktų tyrimų peržiūra ir automatinė darbo krūvio analizė |

Naudotojų tipai:

- **Klientas** – kuria ir redaguoja užsakymus, kelia dokumentus, gauna užsakymo patvirtinimo laišką;
- **Laboratorijos specialistas** (genetikas, tyrėjas) – atlieka DNR pavyzdžių analizę, įveda rezultatus, valdo inkubatorius;
- **Administratorius** – kuria ir prižiūri darbuotojų profilius, analizuoja jų darbo krūvį.

### 1.2. Funkcijų hierarchija

Kiekviena posistemė turi pagrindines (kūrimo, peržiūros, redagavimo, trynimo) ir automatines funkcijas: užsakymo duomenų pilnumo patikrinimą, kliento augintinių informacijos suvestinę, DNR pavyzdžio tinkamumo nustatymą, inkubatoriaus priskyrimą ir darbuotojų darbo krūvio analizę.

![Funkcijų hierarchijos diagrama](diagramos/funkciju-hierarchija.jpg)

## 2.1. Panaudojimo atvejai

Svečias gali registruotis, sistemos naudotojas – prisijungti ir atsijungti. Užsakymų valdytojas peržiūri užsakymus ir dokumentus. Klientas ir laboratorijos specialistas paveldi užsakymų valdytojo funkcijas: klientas kuria, redaguoja, trina ir tvirtina užsakymus, valdo augintinius ir jų dokumentus, tikrina savo istoriją; specialistas valdo DNR pavyzdžius ir inkubatorius. Administratorius valdo darbuotojus ir analizuoja jų darbo krūvį. Sistema sąveikauja su el. pašto serveriu (užsakymo patvirtinimas) ir CourtListener API (kliento istorijos patikrinimas).

![Panaudojimo atvejų diagrama](diagramos/panaudojimo-atvejai.png)

## 2.2. Panaudojimo atvejų sekų diagramos

### Kliento ir augintinio valdymas

#### Registracija

Svečias atidaro registracijos puslapį ir įveda duomenis. Sistema patikrina jų teisingumą: jei duomenys neteisingi, pateikiamas klaidos pranešimas, jei teisingi – sukuriama paskyra ir atidaromas prisijungimo puslapis.

![PA „Registracija“](diagramos/sekos/02-registracija.png)

#### Prisijungimas

Sistemos naudotojas įveda prisijungimo duomenis. Jei jie klaidingi, pateikiamas klaidos pranešimas, jei teisingi – naudotojas prijungiamas ir atidaromas pradinis puslapis.

![PA „Prisijungimas“](diagramos/sekos/03-prisijungimas.png)

#### Atsijungimas

Naudotojui paspaudus atsijungimo mygtuką, sistema užbaigia sesiją ir grąžina į prisijungimo puslapį.

![PA „Atsijungimas“](diagramos/sekos/04-atsijungimas.png)

#### Peržiūrėti dokumentus

Užsakymų valdytojas atidaro dokumentų sąrašą. Jei augintinis dokumentų neturi, rodomas pranešimas, kitu atveju – sąrašas, iš kurio pasirinktas dokumentas atidaromas peržiūrai.

![PA „Peržiūrėti dokumentus“](diagramos/sekos/05-perziureti-dokumentus.png)

#### Įkelti dokumentą

Klientas pasirenka failą ir įveda dokumento duomenis. Sistema patikrina failo formatą ir duomenis: klaidos atveju pateikia pranešimą, kitu atveju išsaugo dokumentą.

![PA „Įkelti dokumentą“](diagramos/sekos/06-ikelti-dokumenta.png)

#### Redaguoti dokumentą

Klientas pakeičia dokumento duomenis, sistema juos patikrina ir išsaugo arba pateikia klaidos pranešimą.

![PA „Redaguoti dokumentą“](diagramos/sekos/07-redaguoti-dokumenta.png)

#### Ištrinti dokumentą

Sistema pateikia ištrynimo patvirtinimo langą. Atšaukus grįžtama į sąrašą, patvirtinus – dokumentas ištrinamas.

![PA „Ištrinti dokumentą“](diagramos/sekos/08-istrinti-dokumenta.jpg)

#### Patikrinti kliento istoriją

Klientas įveda asmens duomenis, sistema per CourtListener API išsiunčia teistumo istorijos užklausą ir įvertina rezultatus. Radus įrašų klientas pažymimas netinkamu, neradus – patikrintu.

![PA „Patikrinti kliento istoriją“](diagramos/sekos/09-patikrinti-kliento-istorija.png)

#### Sukurti gyvūną

Klientas įveda gyvūno duomenis (vardą, veislę, tipą, amžių, lytį, svorį, mikroschemos numerį). Teisingi duomenys išsaugomi ir atidaromas augintinio informacijos puslapis.

![PA „Sukurti gyvūną“](diagramos/sekos/10-sukurti-gyvuna.png)

### Užsakymo valdymas

#### Peržiūrėti užsakymą

Klientas užsakymų sąraše pasirenka užsakymą, sistema suranda ir pateikia jo duomenis.

![PA „Peržiūrėti užsakymą“](diagramos/sekos/11-perziureti-uzsakyma.png)

#### Sukurti užsakymą

Klientas pasirenka augintinį ir įveda užsakymo duomenis. Sistema pateikia užpildytos formos peržiūrą, patikrina duomenis ir sukuria užsakymą su būsena „Nepatvirtintas“.

![PA „Sukurti užsakymą“](diagramos/sekos/12-sukurti-uzsakyma.png)

#### Redaguoti užsakymą

Redaguoti galima tik nepatvirtintą užsakymą. Pakeisti duomenys patikrinami ir išsaugomi.

![PA „Redaguoti užsakymą“](diagramos/sekos/13-redaguoti-uzsakyma.png)

#### Ištrinti užsakymą

Nepatvirtintam užsakymui pateikiamas ištrynimo patvirtinimo langas; patvirtinto užsakymo ištrinti negalima.

![PA „Ištrinti užsakymą“](diagramos/sekos/14-istrinti-uzsakyma.png)

#### Patvirtinti užsakymą

Sistema patikrina užsakymo būseną ir DNR pavyzdžio tinkamumą. Jei pavyzdys tinkamas klonuoti, užsakymas patvirtinamas ir per el. pašto serverį išsiunčiamas pranešimas.

![PA „Patvirtinti užsakymą“](diagramos/sekos/15-patvirtinti-uzsakyma.png)

### DNR pavyzdžio valdymas

#### Sukurti DNR pavyzdį

Specialistas įveda pavyzdžio duomenis. Teisingi duomenys išsaugomi, pasirinkus sugeneruojama pavyzdžio etiketė.

![PA „Sukurti DNR pavyzdį“](diagramos/sekos/16-sukurti-dnr-pavyzdi.png)

#### Peržiūrėti DNR pavyzdį

Specialistas DNR pavyzdžių sąraše pasirenka pavyzdį, sistema pateikia išsamią jo informaciją.

![PA „Peržiūrėti DNR pavyzdį“](diagramos/sekos/17-perziureti-dnr-pavyzdi.png)

#### Ištrinti DNR pavyzdį

Sistema paprašo patvirtinti trynimą; patvirtinus pavyzdys ištrinamas ir pateikiamas atnaujintas sąrašas.

![PA „Ištrinti DNR pavyzdį“](diagramos/sekos/18-istrinti-dnr-pavyzdi.png)

#### Redaguoti DNR pavyzdį

Sistema pateikia redagavimo formą su esamais duomenimis, patikrina įvestus duomenis ir juos išsaugo.

![PA „Redaguoti DNR pavyzdį“](diagramos/sekos/19-redaguoti-dnr-pavyzdi.png)

#### Nustatyti DNR pavyzdžio tinkamumą

Sistema kiekvieną biologinį parametrą palygina su nustatyta norma ir suskaičiuoja neatitikimus. Jei jų nėra, pavyzdžiui priskiriamas statusas „Tinkamas klonuoti“ ir pateikiamas sertifikatas, kitu atveju – statusas „Netinkamas“ ir atmetimo priežasčių ataskaita.

![PA „Nustatyti DNR pavyzdžio tinkamumą“](diagramos/sekos/20-nustatyti-dnr-tinkamuma.png)

### Darbuotojų valdymas

#### Sukurti darbuotoją

Administratorius įveda darbuotojo duomenis. Jei jie netinkami, sistema nurodo neteisingai užpildytus laukus.

![PA „Sukurti darbuotoją“](diagramos/sekos/21-sukurti-darbuotoja.png)

#### Peržiūrėti darbuotoją

Pasirinkus darbuotoją sąraše, pateikiami jo duomenys.

![PA „Peržiūrėti darbuotoją“](diagramos/sekos/22-perziureti-darbuotoja.png)

#### Redaguoti darbuotoją

Pakeisti duomenys patikrinami; klaidingi laukai nurodomi redagavimo formoje.

![PA „Redaguoti darbuotoją“](diagramos/sekos/23-redaguoti-darbuotoja.png)

#### Ištrinti darbuotoją

Patvirtinus ištrynimą darbuotojas ištrinamas, atšaukus – informuojama, kad jis nebuvo ištrintas.

![PA „Ištrinti darbuotoją“](diagramos/sekos/24-istrinti-darbuotoja.png)

#### Peržiūrėti darbuotojo tyrimus

Sistema pateikia pasirinkto darbuotojo tyrimų sąrašą, o pasirinkus tyrimą – išsamią jo informaciją.

![PA „Peržiūrėti darbuotojo tyrimus“](diagramos/sekos/25-perziureti-darbuotojo-tyrimus.png)

#### Analizuoti darbuotojų krūvį

Kiekvienam laboratorijos specialistui suskaičiuojamas aktyvių klonavimų kiekis ir trukmė, pagal etatą apskaičiuojamas darbo krūvio rodiklis ir priskiriamas lygis: mažas (< 70 %), normalus (70–100 %) arba didelis (> 100 %).

![PA „Analizuoti darbuotojų krūvį“](diagramos/sekos/26-analizuoti-darbuotoju-kruvi.png)

### Inkubatorių valdymas

#### Sukurti inkubatorių

Specialistas užpildo pradinius inkubatoriaus parametrus; sistema juos patikrina ir sukuria inkubatorių arba pateikia klaidos pranešimą.

![PA „Sukurti inkubatorių“](diagramos/sekos/27-sukurti-inkubatoriu.png)

#### Peržiūrėti inkubatoriaus parametrus

Pasirinkus inkubatorių, sistema nuskaito ir pateikia jo duomenis.

![PA „Peržiūrėti inkubatoriaus parametrus“](diagramos/sekos/28-perziureti-inkubatoriu.png)

#### Redaguoti inkubatoriaus būseną

Specialistas keičia inkubatoriaus parametrus; teisingi pakeitimai išsaugomi.

![PA „Redaguoti inkubatoriaus būseną“](diagramos/sekos/29-redaguoti-inkubatoriaus-busena.png)

#### Ištrinti inkubatorių

Sistema patikrina inkubatoriaus būseną: laisvas inkubatorius ištrinamas, užimtam pateikiamas klaidos pranešimas.

![PA „Ištrinti inkubatorių“](diagramos/sekos/30-istrinti-inkubatoriu.jpg)

#### Priskirti inkubatorių

Specialistas pasirenka DNR pavyzdį. Sistema patikrina kiekvieno inkubatoriaus parametrus ir tinkamus įtraukia į sąrašą; jei tinkamų yra, vienas jų priskiriamas, jei nėra – pateikiamas pranešimas su reikiamais parametrais.

![PA „Priskirti inkubatorių“](diagramos/sekos/31-priskirti-inkubatoriu.png)

## 2.3. Dalykinės srities esybių klasės

Diagramą sudaro esybės, apimančios klientų ir jų augintinių informaciją, dokumentus, užsakymus, DNR pavyzdžius, klonavimo procesus, darbuotojus ir inkubatorius. „Klientas“ turi augintinius, kuriems priskiriami dokumentai ir DNR pavyzdžiai. „Užsakymas“ susieja augintinį su klonavimo procesu ir pranešimu apie patvirtinimą. „Klonavimas“ susietas su DNR pavyzdžiu, laboratorijos specialistu ir inkubatoriumi, o inkubatoriaus parametrai fiksuojami esybėje „Inkubatoriaus matavimas“. Laboratorijos specialistas paveldi „Darbuotojo“ atributus, gyvūno tipas apibrėžtas išvardijimu „GyvunoTipas“. Spalvos atitinka komandos narių atsakomybes.

![Dalykinės srities esybių klasių diagrama](diagramos/esybiu-klases.png)

Iš šios diagramos gauta [duomenų bazės schema](duomenu-baze.md).
