# Selecao de perfil de hardware

## Objetivo

A aba `Perfil hardware` prepara a selecao de familias, modelos, modulos,
comunicacao e perfis de teste RION/NEON usando o catalogo em
`app/TestadorCLPHI.App/Data/Hardware/hi-hardware-catalog.json`.

Ela nao substitui a validacao em bancada.

## Como usar

1. Abrir o Testador em modo normal.
2. Entrar na aba `Perfil hardware`.
3. Selecionar a familia.
4. Selecionar o modelo.
5. Selecionar o modulo de I/O.
6. Selecionar o perfil de comunicacao.
7. Selecionar o perfil de teste.
8. Ler o relatorio/checklist, os itens pendentes e as necessidades de
   validacao.
9. Usar `Copiar resumo de teste` para copiar o relatorio para registro de
   bancada ou roteiro do operador.

A selecao e informativa. Ela nao altera porta, baud rate, slave ID, paridade,
timeout ou qualquer parametro real de Modbus. Tambem nao envia comandos ao CLP.
Gerar ou copiar o relatorio segue a mesma regra: nao abre conexao, nao escreve
em PLC e nao executa acao fisica.

## Relatorio de preparacao de teste

A area de texto da aba `Perfil hardware` mostra um relatorio de preparacao de
teste gerado a partir do perfil selecionado. O botao `Copiar resumo de teste`
apenas copia esse texto para a area de transferencia.

O relatorio inclui:

- familia selecionada;
- modelo selecionado;
- modulo de I/O selecionado;
- perfil de comunicacao selecionado;
- perfil de teste selecionado;
- status de validacao do catalogo;
- itens `pending_manual_validation`, `official_reference` ou
  `official_reference_pending`;
- itens `field_observed`;
- necessidades de validacao em bancada;
- checklist do operador antes do teste;
- aviso de conflito de COM;
- aviso para nao usar HIstudio, XCTU e Testador na mesma COM ao mesmo tempo;
- aviso de que a selecao e o relatorio nao executam comandos fisicos;
- aviso de que validacao em bancada ainda e obrigatoria.

Gerar ou copiar o relatorio nao abre conexao, nao altera configuracao e nao
envia comando fisico. O relatorio tambem nao substitui validacao de bancada:
ele serve para preparar a execucao, organizar pendencias e registrar o que deve
ser conferido antes do teste real.

## Validacao nao visual

O comando abaixo valida a selecao sem abrir a UI WinForms:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-profile-selection
```

Ele carrega o catalogo, executa cenarios do resolver para NEON-1S + DIO605,
RION-502 + HIO115, `COMMUNICATION_DIAGNOSTIC`, `REMOTE_IO_RS485` e modelos
NEON 5/RION 5 pendentes. O comando verifica que os ids existentes resolvem,
que pendencias/manual validation continuam visiveis, que itens
`field_observed` aparecem quando aplicavel e que a selecao segue consistente
com as listas usadas pela UI.

Esta validacao e nao visual e nao fisica: nao abre tela, nao altera
parametros Modbus, nao escreve em PLC e nao envia comandos. Ela nao substitui
validacao em bancada; apenas cobre consistencia entre catalogo, resolver e UI.

O comando abaixo valida a geracao do relatorio de preparacao sem abrir a UI:

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-test-report
```

Ele carrega o catalogo, resolve os cenarios principais, gera um relatorio para
cada um e verifica que os relatorios nao ficam vazios, mantem pendencias
explicitas e contem os avisos de seguranca sobre comando fisico, COM e bancada.

## O que a tela mostra

- perfil selecionado;
- status de validacao;
- `pending_manual_validation` quando existir;
- modulos compativeis;
- perfis de comunicacao possiveis;
- testes aplicaveis;
- itens `field_observed`;
- pendencias e necessidades de bancada.

Se o catalogo estiver vazio ou indisponivel, a tela deve continuar segura,
mostrar a indisponibilidade e nao executar nenhuma acao fisica.

Se o campo de modulo mostrar "nenhum modulo confirmado para este modelo", a
mensagem significa que nao existe modulo associado com confianca suficiente no
catalogo para o modelo escolhido. Isso nao significa erro de COM, falha Modbus
ou resposta negativa do CLP.

## Estados de evidencia

