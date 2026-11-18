// Gyvūnų klonavimo sistema – langų turinys iš demonstracinių duomenų
// Kiekvienam langui – funkcija, kuri užpildo jį duomenimis ir aprašo veiksmus
// pagal panaudojimo atvejų sekų diagramas (ataskaitos 2.2 skyrius).

const PUSLAPIAI = {};
const $ = (selektorius, kur = document) => kur.querySelector(selektorius);

// ---------- Bendros dalys ----------

const html = s => ({ html: s });
const nuorodaHtml = (tekstas, href) => html(`<a href="${href}">${esc(tekstas)}</a>`);
const augintinioPavadinimas = a => a ? `${a.vardas} (${a.tipas.toLowerCase()})` : "–";
const metai = n => n == null ? "–" : `${n} m.`;
const laikotarpis = k => `${k.pradzios_data} – ${k.pabaigos_data}`;
const galiojimas = d => d.galiojimo_data >= siandien() ? "Galioja" : "Nebegalioja";
const esuKlientas = () => gautiRole() === "klientas";
const pasiekiamiAugintiniai = () => esuKlientas() ? klientoAugintiniai(naudotojas().id) : visi("augintiniai");
const pasiekiamiUzsakymai = () => esuKlientas() ? klientoUzsakymai(naudotojas().id) : visi("uzsakymai");

// Trumpa santrauka virš sąrašo: „Augintiniai: 3 | Dokumentai: 4“
function rodikliai(vieta, sarasas) {
    vieta.innerHTML = sarasas.map(([reiksme, pavadinimas]) => `${esc(pavadinimas)}: <b>${esc(reiksme)}</b>`).join(" &nbsp;|&nbsp; ");
}

// Lentelės eilutės; tuščiam sąrašui – pranešimas per visą plotį
function eilutes(tbody, irasai, eilute, tusciaTekstas) {
    const stulpeliai = tbody.closest("table").querySelectorAll("thead th").length;
    tbody.innerHTML = irasai.length ? irasai.map(eilute).join("")
        : `<tr><td class="tuscia" colspan="${stulpeliai}">${esc(tusciaTekstas)}</td></tr>`;
}

// Lentelės eilutės veiksmai – nuorodos „Peržiūrėti | Redaguoti | Ištrinti“
const nuorodosMygtukas = (tekstas, href) => `<a href="${href}">${tekstas}</a>`;
const veiksmoMygtukas = (tekstas, veiksmas, id, klase = "") =>
    `<button type="button" class="nuoroda ${klase === "pavojingas" ? "pavojinga" : ""}" data-veiksmas="${veiksmas}" data-id="${id}">${tekstas}</button>`;
const veiksmai = (...mygtukai) => `<td class="veiksmai">${mygtukai.filter(Boolean).join(" | ")}</td>`;

// Eilučių mygtukų paspaudimai: kaiPaspausta(tbody, { istrinti: id => ... })
function kaiPaspausta(konteineris, apdorojimai) {
    konteineris.addEventListener("click", e => {
        const mygtukas = e.target.closest("[data-veiksmas]");
        if (mygtukas) apdorojimai[mygtukas.dataset.veiksmas]?.(Number(mygtukas.dataset.id), mygtukas);
    });
}

function parinktys(select, irasai, tekstas, pazymetas) {
    select.insertAdjacentHTML("beforeend", irasai.map(x =>
        `<option value="${x.id}"${x.id === pazymetas ? " selected" : ""}>${esc(tekstas(x))}</option>`).join(""));
}

// Lango įrašas pagal adreso parametrą; atidarius be jo (pvz., iš „Visi langai“) – pirmas galimas
function pagalParametra(vardas, galimi) {
    if (!param(vardas)) return galimi[0] || null;
    return galimi.find(x => x.id === Number(param(vardas))) || null;
}

// Įrašo nėra (ištrintas ar nepasiekiamas) – vietoj lango turinio rodomas pranešimas
function nera(tekstas, atgal, atgalTekstas) {
    const main = $("main");
    [...main.children].forEach(el => { if (!el.matches(".antraste, .kelias, #pranesimas")) el.hidden = true; });
    main.insertAdjacentHTML("beforeend", `<div class="kortele tuscias-blokas"><p>${esc(tekstas)}</p>
        ${atgal ? `<a class="mygtukas antrinis" href="${atgal}">${esc(atgalTekstas)}</a>` : ""}</div>`);
}

// ---------- Pradžios / pagrindinis langas ----------

PUSLAPIAI["index.html"] = () => {
    const role = gautiRole();
    const asmuo = naudotojas();
    if (asmuo) $("#pasisveikinimas").textContent = `Sveiki, ${asmuo.vardas}! Pasirinkite, ką norite daryti.`;
    $("#pasisveikinimas").hidden = !asmuo;

    if (role === "klientas") {
        const uzsakymai = klientoUzsakymai(asmuo.id).sort((a, b) => b.sukurimo_data.localeCompare(a.sukurimo_data));
        rodikliai($("#kliento-rodikliai"), [
            [klientoAugintiniai(asmuo.id).length, "Augintiniai"],
            [uzsakymai.length, "Užsakymai", "info"],
            [uzsakymai.filter(u => !u.busena).length, "Laukia patvirtinimo", "ispejimas"],
            [asmuo.istorijos_busena, "Teistumo istorijos patikrinimas", asmuo.istorijos_busena === "Patikrintas" ? "sekme" : "klaida"]
        ]);
        eilutes($("#kliento-uzsakymai"), uzsakymai, u => {
            const a = rasti("augintiniai", u.augintinis);
            const href = nuoroda("uzsakymas.html", { uzsakymas: u.id });
            return `<tr><td><a class="pagrindinis" href="${href}">${uzsakymoNr(u)}</a></td>
                <td>${esc(a.vardas)}<small>${esc(a.tipas)}</small></td>
                <td class="skaicius">${pinigai(u.kaina)}</td>
                <td>${zyma(busena("uzsakymas", u.busena))}</td>
                ${veiksmai(nuorodosMygtukas("Peržiūrėti", href))}</tr>`;
        }, "Užsakymų dar nėra.");
    }

    if (role === "specialistas") {
        const inkubatoriai = visi("inkubatoriai");
        rodikliai($("#specialisto-rodikliai"), [
            [visi("dnr").length, "DNR pavyzdžiai"],
            [visi("dnr").filter(x => !x.tinkamumas).length, "Laukia tinkamumo nustatymo", "ispejimas"],
            [laukiaInkubatoriaus().length, "Laukia inkubatoriaus", "info"],
            [`${inkubatoriai.filter(i => !i.busena).length} / ${inkubatoriai.length}`, "Laisvi inkubatoriai", "sekme"]
        ]);
        const naujausi = [...visi("dnr")].sort((a, b) => b.paemimo_data.localeCompare(a.paemimo_data)).slice(0, 5);
        eilutes($("#naujausi-dnr"), naujausi, x => {
            const href = nuoroda("dnr-pavyzdys.html", { dnr: x.id });
            return `<tr><td><a class="pagrindinis" href="${href}">${dnrKodas(x)}</a></td>
                <td>${esc(augintinioPavadinimas(rasti("augintiniai", x.augintinis)))}</td>
                <td>${x.paemimo_data}</td><td>${zyma(x.tinkamumas || "Neįvertintas")}</td>
                ${veiksmai(nuorodosMygtukas("Peržiūrėti", href))}</tr>`;
        }, "DNR pavyzdžių nėra.");
    }

    if (role === "administratorius") {
        const kruviai = kruvioAnalize();
        rodikliai($("#administratoriaus-rodikliai"), [
            [visi("darbuotojai").length, "Darbuotojai"],
            [specialistai().length, "Laboratorijos specialistai", "info"],
            [visi("klonavimai").filter(k => k.busena).length, "Aktyvūs klonavimai", "sekme"],
            [kruviai.filter(r => r.kruvis > 100).length, "Didelis darbo krūvis", "klaida"]
        ]);
        eilutes($("#pradzios-darbuotojai"), visi("darbuotojai"), d => {
            const href = nuoroda("darbuotojas.html", { darbuotojas: d.id });
            const r = kruviai.find(x => x.darbuotojas === d);
            return `<tr><td><a class="pagrindinis" href="${href}">${esc(vardasPavarde(d))}</a></td>
                <td>${zyma(ROLES[d.role])}</td><td class="skaicius">${skaicius(d.etatas)}</td>
                <td>${r ? `${zyma(r.lygis)} <small>${skaicius(r.kruvis, 1)} %</small>` : "–"}</td>
                ${veiksmai(nuorodosMygtukas("Peržiūrėti", href))}</tr>`;
        }, "Darbuotojų nėra.");
    }
};

// ---------- Kliento ir augintinio valdymas ----------

// 2.3 pav. PA „Prisijungimas“
PUSLAPIAI["prisijungimas.html"] = () => {
    const forma = $("#prisijungimo-forma");
    const l = forma.elements;
    if (param("el_pastas")) l.el_pastas.value = param("el_pastas");
    const demo = [
        [rasti("klientai", 1), "Klientas, istorija patikrinta"],
        [rasti("klientai", 2), "Klientas, istorija nepatikrinta"],
        [rasti("darbuotojai", 1), "Laboratorijos specialistas"],
        [rasti("darbuotojai", 5), "Administratorius"]
    ].filter(([x]) => x);
    const sarasas = $("#demo-paskyros");
    // Paspaudus vardą užpildoma prisijungimo forma
    sarasas.innerHTML = demo.map(([x, aprasas]) =>
        `<li><button type="button" class="nuoroda" data-el="${esc(x.el_pastas)}">${esc(vardasPavarde(x))}</button> <span>${aprasas.toLowerCase()}</span></li>`).join("");
    sarasas.addEventListener("click", e => {
        const mygtukas = e.target.closest("[data-el]");
        if (!mygtukas) return;
        l.el_pastas.value = mygtukas.dataset.el;
        l.slaptazodis.value = DEMO_SLAPTAZODIS;
        forma.querySelectorAll(".netinkamas").forEach(x => x.classList.remove("netinkamas"));
    });
    kaiPateikta(forma, d => {
        const paskyra = rastiPaskyra(d.el_pastas, d.slaptazodis);
        if (!paskyra) return pazymetiLaukus([l.el_pastas, l.slaptazodis], "Neteisingi prisijungimo duomenys.");
        prisijungti(paskyra.role, paskyra.id);
        eiti("index.html", "Sėkmingai prisijungta.");
    });
};

// 2.2 pav. PA „Registracija“
PUSLAPIAI["registracija.html"] = () => {
    const forma = $("#registracijos-forma");
    const l = forma.elements;
    kaiPateikta(forma, d => {
        if (elPastasUzimtas(d.el_pastas)) return pazymetiLaukus([l.el_pastas], "Toks el. paštas jau užregistruotas.");
        if (visi("klientai").some(k => k.asmens_kodas === d.asmens_kodas))
            return pazymetiLaukus([l.asmens_kodas], "Klientas su tokiu asmens kodu jau užregistruotas.");
        if (d.slaptazodis !== d.slaptazodis2) return pazymetiLaukus([l.slaptazodis2], "Slaptažodžiai nesutampa.");
        sukurti("klientai", {
            vardas: d.vardas, pavarde: d.pavarde, el_pastas: d.el_pastas, telefono_numeris: d.telefono_numeris,
            asmens_kodas: d.asmens_kodas, slaptazodis: d.slaptazodis,
            istorijos_busena: "Nepatikrintas", patikrinimo_data: null, rasta_irasu: null
        });
        eiti(nuoroda("prisijungimas.html", { el_pastas: d.el_pastas }), "Paskyra sukurta. Galite prisijungti.");
    });
};

