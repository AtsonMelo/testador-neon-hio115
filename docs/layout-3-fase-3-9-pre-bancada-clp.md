# Layout 3 - Fase 3.9 - Preparacao segura para bancada CLP

## Objetivo

Preparar o Layout 3 para uma futura leitura supervisionada sem abrir qualquer
comunicacao real nesta fase. A entrega consolida inventario, checklist, plano de
rollback, matriz de riscos, ficha de parametros, plano de evidencias e um
validador local fail-closed.

Esta fase nao autoriza conexao TCP ou serial, descoberta de portas, DNS, ping,
Modbus, polling, leitura de registrador, escrita ou comando fisico.

## Ponto de partida

- branch base: `ui/issue-24-liga-io-industrial-host`;
- commit base: `8dd0af9eb209f4f8f5682c9318ad1a55a34a6634`;
- checkpoint: `layout-3-fase-3-8-polimento-visual-completo-host-ok-20260806`;
- branch da fase: `prep/layout-3-fase-3-9-pre-bancada-clp-readonly`;
- PR #56 e a evolucao documental da `main` permanecem fora desta branch.

## Fontes auditadas

Foram auditados o conteudo atual, a documentacao, o catalogo local, o perfil
HIO115, o codigo de comunicacao legado e o historico Git. Os principais pontos
de evidencia sao:

- `docs/estado-atual.md`;
- `docs/pendencias-validacao-hardware.md`;
- `docs/matriz-modelos-rion-neon.md`;
- `docs/variaveis-geradas-hio115.md`;
- `profiles/hio115.json`;
- `app/TestadorCLPHI.App/Data/Hardware/hi-hardware-catalog.json`;
- `app/TestadorCLPHI.App/Plc/Hio115MemoryMap.cs`;
- commits historicos que introduziram configuracao serial e mapas do fluxo
  legado.

Defaults de software e perfis genericos nao sao confirmacao do equipamento
fisico. Nenhum valor foi promovido por semelhanca de nome ou conveniencia.

Em evidencias posteriores, o operador forneceu dados observados diretamente no
HIstudio. Esses dados confirmam identidade parcial, inventario das interfaces,
capacidades declaradas dos modulos, configuracao do canal serial e programa
observado, mas nao confirmam a interface fisica efetivamente usada, protocolo,
firmware especifico da CPU, mapa, allow-list ou condicoes seguras de bancada.

Uma inspecao frontal posterior identificou `OMNI-PLC2` da `OMNICONTROL`, em
conflito com `NEON5-1S / CPU450 / HIO115` no HIstudio. A relacao entre essas
identidades nao foi confirmada documentalmente e nenhuma equivalencia foi
deduzida.

As capturas mais recentes tambem mostram `Equipamento remoto offline` e
`Nao existe base de hardware definida no ambiente`. Portanto, estados atuais
de entradas e saidas, valores analogicos, contadores e PWM nao foram promovidos
a evidencia fisica.

## Inventario tecnico

