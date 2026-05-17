param(
    [string]$AssetDir = ".\app\TestadorCLPHI.App\Assets\Ui",
    [string[]]$RequiredAssets = @(
        "led_on_green.png",
        "led_off_gray.png"
    ),
    [int]$ExpectedWidth = 256,
    [int]$ExpectedHeight = 256,
    [switch]$Strict
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$hasError = $false

function Test-PngSignature {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)

    if ($bytes.Length -lt 8) {
        return $false
    }

    $signature = @(137, 80, 78, 71, 13, 10, 26, 10)

    for ($i = 0; $i -lt $signature.Count; $i++) {
        if ($bytes[$i] -ne $signature[$i]) {
            return $false
        }
    }

    return $true
}

function Test-HasTransparency {
    param([System.Drawing.Bitmap]$Bitmap)

    for ($y = 0; $y -lt $Bitmap.Height; $y++) {
        for ($x = 0; $x -lt $Bitmap.Width; $x++) {
            if ($Bitmap.GetPixel($x, $y).A -lt 255) {
                return $true
            }
        }
    }

    return $false
}

Write-Output "=== VALIDACAO DE ASSETS UI ==="
Write-Output "Diretorio: $AssetDir"
Write-Output "Tamanho esperado: ${ExpectedWidth}x${ExpectedHeight}"
Write-Output ""

if (-not (Test-Path $AssetDir)) {
    Write-Output "ERRO: diretorio nao encontrado: $AssetDir"
    exit 1
}

foreach ($assetName in $RequiredAssets) {
    $assetPath = Join-Path $AssetDir $assetName

    Write-Output "Asset: $assetName"

    if (-not (Test-Path $assetPath)) {
        Write-Output "  PENDENTE: arquivo nao encontrado."

        if ($Strict) {
            $hasError = $true
        }

        Write-Output ""
        continue
    }

    if (-not (Test-PngSignature -Path $assetPath)) {
        Write-Output "  ERRO: arquivo nao parece ser PNG valido."
        $hasError = $true
        Write-Output ""
        continue
    }

    $image = $null

    try {
        $image = [System.Drawing.Image]::FromFile($assetPath)
        $bitmap = [System.Drawing.Bitmap]::new($image)

        Write-Output "  Formato: PNG"
        Write-Output "  Dimensoes: $($image.Width)x$($image.Height) px"

        if ($image.Width -ne $ExpectedWidth -or $image.Height -ne $ExpectedHeight) {
            Write-Output "  ERRO: dimensao diferente do esperado."
            $hasError = $true
        }
        else {
            Write-Output "  OK: dimensao correta."
        }

        if (Test-HasTransparency -Bitmap $bitmap) {
            Write-Output "  OK: possui transparencia."
        }
        else {
            Write-Output "  ERRO: nao foi detectada transparencia."
            $hasError = $true
        }

        $bitmap.Dispose()
    }
    finally {
        if ($null -ne $image) {
            $image.Dispose()
        }
    }

    Write-Output ""
}

if ($hasError) {
    Write-Output "RESULTADO: FALHA"
    exit 1
}

Write-Output "RESULTADO: OK"
exit 0
