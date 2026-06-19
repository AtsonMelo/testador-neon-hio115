# Layout 3 - preview isolado

## Objetivo

Esta entrega implementa a Fase 2 do conceito hibrido como uma janela opt-in.
Ela nao substitui o `MainForm`, nao altera o modo padrao e nao cria servicos de
comunicacao com o CLP.

## Como abrir

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --preview-layout-3
```

Sem a flag, o aplicativo continua abrindo a interface de producao existente.

## Estrutura visual

- barra superior compacta com identidade, estado geral, selo `PREVIEW ONLY` e
  parada estritamente visual;
- tres zonas proporcionais e legiveis: conexao/diagnostico, I/O manual e
  perfil/preparacao;
- painel de 4 DO / 8 DI com assets industriais reais, botoeiras verdes inertes
  e LEDs cinza desligados; o componente usa LED verde somente para estado
  visual ligado, sem leitura na preview;
- ficha tecnica somente leitura para familia, modelo, modulo, comunicacao e
  perfil de teste, baseada apenas no catalogo carregado em memoria;
- resumo curto de preparacao com copia local, sem exibir o relatorio extenso;
- terminal inferior reduzido, reservado para a garantia de isolamento.

A composicao tem largura minima de 1180 px, colunas de 26% / 44% / 30% e
`AutoScroll` apenas como contingencia. Em 1280x720 os elementos principais ficam
visiveis na abertura; em 1366x768 e 1920x1080 o espaco adicional e distribuido
entre as tres zonas.

## Garantias de isolamento

- nenhum servico PLC ou Modbus e criado pelo formulario ou pelo controle;
- os campos COM, baud rate e Slave ID sao referencias visuais;
- as saidas do painel manual sao representacoes inertes e nao possuem eventos;
- a parada de emergencia e somente um indicador visual sem evento associado;
- a referencia de hardware e somente leitura e usa apenas listas do catalogo
  local;
- o unico botao ativo copia o resumo local ou limpa o texto do terminal;
- RION 5 e NEON 5 continuam identificados como `pending_manual_validation`;
- fechar a janela nao executa leitura, escrita, conexao ou desconexao fisica.

## Validacao visual pendente

A aprovacao manual ainda deve cobrir 1280x720, 1366x768, escala de 125%,
navegacao por teclado, contraste, rolagem e leitura integral dos relatorios.
