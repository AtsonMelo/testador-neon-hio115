# Teste supervisionado de saídas

## Allow-list fechada

| Canal | Referência | Acesso planejado |
|---|---:|---|
| DO00 | 31128 | R/W supervisionado |
| DO01 | 31129 | R/W supervisionado |
| DO02 | 31130 | R/W supervisionado |
| DO03 | 31131 | R/W supervisionado |

`Layout3OutputChannel` é o único contrato aceito pelo serviço de aplicação. A
UI não fornece endereço bruto. O mapeamento em referência Modbus permanece
encapsulado.

## Bloqueios permanentes do escopo atual

- `31137`, `31140`, `31143`: reservados R/W;
- `31144`: frequência PWM;
- `31145`: duty cycle PWM;
- qualquer outra referência arbitrária;
- duas saídas simultâneas por padrão.

## Gate simulado

O fake permite um ciclo somente quando:

- equipamento simulado identificado;
- assinatura válida;
- F21 aceitável;
- operador habilita explicitamente o modo na UI;
- `SimulationOnly=true`;
- `PhysicalGateAuthorized=false`.

Uma saída é ligada, mantida por duração limitada e desligada no final. Em
cancelamento ou falha, o serviço tenta o caminho de desligamento e retorna
`AttentionRequired` quando não pode confirmá-lo.

## Gate físico futuro

[Bloqueado] Escrita física não foi autorizada e não existe transporte serial na
nova plataforma. Um teste real exigirá, no mínimo:

1. identidade e assinatura confirmadas;
2. F21 sem falha crítica;
3. checklist e evidências de bancada aprovados;
4. modo físico de escrita habilitado explicitamente;
5. gate específico autorizado por Atson;
6. uma saída por comando;
7. duração máxima e cancelamento;
8. confirmação de desligamento e plano de atenção.

Perda de comunicação nunca deve ser interpretada como desligamento físico.

## Auditoria

Cada operação registra timestamp, modo, perfil, transporte, endereço, operação,
alias, resultado, duração, erro e marcador `SIMULATED`. Contadores físicos e
simulados são separados.
