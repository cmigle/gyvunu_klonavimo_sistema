-- Gyvūnų klonavimo sistema – duomenų bazės schema (MySQL 8.4)
-- Gauta transformuojant dalykinės srities esybių klasių diagramą (2.3 pav.):
-- kiekviena «Entity» klasė tampa lentele, asociacijos – išoriniais raktais,
-- paveldėjimas Darbuotojas ← Laboratorijos specialistas – lentele su bendru pirminiu raktu,
-- išvardijimo tipas GyvunoTipas – ENUM tipu.

CREATE DATABASE IF NOT EXISTS gyvunu_klonavimas
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_lt_0900_ai_ci;

USE gyvunu_klonavimas;

-- ---------- Kliento ir augintinio valdymas ----------

CREATE TABLE klientas (
    id                INT          NOT NULL AUTO_INCREMENT,
    vardas            VARCHAR(50)  NOT NULL,
    pavarde           VARCHAR(50)  NOT NULL,
    el_pastas         VARCHAR(100) NOT NULL,
    telefono_numeris  VARCHAR(20)  NOT NULL,
    asmens_kodas      BIGINT       NOT NULL,
    slaptazodis       VARCHAR(255) NOT NULL,
    istorijos_busena  VARCHAR(20)  NOT NULL DEFAULT 'Nepatikrintas',
    patikrinimo_data  DATETIME     NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_klientas_el_pastas UNIQUE (el_pastas),
    CONSTRAINT uq_klientas_asmens_kodas UNIQUE (asmens_kodas),
    CONSTRAINT ck_klientas_istorijos_busena
        CHECK (istorijos_busena IN ('Nepatikrintas', 'Patikrintas', 'Netinkamas'))
);

CREATE TABLE augintinis (
    id                    INT          NOT NULL AUTO_INCREMENT,
    vardas                VARCHAR(50)  NOT NULL,
    veisle                VARCHAR(80)  NOT NULL,
    amzius                INT          NOT NULL,
    tipas                 ENUM('Šuo', 'Katė', 'Triušis', 'Žirgas', 'Avis', 'Kiaulė', 'Papūga') NOT NULL,
    lytis                 VARCHAR(10)  NOT NULL,
    svoris                DOUBLE       NOT NULL,
    mikroschemos_numeris  CHAR(15)     NOT NULL,
    klientas_id           INT          NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_augintinis_mikroschemos_numeris UNIQUE (mikroschemos_numeris),
    CONSTRAINT fk_augintinis_klientas FOREIGN KEY (klientas_id)
        REFERENCES klientas (id) ON DELETE CASCADE,
    CONSTRAINT ck_augintinis_amzius CHECK (amzius BETWEEN 0 AND 100),
    CONSTRAINT ck_augintinis_svoris CHECK (svoris > 0),
    CONSTRAINT ck_augintinis_lytis CHECK (lytis IN ('Patinas', 'Patelė'))
);

CREATE TABLE dokumentas (
    id                 INT          NOT NULL AUTO_INCREMENT,
    failo_pavadinimas  VARCHAR(255) NOT NULL,
    dokumento_tipas    VARCHAR(50)  NOT NULL,
    ikelimo_data       DATE         NOT NULL,
    failo_formatas     VARCHAR(10)  NOT NULL,
    galiojimo_data     DATE         NOT NULL,
    augintinis_id      INT          NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_dokumentas_augintinis FOREIGN KEY (augintinis_id)
        REFERENCES augintinis (id) ON DELETE CASCADE,
    CONSTRAINT ck_dokumentas_tipas
        CHECK (dokumento_tipas IN ('Kilmės sertifikatas', 'Sutartis', 'Veterinarinis pasas', 'Kita')),
    CONSTRAINT ck_dokumentas_formatas CHECK (failo_formatas IN ('PDF', 'JPG', 'PNG'))
);

-- ---------- Užsakymo valdymas ----------

