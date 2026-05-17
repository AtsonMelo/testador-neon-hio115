# Issue #16 — Estratégia de assets industriais

## Decisão

Para aproximar o TestadorCLPHI.App do visual alvo, a UI não deve depender apenas de desenhos manuais em GDI+ para componentes industriais complexos.

A estratégia passa a ser usar assets visuais reutilizáveis para elementos com aparência realista.

## Motivo

Durante os experimentos com STOP, DO e DI desenhados manualmente, o visual ficou distante do alvo e com aparência cartunesca.

Para componentes como botoeiras, sinaleiras e botão de emergência, assets PNG/SVG oferecem melhor acabamento, menos retrabalho e maior consistência visual.

## Assets planejados

Diretório sugerido:

```text
app/TestadorCLPHI.App/Assets/Ui/
Arquivos planejados:

stop_emergency.png
push_button_red.png
push_button_green.png
push_button_yellow.png
push_button_blue.png
led_on_green.png
led_off_gray.png
Componentes planejados
AssetPushButtonControl
AssetLedIndicatorControl
AssetEmergencyStopControl
Aplicação inicial

A primeira aplicação deve ser no painel manual 4DO/8DI:

D000 a D003 com aparência de botoeiras industriais;
DI00 a DI07 com aparência de sinaleiras/LEDs;
manter lógica atual de clique e leitura;
não adicionar regra pesada no MainForm.
Regras
MainForm apenas instancia controles, liga eventos e atualiza estado visual.
UserControls cuidam do layout e aparência.
Services cuidam de lógica técnica.
Assets devem ser locais do projeto e versionados.
Evitar imagens com marca d'água, logos de terceiros ou origem duvidosa.
Preferir assets próprios gerados para o projeto.
Ordem sugerida
Gerar assets próprios para botoeiras e LEDs.
Adicionar assets ao projeto.
Criar AssetLedIndicatorControl.
Criar AssetPushButtonControl.
Aplicar no DigitalIoManualPanelControl.
Validar visual.
Depois voltar ao STOP final.
