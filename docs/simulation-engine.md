# Motor genérico de simulação

## Objetivo

`SimulationEngine` executa perfis declarativos pequenos. Ele não conhece CLP,
porta serial, Modbus ou controles WinForms. Pivô e Poço usam o mesmo motor.

## Modelo

- `SimulationProfile`: identidade e estado inicial;
- `SimulationSignalDefinition`: entrada digital, analógica ou saída virtual;
- `SimulationRule`: condições, prioridade, estado, alarme e bloqueio de saídas;
- `SimulationScenario`: conjunto de valores reproduzível;
- `SimulationExclusiveOutputGroup`: intertravamento lógico;
- `SimulationSnapshot`: estado imutável para UI e integração;
- `SimulationCounters` e `SimulationLogEntry`: auditoria simulada.

Os perfis ficam em `Data/Simulation/Profiles` e usam JSON schema version `1`.
Campos desconhecidos são rejeitados pelo desserializador. IDs, faixas,
referências e cenários duplicados ou inválidos falham fechados.

## Execução

Cada alteração valida tipo e faixa, atualiza o valor, avalia regras por
prioridade e recalcula alarmes e bloqueio de saídas. Não existe timer, polling
ou processo de background no motor.

## Limites

[Certo] Valores, unidades e thresholds dos perfis são parâmetros de simulação.
Eles não são especificação definitiva de uma máquina real.

[Certo] A integração HIO115 transfere analógicos como raw simulado. Nenhuma
escala física de 4-20 mA foi inferida.

[Bloqueado] Persistência de perfis editados, versionamento de cenários pelo
usuário e editor visual permanecem para milestones futuras.

## Validação

O validador `--validate-layout-3-simulation-engine` cobre schema, faixas,
cenários, prioridades, alarmes, intertravamentos, reset e independência de
transporte físico.