// 2.4 pav. PA „Atsijungimas“ (atsijungimo mygtukas – navigacija.js)
PUSLAPIAI["atsijungimas.html"] = () => {
    const asmuo = naudotojas();
    $("#atsijungimo-tekstas").textContent = asmuo
        ? `Prisijungęs: ${vardasPavarde(asmuo)} (${ROLES[gautiRole()]}). Ar norite atsijungti nuo sistemos?`
        : "Jūs neprisijungęs.";
    $("#atsijungti").hidden = !asmuo;
};

PUSLAPIAI["profilis.html"] = () => {
    const k = naudotojas();
    const augintiniai = klientoAugintiniai(k.id);
    rodikliai($("#rodikliai"), [
        [augintiniai.length, "Augintiniai"],
        [klientoUzsakymai(k.id).length, "Užsakymai", "info"],
        [augintiniai.reduce((suma, a) => suma + augintinioDokumentai(a.id).length, 0), "Dokumentai"]
    ]);
    uzpildyti($("main"), {
        vardas: k.vardas, pavarde: k.pavarde, el_pastas: k.el_pastas, telefono_numeris: k.telefono_numeris,
        asmens_kodas: k.asmens_kodas, istorijos_busena: k.istorijos_busena, istorijos_zyma: html(zyma(k.istorijos_busena)),
        patikrinimo_data: k.patikrinimo_data, rasta_irasu: k.rasta_irasu
    });
    $("#istrinti-paskyra").addEventListener("click", async () => {
        if (klientoUzsakymai(k.id).some(u => u.busena)) return klaida("Paskyros ištrinti negalima, nes turite patvirtintų užsakymų.");
        const taip = await klausti({ tekstas: "Ar tikrai norite ištrinti savo paskyrą? Kartu bus ištrinti jūsų augintiniai, jų dokumentai ir nepatvirtinti užsakymai." });
        if (!taip) return;
        istrintiKlienta(k.id);
        atsijungti();
        eiti("prisijungimas.html", "Paskyra ištrinta.");
    });
};

PUSLAPIAI["profilis-redaguoti.html"] = () => {
    const k = naudotojas();
    const forma = $("#profilio-forma");
    uzpildytiForma(forma, k);
    kaiPateikta(forma, d => {
        if (elPastasUzimtas(d.el_pastas, k)) return pazymetiLaukus([forma.elements.el_pastas], "Toks el. paštas jau naudojamas kitos paskyros.");
        // Pasikeitus asmens duomenims, ankstesnis istorijos patikrinimas nebegalioja
        const pasikeite = ["vardas", "pavarde", "asmens_kodas"].some(l => d[l] !== k[l]) && k.istorijos_busena !== "Nepatikrintas";
        atnaujinti("klientai", k.id, Object.assign(d, pasikeite ? { istorijos_busena: "Nepatikrintas", patikrinimo_data: null, rasta_irasu: null } : {}));
        eiti("profilis.html", pasikeite
            ? "Profilio duomenys atnaujinti. Pasikeitus asmens duomenims, teistumo istoriją reikia patikrinti iš naujo."
            : "Profilio duomenys atnaujinti.");
    });
};

// 2.9 pav. PA „Patikrinti kliento istoriją“
PUSLAPIAI["istorijos-patikrinimas.html"] = () => {
    const k = naudotojas();
    const forma = $("#istorijos-forma");
    uzpildytiForma(forma, k);
    kaiPateikta(forma, d => {
        const rezultatas = patikrintiIstorija(k.id, d.vardas, d.pavarde);
        const tikslas = nuoroda("istorijos-rezultatas.html", { grizti: param("grizti") });
        if (rezultatas.rasta_irasu) eiti(tikslas, "Rasta teistumo įrašų, todėl klientas netinkamas.", "klaida");
        else eiti(tikslas, "Patikrinimas sėkmingas: teistumo įrašų nerasta. Esate pažymėti kaip patikrintas klientas.");
    });
};

PUSLAPIAI["istorijos-rezultatas.html"] = () => {
    const k = naudotojas();
    if (!k.patikrinimo_data) return nera("Teistumo istorija dar netikrinta.", "istorijos-patikrinimas.html", "Patikrinti istoriją");
    const patikrintas = k.istorijos_busena === "Patikrintas";
    const [vardas, ...pavarde] = (k.istorijos_asmuo || vardasPavarde(k)).split(" ");
    uzpildyti($("#rezultatas"), {
        zyma: html(zyma(k.istorijos_busena)),
        asmuo: k.istorijos_asmuo || vardasPavarde(k),
        patikrinimo_data: k.patikrinimo_data,
        uzklausa: html(`<code>${esc(k.istorijos_uzklausa || courtListenerPaieska(vardas, pavarde.join(" ")).uzklausa)}</code>`),
        rasta_irasu: k.rasta_irasu,
        rezultatas: patikrintas ? "Teistumo įrašų nerasta, klientas pažymėtas kaip patikrintas." : "Rasta teistumo įrašų, todėl klientas netinkamas."
    });
    const bylos = k.istorijos_irasai || [];
    if (bylos.length) {
        $("#bylos").hidden = false;
        eilutes($("#bylos tbody"), bylos, b => `<tr><td>${esc(b.numeris)}</td><td>${esc(b.byla)}</td><td>${esc(b.teismas)}</td><td>${b.data}</td></tr>`, "");
    }
    // Atėjus iš užsakymo kūrimo («include»), siūloma jį tęsti
    if (param("grizti") === "uzsakymas-kurti.html" && patikrintas)
        $("#rezultato-mygtukai").insertAdjacentHTML("afterbegin", '<a class="mygtukas" href="uzsakymas-kurti.html">Tęsti užsakymo kūrimą</a>');
};

PUSLAPIAI["augintiniai.html"] = () => {
    const augintiniai = klientoAugintiniai(naudotojas().id);
    // Kliento augintinių informacijos suvestinė
    rodikliai($("#rodikliai"), [
        [augintiniai.length, "Augintiniai"],
        [augintiniai.reduce((suma, a) => suma + augintinioDokumentai(a.id).length, 0), "Dokumentai"],
        [augintiniai.filter(a => augintinioUzsakymas(a.id)).length, "Su užsakymu", "info"],
        [augintiniai.filter(a => augintinioDnr(a.id)?.tinkamumas === TINKAMAS).length, "Tinkami DNR pavyzdžiai", "sekme"]
    ]);
    const tbody = $("#augintiniai");
    eilutes(tbody, augintiniai, a => {
        const u = augintinioUzsakymas(a.id);
        const p = { augintinis: a.id };
        return `<tr>
            <td><a class="pagrindinis" href="${nuoroda("augintinis.html", p)}">${esc(a.vardas)}</a><small>${esc(a.lytis)}</small></td>
            <td>${esc(a.tipas)}</td><td>${esc(a.veisle)}</td><td class="skaicius">${metai(a.amzius)}</td>
            <td><code>${esc(a.mikroschemos_numeris)}</code></td>
            <td>${u ? `${zyma(busena("uzsakymas", u.busena))}<small>${uzsakymoNr(u)}</small>` : zyma("Nėra")}</td>
            <td class="skaicius">${augintinioDokumentai(a.id).length}</td>
            ${veiksmai(
                nuorodosMygtukas("Peržiūrėti", nuoroda("augintinis.html", p)),
                nuorodosMygtukas("Redaguoti", nuoroda("augintinis-redaguoti.html", p)),
                nuorodosMygtukas("Dokumentai", nuoroda("dokumentai.html", p)),
                veiksmoMygtukas("Ištrinti", "istrinti", a.id, "pavojingas"))}
        </tr>`;
    }, "Augintinių dar nėra. Sukurkite pirmąjį augintinį.");

    // 3.1.11 pav.: ištrinant augintinį pašalinami ir jo dokumentai
    kaiPaspausta(tbody, {
        istrinti: async id => {
            const a = rasti("augintiniai", id);
            const u = augintinioUzsakymas(id);
            if (u?.busena) return klaida(`Augintinio „${a.vardas}“ ištrinti negalima, nes jo užsakymas ${uzsakymoNr(u)} jau patvirtintas.`);
            const dokumentai = augintinioDokumentai(id).length;
            const kartu = [dokumentai && `dokumentai (${dokumentai})`, u && `nepatvirtintas užsakymas ${uzsakymoNr(u)}`].filter(Boolean);
            const taip = await klausti({
                tekstas: `Ar tikrai norite ištrinti augintinį „${a.vardas}“?${kartu.length ? ` Kartu bus ištrinti: ${kartu.join(", ")}.` : ""} Šio veiksmo atšaukti negalima.`
            });
            if (!taip) return;
            istrintiAugintini(id);
            eiti("augintiniai.html", `Augintinis „${a.vardas}“ ištrintas.`);
        }
    });
};

function tipoParinktys(select) {
    select.insertAdjacentHTML("beforeend", GYVUNO_TIPAI.map(t => `<option value="${t}">${t}</option>`).join(""));
}

const mikroschemaUzimta = (numeris, isskyrus) => visi("augintiniai").some(a => a !== isskyrus && a.mikroschemos_numeris === numeris);

// 2.10 pav. PA „Sukurti gyvūną“
PUSLAPIAI["augintinis-kurti.html"] = () => {
    const forma = $("#augintinio-forma");
    tipoParinktys(forma.elements.tipas);
    kaiPateikta(forma, d => {
        if (mikroschemaUzimta(d.mikroschemos_numeris))
            return pazymetiLaukus([forma.elements.mikroschemos_numeris], "Augintinis su tokiu mikroschemos numeriu jau užregistruotas.");
        const a = sukurti("augintiniai", { klientas: naudotojas().id, ...d });
        eiti(nuoroda("augintinis.html", { augintinis: a.id }), `Augintinis „${a.vardas}“ sukurtas.`);
    });
};

PUSLAPIAI["augintinis.html"] = () => {
    const a = pagalParametra("augintinis", pasiekiamiAugintiniai());
    if (!a) return nera("Augintinis nerastas.", "augintiniai.html", "Grįžti į sąrašą");
    const u = augintinioUzsakymas(a.id);
    const pavyzdys = augintinioDnr(a.id);
    const kl = u && uzsakymoKlonavimas(u.id);
    uzpildyti($("main"), {
        vardas: a.vardas, veisle: a.veisle, tipas: a.tipas, lytis: a.lytis, mikroschemos_numeris: a.mikroschemos_numeris,
        tipas_zyma: html(zyma(a.tipas, "info")), amzius: metai(a.amzius), svoris: suVienetais(a.svoris, "kg"),
        savininkas: vardasPavarde(rasti("klientai", a.klientas)),
        uzsakymas: u ? html(`<a href="${nuoroda("uzsakymas.html", { uzsakymas: u.id })}">${uzsakymoNr(u)}</a> ${zyma(busena("uzsakymas", u.busena))}`) : "Užsakymo nėra",
        dnr: pavyzdys ? html(`${dnrKodas(pavyzdys)} ${zyma(pavyzdys.tinkamumas || "Neįvertintas")}`) : "Pavyzdys dar nepaimtas",
        klonavimas: kl ? html(`${klonavimoNr(kl)} ${zyma(busena("klonavimas", kl.busena))}`) : "Nepradėtas",
        dokumentai: String(augintinioDokumentai(a.id).length)
    });
    $("#redaguoti").href = nuoroda("augintinis-redaguoti.html", { augintinis: a.id });
    $("#dokumentu-nuoroda").href = nuoroda("dokumentai.html", { augintinis: a.id });
    // Augintinis gali turėti tik vieną užsakymą
    $("#kurti-uzsakyma").hidden = !!u;
    $("#kurti-uzsakyma").href = nuoroda("uzsakymas-kurti.html", { augintinis: a.id });
};

