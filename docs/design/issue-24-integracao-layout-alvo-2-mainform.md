# Issue #24 - Integração do Layout Alvo 2 na MainForm

## Decisão técnica

O Preview Layout Alvo 2 foi aprovado como referência visual principal para a próxima integração gradual na MainForm.

Arquivo de origem do preview:

- app/TestadorCLPHI.App/Ui/Controls/IndustrialLayoutAlvo2PreviewForm.cs

Argumento validado:

- --preview-layout-alvo-2

## Estado atual

- Branch: ui/issue-24-integra-visual-industrial-main
- Status: limpo
- MainForm.cs: 685 linhas
- IndustrialLayoutAlvo2PreviewForm.cs: 609 linhas
- Último commit relevante: 4c90340 - ui: aplica botoeiras e leds png no layout alvo 2

## Resultado visual aprovado

O Layout Alvo 2 ficou visualmente bom usando os PNGs reais:

- push_button_red.png para D000
- push_button_green.png para D001
- push_button_yellow.png para D002
- push_button_blue.png para D003
- led_on_green.png para entradas digitais ativas
- led_off_gray.png para entradas digitais inativas

Também foi validado que os LEDs DI ficaram proporcionais com tamanho 36 e coluna 54F.

## Problema a evitar

Não devemos copiar o conteúdo do preview diretamente para a MainForm, pois isso aumentaria novamente a complexidade da MainForm.

A MainForm já foi reduzida e organizada por services. A integração visual precisa preservar esse ganho.

## Regra obrigatória de arquitetura

A MainForm não deve voltar a crescer com código de layout detalhado.

Qualquer bloco visual novo deve nascer em componente separado, preferencialmente dentro de app/TestadorCLPHI.App/Ui/Industrial.

A MainForm deve receber apenas o mínimo necessário para:

- instanciar o componente
- posicionar o componente
- ligar eventos
- repassar estados para atualização visual

Se uma alteração exigir muitas linhas novas na MainForm, ela deve ser interrompida e redesenhada como componente separado antes de continuar.

## Direção arquitetural

Criar componentes visuais separados e reutilizáveis, deixando a MainForm apenas como composição e ligação de eventos.

Sugestão de pasta:

- app/TestadorCLPHI.App/Ui/Industrial

Componentes sugeridos:

- IndustrialShellLayoutControl.cs
- IndustrialTopCommandPanelControl.cs
- IndustrialConnectionPanelControl.cs
- IndustrialManualIoPanelControl.cs
- IndustrialTerminalLogPanelControl.cs
- IndustrialTheme.cs

## Responsabilidade da MainForm

A MainForm deve ficar responsável apenas por:

- instanciar os componentes principais
- ligar eventos de clique
- chamar services existentes
- atualizar estados visuais dos componentes
- aplicar tema quando necessário

A MainForm não deve conter desenho detalhado do layout industrial.

## Ordem recomendada de integração

1. Extrair o painel I/O industrial aprovado do Layout Alvo 2.
2. Ligar o painel I/O industrial aos eventos já existentes de D000-D003 e DI00-DI07.
3. Integrar o painel de conexão industrial.
4. Integrar o terminal/log industrial.
5. Integrar o topo/comandos superiores.
6. Remover ou arquivar previews somente depois da MainForm estar validada.

## Primeiro bloco recomendado

O primeiro bloco real deve ser o painel I/O industrial, porque:

- já está visualmente aprovado
- usa PNGs reais
- tem escopo bem delimitado
- já existe lógica separada em MainFormDigitalIoUiService
- reduz risco de mexer no layout inteiro de uma vez

## Regras dos próximos ciclos

- Não misturar nova refatoração com alteração visual ampla.
- Não mexer em HIstudio ou arquivo .dpk.
- Uma integração por commit.
- Antes de commit: check-csharp-literals.ps1, git diff --check, dotnet build e validação visual.
- Tratar aviso LF/CRLF como não bloqueante quando for o único aviso.
- Manter o stash antigo do STOP apenas como backup até decidir descartar.

## Stash conhecido

- stash@{0}: wip: ajuste stop layout atual antes de integrar pngs no preview

## Próximo passo

Criar o primeiro componente real em pasta separada para o painel I/O industrial, sem aumentar a MainForm.
