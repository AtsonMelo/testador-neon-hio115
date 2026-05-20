# Issue #24 - Harmonia dos assets industriais

## Objetivo

Validar e harmonizar os assets PNG industriais antes de implementar o novo layout principal.

## Estado inspecionado

- Branch: ui/issue-24-integra-visual-industrial-main
- Status: limpo
- Último commit inspecionado: db9b361
- MainForm.cs: 685 linhas

## Assets PNG disponíveis

- led_off_gray.png
- led_on_green.png
- push_button_blue.png
- push_button_green.png
- push_button_red.png
- push_button_yellow.png
- stop_emergency.png

Todos os assets foram lidos como PNG 256x256 com PixelFormat Format32bppArgb.

## Paleta atual encontrada

| Área | Cor atual | Observação |
|---|---:|---|
| Fundo geral do app | Color.FromArgb(32, 32, 32) | Cinza neutro |
| Botões comuns | Color.FromArgb(55, 55, 55) | Cinza genérico |
| Campos | Color.FromArgb(45, 45, 45) | Cinza genérico |
| STOP | Color.FromArgb(31, 31, 31) | Escuro, pode gerar quadrado visual |
| Botoeiras industriais | Color.FromArgb(42, 52, 64) | Azul-grafite, melhor direção |
| LEDs industriais | Color.FromArgb(24, 32, 42) | Azul escuro técnico |
| Terminal/log | Color.FromArgb(18, 24, 32) | Fundo técnico escuro |

## Direção visual recomendada

Migrar gradualmente o app de cinza neutro para uma paleta azul-grafite industrial, alinhada aos PNGs de botoeiras, LEDs e terminal.

## Paleta candidata

| Uso | Cor candidata | Motivo |
|---|---:|---|
| Fundo principal | Color.FromArgb(18, 24, 32) | Base escura técnica, combina com terminal |
| Painéis/cards | Color.FromArgb(24, 32, 42) | Harmoniza com LEDs |
| Botões/cards industriais | Color.FromArgb(42, 52, 64) | Já usado nas botoeiras |
| Bordas | Color.FromArgb(72, 96, 116) | Já aparece no IndustrialPushButtonControl |
| Texto principal | Color.FromArgb(226, 232, 240) | Alto contraste sem branco puro |
| Texto secundário | Color.FromArgb(203, 213, 225) | Bom para labels e descrições |

## Teste visual recomendado antes do layout novo

1. Ajustar somente cores do tema/app atual.
2. Não alterar estrutura de layout.
3. Abrir app em notebook/DPI alto.
4. Validar STOP, botoeiras DO, LEDs DI, terminal/log e painéis.
5. Se harmonizar, commitar como ajuste visual pequeno.
6. Só depois avançar para implementação do novo layout.

## Regra do ciclo

- Não misturar refatoração com ajuste visual.
- Não mexer em HIstudio ou arquivo .dpk.
- Uma alteração pequena por vez.
- Antes de commit: check-csharp-literals.ps1, git diff --check, dotnet build e validação visual.

