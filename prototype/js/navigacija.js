// Gyvūnų klonavimo sistema – GUI prototipo navigacija ir bendri sąsajos elementai
// Prisijungęs naudotojas laikomas naršyklėje, duomenys – js/duomenys.js.

const ROLES = {
    klientas: "Klientas",
    specialistas: "Laboratorijos specialistas",
    administratorius: "Administratorius"
};

// Meniu punktai kiekvienai rolei: [langas, tekstas]
const MENIU = {
    svecias: [
        ["index.html", "Pradžia"]
    ],
    klientas: [
        ["index.html", "Pagrindinis"],
        ["profilis.html", "Mano profilis"],
        ["augintiniai.html", "Mano augintiniai"],
        ["uzsakymai.html", "Užsakymai"]
    ],
    specialistas: [
        ["index.html", "Pagrindinis"],
        ["uzsakymai.html", "Užsakymai"],
        ["dnr.html", "DNR pavyzdžiai"],
        ["inkubatoriai.html", "Inkubatoriai"]
    ],
    administratorius: [
        ["index.html", "Pagrindinis"],
        ["darbuotojai.html", "Darbuotojai"],
        ["kruvio-analize.html", "Krūvio analizė"]
    ]
};
// Dešinėje meniu juostos pusėje
const MENIU_DESINE = {
    svecias: [["prisijungimas.html", "Prisijungti"], ["registracija.html", "Registruotis"]],
    prisijunges: [["atsijungimas.html", "Atsijungti"]]
};

// Langų hierarchija: puslapis -> [trumpas pavadinimas, tėvinis langas]
const LANGAI = {
    "index.html": ["Pradžia"],
    "prisijungimas.html": ["Prisijungimas", "index.html"],
    "registracija.html": ["Registracija", "index.html"],
    "atsijungimas.html": ["Atsijungimas", "index.html"],
    "profilis.html": ["Mano profilis", "index.html"],
    "profilis-redaguoti.html": ["Redagavimas", "profilis.html"],
    "istorijos-patikrinimas.html": ["Istorijos patikrinimas", "profilis.html"],
    "istorijos-rezultatas.html": ["Rezultatas", "istorijos-patikrinimas.html"],
    "augintiniai.html": ["Mano augintiniai", "index.html"],
    "augintinis-kurti.html": ["Naujas augintinis", "augintiniai.html"],
    "augintinis.html": ["Augintinis", "augintiniai.html"],
    "augintinis-redaguoti.html": ["Redagavimas", "augintiniai.html"],
    "dokumentai.html": ["Dokumentai", "augintiniai.html"],
    "dokumentas-ikelti.html": ["Įkėlimas", "dokumentai.html"],
    "dokumentas.html": ["Dokumentas", "dokumentai.html"],
    "dokumentas-redaguoti.html": ["Redagavimas", "dokumentai.html"],
    "uzsakymai.html": ["Užsakymai", "index.html"],
    "uzsakymas-kurti.html": ["Naujas užsakymas", "uzsakymai.html"],
    "uzsakymas.html": ["Užsakymas", "uzsakymai.html"],
    "uzsakymas-redaguoti.html": ["Redagavimas", "uzsakymai.html"],
    "dnr.html": ["DNR pavyzdžiai", "index.html"],
    "dnr-kurti.html": ["Naujas pavyzdys", "dnr.html"],
    "dnr-pavyzdys.html": ["Pavyzdys", "dnr.html"],
    "dnr-redaguoti.html": ["Redagavimas", "dnr.html"],
    "dnr-tinkamumo-rezultatas.html": ["Ataskaita", "dnr.html"],
    "inkubatoriai.html": ["Inkubatoriai", "index.html"],
    "inkubatorius-kurti.html": ["Naujas inkubatorius", "inkubatoriai.html"],
    "inkubatorius.html": ["Parametrai", "inkubatoriai.html"],
    "inkubatorius-busena.html": ["Būsenos redagavimas", "inkubatoriai.html"],
    "inkubatoriaus-priskyrimo-rezultatas.html": ["Priskyrimo rezultatas", "inkubatoriai.html"],
    "darbuotojai.html": ["Darbuotojai", "index.html"],
    "darbuotojas-kurti.html": ["Naujas darbuotojas", "darbuotojai.html"],
    "darbuotojas.html": ["Darbuotojas", "darbuotojai.html"],
    "darbuotojas-redaguoti.html": ["Redagavimas", "darbuotojai.html"],
    "darbuotojo-tyrimai.html": ["Tyrimai", "darbuotojai.html"],
    "tyrimas.html": ["Tyrimas", "darbuotojo-tyrimai.html"],
    "kruvio-analize.html": ["Krūvio analizė", "darbuotojai.html"],
    "kruvio-suvestine.html": ["Suvestinė", "kruvio-analize.html"],
    "langai.html": ["Visi langai", "index.html"]
};

