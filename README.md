# gyvunu_klonavimo_sistema

Gyvūnų klonavimo sistemos GUI prototipas (HTML, CSS, JS, be serverio dalies).

## Paleidimas

Paprasčiausia – atidarykite `index.html` naršyklėje.

### VS Code

1. **File → Open Folder…** ir pasirinkite `gyvunu_klonavimo_sistema` aplanką.
2. Įdiekite plėtinį **Live Server**: **Extensions** (`Ctrl+Shift+X`) → „Live Server“ (Ritwick Dey) → **Install**.
3. Dešiniu pelės mygtuku spustelėkite `index.html` → **Open with Live Server**. Svetainė atsidarys naršyklėje adresu `http://127.0.0.1:5500/index.html`.

Norint peržiūrėti pačiame VS Code: `Ctrl+Shift+P` → **Simple Browser: Show** → `http://127.0.0.1:5500/index.html`.

Be plėtinio (reikia Node.js): **Terminal → New Terminal**, paleiskite `npx http-server -p 8080` ir atidarykite `http://localhost:8080/`.

## Demonstraciniai duomenys

Serverio dalies dar nėra – `js/duomenys.js` sukuria demonstracinius duomenis (klientai, augintiniai, dokumentai, užsakymai, DNR pavyzdžiai, inkubatoriai, darbuotojai, klonavimai) ir laiko juos naršyklės `localStorage`. Sukurti, pakeisti ar ištrinti įrašai išlieka pereinant tarp langų; pradinę būseną atkuria nuoroda „Atstatyti duomenis“ viršutinėje prototipo juostoje.

Demonstracinės paskyros (slaptažodis visoms – `demo1234`):

| Paskyra | El. paštas | Ką pademonstruoti |
|---|---|---|
| Jonas Petrauskas, klientas | jonas@pavyzdys.lt | sukurti užsakymą Žaibui |
| Rūta Kazlauskienė, klientas | ruta@pavyzdys.lt | istorija nepatikrinta – užsakymas reikalauja patikrinimo („Petras Petraitis“ – rasta įrašų) |
| Laura Vaitkutė, lab. specialistė | laura@klonavimas.lt | DNR-0012 tinkamumas, DNR-0013 rezultatų įvedimas (Redaguoti), inkubatoriaus priskyrimas (Kiki – pavyks, Pūkis – tinkamo nėra) |
| Asta Kairienė, administratorė | asta@klonavimas.lt | darbuotojai, tyrimai, krūvio analizė (didelis / normalus / mažas) |

## Naršymas

- Prisijungti galima prisijungimo lange arba viršutinės prototipo juostos laukelyje „Prisijungti kaip“.
- „Visi langai“ – visų prototipo langų sąrašas pagal posistemes; pavadinimai sutampa su analizės diagramų «boundary» klasėmis.
- Langų perėjimai aprašyti `navigavimo-planas.txt`.

## Failai

- `js/duomenys.js` – esybės, demonstraciniai duomenys ir automatinės funkcijos (DNR tinkamumas, inkubatoriaus priskyrimas, krūvio analizė, CourtListener imitacija).
- `js/navigacija.js` – meniu, naršymo kelias, prieiga pagal rolę, formų tikrinimas, patvirtinimo langas.
- `js/langai.js` – kiekvieno lango turinys ir veiksmai pagal sekų diagramas.
