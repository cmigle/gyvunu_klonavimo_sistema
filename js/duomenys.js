// Gyvūnų klonavimo sistema – demonstraciniai duomenys ir sistemos logika
// Serverio dalies dar nėra: duomenys laikomi naršyklės localStorage, todėl
// sukurti, pakeisti ar ištrinti įrašai išlieka pereinant tarp langų.
// Esybės ir jų atributai atitinka dalykinės srities esybių klasių diagramą (2.3 pav.).

const DUOMENU_VERSIJA = 3;
const DEMO_SLAPTAZODIS = "demo1234";

const GYVUNO_TIPAI = ["Šuo", "Katė", "Triušis", "Žirgas", "Avis", "Kiaulė", "Papūga"];

// Klonavimo kaina pagal gyvūno tipą, €
const KAINOS = { "Šuo": 50000, "Katė": 40000, "Triušis": 15000, "Žirgas": 85000, "Avis": 20000, "Kiaulė": 25000, "Papūga": 12000 };

// Klonavimo trukmė pagal gyvūno tipą (vaikingumo ar inkubacijos laikotarpis), d.
const KLONAVIMO_TRUKME = { "Šuo": 63, "Katė": 65, "Triušis": 31, "Žirgas": 340, "Avis": 147, "Kiaulė": 114, "Papūga": 28 };

// Embriono vystymosi poreikiai – pagal juos parenkamas inkubatorius
const INKUBAVIMO_SALYGOS = {
    "Šuo":     { temperatura: 38.5, co2: 5.0, o2: 5.0, ph: [7.2, 7.4] },
    "Katė":    { temperatura: 38.5, co2: 5.0, o2: 5.0, ph: [7.2, 7.4] },
    "Triušis": { temperatura: 38.0, co2: 5.0, o2: 5.0, ph: [7.3, 7.5] },
    "Žirgas":  { temperatura: 38.2, co2: 6.0, o2: 5.0, ph: [7.2, 7.4] },
    "Avis":    { temperatura: 38.8, co2: 6.0, o2: 5.0, ph: [7.2, 7.4] },
    "Kiaulė":  { temperatura: 38.8, co2: 5.0, o2: 5.0, ph: [7.2, 7.4] },
    "Papūga":  { temperatura: 37.5, co2: 0.5, o2: 20.0, ph: [7.6, 7.8] }
};
const LEIDZIAMI_NUOKRYPIAI = { temperatura: 0.3, co2: 0.5, o2: 1.0 };

// DNR pavyzdžio biologinių parametrų normos tinkamumui nustatyti
const DNR_NORMOS = [
    { laukas: "saugojimo_temperatura", pavadinimas: "Saugojimo temperatūra", vienetai: "°C", maks: -20 },
    { laukas: "dnr_koncentracija", pavadinimas: "DNR koncentracija", vienetai: "ng/µl", min: 50 },
    { laukas: "tirpalo_turis", pavadinimas: "Tirpalo tūris", vienetai: "ml", min: 0.02 },
    { laukas: "baltymu_tarsa", pavadinimas: "Baltymų tarša (A260/280)", vienetai: "", min: 1.8, maks: 2.0 },
    { laukas: "din_verte", pavadinimas: "DIN vertė", vienetai: "", min: 7 },
    { laukas: "lasteliu_gyvybingumas", pavadinimas: "Ląstelių gyvybingumas", vienetai: "%", min: 80 }
];
// Laukai, kuriuos specialistas įveda kaip analizės rezultatus
const DNR_REZULTATAI = DNR_NORMOS.slice(1).map(n => n.laukas);
const TINKAMAS = "Tinkamas klonuoti";
const NETINKAMAS = "Netinkamas";

// Darbo krūvis (%) = aktyvių klonavimų trukmių suma ÷ (etatas × KRUVIO_NORMA) × 100
const KRUVIO_NORMA = 180;

// Loginių (boolean) esybių atributų reikšmės: [false, true]
const BUSENOS = {
    uzsakymas: ["Nepatvirtintas", "Patvirtintas"],
    klonavimas: ["Baigtas", "Aktyvus"],
    inkubatorius: ["Laisvas", "Užimtas"],
    specialistas: ["Neaktyvus", "Aktyvus"],
    pranesimas: ["Neišsiųstas", "Išsiųstas"]
};
const busena = (esybe, reiksme) => BUSENOS[esybe][reiksme ? 1 : 0];

// Demonstracinė paskyra, į kurią perjungia rolės pasirinkimas
const DEMO_PASKYROS = { klientas: 1, specialistas: 1, administratorius: 5 };

