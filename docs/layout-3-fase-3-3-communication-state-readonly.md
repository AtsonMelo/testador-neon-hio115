# Layout 3 - Fase 3.3 - Estado de comunicacao read-only

## Objetivo

Adicionar ao host Layout 3 um estado estruturado e uma leitura visual da
comunicacao local. A informacao e calculada somente com o catalogo ja carregado
em memoria: nao cria canal, nao tenta conectar e nao executa comando fisico.

## Flag de abertura

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --layout-3-host-readonly
```

A flag e o titulo da janela permanecem inalterados. `MainForm.cs` e
`Program.cs` nao foram modificados nesta fase.

## Estado estruturado

`Layout3CommunicationState` concentra:

- estado geral (`Nao conectada`, `Local read-only`, `Simulada` ou `Bloqueada`);
- origem (`Catalogo local`, `Host local` ou `Sem hardware`);
- protocolo informado pelo perfil selecionado;
- mensagem operacional curta;
- data e hora local da ultima atualizacao;
- tentativas reais, invariavelmente `0`;
- comandos fisicos executados, invariavelmente `0`.

O estado inicial e recalculado a partir de `Layout3ProfileSelection`:

- com familia selecionada, o estado e `Local read-only`;
- sem familia/hardware, o estado e `Nao conectada`;
- quando existe perfil de comunicacao, a origem e `Catalogo local` e o painel
  mostra o nome e o protocolo como informacao de catalogo;
- sem perfil de comunicacao, a origem e `Host local` e o protocolo fica
  indisponivel;
- uma intencao registrada no botao local de auditoria e negada pela guarda e
  apenas muda a representacao para `Bloqueada`.

O valor `Simulada` faz parte do dominio para evolucao local controlada, mas esta
fase nao ativa simulador nem cria transporte.

## Atualizacao pela selecao de perfil

Trocas de familia, modelo, modulo ou teste publicam a nova selecao local para o
host. O estado de comunicacao e entao reconstruido e o painel passa a mostrar,
por exemplo, dados de Modbus/TCP, RS-485 ou radio existentes no catalogo. Esses
dados sao rotulados como `PROTOCOLO DO PERFIL (CATALOGO)` e acompanhados por
`SEM CONEXAO ABERTA`.

Cada recalculo acrescenta ao log local:

- estado e origem;
- perfil/protocolo de catalogo;
- confirmacao de que nenhuma conexao fisica foi criada;
- tentativas reais em `0`;
- comandos fisicos em `0`.

## Painel visual

O antigo bloco `HOST / ESTADO LOCAL` foi especializado como
`HOST / COMUNICACAO`. O painel ocupa a mesma coluna esquerda e preserva a
composicao de tres colunas do host:

- estado e origem em campos curtos;
- protocolo em campo com quebra de linha para larguras compactas;
- mensagem e timestamp local;
- faixa unica de diagnostico com os dois contadores zerados;
- botao local de auditoria e aviso read-only.

As cores usam exclusivamente `Layout3ThemePalette`, cobrindo os temas Escuro,
Claro e Automatico. O conteudo mantem largura minima controlada e rolagem do
viewport quando a janela fica abaixo da area segura.

## Seguranca

- nenhuma porta serial e aberta;
- nenhum cliente ou socket de rede e criado;
- nenhuma biblioteca Modbus e chamada;
- nenhum servico PLC e instanciado;
- nenhum mapa ou registrador e lido ou escrito;
- nenhuma tentativa de conexao real ocorre;
- comandos fisicos executados permanecem em `0`.

As classes novas dependem apenas de estado de UI, selecao de perfil e controles
WinForms. Nao existe referencia a executores ou servicos de hardware.

## Validacao

- build do projeto;
- `git diff --check`;
- abertura manual com `--layout-3-host-readonly`;
- troca de perfil e verificacao do log/painel;
- temas Escuro, Claro e Automatico;
- janela normal, maximizada e acoplada a esquerda;
- QA Branch com `-RunGuiSmoke` e os quatro smokes esperados.
