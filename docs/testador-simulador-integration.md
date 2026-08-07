# Integração Testador e Simulador

## Fluxo em memória

```text
SimulationEngine
      |
SimulationHio115Adapter
      |
NeonHio115FakeDevice (endereço 1)
      |
InMemoryRtuTransport
      |
Serviços do Testador
```

Uma única `IndustrialPlatformSession` é compartilhada pelos controles Testador
e Simulador. A troca de perfil recria a sessão e seus componentes de forma
determinística; não há polling ou thread de background.

O `SimulationProcessState` é uma projeção paralela usada somente pela UI e não
fica no caminho de transporte. O fluxo executável de I/O é:

```text
Process signal -> SimulationIoBinding -> SimulationHio115Adapter
               -> NeonHio115FakeDevice -> InMemoryRtuTransport -> Testador
```

## Entradas

Alterações no Simulador são sincronizadas para DI e AI do fake. O Testador lê os
mesmos registros por FC03 e apresenta valores digitais e raw analógicos.
Cada linha mantém o alias técnico (`DI00`, `AI00` etc.) e exibe ao lado o nome
do processo. O usuário não precisa memorizar o canal.

## Saídas

O Testador chama o serviço fechado com `Layout3OutputChannel`. O fake aceita
somente DO00..DO03, emite o evento tipado e o adapter atualiza a saída virtual
correspondente. O ciclo momentâneo desliga o canal ao terminar.

Se o processo simulado estiver em emergência ou falha bloqueante, o engine
rejeita o efeito do comando. A operação continua registrada como tentativa
simulada; nenhuma saída física existe.

## Fonte do mapeamento

O adapter não contém tabelas especiais para Pivô ou Poço. Ele constrói os mapas
DI, AI e DO a partir de `profile.ioBindings`. A troca de perfil descarta a
sessão anterior e carrega os bindings do novo JSON; aliases não são
reaproveitados entre perfis.

A aba `Mapa de I/O` mostra Processo, Canal CLP, Registro, Tipo, Evidência e
Descrição. Todas as linhas distribuídas nesta fase exibem
`PERFIL DE SIMULAÇÃO` e nunca `PhysicalConfirmed`.

## UI

O comando `--industrial-platform` abre uma tela inicial com:

- `TESTADOR`: configuração RTU editável, descoberta, identificação, entradas,
  saídas simuladas, aliases de processo, Mapa de I/O, diagnóstico e log;
- `SIMULADOR`: seleção Pivô/Poço, cenários, entradas, analógicas, saídas e
  alarmes; no Pivô, renderer leve com torres configuráveis.

Os controles são carregados sob demanda. `Atualizar portas` fica desabilitado e
o status mostra `IN_MEMORY_RTU`, `COM FISICA: OFF` e contadores físicos zero.

## Validação

`--validate-layout-3-test-simulator-integration` prova Pivô, Poço, discovery,
identificação, DI, AI, DO, desligamento, emergência, troca de perfil, mapeamento
fail-closed e independência serial.

`--validate-layout-3-industrial-platform-ui` constrói e descarta a UI sem message
loop, valida lazy loading, os dois perfis, identificação, saída momentânea,
ausência de tipos serial/TCP e contadores físicos zero.

`--validate-industrial-io-mapping` valida SignalIds, unicidade de canais,
compatibilidade DI/AI/DO, endereços conhecidos, allow-list, bloqueio de
reservados/PWM e evidência de simulação.
