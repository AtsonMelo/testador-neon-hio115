# Layout 3 — roteiro visual do host read-only

## Objetivo

Roteiro manual de verificação visual do Host Layout 3 e dos previews isolados,
usando **apenas as flags reais da base operacional**. Este roteiro é estritamente
visual e local: não abre conexão, não lê CLP, não escreve registrador e mantém os
comandos físicos em zero.

## Flags reais (UI)

As únicas flags de UI válidas na base são:

- `--layout-3-host-readonly` — abre o Host Layout 3 read-only;
- `--preview-layout-3` — preview isolado, paleta dark aprovada;
- `--preview-layout-3-light` — preview isolado, paleta clara técnica;
- `--preview-layout-3-auto` — preview isolado, paleta conforme a preferência do
  Windows.

Sem flag, o aplicativo continua abrindo o fluxo de produção do `MainForm`.

### Flags que NÃO existem (não citar)

Os nomes abaixo já foram citados por engano em rascunhos e **não existem** no
`Program.cs`. Não devem aparecer em nenhum roteiro:

- `--host-layout-3-readonly`;
- `--host-layout-3-profile-selection-readonly`;
- `--preview-layout-3-host`.

Padronizar nomes de flags, se desejado, fica para uma milestone própria. Esta
entrega não cria alias nem renomeia argumentos.

## Roteiro visual do host read-only

Abrir o host read-only:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --layout-3-host-readonly
```

Verificar visualmente, sem acionar nada:

- a janela abre separada do `MainForm`, identificada como host read-only;
- a seleção de família, modelo, módulo, comunicação e perfil é apenas
  informativa, vinda do catálogo local;
- o painel do estado de comunicação reflete a seleção, sempre desconectado;
- o painel do **bridge de leitura** aparece como **PREPARADO / DESLIGADO**, sem
  leitura real, conexão ativa ou contador diferente de zero;
- o painel do **gate de ativação** aparece como **BLOQUEADO**, com os requisitos
  futuros pendentes e nenhuma ativação concedida;
- fechar a janela não executa leitura, escrita, conexão ou desconexão física.

## Hierarquia visual e numeração dos painéis

O host read-only organiza os blocos em uma sequência numerada coerente, de cima
para baixo, sempre em estado seguro:

- **01 — HOST / COMUNICACAO**: diagnóstico local read-only do estado de
  comunicação; sempre desconectado, sem conexão física criada;
- **02 — I/O MANUAL**: painel industrial em estado seguro, apenas visual;
- **03 — PERFIL / SELECAO LOCAL**: seleção de família/modelo/módulo/teste a partir
  do catálogo local; 0 comandos físicos;
- **04 — BRIDGE DE LEITURA — PREPARADO / DESLIGADO**: bridge no-op, sem leitura
  real, sem conexão ativa e com todos os contadores em zero;
- **05 — GATE DE ATIVACAO — BLOQUEADO**: nenhuma ativação concedida, requisitos
  futuros pendentes;
- **06 — LOG LOCAL DO HOST**: registro local read-only, sem escrita física e com
  0 comandos físicos.

O topo do host reforça as garantias em três indicadores: estado de comunicação,
**MODO READ-ONLY / SEM ESCRITA FISICA** e **0 COMANDOS FISICOS / SEM CONEXAO
FISICA**. Nenhum botão da tela executa acionamento físico real; o bridge
permanece desligado/no-op e o gate permanece bloqueado.

## Roteiro visual dos previews isolados

Paleta dark aprovada:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --preview-layout-3
```

Paleta clara técnica:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --preview-layout-3-light
```

Paleta automática, seguindo a preferência de aplicativos do Windows:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --preview-layout-3-auto
```

As três flags abrem o mesmo layout isolado; mudam apenas a paleta e o tratamento
da barra de título. Detalhes da estrutura e das garantias de isolamento estão em
[layout-3-preview-isolado.md](layout-3-preview-isolado.md).

## Validadores não visuais (confirmação local)

Os validadores são locais, não visuais e falham se detectarem conexão, leitura,
escrita ou comando físico diferente de zero:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release --no-build -- --validate-hardware-profile-selection
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release --no-build -- --validate-hardware-test-report
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release --no-build -- --validate-layout-3-host-readonly-safety
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release --no-build -- --validate-layout-3-read-bridge-disabled
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release --no-build -- --validate-layout-3-read-bridge-activation-gate
```

O validador `--validate-layout-3-read-bridge-activation-gate` pertence à Fase 3.6
e existe enquanto a branch dessa milestone não for integrada à base.

## Garantias

- conexão ativa: não;
- leituras reais: 0;
- escritas reais: 0;
- comandos físicos executados: 0;
- nenhum Serial/TCP/Socket/Modbus é criado por este roteiro;
- `MainForm.cs`, artefatos HIstudio e mapas/registradores permanecem fora do
  escopo.
