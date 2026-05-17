param(
    [string]$SourceDir,
    [string]$OutputRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path $SourceDir)) {
    throw "SourceDir nao encontrado: $SourceDir"
}

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

$files = Get-ChildItem $SourceDir -Filter "*.png" |
    Where-Object { $_.Name -notlike "_contact*" } |
    Sort-Object Name

$mapping = @(
    @{ Index = 0; Name = "led_on_green.png";       Size = 210 },
    @{ Index = 1; Name = "led_off_gray.png";       Size = 210 },
    @{ Index = 2; Name = "push_button_red.png";    Size = 224 },
    @{ Index = 3; Name = "push_button_green.png";  Size = 224 },
    @{ Index = 4; Name = "push_button_yellow.png"; Size = 224 },
    @{ Index = 5; Name = "push_button_blue.png";   Size = 224 },
    @{ Index = 6; Name = "stop_emergency.png";     Size = 238 }
)

$fuzzValues = @(16, 22, 28)

foreach ($fuzz in $fuzzValues) {
    $outDir = Join-Path $OutputRoot "fuzz_$fuzz"
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null

    Write-Output ""
    Write-Output "=== FUZZ $fuzz ==="

    foreach ($item in $mapping) {
        if ($item.Index -ge $files.Count) {
            Write-Output "PULADO: indice $($item.Index) nao existe."
            continue
        }

        $source = $files[$item.Index].FullName
        $target = Join-Path $outDir $item.Name

        $dim = (& magick identify -format "%w %h" "$source") -split " "
        $w = [int]$dim[0]
        $h = [int]$dim[1]

        $points = @(
            "0,0",
            "$($w - 1),0",
            "0,$($h - 1)",
            "$($w - 1),$($h - 1)",
            "$([int]($w / 2)),0",
            "$([int]($w / 2)),$($h - 1)",
            "0,$([int]($h / 2))",
            "$($w - 1),$([int]($h / 2))"
        )

        $args = @(
            "$source",
            "-alpha", "set",
            "-fuzz", "$fuzz%",
            "-fill", "none"
        )

        foreach ($point in $points) {
            $args += @("-draw", "color $point floodfill")
        }

        $args += @(
            "-trim", "+repage",
            "-resize", "$($item.Size)x$($item.Size)",
            "-background", "none",
            "-gravity", "center",
            "-extent", "256x256",
            "$target"
        )

        & magick @args

        Write-Output "OK: $($files[$item.Index].Name) -> fuzz_$fuzz\$($item.Name)"
    }

    $assets = Get-ChildItem $outDir -Filter "*.png" | Sort-Object Name
    $thumbs = @()

    foreach ($asset in $assets) {
        $thumb = Join-Path $outDir ("thumb_" + $asset.Name)

        & magick "$($asset.FullName)" `
            -resize 96x96 `
            -background none `
            -gravity center `
            -extent 128x128 `
            "$thumb"

        $thumbs += $thumb
    }

    if ($thumbs.Count -gt 0) {
        $preview = Join-Path $outDir "_preview_dark_fuzz_$fuzz.png"

        & magick montage $thumbs `
            -background "#1b222a" `
            -geometry +18+18 `
            -tile 4x2 `
            "$preview"

        Write-Output "PREVIEW: $preview"
    }
}
