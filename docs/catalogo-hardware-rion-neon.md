# Catalogo de hardware RION/NEON

## Objetivo

Este documento descreve a base interna criada em
`app/TestadorCLPHI.App/Data/Hardware/hi-hardware-catalog.json`.

A base separa cadastro de hardware, comunicacao e perfis de teste sem alterar
arquivos HIstudio e sem mudar a logica atual de acionamento do app.

## Conceitos

| Conceito | Uso no catalogo |
|---|---|
| Familia | Linha ou geracao de produto, como `NEON_LEGACY`, `NEON_5`, `RION_LEGACY`, `RION_PLUS` e `RION_5`. |
| Modelo de controlador | Equipamento selecionavel pelo testador, como `NEON-1S` ou `RION-502`. |
| CPU | Identificacao da CPU observada no controlador, como `CPU401` ou `CPU502`. Nao substitui o modelo do controlador. |
| Modulo de I/O | Modulo acoplado ao controlador ou remoto, como `HIO115` ou `DIO605`. |
| Perfil de comunicacao | Parametros de transporte/protocolo, por exemplo RS485 38400 8N1 Modbus RTU. |
| Perfil de teste | Procedimento logico de validacao, com prerequisitos, acoes, criterios e cuidados. |

## Estrutura

O JSON possui cinco secoes principais:

- `families`: familias de hardware.
- `models`: modelos de controlador.
- `ioModules`: modulos de I/O.
- `communicationProfiles`: perfis de comunicacao.
- `testProfiles`: perfis de teste.

Os itens usam campos como `id`, `displayName`, `family`,
`generation`, `sourceStatus`, `validationStatus`, `notes`,
`officialReferences`, `supportedSlots`, `defaultCommunicationProfiles`,
`supportedIoModules` e `testProfiles`.

`officialReferences` e uma lista informativa. Cada entrada guarda `title`,
`url`, `sourceType` e `note`. Ela documenta a origem oficial consultada, mas
nao cria validacao operacional.

## Uso pela interface de selecao

A aba `Perfil hardware`, disponivel no modo normal do Testador, le este
catalogo e permite selecionar:

- familia;
- modelo;
- modulo de I/O;
- perfil de comunicacao;
- perfil de teste.

A selecao e informativa. Ela nao altera porta COM, baud rate, slave ID,
paridade, timeout ou qualquer parametro real de Modbus. Tambem nao envia
comandos fisicos ao CLP. Os parametros reais continuam no painel de conexao
normal e os comandos continuam nos paineis de teste existentes.

A UI mostra resumo, modulos compativeis, perfis possiveis, testes aplicaveis,
itens `field_observed`, pendencias e necessidades de validacao em bancada.
Quando qualquer item estiver como `pending_manual_validation`, isso deve
continuar explicito ate haver registro real de bancada.

Quando o combo de modulo mostrar "nenhum modulo confirmado para este modelo",
isso significa ausencia de associacao confirmada no catalogo. Nao deve ser
interpretado como falha de comunicacao com o CLP.

## Validacao da selecao

