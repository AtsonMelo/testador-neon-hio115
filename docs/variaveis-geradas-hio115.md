# Variáveis geradas - HIO115

Modelo: `HIO115`
Descrição: Modulo HIO115 no slot 1 - 4 DO, 8 DI, 3 AI

## Variáveis locais necessárias no PROGRAM_01

| Nome | Classe | Tipo | Valor inicial | Função |
|---|---|---|---|---|
| T_STEP | Local | TON | - | Temporizador do passo automático |
| STEP_TESTE | Local | INT | 0 | Passo atual do teste |
| RESET_TIMER | Local | BOOL | FALSE | Reset lógico do TON |

## Mapa de saídas digitais

| Canal | SysVar |
|---|---:|
| DO00 | 1128 |
| DO01 | 1129 |
| DO02 | 1130 |
| DO03 | 1131 |

## Mapa de entradas digitais

| Canal | SysVar |
|---|---:|
| DI00 | 1120 |
| DI01 | 1121 |
| DI02 | 1122 |
| DI03 | 1123 |
| DI04 | 1124 |
| DI05 | 1125 |
| DI06 | 1126 |
| DI07 | 1127 |

## Mapa de entradas analógicas

| Canal | SysVar |
|---|---:|
| AI00 | 1132 |
| AI01 | 1133 |
| AI02 | 1134 |

## Próximo uso

Copiar o conteúdo de `src-st/generated/program_01_hio115_auto.st` para a aba Código do `PROGRAM_01` no HIstudio.
As variáveis locais devem ser criadas manualmente ou por automação futura.
