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

- barra superior com identidade, estado geral, selo `PREVIEW ONLY` e parada
  visual desabilitada;
- coluna esquerda com resumo inerte de conexao e diagnostico;
- area central com comandos demonstrativos e painel industrial de 4 DO / 8 DI;
- coluna direita com selecao informativa de hardware e preparacao do teste;
- terminal inferior que registra somente interacoes locais da preview.

O conteudo usa largura e altura minimas com `AutoScroll` para manter as zonas
acessiveis em resolucoes menores.

## Garantias de isolamento

- nenhum servico PLC ou Modbus e criado pelo formulario ou pelo controle;
- os campos COM, baud rate e Slave ID sao referencias visuais;
- as saidas do painel manual ficam desabilitadas e seus eventos nao sao ligados;
- os botoes centrais escrevem apenas no log local da preview;
- a parada de emergencia e somente visual e fica desabilitada;
- a selecao de hardware usa apenas o catalogo e o formatador de relatorio local;
- RION 5 e NEON 5 continuam identificados como `pending_manual_validation`;
- fechar a janela nao executa leitura, escrita, conexao ou desconexao fisica.

## Validacao visual pendente

A aprovacao manual ainda deve cobrir 1280x720, 1366x768, escala de 125%,
navegacao por teclado, contraste, rolagem e leitura integral dos relatorios.
