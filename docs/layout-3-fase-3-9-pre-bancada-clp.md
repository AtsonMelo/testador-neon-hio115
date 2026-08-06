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

## Inventario tecnico

| Item | Status | Evidencia e decisao |
|---|---|---|
| Modelo exato do CLP | `CONFLITANTE` | `docs/estado-atual.md` registra `NEON5-1S/CPU450`; o catalogo registra `NEON-1S/CPU401` e `RION-502/CPU502`. A unidade alvo nao foi identificada por etiqueta. |
| Modelo do modulo | `PENDENTE` | HIO115 e a referencia principal do projeto, mas DIO605 aparece no conjunto NEON-1S. Confirmar a unidade fisica e o slot. |
| NEON/RION/HIO115 | `CONFLITANTE` | Existem combinacoes historicas distintas. Nenhuma combinacao foi selecionada para este teste. |
| Protocolo | `PENDENTE` | O app legado implementa Modbus RTU; o catalogo tambem lista perfis genericos RS-232, RS-485 e Modbus TCP reservado. Confirmar no equipamento/programa carregado. |
| Topologia | `PENDENTE` | Serial direto, RS-485, radio transparente e Ethernet aparecem como possibilidades, sem definicao da bancada alvo. |
| IP | `NÃO DISPONÍVEL` | Nenhum IP aprovado do CLP ou do PC esta registrado. |
| Porta TCP | `NÃO DISPONÍVEL` | `502` aparece apenas em perfil generico reservado, com implementacao TCP ausente. Nao usar como parametro real. |
| Porta serial | `NÃO DISPONÍVEL` | `COM1` e default de software, nao evidencia fisica. |
| Baud rate | `CONFLITANTE` | O codigo possui default 9600; o catalogo oferece 38400 e 57600 como perfis pendentes. |
| Paridade | `PENDENTE` | `None` aparece como default/perfil generico, sem confirmacao da unidade. |
| Stop bits | `PENDENTE` | `One` aparece como default/perfil generico, sem confirmacao da unidade. |
| Endereco do dispositivo / Unit ID | `PENDENTE` | Slave `1` e default de software; o historico exige confirmar o endereco. |
| Firmware | `PENDENTE` | Ha referencia `G5PLC.C950.ST 3.3.10` para um conjunto historico, mas a unidade alvo nao foi identificada. |
| Mapa de registradores | `CONFLITANTE` | O fluxo legado usa `%MW10..74`; o perfil HIO115 tambem registra SysVars `1120..1134`. Falta mapa aprovado para o conjunto e programa reais. |
| Registradores somente leitura | `NÃO DISPONÍVEL` | Nenhuma allow-list foi aprovada. Enderecos de comando/escrita do fluxo legado sao proibidos nesta fase. |
| Tensao | `PENDENTE` | A documentacao cita cuidados com 24 V, mas nao confirma a alimentacao da bancada alvo. |
| Alimentacao | `PENDENTE` | Confirmar fonte, polaridade, protecao e grupos de alimentacao no equipamento real. |
| Aterramento | `NÃO DISPONÍVEL` | Sem evidencia registrada para a bancada alvo. |
| Isolamento das saidas | `PENDENTE` | Deve ser confirmado fisicamente antes de qualquer conexao. |
| Estado da maquina | `NÃO DISPONÍVEL` | Sem registro de bloqueio/impedimento de operacao. |
| Responsavel da bancada | `NÃO DISPONÍVEL` | Nome e funcao ainda nao informados. |
| Desconexao rapida | `NÃO DISPONÍVEL` | Meio, responsavel e tempo de retirada ainda nao definidos. |

## Arquitetura local

O validador usa somente:

1. `Data/Bench/layout-3-bench-readiness.json`;
2. existencia dos seis documentos obrigatorios;
3. avaliacao pura de campos, aprovacao, allow-list e limites;
4. cenarios sinteticos em memoria.

O namespace novo nao referencia `Plc`, o servico de comunicacao, o bridge
legado nem bibliotecas de transporte. O despacho em `Program.cs` retorna antes
da inicializacao da UI.

### Flags

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release -- --validate-layout-3-bench-readiness-self-tests
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -c Release -- --validate-layout-3-bench-readiness
```

A primeira flag valida onze cenarios sinteticos. A segunda tambem avalia a
configuracao versionada e retorna codigo diferente de zero enquanto qualquer
dado estiver ausente, pendente ou conflitante.

## Regras fail-closed

- schema desconhecido bloqueia;
- equipamento, firmware ou etiqueta ausentes bloqueiam;
- protocolo e parametros incompativeis bloqueiam;
- protocolo nao suportado pelo preflight bloqueia;
- mapa ausente ou allow-list vazia bloqueiam;
- area `coil` ou acesso diferente de `read` bloqueiam;
- timeout fora de 100 a 5000 ms bloqueia;
- limite de leituras nulo, negativo ou superior a allow-list bloqueia;
- feature flag, comunicacao, escrita, polling ou reconexao ligados bloqueiam;
- politica diferente de single-shot bloqueia;
- seguranca eletrica ou de bancada nao confirmada bloqueia;
- documento obrigatorio ausente bloqueia;
- qualquer status diferente de `CONFIRMADO` bloqueia.

Os limites do preflight sao controles de software. Eles nao definem os valores
reais do equipamento.

## Estado atual

`STATUS: NOT READY - GATE C BLOQUEADO`

Motivos principais:

- identidade do equipamento conflitante;
- protocolo e topologia pendentes;
- parametros de conexao nao confirmados;
- mapa sem aprovacao para o conjunto real;
- allow-list read-only vazia;
- dados eletricos e responsavel ausentes.

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
4. Gate D: autorizar, em milestone e branch separadas, o desenho do transporte
   real read-only ainda sem teste fisico.
5. Somente depois de build, testes com fake server, revisao e autorizacao final
   sera possivel apresentar um comando real de teste.

Esta fase nao implementa o Gate D e nao autoriza conectar o CLP.
