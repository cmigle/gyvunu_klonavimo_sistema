# GUI prototipas

Gyvūnų klonavimo sistemos naudotojo sąsajos prototipas (HTML, CSS, JS, be serverio dalies). Langų pavadinimai sutampa su reikalavimų analizės diagramų «boundary» klasėmis.

## Paleidimas

Prototipas paskelbtas GitHub Pages: <https://cmigle.github.io/gyvunu_klonavimo_sistema/>

Vietoje paprasčiausia – atidarykite `index.html` naršyklėje.

### VS Code

1. **File → Open Folder…** ir pasirinkite saugyklos aplanką.
2. Įdiekite plėtinį **Live Server**: **Extensions** (`Ctrl+Shift+X`) → „Live Server“ (Ritwick Dey) → **Install**.
3. Dešiniu pelės mygtuku spustelėkite `prototype/index.html` → **Open with Live Server**.

Norint peržiūrėti pačiame VS Code: `Ctrl+Shift+P` → **Simple Browser: Show** → `http://127.0.0.1:5500/prototype/index.html`.

Be plėtinio (reikia Node.js): `npx http-server prototype -p 8080` ir atidarykite `http://localhost:8080/`.

## Demonstraciniai duomenys

`js/duomenys.js` sukuria demonstracinius duomenis (klientai, augintiniai, dokumentai, užsakymai, DNR pavyzdžiai, inkubatoriai, darbuotojai, klonavimai) ir laiko juos naršyklės `localStorage`. Sukurti, pakeisti ar ištrinti įrašai išlieka pereinant tarp langų; pradinę būseną atkuria nuoroda „Atstatyti duomenis“ viršutinėje prototipo juostoje.

Demonstracinės paskyros (slaptažodis visoms – `demo1234`):

| Paskyra | El. paštas | Ką pademonstruoti |
|---|---|---|
| Jonas Petrauskas, klientas | jonas@pavyzdys.lt | sukurti užsakymą Žaibui |
| Rūta Kazlauskienė, klientas | ruta@pavyzdys.lt | istorija nepatikrinta – užsakymas reikalauja patikrinimo („Petras Petraitis“ – rasta įrašų) |
| Laura Vaitkutė, lab. specialistė | laura@klonavimas.lt | DNR-0012 tinkamumas, DNR-0013 rezultatų įvedimas (Redaguoti), inkubatoriaus priskyrimas (Kiki – pavyks, Pūkis – tinkamo nėra) |
| Asta Kairienė, administratorė | asta@klonavimas.lt | darbuotojai, tyrimai, krūvio analizė (didelis / normalus / mažas) |

## Naršymas

- Prisijungti galima prisijungimo lange arba viršutinės prototipo juostos laukelyje „Prisijungti kaip“.
- „Visi langai“ – visų prototipo langų sąrašas pagal posistemes.
- Langų perėjimai aprašyti `navigavimo-planas.txt`, schema – `navigavimo-planas-schema.png`.
- Kiekvieno lango ekrano kopijos – `ekrano-kopijos/`.

## Failai

- `js/duomenys.js` – esybės, demonstraciniai duomenys ir automatinės funkcijos (DNR tinkamumas, inkubatoriaus priskyrimas, krūvio analizė, CourtListener imitacija).
- `js/navigacija.js` – meniu, naršymo kelias, prieiga pagal rolę, formų tikrinimas, patvirtinimo langas.
- `js/langai.js` – kiekvieno lango turinys ir veiksmai pagal sekų diagramas.
