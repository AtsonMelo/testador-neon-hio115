# Layout 3 - Fase 3.2 - Selecao de perfil no host read-only

## Objetivo

Permitir que o host Layout 3 read-only (Fase 3.1) selecione familia, modelo,
modulo de I/O e perfil de teste a partir do catalogo local ja carregado pela
aplicacao, refletindo a escolha no estado visual da tela. A evolucao e pequena
e segura: nao abre comunicacao, nao envia comando e nao escreve registrador.

## Flag de abertura

```powershell
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --layout-3-host-readonly
```

A flag e o titulo da janela (`Layout 3 - Host read-only | Testador CLP HI`)
permanecem inalterados. Sem a flag, o fluxo padrao continua abrindo o
`MainForm`, que nao foi tocado nesta fase.

## O que muda

O painel de perfil do host deixa de exibir apenas uma referencia fixa e passa a
oferecer selecao local:

- seletor de **familia**;
- seletor de **modelo**;
- seletor de **modulo de I/O**;
- seletor de **perfil de teste**;
- **perfil de comunicacao** derivado da familia (somente leitura);
- **status de validacao**, com sinalizacao clara de pendencia quando aplicavel,
  sem bloquear a interface;
- resumo textual atualizado a cada selecao.

Cada confirmacao de selecao registra uma linha no log local do host, deixando
explicito que a operacao e read-only e que os comandos fisicos executados
permanecem em zero.

## Arquivos criados e alterados

Arquivos criados em `Ui/Industrial/Layout3/`:

- `Layout3ProfileSelection.cs`: selecao imutavel e resolver central que le o
  catalogo local (familia, modelo, modulo, comunicacao, teste) e deriva o status
  de validacao/pendencia;
- `Layout3HostProfileSelectionControl.cs`: controle visual de selecao do host,
  com combos read-only e atualizacao do resumo e do status.

Arquivos alterados em `Ui/Industrial/Layout3/`:

- `Layout3HostControl.cs`: usa o novo controle de selecao no lugar do painel
  estatico e encaminha cada selecao para o log local;
- `Layout3HostState.cs`: agrega a selecao de perfil inicial e registra o perfil
  local no log de eventos.

O `Layout3ProfilePanelControl.cs` (compartilhado com o preview) nao foi alterado
e continua atendendo o preview isolado.

## Estado e seguranca

- comunicacao permanece `Nao conectada` (read-only local);
- entradas continuam sem leitura fisica e saidas continuam bloqueadas;
- comandos fisicos executados permanecem em `0`;
- `Layout3ReadOnlyCommandGuard` continua negando qualquer intencao;
- nenhum servico PLC, porta serial, Modbus, mapa ou registrador e criado ou
  acessado;
- a selecao apenas le estruturas ja existentes do catalogo em memoria.

## Validacoes

- build do projeto;
- `git diff --check`;
- QA local da branch com `-RunGuiSmoke`;
- smoke da flag `--layout-3-host-readonly`;
- verificacao visual da selecao de perfil e das sinalizacoes read-only.

## Limitacoes

- a selecao nao observa hardware real e nao dispara nenhuma comunicacao;
- a comunicacao e exibida de forma derivada, sem escolha independente;
- nao ha polling, leitura de entradas ou escrita de saidas;
- qualquer execucao de teste real permanece fora desta fase e exige
  planejamento, revisao de seguranca e autorizacao de bancada proprios.
