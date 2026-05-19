param(
    [string]$RootPath = "."
)

$ErrorActionPreference = "Stop"

$grave = [char]96
$literalCrLf = [string]$grave + "r" + [string]$grave + "n"
$hasError = $false

$files = Get-ChildItem -Path $RootPath -Filter "*.cs" -Recurse -File |
    Where-Object {
        $_.FullName -notmatch "\\bin\\" -and
        $_.FullName -notmatch "\\obj\\"
    }

foreach ($file in $files) {
    $text = Get-Content -Path $file.FullName -Raw

    if ($text.Contains($literalCrLf)) {
        Write-Host "ERRO: literal de quebra de linha encontrado em arquivo C#:" -ForegroundColor Red
        Write-Host "  $($file.FullName)" -ForegroundColor Red
        $hasError = $true
    }

    $lines = Get-Content -Path $file.FullName

    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index].Contains($grave)) {
            Write-Host "ERRO: caractere de crase encontrado em arquivo C#:" -ForegroundColor Red
            Write-Host "  $($file.FullName):$($index + 1)" -ForegroundColor Red
            Write-Host "  $($lines[$index])" -ForegroundColor DarkYellow
            $hasError = $true
        }
    }
}

if ($hasError) {
    exit 1
}

Write-Host "OK: nenhum literal problemático encontrado em arquivos C#." -ForegroundColor Green
exit 0
