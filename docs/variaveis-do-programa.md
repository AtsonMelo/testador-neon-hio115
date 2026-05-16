# Variáveis do PROGRAM_01

## Variáveis locais para sequenciador automático

| Nome | Classe | Tipo | Inicial | Função |
|---|---|---|---|---|
| T_STEP | Local | TON | - | Temporizador do passo automático |
| STEP_TESTE | Local | INT | 0 | Número do passo atual |
| RESET_TIMER | Local | BOOL | FALSE | Reset lógico do TON |

## Variáveis futuras para modo manual

| Nome | Classe | Tipo | Inicial | Função |
|---|---|---|---|---|
| MODO_AUTO | Global | BOOL | TRUE | Habilita teste automático |
| MODO_MANUAL | Global | BOOL | FALSE | Habilita teste manual |
| CMD_MANUAL_DO00 | Global | BOOL | FALSE | Comando manual da DO00 |
| CMD_MANUAL_DO01 | Global | BOOL | FALSE | Comando manual da DO01 |
| CMD_MANUAL_DO02 | Global | BOOL | FALSE | Comando manual da DO02 |
| CMD_MANUAL_DO03 | Global | BOOL | FALSE | Comando manual da DO03 |

## Variáveis futuras de diagnóstico

| Nome | Tipo | Função |
|---|---|---|
| OK_DO00 | BOOL | Retorno correto da DO00 |
| OK_DO01 | BOOL | Retorno correto da DO01 |
| OK_DO02 | BOOL | Retorno correto da DO02 |
| OK_DO03 | BOOL | Retorno correto da DO03 |
| FALHA_DO00 | BOOL | Falha no retorno da DO00 |
| FALHA_DO01 | BOOL | Falha no retorno da DO01 |
| FALHA_DO02 | BOOL | Falha no retorno da DO02 |
| FALHA_DO03 | BOOL | Falha no retorno da DO03 |
