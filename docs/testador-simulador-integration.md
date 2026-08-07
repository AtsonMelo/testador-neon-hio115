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

## Entradas

Alterações no Simulador são sincronizadas para DI e AI do fake. O Testador lê os
mesmos registros por FC03 e apresenta valores digitais e raw analógicos.

## Saídas

O Testador chama o serviço fechado com `Layout3OutputChannel`. O fake aceita
somente DO00..DO03, emite o evento tipado e o adapter atualiza a saída virtual
correspondente. O ciclo momentâneo desliga o canal ao terminar.

Se o processo simulado estiver em emergência ou falha bloqueante, o engine
rejeita o efeito do comando. A operação continua registrada como tentativa
simulada; nenhuma saída física existe.

## UI

O comando `--industrial-platform` abre uma tela inicial com:

- `TESTADOR`: configuração RTU editável, descoberta, identificação, entradas,
  saídas simuladas, diagnóstico e log;
- `SIMULADOR`: seleção Pivô/Poço, cenários, entradas, analógicas, saídas e
  alarmes.

Os controles são carregados sob demanda. `Atualizar portas` fica desabilitado e
o status mostra `IN_MEMORY_RTU`, `COM FISICA: OFF` e contadores físicos zero.

## Validação

`--validate-layout-3-test-simulator-integration` prova Pivô, Poço, discovery,
identificação, DI, AI, DO, emergência, mapeamento fail-closed e independência
serial.

`--validate-layout-3-industrial-platform-ui` constrói e descarta a UI sem message
loop, valida lazy loading, os dois perfis, identificação, saída momentânea,
ausência de tipos serial/TCP e contadores físicos zero.
