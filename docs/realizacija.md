# 4.3. Sistemos realizacija

Realizuotos sistemos langai su demonstraciniais duomenimis. Langų pavadinimai sutampa su navigavimo plano «boundary» klasėmis, o spalvotas kvadratėlis lango antraštėje rodo posistemę pagal komandos spalvinę legendą. Ekrano kopijos padarytos ką tik sukurtoje demonstracinėje duomenų bazėje (`dotnet run -- --atkurti-duomenis`).

## Bendri langai

**Pradžios langas** – neprisijungusiam naudotojui siūloma prisijungti arba užsiregistruoti.

![Pradžios langas](ekrano-kopijos/pradzios-langas.png)

**Prisijungimo langas** – prisijungia ir klientai, ir darbuotojai. Neteisingi duomenys pažymimi formoje.

![Prisijungimo langas](ekrano-kopijos/prisijungimo-langas.png)

![Prisijungimo langas su klaida](ekrano-kopijos/prisijungimo-langas-klaida.png)

**Pagrindinis langas** – kiekvienai rolei rodomi jos rodikliai, veiksmai ir svarbiausi įrašai.

![Pagrindinis langas – klientas](ekrano-kopijos/pagrindinis-langas-klientas.png)

![Pagrindinis langas – laboratorijos specialistas](ekrano-kopijos/pagrindinis-langas-specialistas.png)

![Pagrindinis langas – administratorius](ekrano-kopijos/pagrindinis-langas-administratorius.png)

**Atsijungimo langas**

![Atsijungimo langas](ekrano-kopijos/atsijungimo-langas.png)

## Kliento ir augintinio valdymas

**Registracijos langas**

![Registracijos langas](ekrano-kopijos/registracijos-langas.png)

**Kliento profilio langas** ir **Kliento profilio redagavimo langas** – pakeitus vardą, pavardę ar asmens kodą, teistumo istoriją reikia patikrinti iš naujo.

![Kliento profilio langas](ekrano-kopijos/kliento-profilio-langas.png)

![Kliento profilio redagavimo langas](ekrano-kopijos/kliento-profilio-redagavimo-langas.png)

**Kliento istorijos patikrinimo langas** ir **Patikrinimo rezultato langas** – užklausa siunčiama per CourtListener API sąsają. Radus įrašų klientas pažymimas netinkamu, neradus – patikrintu; jei patikrinimas pradėtas kuriant užsakymą, siūloma jį tęsti.

![Kliento istorijos patikrinimo langas](ekrano-kopijos/kliento-istorijos-patikrinimo-langas.png)

![Patikrinimo rezultato langas – rasta įrašų](ekrano-kopijos/patikrinimo-rezultato-langas-rasta-irasu.png)

![Patikrinimo rezultato langas – įrašų nerasta](ekrano-kopijos/patikrinimo-rezultato-langas-irasu-nerasta.png)

**Augintinių sąrašo langas**, **Augintinio kūrimo**, **peržiūros** ir **redagavimo langai**

![Augintinių sąrašo langas](ekrano-kopijos/augintiniu-saraso-langas.png)

![Augintinio kūrimo langas](ekrano-kopijos/augintinio-kurimo-langas.png)

![Augintinio peržiūros langas](ekrano-kopijos/augintinio-perziuros-langas.png)

![Augintinio redagavimo langas](ekrano-kopijos/augintinio-redagavimo-langas.png)

**Dokumentų sąrašo langas** – augintiniui be dokumentų rodomas pranešimas „Dokumentų nėra“.

![Dokumentų sąrašo langas](ekrano-kopijos/dokumentu-saraso-langas.png)

![Dokumentų sąrašo langas be dokumentų](ekrano-kopijos/dokumentu-saraso-langas-tuscias.png)

**Dokumento įkėlimo**, **peržiūros** ir **redagavimo langai** – leidžiami PDF, JPG ir PNG failai iki 10 MB.

![Dokumento įkėlimo langas](ekrano-kopijos/dokumento-ikelimo-langas.png)

![Dokumento peržiūros langas](ekrano-kopijos/dokumento-perziuros-langas.png)

![Dokumento redagavimo langas](ekrano-kopijos/dokumento-redagavimo-langas.png)

## Užsakymo valdymas

**Užsakymų sąrašo langas** – klientas mato savo užsakymus, laboratorijos specialistas – visus.

![Užsakymų sąrašo langas – klientas](ekrano-kopijos/uzsakymu-saraso-langas-klientas.png)

![Užsakymų sąrašo langas – specialistas](ekrano-kopijos/uzsakymu-saraso-langas-specialistas.png)

**Užsakymo kūrimo langas** – rodoma užpildytos formos peržiūra; klientas, kurio istorija nepatikrinta, nukreipiamas į istorijos patikrinimą («include»).

![Užsakymo kūrimo langas](ekrano-kopijos/uzsakymo-kurimo-langas.png)

![Užsakymo kūrimo langas – formos peržiūra](ekrano-kopijos/uzsakymo-kurimo-langas-perziura.png)