PUSLAPIAI["augintinis-redaguoti.html"] = () => {
    const a = pagalParametra("augintinis", pasiekiamiAugintiniai());
    if (!a) return nera("Augintinis nerastas.", "augintiniai.html", "Grįžti į sąrašą");
    $("#aprasas").textContent = `Redaguojami augintinio „${a.vardas}“ duomenys.`;
    const forma = $("#augintinio-forma");
    tipoParinktys(forma.elements.tipas);
    uzpildytiForma(forma, a);
    kaiPateikta(forma, d => {
        if (mikroschemaUzimta(d.mikroschemos_numeris, a))
            return pazymetiLaukus([forma.elements.mikroschemos_numeris], "Augintinis su tokiu mikroschemos numeriu jau užregistruotas.");
        atnaujinti("augintiniai", a.id, d);
        // Nepatvirtinto užsakymo kaina perskaičiuojama pagal gyvūno tipą
        const u = augintinioUzsakymas(a.id);
        if (u && !u.busena) atnaujinti("uzsakymai", u.id, { kaina: kainaPagalTipa(d.tipas) });
        eiti(nuoroda("augintinis.html", { augintinis: a.id }), "Augintinio duomenys atnaujinti.");
    });
};

// Augintinis, kurio dokumentai rodomi: pagal parametrą „augintinis“ arba „uzsakymas“
function dokumentuAugintinis(galimi) {
    const uzsakymas = !param("augintinis") && param("uzsakymas") && rasti("uzsakymai", param("uzsakymas"));
    return uzsakymas ? galimi.find(a => a.id === uzsakymas.augintinis) || null : pagalParametra("augintinis", galimi);
}

// Specialistas dokumentus pasiekia per užsakymą, todėl perduodamas ir užsakymo numeris
const dokumentuParametrai = a => esuKlientas() ? { augintinis: a.id } : { uzsakymas: augintinioUzsakymas(a.id)?.id, augintinis: a.id };

function failoFormatas(failas) {
    const pletinys = failas?.name.split(".").pop().toLowerCase();
    return { pdf: "PDF", jpg: "JPG", jpeg: "JPG", png: "PNG" }[pletinys] || null;
}

// 2.5 pav. PA „Peržiūrėti dokumentus“ (klientas ir laboratorijos specialistas), 2.8 pav. PA „Ištrinti dokumentą“
PUSLAPIAI["dokumentai.html"] = () => {
    const klientas = esuKlientas();
    const augintiniai = klientas ? pasiekiamiAugintiniai() : visi("augintiniai").filter(a => augintinioUzsakymas(a.id));
    const a = dokumentuAugintinis(augintiniai);
    if (!a) return nera(klientas ? "Augintinių dar nėra. Pirmiausia sukurkite augintinį." : "Augintinis nerastas.",
        klientas ? "augintiniai.html" : "uzsakymai.html", "Grįžti");
    const p = dokumentuParametrai(a);
    $("#aprasas").textContent = `Augintinio „${a.vardas}“ (${a.tipas.toLowerCase()}) dokumentai.`;

    const pasirinkimas = $("#augintinio-pasirinkimas");
    parinktys(pasirinkimas, augintiniai, x => klientas ? x.vardas : `${x.vardas} (${vardasPavarde(rasti("klientai", x.klientas))})`, a.id);
    pasirinkimas.addEventListener("change", () => {
        location.href = nuoroda("dokumentai.html", dokumentuParametrai(rasti("augintiniai", pasirinkimas.value)));
    });
    // Specialistas (užsakymų valdytojas) dokumentus tik peržiūri
    $("#ikelti").hidden = !klientas;
    $("#ikelti").href = nuoroda("dokumentas-ikelti.html", p);
    $("#grizti").href = klientas ? "augintiniai.html" : nuoroda("uzsakymas.html", { uzsakymas: p.uzsakymas });
    $("#grizti").textContent = klientas ? "Grįžti į augintinių sąrašą" : "Grįžti į užsakymą";

    const tbody = $("#dokumentai");
    eilutes(tbody, augintinioDokumentai(a.id), d => {
        const dp = { ...p, dokumentas: d.id };
        const mygtukai = [nuorodosMygtukas("Peržiūrėti", nuoroda("dokumentas.html", dp))];
        if (klientas) mygtukai.push(nuorodosMygtukas("Redaguoti", nuoroda("dokumentas-redaguoti.html", dp)),
            veiksmoMygtukas("Ištrinti", "istrinti", d.id, "pavojingas"));
        return `<tr>
            <td><a class="pagrindinis" href="${nuoroda("dokumentas.html", dp)}">${esc(d.pavadinimas)}</a><small>${esc(d.failo_pavadinimas || "")}</small></td>
            <td>${esc(d.dokumento_tipas)}</td><td>${d.ikelimo_data}</td><td>${esc(d.failo_formatas)}</td>
            <td>${d.galiojimo_data} ${zyma(galiojimas(d))}</td>
            ${veiksmai(...mygtukai)}
        </tr>`;
    }, "Dokumentų nėra.");

    kaiPaspausta(tbody, {
        istrinti: async id => {
            const d = rasti("dokumentai", id);
            const taip = await klausti({ tekstas: `Ar tikrai norite ištrinti dokumentą „${d.pavadinimas}“? Šio veiksmo atšaukti negalima.` });
            if (!taip) return;
            pasalinti("dokumentai", id);
            eiti(nuoroda("dokumentai.html", p), "Dokumentas sėkmingai ištrintas.");
        }
    });
};

// 2.6 pav. PA „Įkelti dokumentą“
PUSLAPIAI["dokumentas-ikelti.html"] = () => {
    const a = dokumentuAugintinis(pasiekiamiAugintiniai());
    if (!a) return nera("Augintinių dar nėra. Pirmiausia sukurkite augintinį.", "augintiniai.html", "Grįžti");
    $("#aprasas").textContent = `Dokumentas bus priskirtas augintiniui „${a.vardas}“.`;
    $("#atsaukti").href = nuoroda("dokumentai.html", { augintinis: a.id });
    const forma = $("#dokumento-forma");
    kaiPateikta(forma, d => {
        const formatas = failoFormatas(d.failas);
        if (!formatas) return pazymetiLaukus([forma.elements.failas], "Netinkamas failo formatas. Leidžiami formatai: PDF, JPG, PNG.");
        if (d.failas.size > 10 * 1024 * 1024) return pazymetiLaukus([forma.elements.failas], "Failas per didelis. Leidžiama iki 10 MB.");
        sukurti("dokumentai", {
            augintinis: a.id, pavadinimas: d.pavadinimas, dokumento_tipas: d.dokumento_tipas, ikelimo_data: siandien(),
            failo_formatas: formatas, galiojimo_data: d.galiojimo_data, failo_pavadinimas: d.failas.name
        });
        eiti(nuoroda("dokumentai.html", { augintinis: a.id }), "Dokumentas sėkmingai įkeltas.");
    });
};

PUSLAPIAI["dokumentas.html"] = () => {
    const galimi = pasiekiamiAugintiniai().flatMap(a => augintinioDokumentai(a.id));
    const d = pagalParametra("dokumentas", galimi);
    if (!d) return nera("Dokumentas nerastas.", "dokumentai.html", "Grįžti į sąrašą");
    const a = rasti("augintiniai", d.augintinis);
    uzpildyti($("main"), {
        pavadinimas: d.pavadinimas, dokumento_tipas: d.dokumento_tipas, ikelimo_data: d.ikelimo_data,
        failo_formatas: d.failo_formatas, galiojimo_data: d.galiojimo_data, failo_pavadinimas: d.failo_pavadinimas,
        galiojimas: html(zyma(galiojimas(d))), augintinis: augintinioPavadinimas(a)
    });
    $(".failo-perziura .formatas").style.background = d.failo_formatas === "PDF" ? "#dc3545" : "#0d6efd";
    $("#grizti").href = nuoroda("dokumentai.html", dokumentuParametrai(a));
    // Specialistas dokumentus tik peržiūri
    $("#redaguoti").hidden = !esuKlientas();
    $("#redaguoti").href = nuoroda("dokumentas-redaguoti.html", { augintinis: a.id, dokumentas: d.id });
};

// 2.7 pav. PA „Redaguoti dokumentą“
PUSLAPIAI["dokumentas-redaguoti.html"] = () => {
    const d = pagalParametra("dokumentas", pasiekiamiAugintiniai().flatMap(a => augintinioDokumentai(a.id)));
    if (!d) return nera("Dokumentas nerastas.", "dokumentai.html", "Grįžti į sąrašą");
    $("#aprasas").textContent = `Redaguojamas dokumentas „${d.pavadinimas}“.`;
    $("#dabartinis-failas").textContent = `Dabartinis failas: ${d.failo_pavadinimas || d.failo_formatas}. Leidžiami formatai: PDF, JPG, PNG.`;
    $("#atsaukti").href = nuoroda("dokumentai.html", { augintinis: d.augintinis });
    const forma = $("#dokumento-forma");
    uzpildytiForma(forma, d);
    kaiPateikta(forma, x => {
        const laukai = { pavadinimas: x.pavadinimas, dokumento_tipas: x.dokumento_tipas, galiojimo_data: x.galiojimo_data };
        if (x.failas) {
            const formatas = failoFormatas(x.failas);
            if (!formatas) return pazymetiLaukus([forma.elements.failas], "Netinkamas failo formatas. Leidžiami formatai: PDF, JPG, PNG.");
            Object.assign(laukai, { failo_formatas: formatas, failo_pavadinimas: x.failas.name, ikelimo_data: siandien() });
        }
        atnaujinti("dokumentai", d.id, laukai);
        eiti(nuoroda("dokumentas.html", { augintinis: d.augintinis, dokumentas: d.id }), "Dokumentas atnaujintas.");
    });
};

// ---------- Užsakymo valdymas ----------

