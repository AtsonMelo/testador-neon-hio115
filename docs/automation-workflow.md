# Fluxo de automação assistida por IA

Este documento descreve como o projeto opera num fluxo de automação assistida por
IA, no qual assistentes (Claude/Codex) implementam código, rodam validações,
fazem commit/push/PR Draft e deixam cada milestone pronta para validação humana.
O objetivo é reduzir cópia/cola manual mantendo a validação visual humana e a
segurança exigida por automação industrial.

A camada de QA é o script `tools/qa/Invoke-TestadorQa.ps1`. Os detalhes de cada
gate estão em [`qa-automation.md`](qa-automation.md).

## Princípios

- A validação **visual** da UI continua sendo humana, ao final de cada milestone.
- Nenhum comando físico é enviado ao controlador. O contador
  `Comandos físicos executados` deve permanecer `0`.
- A automação **bloqueia** alterações em arquivos sensíveis (app de comunicação,
  Modbus, Serial, PLC, mapas/registradores, artefatos HIstudio, `MainForm.cs`,
  `Program.cs`, `TESTADOR_NEON_HIO115_1_1`).
- O QA **não usa `exit`**; ele retorna de forma segura para não fechar a sessão
  interativa do PowerShell. Em bloqueio, registra `BLOQUEADO` e retorna.

## Ciclo de uma milestone

1. **Usuário** define o objetivo/milestone e o escopo permitido.
2. **IA executora** cria ou atualiza a branch de trabalho.
3. **IA executora** implementa as alterações dentro do escopo.
4. **IA executora** roda o QA local (`-Mode LocalWip`).
5. **IA executora** corrige os erros apontados e repete o QA até ficar limpo.
6. **IA executora** abre um **PR Draft** contra a base combinada.
7. **Usuário** valida visualmente a UI e aprova ou pede ajustes.
8. **IA executora** ajusta, se necessário, e roda o QA novamente.
9. PR fica **pronto para merge** após aprovação humana.
10. **Pós-merge**: roda o QA em `-Mode PostMerge`.
11. Opcionalmente, cria tag/checkpoint da milestone validada.

## Comandos principais

Análise local da árvore de trabalho:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode LocalWip -CopyToClipboard
```

Análise de branch contra a base, incluindo smoke GUI:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode Branch -BaseBranch ui/issue-24-liga-io-industrial-host -ExpectedBranch <branch> -RunGuiSmoke -CopyToClipboard
```

Análise de um PR existente:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode PrReview -PrNumber <numero> -CopyToClipboard
```

Pós-merge:

```powershell
.\tools\qa\Invoke-TestadorQa.ps1 -Mode PostMerge -RunGuiSmoke -CopyToClipboard
```

## Responsabilidades

### IA executora

- cria a branch de trabalho;
- altera código/documentação **dentro do escopo** permitido;
- roda o QA local e interpreta o relatório;
- corrige os erros apontados;
- faz commit/push e abre PR Draft;
- resume o resultado para o usuário.

### Usuário

- valida visualmente a UI;
- aprova ou pede ajuste;
- sugere novas ideias;
- decide o próximo passo / a próxima milestone.

### Assistente supervisor

- define o escopo permitido e o escopo proibido;
- analisa o relatório de QA;
- recomenda merge ou ajuste;
- protege contra risco operacional (comando físico, arquivos sensíveis).

## Artefatos

Cada execução grava um log em `artifacts/qa/`. Esses logs **não são versionados**
(a pasta pode permanecer não rastreada pelo Git). Com `-CopyToClipboard`, o resumo
também vai para o clipboard, facilitando colar o resultado na conversa.
