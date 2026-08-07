# Layout 3 - Arquitetura RTU planejada e controles de I/O

> O nome historico do arquivo e mantido por rastreabilidade. A arquitetura
> continua read-only nos modos de identificacao e entradas, mas preve um servico
> separado para saidas supervisionadas.

## Estado

- desenho offline: presente;
- transporte RTU: nao implementado;
- biblioteca Modbus: nao selecionada;
- fake server: somente plano;
- COM8: nao acessada;
- Gate D offline: autorizado;
- transporte fisico: nao autorizado;
- modo de saida: OFF;
- TCP: backlog.

## Camadas

1. **UI**: edita perfil, inicia comandos explicitos, mostra estados e contadores.
2. **Session/Test Controller**: maquina de estados dos tres modos e cancelamento.
3. **Equipment Identification**: classifica assinatura e F21, sem escrita.
4. **Read-only Service**: expoe apenas leituras tipadas da allow-list.
5. **Supervised Output Service**: aceita somente `DO00`, `DO01`, `DO02`, `DO03`.
6. **RTU Transport**: futuro detalhe de baixo nivel, inacessivel diretamente a UI.

Nenhuma API publica do tipo `WriteRegister(address, value)` deve existir. O
servico de saida recebe um enum fechado de canal; o mapeamento para
31128..31131 permanece encapsulado. PWM, reservados e endereco arbitrario sao
irrepresentaveis no contrato de dominio.

## Maquina de estados

### Identificacao

`IDLE -> VALIDATING -> SEARCHING -> IDENTIFIED | NOT_RECOGNIZED | NO_RESPONSE |
SIGNATURE_MISMATCH | CRITICAL_FAILURE | CANCELLED`

- explicitamente iniciada;
- 1..247, crescente, uma tentativa por endereco;
- somente FC03;
- assinatura exige firmware/versao/ID/CRC e F21 aceitavel;
- para ao encontrar assinatura valida;
- sem polling, repeticao ou reconexao automatica;
- nenhuma saida acessivel.

### Teste de entradas

So pode iniciar depois de `IDENTIFIED`. Le DI00..DI07 e AI00..AI02 em operacoes
limitadas. Valor bruto deve ser preservado; conversao/unidade so e mostrada com
escala confirmada. Nao possui caminho para saidas.

### Teste supervisionado de saidas

So pode iniciar se todos os gates forem verdadeiros:

- equipamento identificado;
- assinatura valida;
- F21 sem falha critica;
- checklist de bancada aprovado;
- operador habilitou explicitamente o modo;
- gate fisico especifico autorizado.

Uma unica saida pode ficar ativa. Cada comando e momentaneo, possui duracao
maxima e cancelamento. No encerramento normal, o servico solicita desligamento,
valida o retorno e registra o resultado. Timeout ou resposta invalida aborta a
sequencia, impede avancar para outra saida e exige intervencao do operador.

Perda de comunicacao nao prova estado OFF. O procedimento fisico de isolamento e
desconexao continua sendo a barreira de seguranca.

## UI planejada

- COM e botao Atualizar portas;
- RS-232/RS-485, baud, data bits, paridade e stop bits;
- timeout, intervalo, endereco manual, inicio e fim da descoberta;
- Validar configuracao, Identificar equipamento, Procurar endereco e Cancelar;
- status de endereco, equipamento, firmware, versao, programa, ID, CRC e F21;
- abas DI00..DI07, AI00..AI02 e DO00..DO03;
- modo de escrita supervisionada claramente `DESABILITADO` por padrao;
- contadores de tentativas, conexoes, leituras, escritas e comandos fisicos.

## Logs planejados

Registrar inicio/fim, configuracao serial, tentativa de endereco, identificacao,
assinatura, F21, leitura de entrada, habilitacao do modo de saida, canal, valor,
duracao, retorno, desligamento, timeout, cancelamento e erro. Nunca registrar
segredo inexistente nem promover tentativa a operacao confirmada.

## Gate D offline

A implementacao autorizada deve registrar arquivos/classes, biblioteca quando
existir, encapsulamento, riscos, fake server, testes e rollback. O transporte
fisico permanece desabilitado por padrao e COM8 nao pode ser acessada.

## Fake server planejado

O simulador deve cobrir endereco correto/incorreto, sem resposta, assinatura
esperada/divergente, F21 normal/critico, DI, AI, DO, timeout, cancelamento,
resposta invalida e tentativa fora das allow-lists. Deve provar:

- nenhuma saida durante identificacao ou teste de entradas;
- somente DO00..DO03 no modo supervisionado;
- PWM, reservados e arbitrarios rejeitados;
- uma saida por vez;
- limite de duracao e cancelamento;
- desligamento solicitado e validado ao final;
- API generica de escrita nao exposta.

O fake server autorizado nunca usara COM8 nem endereco de equipamento real.
