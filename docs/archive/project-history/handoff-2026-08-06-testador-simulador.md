# Handoff Testador e Simulador

Data: 2026-08-06.

## Status

- Software RTU offline: `OFFLINE_READY`.
- Motor e integração: `SIMULATION_READY`.
- Bancada física: `BENCH_PREP_REQUIRED`.
- COM8: não autorizada e não acessada.
- Conexões/leitura/escrita/comandos físicos: `0/0/0/0`.

O preflight físico permanece corretamente bloqueado. Este checkpoint não é
`READY_FOR_BENCH`.

## Git e GitHub

Branch da Fase 3.9:

```text
prep/layout-3-fase-3-9-pre-bancada-clp-readonly
174ecc83d4d557651db0e302aa3845b2136f5c02
PR #57 Draft
https://github.com/AtsonMelo/testador-neon-hio115/pull/57
```

Branch empilhada:

```text
feat/testador-simulador-foundation
checkpoint funcional validado: cddd6e7bd6f22478c1ed7320389b519f3f6138c6
PR #58 Draft
https://github.com/AtsonMelo/testador-neon-hio115/pull/58
```

Commits preservados desde `e565c71`:

```text
b4ddb1d feat(layout3): registra evidencias fisicas fail-closed
174ecc8 feat(layout3): consolida fluxo RTU e teste supervisionado de saidas
e11f37c feat(layout3): adiciona gate D RTU estritamente offline
d343390 feat(simulator): cria motor generico com perfis de pivo e poco
041fcf0 feat(simulator): integra testador RTU ao processo virtual
1fa7a88 feat(platform): adiciona UX local de testador e simulador
1b19a90 docs: registra arquitetura industrial de teste e simulacao
cddd6e7 test(platform): mede ciclo simulado canonico
```

O commit que contém este handoff é posterior ao checkpoint funcional acima. Use
`git rev-parse HEAD` para obter o SHA final da branch.

## Implementação

### Testador

- codec interno CRC16, FC03 e FC06 mínimo;
- `IRtuTransport` com única implementação nova em memória;
- fake NEON5/CPU450/HIO115 configurável;
- descoberta `1..247`, uma tentativa por endereço, cancelável;
- identificação por `PROG_ID`, `PROG_CRC` e F21;
- DI00..DI07 e AI00..AI02 read-only;
- DO00..DO03 por enum fechado e ciclo momentâneo;
- PWM, reservados e endereço arbitrário bloqueados;
- gate simulado separado do gate físico;
- log estruturado e contadores separados.

### Simulador

- `SimulationEngine` genérico e declarativo;
- perfil Pivô Central;
- perfil Poço;
- cenários, regras, alarmes, faixas e outputs exclusivos via JSON;
- nenhum timer agressivo ou processo de background;
- valores de processo marcados como simulados.

### Integração

`SimulationHio115Adapter` sincroniza sinais do processo com o fake HIO115. O
Testador observa DI/AI por FC03 e DO tipada retorna ao processo por evento, tudo
em memória.

### UI

`--industrial-platform` abre um host novo com seleção `TESTADOR` e `SIMULADOR`.
Os controles são carregados sob demanda e compartilham a mesma sessão fake.

O Testador contém configuração RTU editável, discovery, identificação, abas de
entradas, saídas, diagnóstico e log. `Atualizar portas` permanece desabilitado.

O Simulador seleciona Pivô/Poço, aplica cenários e edita entradas digitais e
analógicas. Saídas virtuais e alarmes são atualizados pelo engine.

A UI foi construída e descartada pelo validator `11/11`. Não houve abertura
manual da janela nem captura visual neste checkpoint, para evitar deixar um
processo gráfico ativo sem supervisão.

## Validação final

```text
git diff --check: OK
Build Release: 0 warnings, 0 errors
Cinco validadores históricos: 5/5
Catálogo de hardware: OK
Bench readiness self-tests: 59/59
Documentos físicos obrigatórios: 7/7
RTU offline: 45/45
Simulation Engine: 22/22
Integração Testador-Simulador: 12/12
UI industrial: 11/11
Preflight físico: exit code 2 esperado
Pendências físicas: 24
```

Ciclo canônico do validator da UI:

```text
SimulatedConnections: 1
SimulatedReads: 5
SimulatedWrites: 2
SimulatedCommands: 2
SimulationOperations: 2
SimulationCommands: 2
PhysicalConnections: 0
PhysicalReads: 0
PhysicalWrites: 0
PhysicalCommands: 0
```

## Comandos

Build:

```powershell
dotnet build app/TestadorCLPHI.App/TestadorCLPHI.App.csproj -c Release
```

Abrir a plataforma offline:

```powershell
dotnet run --project app/TestadorCLPHI.App/TestadorCLPHI.App.csproj -c Release -- --industrial-platform
```

No primeiro painel, selecione `SIMULADOR`. O perfil inicial é Pivô Central. Use
o seletor de perfil para carregar Poço. Para enxergar os mesmos sinais pelo
fake, alterne para `TESTADOR`, identifique o endereço `1` e leia as entradas.

Validadores novos:

```powershell
$dll = ".\app\TestadorCLPHI.App\bin\Release\net8.0-windows\TestadorCLPHI.App.dll"
dotnet $dll --validate-layout-3-rtu-offline
dotnet $dll --validate-layout-3-simulation-engine
dotnet $dll --validate-layout-3-test-simulator-integration
dotnet $dll --validate-layout-3-industrial-platform-ui
```

## Riscos e dívida técnica

- [Certo] `System.IO.Ports` e um serviço serial genérico são legado anterior.
  O startup não o instancia; a nova plataforma não o referencia.
- [Bloqueado] Não existe transporte serial real na nova plataforma.
- [Bloqueado] Identidade OMNI-PLC2 / NEON5-1S continua sem equivalência OEM
  documental.
- [Bloqueado] Protocolo/canal real, evidências elétricas e autorização física
  continuam pendentes.
- [Incerto] F10/F11 aguardam mapa documental versionado.
- [Certo] A troca de perfil recria a sessão e zera estado/log simulado.
- [Certo] Não houve teste visual humano neste checkpoint; somente QA estrutural
  WinForms.

Durante a validação, três processos de harness ficaram presos por uso incorreto
do apphost WinExe. Os PIDs foram identificados e encerrados com autorização
explícita. A causa foi corrigida executando validators pelo host `dotnet` e
isolando continuações assíncronas. O build completo foi repetido depois.

## Próxima ação

1. Revisar os Draft PRs #57 e #58 sem merge automático.
2. Fazer teste visual humano somente do modo offline `--industrial-platform`.
3. Manter COM8 desconectada até resolver as 24 pendências e aprovar um gate
   específico para transporte serial real.
4. Planejar `RealSerialRtuTransport` em branch própria, com feature flag OFF,
   fake obrigatório e revisão da dependência legada.

## Decisão operacional

PODE ATSON ABRIR E TESTAR O SIMULADOR AGORA? **SIM**, no modo offline indicado.

PODE ATSON CONECTAR A COM8 AGORA? **NÃO**.
