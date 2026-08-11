# Issue #24 — Plano de layout responsivo do TestadorCLPHI.App

## Estado atual

A Issue #24 já possui um preview do layout alvo principal e componentes extraídos para partes específicas da interface.

Commits relevantes:

- `2825494` — adiciona preview do layout alvo principal.
- `a2efad9` — extrai preview de conexão industrial.
- `e1e806f` — extrai preview de comandos industriais.
- `01aea40` — reorganiza topo do preview layout alvo.

## Diagnóstico

A evolução por pequenos ajustes de tamanho, padding e largura começou a gerar tentativa/erro visual.

Problemas observados:

- O topo ainda tenta concentrar comandos, status e STOP em pouco espaço.
- O STOP com imagem e texto grande quebra em largura mínima.
- `Estado da conexão` e `Conexão com CLP` estão separados, mas pertencem ao mesmo contexto funcional.
- Botões secundários como `Simular erro` e `Ler %MW70` têm destaque excessivo.
- O painel ainda usa muitas áreas fixas e bordas competindo visualmente.
- A responsividade precisa vir da hierarquia dos componentes, não de ajustes numéricos isolados.

## Decisão técnica

Não continuar polindo por tentativa/erro.

A estratégia passa a ser:

1. Reduzir complexidade visual.
2. Unificar blocos relacionados.
3. Esconder ações secundárias.
4. Componentizar antes de integrar no `MainForm`.
5. Validar cada componente em largura mínima e largura normal.

## Layout alvo revisado

### Topo compacto

Conteúdo:

- Ações principais:
  - `Habilitar teste`
  - `Resetar saídas`
  - `Teste automático`
- Status compacto:
  - CLP conectado/desconectado
  - Teste habilitado/desabilitado
- STOP compacto.

Regra:

- Não usar título/subtítulo grandes no topo.
- O STOP deve permanecer sempre visível.
- Em telas estreitas, preferir botão textual compacto em vez de imagem + texto grande.

### Comunicação com CLP

Unificar em um único componente:

- Status atual.
- Mensagem.
- Última atualização.
- Porta COM.
- Baud rate.
- Slave ID.
- Atualizar portas.
- Detectar CLP.
- Lista de baud rates.

Nome sugerido:

```text
IndustrialCommunicationPreviewControl
Botões secundários devem ir para uma área avançada:

Simular erro
Ler %MW70
Desconectar, quando aplicável

Opção inicial simples:

Botão "Avançado" no canto inferior do bloco Comunicação.
Painel I/O

Regras:

DO e DI não podem cortar canais.
DO pode usar agrupamento por FlowLayoutPanel.
DI pode usar FlowLayoutPanel com quebra ou rolagem horizontal controlada.
A visualização deve funcionar em notebook e monitor maior.

Nome sugerido:

IndustrialIoPreviewControl
Terminal / Log

Regras:

Deve ter altura útil mínima.
Botão Limpar log não deve desperdiçar altura.
O log deve permanecer legível em largura mínima.

Nome sugerido:

IndustrialTerminalPreviewControl
Fases de execução
Fase 1 — Planejamento e limpeza
Descartar experimentos visuais não aprovados.
Registrar este plano.
Manter branch limpa antes de voltar ao código.
Fase 2 — Comunicação unificada

Criar:

IndustrialCommunicationPreviewControl

Substituir no preview:

CreateStatePanel()
CreateClpConnectionPanel()

por um bloco único:

CreateCommunicationPanel()

Critérios de validação:

Menos bordas.
Menos altura desperdiçada.
Status e configuração no mesmo bloco.
Botões secundários escondidos em área avançada.
Build OK.
Preview OK em largura mínima e normal.
Fase 3 — Topo definitivo

Criar ou revisar:

IndustrialHeaderPreviewControl

Critérios:

Botões principais visíveis.
STOP sempre visível.
CLP/Teste com status legível.
Sem imagem quebrada ou texto cortado.
Fase 4 — I/O responsivo

Criar ou revisar:

IndustrialIoPreviewControl

Critérios:

Nenhum canal cortado.
DO e DI organizados.
Funciona em notebook e monitor maior.
Fase 5 — Terminal responsivo

Criar ou revisar:

IndustrialTerminalPreviewControl

Critérios:

Log com altura útil.
Botão limpar log compacto.
Texto legível.
Fase 6 — Integração no MainForm

Só integrar no MainForm depois que o preview estiver visualmente aprovado.

Regra de trabalho daqui em diante

Não corrigir espaçamento antes de corrigir hierarquia.

Não polir antes de reduzir complexidade.

Não integrar no MainForm antes do preview estar aprovado.

Cada ciclo deve seguir:

estado limpo
→ um componente
→ build
→ preview em largura mínima e normal
→ validação visual
→ commit
→ push
Próximo passo imediato

Implementar:

IndustrialCommunicationPreviewControl

Objetivo:

Unificar Estado da conexão + Conexão com CLP em um único bloco mais limpo.
