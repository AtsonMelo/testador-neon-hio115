# Issue #16 — Especificação dos assets UI

## Objetivo

Definir os requisitos visuais dos assets industriais antes de aplicá-los no TestadorCLPHI.App.

Esta especificação existe para evitar tentativa e erro dentro do WinForms.

## Estratégia

O app não deve tentar desenhar componentes realistas em GDI+.

O fluxo correto será:

1. definir asset;
2. gerar PNG transparente;
3. validar asset isolado;
4. adicionar ao projeto;
5. aplicar em UserControl;
6. validar no app.

## Formato padrão

- Formato: PNG
- Fundo: transparente
- Tamanho base: 256 x 256 px
- Tema alvo: industrial dark / SCADA-like
- Sem textos embutidos na imagem
- Sem marca d'água
- Sem logo de fabricante
- Sem borda branca externa
- Boa leitura reduzido para 32 px, 48 px e 64 px

## Assets mínimos

### led_on_green.png

Sinaleira industrial verde ligada.

Características:

- lente circular verde;
- brilho central;
- leve reflexo no topo;
- aro metálico escuro;
- sombra suave;
- aparência de painel elétrico real;
- fundo transparente.

### led_off_gray.png

Sinaleira industrial desligada/cinza.

Características:

- lente circular cinza escura;
- pouco brilho;
- aro metálico escuro;
- sem glow verde;
- sombra suave;
- aparência de painel elétrico real;
- fundo transparente.

### push_button_red.png

Botoeira industrial vermelha.

Características:

- botão circular vermelho;
- base/aro metálico escuro;
- profundidade visível;
- brilho discreto;
- sombra natural;
- sem texto;
- fundo transparente.

### push_button_green.png

Botoeira industrial verde.

Características:

- botão circular verde;
- base/aro metálico escuro;
- profundidade visível;
- brilho discreto;
- sombra natural;
- sem texto;
- fundo transparente.

### stop_emergency.png

Botoeira de emergência.

Características:

- botão cogumelo vermelho;
- base amarela circular;
- aparência robusta;
- sombra natural;
- sem texto embutido;
- fundo transparente.

O texto STOP deve ser desenhado pelo app, não dentro do asset.

## Critérios de reprovação

Reprovar asset se tiver:

- aparência cartoon;
- aparência flat demais;
- excesso de brilho artificial;
- baixa nitidez;
- fundo branco;
- texto dentro da imagem;
- marca d'água;
- logo de terceiro;
- sombra cortada;
- visual inconsistente com tema escuro;
- ilegível quando reduzido.

## Ordem de implementação

1. Criar led_on_green.png.
2. Criar led_off_gray.png.
3. Criar preview isolado dos LEDs.
4. Aplicar DI00 a DI07 no painel manual.
5. Criar botoeiras DO.
6. Aplicar D000 a D003.
7. Criar STOP de emergência.
8. Aplicar STOP no header.

## Observação arquitetural

Os assets não alteram lógica de CLP.

Não alterar nesta etapa:

- Modbus RTU;
- HIstudio;
- .dpk;
- mapa de memória;
- lógica de teste;
- lógica de diagnóstico.
