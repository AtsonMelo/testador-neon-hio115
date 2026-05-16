# Variáveis geradas - HIO115

Modelo: `HIO115`
Descrição: Modulo HIO115 no slot 1 - 4 DO, 8 DI, 3 AI

## Arquivos gerados

- `src-st/generated/program_01_hio115_auto.st`
- `src-st/generated/program_01_hio115_auto_manual.st`
- `histudio-import/globais_completo_com_classe.csv`
- `histudio-import/program_01_completo_com_classe.csv`

## Regra importante do HIstudio

Para o vínculo Global/Externa funcionar, o CSV precisa conter a coluna `Classe`.

Nas variáveis globais:

- `Classe = Global`

No `PROGRAM_01`:

- `Classe = Local` para variáveis internas
- `Classe = Externa` para referências às globais

## Mapa de saídas digitais

| Canal | Nome lógico | SysVar |
|---|---|---:|
| DO00 | D000 | 1128 |
| DO01 | D001 | 1129 |
| DO02 | D002 | 1130 |
| DO03 | D003 | 1131 |

## Mapa de entradas digitais usadas como retorno

| Canal | Nome lógico | SysVar |
|---|---|---:|
| DI00 | DI00 | 1120 |
| DI01 | DI01 | 1121 |
| DI02 | DI02 | 1122 |
| DI03 | DI03 | 1123 |
| DI04 | DI04 | 1124 |
| DI05 | DI05 | 1125 |
| DI06 | DI06 | 1126 |
| DI07 | DI07 | 1127 |

## Diagnósticos gerados

| Variável | Função |
|---|---|
| `OK_Dxxx` | Indica que a saída acionada recebeu retorno correto e nenhum retorno cruzado. |
| `FALHA_Dxxx` | Indica que a saída foi acionada, mas o retorno esperado não apareceu. |
| `ERRO_Dxxx` | Indica que apareceu retorno em entrada diferente da esperada. |
| `TESTE_OK_GERAL` | Indica que há canal OK e não há falha nem erro ativo. |
| `TESTE_FALHA_GERAL` | Indica falha de retorno em pelo menos um canal ativo. |
| `TESTE_ERRO_GERAL` | Indica retorno cruzado em pelo menos um canal ativo. |

## Variáveis de segurança e app

| Variável | Endereço | Função |
|---|---:|---|
| `HABILITA_TESTE` | `%MW70` | Trava principal. Se estiver 0, todas as saídas físicas ficam desligadas. |
| `RESET_SAIDAS` | `%MW71` | Quando recebe 1, zera modos e comandos manuais. |
| `WATCHDOG_APP` | `%MW72` | Sinal vivo que o app deverá alternar periodicamente. |
| `FALHA_WATCHDOG` | `%MW73` | Liga quando o watchdog ativo não recebe atualização dentro do tempo. |
| `WATCHDOG_ATIVO` | `%MW74` | Habilita a supervisão de watchdog. Pode ficar 0 durante teste manual no HIstudio. |
