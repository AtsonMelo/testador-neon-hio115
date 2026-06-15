# Plano de testes RION/NEON

## Premissas

- O modo normal existente do Testador deve continuar funcionando.
- Nenhum arquivo HIstudio `.dpk`, `.dmf`, `.prj`, `.hst` ou equivalente e
  alterado nesta milestone.
- Modulos sem mapa confirmado ficam pendentes.
- A selecao futura de modelo nao deve bloquear o fluxo atual do app.

## Fluxo comum

1. Confirmar que HIstudio, XCTU e outros softwares nao estao usando a mesma COM.
2. Selecionar ou registrar o perfil de comunicacao esperado.
3. Executar `COMMUNICATION_DIAGNOSTIC`.
4. So executar perfil de I/O quando o modulo e o mapa do programa carregado
   estiverem confirmados.
5. Registrar resultado por conjunto: modelo, CPU, modulo, slot, porta, baud
   rate, slave ID e observacoes.

## Uso da aba Perfil hardware

1. Abrir o Testador em modo normal.
2. Entrar na aba `Perfil hardware`.
3. Selecionar familia, modelo, modulo de I/O, comunicacao e perfil de teste.
4. Ler o resumo antes de conectar ao CLP.
5. Tratar `pending_manual_validation` como bloqueio de validacao operacional,
   nao como erro de cadastro.
6. Ajustar porta, baud rate e slave ID somente no painel de conexao normal.

A aba prepara a execucao e mostra o que se aplica ao conjunto selecionado. Ela
nao valida bancada sozinha, nao altera parametros reais e nao envia comandos
fisicos.

## NEON-1S + DIO605

Conjunto observado:

- Modelo: `NEON-1S`.
- CPU: `CPU401` no slot 0.
- Modulo: `DIO605` no slot 1.

Plano inicial:

1. Na aba `Perfil hardware`, selecionar `NEON_LEGACY`, `NEON-1S`, `DIO605`,
   um perfil serial compativel com a bancada e `COMMUNICATION_DIAGNOSTIC`.
2. Ajustar porta, baud rate e slave ID no painel de conexao normal.
3. Usar `COMMUNICATION_DIAGNOSTIC` para confirmar resposta Modbus.
4. Manter `DIO605` como `pending_manual_validation` ate confirmar mapa e
   ligacao de bancada.
5. Preparar `DIGITAL_IO_BASIC`, `DIGITAL_OUTPUT_MANUAL` e
   `DIGITAL_INPUT_READ`, mas nao tratar sucesso em HIO115 como prova para
   DIO605.
6. Registrar qualquer diferenca entre retorno esperado e retorno observado.

## RION-502 + HIO115

Conjunto observado:

- Modelo: `RION-502`.
- CPU: `CPU502` no slot 0.
- Modulo: `HIO115` no slot 1.

Plano inicial:

1. Na aba `Perfil hardware`, selecionar `RION_LEGACY`, `RION-502`, `HIO115`,
   um perfil RS485 compativel com a bancada e `COMMUNICATION_DIAGNOSTIC`.
2. Ajustar porta, baud rate e slave ID no painel de conexao normal.
3. Executar `COMMUNICATION_DIAGNOSTIC`.
4. Executar `REMOTE_IO_RS485` somente depois de confirmar topologia,
   endereco e ausencia de outro mestre na rede.
5. Executar `DIGITAL_IO_BASIC` apenas se o programa carregado e o mapa usado
   pelo app estiverem confirmados para o conjunto RION-502 + HIO115.
6. Nao assumir que validacao anterior do HIO115 em outro controlador valida o
   conjunto RION automaticamente.

## RION 5 e NEON 5

`RION_5_CONTROLLER` e `NEON_5_CONTROLLER` foram cadastrados para preparacao.

Antes de uso operacional:

1. Confirmar referencia oficial do modelo.
2. Confirmar CPU, slots, comunicacao e modulos aplicaveis.
3. Criar ou ajustar perfil de comunicacao.
4. Validar `COMMUNICATION_DIAGNOSTIC`.
5. So depois criar perfis de I/O especificos.

Na UI, `RION_5_CONTROLLER` e `NEON_5_CONTROLLER` devem continuar aparecendo
como preparacao futura enquanto faltarem referencia oficial, modulos,
comunicacao e validacao de bancada.

## Cuidados

- Nao acionar carga real sem revisao eletrica.
- Nao usar mapa de registradores de um modulo em outro modulo por inferencia.
- Nao deixar HIstudio e Testador usando a mesma COM.
- Nao usar o app para configurar radio; usar XCTU para radio e documentar como
  processo externo.