// ---------- Pagalbinės funkcijos ----------

const esc = s => String(s ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
const param = vardas => new URLSearchParams(location.search).get(vardas);

// Lango adresas su parametrais: nuoroda("augintinis.html", { augintinis: 3 })
function nuoroda(langas, parametrai = {}) {
    const p = new URLSearchParams();
    Object.entries(parametrai).forEach(([k, v]) => { if (v != null && v !== "") p.set(k, v); });
    const q = p.toString();
    return q ? `${langas}?${q}` : langas;
}

// Būsenos žymos spalva pagal reikšmę
const ZYMU_TIPAI = {
    sekme: ["Patvirtintas", TINKAMAS, "Laisvas", "Aktyvus", "Patikrintas", "Išsiųstas", "Galioja", "Normalus darbo krūvis", "Tinka", "Atitinka", "Taip"],
    ispejimas: ["Nepatvirtintas", "Neįvertintas", "Nepatikrintas"],
    klaida: [NETINKAMAS, "Nebegalioja", "Didelis darbo krūvis", "Netinka", "Neatitinka", "Neišsiųstas"],
    info: ["Užimtas", "Mažas darbo krūvis", "Laboratorijos specialistas", "Vykdomas"],
    neutrali: ["Baigtas", "Neaktyvus", "Administratorius", "Ne", "Nėra"]
};
function zyma(tekstas, tipas) {
    tipas = tipas || Object.keys(ZYMU_TIPAI).find(t => ZYMU_TIPAI[t].includes(tekstas)) || "neutrali";
    return `<span class="zyma ${tipas}">${esc(tekstas)}</span>`;
}

function dabartinisPuslapis() {
    return location.pathname.split("/").pop() || "index.html";
}

// ---------- Prisijungęs naudotojas ----------

function sesija() {
    try {
        const s = JSON.parse(localStorage.getItem("prisijunges"));
        return s && ROLES[s.role] ? s : null;
    } catch (e) { return null; }
}

function prisijungti(role, id) {
    try { localStorage.setItem("prisijunges", JSON.stringify({ role, id: Number(id) })); }
    catch (e) { /* naršyklė neleidžia saugoti – prototipas veikia ir be to */ }
}

function atsijungti() {
    try { localStorage.removeItem("prisijunges"); } catch (e) { /* žr. aukščiau */ }
}

// Prisijungusio naudotojo esybės įrašas (Klientas arba Darbuotojas)
function naudotojas() {
    const s = sesija();
    if (!s) return null;
    const irasas = rasti(s.role === "klientas" ? "klientai" : "darbuotojai", s.id);
    return irasas && (s.role === "klientas" || irasas.role === s.role) ? irasas : null;
}

const gautiRole = () => naudotojas() ? sesija().role : "";

// Demonstracinė tos rolės paskyra (jei ištrinta – pirma likusi)
function demoPaskyra(role) {
    const sarasas = role === "klientas" ? DB.klientai : DB.darbuotojai.filter(d => d.role === role);
    return sarasas.find(x => x.id === DEMO_PASKYROS[role]) || sarasas[0] || null;
}

// ---------- Navigacija ----------

function tevinisLangas(puslapis, role) {
    // Specialistas dokumentus pasiekia per užsakymą, ne per augintinį
    if (puslapis === "dokumentai.html" && role === "specialistas") return "uzsakymas.html";
    return LANGAI[puslapis][1];
}

// Langų grandinė nuo pradinio lango iki nurodyto puslapio
function kelias(puslapis, role) {
    const grandine = [];
    for (let p = puslapis; LANGAI[p]; p = tevinisLangas(p, role)) grandine.unshift(p);
    return grandine;
}

// Pereiti į kitą langą su pranešimu (tipas: sekme, klaida, info)
function eiti(tikslas, pranesimas, tipas = "sekme") {
    const url = new URL(tikslas, location.href);
    if (pranesimas) url.searchParams.set({ sekme: "msg", klaida: "klaida", info: "info" }[tipas], pranesimas);
    location.href = url.href;
}

function pieštiMeniu() {
    const nav = document.getElementById("meniu");
    if (!nav) return;
    const role = gautiRole();
    const asmuo = naudotojas();
    const punktai = MENIU[role || "svecias"];
    const desine = MENIU_DESINE[role ? "prisijunges" : "svecias"];
    // Pažymimas meniu punktas, kurio skyriuje yra dabartinis langas
    const puslapis = dabartinisPuslapis();
    const visiPunktai = [...punktai, ...desine];
    const aktyvus = puslapis === "index.html" ? puslapis : kelias(puslapis, role).reverse()
        .find(p => p !== "index.html" && visiPunktai.some(([href]) => href === p));
    const punktas = ([href, tekstas]) => {
        const dabar = href === aktyvus ? ` aria-current="${href === puslapis ? "page" : "true"}"` : "";
        return `<a class="punktas" href="${href}"${dabar}>${tekstas}</a>`;
    };
    const parinktis = (r, x) => {
        const pazymeta = role === r && asmuo === x ? " selected" : "";
        return `<option value="${r}:${x.id}"${pazymeta}>${esc(vardasPavarde(x))}</option>`;
    };
    const grupe = (pavadinimas, r, sarasas) => sarasas.length
        ? `<optgroup label="${pavadinimas}">${sarasas.map(x => parinktis(r, x)).join("")}</optgroup>` : "";

    // Viršuje – tik prototipui skirta juosta, žemiau – sistemos meniu
    nav.innerHTML = `
        <div class="prototipas"><div class="vidus">
            <span>Prototipas</span>
            <label>Prisijungti kaip:
                <select>
                    <option value=""${role ? "" : " selected"}>svečias (neprisijungęs)</option>
                    ${grupe("Klientai", "klientas", DB.klientai)}
                    ${grupe("Laboratorijos specialistai", "specialistas", DB.darbuotojai.filter(d => d.role === "specialistas"))}
                    ${grupe("Administratoriai", "administratorius", DB.darbuotojai.filter(d => d.role === "administratorius"))}
                </select>
            </label>
            <a href="langai.html">Visi langai</a>
            <button type="button" class="atstatyti">Atstatyti duomenis</button>
        </div></div>
        <div class="virsus"><div class="vidus">
            <a class="logotipas" href="index.html">Gyvūnų klonavimo sistema</a>
            <div class="punktai">${punktai.map(punktas).join("")}</div>
            <div class="naudotojas">
                ${asmuo ? `<span title="${ROLES[role]}">${esc(vardasPavarde(asmuo))}</span>` : ""}
                ${desine.map(punktas).join("")}
            </div>
        </div></div>`;
    nav.querySelector(".prototipas select").addEventListener("change", e => {
        const [r, id] = e.target.value.split(":");
        if (r) prisijungti(r, id); else atsijungti();
        location.reload();
    });
    nav.querySelector(".atstatyti").addEventListener("click", async () => {
        const taip = await klausti({
            antraste: "Demonstracinių duomenų atstatymas",
            tekstas: "Visi prototipe sukurti, pakeisti ar ištrinti įrašai bus atkurti į pradinę būseną.",
            taip: "Atstatyti", pavojinga: false
        });
        if (!taip) return;
        atstatytiDuomenis();
        eiti(location.pathname.split("/").pop() || "index.html", "Demonstraciniai duomenys atstatyti.");
    });
}

// Naršymo kelias virš lango antraštės; parametrai perduodami tėviniams langams
function pieštiKelia() {
    const main = document.querySelector("main");
    const role = gautiRole();
    const grandine = kelias(dabartinisPuslapis(), role);
    if (!main || grandine.length < 2) return;
    const p = new URLSearchParams(location.search);
    ["msg", "klaida", "info"].forEach(k => p.delete(k));
    const q = p.toString() ? "?" + p.toString() : "";
    // Prisijungusiam pradinis langas vadinamas „Pagrindinis“, kaip ir meniu
    const pavadinimas = x => x === "index.html" && role ? "Pagrindinis" : LANGAI[x][0];
    const nav = document.createElement("nav");
    nav.className = "kelias";
    nav.setAttribute("aria-label", "Naršymo kelias");
    nav.innerHTML = grandine.map((x, i) => i === grandine.length - 1
        ? `<span aria-current="page">${pavadinimas(x)}</span>`
        : `<a href="${x === "index.html" ? x : x + q}">${pavadinimas(x)}</a>`).join('<span aria-hidden="true">/</span>');
    main.prepend(nav);
}

function pieštiLegenda() {
    const footer = document.querySelector("footer");
    if (!footer) return;
    footer.innerHTML = `<div class="vidus">
        <span>© 2026 Gyvūnų klonavimo sistema. Komanda „Šešta koja“</span>
        <div class="legenda" aria-label="Komandos spalvinė legenda">
            <span style="--c: var(--vytaute)">Vytautė Pratkevičiūtė</span>
            <span style="--c: var(--evelina)">Evelina Riukaitė</span>
            <span style="--c: var(--pijus)">Pijus Buknys</span>
            <span style="--c: var(--ugne)">Ugnė Čiauškaitė</span>
            <span style="--c: var(--migle)">Miglė Cirtautaitė</span>
        </div>
    </div>`;
}

// Svečiui – „Pradžios langas“, prisijungusiam – „Pagrindinis langas“
function pavadintiPradiniLanga() {
    const pavadinimas = gautiRole() ? "Pagrindinis langas" : "Pradžios langas";
    document.querySelectorAll("[data-pradinis]").forEach(el => { el.textContent = pavadinimas; });
    if (dabartinisPuslapis() === "index.html") document.title = `${pavadinimas} – Gyvūnų klonavimo sistema`;
}

// Pradiniame lange rodomos tik prisijungusios rolės dalys
function rodytiRolesDalis() {
    const role = gautiRole() || "svecias";
    document.querySelectorAll("[data-rolei]").forEach(el => {
        el.hidden = !el.dataset.rolei.split(" ").includes(role);
    });
}

// Langas, skirtas kitai rolei, neatveriamas – pasiūloma prisijungti
let prieigaLeidziama = true;
function tikrintiPrieiga() {
    const main = document.querySelector("main[data-prieiga]");
    if (!main) return true;
    const leidziamos = main.dataset.prieiga.split(" ");
    const role = gautiRole();
    if (leidziamos.includes(role)) return true;
    [...main.children].forEach(el => { if (!el.matches(".antraste, .kelias, #pranesimas")) el.hidden = true; });
    const demo = demoPaskyra(leidziamos[0]);
    const kortele = document.createElement("div");
    kortele.className = "kortele prieiga";
    kortele.innerHTML = `
        <h2>Langas skirtas: ${leidziamos.map(r => ROLES[r]).join(" arba ")}</h2>
        <p>${role ? `Prisijungęs naudotojas (${ROLES[role]}) šio lango atidaryti negali.` : "Norėdami atidaryti šį langą, prisijunkite."}</p>
        <div class="mygtukai">
            ${demo ? `<button type="button" class="mygtukas">Prisijungti kaip ${esc(vardasPavarde(demo))}</button>` : ""}
            <a class="mygtukas antrinis" href="prisijungimas.html">Prisijungimo langas</a>
        </div>`;
    main.append(kortele);
    kortele.querySelector("button")?.addEventListener("click", () => {
        prisijungti(leidziamos[0], demo.id);
        location.reload();
    });
    return false;
}

// ---------- Pranešimai ----------

function pranesti(tekstas, tipas = "sekme") {
    const vieta = document.getElementById("pranesimas");
    if (!vieta) return;
    vieta.innerHTML = `<div class="pranesimas ${tipas}" role="${tipas === "klaida" ? "alert" : "status"}"></div>`;
    vieta.firstChild.textContent = tekstas;
    vieta.scrollIntoView({ block: "nearest" });
}

const klaida = tekstas => pranesti(tekstas, "klaida");

function rodytiPranesima() {
    const p = new URLSearchParams(location.search);
    const [raktas, tipas] = [["msg", "sekme"], ["klaida", "klaida"], ["info", "info"]].find(([k]) => p.get(k)) || [];
    if (!raktas) return;
    pranesti(p.get(raktas), tipas);
    // Pašalinamas iš adreso, kad atnaujinus ar grįžus atgal nesikartotų
    ["msg", "klaida", "info"].forEach(k => p.delete(k));
    const q = p.toString();
    history.replaceState(null, "", location.pathname + (q ? "?" + q : ""));
}

// ---------- Formos ----------

// Puslapio skriptas nurodo, ką daryti su teisingai užpildytos formos duomenimis
const formuVeiksmai = new Map();
const kaiPateikta = (forma, veiksmas) => formuVeiksmai.set(forma, veiksmas);

function laukoPavadinimas(laukas) {
    const zyme = laukas.closest(".laukas")?.querySelector("label");
    return (zyme ? zyme.textContent : laukas.name).trim();
}

// Neteisingai užpildyti laukai pažymimi ir išvardijami klaidos pranešime
function pazymetiLaukus(laukai, tekstas) {
    laukai.forEach(l => l.closest(".laukas")?.classList.add("netinkamas"));
    klaida(tekstas || `Neteisingai užpildyti laukai: ${[...new Set(laukai.map(laukoPavadinimas))].join(", ")}.`);
    laukai[0]?.focus();
}

function formosDuomenys(forma) {
    const d = {};
    [...forma.elements].forEach(l => {
        if (!l.name || l.disabled) return;
        if (l.type === "checkbox") d[l.name] = l.checked;
        else if (l.type === "file") d[l.name] = l.files[0] || null;
        else if (l.type === "number") d[l.name] = l.value === "" ? null : Number(l.value);
        else if (l.tagName === "SELECT" && (l.value === "true" || l.value === "false")) d[l.name] = l.value === "true";
        else d[l.name] = l.value.trim();
    });
    return d;
}

function uzpildytiForma(forma, irasas) {
    [...forma.elements].forEach(l => {
        if (!l.name || !(l.name in irasas) || l.type === "file") return;
        const v = irasas[l.name];
        if (l.type === "checkbox") l.checked = !!v;
        else l.value = v == null ? "" : String(v);
    });
}

// Peržiūros langų laukai: <dd data-f="vardas">; { html: "..." } įterpiamas kaip HTML
function uzpildyti(konteineris, reiksmes) {
    konteineris.querySelectorAll("[data-f]").forEach(el => {
        if (!(el.dataset.f in reiksmes)) return;
        const v = reiksmes[el.dataset.f];
        if (v && typeof v === "object") el.innerHTML = v.html;
        else el.textContent = v == null || v === "" ? "–" : v;
    });
}

function paruostiFormas() {
    document.querySelectorAll("form").forEach(forma => {
        forma.noValidate = true;
        forma.addEventListener("input", e => e.target.closest(".laukas")?.classList.remove("netinkamas"));
        forma.addEventListener("submit", e => {
            e.preventDefault();
            forma.querySelectorAll(".netinkamas").forEach(l => l.classList.remove("netinkamas"));
            const netinkami = [...forma.elements].filter(l => l.willValidate && !l.checkValidity());
            if (netinkami.length) {
                pazymetiLaukus(netinkami);
                return;
            }
            const veiksmas = formuVeiksmai.get(forma);
            if (veiksmas) veiksmas(formosDuomenys(forma), e.submitter);
            else if (forma.dataset.tikslas) eiti(forma.dataset.tikslas, forma.dataset.pranesimas);
        });
    });
}

// ---------- Patvirtinimo langas ----------

let dialogas;
function klausti({ antraste = "Ištrynimo patvirtinimo langas", tekstas = "", taip = "Patvirtinti", ne = "Atšaukti", pavojinga = true } = {}) {
    if (!dialogas) {
        dialogas = document.createElement("dialog");
        dialogas.innerHTML = `<h2></h2><p></p><div class="mygtukai"><button type="button" class="mygtukas taip"></button><button type="button" class="mygtukas antrinis ne"></button></div>`;
        document.body.appendChild(dialogas);
    }
    dialogas.querySelector("h2").textContent = antraste;
    dialogas.querySelector("p").textContent = tekstas;
    const mygtukasTaip = dialogas.querySelector(".taip");
    mygtukasTaip.textContent = taip;
    mygtukasTaip.classList.toggle("pavojingas", pavojinga);
    dialogas.querySelector(".ne").textContent = ne;
    return new Promise(atsakymas => {
        mygtukasTaip.onclick = () => { dialogas.close(); atsakymas(true); };
        dialogas.querySelector(".ne").onclick = () => { dialogas.close(); atsakymas(false); };
        dialogas.oncancel = () => atsakymas(false);
        dialogas.showModal();
    });
}

// Spausdinamas visas langas arba tik nurodytas elementas (pvz., etiketė)
function spausdinti(elementas) {
    if (elementas) {
        document.body.classList.add("spausdinimas");
        elementas.classList.add("spausdinama");
        window.addEventListener("afterprint", () => {
            document.body.classList.remove("spausdinimas");
            elementas.classList.remove("spausdinama");
        }, { once: true });
    }
    window.print();
}

function paruostiVeiksmus() {
    document.getElementById("atsijungti")?.addEventListener("click", () => {
        atsijungti();
        eiti("prisijungimas.html", "Sėkmingai atsijungta.");
    });
}

document.addEventListener("DOMContentLoaded", () => {
    pieštiMeniu();
    pieštiKelia();
    pieštiLegenda();
    pavadintiPradiniLanga();
    rodytiRolesDalis();
    prieigaLeidziama = tikrintiPrieiga();
    rodytiPranesima();
    paruostiFormas();
    paruostiVeiksmus();
});
