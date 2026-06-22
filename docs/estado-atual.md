# Estado atual - Testador NEON HIO115

## Referência do projeto HIstudio

- Projeto: `TESTADOR_NEON_HIO115`.
- Modelo: NEON5-1S.
- CPU: CPU450, slot 0.
- Módulo: HIO115, slot 1.
- Firmware de referência: `G5PLC.C950.ST` 3.3.10.

Os artefatos HIstudio e a árvore `TESTADOR_NEON_HIO115_1_1` permanecem fora do
escopo da evolução atual e não foram alterados neste ciclo.

## Estado consolidado da aplicação

- A base operacional do Layout 3 está em
  `ui/issue-24-liga-io-industrial-host`, no commit `cf075d5`.
- O Layout 3 continua opt-in e separado do fluxo padrão do `MainForm`.
- As Fases 3.1 a 3.6 estão consolidadas na base.
- A seleção de hardware, o estado de comunicação, o bridge disabled/no-op e o
  gate bloqueado usam somente dados locais.
- O gate da Fase 3.6 permanece fail-closed, sem liberar ativação.
- `MainForm.cs` não foi alterado na consolidação documental.

## Limites operacionais vigentes

- Nenhuma comunicação real foi implementada ou ativada.
- Nenhuma conexão física foi aberta.
- Nenhuma leitura ou escrita de registradores foi executada.
- Leituras reais: 0.
- Escritas reais: 0.
- Comandos físicos executados: 0.

## Validação mais recente

Em 21 de junho de 2026, o QA `PostMerge` foi concluído sem bypass no commit
`cf075d5`:

- validadores locais: OK;
- build: OK, 0 erros e 0 avisos;
- `git diff --check`: OK;
- smoke GUI: OK, 4/4;
- comandos físicos executados: 0.

O perfil atual do AgentGate foi mantido porque seu caminho é exatamente o
checkout ativo: `C:\Users\atson\Documents\testador-neon-hio115`. O backup
comparado aponta para `C:\Atson\testador` e não corresponde ao `cwd` atual.

## Próxima etapa

Concluir a revisão documental e o PR Draft. Qualquer futura leitura real exige
milestone própria, análise de risco e autorização humana explícita; não faz
parte do estado atual.