Use tambem o validador nao visual:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-profile-selection
```

Ele carrega este catalogo e exercita o resolver que alimenta a aba
`Perfil hardware`. Os cenarios cobrem NEON-1S + DIO605, RION-502 + HIO115,
`COMMUNICATION_DIAGNOSTIC`, `REMOTE_IO_RS485` e modelos NEON 5/RION 5 com
dados pendentes. O objetivo e validar consistencia catalogo/resolver/UI:
ids existentes precisam resolver, listas compativeis precisam conter os itens
esperados, pendencias continuam expostas e `field_observed` aparece quando
aplicavel.

O comando e nao visual e nao fisico. Ele nao altera parametros reais, nao
executa escrita Modbus, nao envia comando a PLC e nao substitui validacao em
bancada.

## Status

Valores usados:

- `field_observed`: item observado em bancada ou no historico do projeto.
- `official_reference`: ha referencia oficial HI Tecnologia registrada, mas
  isso nao equivale a validacao de bancada.
- `official_reference_pending`: cadastro depende de referencia oficial.
- `pending_manual_validation`: precisa de teste manual antes de uso operacional.
- `verified_in_bench`: validado em bancada e acompanhado de observacao.

Referencia oficial, item observado e bancada validada sao evidencias
diferentes:

- referencia oficial confirma apenas que uma informacao foi encontrada em
  fonte HI Tecnologia;
- `field_observed` preserva historico do projeto, mas pode continuar pendente
  por conjunto, mapa ou procedimento;
- `verified_in_bench` so deve ser usado quando houver resultado de bancada
  documentado.

Dados incompletos ficam como pendentes. Nao cadastrar pinagem, quantidade de
pontos, mapa de registradores ou especificacao eletrica sem confirmacao.

## Referencias oficiais adicionadas

RION 5:

- Pagina oficial: https://www.hitecnologia.com.br/produto/rion/
- Manual do usuario RION-5:
  https://materiais.hitecnologia.com.br/downloads/pmuc0150200.pdf
- Navegacao oficial de hardware:
  https://www.hitecnologia.com.br/categoria-produto/hardware/

O catalogo registra de forma conservadora que RION 5 pode operar como CLP
e/ou I/O remoto, suporta 1 modulo, aparece como ate 16 pontos de I/O na
navegacao oficial e pode ter ate 14 unidades RION 5 como I/O remoto ao CLP
NEON. Isso permanece `pending_manual_validation`.

NEON 5:

- Navegacao oficial de hardware:
  https://www.hitecnologia.com.br/categoria-produto/hardware/
- Base de conhecimento/navegacao oficial:
  https://www.hitecnologia.com.br/base-de-conhecimento/

O catalogo registra NEON 5 como ate 240 pontos de I/O conforme navegacao
oficial. A referencia do RION 5 cita expansao por RION 5 como I/O remoto ao
CLP NEON, mas o catalogo nao valida topologia, CPU, modulos ou mapas para
NEON 5.

Modulos com referencia adicionada:

- HIO115:
  https://materiais.hitecnologia.com.br/downloads/pmu11111500.pdf
- HIO130:
  https://materiais.hitecnologia.com.br/downloads/PMU11113000.pdf
- HIO140:
  https://materiais.hitecnologia.com.br/downloads/pmu11114000.pdf
- HIO165:
  https://materiais.hitecnologia.com.br/downloads/pmu11116500.pdf
- DIO605:
  https://materiais.hitecnologia.com.br/downloads/pmu11160500.pdf

Esses links nao adicionam pinagem, detalhes eletricos, mapas exatos ou
contagens de canais ao schema atual. Eles apenas documentam a fonte oficial
para futura validacao manual.

## Como cadastrar novo modelo

1. Adicionar o item em `models`.
2. Usar um `id` unico no catalogo inteiro.
3. Apontar `family` para uma familia existente.
4. Informar `controllerCpu` somente quando a CPU tiver sido observada ou
   confirmada.
5. Preencher `supportedIoModules` apenas com modulos existentes no catalogo.
6. Preencher `defaultCommunicationProfiles` apenas com perfis existentes.
7. Deixar `validationStatus` como `pending_manual_validation` ate haver
   registro de bancada.
8. Se `sourceStatus` for `official_reference`, preencher `officialReferences`
   com URL oficial.
9. Executar a validacao do catalogo e o build.
10. Abrir a aba `Perfil hardware` e conferir se o modelo aparece sem criar
   associacoes tecnicas nao confirmadas.

## Como cadastrar novo modulo

1. Adicionar o item em `ioModules`.
2. Usar `family` existente.
3. Informar `supportedFamilies` somente quando a aplicabilidade for conhecida.
4. Informar `testProfiles` somente quando o procedimento for adequado ao modulo.
5. Nao copiar mapa, pontos ou pinagem de outro modulo por semelhanca de nome.
6. Marcar dados nao confirmados como `pending_manual_validation` ou
   `official_reference_pending`.
7. Se `sourceStatus` for `official_reference`, preencher `officialReferences`
   com URL oficial.
8. Executar a validacao do catalogo, o build e conferir a aba `Perfil hardware`.

## Como marcar validado em bancada

Para trocar um modelo para `verified_in_bench`:

1. Confirmar controlador, CPU, modulo, slot e perfil de comunicacao usados.
2. Registrar data, bancada, programa HIstudio carregado e resultado do teste.
3. Preencher `validationNotes` no modelo.
4. Garantir que o conjunto realmente passou no perfil de teste indicado.
5. Rodar o validador; ele bloqueia modelo `verified_in_bench` sem
   `validationNotes`.

## Radio e XCTU

`WIRELESS_RADIO_TRANSPARENT` e apenas um perfil de comunicacao/configuracao.
O app nao configura radio XBee/R9X307 nesta milestone.

Configuracao profunda de radio deve ser feita como processo externo via XCTU.
Nao confundir comunicacao normal do CLP com configuracao interna do radio.

Selecionar `WIRELESS_RADIO_TRANSPARENT` na UI nao abre XCTU, nao aplica
parametros de radio e nao substitui a validacao serial/Modbus normal do CLP.

## Porta COM

HIstudio e Testador nao devem usar a mesma COM simultaneamente. Antes de testar
no app, desconectar ou fechar a sessao que estiver usando a porta no HIstudio,
XCTU ou outro software serial.
