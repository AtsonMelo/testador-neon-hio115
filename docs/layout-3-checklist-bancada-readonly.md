# Layout 3 - Checklist de bancada e gates de I/O

> O nome historico `readonly` permanece porque identificacao e entradas sao
> somente leitura. Saidas usam checklist e gate fisico adicionais.

## Gate C - Evidencias

- [ ] Foto legivel da etiqueta anexada e referenciada.
- [x] Identificacao frontal PIVODRIP / OMNICONTROL / OMNI-PLC2 registrada.
- [x] Numero de serie 111.20023 e part number 300.111.622.801 registrados.
- [x] Identidade HIstudio NEON5-1S / CPU450 / HIO115 slot 1 registrada.
- [ ] Relacao documental OMNI-PLC2 / NEON5-1S comprovada.
- [ ] Firmware da CPU confirmado por evidencia especifica; 3.3.11 ainda PROVAVEL.
- [x] Programa, versao 3220, ID 31134 e CRC 23248 registrados.
- [ ] Protocolo Modbus RTU ativado no canal confirmado.
- [x] Mapa documental F12, F13, F21, DI, AI e DO registrado.
- [x] Allow-list de leitura fechada registrada.
- [x] Allow-list de saida fechada em DO00..DO03 registrada.
- [ ] Referencias F10/F11 e conversao para endereco PDU confirmadas.

## Perfil RTU

- [x] COM8 registrada como evidencia observada, sem acesso pelo testador.
- [x] RS-232, 38400, 8-N-1 registrados.
- [x] ITF-A1 ou ITF-A2 registrada sem falsa precisao.
- [ ] Interface fisica A1/A2 exata fotografada ou documentada.
- [x] Endereco inicial 1 e descoberta 1..247 configurados offline.
- [x] Endereco historico 10 separado do default.
- [ ] Timeout operacional aprovado.
- [ ] Intervalo entre tentativas aprovado.
- [x] Uma tentativa por endereco, sem polling/reconexao.
- [x] Cancelamento imediato e inicio explicito planejados.
- [x] 0 e 248..255 bloqueados na descoberta.

## Seguranca eletrica e operacional

- [ ] Tensao realmente medida, instrumento e horario registrados.
- [ ] Aterramento comprovado por evidencia aplicavel.
- [ ] Canal serial de bancada isolado conforme avaliacao eletrica.
- [ ] Saidas desenergizadas/isoladas comprovadas.
- [ ] Maquina impedida de operar comprovada.
- [ ] Estado seguro da maquina comprovado.
- [x] Responsavel declarado: Atson Melo.
- [ ] Presenca do responsavel evidenciada no teste.
- [ ] Emergencia identificada e evidenciada.
- [ ] Desconexao rapida identificada e evidenciada.
- [ ] Criterios de aborto revisados com o responsavel.

As declaracoes de aterramento, isolamento, maquina segura, emergencia,
desconexao e backup estao registradas como `DECLARADO PELO RESPONSAVEL`; nao
substituem as evidencias acima.

## Backup e rastreabilidade

- [ ] Caminho/nome do backup registrado.
- [ ] SHA-256 do backup registrado.
- [ ] Backup verificado sem download/restart do controlador.
- [ ] Configuracao original da porta serial/rede do PC registrada.
- [ ] Diretorio de logs e identificador da sessao definidos.
- [ ] Relogio do PC e responsavel registrados.

## Gate D offline - Transporte em memoria e fake server

- [x] Gate D offline autorizado explicitamente em 2026-08-06.
- [ ] Transporte fisico autorizado separadamente.
- [ ] Biblioteca, versao e licenca aprovadas.
- [ ] Escrita arbitraria encapsulada e inacessivel.
- [ ] Transporte/feature default OFF.
- [ ] Fake server aprovado sem hardware.
- [ ] Timeout, cancelamento e limite de tentativas testados.
- [ ] Identificacao divergente bloqueia I/O.
- [ ] Varredura estatica confirma ausencia de caminho nao autorizado.

## Modo 1 - Identificacao

- [ ] Comando explicito do operador.
- [ ] Somente FC03 e referencias aprovadas.
- [ ] ID e CRC coincidem simultaneamente.
- [ ] Firmware/versao coincidem quando seus enderecos forem confirmados.
- [ ] F21 sem bit critico.
- [ ] Nenhuma escrita, coil ou saida executada.

## Modo 2 - Entradas

- [ ] Equipamento previamente identificado.
- [ ] Somente DI00..DI07 e AI00..AI02.
- [ ] Valor bruto preservado.
- [ ] Escala/unidade analogica confirmada antes de converter.
- [ ] Escrita permanece tecnicamente indisponivel.

## Modo 3 - Saidas supervisionadas

- [ ] Gate fisico especifico autorizado.
- [ ] Checklist eletrico aprovado e responsavel presente.
- [ ] Modo de escrita habilitado explicitamente.
- [ ] Duracao maxima aprovada.
- [ ] Somente uma saida por vez.
- [ ] Somente DO00..DO03 selecionaveis.
- [ ] Comando momentaneo e cancelamento disponiveis.
- [ ] Desligamento ao final e validacao de retorno previstos.
- [ ] Timeout/resposta invalida abortam sem avancar.
- [ ] Perda de comunicacao nao e interpretada como saida desligada.
- [ ] Escritas e comandos fisicos auditados individualmente.

## Criterios de aborto

- identidade ou assinatura divergente;
- qualquer bit critico em F21;
- endereco, protocolo ou referencia ambigua;
- tensao, aterramento, isolamento ou estado da maquina nao comprovados;
- resposta invalida, timeout ou cancelamento;
- tentativa fora das allow-lists;
- mais de uma saida solicitada;
- impossibilidade de confirmar desligamento;
- ausencia do responsavel ou perda da desconexao rapida.

Enquanto qualquer item obrigatorio permanecer aberto: `STATUS: NOT READY`.