| Item | Status | Evidencia e decisao |
|---|---|---|
| Identidade do equipamento | `CONFLITANTE` | HIstudio: `NEON5-1S / CPU450 / HIO115`; frontal: `OMNICONTROL OMNI-PLC2`. A relacao entre as identidades e `NÃO CONFIRMADA`. |
| Identificacao frontal | `OBSERVADO` | `PIVODRIP`; fabricante/marca `OMNICONTROL`; modelo `OMNI-PLC2`; serie `111.20023`; part number `300.111.622.801`. |
| Quantidade de modulos | `CONFIRMADO` | Maximo exibido: 2; modulos detectados: 2. |
| Modelo do modulo no HIstudio | `CONFIRMADO` | Evidencia direta do HIstudio: `HIO115` no slot 1. A identificacao frontal `Slot 1115` e compativel, mas nao e prova definitiva. |
| NEON/RION/HIO115 | `CONFLITANTE` | O conjunto `NEON5-1S / CPU450 / HIO115` foi observado no HIstudio, mas ainda nao foi documentalmente relacionado ao frontal `OMNI-PLC2`. |
| Interfaces disponiveis | `CONFIRMADO` | `ITF-A1` e `ITF-A2`: RS232-C; `ITF-B`: RS485. O perfil atual usa RS232 em A1 ou A2; falta diferenciar fisicamente qual delas e registrar o cabo. |
| Estado observado da CPU | `CONFIRMADO` | Revisao de hardware exibida 1, revisao de firmware do modulo exibida 0, funcional operacional e sem falhas de inicializacao, operacao, intermitencia ou configuracao. Nao confirma o firmware da CPU. |
| Capacidades observadas do HIO115 | `CONFIRMADO` | Revisoes de hardware/firmware exibidas 0/0, funcional operacional, 8 DI `I00-I07`, 4 DO `O00-O03`, 3 AI `AI00-AI02`, 3 FCT `FCT0-FCT2`, 1 PWM `PWM00`; apresentacao analogica 4-20 mA. |
| Estados e valores atuais de I/O | `NÃO DISPONÍVEL` | Capturas com equipamento remoto offline e sem base de hardware definida; nenhum estado ou valor foi confirmado fisicamente. |
| HIstudio | `CONFIRMADO` | Versao observada: `2.4.03`. |
| Protocolos previstos pelo testador | `CONFIRMADO` | Selecao explicita entre Modbus RTU e Modbus TCP; nenhuma opcao conecta automaticamente. |
| Protocolo ativo no canal atual | `PENDENTE` | O transporte observado e serial, mas ainda falta confirmar que Modbus RTU esta realmente ativado no canal. |
| Perfil fisico atual | `CONFIRMADO` | Serial RS-232 por `ITF-A1` ou `ITF-A2`; ambas sao RS232-C. A interface exata nao foi identificada e permanece pendencia documental, sem bloquear o perfil de software. |
| Conector atual | `CONFIRMADO` | DB9 frontal identificado como `Serial`. Bornes RS-485 `D+ / D-` e chave de terminacao existem separadamente, mas nao descrevem o cabo atual. |
| IP | `NÃO DISPONÍVEL` | Nenhum IP aprovado do CLP ou do PC esta registrado. |
| Porta TCP | `NÃO DISPONÍVEL` | Nenhuma porta foi confirmada. Nao preencher por default. |
| Driver/canal serial | `CONFIRMADO` | `SERIAL_DRIVER` / `Channel_01`. |
| Porta serial | `CONFIRMADO` | `COM8`. O default `COM1` do codigo nao foi usado. |
| Baud rate | `CONFIRMADO` | `38400`. |
| Data bits | `CONFIRMADO` | `8`. |
| Paridade | `CONFIRMADO` | Nenhuma (`None`). |
| Stop bits | `CONFIRMADO` | `1` (`One`). |
| Temporizacao serial | `CONFIRMADO` | Entre caracteres: 50 ms; transmissao: 2 ms; remover portadora: 0 ms. Nao confundir com timeout de leitura. |
| Frame/remapeamento | `CONFIRMADO` | Frame maximo: 256; remapeamento de endereco: nao. |
| Endereco do dispositivo / Unit ID | `CONFIRMADO` | Endereco atual conhecido: `10`; usado como valor inicial editavel nos perfis RTU/TCP. |
| Firmware | `PROVÁVEL` | `3.3.11` aparece na barra de status, mas nao ha evidencia especifica da CPU. O campo confirmado permanece vazio. |
| Mapa de registradores | `CONFLITANTE` | O fluxo legado usa `%MW10..74`; o perfil HIO115 tambem registra SysVars `1120..1134`. Falta mapa aprovado para o conjunto e programa reais. |
| Registradores somente leitura | `NÃO DISPONÍVEL` | Nenhuma allow-list foi aprovada. Enderecos de comando/escrita do fluxo legado sao proibidos nesta fase. |
| Indicacao nominal de alimentacao | `OBSERVADO` | Texto frontal registrado literalmente como `1030 VDC`; nao reinterpretar como faixa e nao tratar como medicao. |
| Tensao efetivamente medida | `PENDENTE` | Nenhuma medicao e evidencia correspondente foram registradas. |
| Alimentacao | `PENDENTE` | Confirmar fonte, polaridade, protecao e grupos de alimentacao no equipamento real. |
| Aterramento | `DECLARADO PELO RESPONSÁVEL` | Atson Melo declarou `OK`; falta evidencia verificavel. |
| Isolamento das saidas | `DECLARADO PELO RESPONSÁVEL` | Atson Melo declarou saidas desenergizadas ou isoladas; falta evidencia verificavel. |
| Estado da maquina | `DECLARADO PELO RESPONSÁVEL` | Atson Melo declarou maquina impedida e em estado seguro; falta evidencia verificavel. |
| Responsavel da bancada | `DECLARADO PELO RESPONSÁVEL` | Atson Melo informou estar presente; falta registro de evidencia/assinatura. |
| Emergencia/desconexao rapida | `DECLARADO PELO RESPONSÁVEL` | Declaradas `OK`; faltam identificacao e evidencia registradas. |
| Backup do programa | `DECLARADO PELO RESPONSÁVEL` | Declarado `OK`; caminho, nome, algoritmo e hash permanecem pendentes. |

As faixas `I00-I07`, `O00-O03`, `AI00-AI02`, `FCT0-FCT2` e `PWM00` descrevem
recursos exibidos para o modulo. Elas nao constituem mapa de registradores,
allow-list read-only nem autorizacao para ler, habilitar ou configurar I/O.

## Programa observado no HIstudio

