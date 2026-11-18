// Gyvūnų klonavimo sistema – nedideli naudotojo sąsajos patogumai.
// Visa logika vykdoma serveryje; be JavaScript formos veikia, tik be patvirtinimo lango.

// «boundary» Ištrynimo patvirtinimo langas: <form data-patvirtinimas="Ar tikrai...?" data-atsaukta="...">
let dialogas;
function klausti(tekstas, antraste) {
    if (!dialogas) {
        dialogas = document.createElement("dialog");
        dialogas.innerHTML = `<h2></h2><p></p><div class="mygtukai">
            <button type="button" class="mygtukas pavojingas taip">Patvirtinti</button>
            <button type="button" class="mygtukas antrinis ne">Atšaukti</button></div>`;
        document.body.appendChild(dialogas);
    }
    dialogas.querySelector("h2").textContent = antraste || "Ištrynimo patvirtinimo langas";
    dialogas.querySelector("p").textContent = tekstas;
    return new Promise(atsakymas => {
        dialogas.querySelector(".taip").onclick = () => { dialogas.close(); atsakymas(true); };
        dialogas.querySelector(".ne").onclick = () => { dialogas.close(); atsakymas(false); };
        dialogas.oncancel = () => atsakymas(false);
        dialogas.showModal();
    });
}

function pranesti(tekstas, tipas) {
    const vieta = document.getElementById("pranesimas");
    if (!vieta) return;
    vieta.innerHTML = `<div class="pranesimas ${tipas}" role="status"></div>`;
    vieta.firstChild.textContent = tekstas;
}

document.addEventListener("submit", async e => {
    const forma = e.target;
    if (!forma.dataset.patvirtinimas || forma.dataset.patvirtinta) return;
    e.preventDefault();
    if (await klausti(forma.dataset.patvirtinimas, forma.dataset.antraste)) {
        forma.dataset.patvirtinta = "1";
        forma.requestSubmit(e.submitter);
    } else if (forma.dataset.atsaukta) {
        pranesti(forma.dataset.atsaukta, "info");
    }
});

// Spausdinamas visas langas arba tik nurodytas elementas: <button data-spausdinti="#etikete">
document.addEventListener("click", e => {
    const mygtukas = e.target.closest("[data-spausdinti]");
    if (!mygtukas) return;
    const elementas = mygtukas.dataset.spausdinti && document.querySelector(mygtukas.dataset.spausdinti);
    if (elementas) {
        document.body.classList.add("spausdinimas");
        elementas.classList.add("spausdinama");
        window.addEventListener("afterprint", () => {
            document.body.classList.remove("spausdinimas");
            elementas.classList.remove("spausdinama");
        }, { once: true });
    }
    window.print();
});

// Pataisius neteisingai užpildytą lauką, jo žymėjimas pašalinamas
document.addEventListener("input", e => e.target.closest(".laukas")?.classList.remove("netinkamas"));
