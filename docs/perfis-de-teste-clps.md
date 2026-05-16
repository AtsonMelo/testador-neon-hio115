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
