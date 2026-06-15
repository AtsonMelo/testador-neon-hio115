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

## NEON-1S + DIO605

Conjunto observado:

- Modelo: `NEON-1S`.
- CPU: `CPU401` no slot 0.
- Modulo: `DIO605` no slot 1.

Plano inicial:

1. Usar `COMMUNICATION_DIAGNOSTIC` para confirmar resposta Modbus.
2. Manter `DIO605` como `pending_manual_validation` ate confirmar mapa e
   ligacao de bancada.
3. Preparar `DIGITAL_IO_BASIC`, `DIGITAL_OUTPUT_MANUAL` e
   `DIGITAL_INPUT_READ`, mas nao tratar sucesso em HIO115 como prova para
   DIO605.
4. Registrar qualquer diferenca entre retorno esperado e retorno observado.

## RION-502 + HIO115

Conjunto observado:

- Modelo: `RION-502`.
- CPU: `CPU502` no slot 0.
- Modulo: `HIO115` no slot 1.

Plano inicial:

1. Usar perfil RS485 Modbus RTU compativel com a bancada.
2. Executar `COMMUNICATION_DIAGNOSTIC`.
3. Executar `REMOTE_IO_RS485` somente depois de confirmar topologia,
   endereco e ausencia de outro mestre na rede.
4. Executar `DIGITAL_IO_BASIC` apenas se o programa carregado e o mapa usado
   pelo app estiverem confirmados para o conjunto RION-502 + HIO115.
5. Nao assumir que validacao anterior do HIO115 em outro controlador valida o
   conjunto RION automaticamente.

## RION 5 e NEON 5

`RION_5_CONTROLLER` e `NEON_5_CONTROLLER` foram cadastrados para preparacao.

Antes de uso operacional:

1. Confirmar referencia oficial do modelo.
2. Confirmar CPU, slots, comunicacao e modulos aplicaveis.
3. Criar ou ajustar perfil de comunicacao.
4. Validar `COMMUNICATION_DIAGNOSTIC`.
5. So depois criar perfis de I/O especificos.

## Cuidados

- Nao acionar carga real sem revisao eletrica.
- Nao usar mapa de registradores de um modulo em outro modulo por inferencia.
- Nao deixar HIstudio e Testador usando a mesma COM.
- Nao usar o app para configurar radio; usar XCTU para radio e documentar como
  processo externo.
