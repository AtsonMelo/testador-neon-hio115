# Arquitetura Modbus RTU offline

Data do checkpoint: 2026-08-06.

## Estado

- [Certo] O escopo ativo é exclusivamente Modbus RTU.
- [Certo] Modbus TCP permanece somente no backlog.
- [Certo] A implementação nova não possui `SerialPort`, `System.IO.Ports`, `TcpClient` ou `Socket`.
- [Certo] `RealCommunicationEnabled` continua falso na configuração de bancada.
- [Bloqueado] Nenhuma porta COM pode ser aberta antes de um gate físico posterior.

## Camadas

```text
UI Testador / UI Simulador
        |
IndustrialPlatformSession
        |
+-------+------------------+
|                          |
RtuEquipment...            SimulationEngine
RtuInputTestService               |
RtuSupervisedOutputService        |
        |                    SimulationHio115Adapter
        +------------+-------------+
                     |
                RtuClient
                     |
              IRtuTransport
                     |
           InMemoryRtuTransport
                     |
            NeonHio115FakeDevice
```

`IRtuTransport` expõe somente troca de frames e informa se a execução é
simulada. Não existe implementação serial na nova plataforma. O transporte real
foi deliberadamente adiado.

## Protocolo implementado

Foi escolhido um subconjunto interno pequeno e auditável, sem nova dependência
NuGet:

- CRC16 Modbus;
- criação e validação de frame RTU;
- FC03 para leitura de holding registers;
- FC06 mínimo, interno, usado apenas pelo serviço fechado de saídas;
- exception responses;
- validação de endereço, tamanho, função, CRC e quantidade;
- timeout e cancelamento na abstração.

Não existe API pública `WriteRegister(address, value)` na UI ou nos serviços de
aplicação. A conversão de `DO00..DO03` em referências ocorre internamente.

## Gates

Os gates de saída possuem semânticas separadas:

- execução simulada: `SimulationOnly=true` e `PhysicalGateAuthorized=false`;
- execução física futura: `SimulationOnly=false` e gate físico explicitamente
  autorizado.

O serviço seleciona a política a partir do tipo do transporte. Um gate simulado
não autoriza transporte físico.

## Dependência legada

[Certo] O projeto já possuía `System.IO.Ports` e um serviço serial legado antes
desta evolução. Esse serviço permanece no repositório por rastreabilidade, mas o
startup padrão passou a instanciar `DisabledPlcCommunicationService`.

[Bloqueado] A remoção ou substituição definitiva do serviço legado exige uma
milestone própria. Enquanto isso, a nova plataforma não o referencia.

## Inicialização

O host novo é iniciado somente por comando explícito:

```powershell
dotnet run --project app/TestadorCLPHI.App/TestadorCLPHI.App.csproj -c Release -- --industrial-platform
```

O comando não conecta automaticamente. O campo `COM8` é somente dado editável
do perfil de laboratório. O botão de atualização de portas permanece
desabilitado no host offline.

## Status possíveis

- `DEVELOPMENT`: configuração local inválida ou falha de software;
- `OFFLINE_READY`: RTU em memória validado;
- `SIMULATION_READY`: fake identificado e simulador operacional;
- `BENCH_PREP_REQUIRED`: faltam gates ou evidências físicas;
- `READY_FOR_BENCH`: reservado ao futuro gate físico;
- `CONNECTED` e `TESTING`: impossíveis nesta etapa.
