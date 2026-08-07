# Mapeamento industrial de I/O

## Objetivo

O mapeamento conecta nomes de processo a canais técnicos sem substituir a
identidade do CLP. Nesta fase ele descreve somente o fake HIO115 e é sempre
marcado como `MAPEAMENTO DO PERFIL DE SIMULAÇÃO`.

```text
PROCESS SIGNAL
      |
SimulationIoBinding
      |
SimulationHio115Adapter
      |
FAKE HIO115 / registros em memória
      |
InMemoryRtuTransport
      |
TESTER / DIxx + alias do processo
```

Nenhuma linha deste documento confirma ligação elétrica, borne, escala física,
firmware ou programa de um equipamento real.

## Contrato `SimulationIoBinding`

| Campo | Função |
|---|---|
| `signalId` | ID existente em `signals` |
| `signalLabel` | cópia legível validada contra o perfil, quando informada |
| `direction` | `input` ou `output` |
| `ioType` | `digital` ou `analog` |
| `channel` | índice zero-based do canal no fake |
| `register` | referência documentada usada pelo RTU em memória |
| `registerAlias` | identidade técnica `DIxx`, `AIxx` ou `DOxx` |
| `description` | explicação do vínculo conceitual |
| `evidenceStatus` | `simulationProfile` nesta fase |

O modelo é genérico e pertence ao perfil. Ele não contém regras específicas de
Pivô, Poço, bomba ou reservatório.

## Política HIO115 fake

| Tipo | Canais aceitos | Registros aceitos |
|---|---:|---:|
| Entrada digital | DI00..DI07 | 31120..31127 |
| Saída digital | DO00..DO03 | 31128..31131 |
| Entrada analógica | AI00..AI02 | 31132..31134 |

O validador exige correspondência exata entre tipo, canal, alias e registro.
DO04 ou superior, saída analógica, canal negativo, endereço reservado e alias
PWM falham fechados.

## Perfil Pivô Central

| Processo | Canal | Registro | Tipo |
|---|---:|---:|---|
| Emergência | DI00 | 31120 | Entrada digital |
| Pressostato | DI01 | 31121 | Entrada digital |
| Alinhamento | DI02 | 31122 | Entrada digital |
| Fim de curso | DI03 | 31123 | Entrada digital |
| Falha de torre | DI04 | 31124 | Entrada digital |
| Permissivo de água | DI05 | 31125 | Entrada digital |
| Pressão | AI00 | 31132 | Entrada analógica raw simulada |
| Corrente | AI01 | 31133 | Entrada analógica raw simulada |
| Posição | AI02 | 31134 | Entrada analógica raw simulada |
| Bomba | DO00 | 31128 | Saída digital virtual |
| Frente | DO01 | 31129 | Saída digital virtual |
| Reverso | DO02 | 31130 | Saída digital virtual |
| Válvula de água | DO03 | 31131 | Saída digital virtual |

DI06 e DI07 permanecem sem binding. Velocidade permanece no processo, sem canal
analógico livre no fake atual.

## Perfil Poço

| Processo | Canal | Registro | Tipo |
|---|---:|---:|---|
| Nível mínimo | DI00 | 31120 | Entrada digital |
| Nível máximo | DI01 | 31121 | Entrada digital |
| Falta de fase | DI02 | 31122 | Entrada digital |
| Pressostato | DI03 | 31123 | Entrada digital |
| Emergência | DI04 | 31124 | Entrada digital |
| Sensor inválido | DI05 | 31125 | Entrada digital |
| Nível | AI00 | 31132 | Entrada analógica raw simulada |
| Pressão | AI01 | 31133 | Entrada analógica raw simulada |
| Corrente | AI02 | 31134 | Entrada analógica raw simulada |
| Bomba | DO00 | 31128 | Saída digital virtual |
| Válvula | DO01 | 31129 | Saída digital virtual |

DI06, DI07, DO02 e DO03 permanecem sem binding no Poço.

## Validação

`--validate-industrial-io-mapping` verifica:

- existência de todos os `SignalId`;
- uma única associação por sinal e por canal/tipo/direção;
- DI somente para `DigitalInput`;
- AI somente para `AnalogInput`;
- DO somente para `VirtualOutput`;
- DO limitado a DO00..DO03;
- alias e registro exatos para o canal conhecido;
- registro dentro da allow-list;
- ausência de reservado e PWM;
- `evidenceStatus = simulationProfile`.

Resultado esperado: `IO_MAPPING_READY`, com todos os contadores físicos em
zero.

## Estado

- contrato e perfis: `STRUCTURALLY_VALIDATED`;
- validação física: não executada e não autorizada;
- revisão visual da tabela: `AWAITING_HUMAN_VISUAL_REVIEW`.