- `official_reference`: fonte oficial HI Tecnologia registrada. Serve para
  rastrear a origem documental, mas nao valida bancada.
- `field_observed`: item observado no projeto ou historico de bancada.
- `verified_in_bench`: validacao real documentada. So deve ser usado com
  resultado registrado.
- `pending_manual_validation`: validacao manual ainda obrigatoria.

RION 5 e NEON 5 agora possuem referencias oficiais registradas, mas continuam
pendentes porque CPU, modulos confirmados, mapas aplicaveis, comunicacao real
e testes em bancada ainda precisam ser documentados.

## NEON-1S + DIO605

Preparacao inicial:

1. Selecionar `NEON_LEGACY`.
2. Selecionar `NEON-1S`.
3. Selecionar `DIO605`.
4. Comecar por `COMMUNICATION_DIAGNOSTIC`.
5. Ajustar porta, baud rate e slave ID no painel de conexao normal.
6. Copiar o relatorio/checklist e conferir os avisos de COM antes de conectar.
7. Manter `DIO605` e o conjunto como `pending_manual_validation` ate confirmar
   mapa, ligacao e retorno em bancada.

Nao usar validacao anterior do HIO115 como prova para DIO605.

## RION-502 + HIO115

Preparacao inicial:

1. Selecionar `RION_LEGACY`.
2. Selecionar `RION-502`.
3. Selecionar `HIO115`.
4. Comecar por `COMMUNICATION_DIAGNOSTIC`.
5. Usar `REMOTE_IO_RS485` somente depois de confirmar topologia, endereco e
   ausencia de outro mestre na rede.
6. Copiar o relatorio/checklist e anexar ao registro do teste quando
   aplicavel.
7. Executar I/O apenas quando o programa carregado e o mapa usado pelo app
   estiverem confirmados para o conjunto.

`HIO115` aparece com historico de bancada no fluxo atual, mas isso nao valida
automaticamente `RION-502 + HIO115`.

## NEON 5 e RION 5 futuros

`NEON_5_CONTROLLER` e `RION_5_CONTROLLER` sao preparacao futura. Antes de uso:

1. Revisar as referencias oficiais ja registradas.
2. Confirmar CPU, slots e modulos em bancada.
3. Confirmar comunicacao disponivel.
4. Ajustar ou criar perfis no catalogo.
5. Validar `COMMUNICATION_DIAGNOSTIC` em bancada.
6. Criar perfis de I/O somente depois de confirmar mapa e bancada.

Referencias documentais adicionadas:

- RION 5: pagina oficial, manual RION-5 e navegacao oficial de hardware.
- NEON 5: navegacao oficial de hardware/base.
- HIO115, HIO130, HIO140, HIO165 e DIO605: PDFs oficiais de especificacao.

## Como adicionar modelo

1. Editar `models` no catalogo.
2. Usar `id` unico.
3. Informar `family` existente.
4. Informar `controllerCpu` somente quando observado ou confirmado.
5. Referenciar apenas modulos e perfis existentes.
6. Manter `validationStatus` como `pending_manual_validation`.
7. Rodar o validador e o build.

## Como adicionar modulo

1. Editar `ioModules` no catalogo.
2. Usar `id` unico.
3. Informar `family` existente.
4. Preencher `supportedFamilies` somente com aplicabilidade conhecida.
5. Referenciar `testProfiles` apenas quando o procedimento for adequado.
6. Nao inventar pinagem, quantidade de pontos, mapa ou especificacao eletrica.
7. Rodar o validador e o build.

## Validacao em bancada

Para validar:

1. Identificar modelo, CPU, modulo, slot e perfil de comunicacao.
2. Garantir que o programa HIstudio correto esteja carregado.
3. Garantir que HIstudio, XCTU e Testador nao usem a mesma COM.
4. Confirmar energia, cabeamento, porta, baud rate e slave ID.
5. Executar primeiro `COMMUNICATION_DIAGNOSTIC`.
6. Executar I/O somente com mapa e bancada confirmados.
7. Registrar resultado antes de mudar status para `verified_in_bench`.

## Radio e XCTU

Comunicacao normal do CLP pelo Testador nao e configuracao interna de radio.

Quando houver radio transparente, a configuracao profunda deve ser feita fora
do app, via XCTU. Nao usar XCTU e Testador ao mesmo tempo se ambos disputarem a
mesma porta COM.
