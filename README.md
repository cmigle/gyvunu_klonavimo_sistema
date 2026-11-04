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

## Naršymas

- Rolę galima pasirinkti prisijungimo lange arba meniu juostoje („Rolė“).
- „Visi langai“ – visų prototipo langų sąrašas pagal posistemes.
- Langų perėjimai aprašyti `navigavimo-planas.txt`.