| Campo | Valor | Classificacao |
|---|---|---|
| Condicao | Programa rodando | `CONFIRMADO` como estado observado; nao equivale a estado seguro da maquina |
| Nome | `MOTOR_HIDRO:PROD_NEON5_HIO115` | `CONFIRMADO` |
| Versao | `3220` | `CONFIRMADO` |
| Identificador | `31134` | `CONFIRMADO` |
| CRC | `23248` | `CONFIRMADO` |
| Inicializacao observada | `Cold restart` | `CONFIRMADO` |

## Arquitetura local

O validador usa somente:

1. `Data/Bench/layout-3-bench-readiness.json`;
2. existencia dos sete documentos obrigatorios;
3. avaliacao pura de campos, aprovacao, allow-list e limites;
4. cenarios sinteticos em memoria.

O schema 2 representa perfis `RTU` e `TCP` separados, politica de enderecos e
desenho de descoberta. O perfil selecionado e RTU, mas
`activeProtocolConfirmed = false`. O desenho completo esta em
`docs/layout-3-arquitetura-transporte-readonly-planejada.md`.

O namespace novo nao referencia `Plc`, o servico de comunicacao, o bridge
legado nem bibliotecas de transporte. O despacho em `Program.cs` retorna antes
da inicializacao da UI.

### Flags

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release -- --validate-layout-3-bench-readiness-self-tests
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release -- --validate-layout-3-bench-readiness
```

A primeira flag valida cenarios sinteticos. A segunda tambem avalia a
configuracao versionada e retorna codigo diferente de zero enquanto qualquer
dado estiver ausente, pendente ou conflitante.

## Regras fail-closed

- schema desconhecido bloqueia;
- equipamento, firmware ou etiqueta ausentes bloqueiam;
- protocolo e parametros incompativeis bloqueiam;
- protocolo nao suportado pelo preflight bloqueia;
- protocolo ativo nao confirmado bloqueia;
- perfil RTU exige COM, RS232/RS485, serial, timeout, endereco 1..247, uma
  tentativa e intervalo;
- perfil TCP exige IP, porta, topologia isolada, timeout, endereco 1..247 e uma
  tentativa;
- somente o perfil selecionado e validado; o outro pode permanecer incompleto;
- identidade fisica conflitante bloqueia ate existir relacao documental;
- declaracao do responsavel nao substitui foto, medicao, caminho ou hash;
- endereco 0 e proibido; 248..255 exigem aprovacao avancada manual;
- descoberta permanece OFF, limitada a 1..247 e a uma tentativa por endereco;
- somente FC03 e os pares F12/30012 e F13/30013 podem constar no desenho de
  identificacao; os dois valores devem coincidir;
- mapa ausente ou allow-list vazia bloqueiam;
- area `coil` ou acesso diferente de `read` bloqueiam;
- timeout fora de 100 a 5000 ms bloqueia;
- limite de leituras nulo, negativo ou superior a allow-list bloqueia;
- feature flag, comunicacao, escrita, polling ou reconexao ligados bloqueiam;
- politica diferente de single-shot bloqueia;
- seguranca eletrica ou de bancada nao confirmada bloqueia;
- documento obrigatorio ausente bloqueia;
- qualquer status diferente de `CONFIRMADO` bloqueia.
- Gate D nao autorizado bloqueia.

Os limites do preflight sao controles de software. Eles nao definem os valores
reais do equipamento.

## Estado atual

`STATUS: NOT READY - GATE C E GATE D BLOQUEADOS`

Motivos principais:

- identidade `OMNI-PLC2` versus `NEON5-1S` conflitante e sem relacao documental;
- foto/etiqueta e firmware especifico da CPU ainda nao confirmados;
- protocolo realmente ativo no canal ainda nao confirmado;
- timeout RTU e intervalo entre tentativas ainda nao confirmados;
- IP/porta TCP permanecem vazios e so serao exigidos quando TCP for selecionado;
- mapa sem aprovacao para o conjunto real;
- allow-list read-only vazia;
- seguranca e backup apenas declarados, sem evidencias verificaveis;
- tensao medida e configuracao original de rede do PC ausentes;
- Gate D nao autorizado.

## Invariantes desta fase

- feature flag default: `OFF`;
- comunicacao real default: `OFF`;
- escrita: `OFF`;
- polling: `OFF`;
- reconexao automatica: `OFF`;
- modo: `single-shot only`;
- conexoes reais: `0`;
- leituras reais: `0`;
- escritas reais: `0`;
- comandos fisicos: `0`.

## Gates seguintes

1. Gate C: preencher e confirmar os dados da ficha de parametros.
2. Reexecutar o preflight ate retornar codigo zero.
3. Revisar e aprovar a documentacao e a allow-list.
4. Gate D: autorizar, em milestone e branch separadas, a implementacao do
   transporte real read-only ainda sem teste fisico.
5. Somente depois de build, testes com fake server, revisao e autorizacao final
   sera possivel apresentar um comando real de teste.

Esta fase nao implementa o Gate D e nao autoriza conectar o CLP.