const COURTLISTENER_API = "https://www.courtlistener.com/api/rest/v4/search/";
// Išgalvoti teistumo įrašai, kuriuos „grąžina“ CourtListener API imitacija
const COURTLISTENER_IRASAI = [
    { asmuo: "Petras Petraitis", numeris: "1-241/2019", byla: "Sukčiavimas", teismas: "Demonstracinis apygardos teismas", data: "2019-04-12" },
    { asmuo: "Petras Petraitis", numeris: "1-1187/2021", byla: "Žiaurus elgesys su gyvūnais", teismas: "Demonstracinis apylinkės teismas", data: "2021-09-30" },
    { asmuo: "Antanas Kazlauskas", numeris: "1-86/2017", byla: "Kontrabanda", teismas: "Demonstracinis apygardos teismas", data: "2017-02-08" }
];

// ---------- Datos ir formatavimas ----------

const dvizenklis = n => String(n).padStart(2, "0");
const isoData = d => `${d.getFullYear()}-${dvizenklis(d.getMonth() + 1)}-${dvizenklis(d.getDate())}`;
const siandien = () => isoData(new Date());
function dabar() {
    const d = new Date();
    return `${isoData(d)} ${dvizenklis(d.getHours())}:${dvizenklis(d.getMinutes())}`;
}
function pridetiDienu(data, dienos) {
    const d = new Date(data + "T00:00:00");
    d.setDate(d.getDate() + dienos);
    return isoData(d);
}
const dienuSkirtumas = (nuo, iki) => Math.round((new Date(iki + "T00:00:00") - new Date(nuo + "T00:00:00")) / 86400000);