PUSLAPIAI["uzsakymai.html"] = () => {
    const klientas = esuKlientas();
    const uzsakymai = [...pasiekiamiUzsakymai()].sort((a, b) => b.sukurimo_data.localeCompare(a.sukurimo_data));
    $("#aprasas").textContent = klientas ? "Jūsų klonavimo užsakymai ir jų būsenos." : "Visų klientų užsakymai ir augintinių dokumentai.";
    $("#kurti").hidden = !klientas;
    if (klientas) $("th.specialistui").remove();
    rodikliai($("#rodikliai"), [
        [uzsakymai.length, "Užsakymai"],
        [uzsakymai.filter(u => !u.busena).length, "Nepatvirtinti", "ispejimas"],
        [uzsakymai.filter(u => u.busena).length, "Patvirtinti", "sekme"],
        [pinigai(uzsakymai.reduce((suma, u) => suma + u.kaina, 0)), "Bendra suma", "info"]
    ]);

    const tbody = $("#uzsakymai");
    eilutes(tbody, uzsakymai, u => {
        const a = rasti("augintiniai", u.augintinis);
        const perziura = nuoroda("uzsakymas.html", { uzsakymas: u.id });
        const mygtukai = klientas
            ? [nuorodosMygtukas("Peržiūrėti", perziura), veiksmoMygtukas("Redaguoti", "redaguoti", u.id),
               veiksmoMygtukas("Ištrinti", "istrinti", u.id, "pavojingas")]
            : [nuorodosMygtukas("Peržiūrėti", perziura), nuorodosMygtukas("Dokumentai", nuoroda("dokumentai.html", { uzsakymas: u.id, augintinis: a.id }))];
        return `<tr>
            <td><a class="pagrindinis" href="${perziura}">${uzsakymoNr(u)}</a></td>
            <td>${esc(a.vardas)}<small>${esc(a.tipas)}</small></td>
            ${klientas ? "" : `<td>${esc(vardasPavarde(uzsakymoKlientas(u)))}</td>`}
            <td class="skaicius">${pinigai(u.kaina)}</td><td>${u.sukurimo_data}</td>
            <td>${zyma(busena("uzsakymas", u.busena))}</td><td>${u.patvirtinimo_data || "–"}</td>
            ${veiksmai(...mygtukai)}
        </tr>`;
    }, klientas ? "Užsakymų dar nėra. Sukurkite pirmąjį užsakymą." : "Užsakymų nėra.");

    kaiPaspausta(tbody, {
        // 2.13 pav.: redaguoti galima tik nepatvirtintą užsakymą
        redaguoti: id => {
            const u = rasti("uzsakymai", id);
            if (u.busena) return klaida(`Užsakymas ${uzsakymoNr(u)} negali būti redaguojamas, nes jis jau patvirtintas.`);
            location.href = nuoroda("uzsakymas-redaguoti.html", { uzsakymas: id });
        },
        // 2.14 pav.: ištrinti galima tik nepatvirtintą užsakymą
        istrinti: async id => {
            const u = rasti("uzsakymai", id);
            if (u.busena) return klaida(`Užsakymo ${uzsakymoNr(u)} ištrinti negalima, nes jis jau patvirtintas.`);
            const taip = await klausti({ tekstas: `Ar tikrai norite ištrinti užsakymą ${uzsakymoNr(u)}? Šio veiksmo atšaukti negalima.` });
            if (!taip) return;
            pasalinti("uzsakymai", id);
            eiti("uzsakymai.html", `Užsakymas ${uzsakymoNr(u)} ištrintas.`);
        }
    });
};

const augintinioParinktis = a => `${a.vardas} (${a.tipas.toLowerCase()}, ${a.veisle})`;

function rodytiKaina(select) {
    const a = rasti("augintiniai", select.value);
    $("#kaina").textContent = a ? pinigai(kainaPagalTipa(a.tipas)) : "–";
}

// 2.12 pav. PA „Sukurti užsakymą“, kuris «include» PA „Patikrinti kliento istoriją“
PUSLAPIAI["uzsakymas-kurti.html"] = () => {
    const k = naudotojas();
    const forma = $("#uzsakymo-forma");
    const perziura = $("#formos-perziura");
    const kliutis = $("#istorijos-reikalavimas");
    const sustabdyti = turinys => {
        forma.hidden = true;
        kliutis.hidden = false;
        kliutis.innerHTML = turinys;
    };
    const tikrinti = nuoroda("istorijos-patikrinimas.html", { grizti: "uzsakymas-kurti.html" });
    if (k.istorijos_busena === "Netinkamas") return sustabdyti(`<h2>Užsakymo sukurti negalima</h2>
        <p>Patikrinus teistumo istoriją per CourtListener API rasta įrašų, todėl klientas netinkamas.</p>
        <div class="mygtukai"><a class="mygtukas" href="${tikrinti}">Tikrinti iš naujo</a><a class="mygtukas antrinis" href="uzsakymai.html">Grįžti į užsakymų sąrašą</a></div>`);
    if (k.istorijos_busena !== "Patikrintas") return sustabdyti(`<h2>Reikia patikrinti kliento istoriją</h2>
        <p>Prieš kuriant užsakymą sistema turi patikrinti jūsų teistumo istoriją per CourtListener API.</p>
        <div class="mygtukai"><a class="mygtukas" href="${tikrinti}">Patikrinti istoriją</a><a class="mygtukas antrinis" href="uzsakymai.html">Atšaukti</a></div>`);

    // Gaunamas kliento augintinių sąrašas (augintinis turi ne daugiau kaip vieną užsakymą)
    const laisvi = klientoAugintiniai(k.id).filter(a => !augintinioUzsakymas(a.id));
    if (!laisvi.length) return sustabdyti(`<h2>Nėra augintinių be užsakymo</h2>
        <p>Kiekvienas augintinis gali turėti vieną užsakymą. Norėdami sukurti naują užsakymą, pirmiausia sukurkite augintinį.</p>
        <div class="mygtukai"><a class="mygtukas" href="augintinis-kurti.html">+ Sukurti gyvūną</a><a class="mygtukas antrinis" href="uzsakymai.html">Grįžti</a></div>`);

    const augintinis = forma.elements.augintinis;
    parinktys(augintinis, laisvi, augintinioParinktis, Number(param("augintinis")));
    augintinis.addEventListener("change", () => rodytiKaina(augintinis));
    rodytiKaina(augintinis);

    // Pateikiama užpildytos formos peržiūra
    kaiPateikta(forma, d => {
        const a = rasti("augintiniai", d.augintinis);
        uzpildyti(perziura, {
            zyma: html(zyma("Nepatvirtintas")), augintinis: a.vardas, tipas: a.tipas, veisle: a.veisle,
            kaina: pinigai(kainaPagalTipa(a.tipas)), sukurimo_data: siandien(), busena: "Nepatvirtintas"
        });
        $("#pranesimas").innerHTML = "";
        forma.hidden = true;
        perziura.hidden = false;
    });
    $("#grizti-redaguoti").addEventListener("click", () => {
        perziura.hidden = true;
        forma.hidden = false;
    });
    $("#pateikti").addEventListener("click", () => {
        // Sistema patikrina užsakymo duomenis
        const a = rasti("augintiniai", augintinis.value);
        if (!a || a.klientas !== k.id || augintinioUzsakymas(a.id)) {
            perziura.hidden = true;
            forma.hidden = false;
            return pazymetiLaukus([augintinis], "Užsakymo duomenys neteisingi: pasirinktas augintinis neegzistuoja arba jau turi užsakymą.");
        }
        const u = sukurti("uzsakymai", { augintinis: a.id, kaina: kainaPagalTipa(a.tipas), sukurimo_data: siandien(), busena: false, patvirtinimo_data: null });
        eiti("uzsakymai.html", `Užsakymas ${uzsakymoNr(u)} sukurtas. Būsena: „Nepatvirtintas“.`);
    });
};

// 2.11 pav. PA „Peržiūrėti užsakymą“
PUSLAPIAI["uzsakymas.html"] = () => {
    const u = pagalParametra("uzsakymas", pasiekiamiUzsakymai());
    if (!u) return nera("Užsakymas nerastas.", "uzsakymai.html", "Grįžti į sąrašą");
    const klientas = esuKlientas();
    const a = rasti("augintiniai", u.augintinis);
    const pavyzdys = augintinioDnr(a.id);
    const kl = uzsakymoKlonavimas(u.id);
    const inkubatorius = kl && rasti("inkubatoriai", kl.inkubatorius);
    uzpildyti($("main"), {
        nr: uzsakymoNr(u),
        busenos_zyma: html(zyma(busena("uzsakymas", u.busena))),
        klientas: vardasPavarde(uzsakymoKlientas(u)),
        augintinis: klientas ? nuorodaHtml(augintinioPavadinimas(a), nuoroda("augintinis.html", { augintinis: a.id })) : augintinioPavadinimas(a),
        kaina: pinigai(u.kaina), sukurimo_data: u.sukurimo_data, busena: busena("uzsakymas", u.busena),
        patvirtinimo_data: u.patvirtinimo_data,
        dnr_kodas: !pavyzdys ? "Pavyzdys dar nepaimtas"
            : klientas ? dnrKodas(pavyzdys) : nuorodaHtml(dnrKodas(pavyzdys), nuoroda("dnr-pavyzdys.html", { dnr: pavyzdys.id })),
        paemimo_data: pavyzdys?.paemimo_data,
        tinkamumas: pavyzdys ? html(zyma(pavyzdys.tinkamumas || "Neįvertintas")) : "–",
        klonavimo_busena: kl ? html(`${klonavimoNr(kl)} ${zyma(busena("klonavimas", kl.busena))}`) : "Nepradėtas",
        laikotarpis: kl ? laikotarpis(kl) : "–",
        inkubatorius: !inkubatorius ? "–"
            : klientas ? inkubatorius.modelis : nuorodaHtml(inkubatorius.modelis, nuoroda("inkubatorius.html", { inkubatorius: inkubatorius.id })),
        rezultatas: kl?.rezultatas
    });
    const pranesimas = uzsakymoPranesimas(u.id);
    if (pranesimas) {
        $("#pranesimo-kortele").hidden = false;
        uzpildyti($("#pranesimo-kortele"), {
            pranesimo_busena: html(zyma(busena("pranesimas", pranesimas.busena))),
            gavejas: uzsakymoKlientas(u).el_pastas, siuntimo_data: pranesimas.siuntimo_data, tekstas: pranesimas.tekstas
        });
    }
    $("#dokumentu-nuoroda").href = nuoroda("dokumentai.html", dokumentuParametrai(a));
    // Redaguoti galima tik kliento nepatvirtintą užsakymą
    $("#redaguoti").hidden = !klientas || u.busena;
    $("#redaguoti").href = nuoroda("uzsakymas-redaguoti.html", { uzsakymas: u.id });
};

