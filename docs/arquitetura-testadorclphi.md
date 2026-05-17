# Arquitetura - TestadorCLPHI.App

## Objetivo

Manter o app organizado, evitando God Form, retrabalho e mistura de responsabilidades.

## Regra principal

Cada camada deve ter uma responsabilidade clara.

| Parte | Responsabilidade |
|---|---|
| Program.cs | Iniciar o app, configurar WinForms e montar dependências principais |
| MainForm.cs | Coordenar eventos da tela e atualizar a interface |
| Services | Executar regras técnicas, comunicação, comandos, diagnóstico e detecção |
| Controls | Encapsular partes visuais reutilizáveis |
| Models/Maps | Guardar dados, estados, configurações e endereços de memória |

## Program.cs

Pode fazer:

- iniciar `ApplicationConfiguration`;
- criar dependências principais;
- abrir `MainForm`;
- configurar tratamento global de erro no futuro.

Não deve fazer:

- lógica de Modbus;
- leitura/escrita de registradores;
- regra de detecção automática;
- lógica de botão;
- lógica visual detalhada;
- diagnóstico técnico do CLP.

## MainForm.cs

Pode fazer:

- receber eventos de clique;
- ler campos da tela;
- chamar services;
- atualizar labels, botões e painéis;
- exibir mensagens ao usuário;
- coordenar fluxo visual da interface.

Não deve fazer:

- implementar protocolo Modbus;
- concentrar regra de diagnóstico;
- concentrar regra de detecção automática;
- conter lógica pesada de tema;
- acessar detalhes internos de controles extraídos;
- crescer como God Form.

## Services

Devem conter regras técnicas específicas.

Exemplos:

- `ModbusRtuPlcCommunicationService`: comunicação Modbus RTU;
- `PlcRegisterCommandService`: leitura/escrita de registradores;
- `PlcAutoDetectionService`: detecção de porta, baud rate e slave ID;
- `AppThemeService`: aplicação de tema;
- `PlcConnectionSettingsUiService`: leitura e validação de configuração da UI.

## Controls

Devem encapsular partes visuais com estado próprio.

Exemplo:

- `ConnectionStatePanelControl`: painel de estado da conexão, botões de conectar, desconectar e ler MW70.

O `MainForm` deve conversar com o control por métodos/eventos públicos, sem acessar campos internos.

## Models, Maps e estados

Devem conter dados simples e estáveis.

Exemplos:

- `Hio115MemoryMap`: endereços de memória;
- `PlcConnectionSettings`: configuração de conexão;
- `PlcConnectionState`: estado da conexão;
- `PlcAutoDetectionResult`: resultado da detecção.

## Checklist antes de nova alteração

Antes de alterar o código, responder:

1. Esta mudança é refatoração, correção ou funcionalidade nova?
2. A alteração pertence ao `MainForm`, a um service, a um control ou a um model/map?
3. Estou misturando responsabilidades?
4. Estou mexendo em Modbus sem necessidade?
5. Estou mexendo em layout sem necessidade?
6. O commit terá uma única intenção?
7. O build será validado antes do commit?
8. O teste manual mínimo está definido?

## Critério para extrair código do MainForm

Extrair do `MainForm` quando:

- o método tiver regra técnica;
- o trecho for visual reutilizável;
- houver acesso direto demais a controles internos;
- o arquivo continuar crescendo;
- a alteração dificultar teste manual;
- a responsabilidade puder ser nomeada claramente em uma classe/service/control.

## Meta atual

Reduzir `MainForm.cs` gradualmente:

| Marco | Meta |
|---|---|
| Atual | 705 linhas |
| Próximo alvo | Menos de 600 linhas |
| Alvo seguinte | Menos de 500 linhas |

A redução só é válida se não quebrar:

- comunicação Modbus RTU;
- tema claro/escuro;
- painel de estado;
- leitura de `%MW70`;
- comandos básicos do CLP.
