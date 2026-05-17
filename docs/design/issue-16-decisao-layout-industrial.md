# Issue #16 — Decisão de layout industrial

## Contexto

Durante a evolução visual do painel manual 4DO/8DI, foi validado que os assets industriais com alpha real funcionam tecnicamente.

Também foi validado que aplicar os LEDs diretamente no layout atual do `DigitalIoManualPanelControl` gerou problemas visuais:

- painel apertado;
- sobreposição de textos;
- perda de espaço do terminal/log;
- necessidade de ajustar coordenadas manualmente;
- resultado visual inferior ao esperado.

## Diagnóstico

O problema principal não é o asset PNG.

O problema está na arquitetura de layout atual, baseada em posições absolutas dentro de um `GroupBox` pequeno.

Continuar ajustando `Top`, `Left`, `Height` e `ImageSize` tende a gerar tentativa e erro.

## Decisão

Antes de aplicar novamente os assets no painel principal, será criado um protótipo isolado do painel manual industrial.

Nome sugerido:

- `IndustrialDigitalIoPanelControl`

Objetivo:

- validar layout industrial 4DO/8DI fora do `MainForm`;
- organizar DO e DI sem sobreposição;
- manter terminal/log visível no app principal;
- permitir ajuste visual controlado;
- evitar inflar `MainForm.cs`.

## Diretriz de layout

Priorizar contêineres de layout do WinForms:

- `TableLayoutPanel` para estrutura em linhas/colunas;
- `FlowLayoutPanel` para sequências de LEDs/botoeiras;
- `Padding` e `Margin` para espaçamento;
- `Dock` para preenchimento controlado;
- evitar coordenadas absolutas quando o painel crescer.

## Estratégia de implementação

1. Não mexer no `MainForm` nesta etapa.
2. Criar protótipo isolado do painel manual.
3. Usar assets reais somente no protótipo.
4. Validar visual em janela/teste separado.
5. Só integrar ao `MainForm` quando o painel estiver visualmente aprovado.
6. Substituir o painel atual por inteiro, em vez de fazer remendos parciais.

## Fora de escopo

Não alterar nesta etapa:

- Modbus RTU;
- mapa de memória;
- HIstudio;
- `.dpk`;
- lógica de acionamento;
- lógica de diagnóstico;
- teste automático.

## Próxima ação

Criar um protótipo isolado do painel `IndustrialDigitalIoPanelControl` usando os LEDs industriais já versionados.

Depois validar visualmente antes de integrar ao app principal.
