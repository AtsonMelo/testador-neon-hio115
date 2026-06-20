# Fase 3.5 — Bridge de leitura preparado e desligado (Host Layout 3)

## Objetivo

Preparar a arquitetura para um futuro bridge de leitura real do Host Layout 3,
mantendo tudo **100% desligado, local e seguro** nesta fase. A milestone cria
apenas contrato e estrutura: nenhum codigo conecta, le CLP, abre porta, fala
protocolo de campo ou executa comando fisico.

Esta fase NAO conecta em nada, NAO le CLP, NAO abre porta, NAO usa protocolo
real e NAO executa comando fisico.

## Escopo

Incluido:

- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/ILayout3ReadBridge.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeStatus.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeSnapshot.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3DisabledReadBridge.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgePanelControl.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeSafetyValidator.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeSafetyValidationResult.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3HostControl.cs` — integra o
  painel do bridge ao host read-only e atualiza o snapshot quando o perfil muda.
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3HostForm.cs` — apenas ajuste
  de tamanho da janela para acomodar o novo painel compacto.
- `app/TestadorCLPHI.App/Program.cs` — apenas registro da nova flag de validacao
  nao visual, sem alterar o fluxo normal do app.
- `docs/layout-3-fase-3-5-read-bridge-disabled.md`

Fora de escopo (inalterado): `MainForm.cs`, logica real de Modbus/Serial/PLC,
mapas/registradores, comandos fisicos, arquivos `.dpk/.dmf/.hst/.prj` e
`TESTADOR_NEON_HIO115_1_1`.

## Arquitetura criada

A camada de bridge e apenas contrato/estrutura, projetada para que a troca por
uma implementacao real (em milestone futura, com aprovacao explicita) seja
trivial sem reescrever a UI:

- `ILayout3ReadBridge` — contrato. Dada uma selecao local de perfil, devolve um
  snapshot imutavel. Nada no contrato autoriza abrir canal ou ler/escrever.
- `Layout3ReadBridgeStatus` — enum de estado. Nesta fase o unico valor e
  `Disabled` (exibido como "Desligado"); permanece extensivel para estados
  ativos futuros, mas nenhum valor atual representa canal aberto.
- `Layout3ReadBridgeSnapshot` — fotografia imutavel: estado, modo, origem,
  conexao ativa, leituras reais, escritas reais, comandos fisicos, contexto de
  perfil/protocolo e mensagem. Por construcao a fabrica `Disabled(...)` fixa
  conexao ativa = false e todos os contadores reais em 0.
- `Layout3DisabledReadBridge` — unica implementacao desta fase. No-op: nunca abre
  conexao, nunca le, nunca escreve, nunca envia comando. Apenas monta o snapshot
  local a partir do catalogo/selecao em memoria.
- `Layout3ReadBridgePanelControl` — painel estritamente visual; recebe um
  snapshot pronto e nao conhece nem instancia qualquer canal ou servico.
- `Layout3ReadBridgeSafetyValidator` + `Layout3ReadBridgeSafetyValidationResult`
  — verificacao nao visual de que o bridge nasce e permanece desligado.

## O que esta preparado

- contrato e tipos locais para um futuro bridge de leitura;
- estado visual do bridge no host read-only, reagindo a selecao de perfil apenas
  como contexto informativo (perfil e protocolo do catalogo);
- ponto de extensao unico (`ILayout3ReadBridge`) para uma implementacao real
  futura, sem que ela exista agora;
- validacao automatica que protege o invariante "bridge desligado".

## O que permanece desligado

- conexao ativa: nao;
- leituras reais: 0;
- escritas reais: 0;
- comandos fisicos: 0;
- modo: disabled / no-op / local;
- origem: arquitetura preparada;
- mensagem: "Bridge preparado, mas bloqueado/desligado nesta fase."

O painel deixa explicito que nao ha leitura real, nao ha conexao aberta, o bridge
e apenas contrato/estrutura e a ativacao real exigira milestone futura e
aprovacao explicita.

## Por que e seguro

- a unica implementacao e no-op e nao possui caminho de codigo que crie
  transporte;
- o snapshot fixa conexao ativa = false e os contadores reais em 0 por
  construcao, independente da selecao;
- a selecao de perfil entra somente como informacao local; nunca dispara conexao;
- nao ha instanciacao de cliente TCP, abertura de porta serial, servico PLC,
  Modbus ou leitura/escrita de registrador;
- o validador nao visual reexecuta o bridge para todos os perfis do catalogo e
  falha se qualquer contador real, conexao ou estado divergir do desligado.

## Restricoes tecnicas respeitadas

Nao foi usado: porta serial (`System.IO.Ports`), cliente TCP, soquete de rede,
bibliotecas Modbus (EasyModbus / N-Modbus), leitura/escrita de registrador,
timer de polling real, thread/worker de comunicacao real ou qualquer conexao
fisica.

## Como executar o validador

Verificacao do bridge desligado:

```
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-layout-3-read-bridge-disabled
```

Verificacao read-only anterior (Fase 3.4):

```
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-layout-3-host-readonly-safety
```

Abertura visual do host read-only:

```
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --layout-3-host-readonly
```

Build:

```
dotnet build .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj
```

QA local (branch empilhada):

```
.\tools\qa\Invoke-TestadorQa.ps1 -Mode Branch -BaseBranch ui/issue-24-liga-io-industrial-host -ExpectedBranch ui/layout-3-read-bridge-disabled -RunGuiSmoke -CopyToClipboard
```

## O que o validador verifica

- o bridge inicia desligado (antes de qualquer cenario);
- para cada perfil conhecido do catalogo: conexao ativa = false, leituras reais
  = 0, escritas reais = 0, comandos fisicos = 0;
- modo declarando natureza desligada/no-op/local;
- mensagem confirmando desligado/bloqueado, nunca canal aberto;
- cenarios pendentes `NEON_5_CONTROLLER` e `RION_5_CONTROLLER` nao ativam nada;
- nenhum cenario indicando conexao ativa;
- totais de leituras, escritas e comandos fisicos iguais a 0.

## Criterio de aceite

- novo validador retorna `Resultado: OK` e codigo de saida 0;
- validador read-only anterior continua OK;
- validadores existentes continuam OK;
- build OK;
- `git diff --check` OK;
- smoke GUI 4/4;
- comandos fisicos executados: 0;
- arvore de trabalho limpa.

## Proximos passos futuros

- definir, em milestone separada e com aprovacao explicita, uma implementacao
  real de `ILayout3ReadBridge` (somente leitura), iniciando desligada por padrao;
- so entao introduzir estados ativos em `Layout3ReadBridgeStatus`;
- manter o validador como rede de seguranca: a implementacao real deve continuar
  desligada por padrao ate liberacao consciente.

## Comandos fisicos executados

Devem permanecer **0**. O validador soma leituras, escritas e comandos fisicos de
todos os cenarios e falha se qualquer total for diferente de 0; a saida sempre
reporta `Comandos fisicos executados: 0`.
