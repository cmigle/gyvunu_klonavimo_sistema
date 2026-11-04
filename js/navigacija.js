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
        ["index.html", "Pradinis"],
        ["prisijungimas.html", "Prisijungti"],
        ["registracija.html", "Registruotis"]
    ],
    klientas: [
        ["index.html", "Pradinis"],
        ["profilis.html", "Mano profilis"],
        ["augintiniai.html", "Mano augintiniai"],
        ["uzsakymai.html", "Užsakymai"],
        ["atsijungimas.html", "Atsijungti"]
    ],
    specialistas: [
        ["index.html", "Pradinis"],
        ["uzsakymai.html", "Užsakymai"],
        ["dnr.html", "DNR pavyzdžiai"],
        ["inkubatoriai.html", "Inkubatoriai"],
        ["atsijungimas.html", "Atsijungti"]
    ],
    administratorius: [
        ["index.html", "Pradinis"],
        ["darbuotojai.html", "Darbuotojai"],
        ["kruvio-analize.html", "Krūvio analizė"],
        ["atsijungimas.html", "Atsijungti"]
    ]
};

function gautiRole() {
    try { return localStorage.getItem("role") || ""; } catch (e) { return ""; }
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
    const punktai = MENIU[role] || MENIU["svecias"];
    nav.innerHTML = '<a class="logotipas" href="index.html">Gyvūnų klonavimo sistema</a>' +
        punktai.map(([href, tekstas]) => `<a class="punktas" href="${href}">${tekstas}</a>`).join("") +
        `<span class="role">${role ? ROLES[role] : "Svečias"}</span>`;
}

function rodytiPranesima() {
    const vieta = document.getElementById("pranesimas");
    if (!vieta) return;
    const p = new URLSearchParams(location.search);
    const tekstas = p.get("msg") || p.get("klaida");
    if (!tekstas) return;
    vieta.innerHTML = `<div class="pranesimas${p.get("klaida") ? " klaida" : ""}"></div>`;
    vieta.firstChild.textContent = tekstas;
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
    rodytiPranesima();
    paruostiFormas();
    paruostiIstrynima();
    paruostiVeiksmus();
    rodytiPlyteles();
});