// 2.13 pav. PA „Redaguoti užsakymą“
PUSLAPIAI["uzsakymas-redaguoti.html"] = () => {
    const k = naudotojas();
    const u = pagalParametra("uzsakymas", klientoUzsakymai(k.id));
    if (!u) return nera("Užsakymas nerastas.", "uzsakymai.html", "Grįžti į sąrašą");
    const forma = $("#uzsakymo-forma");
    $("#aprasas").textContent = `Redaguojamas užsakymas ${uzsakymoNr(u)}.`;
    if (u.busena) {
        forma.hidden = true;
        $("#negalima").hidden = false;
        return klaida(`Užsakymas ${uzsakymoNr(u)} negali būti redaguojamas, nes jis jau patvirtintas.`);
    }
    const galimi = klientoAugintiniai(k.id).filter(a => a.id === u.augintinis || !augintinioUzsakymas(a.id));
    const augintinis = forma.elements.augintinis;
    parinktys(augintinis, galimi, augintinioParinktis, u.augintinis);
    augintinis.addEventListener("change", () => rodytiKaina(augintinis));
    rodytiKaina(augintinis);

    kaiPateikta(forma, d => {
        // Patikrinami pakeisti duomenys; klaidingi laukai nurodomi redagavimo formoje
        const a = rasti("augintiniai", d.augintinis);
        if (!a || a.klientas !== k.id || (a.id !== u.augintinis && augintinioUzsakymas(a.id))) return pazymetiLaukus([augintinis]);
        atnaujinti("uzsakymai", u.id, { augintinis: a.id, kaina: kainaPagalTipa(a.tipas) });
        eiti("uzsakymai.html", `Užsakymas ${uzsakymoNr(u)} sėkmingai atnaujintas.`);
    });
};

// ---------- DNR pavyzdžio valdymas ----------

// Demonstracinis brūkšninis kodas (dekoratyvus) pagal pavyzdžio ID
function barkodas(tekstas) {
    let x = 0;
    const juostos = [];
    [...tekstas].forEach((simbolis, i) => {
        const kodas = simbolis.charCodeAt(0) * 7 + i * 13;
        for (let b = 0; b < 6; b++) {
            const plotis = 1 + ((kodas >> b) & 1) + ((kodas >> (b + 3)) & 1);
            if (b % 2 === 0) juostos.push(`<rect x="${x}" width="${plotis}" height="46"/>`);
            x += plotis;
        }
    });
    return `<svg viewBox="0 0 ${x} 46" preserveAspectRatio="none" fill="currentColor">${juostos.join("")}</svg>`;
}

// 2.16 pav. (etiketė), 2.18 pav. PA „Ištrinti DNR pavyzdį“, 3.1.3 pav. (tinkamumas inicijuojamas iš DNR pavyzdžių puslapio)
PUSLAPIAI["dnr.html"] = () => {
    const pavyzdziai = [...visi("dnr")].sort((a, b) => b.paemimo_data.localeCompare(a.paemimo_data) || b.id - a.id);
    const kiek = tinkamumas => pavyzdziai.filter(x => x.tinkamumas === tinkamumas).length;
    rodikliai($("#rodikliai"), [
        [pavyzdziai.length, "DNR pavyzdžiai"],
        [kiek(TINKAMAS), "Tinkami klonuoti", "sekme"],
        [kiek(NETINKAMAS), "Netinkami", "klaida"],
        [kiek(null), "Neįvertinti", "ispejimas"]
    ]);

    const tbody = $("#pavyzdziai");
    eilutes(tbody, pavyzdziai, x => {
        const a = rasti("augintiniai", x.augintinis);
        const p = { dnr: x.id };
        const yraRezultatai = DNR_REZULTATAI.every(l => x[l] != null);
        return `<tr>
            <td><a class="pagrindinis" href="${nuoroda("dnr-pavyzdys.html", p)}">${dnrKodas(x)}</a></td>
            <td>${esc(a?.vardas || "–")}<small>${a ? esc(`${a.tipas} · ${vardasPavarde(rasti("klientai", a.klientas))}`) : ""}</small></td>
            <td>${x.paemimo_data}<small>Laikoma ${suVienetais(x.saugojimo_temperatura, "°C")}</small></td>
            <td>${yraRezultatai ? zyma("Įvesti", "sekme") : zyma("Neįvesti", "ispejimas")}</td>
            <td>${x.tinkamumas ? `<a href="${nuoroda("dnr-tinkamumo-rezultatas.html", p)}">${zyma(x.tinkamumas)}</a>` : zyma("Neįvertintas")}</td>
            ${veiksmai(
                nuorodosMygtukas("Peržiūrėti", nuoroda("dnr-pavyzdys.html", p)),
                nuorodosMygtukas("Redaguoti", nuoroda("dnr-redaguoti.html", p)),
                yraRezultatai && veiksmoMygtukas("Nustatyti tinkamumą", "tinkamumas", x.id),
                veiksmoMygtukas("Ištrinti", "istrinti", x.id, "pavojingas"))}
        </tr>`;
    }, "DNR pavyzdžių dar nėra.");

    kaiPaspausta(tbody, {
        // 2.20 pav. PA „Nustatyti DNR pavyzdžio tinkamumą“
        tinkamumas: id => {
            const x = rasti("dnr", id);
            const analize = nustatytiTinkamuma(id);
            if (analize.truksta) return klaida(`Pavyzdžio ${dnrKodas(x)} tinkamumo nustatyti negalima, nes neįvesti analizės rezultatai: ${analize.truksta.join(", ")}.`);
            const tinka = analize.statusas === TINKAMAS;
            eiti(nuoroda("dnr-tinkamumo-rezultatas.html", { dnr: id }), tinka
                ? `Analizė atlikta: pavyzdys ${dnrKodas(x)} tinkamas klonuoti. Sugeneruotas sėkmingos analizės sertifikatas.`
                : `Analizė atlikta: pavyzdys ${dnrKodas(x)} netinkamas, neatitikimų: ${analize.neatitikimai.length}.`, tinka ? "sekme" : "klaida");
        },
        istrinti: async id => {
            const x = rasti("dnr", id);
            const taip = await klausti({ tekstas: `Ar tikrai norite ištrinti DNR pavyzdį ${dnrKodas(x)}? Šio veiksmo atšaukti negalima.` });
            if (!taip) return;
            istrintiDnr(id);
            eiti("dnr.html", `DNR pavyzdys ${dnrKodas(x)} ištrintas.`);
        }
    });

    // Pasirinkus spausdinti etiketę – sugeneruota pavyzdžio etiketė
    const etiketesPavyzdys = param("etikete") && rasti("dnr", param("etikete"));
    if (etiketesPavyzdys) {
        const kortele = $("#etikete");
        kortele.hidden = false;
        uzpildyti(kortele, {
            kodas: dnrKodas(etiketesPavyzdys), barkodas: html(barkodas(dnrKodas(etiketesPavyzdys))),
            augintinis: augintinioPavadinimas(rasti("augintiniai", etiketesPavyzdys.augintinis)),
            paemimo_data: etiketesPavyzdys.paemimo_data, temperatura: suVienetais(etiketesPavyzdys.saugojimo_temperatura, "°C")
        });
        $("#spausdinti-etikete").addEventListener("click", () => spausdinti(kortele));
        $("#uzdaryti-etikete").addEventListener("click", () => {
            kortele.hidden = true;
            history.replaceState(null, "", location.pathname);
        });
    }
};

const augintinioSuKlientu = a => `${a.vardas} (${a.tipas.toLowerCase()}), ${vardasPavarde(rasti("klientai", a.klientas))}`;

// 2.16 pav. PA „Sukurti DNR pavyzdį“
PUSLAPIAI["dnr-kurti.html"] = () => {
    const forma = $("#dnr-forma");
    const l = forma.elements;
    const be = visi("augintiniai").filter(a => !augintinioDnr(a.id));
    parinktys(l.augintinis, be, augintinioSuKlientu);
    if (!be.length) l.augintinis.insertAdjacentHTML("afterend", '<p class="pagalba">Visi augintiniai jau turi DNR pavyzdį.</p>');
    l.paemimo_data.max = siandien();
    l.paemimo_data.value = siandien();
    kaiPateikta(forma, d => {
        const x = sukurti("dnr", {
            augintinis: Number(d.augintinis), paemimo_data: d.paemimo_data, saugojimo_temperatura: d.saugojimo_temperatura,
            ...Object.fromEntries(DNR_REZULTATAI.map(r => [r, null])), tinkamumas: null, tinkamumo_data: null
        });
        eiti(nuoroda("dnr.html", { etikete: d.spausdinti_etikete ? x.id : null }), `DNR pavyzdys ${dnrKodas(x)} sukurtas.`);
    });
};

// 2.17 pav. PA „Peržiūrėti DNR pavyzdį“, 2.18 pav. PA „Ištrinti DNR pavyzdį“
PUSLAPIAI["dnr-pavyzdys.html"] = () => {
    const x = pagalParametra("dnr", visi("dnr"));
    if (!x) return nera("DNR pavyzdys nerastas.", "dnr.html", "Grįžti į sąrašą");
    const a = rasti("augintiniai", x.augintinis);
    const u = a && augintinioUzsakymas(a.id);
    const kl = dnrKlonavimas(x.id);
    const inkubatorius = kl && rasti("inkubatoriai", kl.inkubatorius);
    const verte = laukas => suVienetais(x[laukas], DNR_NORMOS.find(n => n.laukas === laukas).vienetai);
    uzpildyti($("main"), {
        kodas: dnrKodas(x), tinkamumo_zyma: html(zyma(x.tinkamumas || "Neįvertintas")),
        augintinis: augintinioPavadinimas(a), klientas: vardasPavarde(a && rasti("klientai", a.klientas)),
        paemimo_data: x.paemimo_data,
        ...Object.fromEntries(DNR_NORMOS.map(n => [n.laukas, verte(n.laukas)])),
        tinkamumas: x.tinkamumas ? `${x.tinkamumas} (${x.tinkamumo_data})` : "Neįvertintas",
        uzsakymas: u ? html(`<a href="${nuoroda("uzsakymas.html", { uzsakymas: u.id })}">${uzsakymoNr(u)}</a> ${zyma(busena("uzsakymas", u.busena))}`) : "Užsakymo nėra",
        klonavimas: kl ? html(`${klonavimoNr(kl)} ${zyma(busena("klonavimas", kl.busena))}`) : "Nepradėtas",
        inkubatorius: inkubatorius ? nuorodaHtml(inkubatorius.modelis, nuoroda("inkubatorius.html", { inkubatorius: inkubatorius.id }))
            : laukiaInkubatoriaus().includes(x) ? "Laukia priskyrimo" : "–"
    });
    $("#redaguoti").href = nuoroda("dnr-redaguoti.html", { dnr: x.id });
    $("#ataskaita").hidden = !x.tinkamumas;
    $("#ataskaita").href = nuoroda("dnr-tinkamumo-rezultatas.html", { dnr: x.id });
    $("#istrinti").addEventListener("click", async () => {
        const taip = await klausti({ tekstas: `Ar tikrai norite ištrinti DNR pavyzdį ${dnrKodas(x)}? Šio veiksmo atšaukti negalima.` });
        // Nepatvirtinus ištrynimo grąžinama į DNR pavyzdžių puslapį
        if (!taip) return eiti("dnr.html");
        istrintiDnr(x.id);
        eiti("dnr.html", `DNR pavyzdys ${dnrKodas(x)} ištrintas.`);
    });
};

// Analizės rezultatų laukai su normomis
function rezultatuLaukai(vieta) {
    vieta.innerHTML = DNR_NORMOS.filter(n => DNR_REZULTATAI.includes(n.laukas)).map(n => `
        <div class="laukas">
            <label for="${n.laukas}">${n.pavadinimas}${n.vienetai ? ` (${n.vienetai})` : ""}</label>
            <input id="${n.laukas}" name="${n.laukas}" type="number" step="0.01" min="0"${n.vienetai === "%" ? ' max="100"' : ""}>
            <p class="pagalba">Norma: ${normosTekstas(n)}</p>
        </div>`).join("");
}

