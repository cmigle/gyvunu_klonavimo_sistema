// Gyvūnų klonavimo sistema – GUI prototipo navigacija
// Prototipas be duomenų bazės: naudotojo rolė laikoma naršyklėje,
// formos tik patikrina laukus ir pereina į kitą langą.

const ROLES = {
    klientas: "Klientas",
    specialistas: "Laboratorijos specialistas",
    administratorius: "Administratorius"
};

// Meniu punktai kiekvienai rolei
const MENIU = {
    svecias: [
        ["index.html", "Pradžia"],
        ["prisijungimas.html", "Prisijungti"],
        ["registracija.html", "Registruotis"]
    ],
    klientas: [
        ["index.html", "Pagrindinis"],
        ["profilis.html", "Mano profilis"],
        ["augintiniai.html", "Mano augintiniai"],
        ["uzsakymai.html", "Užsakymai"],
        ["atsijungimas.html", "Atsijungti"]
    ],
    specialistas: [
        ["index.html", "Pagrindinis"],
        ["uzsakymai.html", "Užsakymai"],
        ["dnr.html", "DNR pavyzdžiai"],
        ["inkubatoriai.html", "Inkubatoriai"],
        ["atsijungimas.html", "Atsijungti"]
    ],
    administratorius: [
        ["index.html", "Pagrindinis"],
        ["darbuotojai.html", "Darbuotojai"],
        ["kruvio-analize.html", "Krūvio analizė"],
        ["atsijungimas.html", "Atsijungti"]
    ]
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
    "augintinis-redaguoti.html": ["Redagavimas", "augintinis.html"],
    "dokumentai.html": ["Dokumentai", "augintinis.html"],
    "dokumentas-ikelti.html": ["Įkėlimas", "dokumentai.html"],
    "dokumentas.html": ["Dokumentas", "dokumentai.html"],
    "dokumentas-redaguoti.html": ["Redagavimas", "dokumentas.html"],
    "uzsakymai.html": ["Užsakymai", "index.html"],
    "uzsakymas-kurti.html": ["Naujas užsakymas", "uzsakymai.html"],
    "uzsakymas-formos-perziura.html": ["Formos peržiūra", "uzsakymas-kurti.html"],
    "uzsakymas.html": ["Užsakymas", "uzsakymai.html"],
    "uzsakymas-redaguoti.html": ["Redagavimas", "uzsakymas.html"],
    "uzsakymo-patvirtinimas.html": ["Patvirtinimas", "uzsakymas.html"],
    "dnr.html": ["DNR pavyzdžiai", "index.html"],
    "dnr-kurti.html": ["Naujas pavyzdys", "dnr.html"],
    "dnr-pavyzdys.html": ["Pavyzdys", "dnr.html"],
    "dnr-redaguoti.html": ["Redagavimas", "dnr-pavyzdys.html"],
    "dnr-rezultatai.html": ["Rezultatų įvedimas", "dnr-pavyzdys.html"],
    "dnr-tinkamumas.html": ["Tinkamumo analizė", "dnr-pavyzdys.html"],
    "dnr-tinkamumo-rezultatas.html": ["Analizės rezultatas", "dnr-tinkamumas.html"],
    "inkubatoriai.html": ["Inkubatoriai", "index.html"],
    "inkubatorius-kurti.html": ["Naujas inkubatorius", "inkubatoriai.html"],
    "inkubatorius.html": ["Inkubatorius", "inkubatoriai.html"],
    "inkubatorius-busena.html": ["Būsenos redagavimas", "inkubatorius.html"],
    "inkubatoriaus-priskyrimas.html": ["Priskyrimas", "inkubatoriai.html"],
    "inkubatoriaus-priskyrimo-rezultatas.html": ["Rezultatas", "inkubatoriaus-priskyrimas.html"],
    "darbuotojai.html": ["Darbuotojai", "index.html"],
    "darbuotojas-kurti.html": ["Naujas darbuotojas", "darbuotojai.html"],
    "darbuotojas.html": ["Darbuotojas", "darbuotojai.html"],
    "darbuotojas-redaguoti.html": ["Redagavimas", "darbuotojas.html"],
    "darbuotojo-tyrimai.html": ["Tyrimai", "darbuotojas.html"],
    "tyrimas.html": ["Tyrimas", "darbuotojo-tyrimai.html"],
    "kruvio-analize.html": ["Krūvio analizė", "index.html"],
    "langai.html": ["Visi langai", "index.html"]
};

