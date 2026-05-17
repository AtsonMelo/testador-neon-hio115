# Produtividade de Desenvolvimento - TestadorCLPHI.App

## Registro semanal

| Data | Branch | Tarefa | Tipo | Tempo estimado | Tempo real | Build OK | Teste manual | Retrabalho | Resultado |
|---|---|---|---|---:|---:|---|---|---|---|
| 2026-05-17 | refactor/revisao-mainform | Extrair painel de estado da conexão | refactor | - | - | Sim | Sim | Não identificado | Commit ab1a3b1 criado e enviado |

## Métricas atuais

| Métrica | Valor |
|---|---:|
| Linhas MainForm.cs | 705 |
| Último commit | ab1a3b1 |
| Build atual | OK |
| Erros atuais | 0 |
| Avisos atuais | 0 |

## Perfil de trabalho

- Uma tarefa por vez.
- Commits pequenos.
- Build antes de commit.
- Teste manual mínimo antes de avançar.
- Não misturar refatoração com funcionalidade nova.
- Registrar retrabalho.
- Medir evolução semanalmente.

## Tipos de tarefa

- refactor
- feat
- fix
- docs
- test
- chore

## Critério de qualidade

Uma etapa só é considerada concluída quando:

- build passa;
- app abre;
- alteração tem objetivo único;
- diff é compreensível;
- não quebrou funcionalidade existente;
- commit foi pequeno e rastreável.

## Metas iniciais

| Meta | Estado |
|---|---|
| Reduzir MainForm.cs para menos de 600 linhas | Pendente |
| Reduzir MainForm.cs para menos de 500 linhas | Pendente |
| Preservar comunicação Modbus RTU | Em acompanhamento |
| Preservar tema claro/escuro | Em acompanhamento |
| Preservar painel de estado da conexão | Em acompanhamento |
| Medir retrabalho por etapa | Iniciado |

## Método de feedback dos comandos

Para evitar repetição de orientações e retrabalho, cada etapa prática deve seguir este ciclo:

1. O assistente envia um bloco de comandos.
2. O usuário executa exatamente o bloco no PowerShell.
3. O usuário retorna apenas a saída relevante do terminal.
4. O assistente interpreta o resultado e decide o próximo passo.
5. Não repetir comandos já validados, salvo se houver nova alteração.

## Padrão de resposta esperado após executar comandos

Ao retornar uma execução do PowerShell, informar preferencialmente:

```text
COMANDO/ETAPA:
descrever brevemente o que foi executado

RESULTADO:
colar a saída principal do PowerShell

STATUS:
OK / ERRO / DÚVIDA

OBSERVAÇÃO:
algo visual ou comportamento do app, se houver
Comando padrão para copiar saída do PowerShell

Quando precisar enviar a saída completa de uma etapa, usar:

$saida = @"Comando padrão para copiar saída do PowerShell

Quando precisar enviar a saída completa de uma etapa, usar:

$saida = @"
COLE AQUI A SAÍDA DO POWERSHELL
"@

$saida | Set-Clipboard

Depois colar no chat.

Regra para próximas etapas
Se o build já passou, não repetir build sem motivo.
Se o git status já foi mostrado limpo, não pedir novamente antes de uma nova alteração.
Se uma etapa já foi commitada e enviada, considerar concluída.
Sempre usar o último commit, branch e resultado informado como estado atual.
Antes de novo commit, validar build, teste manual mínimo, git status --short e git diff --stat.
