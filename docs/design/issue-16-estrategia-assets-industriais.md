# Issue #16 — Estratégia de assets industriais

## Decisão técnica

Para aproximar o TestadorCLPHI.App do visual alvo, a UI não deve depender de desenhos manuais em GDI+/PaintEvent para componentes industriais complexos.

A estratégia oficial passa a ser:

1. definir o visual alvo;
2. gerar assets próprios;
3. validar os assets isoladamente;
4. aplicar os assets em controles pequenos;
5. validar visualmente no app;
6. só então expandir para o restante da interface.

## Motivo

Durante os experimentos com STOP, DO e DI desenhados manualmente, o visual ficou distante do alvo e com aparência cartunesca.

Para componentes como botoeiras, sinaleiras e botão de emergência, assets PNG/SVG oferecem:

- melhor acabamento;
- menos ajuste fino manual;
- menor retrabalho;
- maior consistência visual;
- melhor harmonia com o tema escuro;
- melhor aproximação com o mockup industrial/SCADA-like.

## Regra contra tentativa e erro

Não continuar tentando obter realismo industrial ajustando manualmente sombras, gradientes, brilhos e relevos em GDI+.

WinForms deve montar a interface.

Os assets devem carregar o peso visual:

- sombra;
- brilho;
- profundidade;
- textura;
- borda;
- aparência industrial.

## Diretório dos assets

- app/TestadorCLPHI.App/Assets/Ui/

## Assets planejados

- stop_emergency.png
- push_button_red.png
- push_button_green.png
- push_button_yellow.png
- push_button_blue.png
- led_on_green.png
- led_off_gray.png

## Componentes planejados

- AssetPushButtonControl
- AssetLedIndicatorControl
- AssetEmergencyStopControl

## Status atual

Já existe base técnica para a estratégia por assets:

- diretório Assets/Ui;
- documentação inicial dos assets;
- AssetLedIndicatorControl;
- AssetPushButtonControl.

Commit relacionado:

- ca0a181 ui: adiciona base de controles industriais por assets

## Critérios de aceitação visual

Um asset só deve ser aplicado no painel se atender aos critérios abaixo:

- aparência industrial;
- sem aparência cartoon;
- leitura clara em tamanho pequeno;
- fundo transparente ou compatível com tema escuro;
- sombra coerente;
- brilho visível;
- borda definida;
- proporção compatível com o mockup alvo;
- boa harmonia com cards escuros;
- sem marca d'água;
- sem logos de terceiros;
- origem própria ou segura para versionamento.

## Aplicação inicial

A primeira aplicação deve ser no painel manual 4DO/8DI.

### DO

D000 a D003 devem ter aparência de botoeiras industriais.

Mapeamento atual:

- D000 -> DI00 + DI04
- D001 -> DI01 + DI05
- D002 -> DI02 + DI06
- D003 -> DI03 + DI07

### DI

DI00 a DI07 devem ter aparência de sinaleiras/LEDs de painel.

## Ordem correta a partir daqui

1. Gerar/adicionar led_on_green.png e led_off_gray.png.
2. Validar os LEDs visualmente antes de mexer no painel.
3. Aplicar os LEDs no DigitalIoManualPanelControl.
4. Validar se DI00 a DI07 ficaram coerentes com o mockup.
5. Gerar/adicionar botoeiras DO.
6. Aplicar botoeiras DO.
7. Gerar/adicionar STOP de emergência.
8. Aplicar STOP por último.

## Regras arquiteturais

- Não adicionar lógica pesada no MainForm.cs.
- MainForm apenas instancia controles, liga eventos e atualiza estado visual.
- UserControls cuidam do layout e aparência.
- Services cuidam de lógica técnica.
- Assets/Ui concentra imagens reutilizáveis da interface.

## Fora do escopo desta etapa

Não alterar:

- Modbus RTU;
- mapa de memória;
- HIstudio;
- .dpk;
- lógica de acionamento;
- lógica de diagnóstico;
- teste automático.

## Próxima ação recomendada

Gerar/adicionar primeiro os assets reais dos LEDs:

- led_on_green.png
- led_off_gray.png

Depois aplicar somente os LEDs DI no painel manual.
