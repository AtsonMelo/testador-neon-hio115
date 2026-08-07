# Arquitetura planejada do transporte read-only - Layout 3

## Estado e limite

`STATUS: DESENHO OFFLINE - TRANSPORTE NAO IMPLEMENTADO`

Este documento define contratos e gates para uma futura milestone. A Fase 3.9
nao contem porta serial, cliente TCP, socket, biblioteca Modbus, acesso a COM8,
acesso a IP, fake server, descoberta executavel ou funcao de escrita.

O transporte somente podera ser implementado em branch separada depois do
Gate D. Mesmo nessa branch, feature e comunicacao real permanecerao `OFF` por
padrao e nenhum teste fisico sera executado sem o gate final.

## Fronteiras planejadas

1. **Configuracao**: perfis RTU/TCP editaveis, politica de enderecos e gates.
2. **Validacao pura**: rejeita campos ausentes, conflito e endereco reservado.
3. **Orquestracao read-only**: futura operacao single-shot e cancelavel.
4. **Adapter RTU ou TCP**: futuro componente interno sem API publica de escrita.
5. **Evidencia**: logs e contadores de tentativas, conexoes e leituras.

As camadas 3 e 4 nao existem nesta fase. A configuracao e o preflight nao
instanciam nem simulam transporte.

## Selecao explicita

A interface futura deve ter um seletor com somente:

- `RTU`;
- `TCP`.

Trocar o seletor apenas muda o perfil validado. Nao abre conexao, nao acessa
porta/IP e nao altera a interface fisica do controlador.

### Perfil RTU

Campos: porta COM, camada fisica `RS232|RS485`, interface do controlador, baud,
data bits, paridade, stop bits, timeout, endereco, maximo de tentativas e
intervalo entre tentativas.

Perfil inicial registrado: COM8, RS232, `38400 / 8-N-1`, endereco 10. A
interface e `ITF-A1_OR_ITF-A2` com status `NÃO IDENTIFICADA`; isso nao bloqueia
o perfil de software porque ambas sao RS232-C, mas permanece pendencia fisica
documental. O conector atual e DB9 identificado como `Serial`. Os bornes RS-485
`D+ / D-` e a chave de terminacao existem separadamente e nao indicam uso atual
de RS-485. Selecionar RS485 nao muda o hardware automaticamente: exige cabo,
conversor e interface fisica confirmados separadamente.

### Perfil TCP

Campos: IP do controlador, porta TCP, Unit ID/endereco, topologia isolada,
timeout e maximo de tentativas. IP, porta e topologia permanecem vazios. O
endereco inicial 10 e editavel. O perfil TCP so e exigido quando selecionado.

O preflight valida exclusivamente o perfil selecionado. RTU nao exige dados
TCP; TCP nao exige dados RTU. Trocar o seletor nao abre nem reconfigura qualquer
interface.

## Identidade e evidencia fisica

O HIstudio apresenta `NEON5-1S / CPU450 / HIO115`, enquanto o frontal apresenta
`PIVODRIP / OMNICONTROL / OMNI-PLC2`. A relacao e `NÃO CONFIRMADA`; o Gate C
permanece bloqueado. `Slot 1115` e apenas compativel com HIO115, sem provar a
equivalencia.

O texto nominal `1030 VDC` e preservado literalmente, sem conversao para faixa
e sem equivaler a tensao medida. Declaracoes do responsavel sobre aterramento,
isolamento, estado seguro, emergencia, desconexao e backup nao satisfazem os
gates enquanto faltarem evidencias verificaveis.

## Enderecamento

| Regra | Valor |
|---|---:|
| Faixa representavel na interface | 1..255 |
| Operacao/descoberta padrao | 1..247 |
| Reservado/vendor-specific | 248..255 |
| Broadcast proibido | 0 |
| Nunca sondar automaticamente | 255 |
| Endereco atual conhecido | 10 |

Um endereco 248..255 so pode ser considerado futuramente com modo avancado,
selecao manual unica, aviso reconhecido, aprovacao explicita e Gate D. Ele
nunca entra em faixa automatica. O endereco 255 nunca e sondado
automaticamente, mesmo no modo avancado.

## Descoberta futura

- desligada por padrao;
- iniciada somente por comando explicito;
- faixa escolhida pelo usuario e materializada em allow-list;
- default recomendado: somente endereco 10;
- uma tentativa por endereco;
- intervalo configurado entre tentativas;
- sem repeticao continua ou reconexao automatica;
- cancelamento imediato;
- somente FC03;
- nenhuma escrita ou coil.

Os unicos candidatos de identificacao sao:

| Referencia exibida | Finalidade | Acesso | Valor esperado |
|---|---|---|---:|
| F12 / 30012 | ID do programa | R | 31134 |
| F13 / 30013 | CRC do programa | R | 23248 |

Um endereco so pode ser identificado se os dois valores coincidirem. O campo
`protocolDataAddress` permanece vazio ate o mapa definir e revisar a traducao
para endereco de dados do protocolo. Nenhum offset sera deduzido do numero de
exibicao. Entradas, saidas, analogicos, contadores e PWM do HIO115 nao podem ser
usados para descoberta.

## Controles da interface futura

- seletor RTU/TCP;
- painel do perfil selecionado;
- endereco manual;
- faixa/allow-list de descoberta;
- comando `Validar configuracao`;
- comando `Descobrir endereco`, inicialmente desabilitado;
- indicador `READY / NOT READY`;
- comando `Cancelar`;
- contadores de tentativas, conexoes e leituras;
- escritas e comandos fisicos fixados em zero.

Validar configuracao e uma operacao local. O comando de descoberta so podera
ser habilitado depois de transporte implementado, testes com fake, parametros
aprovados, Gate D e autorizacao especifica de bancada.

## Invariantes

- `featureEnabled = false`;
- `realCommunicationEnabled = false`;
- `writesEnabled = false`;
- `pollingEnabled = false`;
- `automaticReconnectEnabled = false`;
- `singleShotOnly = true`;
- API publica de escrita inexistente;
- coils inexistentes;
- conexoes, leituras, escritas e comandos fisicos reais iguais a zero nesta
  fase.

## Criterios para a futura milestone

Antes de escrever transporte: Gate D explicito, biblioteca/versao/licenca
aprovadas, interface sem escrita revisada e plano de fake definido. Antes de
qualquer bancada: protocolo ativo, perfil selecionado, mapa, allow-lists,
timeout, intervalos, seguranca e responsavel devem estar confirmados, e o
preflight deve retornar zero.
