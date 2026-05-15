# Atualização - Botões virtuais DO

## Estado validado

- Projeto aberto e ajustado no notebook.
- Comunicação serial via COM3 voltou a funcionar.
- Variáveis globais criadas:
  - G_CMD_DO00
  - G_CMD_DO01
  - G_CMD_DO02
  - G_CMD_DO03
- Variáveis externas criadas no PROGRAM_01.
- Compilação executada com sucesso.
- Carga da aplicação executada com sucesso.
- DO00 ligou quando G_CMD_DO00 foi colocado em valor inicial 1.
- DataMonitor reconheceu %MW0, mas ainda ficou como acesso somente leitura.
- Próxima etapa: ajustar permissão de escrita no DataMonitor ou nas opções da variável mapeada.

## Mapeamento iniciado

- G_CMD_DO00 associado a %MW0.
