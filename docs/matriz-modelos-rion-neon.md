# Matriz de modelos RION/NEON

## Familias

| Familia | Nome | Status |
|---|---|---|
| `NEON_LEGACY` | NEON legado | Observada em campo/projeto, validacao por conjunto pendente. |
| `NEON_5` | NEON 5 | Referencia oficial e bancada pendentes. |
| `RION_LEGACY` | RION legado | Observada por RION-502 + HIO115, validacao por conjunto pendente. |
| `RION_PLUS` | RION Plus | Referencia oficial e bancada pendentes. |
| `RION_5` | RION 5 | Referencia oficial e bancada pendentes. |

## Modelos

| Modelo | Familia | CPU | Modulo observado/inicial | Perfis iniciais | Validacao |
|---|---|---|---|---|---|
| `NEON-1S` | `NEON_LEGACY` | `CPU401` | `DIO605` | Comunicacao, I/O digital basico, saida manual, leitura de entrada | `pending_manual_validation` |
| `NEON-2S` | `NEON_LEGACY` | Pendente | Pendente | Comunicacao | `pending_manual_validation` |
| `NEON_5_CONTROLLER` | `NEON_5` | Pendente | Pendente | Comunicacao | `pending_manual_validation` |
| `RION-502` | `RION_LEGACY` | `CPU502` | `HIO115` | RS485 remoto, comunicacao, I/O digital basico | `pending_manual_validation` |
| `RION_5_CONTROLLER` | `RION_5` | Pendente | Pendente | RS485 remoto, comunicacao | `pending_manual_validation` |

## Modulos de I/O

| Modulo | Familia inicial | Status | Observacao |
|---|---|---|---|
| `HIO115` | `RION_LEGACY` | `verified_in_bench` no historico do fluxo atual | Uso com RION-502 ainda deve ser validado por conjunto. |
| `DIO605` | `NEON_LEGACY` | `pending_manual_validation` | Observado com NEON-1S; mapa e ligacao pendentes. |
| `HIO130` | `RION_LEGACY` | `pending_manual_validation` | Cadastro inicial sem especificacao confirmada. |
| `HIO140` | `RION_LEGACY` | `pending_manual_validation` | Cadastro inicial sem especificacao confirmada. |
| `HIO165` | `RION_LEGACY` | `pending_manual_validation` | Perfil contador/encoder reservado e pendente. |

## Perfis de comunicacao

| Perfil | Uso |
|---|---|
| `SERIAL_RS232_38400_8N1_MODBUS_RTU` | Perfil serial inicial; confirmar suporte por modelo. |
| `SERIAL_RS485_38400_8N1_MODBUS_RTU` | Perfil RS485 inicial; confirmar cabeamento, terminacao e slave ID. |
| `SERIAL_RS485_57600_8N1_MODBUS_RTU` | Perfil RS485 alternativo; confirmar suporte antes de usar. |
| `ETHERNET_MODBUS_TCP` | Reservado; o app atual nao implementa Modbus TCP nesta milestone. |
| `WIRELESS_RADIO_TRANSPARENT` | Enlace transparente; configuracao de radio fica fora do app via XCTU. |

## Perfis de teste

| Perfil | Aplicacao inicial |
|---|---|
| `DIGITAL_IO_BASIC` | HIO115 e DIO605 quando mapa e bancada estiverem confirmados. |
| `DIGITAL_OUTPUT_MANUAL` | Saida manual por canal no fluxo ja previsto. |
| `DIGITAL_INPUT_READ` | Leitura de entradas pelo fluxo atual. |
| `REMOTE_IO_RS485` | Preparacao para RION/remote I/O via RS485. |
| `COMMUNICATION_DIAGNOSTIC` | Diagnostico inicial antes de qualquer I/O. |
| `COUNTER_ENCODER_PENDING_VALIDATION` | Reservado; nao executar automaticamente nesta milestone. |
