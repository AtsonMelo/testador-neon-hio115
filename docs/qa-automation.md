# Automação local de QA

O script `tools/qa/Invoke-TestadorQa.ps1` consolida as verificações repetitivas do
projeto em uma execução somente de leitura. Ele não envia comandos físicos, não
abre comunicação com o controlador e não altera o estado do Git. A única escrita
feita em runtime é a criação de logs ignorados pelo Git em `artifacts/qa/`.

## Pré-requisitos

- Windows com PowerShell 5.1 ou superior;
- SDK .NET 8;
- Git disponível no `PATH`;
- GitHub CLI (`gh`) apenas quando `-Mode PrReview -PrNumber` for usado;
- sessão gráfica disponível para `-RunGuiSmoke`.

O script pode ser chamado de qualquer diretório. O projeto usado pelos
validadores e pelo build é `app/TestadorCLPHI.App/TestadorCLPHI.App.csproj`.

## Exemplos

Análise local, sem abrir janelas:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode LocalWip -CopyToClipboard
```

Revisão de branch contra a base padrão, incluindo os quatro smokes GUI:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode Branch -BaseBranch ui/issue-24-liga-io-industrial-host -RunGuiSmoke -CopyToClipboard
```

Revisão de um PR existente:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode PrReview -PrNumber 42 -RunGuiSmoke
```

Pós-merge, verificando os arquivos do último commit:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode PostMerge -RunGuiSmoke -CopyToClipboard
```

Para bloquear uma execução iniciada na branch errada, informe a branch esperada:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode Branch -ExpectedBranch tools/qa-automation-local
```

## O que é verificado

A execução registra o estado resumido do Git, branch, HEAD, log curto, arquivos
alterados e status porcelain. Em `LocalWip` e `Branch`, a comparação principal é
feita contra `-BaseBranch`; em `PrReview -PrNumber`, a lista vem do PR; em
`PostMerge`, a comparação considera o último commit.

Os arquivos alterados são bloqueados quando o caminho inclui `MainForm.cs`,
artefatos HIstudio (`.dpk`, `.dmf`, `.hst` ou `.prj`),
`TESTADOR_NEON_HIO115_1_1` ou nomes associados a Modbus, Serial, PLC,
registradores e mapas. A busca de conteúdo também alerta para termos operacionais
fortes. O próprio script é excluído dessa busca porque contém a declaração dos
termos pesquisados.

Depois dessas verificações, o script executa os três validadores de hardware com
`dotnet run -- --validate-*`, seguidos por `dotnet build` e `git diff --check`.
Os comandos `dotnet` recebem explicitamente o caminho do projeto. Nenhuma flag
da aplicação que inicialize comunicação física é usada.

## Smoke GUI

Com `-RunGuiSmoke`, são verificadas as flags:

- `--layout-3-host-readonly`;
- `--preview-layout-3`;
- `--preview-layout-3-light`;
- `--preview-layout-3-auto`.

Cada processo deve abrir em até 30 segundos com o título exato previsto. Após a
validação, a janela e o processo auxiliar são encerrados, inclusive em caso de
falha.

## Resultado e exit code

O console e o arquivo em `artifacts/qa/` recebem o relatório completo. O resumo
final informa status, branch, HEAD, arquivos, validadores, build, diff-check,
smoke GUI, total de comandos físicos e próximo passo recomendado. Com
`-CopyToClipboard`, o conteúdo do log também é copiado para o clipboard.

- `OK`: todos os gates executados passaram;
- `PARCIAL`: os gates passaram, mas termos operacionais exigem revisão manual;
- `BLOQUEADO`: ocorreu uma falha crítica.

Arquivos proibidos, branch diferente de `-ExpectedBranch`, falha ao obter a
comparação, validador reprovado, build reprovado, diff-check reprovado ou smoke
GUI reprovado produzem exit code `1`. A ausência de `-RunGuiSmoke` é registrada
como não solicitada e não reprova a análise.