// 2.19 pav. PA „Redaguoti DNR pavyzdį“
PUSLAPIAI["dnr-redaguoti.html"] = () => {
    const x = pagalParametra("dnr", visi("dnr"));
    if (!x) return nera("DNR pavyzdys nerastas.", "dnr.html", "Grįžti į sąrašą");
    $("#aprasas").textContent = `Redaguojamas pavyzdys ${dnrKodas(x)}. Čia įvedami ir analizės rezultatai; juos ar saugojimo temperatūrą pakeitus, tinkamumą reikės nustatyti iš naujo.`;
    const forma = $("#dnr-forma");
    const l = forma.elements;
    rezultatuLaukai($("#rezultatu-laukai"));
    parinktys(l.augintinis, visi("augintiniai").filter(a => a.id === x.augintinis || !augintinioDnr(a.id)), augintinioSuKlientu, x.augintinis);
    // Klonavime panaudoto pavyzdžio augintinio keisti negalima
    if (dnrKlonavimas(x.id)) l.augintinis.disabled = true;
    l.paemimo_data.max = siandien();
    uzpildytiForma(forma, x);
    kaiPateikta(forma, d => {
        d.augintinis = d.augintinis ? Number(d.augintinis) : x.augintinis;
        const pasikeite = ["saugojimo_temperatura", ...DNR_REZULTATAI].some(r => d[r] !== x[r]);
        atnaujinti("dnr", x.id, Object.assign(d, pasikeite ? { tinkamumas: null, tinkamumo_data: null } : {}));
        eiti("dnr.html", `DNR pavyzdžio ${dnrKodas(x)} duomenys atnaujinti.${pasikeite && x.tinkamumas ? " Tinkamumą reikia nustatyti iš naujo." : ""}`);
    });
};

// 2.20 pav.: sertifikatas arba atmetimo priežasčių suvestinė su detalia ataskaita
PUSLAPIAI["dnr-tinkamumo-rezultatas.html"] = () => {
    const x = param("dnr") ? rasti("dnr", param("dnr")) : visi("dnr").find(d => d.tinkamumas);
    if (!x) return nera("DNR pavyzdys nerastas.", "dnr.html", "Grįžti į sąrašą");
    const a = rasti("augintiniai", x.augintinis);
    const analize = tinkamumoAnalize(x);
    uzpildyti($("main"), {
        kodas: dnrKodas(x), statusas: html(zyma(x.tinkamumas || "Neįvertintas")), augintinis: augintinioPavadinimas(a),
        paemimo_data: x.paemimo_data, tinkamumo_data: x.tinkamumo_data, tinkamumas: x.tinkamumas || "Neįvertintas",
        neatitikimai: x.tinkamumas && !analize.truksta ? String(analize.neatitikimai.length) : "–"
    });
    if (!x.tinkamumas || analize.truksta) {
        ["#patikrinimai", "#isvada", "#spausdinti"].forEach(s => { $(s).hidden = true; });
        return pranesti(`Pavyzdžio ${dnrKodas(x)} tinkamumas dar nenustatytas. Analizę inicijuokite DNR pavyzdžių puslapyje.`, "info");
    }

    eilutes($("#patikrinimai tbody"), analize.patikrinimai, p => `<tr>
        <td>${esc(p.pavadinimas)}</td><td class="skaicius">${suVienetais(p.verte, p.vienetai)}</td>
        <td>${normosTekstas(p)}</td><td>${zyma(p.atitinka ? "Atitinka" : "Neatitinka")}</td></tr>`, "");

    const tinka = x.tinkamumas === TINKAMAS;
    const specialistas = naudotojas();
    $("#isvada").innerHTML = tinka ? `
        <div class="sertifikatas">
            <p class="antrinis-tekstas">Sėkmingos analizės sertifikatas Nr. SERT-${String(x.id).padStart(4, "0")}</p>
            <h3>DNR pavyzdys tinkamas klonuoti</h3>
            <p>Pavyzdžio <strong>${dnrKodas(x)}</strong> (${esc(augintinioPavadinimas(a))}) visi ${analize.patikrinimai.length} biologiniai parametrai atitinka nustatytas normas.</p>
            <p>Analizės data: ${x.tinkamumo_data}</p>
            <div class="parasas"><span>Laboratorijos specialistas<br>${esc(vardasPavarde(specialistas))}</span><span>Parašas</span></div>
        </div>` : `
        <h2>Detali atmetimo ataskaita</h2>
        <p>Pavyzdžiui priskirtas statusas „${NETINKAMAS}“, nes ${analize.neatitikimai.length} iš ${analize.patikrinimai.length} parametrų neatitinka normų.</p>
        <h3 style="font-size: 15px; margin: 16px 0 0">Atmetimo priežasčių suvestinė</h3>
        <ul class="priezastys">${analize.neatitikimai.map(p =>
            `<li><strong>${esc(p.pavadinimas)}</strong>: ${suVienetais(p.verte, p.vienetai)} (norma ${normosTekstas(p)})</li>`).join("")}</ul>
        <p class="pagalba">Rekomendacija: paimti naują DNR pavyzdį ir pakartoti analizę.</p>`;
    $("#spausdinti").textContent = tinka ? "Spausdinti sertifikatą" : "Spausdinti atmetimo ataskaitą";
    $("#spausdinti").addEventListener("click", () => spausdinti());
};

// ---------- Inkubatorių valdymas ----------

const PRISKYRIMO_SAUGYKLA = "gks-priskyrimas";
function issaugotiPriskyrima(rezultatas) {
    try { localStorage.setItem(PRISKYRIMO_SAUGYKLA, JSON.stringify(rezultatas)); } catch (e) { /* prototipas veikia ir be to */ }
}
function gautiPriskyrima() {
    try { return JSON.parse(localStorage.getItem(PRISKYRIMO_SAUGYKLA)); } catch (e) { return null; }
}

PUSLAPIAI["inkubatoriai.html"] = () => {
    const inkubatoriai = visi("inkubatoriai");
    const laukia = laukiaInkubatoriaus();
    rodikliai($("#rodikliai"), [
        [inkubatoriai.length, "Inkubatoriai"],
        [inkubatoriai.filter(i => !i.busena).length, "Laisvi", "sekme"],
        [inkubatoriai.filter(i => i.busena).length, "Užimti", "info"],
        [laukia.length, "DNR pavyzdžiai laukia inkubatoriaus", "ispejimas"]
    ]);

    // PA „Priskirti inkubatorių“: pasirenkamas DNR pavyzdys, kuriam priskirti inkubatorių
    const forma = $("#priskyrimo-forma");
    parinktys(forma.elements.dnr, laukia, x => {
        const a = rasti("augintiniai", x.augintinis);
        return `${dnrKodas(x)}: ${a.vardas} (${a.tipas.toLowerCase()})`;
    }, Number(param("dnr")));
    $("#priskyrimo-pastaba").textContent = laukia.length
        ? "Rodomi tinkami klonuoti pavyzdžiai su patvirtintu užsakymu, kuriems dar nepriskirtas inkubatorius."
        : "Šiuo metu nėra DNR pavyzdžių, laukiančių inkubatoriaus.";
    kaiPateikta(forma, d => {
        const x = rasti("dnr", d.dnr);
        const rezultatas = priskirtiInkubatoriu(x.id, naudotojas().id);
        issaugotiPriskyrima(rezultatas);
        const parinktas = rezultatas.parinktas && rasti("inkubatoriai", rezultatas.parinktas);
        eiti(nuoroda("inkubatoriaus-priskyrimo-rezultatas.html", { dnr: x.id }), parinktas
            ? `Pavyzdžiui ${dnrKodas(x)} priskirtas inkubatorius ${parinktas.modelis} (${parinktas.serijinis_numeris}).`
            : `Pavyzdžiui ${dnrKodas(x)} tinkamų laisvų inkubatorių nerasta.`, parinktas ? "sekme" : "klaida");
    });

    const tbody = $("#inkubatoriai");
    eilutes(tbody, inkubatoriai, i => {
        const kl = inkubatoriausKlonavimas(i.id);
        const a = kl && klonavimoAugintinis(kl);
        const p = { inkubatorius: i.id };
        return `<tr>
            <td><a class="pagrindinis" href="${nuoroda("inkubatorius.html", p)}">${esc(i.modelis)}</a><small>${esc(i.serijinis_numeris)}</small></td>
            <td class="skaicius">${suVienetais(i.tiksline_temperatura, "°C", 1)}</td>
            <td class="skaicius">${suVienetais(i.co2_procentas, "%", 1)}</td>
            <td class="skaicius">${suVienetais(i.o2_procentas, "%", 1)}</td>
            <td class="skaicius">${skaicius(i.ph_lygis, 2)}</td>
            <td class="skaicius">${suVienetais(i.talpa, "l")}</td>
            <td>${zyma(busena("inkubatorius", i.busena))}${a ? `<small>${esc(a.vardas)} · ${klonavimoNr(kl)}</small>` : ""}</td>
            ${veiksmai(
                nuorodosMygtukas("Parametrai", nuoroda("inkubatorius.html", p)),
                nuorodosMygtukas("Redaguoti būseną", nuoroda("inkubatorius-busena.html", p)),
                veiksmoMygtukas("Ištrinti", "istrinti", i.id, "pavojingas"))}
        </tr>`;
    }, "Inkubatorių dar nėra.");

    kaiPaspausta(tbody, {
        // PA „Ištrinti inkubatorių“: tikrinama būsena – užimto inkubatoriaus ištrinti negalima
        istrinti: id => {
            const i = rasti("inkubatoriai", id);
            if (i.busena) return klaida(`Inkubatorius ${i.modelis} (${i.serijinis_numeris}) užimtas, todėl jo ištrinti negalima.`);
            istrintiInkubatoriu(id);
            eiti("inkubatoriai.html", `Inkubatorius ${i.modelis} (${i.serijinis_numeris}) ištrintas.`);
        }
    });
};

// PA „Sukurti inkubatorių“
PUSLAPIAI["inkubatorius-kurti.html"] = () => {
    const forma = $("#inkubatoriaus-forma");
    kaiPateikta(forma, d => {
        if (visi("inkubatoriai").some(i => i.serijinis_numeris.toLowerCase() === d.serijinis_numeris.toLowerCase()))
            return pazymetiLaukus([forma.elements.serijinis_numeris], "Inkubatorius su tokiu serijiniu numeriu jau užregistruotas.");
        const i = sukurti("inkubatoriai", { ...d, busena: false });
        eiti("inkubatoriai.html", `Inkubatorius ${i.modelis} sukurtas.`);
    });
};

