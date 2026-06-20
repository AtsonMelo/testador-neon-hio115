# Fase 3.6 — Gate de ativacao do bridge de leitura, bloqueado (Host Layout 3)

## Objetivo

Preparar a arquitetura de **decisao de ativacao** para um futuro bridge de leitura
real do Host Layout 3, mantendo tudo **100% bloqueado, local e seguro** nesta fase.
A milestone cria apenas contrato, decisao e requisitos declarativos: nenhum codigo
conecta, le CLP, abre porta, fala protocolo de campo, libera ativacao ou executa
comando fisico.

Esta fase NAO conecta em nada, NAO le CLP, NAO abre porta, NAO usa protocolo real,
NAO libera ativacao e NAO executa comando fisico.

Criterio central: **nenhum perfil pode liberar ativacao real**.

## Escopo

Incluido:

- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeActivationStatus.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeActivationRequirement.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeActivationDecision.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/ILayout3ReadBridgeActivationGate.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3BlockedReadBridgeActivationGate.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeActivationGatePanelControl.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeActivationGateValidator.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3ReadBridgeActivationGateValidationResult.cs`
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3HostControl.cs` — integra o
  painel do gate ao host read-only e reavalia a decisao quando o perfil muda.
- `app/TestadorCLPHI.App/Ui/Industrial/Layout3/Layout3HostForm.cs` — apenas ajuste
  de tamanho da janela para acomodar o novo painel compacto.
- `app/TestadorCLPHI.App/Program.cs` — apenas registro da nova flag de validacao
  nao visual, sem alterar o fluxo normal do app.
- `docs/layout-3-fase-3-6-read-bridge-activation-gate.md`

Fora de escopo (inalterado): `MainForm.cs`, logica real de Modbus/Serial/PLC,
mapas/registradores, comandos fisicos, arquivos `.dpk/.dmf/.hst/.prj` e
`TESTADOR_NEON_HIO115_1_1`.

## Arquitetura criada

A camada de gate e apenas contrato/decisao/estrutura, projetada para que a troca
por um gate que libere leitura real (em milestone futura, com aprovacao explicita)
seja trivial sem reescrever a UI:

- `ILayout3ReadBridgeActivationGate` — contrato. Dada uma selecao local de perfil,
  devolve uma decisao imutavel. Nada no contrato autoriza liberar ativacao, abrir
  canal ou ler/escrever.
- `Layout3ReadBridgeActivationStatus` — enum de estado. Nesta fase o unico valor e
  `Blocked` (exibido como "Bloqueado"); permanece extensivel para estados liberados
  futuros, mas nenhum valor atual autoriza ativacao.
- `Layout3ReadBridgeActivationRequirement` — requisito futuro declarativo
  (titulo, detalhe, atendido). Nesta fase todo requisito nasce e permanece
  `Satisfied = false`; satisfaze-los de verdade exigira milestone futura.
- `Layout3ReadBridgeActivationDecision` — decisao imutavel: estado, ativacao
  liberada, conexao ativa, leituras reais, escritas reais, comandos fisicos,
  contexto de perfil/protocolo, lista de requisitos e mensagem. Por construcao a
  fabrica `Blocked(...)` fixa ativacao liberada = false, conexao ativa = false e
  todos os contadores reais em 0, com os requisitos futuros pendentes.
- `Layout3BlockedReadBridgeActivationGate` — unica implementacao desta fase. No-op:
  nunca libera, nunca abre conexao, nunca le, nunca escreve, nunca envia comando.
  Apenas monta a decisao local a partir do catalogo/selecao em memoria.
- `Layout3ReadBridgeActivationGatePanelControl` — painel estritamente visual;
  recebe uma decisao pronta e nao conhece nem instancia qualquer canal ou servico.
- `Layout3ReadBridgeActivationGateValidator` +
  `Layout3ReadBridgeActivationGateValidationResult` — verificacao nao visual de que
  o gate nasce e permanece bloqueado para todos os perfis do catalogo.

## O que esta preparado

- contrato e tipos locais para um futuro gate de ativacao de leitura;
- decisao visual do gate no host read-only, reagindo a selecao de perfil apenas
  como contexto informativo (perfil e protocolo do catalogo);
- ponto de extensao unico (`ILayout3ReadBridgeActivationGate`) para um gate real
  futuro, sem que ele exista agora;
- lista declarativa de requisitos futuros que precisariam ser cumpridos antes da
  primeira leitura real, todos pendentes nesta fase;
