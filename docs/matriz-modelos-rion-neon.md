# Matriz de modelos RION/NEON

## Familias

| Familia | Nome | Status |
|---|---|---|
| `NEON_LEGACY` | NEON legado | Observada em campo/projeto, validacao por conjunto pendente. |
| `NEON_5` | NEON 5 | Referencia oficial encontrada; bancada, CPU, slots, modulos e mapas pendentes. |
| `RION_LEGACY` | RION legado | Observada por RION-502 + HIO115, validacao por conjunto pendente. |
| `RION_PLUS` | RION Plus | Referencia oficial e bancada pendentes. |
| `RION_5` | RION 5 | Referencia oficial encontrada; bancada, modulo confirmado, mapas e perfis pendentes. |

## Modelos

| Modelo | Familia | CPU | Modulo observado/inicial | Perfis iniciais | Validacao |
|---|---|---|---|---|---|
| `NEON-1S` | `NEON_LEGACY` | `CPU401` | `DIO605` | Comunicacao, I/O digital basico, saida manual, leitura de entrada | `pending_manual_validation` |
| `NEON-2S` | `NEON_LEGACY` | Pendente | Pendente | Comunicacao | `pending_manual_validation` |
| `NEON_5_CONTROLLER` | `NEON_5` | Pendente | Nenhum modulo confirmado | Comunicacao | `pending_manual_validation` |
| `RION-502` | `RION_LEGACY` | `CPU502` | `HIO115` | RS485 remoto, comunicacao, I/O digital basico | `pending_manual_validation` |
| `RION_5_CONTROLLER` | `RION_5` | Pendente | Nenhum modulo confirmado | RS485 remoto, comunicacao | `pending_manual_validation` |

Na aba `Perfil hardware`, esta matriz aparece de forma operacional: a familia
filtra modelos, o modelo sugere modulos, comunicacao e testes, e o resumo
mantem `pending_manual_validation` visivel quando a validacao por conjunto
ainda nao existe.

## Modulos de I/O

| Modulo | Familia inicial | Status | Observacao |
|---|---|---|---|
| `HIO115` | `RION_LEGACY` | `field_observed` + `verified_in_bench` no historico do fluxo atual | Referencia oficial adicionada; uso com RION-502 ainda deve ser validado por conjunto. |
| `DIO605` | `NEON_LEGACY` | `field_observed` + `pending_manual_validation` | Referencia oficial adicionada; observado com NEON-1S, mapa e ligacao pendentes. |
| `HIO130` | `RION_LEGACY` | `official_reference` + `pending_manual_validation` | Referencia oficial adicionada; sem perfil de bancada validado. |
| `HIO140` | `RION_LEGACY` | `official_reference` + `pending_manual_validation` | Referencia oficial adicionada; sem perfil de bancada validado. |
| `HIO165` | `RION_LEGACY` | `official_reference` + `pending_manual_validation` | Referencia oficial adicionada; perfil contador/encoder reservado e pendente. |

## Referencias oficiais resumidas

- RION 5: pagina oficial do produto, manual RION-5 e navegacao oficial de
  hardware. O catalogo registra suporte a 1 modulo, ate 16 pontos de I/O,
  operacao como CLP e/ou I/O remoto e ate 14 unidades RION 5 como I/O remoto
  ao CLP NEON.
- NEON 5: navegacao oficial de hardware/base lista NEON 5 como ate 240 pontos
  de I/O. A expansao por RION 5 remoto vem da pagina RION 5 e continua sem
  validacao de bancada neste projeto.
- HIO115, HIO130, HIO140, HIO165 e DIO605: PDFs oficiais de especificacao
  foram registrados como referencia documental.

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

## Interpretacao operacional

- Selecionar um perfil nao muda parametros reais de comunicacao.
- Selecionar um perfil nao executa comando fisico.
- `field_observed` indica item observado, mas nao substitui validacao do
  conjunto completo.
- `official_reference` indica fonte oficial HI Tecnologia registrada, mas nao
  substitui bancada.
- `verified_in_bench` em um modulo nao valida automaticamente outro
  controlador, outro slot ou outro perfil.
- "Nenhum modulo confirmado" significa que o catalogo ainda nao tem modulo
  associado ao modelo selecionado; nao indica falha de comunicacao.
- Radio transparente deve ser configurado fora do app, via XCTU, quando
  aplicavel.
- HIstudio, XCTU e Testador nao devem disputar a mesma COM.
