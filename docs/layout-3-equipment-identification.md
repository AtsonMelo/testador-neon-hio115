# Identificação do equipamento

## Perfil de referência

| Informação | Valor | Classificação |
|---|---|---|
| Controlador HIstudio | NEON5-1S | [Certo] observado |
| CPU | CPU450, slot 0 | [Certo] observado |
| Módulo | HIO115, slot 1 | [Certo] observado |
| HIstudio | 2.4.03 | [Certo] observado |
| Firmware exibido | G5PLC.C950.ST [3.3.11] | [Provável] até evidência específica da CPU |
| Programa | MOTOR_HIDRO:PROD_NEON5_HIO115 | [Certo] observado |
| Versão do programa | 3220 | [Certo] observado |
| PROG_ID | 31134 | [Certo] observado |
| PROG_CRC | 23248 | [Certo] observado |

## Evidência física

| Informação | Valor | Classificação |
|---|---|---|
| Identificação frontal | PIVODRIP | [Certo] observada |
| Marca | OMNICONTROL | [Certo] observada |
| Modelo frontal | OMNI-PLC2 | [Certo] observado |
| Número de série | 111.20023 | [Certo] observado |
| Part number | 300.111.622.801 | [Certo] observado |
| Identificação adicional | Slot 1-115 | [Certo] observada |
| Alimentação nominal indicada | 10-30 VDC | [Certo] etiqueta, não medição |
| Relação OMNI-PLC2 / NEON5-1S | compatível possível | [Incerto] sem documento OEM |

O alias `OMNI-PLC2` é armazenado como evidência compatível ou não confirmada,
nunca como correspondência exata irreversível.

## Critério automático atual

O fake é identificado quando, no mesmo endereço:

1. `PROG_ID == 31134`;
2. `PROG_CRC == 23248`;
3. `F21` não possui bit crítico.

Firmware/família permanecem no catálogo, mas não entram em leitura RTU enquanto
F10/F11 não tiverem endereço documental confirmado. A UI mostra esses dados
como referência do perfil, não como leitura física.

## F21 centralizado

O mask crítico inclui bits `0`, `1`, `2`, `3`, `8`, `9`, `10`, `11`, `12`,
`13` e `14`. A interpretação reside em `Layout3BenchWorkflowPolicy`; a UI não
repete bitmasks.

Qualquer bit crítico resulta em `CriticalFault`, encerra a identificação e
bloqueia testes de I/O.

## Pendências físicas

- [Bloqueado] equivalência documental OMNI-PLC2 / NEON5-1S;
- [Bloqueado] evidência específica do firmware da CPU;
- [Bloqueado] confirmação do protocolo ativado no canal real;
- [Bloqueado] validação física final da etiqueta e da tensão medida.
