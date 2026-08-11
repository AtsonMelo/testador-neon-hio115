# UI Industrial 2.0

## Escopo e decisões

A UI2 moderniza somente a apresentação da plataforma industrial em WinForms/.NET 10. A lógica de domínio, os mapas de I/O, a simulação, os perfis, a SafetyChain e os gates de bancada permanecem inalterados. `MainForm` continua legado e só é alcançado por `--legacy`.

A produção usa uma única fundação visual em `Ui/Theme`: `IndustrialPalette`, `IndustrialTheme`, `IndustrialTypography` e `IndustrialSpacing`. `PlatformUi` é uma fachada de compatibilidade que consome esses tokens. `Layout3ThemePalette`, previews e o tema do legado continuam isolados; nenhum código novo de produção depende deles.

Os temas disponíveis são Escuro, Claro e Windows. A troca reaplica tokens nos controles existentes e não reconstrói Testador, Simulador, mapa de I/O ou Pivô. Assim, sessão, cenário, seleção, log e editores permanecem vivos.

## Composição

- Shell: cabeçalho de produto/equipamento/modo/segurança, navegação Testador e Simulador, conteúdo persistente e status global explicitamente offline.
- Testador: identificação, descoberta, leitura, cancelamento, saídas supervisionadas, mapa, diagnóstico e log permanecem acessíveis; o log é atualizado incrementalmente.
- Simulador: perfil, cenário, entradas editáveis, sinais derivados, saídas virtuais, falhas e reset permanecem acessíveis; mudanças de valor não recriam os editores.
- Mapa de I/O: busca e filtro são locais e não alteram binding, canal, sinal, registro ou evidência.
- Pivô: pintura GDI+ event-driven, quatro torres, estado de segurança textual e visual, sem timer decorativo.

## Responsividade, DPI e acessibilidade

O shell usa `AutoScaleMode.Dpi`, dimensões lógicas e `DeviceDpi`. A sidebar alterna entre 224 e 56 pixels lógicos, preservando navegação, tooltips, nomes acessíveis e SafetyChain. Os layouts estruturais são validados em 1366x768, 1600x900, 1920x1080 e 2560x1440. Controles customizados desenham pelo `ClientRectangle` e foram exercitados em escalas equivalentes a 100%, 125% e 150%.

Estados críticos combinam texto, símbolo e cor. SafetyChain é sempre derivada/read-only e não se parece com entrada editável. Fontes operacionais respeitam mínimo de 8,5 pt; botões importantes têm alvos de interação adequados, foco e teclado.

## Segurança e validação

Nenhuma tela UI2 cria transporte físico. OFFLINE, EM MEMÓRIA, SIMULAÇÃO e FÍSICA BLOQUEADA são explícitos. Os validadores verificam startup, feature parity, identidade persistente dos controles, navegação repetida, troca de tema, contraste, layouts, acessibilidade, estabilidade estrutural/GDI e contadores físicos zerados.

O pacote portátil executa apenas validadores offline. BenchReadiness permanece uma validação local, deliberadamente bloqueada até existirem evidências reais de bancada.

## Dívida visual preservada

Layout3, previews e controles históricos não foram apagados nem promovidos automaticamente. Eles permanecem como referência/legado para evitar regressão e podem ser consolidados em trabalho posterior somente com prova funcional e visual específica.
