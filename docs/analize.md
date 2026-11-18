# Reikalavimų analizės modelis

## 3.1. Panaudojimo atvejų analizės diagramos

Kiekvienas panaudojimo atvejis išskaidytas į «boundary» (naudotojo sąsajos langai, išorinės sąsajos), «control» (valdikliai) ir «entity» (esybės) klases. Šios klasės tiesiogiai atitinka realizuotos sistemos [MVC architektūrą](architektura.md): langai – rodiniai (*views*), valdikliai – kontroleriai, esybės – modelio klasės.

| Valdiklis | Panaudojimo atvejai |
|---|---|
| Prisijungimo valdiklis | Prisijungti |
| Kliento valdiklis | Registruotis, Atsijungti |
| Istorijos patikrinimo valdiklis | Patikrinti kliento istoriją (per «boundary» CourtListener API sąsają) |
| Augintinio valdiklis | Sukurti, peržiūrėti, redaguoti, ištrinti gyvūną |
| Dokumentų valdiklis | Peržiūrėti dokumentus, įkelti, redaguoti, ištrinti dokumentą |
| Užsakymų valdymo valdiklis | Sukurti, peržiūrėti, redaguoti, ištrinti užsakymą, informuoti apie patvirtinimą (per «boundary» Laiško šabloną) |
| DNR pavyzdžių valdiklis | Sukurti, peržiūrėti, redaguoti, ištrinti DNR pavyzdį, nustatyti tinkamumą |
| Darbuotojo valdiklis | Sukurti, peržiūrėti, redaguoti, ištrinti darbuotoją, peržiūrėti tyrimus, analizuoti krūvį |

### DNR pavyzdžio valdymas

#### Peržiūrėti DNR pavyzdį

Specialistas naudojasi „DNR pavyzdžių puslapiu“, „DNR pavyzdžio peržiūros langu“ arba „Pagrindiniu langu“; komandos perduodamos „DNR pavyzdžių valdikliui“, kuris nuskaito esybę „DNR pavyzdys“.

![Analizės diagrama „Peržiūrėti DNR pavyzdį“](diagramos/analize/01-perziureti-dnr-pavyzdi.png)

#### Ištrinti DNR pavyzdį

Trynimas inicijuojamas iš „DNR pavyzdžių puslapio“ arba „DNR pavyzdžio peržiūros lango“; valdiklis pašalina esybę „DNR pavyzdys“.

![Analizės diagrama „Ištrinti DNR pavyzdį“](diagramos/analize/02-istrinti-dnr-pavyzdi.png)

#### Automatiškai nustatyti DNR pavyzdžio tinkamumą

Valdiklis nuskaito biologinius parametrus iš esybės „DNR pavyzdys“ ir įvertina juos pagal nustatytus slenksčius; rezultatas pateikiamas „DNR pavyzdžio ataskaitos lange“.

![Analizės diagrama „Nustatyti DNR pavyzdžio tinkamumą“](diagramos/analize/03-nustatyti-dnr-tinkamuma.png)

#### Redaguoti DNR pavyzdį

„DNR pavyzdžio redagavimo forma“ perduoda atnaujintus duomenis valdikliui, kuris išsaugo juos esybėje „DNR pavyzdys“.

![Analizės diagrama „Redaguoti DNR pavyzdį“](diagramos/analize/04-redaguoti-dnr-pavyzdi.png)

#### Sukurti DNR pavyzdį

„Naujo DNR pavyzdžio įvedimo langas“ perduoda duomenis valdikliui, kuris sukuria naują esybę „DNR pavyzdys“.

![Analizės diagrama „Sukurti DNR pavyzdį“](diagramos/analize/05-sukurti-dnr-pavyzdi.png)

### Užsakymo valdymas

#### Peržiūrėti užsakymą

„Užsakymų sąrašo langas“, „Užsakymo peržiūros langas“ ir „Pagrindinis langas“ perduoda komandas „Užsakymų valdymo valdikliui“, kuris nuskaito esybę „Užsakymas“.

![Analizės diagrama „Peržiūrėti užsakymą“](diagramos/analize/06-perziureti-uzsakyma.png)