// PA „Peržiūrėti inkubatoriaus parametrus“
PUSLAPIAI["inkubatorius.html"] = () => {
    const i = pagalParametra("inkubatorius", visi("inkubatoriai"));
    if (!i) return nera("Inkubatorius nerastas.", "inkubatoriai.html", "Grįžti į sąrašą");
    const kl = inkubatoriausKlonavimas(i.id);
    const pavyzdys = kl && rasti("dnr", kl.dnr);
    uzpildyti($("main"), {
        modelis: i.modelis, serijinis_numeris: i.serijinis_numeris,
        busenos_zyma: html(zyma(busena("inkubatorius", i.busena))), busena: busena("inkubatorius", i.busena),
        tiksline_temperatura: suVienetais(i.tiksline_temperatura, "°C", 1), talpa: suVienetais(i.talpa, "l"),
        co2_procentas: suVienetais(i.co2_procentas, "%", 1), o2_procentas: suVienetais(i.o2_procentas, "%", 1),
        ph_lygis: skaicius(i.ph_lygis, 2),
        klonavimas: kl ? klonavimoNr(kl) : "Inkubatoriuje klonavimas nevykdomas",
        augintinis: kl ? augintinioPavadinimas(klonavimoAugintinis(kl)) : "–",
        dnr: pavyzdys ? nuorodaHtml(dnrKodas(pavyzdys), nuoroda("dnr-pavyzdys.html", { dnr: pavyzdys.id })) : "–",
        specialistas: kl ? vardasPavarde(rasti("darbuotojai", kl.specialistas)) : "–",
        laikotarpis: kl ? laikotarpis(kl) : "–"
    });
    $("#redaguoti").href = nuoroda("inkubatorius-busena.html", { inkubatorius: i.id });
    eilutes($("#matavimai"), inkubatoriausMatavimai(i.id), m => {
        const nuokrypis = Math.round((m.temperatura - i.tiksline_temperatura) * 10) / 10;
        const zenklas = nuokrypis > 0 ? "+" : nuokrypis < 0 ? "−" : "±";
        const tinka = Math.abs(nuokrypis) <= LEIDZIAMI_NUOKRYPIAI.temperatura;
        return `<tr><td>${m.matavimo_data}</td><td class="skaicius">${suVienetais(m.temperatura, "°C", 1)}</td>
            <td class="skaicius">${zyma(`${zenklas}${skaicius(Math.abs(nuokrypis), 1)} °C`, tinka ? "sekme" : "klaida")}</td>
            <td class="skaicius">${suVienetais(m.dregme, "%", 1)}</td></tr>`;
    }, "Matavimų dar nėra.");
};

// PA „Redaguoti inkubatoriaus būseną“
PUSLAPIAI["inkubatorius-busena.html"] = () => {
    const i = pagalParametra("inkubatorius", visi("inkubatoriai"));
    if (!i) return nera("Inkubatorius nerastas.", "inkubatoriai.html", "Grįžti į sąrašą");
    $("#aprasas").textContent = `${i.modelis} (${i.serijinis_numeris})`;
    const forma = $("#inkubatoriaus-forma");
    uzpildytiForma(forma, i);
    kaiPateikta(forma, d => {
        const kl = inkubatoriausKlonavimas(i.id);
        if (!d.busena && kl)
            return pazymetiLaukus([forma.elements.busena], `Inkubatoriuje vykdomas klonavimas ${klonavimoNr(kl)}, todėl būsenos „Laisvas“ nustatyti negalima.`);
        atnaujinti("inkubatoriai", i.id, d);
        eiti("inkubatoriai.html", `Inkubatoriaus ${i.modelis} duomenys atnaujinti.`);
    });
};

// PA „Priskirti inkubatorių“: patikrinimo ciklo ir priskyrimo rezultatas
PUSLAPIAI["inkubatoriaus-priskyrimo-rezultatas.html"] = () => {
    const r = gautiPriskyrima();
    const x = r && rasti("dnr", r.dnr);
    if (!x || (param("dnr") && Number(param("dnr")) !== r.dnr))
        return nera("Priskyrimas dar neatliktas. Pasirinkite DNR pavyzdį inkubatorių lange.", "inkubatoriai.html", "Į inkubatorių langą");
    const a = rasti("augintiniai", x.augintinis);
    const u = augintinioUzsakymas(a.id);
    const s = r.salygos;
    const pliusMinus = (verte, nuokrypis, vienetai) => `${skaicius(verte, 1)} ± ${skaicius(nuokrypis, 1)} ${vienetai}`;
    const reikia = {
        temperatura: pliusMinus(s.temperatura, LEIDZIAMI_NUOKRYPIAI.temperatura, "°C"),
        co2: pliusMinus(s.co2, LEIDZIAMI_NUOKRYPIAI.co2, "%"),
        o2: pliusMinus(s.o2, LEIDZIAMI_NUOKRYPIAI.o2, "%"),
        ph: `${skaicius(s.ph[0], 1)}–${skaicius(s.ph[1], 1)}`
    };
    $("#aprasas").textContent = `Priskyrimas atliktas ${r.data}.`;
    uzpildyti($("#priskyrimo-duomenys"), { kodas: dnrKodas(x), augintinis: a.vardas, tipas: a.tipas, uzsakymas: u ? uzsakymoNr(u) : "–", ...reikia });

    const zymeti = (tekstas, blogai) => blogai ? `<span class="blogai">${tekstas}</span>` : tekstas;
    eilutes($("#patikrinimai tbody"), r.patikrinimai, p => `<tr${p.id === r.parinktas ? ' class="parinktas"' : ""}>
        <td><strong>${esc(p.modelis)}</strong><small>${esc(p.serijinis_numeris)}</small></td>
        <td>${zyma(busena("inkubatorius", p.busena))}</td>
        <td class="skaicius">${zymeti(suVienetais(p.tiksline_temperatura, "°C", 1), p.priezastys.includes("temperatūra"))}</td>
        <td class="skaicius">${zymeti(suVienetais(p.co2_procentas, "%", 1), p.priezastys.includes("CO₂"))}</td>
        <td class="skaicius">${zymeti(suVienetais(p.o2_procentas, "%", 1), p.priezastys.includes("O₂"))}</td>
        <td class="skaicius">${zymeti(skaicius(p.ph_lygis, 2), p.priezastys.includes("pH"))}</td>
        <td class="tekstas">${p.id === r.parinktas ? zyma("Priskirtas", "sekme") : p.tinka ? zyma("Tinka") : zyma(`Netinka: ${p.priezastys.join(", ")}`, "klaida")}</td>
    </tr>`, "Inkubatorių nėra.");

    const kortele = $("#rezultatas");
    const tinkami = r.patikrinimai.filter(p => p.tinka).length;
    if (r.parinktas) {
        const p = r.patikrinimai.find(x => x.id === r.parinktas);
        const kl = rasti("klonavimai", r.klonavimas);
        kortele.classList.add("rezultatas-sekme");
        kortele.innerHTML = `<div class="korteles-virsus"><h2>Inkubatorius priskirtas</h2>${zyma("Užimtas")}</div>
            <p>Iš ${tinkami} tinkamų laisvų inkubatorių parinktas tinkamiausias: <strong>${esc(p.modelis)}</strong> (${esc(p.serijinis_numeris)}).
            Inkubatoriaus būsena pakeista į „Užimtas“${kl ? `, pradėtas klonavimas ${klonavimoNr(kl)} (${laikotarpis(kl)})` : ""}.</p>
            ${rasti("inkubatoriai", p.id) ? `<div class="mygtukai"><a class="mygtukas antrinis" href="${nuoroda("inkubatorius.html", { inkubatorius: p.id })}">Inkubatoriaus parametrai</a></div>` : ""}`;
    } else {
        // Klaidos pranešimas su reikiamais inkubatoriaus nustatymais
        const artimiausias = r.patikrinimai.filter(p => !p.busena && rasti("inkubatoriai", p.id)).sort((a, b) => a.ivertis - b.ivertis)[0];
        kortele.classList.add("rezultatas-klaida");
        kortele.innerHTML = `<div class="korteles-virsus"><h2>Tinkamų inkubatorių nėra</h2>${zyma("Netinka")}</div>
            <p>Nė vienas laisvas inkubatorius neatitinka embriono vystymosi poreikių. Reikalingi inkubatoriaus nustatymai:
            <strong>temperatūra ${reikia.temperatura}, CO₂ ${reikia.co2}, O₂ ${reikia.o2}, pH ${reikia.ph}</strong>.</p>
            <p class="pagalba">Paredaguokite laisvo inkubatoriaus parametrus ir pakartokite priskyrimą.</p>
            <div class="mygtukai" style="margin-top: 12px">
                ${artimiausias ? `<a class="mygtukas" href="${nuoroda("inkubatorius-busena.html", { inkubatorius: artimiausias.id })}">Redaguoti ${esc(artimiausias.modelis)}</a>` : ""}
                <a class="mygtukas antrinis" href="${nuoroda("inkubatoriai.html", { dnr: x.id })}">Bandyti dar kartą</a>
            </div>`;
    }
};

// ---------- Darbuotojų valdymas ----------

// 2.22 pav. – 2.24 pav.
PUSLAPIAI["darbuotojai.html"] = () => {
    const darbuotojai = visi("darbuotojai");
    rodikliai($("#rodikliai"), [
        [darbuotojai.length, "Darbuotojai"],
        [specialistai().length, "Laboratorijos specialistai", "info"],
        [darbuotojai.filter(d => d.role === "administratorius").length, "Administratoriai"],
        [visi("klonavimai").filter(k => k.busena).length, "Aktyvūs klonavimai", "sekme"]
    ]);
    const tbody = $("#darbuotojai");
    eilutes(tbody, darbuotojai, d => {
        const specialistas = d.role === "specialistas";
        const p = { darbuotojas: d.id };
        const mygtukai = [nuorodosMygtukas("Peržiūrėti", nuoroda("darbuotojas.html", p)), nuorodosMygtukas("Redaguoti", nuoroda("darbuotojas-redaguoti.html", p))];
        if (specialistas) mygtukai.push(nuorodosMygtukas("Tyrimai", nuoroda("darbuotojo-tyrimai.html", p)));
        mygtukai.push(veiksmoMygtukas("Ištrinti", "istrinti", d.id, "pavojingas"));
        return `<tr>
            <td><a class="pagrindinis" href="${nuoroda("darbuotojas.html", p)}">${esc(vardasPavarde(d))}</a>${specialistas ? `<small>Licencija ${esc(d.licencijos_numeris)}</small>` : ""}</td>
            <td>${esc(d.el_pastas)}</td><td>${esc(d.telefono_numeris)}</td><td class="skaicius">${skaicius(d.etatas)}</td>
            <td>${zyma(ROLES[d.role])}</td><td>${specialistas ? zyma(busena("specialistas", d.busena)) : "–"}</td>
            ${veiksmai(...mygtukai)}
        </tr>`;
    }, "Darbuotojų nėra.");

    kaiPaspausta(tbody, {
        // 2.24 pav.: atšaukus ištrynimą pranešama, kad darbuotojas nebuvo ištrintas
        istrinti: async id => {
            const d = rasti("darbuotojai", id);
            if (d === naudotojas()) return klaida("Negalite ištrinti savo paskyros.");
            const taip = await klausti({ tekstas: `Ar tikrai norite ištrinti darbuotoją ${vardasPavarde(d)}? Šio veiksmo atšaukti negalima.` });
            if (!taip) return pranesti(`Ištrynimas atšauktas. Darbuotojas ${vardasPavarde(d)} nebuvo ištrintas.`, "info");
            istrintiDarbuotoja(id);
            eiti("darbuotojai.html", `Darbuotojas ${vardasPavarde(d)} sėkmingai ištrintas.`);
        }
    });
};

