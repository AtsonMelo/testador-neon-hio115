# Motor genérico de simulação

## Objetivo

`SimulationEngine` executa perfis declarativos pequenos. Ele não conhece CLP,
porta serial, Modbus ou controles WinForms. Pivô e Poço usam o mesmo motor.

## Modelo

- `SimulationProfile`: identidade e estado inicial;
- `SimulationVisualizationDefinition`: tipo visual, quantidade de torres e
  papéis de sinais usados pela projeção;
- `SimulationSignalDefinition`: entrada digital, analógica ou saída virtual;
- `SimulationIoBinding`: vínculo declarativo entre sinal, direção, tipo, canal,
  registro, alias e nível de evidência;
- `SimulationRule`: condições, prioridade, estado, alarme e bloqueio de saídas;
- `SimulationScenario`: conjunto de valores reproduzível;
- `SimulationExclusiveOutputGroup`: intertravamento lógico;
- `SimulationSnapshot`: estado imutável para UI e integração;
- `SimulationProcessState`: projeção independente de WinForms e transporte;
- `PivotProcessState`: posição, sentido, bomba, água e estado das torres;
- `SimulationCounters` e `SimulationLogEntry`: auditoria simulada.

Os perfis ficam em `Data/Simulation/Profiles` e usam JSON schema version `1`.
Campos desconhecidos são rejeitados pelo desserializador. IDs, faixas,
referências e cenários duplicados ou inválidos falham fechados.

O schema continua na versão `1`: `visualization`, `group` e `ioBindings` são
extensões aditivas. Perfis antigos ainda podem ser lidos pelo motor; uma sessão
integrada ao fake HIO115 exige bindings válidos e falha fechada quando eles não
existem.

## Separação de responsabilidades

```text
SimulationProfile + SimulationEngine
                |
          SimulationSnapshot
                |
   SimulationProcessStateProjector
                |
       PivotProcessStateProjector
                |
       renderer leve / demais UIs
```

O motor não referencia WinForms. O `PivotProcessControl` não referencia Modbus,
RTU ou o fake. O adapter de I/O não conhece desenho ou controles visuais.

## Execução

Cada alteração valida tipo e faixa, atualiza o valor, avalia regras por
prioridade e recalcula alarmes e bloqueio de saídas. Não existe timer, polling
ou processo de background no motor.

O Pivô 2.0 também não usa timer. O renderer é invalidado apenas quando sua
assinatura visual muda. Alterações de métricas que não participam do desenho
atualizam somente os labels correspondentes.

## Limites

[Certo] Valores, unidades e thresholds dos perfis são parâmetros de simulação.
Eles não são especificação definitiva de uma máquina real.

[Certo] A integração HIO115 transfere analógicos como raw simulado. Nenhuma
escala física de 4-20 mA foi inferida.

[Bloqueado] Persistência de perfis editados, versionamento de cenários pelo
usuário e editor visual permanecem para milestones futuras.

## Validação

O validador `--validate-layout-3-simulation-engine` cobre schema, faixas,
cenários, prioridades, alarmes, intertravamentos, reset, posição `0..100`,
quantidade/estado das torres e independência de transporte físico.

O validador `--validate-industrial-io-mapping` cobre os bindings HIO115 dos
perfis distribuídos. Os dois validadores executam somente em memória.
