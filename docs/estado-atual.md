# Estado atual - Testador NEON HIO115

## Estado do projeto

Projeto criado no HIstudio:

- Nome: TESTADOR_NEON_HIO115
- Modelo: NEON5-1S
- CPU: CPU450 [slot 0]
- Módulo: HIO115 [slot 1]
- Firmware: G5PLC.C950.ST [3.3.10]

## Status validado

- Projeto configurado com CPU450 e HIO115
- PROGRAM_01 associado à Tarefa_01
- Compilação executada com sucesso
- Arquivo APP1000DEV_1_F3310.dbc gerado
- Aplicação carregada no CLP
- DO00 ficou verde no HIstudio
- Código atual liga DO00 fixa via ST

## Código atual

```pascal
HILS.SET_SYSVAR(1128, 1);Próxima etapa

Substituir comando fixo por botões virtuais internos:

CMD_DO00
CMD_DO01
CMD_DO02
CMD_DO03
