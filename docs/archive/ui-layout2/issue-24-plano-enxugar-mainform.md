# Issue #24 — Plano para enxugar a MainForm antes de integrar o Layout Alvo 2

## Contexto

A `MainForm.cs` cresceu demais e passou a concentrar responsabilidades diferentes:

- construção visual da janela;
- configuração de conexão CLP;
- atualização de portas COM;
- detecção automática do CLP;
- conexão/desconexão;
- comandos de teste;
- comandos de saídas digitais;
- leitura de entradas digitais;
- terminal/log;
- tema visual.

Antes de integrar o Layout Alvo 2 no app principal, a prioridade é reduzir a responsabilidade da `MainForm`.

## Decisão técnica

Não integrar o Layout Alvo 2 diretamente na `MainForm` enquanto ela estiver grande e concentrando muita lógica.

A estratégia será:

```text
enxugar MainForm
→ extrair componentes reais
→ integrar Layout Alvo 2 por partes
→ validar comportamento existente
Regras da refatoração

A refatoração deve preservar comportamento.

Não fazer nesta etapa:

mudar visual de forma intencional;
trocar layout principal;
alterar lógica Modbus;
alterar endereços %MW;
alterar mapeamento HIO115;
misturar refatoração com nova feature.

Fazer sempre:

um bloco por vez;
build após cada mudança;
teste manual quando aplicável;
commit pequeno;
status limpo antes do próximo passo.
Fase 1 — Mapear responsabilidades da MainForm

Estado identificado:

MainForm.cs contém construção de UI e handlers de negócio.
Comunicação, I/O, comandos e terminal estão acoplados na mesma classe.
Já existem controles customizados úteis que devem ser reaproveitados.

Controles existentes relevantes:

ConnectionStatePanelControl
TesterCommandPanelControl
DigitalIoManualPanelControl
TerminalLogPanelControl
ThemeSelectorControl
IndustrialLayoutAlvo2PreviewForm
Fase 2 — Extrair configuração visual inicial

Objetivo:

Reduzir código de layout e preparação visual dentro da MainForm.

Candidatos:

MinimumSize
AutoScroll
Theme selector
Header
TableLayoutPanel principal
Tabs inferiores
Dock/Margin/Anchor dos painéis

Possível nome:

MainFormLayoutFactory

ou, se for melhor manter simples:

MainFormLayoutBuilder

Critério:

MainForm continua criando os controles principais;
builder só organiza layout;
eventos continuam na MainForm.

Commit sugerido:

refactor: extrai organizacao inicial da mainform
Fase 3 — Extrair painel de comunicação real

Objetivo:

Criar um controle real inspirado no Layout Alvo 2:

IndustrialCommunicationLayout2Control

Deve encapsular:

porta COM;
baud rate;
slave ID;
botão conectar;
botão detectar;
botão avançado;
status de conexão;
resumo.

A MainForm deve continuar controlando a lógica através de eventos e propriedades.

Commit sugerido:

ui: cria controle de comunicacao layout alvo 2
Fase 4 — Integrar comunicação na MainForm

Substituir gradualmente:

ConnectionStatePanelControl + GroupBox Conexão com CLP

por:

IndustrialCommunicationLayout2Control

Critério:

conectar CLP continua funcionando;
detectar CLP continua funcionando;
atualização de portas continua funcionando;
leitura %MW70 deve ficar em área avançada ou fora da tela principal;
simular erro não deve ficar em destaque.

Commit sugerido:

ui: integra comunicacao layout alvo 2 na mainform
Fase 5 — Adaptar comandos superiores

Objetivo:

Criar ou adaptar barra superior com:

Habilitar teste
Resetar saídas
Teste automático

Ações devem chamar os handlers já existentes.

Commit sugerido:

ui: integra barra superior do layout alvo 2
Fase 6 — Adaptar I/O Manual

Reaproveitar e evoluir:

DigitalIoManualPanelControl

Objetivo visual:

saídas digitais em coluna;
entradas digitais em duas colunas;
LEDs sem corte;
botões e LEDs com visual industrial.

Commit sugerido:

ui: adapta io manual ao layout alvo 2
Fase 7 — Adaptar terminal/log

Reaproveitar:

TerminalLogPanelControl

Objetivo:

terminal no rodapé;
botão limpar log compacto;
texto monoespaçado;
visual dark industrial.

Commit sugerido:

ui: adapta terminal ao layout alvo 2
Fase 8 — Limpeza final

Depois da integração por partes:

remover métodos auxiliares não usados;
remover campos obsoletos;
reduzir linhas da MainForm;
revisar nomes;
validar build;
validar teste manual.

Commit sugerido:

refactor: remove layout legado da mainform
Critérios de sucesso

A etapa de enxugamento será considerada boa quando:

a MainForm ficar menor;
o layout principal for organizado por componentes;
a lógica existente continuar funcionando;
o preview Layout Alvo 2 continuar preservado;
o app principal puder evoluir sem mexer em centenas de linhas por vez.
Próximo passo imediato

Executar a primeira refatoração pequena:

refactor: extrai organizacao inicial da mainform

Escopo desse primeiro código:

não mudar visual;
não mudar lógica;

apenas separar organização visual inicial em método/classe auxiliar segura.
