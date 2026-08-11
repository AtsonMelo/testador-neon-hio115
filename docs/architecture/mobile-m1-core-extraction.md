# MOBILE-M1 — fronteira multiplataforma

## Objetivo

Esta extração prepara um segundo frontend Android sem transformar a UI WinForms em dependência compartilhada. O comportamento Desktop, o modo offline e todos os gates físicos permanecem inalterados.

## Antes e depois

Antes do MOBILE-M1, domínio industrial, simulação, RTU em memória, sessão e WinForms eram compilados no mesmo `net10.0-windows`. Agora a solução possui quatro projetos:

```text
TestadorIndustrial.Core (net10.0)
        ↑
TestadorIndustrial.Application (net10.0)
        ↑
TestadorCLPHI.App (net10.0-windows, WinForms)

TestadorIndustrial.Core.Validation (net10.0)
        └── referencia Core e Application
```

O Desktop também referencia Core diretamente porque suas views consomem modelos de simulação, catálogo e perfis. Não há ciclo: Core não referencia Application ou Desktop; Application referencia somente Core.

## Responsabilidades

### TestadorIndustrial.Core

- modelos e catálogo de hardware;
- perfis/capacidades de dispositivos;
- processo, sinais derivados e SafetyChain;
- modelos, regras e engine de simulação;
- política de mapeamento lógico de I/O;
- CRC e codec RTU puros;
- parsing determinístico dos catálogos e perfis JSON.

Core não depende de WinForms, `System.Drawing`, `System.IO.Ports`, Win32, APIs Android ou MAUI. Implementações continuam internas quando não formam contrato público; `InternalsVisibleTo` é uma ponte controlada para Desktop, Application e validação durante a extração incremental.

### TestadorIndustrial.Application

- sessão/orquestração industrial offline;
- casos de uso de identificação, discovery, leitura e saída supervisionada;
- abstração e transporte RTU em memória;
- equipamento fake e adapter da simulação;
- contadores e log operacional em memória;
- enums neutros de configuração serial.

Application não localiza arquivos e não conhece WinForms, desenho, portas seriais, Win32, Android ou MAUI.

### TestadorCLPHI.App

- frontend WinForms, tema, navegação e renderização GDI+;
- localização de `Data/` a partir de `AppContext.BaseDirectory`;
- carregamento de assets e arquivos empacotados;
- criação de `SerialPort`, enumeração COM e auto-detecção física;
- adapter entre enums seriais neutros e `System.IO.Ports`;
- startup e `MainForm` legado.

`MainForm` e o codec serial legado permanecem Desktop-only. O segundo codec não foi consolidado: embora trate Modbus RTU, sua equivalência completa com o codec industrial não foi provada sem risco ao Legacy.

### TestadorIndustrial.Core.Validation

Console `net10.0`, sem framework de teste externo, que prova Core/Application fora de um TFM Windows. Cobre catálogo, perfis, SafetyChain, torres, sinais derivados, simulação, mapping, CRC/codec, configuração serial neutra, sessão/fake in-memory, cancelamento e contadores físicos.

## Recursos e persistência

Parsing e localização são responsabilidades distintas:

- Core transforma texto JSON em modelos e falha fechado para conteúdo inválido;
- Desktop decide o caminho de `Data/Hardware` e `Data/Simulation/Profiles`;
- Application recebe um `SimulationProfile` pronto, sem conhecer filesystem;
- um frontend mobile poderá ler recurso empacotado e usar o mesmo parser.

Não foi criado um framework genérico de recursos nem uma interface sem segundo consumidor real. Quando o projeto Mobile existir, uma fonte mínima por frontend poderá ser adicionada se o ciclo de vida Android exigir streams assíncronos.

## Serial e segurança física

`IndustrialSerialParity` e `IndustrialSerialStopBits` são neutros. A conversão para `Parity`/`StopBits` ocorre somente em `ModbusRtuPlcCommunicationService`, imediatamente antes da criação da porta. Core e Application não contêm implementação física.

Contadores físicos continuam invariavelmente `0/0/0/0` em sessões in-memory. Um futuro Mobile offline deve preservar o mesmo gate e não referenciar a implementação Desktop.

## Regra para o futuro Mobile

```text
Core ← Application ← Mobile
Core ← Application ← Desktop
```

`TestadorIndustrial.Mobile` não poderá referenciar `TestadorCLPHI.App`. UI, lifecycle, navegação, armazenamento empacotado e renderer do Pivô serão próprios do frontend. O primeiro MVP deve ser offline/simulado e não incluir comunicação física.

## Validação e evolução segura

- manter a matriz Desktop existente sem reduzir contagens;
- executar `TestadorIndustrial.Core.Validation` em qualquer plataforma .NET suportada;
- preservar startup default/industrial/legacy e prioridade dos validators;
- exigir scans estáticos sem dependências Windows em Core/Application;
- mover tipos com commits pequenos, preferindo `git mv`;
- não criar projetos `Simulation`, `Infrastructure` ou `Mobile` até existir responsabilidade concreta que justifique a nova fronteira.

## Decisões adiadas

- API pública estável para um frontend Mobile: definir ao criar o primeiro consumidor;
- fonte Android de recursos empacotados;
- renderer mobile do Pivô;
- transporte Companion e qualquer comando remoto;
- consolidação do codec Modbus legado;
- comunicação física Android.