CREATE TABLE uzsakymas (
    id                 INT            NOT NULL AUTO_INCREMENT,
    kaina              DECIMAL(10, 2) NOT NULL,
    sukurimo_data      DATE           NOT NULL,
    busena             BOOLEAN        NOT NULL DEFAULT FALSE,
    patvirtinimo_data  DATE           NULL,
    augintinis_id      INT            NOT NULL,
    PRIMARY KEY (id),
    -- Augintinis turi ne daugiau kaip vieną užsakymą (1 : 0..1)
    CONSTRAINT uq_uzsakymas_augintinis UNIQUE (augintinis_id),
    CONSTRAINT fk_uzsakymas_augintinis FOREIGN KEY (augintinis_id)
        REFERENCES augintinis (id) ON DELETE CASCADE,
    CONSTRAINT ck_uzsakymas_kaina CHECK (kaina >= 0),
    CONSTRAINT ck_uzsakymas_patvirtinimas CHECK (busena = FALSE OR patvirtinimo_data IS NOT NULL)
);

CREATE TABLE pranesimas (
    id             INT      NOT NULL AUTO_INCREMENT,
    tekstas        TEXT     NOT NULL,
    siuntimo_data  DATETIME NOT NULL,
    busena         BOOLEAN  NOT NULL DEFAULT FALSE,
    uzsakymas_id   INT      NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_pranesimas_uzsakymas UNIQUE (uzsakymas_id),
    CONSTRAINT fk_pranesimas_uzsakymas FOREIGN KEY (uzsakymas_id)
        REFERENCES uzsakymas (id) ON DELETE CASCADE
);

-- ---------- DNR pavyzdžio valdymas ----------

CREATE TABLE dnr_pavyzdys (
    id                     INT         NOT NULL AUTO_INCREMENT,
    paemimo_data           DATE        NOT NULL,
    saugojimo_temperatura  DOUBLE      NOT NULL,
    -- Analizės rezultatai įvedami vėliau, todėl neprivalomi
    dnr_koncentracija      DOUBLE      NULL,
    tirpalo_turis          DOUBLE      NULL,
    baltymu_tarsa          DOUBLE      NULL,
    din_verte              DOUBLE      NULL,
    lasteliu_gyvybingumas  DOUBLE      NULL,
    tinkamumas             VARCHAR(20) NULL,
    tinkamumo_data         DATE        NULL,
    augintinis_id          INT         NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_dnr_pavyzdys_augintinis UNIQUE (augintinis_id),
    CONSTRAINT fk_dnr_pavyzdys_augintinis FOREIGN KEY (augintinis_id)
        REFERENCES augintinis (id) ON DELETE CASCADE,
    CONSTRAINT ck_dnr_pavyzdys_tinkamumas CHECK (tinkamumas IN ('Tinkamas klonuoti', 'Netinkamas'))
);

-- ---------- Inkubatorių valdymas ----------

CREATE TABLE inkubatorius (
    id                    INT          NOT NULL AUTO_INCREMENT,
    modelis               VARCHAR(100) NOT NULL,
    serijinis_numeris     VARCHAR(50)  NOT NULL,
    tiksline_temperatura  DOUBLE       NOT NULL,
    talpa                 DOUBLE       NOT NULL,
    co2_procentas         DOUBLE       NOT NULL,
    busena                BOOLEAN      NOT NULL DEFAULT FALSE,
    ph_lygis              DOUBLE       NOT NULL,
    o2_procentas          DOUBLE       NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_inkubatorius_serijinis_numeris UNIQUE (serijinis_numeris),
    CONSTRAINT ck_inkubatorius_temperatura CHECK (tiksline_temperatura BETWEEN 20 AND 45),
    CONSTRAINT ck_inkubatorius_talpa CHECK (talpa > 0),
    CONSTRAINT ck_inkubatorius_co2 CHECK (co2_procentas BETWEEN 0 AND 20),
    CONSTRAINT ck_inkubatorius_o2 CHECK (o2_procentas BETWEEN 1 AND 21),
    CONSTRAINT ck_inkubatorius_ph CHECK (ph_lygis BETWEEN 6 AND 8.5)
);

