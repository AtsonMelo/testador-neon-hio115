# Roteiro de Validação no HIstudio - Diagnóstico HIO115

## Objetivo

Validar no HIstudio o programa gerado para o testador HIO115 com:

- modo automático
- modo manual
- diagnóstico OK por canal
- diagnóstico de falha por canal
- diagnóstico de retorno cruzado por canal
- resultado geral do teste

## Arquivos usados

### Variáveis globais

`histudio-import/globais_completo_com_classe.csv`

### Variáveis do PROGRAM_01

`histudio-import/program_01_completo_com_classe.csv`

### Código ST principal

`src-st/generated/program_01_hio115_auto_manual.st`

## Mapa físico esperado

| Saída | SysVar saída | Relé | Entrada retorno | SysVar entrada |
|---|---:|---|---|---:|
| D000 / DO00 | 1128 | Relé 00 | DI00 | 1120 |
| D001 / DO01 | 1129 | Relé 01 | DI01 | 1121 |
| D002 / DO02 | 1130 | Relé 02 | DI02 | 1122 |
| D003 / DO03 | 1131 | Relé 03 | DI03 | 1123 |

## Etapa 1 - Importar variáveis globais

No HIstudio:

1. Abrir o projeto do CLP.
2. Acessar a área de variáveis globais.
3. Importar o arquivo:

`histudio-import/globais_completo_com_classe.csv`

Confirmar se aparecem:

- MODO_AUTO
- MODO_MANUAL
- CMD_MANUAL_D000 a CMD_MANUAL_D003
- RETORNO_DI00 a RETORNO_DI03
- OK_D000 a OK_D003
- FALHA_D000 a FALHA_D003
- ERRO_D000 a ERRO_D003
- TESTE_OK_GERAL
- TESTE_FALHA_GERAL
- TESTE_ERRO_GERAL

## Etapa 2 - Importar variáveis do PROGRAM_01

No PROGRAM_01, importar:

`histudio-import/program_01_completo_com_classe.csv`

Confirmar se existem variáveis locais:

- T_STEP
- STEP_TESTE
- RESET_TIMER
- COMANDO_D000 a COMANDO_D003

E variáveis externas:

- MODO_AUTO
- MODO_MANUAL
- CMD_MANUAL_D000 a CMD_MANUAL_D003
- RETORNO_DI00 a RETORNO_DI03
- OK_D000 a OK_D003
- FALHA_D000 a FALHA_D003
- ERRO_D000 a ERRO_D003
- TESTE_OK_GERAL
- TESTE_FALHA_GERAL
- TESTE_ERRO_GERAL

## Etapa 3 - Colar código ST

No PROGRAM_01, substituir o código pelo conteúdo de:

`src-st/generated/program_01_hio115_auto_manual.st`

Depois compilar.

## Etapa 4 - Configuração inicial para teste automático

Antes de rodar:

| Variável | Valor |
|---|---:|
| MODO_AUTO | 1 |
| MODO_MANUAL | 0 |
| CMD_MANUAL_D000 | 0 |
| CMD_MANUAL_D001 | 0 |
| CMD_MANUAL_D002 | 0 |
| CMD_MANUAL_D003 | 0 |

Resultado esperado:

- as saídas D000 a D003 acionam em sequência
- cada relé bate um por vez
- cada entrada DI00 a DI03 aparece no retorno correspondente

## Etapa 5 - Validação por canal no automático

### Canal 0

Quando D000 acionar:

| Variável | Esperado |
|---|---:|
| RETORNO_DI00 | 1 |
| OK_D000 | 1 |
| FALHA_D000 | 0 |
| ERRO_D000 | 0 |

### Canal 1

Quando D001 acionar:

| Variável | Esperado |
|---|---:|
| RETORNO_DI01 | 1 |
| OK_D001 | 1 |
| FALHA_D001 | 0 |
| ERRO_D001 | 0 |

### Canal 2

Quando D002 acionar:

| Variável | Esperado |
|---|---:|
| RETORNO_DI02 | 1 |
| OK_D002 | 1 |
| FALHA_D002 | 0 |
| ERRO_D002 | 0 |

### Canal 3

Quando D003 acionar:

| Variável | Esperado |
|---|---:|
| RETORNO_DI03 | 1 |
| OK_D003 | 1 |
| FALHA_D003 | 0 |
| ERRO_D003 | 0 |

## Etapa 6 - Teste manual

Configurar:

| Variável | Valor |
|---|---:|
| MODO_AUTO | 0 |
| MODO_MANUAL | 1 |

Depois testar um comando por vez:

| Comando | Saída esperada | Retorno esperado |
|---|---|---|
| CMD_MANUAL_D000 = 1 | D000 liga | DI00 liga |
| CMD_MANUAL_D001 = 1 | D001 liga | DI01 liga |
| CMD_MANUAL_D002 = 1 | D002 liga | DI02 liga |
| CMD_MANUAL_D003 = 1 | D003 liga | DI03 liga |

Atenção:

Não deixar dois comandos manuais ligados ao mesmo tempo no primeiro teste.

## Etapa 7 - Teste de falha

Para simular falha:

1. Acionar uma saída.
2. Desconectar temporariamente o retorno da entrada correspondente.
3. Verificar se a falha aparece.

Exemplo:

D000 acionada sem retorno em DI00:

| Variável | Esperado |
|---|---:|
| OK_D000 | 0 |
| FALHA_D000 | 1 |
| ERRO_D000 | 0 |
| TESTE_FALHA_GERAL | 1 |

## Etapa 8 - Teste de retorno cruzado

Para simular erro de retorno cruzado:

1. Acionar D000.
2. Fazer o retorno aparecer em DI01, DI02 ou DI03.
3. Confirmar que ERRO_D000 ativa.

Exemplo:

D000 acionada, mas retorno chegou em DI01:

| Variável | Esperado |
|---|---:|
| OK_D000 | 0 |
| ERRO_D000 | 1 |
| TESTE_ERRO_GERAL | 1 |

## Falhas comuns durante validação

| Sintoma | Possível causa |
|---|---|
| Saída verde no HIstudio, mas relé não bate | fiação, alimentação do grupo de saída, borne errado ou relé |
| Relé bate, mas DI não aparece | contato errado do relé, comum ausente ou entrada ligada no borne errado |
| FALHA_Dxxx liga | retorno esperado não chegou |
| ERRO_Dxxx liga | retorno apareceu em outra entrada |
| Nada muda no monitor | variáveis não importadas como Global/Externa corretamente |
| Compilação falha | variável ausente ou tipo incompatível |

## Resultado esperado da validação

O teste será considerado aprovado quando:

- D000 acionar DI00
- D001 acionar DI01
- D002 acionar DI02
- D003 acionar DI03
- OK_D000 a OK_D003 funcionarem
- FALHA_D000 a FALHA_D003 funcionarem
- ERRO_D000 a ERRO_D003 funcionarem
- TESTE_FALHA_GERAL ativar quando houver falha
- TESTE_ERRO_GERAL ativar quando houver retorno cruzado
