# Paleidžia vietinį MySQL 8.4 serverį be Windows tarnybos: duomenys laikomi naudotojo aplanke,
# root naudotojas be slaptažodžio, jungtis tik iš šio kompiuterio (127.0.0.1).
# Naudojimas: powershell -ExecutionPolicy Bypass -File scripts\paleisti-mysql.ps1
param(
    [string]$MySqlKatalogas = "C:\Program Files\MySQL\MySQL Server 8.4",
    [string]$Duomenys = (Join-Path $env:LOCALAPPDATA "MySQL\gks-data"),
    [int]$Prievadas = 3306
)

$mysqld = Join-Path $MySqlKatalogas "bin\mysqld.exe"
if (-not (Test-Path $mysqld)) {
    throw "Nerastas $mysqld. Įdiekite MySQL Server 8.4 arba nurodykite -MySqlKatalogas."
}

if (-not (Test-Path $Duomenys)) {
    New-Item -ItemType Directory -Force (Split-Path $Duomenys) | Out-Null
    & $mysqld --no-defaults --initialize-insecure --basedir="$MySqlKatalogas" --datadir="$Duomenys" --console
}

Write-Host "MySQL klauso 127.0.0.1:$Prievadas (root, be slaptažodžio). Sustabdyti – Ctrl+C."
& $mysqld --no-defaults --basedir="$MySqlKatalogas" --datadir="$Duomenys" --port=$Prievadas `
    --bind-address=127.0.0.1 --mysqlx=OFF --console
