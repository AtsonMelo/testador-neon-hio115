[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('LocalWip', 'Branch', 'PrReview', 'PostMerge')]
    [string]$Mode,

    [string]$BaseBranch = 'ui/issue-24-liga-io-industrial-host',

    [string]$ExpectedBranch,

    [switch]$RunGuiSmoke,

    [int]$PrNumber,

    [switch]$CopyToClipboard
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ProjectRelativePath = 'app/TestadorCLPHI.App/TestadorCLPHI.App.csproj'
$PhysicalCommandsExecuted = 0
$HasPrNumber = $PSBoundParameters.ContainsKey('PrNumber')
$StrongTerms = @(
    'SerialPort',
    'NModbus',
    'WriteSingle',
    'WriteMultiple',
    'ReadHolding',
    'ReadInput',
    'TcpClient',
    'Socket',
    'OpenPort',
    'ClosePort'
)

function Write-QaLine {
    param([AllowEmptyString()][string]$Message = '')

    Write-Host $Message
    Add-Content -LiteralPath $script:LogPath -Value $Message -Encoding UTF8
}

function Invoke-QaCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Label
    )

    Write-QaLine ''
    Write-QaLine "## $Label"
    Write-QaLine ("> {0} {1}" -f $FilePath, ($Arguments -join ' '))

    $commandOutput = @(& $FilePath @Arguments 2>&1)
    $exitCode = $LASTEXITCODE

    foreach ($line in $commandOutput) {
        Write-QaLine ([string]$line)
    }

    Write-QaLine ("Exit code: {0}" -f $exitCode)

    return [pscustomobject]@{
        ExitCode = $exitCode
        Output = @($commandOutput | ForEach-Object { [string]$_ })
    }
}

function Test-ProhibitedPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $normalizedPath = $Path.Replace('\\', '/')
    $fileName = [IO.Path]::GetFileName($normalizedPath)

    if ($fileName -ieq 'MainForm.cs') {
        return $true
    }

    if ($normalizedPath -match '(?i)\.(dpk|dmf|hst|prj)$') {
        return $true
    }

    if ($normalizedPath -match '(?i)(^|/)TESTADOR_NEON_HIO115_1_1(/|$)') {
        return $true
    }

    return $normalizedPath -match '(?i)(Modbus|Serial|PLC|Plc|Register|Registrador|Mapa)'
}

function Find-StrongTermHits {
    param([Parameter(Mandatory = $true)][string[]]$Paths)

    $hits = New-Object System.Collections.Generic.List[string]

    foreach ($path in $Paths) {
        $normalizedPath = $path.Replace('\\', '/')

        # O script declara os termos pesquisados; analisá-lo causaria autorreferência.
        if ($normalizedPath -ieq 'tools/qa/Invoke-TestadorQa.ps1') {
            continue
        }

        $fullPath = Join-Path $script:RepoRoot $normalizedPath
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            continue
        }

        try {
            $matches = Select-String -LiteralPath $fullPath -Pattern $script:StrongTerms -SimpleMatch -CaseSensitive
            foreach ($match in $matches) {
                $hits.Add(("{0}:{1}: {2}" -f $normalizedPath, $match.LineNumber, $match.Line.Trim()))
            }
        }
        catch {
            Write-QaLine ("AVISO: não foi possível analisar termos em {0}: {1}" -f $normalizedPath, $_.Exception.Message)
        }
    }

    return @($hits)
}

function Test-ValidatorFlagAvailable {
    param([Parameter(Mandatory = $true)][string]$Flag)

    $programPath = Join-Path (Split-Path -Parent $script:ProjectPath) 'Program.cs'
    if (-not (Test-Path -LiteralPath $programPath -PathType Leaf)) {
        throw "Não foi possível localizar Program.cs para detectar a flag $Flag."
    }

    return [bool](Select-String -LiteralPath $programPath -Pattern $Flag -SimpleMatch -Quiet)
}

