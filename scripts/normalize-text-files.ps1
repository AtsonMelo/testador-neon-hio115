param(
    [string]$RootPath = "."
)

$ErrorActionPreference = "Stop"

$extensions = @(
    ".cs",
    ".py",
    ".ps1",
    ".md",
    ".json",
    ".csv",
    ".st",
    ".csproj",
    ".sln",
    ".gitignore",
    ".editorconfig"
)

$ignoredParts = @(
    "\.git\",
    "\bin\",
    "\obj\",
    "\backups-dpk\"
)

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$utf8Bom = New-Object System.Text.UTF8Encoding($true)

$rootFullPath = (Resolve-Path $RootPath).Path

$files = Get-ChildItem -Path $rootFullPath -Recurse -File |
    Where-Object {
        $fullName = $_.FullName
        $extension = $_.Extension

        if ($_.Name -eq ".gitignore" -or $_.Name -eq ".editorconfig") {
            $extension = $_.Name
        }

        $isIgnored = $false

        foreach ($part in $ignoredParts) {
            if ($fullName -match [regex]::Escape($part)) {
                $isIgnored = $true
                break
            }
        }

        -not $isIgnored -and $extensions -contains $extension
    }

foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)

    $hasBom =
        $bytes.Length -ge 3 -and
        $bytes[0] -eq 0xEF -and
        $bytes[1] -eq 0xBB -and
        $bytes[2] -eq 0xBF

    $text = [System.Text.Encoding]::UTF8.GetString($bytes)

    if ($hasBom) {
        $text = $text.TrimStart([char]0xFEFF)
    }

    $lines = $text -split "\r\n|\n|\r"

    if ($file.Extension -eq ".md") {
        $normalizedLines = $lines
    } else {
        $normalizedLines = foreach ($line in $lines) {
            $line.TrimEnd()
        }
    }

    $normalizedText = ($normalizedLines -join "`r`n").TrimEnd() + "`r`n"

    $encoding = if ($file.Extension -eq ".csv" -or $file.Extension -eq ".sln" -or $hasBom) {
        $utf8Bom
    } else {
        $utf8NoBom
    }

    [System.IO.File]::WriteAllText(
        $file.FullName,
        $normalizedText,
        $encoding
    )
}

Write-Output "Arquivos normalizados: $($files.Count)"