#### Automatinis kliento informavimas apie užsakymo patvirtinimą

Valdiklis, naudodamas esybes „Užsakymas“ ir „Pranešimas“, suformuoja pranešimą pagal „Laiško šabloną“ ir perduoda jį išoriniam „El. pašto serveriui“.

![Analizės diagrama „Informuoti apie užsakymo patvirtinimą“](diagramos/analize/17-informuoti-apie-patvirtinima.jpg)

#### Ištrinti užsakymą

![Analizės diagrama „Ištrinti užsakymą“](diagramos/analize/18-istrinti-uzsakyma.png)

#### Redaguoti užsakymą

„Užsakymų sąrašo langas“ ir „Užsakymo redagavimo langas“ perduoda komandas valdikliui, kuris atnaujina esybę „Užsakymas“.

![Analizės diagrama „Redaguoti užsakymą“](diagramos/analize/19-redaguoti-uzsakyma.png)

#### Sukurti užsakymą

Valdiklis nuskaito kliento ir augintinio duomenis ir sukuria naują užsakymą, naudodamas esybes „Užsakymas“, „Augintinis“ ir „Klientas“.

![Analizės diagrama „Sukurti užsakymą“](diagramos/analize/20-sukurti-uzsakyma.png)

### Kliento ir augintinio valdymas

#### Prisijungti

„Prisijungimo langas“ perduoda duomenis „Prisijungimo valdikliui“, kuris juos patikrina pagal esybes „Klientas“ ir „Darbuotojas“.

![Analizės diagrama „Prisijungti“](diagramos/analize/07-prisijungti.png)

#### Registruotis

Svečias iš „Pradžios lango“ pereina į „Registracijos langą“; „Kliento valdiklis“ patikrina duomenis ir sukuria esybę „Klientas“.

![Analizės diagrama „Registruotis“](diagramos/analize/27-registruotis.png)

#### Atsijungti

Atsijungimas inicijuojamas per „Pagrindinį langą“ arba „Atsijungimo langą“; „Kliento valdiklis“ užbaigia sesiją.

![Analizės diagrama „Atsijungti“](diagramos/analize/28-atsijungti.png)

#### Sukurti gyvūną

„Augintinio kūrimo langas“ perduoda duomenis „Augintinio valdikliui“, kuris, naudodamas esybes „Klientas“ ir „Augintinis“, sukuria naują augintinį ir pateikia jį „Augintinio peržiūros lange“.

![Analizės diagrama „Sukurti gyvūną“](diagramos/analize/08-sukurti-gyvuna.png)

#### Redaguoti gyvūną

![Analizės diagrama „Redaguoti gyvūną“](diagramos/analize/09-redaguoti-gyvuna.png)

#### Peržiūrėti gyvūną

![Analizės diagrama „Peržiūrėti gyvūną“](diagramos/analize/10-perziureti-gyvuna.png)

#### Ištrinti gyvūną

Patvirtinus ištrynimą „Ištrynimo patvirtinimo lange“, valdiklis pašalina esybę „Augintinis“ ir su ja susijusius dokumentus.

![Analizės diagrama „Ištrinti gyvūną“](diagramos/analize/11-istrinti-gyvuna.png)

#### Peržiūrėti dokumentus

„Dokumentų valdiklis“ nuskaito augintinio dokumentus iš esybių „Augintinis“ ir „Dokumentas“.

![Analizės diagrama „Peržiūrėti dokumentus“](diagramos/analize/12-perziureti-dokumentus.png)

#### Įkelti dokumentą

Valdiklis patikrina failo formatą ir, susiedamas su esybe „Augintinis“, sukuria esybę „Dokumentas“.

![Analizės diagrama „Įkelti dokumentą“](diagramos/analize/13-ikelti-dokumenta.jpg)

#### Redaguoti dokumentą

![Analizės diagrama „Redaguoti dokumentą“](diagramos/analize/14-redaguoti-dokumenta.png)

#### Ištrinti dokumentą

![Analizės diagrama „Ištrinti dokumentą“](diagramos/analize/15-istrinti-dokumenta.png)

#### Patikrinti kliento istoriją

