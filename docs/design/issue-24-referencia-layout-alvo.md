# Issue #24 — Referência do layout alvo

## Decisão

A Issue #24 deve usar como referência visual principal o layout alvo já registrado na Issue #16.

Documento de origem:

- `docs/design/issue-16-visual-alvo.md`

## Motivo

O layout alvo já tinha sido definido anteriormente durante a Issue #16, incluindo a direção visual industrial do `TestadorCLPHI.App`.

Portanto, a Issue #24 não deve criar um layout genérico novo. Ela deve evoluir o app principal em direção ao layout alvo existente.

## Referência visual

O layout alvo considera:

- tema escuro industrial;
- header superior com título `Testador CLP HI`;
- status de CLP e teste;
- STOP grande de emergência;
- cards/containers para estado da conexão e conexão com CLP;
- comandos do testador em seção própria;
- painel manual 4DO/8DI com botoeiras industriais D000-D003;
- LEDs DI00-DI07 no mesmo bloco visual;
- terminal/log inferior com mensagens coloridas;
- botão para limpar log;
- visual mais próximo de painel técnico profissional.

## Estado atual da Issue #24

A implementação atual é um checkpoint técnico, não o visual final.

Já foram feitos avanços em:

- responsividade inicial;
- suporte a notebook/DPI alto;
- organização de I/O Manual e Terminal/Log em abas;
- checagem contra literais problemáticos em C#;
- integração parcial do visual industrial.

Ainda precisa melhorar:

- acabamento visual dos cards;
- hierarquia do header;
- densidade do painel I/O;
- terminal/log visual;
- alinhamento com o layout alvo da Issue #16.

## Próximo passo recomendado

Antes de continuar alterando layout por tentativa/erro, revisar o documento:

- `docs/design/issue-16-visual-alvo.md`

Depois implementar por componentes visuais reutilizáveis, por exemplo:

1. painel/card industrial base;
2. header industrial;
3. botão industrial com ícone;
4. indicador de status;
5. terminal/log estilizado;
6. painel I/O refinado.
