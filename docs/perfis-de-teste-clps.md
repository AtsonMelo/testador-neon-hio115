# Perfis de teste de CLPs e módulos HI

## Objetivo

Criar uma base modular para o testador de CLPs/módulos HI, permitindo selecionar o modelo do equipamento e executar o teste adequado para aquele hardware.

## Perfil 1 - HIO115

Status: validado em bancada.

### Recursos

- 4 saídas digitais PNP
- 8 entradas digitais PNP
- 3 entradas analógicas 4–20 mA

### Teste validado

- As saídas digitais acionam relés de interface.
- O contato 14 dos relés retorna sinal para as entradas digitais.
- Foi aplicada limitação/proteção na entrada 11.
- O teste automático sequencial acionou os relés.
- As entradas digitais foram identificadas corretamente pelo retorno dos contatos.

### Modos previstos

- Modo automático: aciona uma saída por vez e verifica retorno.
- Modo manual: permite acionar cada saída individualmente.
- Futuro aplicativo Windows: interface para seleção de modelo, comandos manuais e relatório.

## Perfis futuros

| Perfil | Modelo | DO | DI | AI | Tipo | Status |
|---|---:|---:|---:|---:|---|---|
| 1 | HIO115 | 4 | 8 | 3 | PNP | Validado em bancada |
| 2 | DIO605 | 8 | 8 | 0 | PNP | Futuro |
| 3 | HIO130 | 4 | 4 | 4 | Relé | Futuro |
| 4 | HIO140 | 4 | 4 | 4 | PNP + AO | Futuro |
| 5 | HIO160 | 4 | 8 | 1 + PT100 | PNP | Futuro |
| 6 | RION | variável | variável | variável | Remoto | Futuro |

## Próxima implementação

Criar no programa ST:

- MODELO_TESTE
- MODO_AUTO
- MODO_MANUAL
- TOTAL_DO
- TOTAL_DI
- STEP_TESTE
- CMD_MANUAL_DO00 a CMD_MANUAL_DO03
- RETORNO_DI00 a RETORNO_DI03
- OK_DO00 a OK_DO03
- FALHA_DO00 a FALHA_DO03

Primeiro alvo: HIO115 com modo automático e modo manual.

## Mapeamento físico validado - HIO115

| Canal | Saída lógica | SysVar saída | Entrada retorno | SysVar entrada | Status |
|---|---|---:|---|---:|---|
| 0 | D000 / DO00 | 1128 | DI00 | 1120 | Base validada |
| 1 | D001 / DO01 | 1129 | DI01 | 1121 | Base validada |
| 2 | D002 / DO02 | 1130 | DI02 | 1122 | Base validada |
| 3 | D003 / DO03 | 1131 | DI03 | 1123 | Base validada |

Regra para expansão futura:

Primeiro estabilizar o perfil HIO115 com 4 saídas digitais e 4 retornos digitais.

Depois expandir para:

1. Modo manual.
2. Módulos de 8 canais.
3. Módulos de 16 canais.
4. RION.
5. Simulador 4-20 mA.
