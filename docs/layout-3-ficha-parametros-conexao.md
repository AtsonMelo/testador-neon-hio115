# Ficha de parametros de conexao - Layout 3 read-only

## Instrucao de preenchimento

Preencher somente a partir de etiqueta, programa carregado, manual aplicavel ou
confirmacao do responsavel da bancada. Registrar a fonte. Nao copiar defaults do
app como se fossem valores reais.

Status permitidos: `CONFIRMADO`, `PENDENTE`, `NÃO DISPONÍVEL`, `CONFLITANTE`.

## Identificacao

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Modelo exato do CLP |  | `CONFLITANTE` | NEON5-1S/CPU450, NEON-1S/CPU401 e RION-502/CPU502 aparecem em fontes diferentes |  |
| CPU |  | `CONFLITANTE` | Ver inventario da Fase 3.9 |  |
| Modelo do modulo |  | `PENDENTE` | HIO115 e DIO605 aparecem em conjuntos diferentes |  |
| Slot do modulo |  | `PENDENTE` | Slot 1 e historico, nao confirmacao da unidade |  |
| Foto/etiqueta |  | `NÃO DISPONÍVEL` |  |  |
| Firmware |  | `PENDENTE` | Referencia historica 3.3.10 nao confirmada na unidade |  |
| Programa HIstudio carregado |  | `PENDENTE` |  |  |
| Backup/hash do programa |  | `PENDENTE` |  |  |

## Protocolo e topologia

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Protocolo |  | `PENDENTE` |  |  |
| Transporte/interface |  | `PENDENTE` |  |  |
| Topologia |  | `PENDENTE` |  |  |
| Conversor/cabo |  | `NÃO DISPONÍVEL` |  |  |
| Outro mestre ausente |  | `PENDENTE` |  |  |
| Rede/canal isolado |  | `PENDENTE` |  |  |

## Parametros TCP, quando aplicavel

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| IP do CLP |  | `NÃO DISPONÍVEL` |  |  |
| Porta TCP |  | `NÃO DISPONÍVEL` | `502` existe apenas em perfil generico reservado |  |
| IP do PC antes do ajuste |  | `NÃO DISPONÍVEL` |  |  |
| IP do PC para bancada |  | `NÃO DISPONÍVEL` |  |  |
| Mascara |  | `NÃO DISPONÍVEL` |  |  |
| Gateway/DNS alterados |  | `NÃO DISPONÍVEL` |  |  |

## Parametros seriais, quando aplicavel

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Porta serial |  | `NÃO DISPONÍVEL` | `COM1` e apenas default do app |  |
| Baud rate |  | `CONFLITANTE` | Defaults/perfis 9600, 38400 e 57600 |  |
| Data bits |  | `PENDENTE` | 8 aparece em perfis genericos |  |
| Paridade |  | `PENDENTE` | None aparece em defaults/perfis genericos |  |
| Stop bits |  | `PENDENTE` | One aparece em defaults/perfis genericos |  |
| Terminacao/polaridade RS-485 |  | `NÃO DISPONÍVEL` |  |  |

## Endereco e politica de leitura

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Unit ID/endereco |  | `PENDENTE` | Slave 1 e apenas default do app |  |
| Referencia do mapa |  | `CONFLITANTE` | `%MW10..74` e SysVars 1120..1134 nao formam allow-list aprovada |  |
| Revisao independente do mapa |  | `PENDENTE` |  |  |
| Timeout em ms |  | `PENDENTE` | Deve ficar entre 100 e 5000 ms no preflight |  |
| Maximo de leituras |  | `PENDENTE` | Deve ser positivo e nao exceder a allow-list |  |
| Single-shot | `true` | `CONFIRMADO` | Politica de software da Fase 3.9 |  |
| Polling | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |
| Reconexao automatica | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |
| Escrita | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |
| Comunicacao real default | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |

## Allow-list read-only

Nao preencher a partir de `Hio115MemoryMap.cs` sem aprovacao do mapa real.

| Nome | Area | Endereco | Finalidade | Evidencia de aprovacao | Revisor |
|---|---|---:|---|---|---|
|  |  |  |  |  |  |

Areas aceitas pelo preflight: `input_register` e `holding_register`, sempre com
`access = read`. Coil, escrita, comando, setpoint e endereco sem evidencia sao
proibidos.

## Seguranca da bancada

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Tensao/alimentacao |  | `PENDENTE` |  |  |
| Aterramento confirmado |  | `NÃO DISPONÍVEL` |  |  |
| Saidas desenergizadas/isoladas |  | `PENDENTE` |  |  |
| Maquina impedida de operar |  | `NÃO DISPONÍVEL` |  |  |
| Estado seguro da maquina |  | `NÃO DISPONÍVEL` |  |  |
| Responsavel presente |  | `NÃO DISPONÍVEL` |  |  |
| Emergencia identificada |  | `NÃO DISPONÍVEL` |  |  |
| Desconexao rapida definida |  | `NÃO DISPONÍVEL` |  |  |

## Aprovacoes

- Preenchido por:
- Revisado por:
- Data/hora:
- Evidencia da aprovacao do mapa:
- Evidencia da aprovacao da bancada:
- Autorizacao para implementar transporte: nao concedida nesta ficha.
- Autorizacao para conectar ao CLP: nao concedida nesta ficha.
