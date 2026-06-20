# Layout 3 — plano de limpeza operacional

## Objetivo e limite deste PR

Este documento organiza uma futura limpeza de branches do ciclo do Layout 3.
Ele não executa exclusão local ou remota, não fecha PR e não altera tags. Toda
remoção citada abaixo é apenas uma sugestão para uma janela posterior, depois de
verificação manual.

Inventário de referência: base `ui/issue-24-liga-io-industrial-host` no commit
`24f81f0`, observado em 20 de junho de 2026.

## Branches antigas já integradas ao histórico da base

O histórico da base contém os merges ou commits das linhas abaixo:

- `ui/issue-24-preview-compacto-layout-alvo-2` — integrado pelo PR #28;
- `docs/portfolio-readme-testador` — integrado pelo PR #29;
- `docs/readme-referencias-layout-industrial` — integrado pelo PR #30;
- `ui/issue-24-integra-visual-industrial-main` — seu HEAD conhecido é ancestral
  da base atual;
- `design/layout-3-conceito-hibrido` — integrado pelo PR #37;
- `ui/layout-3-host-communication-state-readonly` — entrega da Fase 3.3 já
  presente na base;
- `ui/layout-3-host-readonly-safety-validation` — entrega da Fase 3.4 já
  presente na base;
- `ui/layout-3-read-bridge-disabled` — entrega da Fase 3.5 já presente na base.

A integração histórica não prova que uma branch ainda exista no repositório
local ou remoto. Também não autoriza removê-la sem repetir a verificação de
ancestralidade e conferir PRs abertos no momento da limpeza.

## Candidatas a remoção futura

As branches da seção anterior podem ser candidatas se ainda existirem e se, no
dia da limpeza, todas as condições abaixo forem verdadeiras:

- o HEAD da branch é ancestral da base ou de `main`;
- não existe PR aberto usando a branch como head ou base;
- não existe trabalho exclusivo, commit não publicado ou evidência ainda não
  preservada por tag/documentação;
- a base e o remoto foram atualizados com fast-forward;
- outra pessoa revisou a lista final.

Branches de milestones concluídas no futuro só entram nessa categoria depois do
merge do respectivo PR e da confirmação de ancestralidade. Nenhuma branch deve
ser removida apenas por parecer antiga ou pelo nome sugerir conclusão.

## Branches e referências que devem ser mantidas

- `main` e `ui/issue-24-liga-io-industrial-host`;
- `ui/layout-3-read-bridge-activation-gate` enquanto o PR Draft #48 estiver aberto;
- `tools/qa-layout-3-read-bridge-validators` enquanto o PR Draft #49 estiver aberto;
- `docs/layout-3-fase-3-roadmap-operacional` enquanto o PR Draft #50 estiver aberto;
- `docs/layout-3-operational-cleanup-plan` enquanto o PR desta milestone estiver aberto;
- qualquer branch que apareça em `git branch --no-merged` contra a base;
- branches de checkpoint usadas como rollback operacional, especialmente
  `checkpoint/modo-normal-ok-20260523-094344`, até decisão explícita;
- tags `layout-3-*`, `qa-automation-ai-workflow-ok-20260619` e demais tags de
  evidência técnica.

Tags e branches são referências diferentes. Limpar uma branch não exige nem
autoriza remover a tag que preserva um checkpoint aprovado.

## Verificação sugerida antes de qualquer limpeza

Executar primeiro apenas comandos de inventário:

```powershell
git status -sb
git fetch origin --prune
git branch --merged ui/issue-24-liga-io-industrial-host
git branch --no-merged ui/issue-24-liga-io-industrial-host
git branch -r --merged origin/ui/issue-24-liga-io-industrial-host
git log --oneline --decorate --graph -30
gh pr list --state open
```

Para cada candidata, confirmar individualmente:

```powershell
git log --oneline ui/issue-24-liga-io-industrial-host..NOME_DA_BRANCH
gh pr list --state open --head NOME_DA_BRANCH
```

Qualquer saída com commit exclusivo ou PR aberto remove a branch da lista de
limpeza.

## Comandos sugeridos para uma janela futura

Somente após a revisão anterior e aprovação explícita, a limpeza local poderia
usar, uma branch por vez:

```powershell
git branch -d NOME_DA_BRANCH_CONFIRMADA
```

O uso de `-d` é intencional porque mantém a proteção contra branch não integrada.
Não substituir por `-D`.

Depois de confirmar novamente a branch remota e o PR correspondente, uma
limpeza remota futura poderia usar:

```powershell
git push origin --delete NOME_DA_BRANCH_CONFIRMADA
git fetch origin --prune
```

Esses comandos são exemplos documentais. Eles não fazem parte da validação nem
do runner deste PR e não foram executados.

## Avisos de segurança

- nunca limpar a branch atualmente selecionada;
- nunca usar exclusão forçada para contornar o bloqueio de ancestralidade;
- nunca remover uma branch com PR aberto, Draft ou evidência pendente;
- não fechar PR como atalho para limpeza;
- não remover tags de checkpoint no mesmo lote;
- não misturar limpeza com merge, rebase ou mudança de código;
- registrar a lista aprovada e a saída de verificação antes de agir;
- interromper a operação diante de qualquer dúvida sobre commit exclusivo.

## Declaração final

Este PR cria somente o plano. Nenhuma branch local ou remota é excluída, nenhum
PR é fechado e nenhuma referência Git é alterada pela entrega.