- validacao automatica que protege o invariante "ativacao bloqueada".

## O que permanece bloqueado

- ativacao liberada: nao;
- conexao ativa: nao;
- leituras reais: 0;
- escritas reais: 0;
- comandos fisicos: 0;
- estado: bloqueado;
- requisitos futuros: 5 pendentes / 0 atendidos;
- mensagem: "Ativacao bloqueada nesta fase: nenhum perfil libera conexao ou leitura real."

O painel deixa explicito que nenhum perfil libera ativacao, os requisitos futuros
seguem pendentes e a liberacao real exigira milestone futura e aprovacao explicita.

## Requisitos futuros declarados (todos pendentes)

1. Aprovacao explicita de milestone de leitura real.
2. Referencia oficial dos modelos pendentes validada (`NEON_5_CONTROLLER`,
   `RION_5_CONTROLLER`).
3. Bancada fisica homologada e isolada.
4. Camada de transporte real auditada.
5. Procedimento de leitura somente leitura revisado.

Nenhum desses requisitos executa verificacao fisica nesta fase: sao apenas
declaracoes locais que documentam o que ainda falta.

## Por que e seguro

- a unica implementacao e no-op e nao possui caminho de codigo que crie transporte
  ou libere ativacao;
- a decisao fixa ativacao liberada = false, conexao ativa = false e os contadores
  reais em 0 por construcao, independente da selecao;
- a selecao de perfil entra somente como informacao local; nunca libera o gate;
- nao ha instanciacao de cliente TCP, abertura de porta serial, servico PLC,
  Modbus ou leitura/escrita de registrador;
- o validador nao visual reexecuta o gate para todos os perfis do catalogo e falha
  se qualquer contador real, conexao, ativacao liberada ou estado divergir do
  bloqueado, ou se nenhum requisito futuro estiver pendente.

## Restricoes tecnicas respeitadas

Nao foi usado: porta serial (`System.IO.Ports`), cliente TCP, soquete de rede,
bibliotecas Modbus (EasyModbus / N-Modbus), leitura/escrita de registrador, timer
de polling real, thread/worker de comunicacao real ou qualquer conexao fisica.

## Como executar o validador

Verificacao do gate de ativacao bloqueado:

```
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-layout-3-read-bridge-activation-gate
```

Verificacao do bridge desligado (Fase 3.5):

```
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-layout-3-read-bridge-disabled
```

Verificacao read-only (Fase 3.4):

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
.\tools\qa\Invoke-TestadorQa.ps1 -Mode Branch -BaseBranch ui/issue-24-liga-io-industrial-host -ExpectedBranch ui/layout-3-read-bridge-activation-gate -RunGuiSmoke -CopyToClipboard
```

## O que o validador verifica

- o gate inicia bloqueado (antes de qualquer cenario);
- para cada perfil conhecido do catalogo: ativacao liberada = false, conexao ativa
  = false, leituras reais = 0, escritas reais = 0, comandos fisicos = 0;
- pelo menos um requisito futuro pendente e nenhum requisito atendido;
- mensagem confirmando bloqueio, nunca ativacao concedida;
- cenarios pendentes `NEON_5_CONTROLLER` e `RION_5_CONTROLLER` nao liberam nada;
- nenhum cenario liberando ativacao ou indicando conexao ativa;
- totais de leituras, escritas e comandos fisicos iguais a 0.

## Criterio de aceite

- novo validador retorna `Resultado: OK` e codigo de saida 0;
- validadores das fases 3.4 e 3.5 continuam OK;
- validadores existentes continuam OK;
- build OK;
- `git diff --check` OK;
- smoke GUI 4/4;
- comandos fisicos executados: 0;
- arvore de trabalho limpa.

## Proximos passos futuros

- definir, em milestone separada e com aprovacao explicita, um gate que possa
  liberar ativacao (somente leitura), iniciando bloqueado por padrao e exigindo os
  requisitos futuros atendidos;
- so entao introduzir estados liberados em `Layout3ReadBridgeActivationStatus`;
- manter o validador como rede de seguranca: o gate real deve continuar bloqueado
  por padrao ate liberacao consciente e auditada.

## Comandos fisicos executados

Devem permanecer **0**. O validador soma leituras, escritas e comandos fisicos de
todos os cenarios e falha se qualquer total for diferente de 0; a saida sempre
reporta `Comandos fisicos executados: 0`.
