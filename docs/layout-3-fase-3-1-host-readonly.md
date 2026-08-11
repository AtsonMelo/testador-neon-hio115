# Layout 3 - Fase 3.1 - Host read-only

## Objetivo

Criar o primeiro host opt-in do Layout 3, separado do preview e do `MainForm`,
com estado inicial local, desconectado e inativo. Esta fase estabelece a
fronteira entre apresentação, estado e intenção operacional sem criar nenhuma
comunicação ou execução física.

## Flag de abertura

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --layout-3-host-readonly
```

A janela aberta pela flag tem o título
`Layout 3 - Host read-only | Testador CLP HI`. Este documento registra a
entrega original da Fase 3.1. No estado atual, o fluxo padrão abre o
`IndustrialPlatformForm`; os launchers de preview foram superseded e removidos
pelo Project Cleanup 1, com cobertura funcional no host read-only e em
`--industrial`.

## Arquivos criados e alterados

Arquivos criados em `Ui/Industrial/Layout3/`:

- `Layout3HostForm.cs`: janela opt-in e composição do host;
- `Layout3HostControl.cs`: superfície visual, log local e geração de intenção;
- `Layout3ReadOnlyMode.cs`: capacidade explícita sem escrita física;
- `Layout3HostState.cs`: estado agregado inicial do host;
- `Layout3HardwareState.cs`: resumo do catálogo carregado localmente;
- `Layout3IoState.cs`: entradas indisponíveis e saídas inativas;
- `Layout3CommandIntent.cs`: intenção sem efeito colateral e decisão da guarda;
- `ILayout3CommandGuard.cs`: contrato de avaliação de intenções;
- `Layout3ReadOnlyCommandGuard.cs`: implementação que nega toda intenção.

Arquivos alterados:

- `Program.cs`: somente a entrada opt-in `--layout-3-host-readonly`;
- painel de perfil do preview: recebeu um parâmetro visual opcional para
  distinguir o resumo do preview e do host; esse painel foi posteriormente
  removido junto da árvore de preview superseded.

## Isolamento operacional

O host é criado diretamente por `Program.cs` e não instancia nem substitui o
`MainForm`. Sua única fonte de hardware é o catálogo local já carregado pela
aplicação. O estado inicial fixa:

- comunicação não conectada;
- entradas sem leitura física;
- saídas bloqueadas e inativas;
- comandos físicos executados igual a zero;
- logs restritos a eventos locais do host.

Não existe adaptador de comunicação, temporizador de polling, executor de
comandos ou referência a serviços físicos no host desta fase.

## Garantias de segurança

As sinalizações `MODO READ-ONLY`, `SEM ESCRITA FISICA` e
`0 COMANDOS FISICOS` ficam visíveis na barra superior e no log local. Além da
sinalização visual, `Layout3ReadOnlyCommandGuard` retorna sempre uma decisão
negativa com a mensagem:

> Bloqueado pelo modo read-only. Nenhum comando físico foi enviado.

O botão local de auditoria cria somente um `Layout3CommandIntent`, avalia a
guarda e registra o bloqueio. Não há componente executor depois da guarda; até
uma decisão inesperadamente permissiva é ignorada pelo host.

## Validações

Validações previstas para esta entrega:

- catálogo de hardware;
- seleção de perfil;
- relatório de preparação de teste;
- build do projeto;
- `git diff --check`;
- smoke da flag do host read-only;
- smoke do shell industrial oficial que substituiu os previews;
- auditoria do diff e busca por acoplamento indevido.

Os resultados executados são registrados no corpo do PR Draft.

## Limitações

- o host não observa o estado da conexão existente;
- não há leitura de entradas digitais;
- não há seleção operacional de perfil;
- não há polling ou atualização automática;
- não há escrita nem comando físico;
- o tema do host é automático e não possui flags próprias nesta fase.

## Próximos passos

As próximas fases devem continuar opt-in e preservar a guarda read-only. A
evolução prevista é expor gradualmente perfil real, estado de comunicação e
entradas digitais por adaptadores estritamente de observação. Qualquer escrita
controlada permanece fora desta fase e exige planejamento, revisão de
segurança e autorização de bancada próprios.