function dabartinisPuslapis() {
    return location.pathname.split("/").pop() || "index.html";
}

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

function gautiRole() {
    try {
        const role = localStorage.getItem("role");
        return ROLES[role] ? role : "";
    } catch (e) { return ""; }
}

function nustatytiRole(role) {
    try {
        if (role) localStorage.setItem("role", role);
        else localStorage.removeItem("role");
    } catch (e) { /* naršyklė neleidžia saugoti – prototipas veikia ir be to */ }
}

// Pereiti į kitą langą su pranešimu
function eiti(tikslas, pranesimas, klaida) {
    if (!tikslas) return;
    const url = new URL(tikslas, location.href);
    if (pranesimas) url.searchParams.set(klaida ? "klaida" : "msg", pranesimas);
    location.href = url.href;
}

function pieštiMeniu() {
    const nav = document.getElementById("meniu");
    if (!nav) return;
    const role = gautiRole();
    const punktai = [...(MENIU[role] || MENIU["svecias"]), ["langai.html", "Visi langai"]];
    // Pažymimas meniu punktas, kurio skyriuje yra dabartinis langas
    const puslapis = dabartinisPuslapis();
    const aktyvus = puslapis === "index.html" ? puslapis : kelias(puslapis, role).reverse()
        .find(p => p !== "index.html" && punktai.some(([href]) => href === p));
    const punktas = ([href, tekstas]) => {
        const dabar = href === aktyvus ? ` aria-current="${href === puslapis ? "page" : "true"}"` : "";
        return `<a class="punktas" href="${href}"${dabar}>${tekstas}</a>`;
    };
    nav.innerHTML = '<a class="logotipas" href="index.html">Gyvūnų klonavimo sistema</a>' +
        punktai.map(punktas).join("") +
        '<label class="role" title="Tik prototipe: rolės perjungimas be prisijungimo">Rolė <select>' +
        ["", ...Object.keys(ROLES)].map(r =>
            `<option value="${r}"${r === role ? " selected" : ""}>${ROLES[r] || "Svečias"}</option>`).join("") +
        "</select></label>";
    nav.querySelector(".role select").addEventListener("change", e => {
        nustatytiRole(e.target.value);
        location.reload();
    });
}

// Naršymo kelias virš lango antraštės
function pieštiKelia() {
    const main = document.querySelector("main");
    const role = gautiRole();
    const grandine = kelias(dabartinisPuslapis(), role);
    if (!main || grandine.length < 2) return;
    // Prisijungusiam pradinis langas vadinamas „Pagrindinis“, kaip ir meniu
    const pavadinimas = p => p === "index.html" && role ? "Pagrindinis" : LANGAI[p][0];
    const nav = document.createElement("nav");
    nav.className = "kelias";
    nav.setAttribute("aria-label", "Naršymo kelias");
    nav.innerHTML = grandine.map((p, i) => i === grandine.length - 1
        ? `<span aria-current="page">${pavadinimas(p)}</span>`
        : `<a href="${p}">${pavadinimas(p)}</a>`).join('<span aria-hidden="true"> › </span>');
    main.prepend(nav);
}

// Svečiui – „Pradžios langas“, prisijungusiam – „Pagrindinis langas“
function pavadintiPradiniLanga() {
    const pavadinimas = gautiRole() ? "Pagrindinis langas" : "Pradžios langas";
    document.querySelectorAll("[data-pradinis]").forEach(el => { el.textContent = pavadinimas; });
    if (dabartinisPuslapis() === "index.html") document.title = `${pavadinimas} – Gyvūnų klonavimo sistema`;
}