„Istorijos patikrinimo valdiklis“ per „CourtListener API sąsają“ išsiunčia užklausą išoriniam serveriui, įvertina rezultatus ir atnaujina esybę „Klientas“.

![Analizės diagrama „Patikrinti kliento istoriją“](diagramos/analize/16-patikrinti-kliento-istorija.png)

### Darbuotojų valdymas

#### Peržiūrėti darbuotoją

![Analizės diagrama „Peržiūrėti darbuotoją“](diagramos/analize/21-perziureti-darbuotoja.png)

#### Sukurti darbuotoją

„Darbuotojo valdiklis“ patikrina duomenis ir sukuria esybę „Darbuotojas“; klaidingi laukai nurodomi kūrimo lange.

![Analizės diagrama „Sukurti darbuotoją“](diagramos/analize/22-sukurti-darbuotoja.png)

#### Redaguoti darbuotoją

![Analizės diagrama „Redaguoti darbuotoją“](diagramos/analize/23-redaguoti-darbuotoja.png)

#### Ištrinti darbuotoją

![Analizės diagrama „Ištrinti darbuotoją“](diagramos/analize/24-istrinti-darbuotoja.png)

#### Peržiūrėti darbuotojo tyrimus

Valdiklis gauna tyrimų duomenis iš esybių „Darbuotojas“, „Laboratorijos specialistas“ ir „Klonavimas“.

![Analizės diagrama „Peržiūrėti darbuotojo tyrimus“](diagramos/analize/25-perziureti-darbuotojo-tyrimus.png)

#### Analizuoti darbuotojų krūvį

![Analizės diagrama „Analizuoti darbuotojų krūvį“](diagramos/analize/26-analizuoti-darbuotoju-kruvi.png)

## 3.2. Naudotojo sąsajos modelis

Navigavimo planas – «boundary» klasių diagrama, rodanti perėjimus tarp langų. Langų sąrašas ir perėjimai tekstu – [`prototype/navigavimo-planas.txt`](../prototype/navigavimo-planas.txt), kiekvieno lango ekrano kopijos – [`prototype/ekrano-kopijos/`](../prototype/ekrano-kopijos/).

![Naudotojo navigavimo planas](../prototype/navigavimo-planas-schema.png)

## 3.3. Duomenų srautai

#### DNR pavyzdžių valdymas

Laboratorijos specialistas keičiasi duomenimis su penkiais procesais: DNR pavyzdžio kūrimu, peržiūra, redagavimu, trynimu ir automatiniu tinkamumo nustatymu; visi jie naudoja saugyklą „DNR pavyzdys“.

![DNR pavyzdžių duomenų srautų diagrama](diagramos/duomenu-srautai/1-dnr-pavyzdziai.jpg)

#### Užsakymų valdymas

Klientas kuria, peržiūri, redaguoja ir trina užsakymus. Užsakymo duomenys perduodami automatinio informavimo procesui, kuris įrašo pranešimą į saugyklą „Pranešimas“ ir išsiunčia laišką per išorinį el. pašto serverį.

![Užsakymų posistemės duomenų srautų diagrama](diagramos/duomenu-srautai/2-uzsakymai.png)

#### Darbuotojų valdymas

Administratorius keičiasi duomenimis su šešiais procesais: darbuotojo kūrimu, peržiūra, redagavimu, trynimu, tyrimų peržiūra ir krūvio analize. Tyrimams ir krūvio analizei papildomai naudojamos saugyklos „Laboratorijos specialistas“ ir „Klonavimas“.

![Darbuotojų posistemės duomenų srautų diagrama](diagramos/duomenu-srautai/3-darbuotojai.png)

#### Kliento ir augintinio valdymas

Svečias ir sistemos naudotojas pateikia registracijos ir prisijungimo duomenis (saugykla „Klientas“). Klientas valdo augintinius ir dokumentus (saugyklos „Augintinis“, „Dokumentas“). Istorijos patikrinimo procesas kreipiasi į išorinį CourtListener API serverį ir rezultatą įrašo į saugyklą „Klientas“.

![Kliento ir augintinio valdymo duomenų srautų diagrama](diagramos/duomenu-srautai/4-klientai-ir-augintiniai.png)