![Užsakymo kūrimo langas – istorija nepatikrinta](ekrano-kopijos/uzsakymo-kurimo-langas-istorija-nepatikrinta.png)

**Užsakymo peržiūros langas** – patvirtintam užsakymui rodomas klientui išsiųstas laiškas. Užsakymas UZ-0002 patvirtintas automatiškai, kai specialistas nustatė, kad DNR-0002 tinkamas klonuoti.

![Užsakymo peržiūros langas](ekrano-kopijos/uzsakymo-perziuros-langas.png)

![Automatiškai patvirtintas užsakymas](ekrano-kopijos/uzsakymo-perziuros-langas-patvirtintas-automatiskai.png)

**Užsakymo redagavimo langas** – redaguoti galima tik nepatvirtintą užsakymą.

![Užsakymo redagavimo langas](ekrano-kopijos/uzsakymo-redagavimo-langas.png)

## DNR pavyzdžio valdymas

**DNR pavyzdžių puslapis**, **Naujo DNR pavyzdžio įvedimo langas**, **DNR pavyzdžio peržiūros langas** ir **redagavimo forma** (joje įvedami analizės rezultatai)

![DNR pavyzdžių puslapis](ekrano-kopijos/dnr-pavyzdziu-puslapis.png)

![Naujo DNR pavyzdžio įvedimo langas](ekrano-kopijos/naujo-dnr-pavyzdzio-ivedimo-langas.png)

![DNR pavyzdžio peržiūros langas](ekrano-kopijos/dnr-pavyzdzio-perziuros-langas.png)

![DNR pavyzdžio redagavimo forma](ekrano-kopijos/dnr-pavyzdzio-redagavimo-forma.png)

**DNR pavyzdžio ataskaitos langas** – tinkamam pavyzdžiui pateikiamas sertifikatas, netinkamam – atmetimo ataskaita su neatitinkančiais parametrais.

![Ataskaita – tinkamas klonuoti](ekrano-kopijos/dnr-pavyzdzio-ataskaitos-langas-tinkamas.png)

![Ataskaita – netinkamas](ekrano-kopijos/dnr-pavyzdzio-ataskaitos-langas-netinkamas.png)

## Inkubatorių valdymas

**Inkubatorių langas** su priskyrimo forma, **Naujo inkubatoriaus kūrimo langas**, **Inkubatoriaus parametrų langas** ir **Inkubatoriaus būsenos redagavimo langas**

![Inkubatorių langas](ekrano-kopijos/inkubatoriu-langas.png)

![Naujo inkubatoriaus kūrimo langas](ekrano-kopijos/naujo-inkubatoriaus-kurimo-langas.png)

![Inkubatoriaus parametrų langas](ekrano-kopijos/inkubatoriaus-parametru-langas.png)

![Inkubatoriaus būsenos redagavimo langas](ekrano-kopijos/inkubatoriaus-busenos-redagavimo-langas.png)

**Inkubatoriaus priskyrimo rezultato langas** – papūgai Kiki priskiriamas vienintelis tinkamas inkubatorius; triušiui Pūkiui tinkamo nėra, todėl nurodomi reikalingi parametrai.

![Inkubatorius priskirtas](ekrano-kopijos/inkubatoriaus-priskyrimo-rezultato-langas-priskirtas.png)

![Tinkamo inkubatoriaus nėra](ekrano-kopijos/inkubatoriaus-priskyrimo-rezultato-langas-nera-tinkamo.png)

## Darbuotojų valdymas

**Darbuotojų sąrašo langas**, **Darbuotojo sukūrimo langas** (neteisingai užpildyti laukai pažymimi), **peržiūros** ir **redagavimo langai**

![Darbuotojų sąrašo langas](ekrano-kopijos/darbuotoju-saraso-langas.png)

![Darbuotojo sukūrimo langas](ekrano-kopijos/darbuotojo-sukurimo-langas.png)

![Darbuotojo sukūrimo langas su klaidomis](ekrano-kopijos/darbuotojo-sukurimo-langas-klaidos.png)

![Darbuotojo peržiūros langas](ekrano-kopijos/darbuotojo-perziuros-langas.png)

![Darbuotojo redagavimo langas](ekrano-kopijos/darbuotojo-redagavimo-langas.png)

**Darbuotojo tyrimų sąrašo langas** ir **Tyrimo peržiūros langas**

![Darbuotojo tyrimų sąrašo langas](ekrano-kopijos/darbuotojo-tyrimu-saraso-langas.png)

![Tyrimo peržiūros langas](ekrano-kopijos/tyrimo-perziuros-langas.png)

**Darbo krūvio analizės langas** ir **Darbo krūvio suvestinės langas** – krūvis = aktyvių klonavimų trukmių suma ÷ (etatas × 180 d.) × 100 %.

![Darbo krūvio analizės langas](ekrano-kopijos/darbo-kruvio-analizes-langas.png)

![Darbo krūvio suvestinės langas](ekrano-kopijos/darbo-kruvio-suvestines-langas.png)
