# Assets da interface industrial

Este diretório concentra os assets visuais usados pela interface do TestadorCLPHI.App.

## Objetivo

Aproximar a UI do visual industrial definido na Issue #16, evitando desenhar componentes realistas complexos diretamente via GDI+ dentro dos controles WinForms.

## Estratégia

Usar imagens próprias em PNG/SVG para componentes visuais industriais, mantendo os controles C# responsáveis apenas por:

- carregar o asset;
- posicionar imagem e texto;
- aplicar estados visuais simples;
- disparar eventos de clique;
- respeitar o tema claro/escuro.

## Assets planejados

- stop_emergency.png
- push_button_red.png
- push_button_green.png
- push_button_yellow.png
- push_button_blue.png
- led_on_green.png
- led_off_gray.png

## Controles planejados

- AssetPushButtonControl
- AssetLedIndicatorControl
- AssetEmergencyStopControl

## Regra arquitetural

Não colocar lógica pesada no MainForm.cs.

- MainForm: instancia controles, liga eventos e atualiza estado visual.
- UserControls: layout e apresentação visual.
- Services: lógica técnica.
- Assets/Ui: imagens reutilizáveis da interface.
