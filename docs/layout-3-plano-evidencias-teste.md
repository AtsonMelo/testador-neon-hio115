# Plano de evidencias - teste read-only Layout 3

## Objetivo

Definir evidencias reproduziveis para preparacao local, futura implementacao de
transporte com fake e eventual teste supervisionado. Este documento nao
autoriza executar a ultima etapa.

## Identificador e diretorio

Formato sugerido do identificador:

```text
layout3-readonly-AAAA-MM-DD_HH-mm-ss
```

O diretorio deve ser aprovado antes do teste e ficar fora de caminhos que
possam ser limpos automaticamente. Registrar caminho absoluto, proprietario e
espaco disponivel.

## Evidencias da Fase 3.9 offline

1. `git status -sb` e SHA completo.
2. Tag/base de partida.
3. `git diff --name-only` e `git diff --check`.
4. Build Release completo.
5. Saida dos cinco validadores existentes.
6. Saida de `--validate-layout-3-bench-readiness-self-tests`.
7. Saida de `--validate-layout-3-bench-readiness`.
8. Varredura dos novos arquivos por APIs de rede/serial/protocolo proibidas.
9. Hash SHA-256 da configuracao e dos seis documentos.
10. Contadores zero no inicio e no fim.

## Comando de preflight

```powershell
dotnet run `
    --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj `
    -c Release `
    --no-build `
    -- --validate-layout-3-bench-readiness
```

Exit code diferente de zero bloqueia commit de aprovacao, PR Ready, transporte
e bancada. A lista de pendencias deve ser preservada integralmente.

## Evidencia da ausencia de rede

Revisar somente os novos arquivos do namespace `BenchReadiness`:

- nenhum namespace de rede ou porta serial;
- nenhum `TcpClient`, `Socket`, `SerialPort`, DNS, ping ou cliente HTTP;
- nenhuma biblioteca ou classe Modbus;
- nenhuma referencia a `IPlcCommunicationService`;
- nenhum timer, worker, polling ou reconexao;
- somente leitura de JSON/documentos locais e testes em memoria.

A busca textual e apoio de revisao, nao substitui a leitura do diff e do grafo
de chamadas.

## Evidencias futuras com fake

Somente apos o Gate D:

- nome, versao e licenca de eventual biblioteca;
- arquitetura sem API publica de escrita;
- fake server ligado exclusivamente a endereco reservado para testes locais;
- sucesso single-shot;
- timeout;
- cancelamento;
- desconexao;
- dado invalido;
- tentativa fora da allow-list;
- prova de que escrita/coil nao compila ou nao e acessivel;
- contadores de tentativas, conexoes e leituras;
- escritas e comandos fisicos fixados em zero.

Nenhum teste automatizado deve conter o IP real do CLP.

## Evidencias do futuro teste supervisionado

Antes:

- foto da etiqueta;
- ficha de parametros assinada;
- backup e hash do programa;
- captura da configuracao do PC;
- foto/registro do isolamento de saidas e estado da maquina;
- responsavel, emergencia e desconexao rapida;
- preflight com exit code 0;
- comando exato aprovado, ainda nao executado.

Durante:

- horario de cada tentativa;
- parametros efetivamente carregados;
- endereco de cada leitura e finalidade;
- duracao e resultado;
- contador acumulado de leituras;
- contadores de escrita e comandos fisicos em zero;
- qualquer timeout, cancelamento ou divergencia.

Depois:

- encerramento e retirada do cabo;
- comparacao do estado das saidas;
- restauracao da configuracao do PC;
- resumo de contadores;
- hashes dos logs;
- resultado assinado ou incidente.

## Criterios de aceitacao

- build e testes sem falha;
- todos os validadores com exit code 0;
- configuracao e documentos sem pendencias;
- somente registradores aprovados;
- limite de leituras respeitado;
- nenhum polling ou reconexao;
- zero escritas;
- zero comandos fisicos;
- branch limpa e PR revisavel;
- teste fisico ainda nao realizado durante a preparacao.

## Comando previsto para teste

`[BLOQUEADO]` O comando nao pode ser definido antes de existir transporte
read-only aprovado, parametros confirmados, allow-list validada e Gate final.
Quando existir, ele deve ser apresentado para aprovacao e nao executado
automaticamente.

## Retencao e incidente

- preservar arquivos originais;
- gerar SHA-256;
- anotar redacoes separadamente;
- registrar ausencia de evidencia como falha;
- vincular incidente aos logs, parametros, participantes e criterio de aborto;
- nova tentativa exige nova aprovacao.