CREATE TABLE inkubatoriaus_matavimas (
    id               INT      NOT NULL AUTO_INCREMENT,
    matavimo_data    DATETIME NOT NULL,
    temperatura      DOUBLE   NOT NULL,
    dregme           DOUBLE   NOT NULL,
    inkubatorius_id  INT      NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_inkubatoriaus_matavimas_inkubatorius FOREIGN KEY (inkubatorius_id)
        REFERENCES inkubatorius (id) ON DELETE CASCADE,
    CONSTRAINT ck_inkubatoriaus_matavimas_dregme CHECK (dregme BETWEEN 0 AND 100)
);

-- ---------- Darbuotojų valdymas ----------

CREATE TABLE darbuotojas (
    id                INT          NOT NULL AUTO_INCREMENT,
    vardas            VARCHAR(50)  NOT NULL,
    pavarde           VARCHAR(50)  NOT NULL,
    telefono_numeris  VARCHAR(20)  NOT NULL,
    el_pastas         VARCHAR(100) NOT NULL,
    etatas            DOUBLE       NOT NULL,
    slaptazodis       VARCHAR(255) NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_darbuotojas_el_pastas UNIQUE (el_pastas),
    CONSTRAINT ck_darbuotojas_etatas CHECK (etatas > 0 AND etatas <= 1)
);

-- Paveldėjimas: laboratorijos specialistas yra darbuotojas (bendras pirminis raktas).
-- Darbuotojas be įrašo šioje lentelėje – administratorius.
CREATE TABLE laboratorijos_specialistas (
    darbuotojas_id      INT         NOT NULL,
    busena              BOOLEAN     NOT NULL DEFAULT TRUE,
    licencijos_numeris  VARCHAR(20) NOT NULL,
    PRIMARY KEY (darbuotojas_id),
    CONSTRAINT uq_laboratorijos_specialistas_licencija UNIQUE (licencijos_numeris),
    CONSTRAINT fk_laboratorijos_specialistas_darbuotojas FOREIGN KEY (darbuotojas_id)
        REFERENCES darbuotojas (id) ON DELETE CASCADE
);

-- ---------- Klonavimas ----------

CREATE TABLE klonavimas (
    id                             INT          NOT NULL AUTO_INCREMENT,
    pradzios_data                  DATE         NOT NULL,
    pabaigos_data                  DATE         NOT NULL,
    rezultatas                     VARCHAR(255) NULL,
    specialisto_patvirtinimas      BOOLEAN      NOT NULL DEFAULT FALSE,
    busena                         BOOLEAN      NOT NULL DEFAULT TRUE,
    uzsakymas_id                   INT          NOT NULL,
    dnr_pavyzdys_id                INT          NOT NULL,
    laboratorijos_specialistas_id  INT          NULL,
    inkubatorius_id                INT          NULL,
    PRIMARY KEY (id),
    CONSTRAINT uq_klonavimas_uzsakymas UNIQUE (uzsakymas_id),
    CONSTRAINT uq_klonavimas_dnr_pavyzdys UNIQUE (dnr_pavyzdys_id),
    -- Vykdomo ar baigto klonavimo užsakymo ir DNR pavyzdžio ištrinti negalima
    CONSTRAINT fk_klonavimas_uzsakymas FOREIGN KEY (uzsakymas_id)
        REFERENCES uzsakymas (id) ON DELETE RESTRICT,
    CONSTRAINT fk_klonavimas_dnr_pavyzdys FOREIGN KEY (dnr_pavyzdys_id)
        REFERENCES dnr_pavyzdys (id) ON DELETE RESTRICT,
    CONSTRAINT fk_klonavimas_laboratorijos_specialistas FOREIGN KEY (laboratorijos_specialistas_id)
        REFERENCES laboratorijos_specialistas (darbuotojas_id) ON DELETE SET NULL,
    CONSTRAINT fk_klonavimas_inkubatorius FOREIGN KEY (inkubatorius_id)
        REFERENCES inkubatorius (id) ON DELETE SET NULL,
    CONSTRAINT ck_klonavimas_datos CHECK (pabaigos_data >= pradzios_data)
);