function Invoke-GuiSmoke {
    param(
        [Parameter(Mandatory = $true)][string]$Flag,
        [Parameter(Mandatory = $true)][string]$ExpectedTitle
    )

    Write-QaLine ''
    Write-QaLine ("## Smoke GUI {0}" -f $Flag)
    Write-QaLine ("Título esperado: {0}" -f $ExpectedTitle)

    $existingIds = @(
        Get-Process -Name 'TestadorCLPHI.App' -ErrorAction SilentlyContinue |
            Select-Object -ExpandProperty Id
    )
    $runner = $null
    $appProcess = $null

    try {
        $argumentList = @(
            'run',
            '--no-build',
            '--project',
            $script:ProjectPath,
            '--',
            $Flag
        )

        $runner = Start-Process -FilePath 'dotnet' -ArgumentList $argumentList -WorkingDirectory $script:RepoRoot -WindowStyle Hidden -PassThru
        $deadline = (Get-Date).AddSeconds(30)

        while ((Get-Date) -lt $deadline) {
            Start-Sleep -Milliseconds 250

            $candidate = Get-Process -Name 'TestadorCLPHI.App' -ErrorAction SilentlyContinue |
                Where-Object { $existingIds -notcontains $_.Id -and $_.MainWindowTitle -eq $ExpectedTitle } |
                Select-Object -First 1

            if ($null -ne $candidate) {
                $appProcess = $candidate
                break
            }

            if ($runner.HasExited) {
                break
            }
        }

        if ($null -eq $appProcess) {
            Write-QaLine ("FALHA: janela não encontrada em 30 segundos para {0}." -f $Flag)
            return $false
        }

        Write-QaLine ("OK: janela encontrada (PID {0}): {1}" -f $appProcess.Id, $appProcess.MainWindowTitle)
        return $true
    }
    catch {
        Write-QaLine ("FALHA: smoke {0}: {1}" -f $Flag, $_.Exception.Message)
        return $false
    }
    finally {
        $newAppProcesses = @(
            Get-Process -Name 'TestadorCLPHI.App' -ErrorAction SilentlyContinue |
                Where-Object { $existingIds -notcontains $_.Id }
        )

        foreach ($process in $newAppProcesses) {
            [void]$process.CloseMainWindow()
            try {
                if (-not $process.WaitForExit(5000)) {
                    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                }
            }
            catch {
                # O processo pode terminar entre CloseMainWindow e WaitForExit.
            }
        }

        if ($null -ne $runner -and -not $runner.HasExited) {
            Stop-Process -Id $runner.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoProbe = @(& git -C $scriptDirectory rev-parse --show-toplevel 2>&1)
if ($LASTEXITCODE -ne 0) {
    throw "Não foi possível localizar a raiz do repositório: $($repoProbe -join [Environment]::NewLine)"
}

$script:RepoRoot = [IO.Path]::GetFullPath([string]$repoProbe[0])
$script:ProjectPath = Join-Path $script:RepoRoot $ProjectRelativePath
$artifactDirectory = Join-Path $script:RepoRoot 'artifacts/qa'
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$script:LogPath = Join-Path $artifactDirectory ("testador-qa-{0}-{1}.log" -f $Mode.ToLowerInvariant(), $timestamp)
New-Item -ItemType File -Path $script:LogPath -Force | Out-Null

$criticalFailure = $false
$warningFound = $false
$validatorResults = [ordered]@{}
$buildResultText = 'NÃO EXECUTADO'
$diffCheckResultText = 'NÃO EXECUTADO'
$guiSmokeResultText = 'NÃO SOLICITADO'
$changedPaths = @()
$branchName = 'DESCONHECIDA'
$headSha = 'DESCONHECIDO'

Push-Location $script:RepoRoot
try {
    Write-QaLine 'TESTADOR CLP HI - QA LOCAL'
    Write-QaLine ("Início: {0}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz'))
    Write-QaLine ("Modo: {0}" -f $Mode)
    Write-QaLine ("Base: {0}" -f $BaseBranch)
    if ($HasPrNumber) {
        Write-QaLine ("PR: #{0}" -f $PrNumber)
    }

    [void](Invoke-QaCommand -FilePath 'git' -Arguments @('status', '-sb') -Label 'git status -sb')
    $branchResult = Invoke-QaCommand -FilePath 'git' -Arguments @('branch', '--show-current') -Label 'git branch --show-current'
    if ($branchResult.ExitCode -eq 0 -and $branchResult.Output.Count -gt 0) {
        $branchName = $branchResult.Output[0].Trim()
    }

    $headResult = Invoke-QaCommand -FilePath 'git' -Arguments @('rev-parse', '--short', 'HEAD') -Label 'HEAD'
    if ($headResult.ExitCode -eq 0 -and $headResult.Output.Count -gt 0) {
        $headSha = $headResult.Output[0].Trim()
    }

    [void](Invoke-QaCommand -FilePath 'git' -Arguments @('log', '-5', '--oneline', '--decorate') -Label 'git log curto')

    if (-not [string]::IsNullOrWhiteSpace($ExpectedBranch) -and $branchName -cne $ExpectedBranch) {
        Write-QaLine ("BLOQUEIO: branch atual '{0}' difere de -ExpectedBranch '{1}'." -f $branchName, $ExpectedBranch)
        $criticalFailure = $true
    }

    $comparisonPaths = @()
    if ($Mode -eq 'PostMerge') {
        $comparison = Invoke-QaCommand -FilePath 'git' -Arguments @('diff', '--name-only', 'HEAD^', 'HEAD') -Label 'arquivos do último commit (pós-merge)'
    }
    elseif ($Mode -eq 'PrReview' -and $HasPrNumber) {
        $comparison = Invoke-QaCommand -FilePath 'gh' -Arguments @('pr', 'diff', [string]$PrNumber, '--name-only') -Label 'arquivos do PR'
    }
    else {
        $comparison = Invoke-QaCommand -FilePath 'git' -Arguments @('diff', '--name-only', ("{0}...HEAD" -f $BaseBranch)) -Label 'git diff --name-only contra a base'
    }

    if ($comparison.ExitCode -ne 0) {
        Write-QaLine 'BLOQUEIO: não foi possível determinar os arquivos da comparação principal.'
        $criticalFailure = $true
    }
    else {
        $comparisonPaths = @($comparison.Output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    }

    $porcelain = Invoke-QaCommand -FilePath 'git' -Arguments @('status', '--porcelain') -Label 'git status --porcelain'
    $unstaged = @(& git diff --name-only 2>&1)
    $unstagedExit = $LASTEXITCODE
    $staged = @(& git diff --cached --name-only 2>&1)
    $stagedExit = $LASTEXITCODE
    $untracked = @(& git ls-files --others --exclude-standard 2>&1)
    $untrackedExit = $LASTEXITCODE

    if ($porcelain.ExitCode -ne 0 -or $unstagedExit -ne 0 -or $stagedExit -ne 0 -or $untrackedExit -ne 0) {
        Write-QaLine 'BLOQUEIO: não foi possível consolidar o estado da árvore de trabalho.'
        $criticalFailure = $true
    }

    $changedPaths = @(
        $comparisonPaths + $unstaged + $staged + $untracked |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            ForEach-Object { $_.Trim().Replace('\\', '/') } |
            Sort-Object -Unique
    )

    Write-QaLine ''
    Write-QaLine '## Arquivos alterados consolidados'
    if ($changedPaths.Count -eq 0) {
        Write-QaLine '(nenhum)'
    }
    else {
        foreach ($path in $changedPaths) {
            Write-QaLine ("- {0}" -f $path)
        }
    }

    $prohibitedPaths = @($changedPaths | Where-Object { Test-ProhibitedPath -Path $_ })
    Write-QaLine ''
    Write-QaLine '## Checagem de arquivos proibidos'
    if ($prohibitedPaths.Count -gt 0) {
        foreach ($path in $prohibitedPaths) {
            Write-QaLine ("BLOQUEADO: {0}" -f $path)
        }
        $criticalFailure = $true
    }
    else {
        Write-QaLine 'OK: nenhum arquivo proibido alterado.'
    }

    Write-QaLine ''
    Write-QaLine '## Busca por termos operacionais fortes'
    Write-QaLine 'Escopo: arquivos alterados, exceto o próprio script que declara os termos.'
    if ($changedPaths.Count -eq 0) {
        Write-QaLine 'OK: nenhum arquivo alterado para escanear.'
    }
    else {
        $strongTermHits = @(Find-StrongTermHits -Paths $changedPaths)
        if ($strongTermHits.Count -gt 0) {
            foreach ($hit in $strongTermHits) {
                Write-QaLine ("ALERTA: {0}" -f $hit)
            }
            $warningFound = $true
        }
        else {
            Write-QaLine 'OK: nenhum termo operacional forte encontrado.'
        }
    }

    $validators = @(
        [pscustomobject]@{ Name = 'hardware-catalog'; Flag = '--validate-hardware-catalog'; DetectAvailability = $false },
        [pscustomobject]@{ Name = 'hardware-profile-selection'; Flag = '--validate-hardware-profile-selection'; DetectAvailability = $false },
        [pscustomobject]@{ Name = 'hardware-test-report'; Flag = '--validate-hardware-test-report'; DetectAvailability = $false },
        [pscustomobject]@{ Name = 'layout-3-host-readonly-safety'; Flag = '--validate-layout-3-host-readonly-safety'; DetectAvailability = $true },
        [pscustomobject]@{ Name = 'layout-3-read-bridge-disabled'; Flag = '--validate-layout-3-read-bridge-disabled'; DetectAvailability = $true },
        [pscustomobject]@{ Name = 'layout-3-read-bridge-activation-gate'; Flag = '--validate-layout-3-read-bridge-activation-gate'; DetectAvailability = $true }
    )

    foreach ($validator in $validators) {
        if ($validator.DetectAvailability -and -not (Test-ValidatorFlagAvailable -Flag $validator.Flag)) {
            Write-QaLine ''
            Write-QaLine ("## validador {0}" -f $validator.Name)
            Write-QaLine ("NÃO DISPONÍVEL: flag {0} ausente nesta branch; execução ignorada com segurança." -f $validator.Flag)
            $validatorResults[$validator.Name] = 'NÃO DISPONÍVEL'
            continue
        }

        $result = Invoke-QaCommand -FilePath 'dotnet' -Arguments @('run', '--project', $script:ProjectPath, '--', $validator.Flag) -Label ("validador {0}" -f $validator.Name)
        if ($result.ExitCode -eq 0) {
            $validatorResults[$validator.Name] = 'OK'
        }
        else {
            $validatorResults[$validator.Name] = 'FALHOU'
            $criticalFailure = $true
        }
    }

    $buildResult = Invoke-QaCommand -FilePath 'dotnet' -Arguments @('build', $script:ProjectPath) -Label 'dotnet build'
    if ($buildResult.ExitCode -eq 0) {
        $buildResultText = 'OK'
    }
    else {
        $buildResultText = 'FALHOU'
        $criticalFailure = $true
    }

    $diffCheckResult = Invoke-QaCommand -FilePath 'git' -Arguments @('diff', '--check') -Label 'git diff --check'
    if ($diffCheckResult.ExitCode -eq 0) {
        $diffCheckResultText = 'OK'
    }
    else {
        $diffCheckResultText = 'FALHOU'
        $criticalFailure = $true
    }

    if ($RunGuiSmoke) {
        $smokeCases = @(
            [pscustomobject]@{ Flag = '--layout-3-host-readonly'; Title = 'Layout 3 - Host read-only | Testador CLP HI' },
            [pscustomobject]@{ Flag = '--industrial'; Title = 'Testador Industrial HI' }
        )

        $smokeFailures = 0
        foreach ($smokeCase in $smokeCases) {
            if (-not (Invoke-GuiSmoke -Flag $smokeCase.Flag -ExpectedTitle $smokeCase.Title)) {
                $smokeFailures++
            }
        }

        if ($smokeFailures -eq 0) {
            $guiSmokeResultText = "OK ($($smokeCases.Count)/$($smokeCases.Count))"
        }
        else {
            $guiSmokeResultText = "FALHOU ($smokeFailures/$($smokeCases.Count))"
            $criticalFailure = $true
        }
    }

    if ($criticalFailure) {
        $overallStatus = 'BLOQUEADO'
        $nextStep = 'Corrigir os bloqueios reportados e executar novamente o mesmo modo.'
    }
    elseif ($warningFound) {
        $overallStatus = 'PARCIAL'
        $nextStep = 'Revisar manualmente os termos operacionais encontrados antes de prosseguir.'
    }
    elseif (-not $RunGuiSmoke) {
        $overallStatus = 'OK'
        $nextStep = 'Executar -RunGuiSmoke antes da revisão final quando o ambiente gráfico estiver disponível.'
    }
    else {
        $overallStatus = 'OK'
        $nextStep = 'Prosseguir para a revisão do diff e publicação.'
    }

    Write-QaLine ''
    Write-QaLine '============================================================'
    Write-QaLine 'RESUMO FINAL'
    Write-QaLine ("Status: {0}" -f $overallStatus)
    Write-QaLine ("Branch: {0}" -f $branchName)
    Write-QaLine ("Head: {0}" -f $headSha)
    Write-QaLine ("Arquivos alterados: {0}" -f $changedPaths.Count)
    foreach ($path in $changedPaths) {
        Write-QaLine ("  - {0}" -f $path)
    }
    Write-QaLine 'Validadores:'
    foreach ($validatorResult in $validatorResults.GetEnumerator()) {
        Write-QaLine ("  - {0}: {1}" -f $validatorResult.Key, $validatorResult.Value)
    }
    Write-QaLine ("Build: {0}" -f $buildResultText)
    Write-QaLine ("Diff-check: {0}" -f $diffCheckResultText)
    Write-QaLine ("Smoke GUI: {0}" -f $guiSmokeResultText)
    Write-QaLine ("Comandos físicos executados: {0}" -f $PhysicalCommandsExecuted)
    Write-QaLine ("Próximo passo recomendado: {0}" -f $nextStep)
    Write-QaLine ("Log: {0}" -f $script:LogPath)
    Write-QaLine '============================================================'

    if ($CopyToClipboard) {
        try {
            Get-Content -LiteralPath $script:LogPath -Raw | Set-Clipboard
            Write-QaLine 'Saída copiada para o clipboard.'
        }
        catch {
            Write-QaLine ("AVISO: não foi possível copiar a saída para o clipboard: {0}" -f $_.Exception.Message)
        }
    }

    # Evita 'exit', que encerraria uma sessão interativa do PowerShell.
    # Mantém um código de saída consultável por automação via $LASTEXITCODE/$global:LASTEXITCODE.
    if ($criticalFailure) {
        Write-QaLine 'BLOQUEADO: encerrando de forma segura sem fechar a sessão (código 1).'
        $global:LASTEXITCODE = 1
        return
    }

    $global:LASTEXITCODE = 0
    return
}
finally {
    Pop-Location
}
