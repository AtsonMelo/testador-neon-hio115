# Layout 3 - Ficha de parametros e mapa controlado

## Perfil RTU offline

| Campo | Valor | Estado |
|---|---|---|
| Transporte selecionado | RTU | CONFIRMADO como escopo |
| Protocolo ativado no canal | Modbus RTU | PENDENTE |
| Porta observada no PC | COM8 | CONFIRMADO como observado; nao acessada |
| Camada fisica | RS-232 | DECLARADO/OBSERVADO |
| Interface do controlador | ITF-A1 ou ITF-A2 | NAO IDENTIFICADA |
| Conector atual | DB9 Serial | CONFIRMADO como observado |
| Baud | 38400 | CONFIRMADO como exibido |
| Data bits | 8 | CONFIRMADO como exibido |
| Paridade | None | CONFIRMADO como exibido |
| Stop bits | 1 | CONFIRMADO como exibido |
| Timeout entre caracteres | 50 ms | CONFIRMADO como exibido |
| Atraso de transmissao | 2 ms | CONFIRMADO como exibido |
| Atraso para remover portadora | 0 ms | CONFIRMADO como exibido |
| Frame maximo | 256 | CONFIRMADO como exibido |
| Remapeamento de endereco | nao | CONFIRMADO como exibido |
| Timeout operacional | 500 ms | DEFAULT DE SOFTWARE, PENDENTE DE APROVACAO FISICA |
| Intervalo entre tentativas | 100 ms | DEFAULT DE SOFTWARE, PENDENTE DE APROVACAO FISICA |
| Maximo por endereco | 1 | CONFIRMADO como politica |

RS-485 esta disponivel na ITF-B, com bornes D+/D- e chave de terminacao. Isso nao
indica que o cabo DB9 atual use RS-485. A escolha futura RS-232/RS-485 valida o
perfil e o cabo esperado; nao altera eletricamente a interface do controlador.

## Enderecamento

- endereco manual/default da interface: 1;
- descoberta automatica planejada: 1..247 em ordem crescente;
- uma tentativa por endereco por ciclo;
- maximo do ciclo completo: 247 tentativas;
- endereco historicamente observado: 10, apenas evidencia;
- endereco 0: broadcast, proibido;
- 248..255: reservados/vendor-specific, bloqueados;
- descoberta somente por comando explicito, cancelavel e sem repeticao;
- parar apenas quando a assinatura combinada for valida.

## Assinatura

| Item | Referencia documental | Acesso | Esperado | Estado |
|---|---:|---|---|---|
| Familia do firmware | F10 / endereco nao confirmado | R | G5PLC.C950.ST | PENDENTE |
| Versao do firmware | F11 / endereco nao confirmado | R | 3.3.11 | PENDENTE |
| PROG_ID | F12 / 30012 | R | 31134 | CONFIRMADO |
| PROG_CRC | F13 / 30013 | R | 23248 | CONFIRMADO |
| DEV_GFAIL_STS | F21 / 30021 | R | sem bit critico | CONFIRMADO |

As referencias 3xxxx sao documentais. O endereco de dados PDU/offset permanece
vazio ate o Gate D; nenhuma conversao 0-based/1-based foi inventada.

## Diagnostico F21

Todos os bits documentados abaixo sao tratados conservadoramente como criticos
antes de liberar I/O:

| Bit | Nome |
|---:|---|
| 0 | GFS_BLOCK_FAIL |
| 1 | GFS_NBLOCK_FAIL |
| 2 | GFS_FS_FAIL |
| 3 | GFS_ETH_FAIL |
| 8 | GFS_INIT_FAIL |
| 9 | GFS_IDENT_FAIL |
| 10 | GFS_OPER_FAIL |
| 11 | GFS_UNMATCH |
| 12 | GFS_INV_PROG |
| 13 | GFS_INV_FIRM |
| 14 | GFS_NVR_FAIL |

Qualquer bit critico aborta o fluxo, registra o motivo e bloqueia saidas.

## Mapa HIO115 confirmado documentalmente

### Entradas digitais

| Canal | Referencia | Acesso |
|---|---:|---|
| DI00..DI07 | F1120..F1127 / 31120..31127 | R |

### Entradas analogicas

| Canal | Referencia | Acesso |
|---|---:|---|
| AI00..AI02 | F1132..F1134 / 31132..31134 | R |

### Saidas digitais supervisionadas

| Canal | Referencia | Acesso planejado |
|---|---:|---|
| DO00 | F1128 / 31128 | R/W |
| DO01 | F1129 / 31129 | R/W |
| DO02 | F1130 / 31130 | R/W |
| DO03 | F1131 / 31131 | R/W |

A allow-list de saida contem somente esses quatro canais. A duracao default
offline e 1000 ms. Nesta fase, o modo de saida e `OFF`, `writesEnabled=false` e o
gate fisico nao esta autorizado.

### Bloqueados

| Referencia | Motivo |
|---:|---|
| F1137 / 31137 | reservado R/W |
| F1140 / 31140 | reservado R/W |
| F1143 / 31143 | reservado R/W |
| F1144 / 31144 | PWM frequencia R/W |
| F1145 / 31145 | PWM duty cycle R/W |

Qualquer outro endereco tambem e bloqueado para saida. Contadores e encoder
permanecem fora do primeiro teste operacional.

## Backlog TCP

Modbus TCP nao integra o esquema, o preflight nem a arquitetura executavel da
Fase 3.9. Nao ha IP, porta TCP ou descoberta Ethernet configurados.