function skaicius(x, skaitmenys) {
    if (x == null || x === "") return "–";
    const f = skaitmenys == null ? { maximumFractionDigits: 2 } : { minimumFractionDigits: skaitmenys, maximumFractionDigits: skaitmenys };
    return new Intl.NumberFormat("lt-LT", f).format(x);
}
const pinigai = x => new Intl.NumberFormat("lt-LT", { style: "currency", currency: "EUR", minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(x);
const suVienetais = (x, vienetai, skaitmenys) => x == null ? "–" : `${skaicius(x, skaitmenys)}${vienetai ? " " + vienetai : ""}`;

const uzsakymoNr = u => `UZ-${String(u.id).padStart(4, "0")}`;
const dnrKodas = d => `DNR-${String(d.id).padStart(4, "0")}`;
const klonavimoNr = k => `KL-${String(k.id).padStart(4, "0")}`;
const vardasPavarde = x => x ? `${x.vardas} ${x.pavarde}` : "–";

function normosTekstas(n) {
    const v = n.vienetai ? " " + n.vienetai : "";
    if (n.min != null && n.maks != null) return `${skaicius(n.min, 1)}–${skaicius(n.maks, 1)}${v}`;
    return n.min != null ? `≥ ${skaicius(n.min)}${v}` : `≤ ${skaicius(n.maks)}${v}`;
}

// ---------- Pradiniai demonstraciniai duomenys ----------

// Lentelė iš eilučių masyvų: lentele(["id", "vardas"], [[1, "Jonas"]]) -> [{ id: 1, vardas: "Jonas" }]
const lentele = (laukai, eilutes) => eilutes.map(e => Object.fromEntries(laukai.map((l, i) => [l, e[i]])));

function pradiniaiDuomenys() {
    const d = { versija: DUOMENU_VERSIJA };
    const patikrintas = { istorijos_busena: "Patikrintas", rasta_irasu: 0 };

    d.klientai = lentele(
        ["id", "vardas", "pavarde", "el_pastas", "telefono_numeris", "asmens_kodas", "patikrinimo_data"],
        [
            [1, "Jonas", "Petrauskas", "jonas@pavyzdys.lt", "+37061234567", "38901151234", "2026-08-18 10:42"],
            [2, "Rūta", "Kazlauskienė", "ruta@pavyzdys.lt", "+37062345678", "48705152345", null],
            [3, "Tomas", "Žukauskas", "tomas@pavyzdys.lt", "+37063456789", "39203034567", "2026-05-28 14:05"],
            [4, "Ieva", "Mikalauskaitė", "ieva@pavyzdys.lt", "+37064567890", "49511214321", "2026-09-02 09:17"],
            [5, "Andrius", "Paulauskas", "andrius@pavyzdys.lt", "+37065678901", "38407306543", "2026-09-30 16:31"]
        ]).map(k => Object.assign(k, { slaptazodis: DEMO_SLAPTAZODIS },
            k.patikrinimo_data ? { ...patikrintas, istorijos_asmuo: vardasPavarde(k) }
                : { istorijos_busena: "Nepatikrintas", rasta_irasu: null }));

    d.augintiniai = lentele(
        ["id", "klientas", "vardas", "veisle", "amzius", "tipas", "lytis", "svoris", "mikroschemos_numeris"],
        [
            [1, 1, "Reksas", "Vokiečių aviganis", 9, "Šuo", "Patinas", 34.5, "440110000123456"],
            [2, 1, "Murka", "Meinkūnas", 12, "Katė", "Patelė", 6.8, "440110000234567"],
            [3, 1, "Žaibas", "Lietuvos sunkusis", 15, "Žirgas", "Patinas", 640, "440110000345678"],
            [4, 2, "Pifas", "Britų trumpaplaukė", 10, "Katė", "Patinas", 5.6, "440110000456789"],
            [5, 3, "Bella", "Labradoro retriveris", 11, "Šuo", "Patelė", 29, "440110000567890"],
            [6, 3, "Baltukė", "Romanovų", 7, "Avis", "Patelė", 55, "440110000678901"],
            [7, 3, "Pūkis", "Olandų nykštukinis", 6, "Triušis", "Patinas", 1.2, "440110000789012"],
            [8, 4, "Rokis", "Biglis", 8, "Šuo", "Patinas", 14.2, "440110000890123"],
            [9, 4, "Snieguolė", "Sibiro", 13, "Katė", "Patelė", 4.9, "440110000901234"],
            [10, 4, "Kaštonas", "Trakėnų", 18, "Žirgas", "Patinas", 580, "440110001012345"],
            [11, 4, "Kiki", "Geltonkuodis kakadu", 28, "Papūga", "Patelė", 0.9, "440110001123456"],
            [12, 5, "Bekonas", "Vietnamo kiaulė", 5, "Kiaulė", "Patinas", 68, "440110001234567"],
            [13, 5, "Ūsas", "Bengalų", 9, "Katė", "Patinas", 5.3, "440110001345678"],
            [14, 5, "Rudis", "Taksas", 4, "Šuo", "Patinas", 9.1, "440110001456789"]
        ]);

    // Žaibas (3) ir Kiki (11) dokumentų neturi – rodomas pranešimas „Dokumentų nėra“
    d.dokumentai = lentele(
        ["id", "augintinis", "pavadinimas", "dokumento_tipas", "ikelimo_data", "failo_formatas", "galiojimo_data", "failo_pavadinimas"],
        [
            [1, 1, "Rekso kilmės sertifikatas", "Kilmės sertifikatas", "2026-08-20", "PDF", "2031-08-20", "reksas-kilmes-sertifikatas.pdf"],
            [2, 1, "Klonavimo sutartis UZ-0001", "Sutartis", "2026-08-22", "PDF", "2027-08-22", "sutartis-UZ-0001.pdf"],
            [3, 1, "Rekso veterinarinis pasas", "Veterinarinis pasas", "2026-08-20", "JPG", "2027-01-10", "reksas-pasas.jpg"],
            [4, 2, "Murkos kilmės sertifikatas", "Kilmės sertifikatas", "2026-10-10", "PNG", "2031-10-10", "murka-sertifikatas.png"],
            [5, 4, "Pifo veterinarinis pasas", "Veterinarinis pasas", "2025-03-01", "PDF", "2026-03-01", "pifas-pasas.pdf"],
            [6, 5, "Bellos kilmės sertifikatas", "Kilmės sertifikatas", "2026-05-29", "PDF", "2031-05-29", "bella-sertifikatas.pdf"],
            [7, 5, "Klonavimo sutartis UZ-0003", "Sutartis", "2026-05-30", "PDF", "2027-05-30", "sutartis-UZ-0003.pdf"],
            [8, 6, "Baltukės veterinarinis pasas", "Veterinarinis pasas", "2026-09-30", "JPG", "2028-09-30", "baltuke-pasas.jpg"],
            [9, 7, "Pūkio kilmės sertifikatas", "Kilmės sertifikatas", "2026-10-28", "PDF", "2031-10-28", "pukis-sertifikatas.pdf"],
            [10, 8, "Rokio kilmės sertifikatas", "Kilmės sertifikatas", "2026-09-05", "PDF", "2031-09-05", "rokis-sertifikatas.pdf"],
            [11, 9, "Snieguolės kilmės sertifikatas", "Kilmės sertifikatas", "2026-09-19", "PNG", "2031-09-19", "snieguole-sertifikatas.png"],
            [12, 10, "Kaštono kilmės sertifikatas", "Kilmės sertifikatas", "2026-02-12", "PDF", "2031-02-12", "kastonas-sertifikatas.pdf"],
            [13, 12, "Bekono veterinarinis pasas", "Veterinarinis pasas", "2026-09-25", "PDF", "2028-09-25", "bekonas-pasas.pdf"],
            [14, 13, "Ūso kilmės sertifikatas", "Kilmės sertifikatas", "2026-10-04", "JPG", "2031-10-04", "usas-sertifikatas.jpg"]
        ]);

    // Augintinis turi ne daugiau kaip vieną užsakymą (esybių diagramoje 0..1)
    d.uzsakymai = lentele(
        ["id", "augintinis", "kaina", "sukurimo_data", "busena", "patvirtinimo_data"],
        [
            [1, 1, 50000, "2026-08-22", true, "2026-09-05"],
            [2, 2, 40000, "2026-10-12", false, null],
            [3, 5, 50000, "2026-05-30", true, "2026-06-10"],
            [4, 6, 20000, "2026-10-02", false, null],
            [5, 8, 50000, "2026-09-08", true, "2026-09-18"],
            [6, 9, 40000, "2026-09-21", true, "2026-10-01"],
            [7, 10, 85000, "2026-02-16", true, "2026-03-02"],
            [8, 12, 25000, "2026-09-29", true, "2026-10-10"],
            [9, 13, 40000, "2026-10-06", true, "2026-10-15"],
            [10, 7, 15000, "2026-10-30", true, "2026-11-04"],
            [11, 11, 12000, "2026-11-02", true, "2026-11-05"],
            [12, 14, 50000, "2026-11-06", false, null]
        ]);

    d.dnr = lentele(
        ["id", "augintinis", "paemimo_data", "saugojimo_temperatura", "dnr_koncentracija", "tirpalo_turis", "baltymu_tarsa", "din_verte", "lasteliu_gyvybingumas", "tinkamumas", "tinkamumo_data"],
        [
            [1, 1, "2026-08-25", -80, 152.4, 0.2, 1.86, 8.6, 94.5, TINKAMAS, "2026-09-01"],
            [2, 2, "2026-10-14", -80, 98.1, 0.15, 1.91, 8.1, 91, TINKAMAS, "2026-10-20"],
            [3, 5, "2026-06-02", -80, 134, 0.18, 1.88, 8.9, 95.2, TINKAMAS, "2026-06-06"],
            [4, 6, "2026-10-05", -80, 31.5, 0.12, 1.62, 5.9, 72.4, NETINKAMAS, "2026-10-09"],
            [5, 7, "2026-11-01", -80, 87.6, 0.1, 1.84, 7.6, 88.3, TINKAMAS, "2026-11-03"],
            [6, 11, "2026-11-02", -80, 64.2, 0.08, 1.83, 7.4, 86.1, TINKAMAS, "2026-11-04"],
            [7, 8, "2026-09-10", -80, 120.7, 0.2, 1.9, 8.4, 93.1, TINKAMAS, "2026-09-15"],
            [8, 9, "2026-09-23", -80, 101.3, 0.16, 1.87, 8, 90.6, TINKAMAS, "2026-09-28"],
            [9, 10, "2026-02-18", -80, 76.2, 0.25, 1.82, 7.3, 84, TINKAMAS, "2026-02-25"],
            [10, 12, "2026-10-01", -80, 143.9, 0.22, 1.93, 8.8, 92.7, TINKAMAS, "2026-10-07"],
            [11, 13, "2026-10-08", -80, 110.5, 0.17, 1.89, 8.2, 91.8, TINKAMAS, "2026-10-13"],
            // Rezultatai įvesti, tinkamumas dar nenustatytas
            [12, 4, "2026-11-04", -80, 71.8, 0.12, 1.85, 7.2, 82.5, null, null],
            // Rezultatai dar neįvesti
            [13, 14, "2026-11-06", -80, null, null, null, null, null, null, null]
        ]);

    d.inkubatoriai = lentele(
        ["id", "modelis", "serijinis_numeris", "tiksline_temperatura", "talpa", "co2_procentas", "busena", "ph_lygis", "o2_procentas"],
        [
            [1, "Thermo Fisher Heracell VIOS 160i", "HV160-22-0147", 38.5, 165, 5, true, 7.3, 5],
            [2, "Eppendorf CellXpert C170i", "CX170-21-0458", 38.6, 170, 5, false, 7.35, 5],
            [3, "Binder CB 170", "CB170-20-1123", 38.8, 170, 5, true, 7.3, 5],
            [4, "PHCbi MCO-170AICUV", "MC170-23-3390", 37, 165, 5, false, 7.4, 20],
            [5, "Esco CelCulture CCL-170B", "CC170-22-0821", 38.5, 170, 5, false, 7.3, 5],
            [6, "Memmert ICO150med", "IC150-19-0042", 37.5, 150, 0.5, false, 7.7, 20],
            [7, "Thermo Fisher Forma Steri-Cycle i160", "FS160-21-7781", 38.5, 160, 5, true, 7.3, 5]
        ]);

    // Paskutinių šešių dienų matavimai kiekvienam inkubatoriui
    const temperaturosNuokrypiai = [0.1, -0.1, 0, 0.2, -0.2, 0.1];
    const dregmes = [93.4, 94.1, 92.8, 95, 93.6, 94.3];
    d.matavimai = d.inkubatoriai.flatMap(ink => temperaturosNuokrypiai.map((n, i) => ({
        inkubatorius: ink.id,
        matavimo_data: `2026-11-0${2 + i} 08:00`,
        temperatura: Math.round((ink.tiksline_temperatura + n * (ink.id % 2 ? 1 : -1)) * 10) / 10,
        dregme: dregmes[(i + ink.id) % dregmes.length]
    }))).map((m, i) => ({ id: i + 1, ...m }));

    d.darbuotojai = lentele(
        ["id", "vardas", "pavarde", "telefono_numeris", "el_pastas", "etatas", "role", "licencijos_numeris", "busena"],
        [
            [1, "Laura", "Vaitkutė", "+37061111222", "laura@klonavimas.lt", 1, "specialistas", "LS-2019-0142", true],
            [2, "Mantas", "Jankauskas", "+37061111333", "mantas@klonavimas.lt", 0.75, "specialistas", "LS-2020-0217", true],
            [3, "Greta", "Stankevičiūtė", "+37061111444", "greta@klonavimas.lt", 1, "specialistas", "LS-2023-0388", true],
            [4, "Darius", "Mockus", "+37061111555", "darius@klonavimas.lt", 0.5, "specialistas", "LS-2017-0091", false],
            [5, "Asta", "Kairienė", "+37061111666", "asta@klonavimas.lt", 1, "administratorius", null, null]
        ]).map(x => Object.assign(x, { slaptazodis: DEMO_SLAPTAZODIS }));

    d.klonavimai = lentele(
        ["id", "uzsakymas", "dnr", "specialistas", "inkubatorius", "pradzios_data", "pabaigos_data", "rezultatas", "specialisto_patvirtinimas", "busena"],
        [
            [1, 1, 1, 1, 1, "2026-09-08", "2026-11-10", "Embrionai vystosi normaliai", true, true],
            [2, 3, 3, 2, null, "2026-06-15", "2026-08-17", "Sėkmingas, gimė sveikas šuniukas", true, false],
            [3, 5, 7, 1, null, "2026-09-22", "2026-11-24", "Embrionas perkeltas surogatinei motinai", true, true],
            [4, 6, 8, 1, 7, "2026-10-03", "2026-12-07", "Vyksta embriono inkubacija", false, true],
            [5, 7, 9, 2, null, "2026-03-10", "2026-04-02", "Nesėkmingas, embrionas nustojo vystytis", true, false],
            [6, 8, 10, 2, 3, "2026-10-14", "2027-02-05", "Vyksta embriono inkubacija", false, true],
            [7, 9, 11, 3, null, "2026-10-20", "2026-12-24", "Embrionas perkeltas surogatinei motinai", true, true]
        ]);

    // Patvirtintiems užsakymams – išsiųsti pranešimai apie patvirtinimą
    d.pranesimai = d.uzsakymai.filter(u => u.busena).map((u, i) => {
        const augintinis = d.augintiniai.find(a => a.id === u.augintinis);
        const klientas = d.klientai.find(k => k.id === augintinis.klientas);
        const pavyzdys = d.dnr.find(x => x.augintinis === augintinis.id);
        return { id: i + 1, uzsakymas: u.id, tekstas: laiskoSablonas(u, augintinis, klientas, pavyzdys).tekstas, siuntimo_data: u.patvirtinimo_data + " 09:15", busena: true };
    });
    return d;
}

// ---------- Saugykla ----------

const SAUGYKLA = "gks-duomenys";

function ikeltiDuomenis() {
    try {
        const d = JSON.parse(localStorage.getItem(SAUGYKLA));
        if (d && d.versija === DUOMENU_VERSIJA) return d;
    } catch (e) { /* nėra localStorage arba duomenys sugadinti – naudojami pradiniai */ }
    return pradiniaiDuomenys();
}

function issaugotiDuomenis() {
    try { localStorage.setItem(SAUGYKLA, JSON.stringify(DB)); } catch (e) { /* prototipas veikia ir be to */ }
}

function atstatytiDuomenis() {
    DB = pradiniaiDuomenys();
    issaugotiDuomenis();
}

const visi = kolekcija => DB[kolekcija];
const rasti = (kolekcija, id) => DB[kolekcija].find(x => x.id === Number(id)) || null;

function sukurti(kolekcija, irasas) {
    const naujas = Object.assign({}, irasas, { id: DB[kolekcija].reduce((m, x) => Math.max(m, x.id), 0) + 1 });
    DB[kolekcija].push(naujas);
    issaugotiDuomenis();
    return naujas;
}

function atnaujinti(kolekcija, id, laukai) {
    const irasas = Object.assign(rasti(kolekcija, id), laukai);
    issaugotiDuomenis();
    return irasas;
}

function pasalinti(kolekcija, id) {
    DB[kolekcija] = DB[kolekcija].filter(x => x.id !== Number(id));
    issaugotiDuomenis();
}

// ---------- Ryšiai tarp esybių ----------

const klientoAugintiniai = klientoId => DB.augintiniai.filter(a => a.klientas === klientoId);
const augintinioDokumentai = id => DB.dokumentai.filter(d => d.augintinis === id);
const augintinioUzsakymas = id => DB.uzsakymai.find(u => u.augintinis === id) || null;
const augintinioDnr = id => DB.dnr.find(d => d.augintinis === id) || null;
const uzsakymoKlientas = u => rasti("klientai", rasti("augintiniai", u.augintinis)?.klientas);
const klientoUzsakymai = klientoId => DB.uzsakymai.filter(u => rasti("augintiniai", u.augintinis)?.klientas === klientoId);
const uzsakymoKlonavimas = id => DB.klonavimai.find(k => k.uzsakymas === id) || null;
const uzsakymoPranesimas = id => DB.pranesimai.find(p => p.uzsakymas === id) || null;
const dnrKlonavimas = id => DB.klonavimai.find(k => k.dnr === id) || null;
const inkubatoriausKlonavimas = id => DB.klonavimai.find(k => k.inkubatorius === id && k.busena) || null;
const inkubatoriausMatavimai = id => DB.matavimai.filter(m => m.inkubatorius === id)
    .sort((a, b) => b.matavimo_data.localeCompare(a.matavimo_data));
const specialistai = () => DB.darbuotojai.filter(d => d.role === "specialistas");
const specialistoKlonavimai = id => DB.klonavimai.filter(k => k.specialistas === id);
const aktyvusKlonavimai = id => specialistoKlonavimai(id).filter(k => k.busena);
const klonavimoAugintinis = k => rasti("augintiniai", rasti("uzsakymai", k.uzsakymas)?.augintinis);
const kainaPagalTipa = tipas => KAINOS[tipas] || 0;

function elPastasUzimtas(elPastas, isskyrus) {
    const el = elPastas.trim().toLowerCase();
    return [...DB.klientai, ...DB.darbuotojai].some(x => x.el_pastas.toLowerCase() === el && x !== isskyrus);
}

// ---------- Prisijungimas ----------

// Prisijungimo valdiklis tikrina duomenis pagal esybes „Klientas“ ir „Darbuotojas“ (3.1.7 pav.)
function rastiPaskyra(elPastas, slaptazodis) {
    const el = elPastas.trim().toLowerCase();
    const klientas = DB.klientai.find(k => k.el_pastas.toLowerCase() === el);
    if (klientas) return klientas.slaptazodis === slaptazodis ? { role: "klientas", id: klientas.id } : null;
    const darbuotojas = DB.darbuotojai.find(x => x.el_pastas.toLowerCase() === el);
    return darbuotojas && darbuotojas.slaptazodis === slaptazodis ? { role: darbuotojas.role, id: darbuotojas.id } : null;
}

// ---------- Kliento istorijos patikrinimas (2.9 pav.) ----------

const normalizuoti = s => s.normalize("NFD").replace(/[̀-ͯ]/g, "").trim().toLowerCase().replace(/\s+/g, " ");

// CourtListener API sąsajos imitacija
function courtListenerPaieska(vardas, pavarde) {
    const asmuo = `${vardas} ${pavarde}`.trim();
    return {
        uzklausa: `GET ${COURTLISTENER_API}?type=r&q=${encodeURIComponent(`"${asmuo}"`)}`,
        rezultatai: COURTLISTENER_IRASAI.filter(i => normalizuoti(i.asmuo) === normalizuoti(asmuo))
    };
}

function patikrintiIstorija(klientoId, vardas, pavarde) {
    const { uzklausa, rezultatai } = courtListenerPaieska(vardas, pavarde);
    return atnaujinti("klientai", klientoId, {
        istorijos_busena: rezultatai.length ? "Netinkamas" : "Patikrintas",
        patikrinimo_data: dabar(),
        rasta_irasu: rezultatai.length,
        istorijos_asmuo: `${vardas} ${pavarde}`,
        istorijos_uzklausa: uzklausa,
        istorijos_irasai: rezultatai
    });
}

// ---------- Pranešimas apie užsakymo patvirtinimą ----------

// „Laiško šablonas“ (3.1.17 pav.)
function laiskoSablonas(uzsakymas, augintinis, klientas, pavyzdys) {
    return {
        gavejas: klientas.el_pastas,
        tema: `Užsakymas ${uzsakymoNr(uzsakymas)} patvirtintas`,
        tekstas: [
            `Gerb. ${vardasPavarde(klientas)},`,
            "",
            `Jūsų klonavimo užsakymas ${uzsakymoNr(uzsakymas)} augintiniui „${augintinis.vardas}“ (${augintinis.tipas.toLowerCase()}) patvirtintas ${uzsakymas.patvirtinimo_data}.`,
            `DNR pavyzdys ${pavyzdys ? dnrKodas(pavyzdys) + " " : ""}įvertintas kaip tinkamas klonuoti.`,
            `Užsakymo kaina: ${pinigai(uzsakymas.kaina)}.`,
            "",
            "Apie tolesnius klonavimo etapus informuosime atskiru laišku.",
            "",
            "Pagarbiai",
            "Gyvūnų klonavimo sistema"
        ].join("\n")
    };
}

// ---------- DNR pavyzdžio tinkamumo nustatymas (2.20 pav.) ----------

function tinkamumoAnalize(pavyzdys) {
    const truksta = DNR_NORMOS.filter(n => pavyzdys[n.laukas] == null).map(n => n.pavadinimas);
    if (truksta.length) return { truksta };
    // Kiekvienas biologinis parametras tikrinamas pagal nustatytą normą
    const patikrinimai = DNR_NORMOS.map(n => {
        const verte = pavyzdys[n.laukas];
        return { ...n, verte, atitinka: (n.min == null || verte >= n.min) && (n.maks == null || verte <= n.maks) };
    });
    const neatitikimai = patikrinimai.filter(p => !p.atitinka);
    return { patikrinimai, neatitikimai, statusas: neatitikimai.length ? NETINKAMAS : TINKAMAS };
}

function nustatytiTinkamuma(id) {
    const analize = tinkamumoAnalize(rasti("dnr", id));
    if (!analize.truksta) atnaujinti("dnr", id, { tinkamumas: analize.statusas, tinkamumo_data: siandien() });
    return analize;
}

// ---------- Automatinis inkubatoriaus priskyrimas (PA „Priskirti inkubatorių“ sekų diagrama) ----------

// Tinkami klonuoti pavyzdžiai patvirtintiems užsakymams, kuriems dar nepradėtas klonavimas
const laukiaInkubatoriaus = () => DB.dnr.filter(x =>
    x.tinkamumas === TINKAMAS && augintinioUzsakymas(x.augintinis)?.busena && !dnrKlonavimas(x.id));

function inkubatoriausPatikrinimas(ink, salygos) {
    const nuokrypiai = {
        temperatura: Math.abs(ink.tiksline_temperatura - salygos.temperatura),
        co2: Math.abs(ink.co2_procentas - salygos.co2),
        o2: Math.abs(ink.o2_procentas - salygos.o2)
    };
    const priezastys = [];
    if (ink.busena) priezastys.push("užimtas");
    if (nuokrypiai.temperatura > LEIDZIAMI_NUOKRYPIAI.temperatura + 1e-9) priezastys.push("temperatūra");
    if (nuokrypiai.co2 > LEIDZIAMI_NUOKRYPIAI.co2 + 1e-9) priezastys.push("CO₂");
    if (nuokrypiai.o2 > LEIDZIAMI_NUOKRYPIAI.o2 + 1e-9) priezastys.push("O₂");
    if (ink.ph_lygis < salygos.ph[0] || ink.ph_lygis > salygos.ph[1]) priezastys.push("pH");
    // Tinkamiausias – kurio parametrai arčiausiai embriono poreikių
    const phVidurys = (salygos.ph[0] + salygos.ph[1]) / 2;
    const ivertis = nuokrypiai.temperatura / LEIDZIAMI_NUOKRYPIAI.temperatura + nuokrypiai.co2 / LEIDZIAMI_NUOKRYPIAI.co2
        + nuokrypiai.o2 / LEIDZIAMI_NUOKRYPIAI.o2 + Math.abs(ink.ph_lygis - phVidurys) / ((salygos.ph[1] - salygos.ph[0]) / 2);
    return {
        id: ink.id, modelis: ink.modelis, serijinis_numeris: ink.serijinis_numeris, busena: ink.busena,
        tiksline_temperatura: ink.tiksline_temperatura, co2_procentas: ink.co2_procentas,
        o2_procentas: ink.o2_procentas, ph_lygis: ink.ph_lygis,
        tinka: !priezastys.length, priezastys, ivertis
    };
}

function priskirtiInkubatoriu(dnrId, specialistoId) {
    const pavyzdys = rasti("dnr", dnrId);
    const augintinis = rasti("augintiniai", pavyzdys.augintinis);
    const salygos = INKUBAVIMO_SALYGOS[augintinis.tipas];
    // Ciklas per visus inkubatorius: tinkami įtraukiami į sąrašą
    const patikrinimai = DB.inkubatoriai.map(i => inkubatoriausPatikrinimas(i, salygos));
    const tinkami = patikrinimai.filter(p => p.tinka).sort((a, b) => a.ivertis - b.ivertis);
    const rezultatas = { dnr: dnrId, tipas: augintinis.tipas, salygos, patikrinimai, data: dabar(), parinktas: null };
    if (!tinkami.length) return rezultatas;

    const parinktas = tinkami[0];
    const klonavimas = sukurti("klonavimai", {
        uzsakymas: augintinioUzsakymas(augintinis.id).id, dnr: dnrId, specialistas: specialistoId, inkubatorius: parinktas.id,
        pradzios_data: siandien(), pabaigos_data: pridetiDienu(siandien(), KLONAVIMO_TRUKME[augintinis.tipas]),
        rezultatas: "Vyksta embriono inkubacija", specialisto_patvirtinimas: false, busena: true
    });
    atnaujinti("inkubatoriai", parinktas.id, { busena: true });
    return Object.assign(rezultatas, { parinktas: parinktas.id, klonavimas: klonavimas.id });
}

// ---------- Darbuotojų krūvio analizė (2.26 pav.) ----------

const kruvioLygis = proc => proc < 70 ? "Mažas darbo krūvis" : proc <= 100 ? "Normalus darbo krūvis" : "Didelis darbo krūvis";

function specialistoKruvis(specialistas) {
    const aktyvus = aktyvusKlonavimai(specialistas.id);
    const trukme = aktyvus.reduce((suma, k) => suma + dienuSkirtumas(k.pradzios_data, k.pabaigos_data), 0);
    const kruvis = specialistas.etatas ? trukme / (specialistas.etatas * KRUVIO_NORMA) * 100 : 0;
    return { darbuotojas: specialistas, kiekis: aktyvus.length, trukme, kruvis, lygis: kruvioLygis(kruvis) };
}
const kruvioAnalize = () => specialistai().map(specialistoKruvis);

// ---------- Ištrynimas kartu su susijusiais įrašais ----------

// Ištrinant augintinį ištrinami ir jo dokumentai (3.1.11 pav.)
function istrintiAugintini(id) {
    const uzsakymas = augintinioUzsakymas(id);
    if (uzsakymas?.busena) return `Augintinio ištrinti negalima, nes jo užsakymas ${uzsakymoNr(uzsakymas)} jau patvirtintas.`;
    DB.dokumentai = DB.dokumentai.filter(d => d.augintinis !== id);
    DB.uzsakymai = DB.uzsakymai.filter(u => u.augintinis !== id);
    DB.dnr = DB.dnr.filter(d => d.augintinis !== id);
    pasalinti("augintiniai", id);
    return null;
}

function istrintiKlienta(id) {
    const augintiniai = klientoAugintiniai(id);
    if (augintiniai.some(a => augintinioUzsakymas(a.id)?.busena))
        return "Paskyros ištrinti negalima, nes turite patvirtintų užsakymų.";
    augintiniai.forEach(a => istrintiAugintini(a.id));
    pasalinti("klientai", id);
    return null;
}

function istrintiDnr(id) {
    DB.klonavimai.filter(k => k.dnr === id).forEach(k => { k.dnr = null; });
    pasalinti("dnr", id);
}

function istrintiInkubatoriu(id) {
    DB.klonavimai.filter(k => k.inkubatorius === id).forEach(k => { k.inkubatorius = null; });
    DB.matavimai = DB.matavimai.filter(m => m.inkubatorius !== id);
    pasalinti("inkubatoriai", id);
}

function istrintiDarbuotoja(id) {
    DB.klonavimai.filter(k => k.specialistas === id).forEach(k => { k.specialistas = null; });
    pasalinti("darbuotojai", id);
}

// Įkeliama pabaigoje, kai visos pradiniams duomenims reikalingos konstantos jau apibrėžtos
let DB = ikeltiDuomenis();
