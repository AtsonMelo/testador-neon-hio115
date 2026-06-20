# Fase 3.4 — Validador de seguranca read-only do Host Layout 3

## Objetivo

Garantir, de forma automatica e **nao visual**, que o Host Layout 3 read-only
continua seguro: o estado de comunicacao exibido deve ser derivado apenas do
catalogo local e da selecao de perfil, sem qualquer conexao fisica, sem leitura
ou escrita real de registrador e sem nenhum comando fisico.

Esta fase nao altera comportamento de runtime do app: adiciona apenas uma flag
de linha de comando que executa a verificacao e encerra.

## Escopo

Incluido:

- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3HostReadOnlySafetyValidator.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3HostReadOnlySafetyValidationResult.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3HostReadOnlySafetyValidationScenario.cs`
- `app/TestadorCLPHI.App/Program.cs` — apenas registro da nova flag de validacao
  nao visual, sem alterar o fluxo normal do app.
- `docs/layout-3-fase-3-4-readonly-safety-validation.md`

Fora de escopo (inalterado): `MainForm.cs`, logica real de Modbus/Serial/PLC,
mapas/registradores, comandos fisicos, arquivos `.dpk/.dmf/.hst/.prj` e
`TESTADOR_NEON_HIO115_1_1`.

## Por que a validacao e segura

A verificacao reaproveita exatamente os mesmos tipos read-only ja usados pela UI:

- `Layout3ProfileSelection` resolve familia/modelo/modulo/comunicacao **a partir
  do catalogo em memoria**, sem abrir transporte.
- `Layout3CommunicationState.FromSelection` produz uma fotografia imutavel do
  estado, com `RealConnectionAttempts = 0` e `PhysicalCommandsExecuted = 0`
  fixados por construcao.
- `Layout3ReadOnlyCommandGuard` apenas **avalia** uma intencao sintetica e
  sempre a bloqueia; nada e transmitido.

Nao ha instanciacao de cliente TCP, nao ha abertura de porta serial, nao ha
servico PLC, nao ha Modbus e nenhum registrador e lido ou escrito. O validador
nao constroi nem inicia nenhum canal real.

## O que e validado

Para cada cenario o validador verifica:

- os perfis conhecidos do catalogo local (uma familia por cenario, em ordem);
- estado de comunicacao calculado sem conexao fisica;
- origem do estado como catalogo/local/sem hardware;
- protocolo exibido como dado de catalogo, nunca como conexao ativa;
- tentativas reais de conexao iguais a 0;
- comandos fisicos executados iguais a 0;
- status read-only preservado (conjunto local: nao conectada, local read-only,
  simulada, bloqueada);
- coerencia entre presenca de hardware e estado derivado;
- mensagem operacional confirmando ausencia de canal aberto;
- cenarios pendentes (`NEON_5_CONTROLLER`, `RION_5_CONTROLLER`) nao causam erro
  nem conexao;
- guard read-only bloqueando qualquer intencao de comando fisico;
- nenhum cenario indicando conexao ativa real.

## O que continua proibido

- abrir conexao real;
- instanciar cliente TCP;
- abrir porta serial;
- usar Modbus;
- instanciar servico PLC;
- ler registrador;
- escrever registrador;
- enviar comando fisico;
- alterar `MainForm.cs`, mapas/registradores ou arquivos de projeto legados.

## Como executar

Verificacao isolada:

```
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-layout-3-host-readonly-safety
```

Build:

```
dotnet build .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj
```

QA local (branch empilhada sobre o PR #44):

```
.\tools\qa\Invoke-TestadorQa.ps1 -Mode Branch -BaseBranch ui/layout-3-host-communication-state-readonly -ExpectedBranch ui/layout-3-host-readonly-safety-validation -RunGuiSmoke -CopyToClipboard
```

## Criterio de aceite

- novo validador retorna `Resultado: OK` e codigo de saida 0;
- validadores existentes continuam OK;
- build OK;
- `git diff --check` OK;
- smoke GUI 4/4;
- arvore de trabalho limpa.

## Comandos fisicos executados

Devem permanecer **0** em todos os cenarios. O validador soma os comandos
fisicos de cada cenario e falha se o total for diferente de 0; a saida sempre
reporta `Comandos fisicos executados: 0`.