// 2.21 pav. PA „Sukurti darbuotoją“ ir 2.23 pav. PA „Redaguoti darbuotoją“
function darbuotojoForma(esamas) {
    const forma = $("#darbuotojo-forma");
    const l = forma.elements;
    const specialistoLaukai = $("#specialisto-laukai");
    if (esamas) {
        uzpildytiForma(forma, esamas);
        if (l.busena.selectedIndex < 0) l.busena.selectedIndex = 0;
    }
    // Licencija ir būsena – tik laboratorijos specialistui
    const perjungti = () => {
        const specialistas = l.role.value === "specialistas";
        specialistoLaukai.hidden = !specialistas;
        specialistoLaukai.querySelectorAll("input, select").forEach(x => { x.disabled = !specialistas; });
    };
    l.role.addEventListener("change", perjungti);
    perjungti();

    kaiPateikta(forma, d => {
        if (elPastasUzimtas(d.el_pastas, esamas)) return pazymetiLaukus([l.el_pastas], "Toks el. paštas jau naudojamas kitos paskyros.");
        if (esamas?.role === "specialistas" && d.role !== "specialistas" && aktyvusKlonavimai(esamas.id).length)
            return pazymetiLaukus([l.role], "Specialistas vykdo aktyvius klonavimus, todėl jo rolės pakeisti negalima.");
        if (esamas && esamas === naudotojas() && d.role !== "administratorius")
            return pazymetiLaukus([l.role], "Negalite atimti administratoriaus teisių iš savo paskyros.");
        const specialistas = d.role === "specialistas";
        const laukai = { ...d, licencijos_numeris: specialistas ? d.licencijos_numeris : null, busena: specialistas ? d.busena : null };
        if (esamas) {
            atnaujinti("darbuotojai", esamas.id, laukai);
            eiti("darbuotojai.html", `Darbuotojo ${vardasPavarde(laukai)} duomenys sėkmingai atnaujinti.`);
        } else {
            sukurti("darbuotojai", { ...laukai, slaptazodis: DEMO_SLAPTAZODIS });
            eiti("darbuotojai.html", `Darbuotojas ${vardasPavarde(laukai)} sėkmingai sukurtas.`);
        }
    });
}

PUSLAPIAI["darbuotojas-kurti.html"] = () => darbuotojoForma(null);

PUSLAPIAI["darbuotojas-redaguoti.html"] = () => {
    const d = pagalParametra("darbuotojas", visi("darbuotojai"));
    if (!d) return nera("Darbuotojas nerastas.", "darbuotojai.html", "Grįžti į sąrašą");
    $("#aprasas").textContent = `Redaguojami darbuotojo ${vardasPavarde(d)} duomenys.`;
    darbuotojoForma(d);
};

// 2.22 pav. PA „Peržiūrėti darbuotoją“
PUSLAPIAI["darbuotojas.html"] = () => {
    const d = pagalParametra("darbuotojas", visi("darbuotojai"));
    if (!d) return nera("Darbuotojas nerastas.", "darbuotojai.html", "Grįžti į sąrašą");
    const specialistas = d.role === "specialistas";
    const kruvis = specialistas && specialistoKruvis(d);
    uzpildyti($("main"), {
        vardas_pavarde: vardasPavarde(d), roles_zyma: html(zyma(ROLES[d.role])),
        vardas: d.vardas, pavarde: d.pavarde, telefono_numeris: d.telefono_numeris, el_pastas: d.el_pastas,
        etatas: skaicius(d.etatas), role: ROLES[d.role], licencijos_numeris: d.licencijos_numeris,
        busena: specialistas ? html(zyma(busena("specialistas", d.busena))) : "–",
        uzduociu_kiekis: specialistas ? String(kruvis.kiekis) : "–",
        kruvis: specialistas ? html(`${zyma(kruvis.lygis)} ${skaicius(kruvis.kruvis, 1)} %`) : "–"
    });
    $("#redaguoti").href = nuoroda("darbuotojas-redaguoti.html", { darbuotojas: d.id });
    $("#specialisto-duomenys").hidden = !specialistas;
    $("#tyrimu-nuoroda").hidden = !specialistas;
    $("#tyrimu-nuoroda").href = nuoroda("darbuotojo-tyrimai.html", { darbuotojas: d.id });
};

// 2.25 pav. PA „Peržiūrėti darbuotojo tyrimus“
PUSLAPIAI["darbuotojo-tyrimai.html"] = () => {
    const d = pagalParametra("darbuotojas", param("darbuotojas") ? visi("darbuotojai") : specialistai());
    if (!d) return nera("Darbuotojas nerastas.", "darbuotojai.html", "Grįžti į sąrašą");
    $("#aprasas").textContent = `${vardasPavarde(d)}, ${ROLES[d.role].toLowerCase()}.`;
    const pasirinkimas = $("#specialisto-pasirinkimas");
    parinktys(pasirinkimas, d.role === "specialistas" ? specialistai() : [d, ...specialistai()], vardasPavarde, d.id);
    pasirinkimas.addEventListener("change", () => { location.href = nuoroda("darbuotojo-tyrimai.html", { darbuotojas: pasirinkimas.value }); });

    const tyrimai = specialistoKlonavimai(d.id).sort((a, b) => b.pradzios_data.localeCompare(a.pradzios_data));
    eilutes($("#tyrimai"), tyrimai, k => {
        const a = klonavimoAugintinis(k);
        const u = rasti("uzsakymai", k.uzsakymas);
        const href = nuoroda("tyrimas.html", { darbuotojas: d.id, tyrimas: k.id });
        return `<tr>
            <td><a class="pagrindinis" href="${href}">${klonavimoNr(k)}</a></td>
            <td>${esc(a?.vardas || "–")}<small>${esc(a?.tipas || "")}</small></td>
            <td>${u ? uzsakymoNr(u) : "–"}</td><td>${k.pradzios_data}</td><td>${k.pabaigos_data}</td>
            <td class="tekstas">${esc(k.rezultatas)}</td><td>${zyma(busena("klonavimas", k.busena))}</td>
            ${veiksmai(nuorodosMygtukas("Peržiūrėti", href))}
        </tr>`;
    }, d.role === "specialistas" ? "Šis darbuotojas tyrimų neturi." : "Administratorius tyrimų neatlieka.");
};

PUSLAPIAI["tyrimas.html"] = () => {
    const galimi = param("darbuotojas") ? specialistoKlonavimai(Number(param("darbuotojas"))) : visi("klonavimai");
    const k = pagalParametra("tyrimas", galimi);
    if (!k) return nera("Tyrimas nerastas.", "darbuotojai.html", "Grįžti į darbuotojų sąrašą");
    const u = rasti("uzsakymai", k.uzsakymas);
    const pavyzdys = rasti("dnr", k.dnr);
    const inkubatorius = rasti("inkubatoriai", k.inkubatorius);
    uzpildyti($("main"), {
        nr: klonavimoNr(k), busenos_zyma: html(zyma(busena("klonavimas", k.busena))),
        specialistas: vardasPavarde(rasti("darbuotojai", k.specialistas)),
        pradzios_data: k.pradzios_data, pabaigos_data: k.pabaigos_data,
        trukme: `${dienuSkirtumas(k.pradzios_data, k.pabaigos_data)} d.`, rezultatas: k.rezultatas,
        specialisto_patvirtinimas: html(zyma(k.specialisto_patvirtinimas ? "Taip" : "Ne")),
        busena: busena("klonavimas", k.busena),
        uzsakymas: u ? `${uzsakymoNr(u)} (${busena("uzsakymas", u.busena).toLowerCase()})` : "–",
        klientas: vardasPavarde(u && uzsakymoKlientas(u)),
        augintinis: augintinioPavadinimas(klonavimoAugintinis(k)),
        dnr: pavyzdys ? html(`${dnrKodas(pavyzdys)} ${zyma(pavyzdys.tinkamumas || "Neįvertintas")}`) : "–",
        inkubatorius: inkubatorius ? `${inkubatorius.modelis} (${inkubatorius.serijinis_numeris})` : "Nepriskirtas"
    });
    $("#grizti").href = nuoroda("darbuotojo-tyrimai.html", { darbuotojas: k.specialistas });
};

const kruvioFormule = () => `Darbo krūvis (%) = Σ aktyvių klonavimų trukmė (d.) ÷ (etatas × ${KRUVIO_NORMA} d.) × 100`;

// 2.26 pav. PA „Analizuoti darbuotojų krūvį“
PUSLAPIAI["kruvio-analize.html"] = () => {
    const specialistuSarasas = specialistai();
    rodikliai($("#rodikliai"), [
        [specialistuSarasas.length, "Laboratorijos specialistai"],
        [specialistuSarasas.filter(s => s.busena).length, "Aktyvūs specialistai", "sekme"],
        [visi("klonavimai").filter(k => k.busena).length, "Aktyvūs klonavimai", "info"]
    ]);
    $("#formule").textContent = kruvioFormule();
    $("#atlikti").addEventListener("click", () => eiti("kruvio-suvestine.html", "Darbo krūvio suvestinė suformuota."));
};

PUSLAPIAI["kruvio-suvestine.html"] = () => {
    const analize = kruvioAnalize().sort((a, b) => b.kruvis - a.kruvis);
    $("#aprasas").textContent = `Suformuota ${dabar()}. Laboratorijos specialistų: ${analize.length}.`;
    const kiek = lygis => analize.filter(r => r.lygis === lygis).length;
    const vidurkis = analize.length ? analize.reduce((suma, r) => suma + r.kruvis, 0) / analize.length : 0;
    rodikliai($("#rodikliai"), [
        [kiek("Didelis darbo krūvis"), "Didelis darbo krūvis", "klaida"],
        [kiek("Normalus darbo krūvis"), "Normalus darbo krūvis", "sekme"],
        [kiek("Mažas darbo krūvis"), "Mažas darbo krūvis", "info"],
        [`${skaicius(vidurkis, 1)} %`, "Vidutinis darbo krūvis"]
    ]);
    eilutes($("#suvestine"), analize, r => {
        const juosta = r.kruvis > 100 ? "klaida" : r.kruvis < 70 ? "info" : "";
        return `<tr>
            <td><a class="pagrindinis" href="${nuoroda("darbuotojas.html", { darbuotojas: r.darbuotojas.id })}">${esc(vardasPavarde(r.darbuotojas))}</a><small>${busena("specialistas", r.darbuotojas.busena)}</small></td>
            <td class="skaicius">${skaicius(r.darbuotojas.etatas)}</td>
            <td class="skaicius">${r.kiekis}</td>
            <td class="skaicius">${r.trukme} d.</td>
            <td><div class="kruvis"><div class="juosta ${juosta}"><span style="width: ${Math.min(r.kruvis, 100)}%"></span></div><b>${skaicius(r.kruvis, 1)} %</b></div></td>
            <td>${zyma(r.lygis)}</td>
        </tr>`;
    }, "Laboratorijos specialistų nėra.");
    $("#formule").textContent = `${kruvioFormule()}. Užimtumo lygis: mažas < 70 %, normalus 70–100 %, didelis > 100 %.`;
    $("#spausdinti").addEventListener("click", () => spausdinti());
};

document.addEventListener("DOMContentLoaded", () => {
    if (prieigaLeidziama) PUSLAPIAI[dabartinisPuslapis()]?.();
});
