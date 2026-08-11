# Issue #24 — Layout Alvo 2 do TestadorCLPHI.App

## Objetivo

Definir uma nova referência visual para a Issue #24, chamada **Layout Alvo 2**, com foco em:

- reduzir complexidade visual;
- melhorar responsividade;
- separar melhor as áreas funcionais;
- evitar ajustes por tentativa/erro;
- preparar uma futura integração limpa no `MainForm`.

## Decisão principal

O Layout Alvo 2 substitui a tentativa de encaixar tudo no Layout Alvo 1.

A nova organização será:

```text
Topo fino:
[Habilitar teste] [Resetar saídas] [Teste automático]

Área principal:
[Comunicação com CLP] [I/O Manual]

Rodapé:
[Terminal / Log]
Referência visual

A referência visual é o mockup validado na conversa em 19/05/2026, com:

janela Preview - Layout Alvo 2 | Testador CLP HI;
tema escuro industrial;
topo fino com botões principais;
bloco Comunicação com CLP à esquerda;
bloco I/O Manual à direita;
terminal ocupando toda a largura inferior;
botão obrigatório Conectar CLP dentro do bloco de comunicação.
Estrutura pretendida
1. Topo fino

Função:

Ações principais do testador

Botões:

Habilitar teste
Resetar saídas
Teste automático

Regras:

sem título grande;
sem subtítulo;
sem STOP por enquanto;
sem status CLP/Teste no topo;
altura baixa;
botões alinhados à esquerda.

Componente sugerido:

IndustrialTopCommandBarPreviewControl
2. Comunicação com CLP

Deve ficar à esquerda da área principal.

Conteúdo obrigatório:

Título: Comunicação com CLP

Status: Disconnected
Mensagem: Desconectado

Porta COM: COM1
Baud rate: 9600
Slave ID: 1

Rodapé:
Pronto para conectar
Conectar CLP
Configuração salva
Avançado

Regras:

o botão Conectar CLP deve aparecer claramente;
Simular erro não fica visível na tela principal;
Ler %MW70 não fica visível na tela principal;
opções secundárias devem ir para Avançado;
layout deve ser limpo, vertical e bem alinhado.

Componente sugerido:

IndustrialCommunicationLayout2PreviewControl
3. I/O Manual

Deve ficar à direita da área principal.

Estrutura:

Saídas digitais (D)
D000
D001
D002
D003

Entradas digitais (DI)
DI00  DI04
DI01  DI05
DI02  DI06
DI03  DI07

Regras:

DO em coluna vertical;
DI em duas colunas de quatro;
nenhum canal pode cortar;
botões e LEDs devem manter aparência industrial;
deve funcionar em 1100x760 e 1366x820.

Componente sugerido:

IndustrialIoLayout2PreviewControl
4. Terminal / Log

Deve ocupar o rodapé inteiro.

Conteúdo:

Título: Terminal / Log
Área preta com logs
Botão Limpar log

Regras:

terminal deve ter boa altura útil;
botão Limpar log deve ficar no cabeçalho ou canto direito sem desperdiçar espaço;
texto deve ser monoespaçado e legível.

Componente sugerido:

IndustrialTerminalLayout2PreviewControl
Fases de implementação
Fase 1 — Documentação da referência

Commit:

docs: registra layout alvo 2 da issue 24

Entrega:

docs/design/issue-24-layout-alvo-2.md
Fase 2 — Preview isolado

Criar:

IndustrialLayoutAlvo2PreviewForm.cs

Adicionar argumento:

--preview-layout-alvo-2

Commit:

ui: adiciona preview do layout alvo 2
Fase 3 — Topo fino

Criar:

IndustrialTopCommandBarPreviewControl.cs

Commit:

ui: cria barra superior do layout alvo 2
Fase 4 — Comunicação com CLP

Criar:

IndustrialCommunicationLayout2PreviewControl.cs

Commit:

ui: cria comunicacao do layout alvo 2
Fase 5 — I/O Manual

Criar:

IndustrialIoLayout2PreviewControl.cs

Commit:

ui: cria io manual do layout alvo 2
Fase 6 — Terminal

Criar:

IndustrialTerminalLayout2PreviewControl.cs

Commit:

ui: cria terminal do layout alvo 2
Fase 7 — Responsividade

Validar em:

1100x760
1366x820
1920x1200
janela maximizada

Commit:

ui: ajusta responsividade do layout alvo 2
Fase 8 — Integração no MainForm

Somente depois do preview aprovado.

Commit futuro:

ui: integra layout alvo 2 no app principal
Critérios de aprovação

O Layout Alvo 2 só será considerado pronto para integração quando:

abrir bem em 1100x760;
abrir bem em 1366x820;
comunicação ficar limpa à esquerda;
I/O ficar organizado à direita;
terminal ocupar bem o rodapé;
nenhum texto importante cortar;
botão Conectar CLP aparecer claramente;
ações secundárias ficarem fora da tela principal;
MainForm permanecer intacto até aprovação do preview.
Regra de trabalho

Não adaptar o MainForm diretamente.

Fluxo obrigatório:

documentar referência
→ criar preview isolado
→ criar um componente por vez
→ validar visual
→ build
→ commit
→ push
→ próximo componente