function rodytiPranesima() {
    const vieta = document.getElementById("pranesimas");
    if (!vieta) return;
    const p = new URLSearchParams(location.search);
    const tekstas = p.get("msg") || p.get("klaida");
    if (!tekstas) return;
    vieta.innerHTML = `<div class="pranesimas${p.get("klaida") ? " klaida" : ""}"></div>`;
    vieta.firstChild.textContent = tekstas;
    // Pašalinamas iš adreso, kad atnaujinus ar grįžus atgal nesikartotų
    history.replaceState(null, "", location.pathname);
}

function klaida(tekstas) {
    const vieta = document.getElementById("pranesimas");
    vieta.innerHTML = '<div class="pranesimas klaida"></div>';
    vieta.firstChild.textContent = tekstas;
    window.scrollTo(0, 0);
}

// Formos: patikrinami privalomi laukai, tada pereinama į tikslinį langą
function paruostiFormas() {
    document.querySelectorAll("form[data-tikslas]").forEach(forma => {
        forma.addEventListener("submit", e => {
            e.preventDefault();
            if (!forma.checkValidity()) {
                klaida(forma.dataset.klaida || "Duomenys neteisingi. Patikrinkite pažymėtus laukus.");
                forma.reportValidity();
                return;
            }
            const sl = forma.querySelectorAll("input[type=password]");
            if (sl.length === 2 && sl[0].value !== sl[1].value) {
                klaida("Slaptažodžiai nesutampa.");
                return;
            }
            if (forma.dataset.role !== undefined) {
                nustatytiRole(forma.querySelector("[name=role]")?.value || "");
            }
            eiti(forma.dataset.tikslas, forma.dataset.pranesimas);
        });
    });
}

// Ištrynimo patvirtinimo langas
function paruostiIstrynima() {
    const langas = document.createElement("dialog");
    langas.innerHTML = `
        <h2>Ištrynimo patvirtinimas</h2>
        <p id="istrinti-tekstas"></p>
        <div class="mygtukai">
            <button class="mygtukas pavojingas" id="istrinti-taip">Patvirtinti</button>
            <button class="mygtukas antrinis" id="istrinti-ne">Atšaukti</button>
        </div>`;
    document.body.appendChild(langas);
    let tikslas = "", pranesimas = "", atsijungti = false;

    document.querySelectorAll("[data-istrinti]").forEach(mygtukas => {
        mygtukas.addEventListener("click", e => {
            e.preventDefault();
            tikslas = mygtukas.dataset.istrinti;
            pranesimas = mygtukas.dataset.pranesimas || "Įrašas ištrintas.";
            atsijungti = mygtukas.dataset.atsijungti !== undefined;
            langas.querySelector("#istrinti-tekstas").textContent =
                `Ar tikrai norite ištrinti ${mygtukas.dataset.objektas || "šį įrašą"}? Šio veiksmo atšaukti negalima.`;
            langas.showModal();
        });
    });
    langas.querySelector("#istrinti-taip").onclick = () => {
        // Ištrynus savo paskyrą naudotojas atjungiamas
        if (atsijungti) nustatytiRole("");
        eiti(tikslas, pranesimas);
    };
    langas.querySelector("#istrinti-ne").onclick = () => langas.close();
}

// Veiksmų mygtukai, kurie pereina į langą su pranešimu
function paruostiVeiksmus() {
    document.querySelectorAll("[data-eiti]").forEach(m => {
        m.addEventListener("click", e => {
            e.preventDefault();
            eiti(m.dataset.eiti, m.dataset.pranesimas, m.dataset.klaida !== undefined);
        });
    });
    const atsijungti = document.getElementById("atsijungti");
    if (atsijungti) atsijungti.addEventListener("click", () => {
        nustatytiRole("");
        eiti("prisijungimas.html", "Sėkmingai atsijungta.");
    });
}

// Pradiniame lange rodomos tik prisijungusios rolės plytelės
function rodytiPlyteles() {
    const role = gautiRole() || "svecias";
    document.querySelectorAll("[data-rolei]").forEach(el => {
        el.hidden = !el.dataset.rolei.split(" ").includes(role);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    pieštiMeniu();
    pieštiKelia();
    pavadintiPradiniLanga();
    rodytiPranesima();
    paruostiFormas();
    paruostiIstrynima();
    paruostiVeiksmus();
    rodytiPlyteles();
});
