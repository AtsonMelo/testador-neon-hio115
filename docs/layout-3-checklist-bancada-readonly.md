# Checklist de bancada read-only - Layout 3

## Identificacao do ensaio

- Data/hora planejada:
- Local:
- Responsavel tecnico:
- Operador:
- Revisor da allow-list:
- Branch/commit:
- Evidencia da aprovacao:

Nenhum item fisico deve ser marcado por inferencia. Campo vazio ou evidencia
indisponivel bloqueia o ensaio.

## Evidencia parcial recebida do HIstudio

- [x] Controlador observado: `NEON5-1S`.
- [x] CPU observada: `CPU450`, slot 0.
- [x] Modulo observado: `HIO115`, slot 1.
- [x] Limite de 2 modulos e 2 modulos detectados observados.
- [x] Interfaces disponiveis observadas: `ITF-A1` e `ITF-A2` RS232-C;
      `ITF-B` RS485.
- [x] CPU observada com revisao de hardware 1, revisao de firmware exibida 0,
      status operacional e sem falhas nos quatro diagnosticos apresentados.
- [x] HIO115 observado com revisoes exibidas 0/0 e status operacional.
- [x] Capacidades do HIO115 observadas: 8 DI `I00-I07`, 4 DO `O00-O03`,
      3 AI `AI00-AI02`, 3 FCT `FCT0-FCT2` e 1 PWM `PWM00`.
- [x] Apresentacao das entradas analogicas observada como 4-20 mA.
- [x] HIstudio observado: `2.4.03`.
- [x] Canal observado: `SERIAL_DRIVER / Channel_01 / COM8 / 38400 / 8N1`.
- [x] Temporizacao observada: 50 ms entre caracteres, 2 ms para transmissao e
      0 ms para remover portadora.
- [x] Frame maximo 256 e remapeamento de endereco desabilitado.
- [x] Programa observado rodando: `MOTOR_HIDRO:PROD_NEON5_HIO115`, versao 3220,
      identificador 31134, CRC 23248, inicializacao `Cold restart`.
- [x] Perfil fisico atual confirmado como RS-232, COM8 e endereco 10.
- [x] Interface registrada como `ITF-A1_OR_ITF-A2`; ambas sao RS232-C.
- [x] Perfis futuros definidos como Modbus RTU e Modbus TCP, com selecao
      explicita e sem conexao automatica.
- [x] Frontal observado: `PIVODRIP / OMNICONTROL / OMNI-PLC2`, serie
      `111.20023`, part number `300.111.622.801`.
- [x] Identificacao adicional `Slot 1115` registrada apenas como compativel com
      HIO115, sem prova definitiva.
- [x] Conector atual observado como DB9 `Serial`; camada atual informada RS-232.
- [x] Bornes RS-485 `D+ / D-` e chave de terminacao observados separadamente;
      nao caracterizam o cabo atual.
- [x] Indicacao nominal frontal registrada literalmente como `1030 VDC`, sem
      tratar como tensao medida ou deduzir faixa.
- [ ] Relacao documental entre `OMNI-PLC2` e `NEON5-1S` confirmada.
- [ ] Foto/etiqueta da unidade recebida.
- [ ] Firmware da CPU confirmado. `3.3.11` permanece apenas `PROVÁVEL`.
- [ ] Interface fisica exata confirmada entre ITF-A1 e ITF-A2. Esta pendencia e
      documental e nao bloqueia o perfil RS232 de software.
- [ ] Protocolo realmente ativado no canal confirmado.

O estado "Programa rodando" nao confirma estado seguro da maquina ou das
saidas e nao autoriza comunicacao pelo Testador. As capturas mostram
`Equipamento remoto offline` e `Nao existe base de hardware definida no
ambiente`; por isso, nenhum estado atual de entrada/saida, valor analogico,
contador ou PWM foi confirmado.

## Equipamento e programa

- [x] Equipamento identificado visualmente como `OMNI-PLC2` no frontal.
- [ ] Foto legivel da etiqueta anexada.
- [ ] Modelo exato reconciliado entre frontal e HIstudio.
- [ ] CPU e slot conferidos.
- [ ] Modelo do modulo e slot conferidos.
- [ ] Firmware conferido no equipamento ou ferramenta oficial.
- [ ] Programa HIstudio carregado identificado por nome, versao e hash/backup.
- [x] Backup declarado `OK` por Atson Melo.
- [ ] Caminho/nome do backup, algoritmo e hash registrados e verificados.
- [ ] Nenhum artefato HIstudio sera alterado pelo teste.

## Protocolo e mapa

- [ ] Protocolo confirmado para a unidade real.
- [x] Perfil atual RTU selecionado explicitamente no JSON offline.
- [x] Camada fisica atual RS232 confirmada.
- [x] Conector atual DB9 `Serial` registrado.
- [ ] Interface fisica exata A1/A2 documentada.
- [ ] IP do PC registrado, quando aplicavel.
- [ ] IP do CLP registrado, quando aplicavel.
- [ ] Porta TCP registrada, quando aplicavel.
- [ ] Topologia isolada confirmada, quando TCP estiver selecionado.
- [ ] Porta serial registrada, quando aplicavel.
- [ ] Baud rate, data bits, paridade e stop bits registrados, quando aplicavel.
- [x] Unit ID/endereco atual confirmado como 10.
- [ ] Mapa de registradores vinculado ao programa e firmware reais.
- [ ] Revisao independente do mapa concluida.
- [ ] Registradores de leitura aprovados individualmente.
- [ ] Allow-list sem coils, comandos, setpoints ou enderecos de escrita.
- [ ] Cada item da allow-list possui evidencia e finalidade.

## Seguranca eletrica e operacional

- [x] Indicacao nominal frontal `1030 VDC` registrada sem interpretacao.
- [ ] Tensao efetivamente medida e instrumento/evidencia registrados.
- [ ] Fonte e polaridade confirmadas.
- [x] Declaracao de aterramento `OK` recebida de Atson Melo.
- [ ] Aterramento verificado por evidencia.
- [ ] Rede ou canal de comunicacao isolado para a bancada.
- [ ] Nenhum outro mestre ou software disputa o canal.
- [x] Saidas desenergizadas/isoladas declaradas `OK` por Atson Melo.
- [ ] Saidas desenergizadas ou eletricamente isoladas verificadas.
- [x] Maquina impedida de operar declarada `OK` por Atson Melo.
- [ ] Maquina impedida de operar verificada.
- [x] Estado seguro da maquina declarado `OK` por Atson Melo.
- [ ] Estado seguro da maquina verificado.
- [ ] Cargas reais removidas ou isoladas conforme procedimento aprovado.
- [x] Presenca de Atson Melo declarada.
- [ ] Presenca do responsavel registrada por evidencia/assinatura.
- [x] Emergencia e desconexao rapida declaradas `OK` por Atson Melo.
- [ ] Botao de emergencia identificado e acessivel por evidencia.
- [ ] Desconexao rapida identificada, acessivel e atribuida por evidencia.
- [ ] Cabo pode ser retirado sem contato com parte energizada.

## Controles do software

- [ ] Feature flag default confirmada como `OFF`.
- [ ] Comunicacao real default confirmada como `OFF`.
- [ ] Escrita tecnicamente desabilitada.
- [ ] Nenhuma API publica de escrita no componente read-only.
- [ ] Coils proibidas.
- [ ] Allow-list fechada.
- [ ] Operacao single-shot confirmada.
- [ ] Polling continuo desabilitado.
- [ ] Reconexao automatica desabilitada.
- [ ] Timeout aprovado: ______ ms.
- [ ] Maximo de leituras aprovado: ______.
- [ ] Intervalo entre tentativas aprovado: ______ ms.
- [x] Maximo de uma tentativa por endereco.
- [x] Faixa representavel 1..255; descoberta automatica limitada a 1..247.
- [x] Endereco 0 proibido e 255 nunca sondado automaticamente.
- [x] Enderecos 248..255 bloqueados sem modo avancado, selecao manual unica,
      aviso e aprovacao explicita.
- [x] Descoberta default OFF, range `10..10` e allow-list `[10]`.
- [x] Descoberta planejada somente FC03, sem coil ou escrita.
- [x] Identificacao planejada exige F12/30012 = 31134 e F13/30013 = 23248.
- [ ] Traducao dos candidatos para endereco de dados do protocolo aprovada no
      mapa; nenhum offset pode ser deduzido.
- [ ] Gate D autorizado para implementar transporte em branch separada.
- [ ] Cancelamento testado com fake.
- [ ] Contadores de escrita e comandos fisicos fixados em zero.

## Preflight local

- [ ] Build Release: 0 erros.
- [ ] Todos os self-tests de configuracao aprovados.
- [ ] Cinco validadores anteriores: 5/5.
- [ ] `--validate-layout-3-bench-readiness`: exit code 0.
- [ ] Varredura de APIs proibidas: limpa.
- [ ] `git diff --check`: OK.
- [ ] Branch limpa e PR revisavel.
- [ ] Evidencias armazenadas no diretorio aprovado.
- [ ] Configuracao original de rede do PC registrada.

## Rollback e logs

- [ ] Plano de rollback revisado pelo responsavel.
- [ ] Configuracao de rede atual do PC registrada antes do teste.
- [ ] Procedimento para restaurar IP do PC disponivel.
- [ ] Local de logs definido e com espaco disponivel.
- [ ] Relogio do PC conferido para correlacao das evidencias.
- [ ] Criterio de zero escritas definido no log.
- [ ] Criterio de zero alteracoes de saida definido por observacao independente.
- [ ] Formulario de incidente pronto.

## Criterios de aborto imediato

Abortar sem nova tentativa quando ocorrer qualquer item abaixo:

- identificacao do equipamento divergir da ficha;
- firmware, programa, protocolo, topologia ou mapa divergirem;
- qualquer parametro estiver pendente ou conflitante;
- tentativa de acesso fora da allow-list;
- tentativa de funcao de escrita ou coil;
- contador de escrita ou comando fisico diferente de zero;
- leitura acima do limite aprovado;
- polling ou reconexao nao planejada;
- timeout repetido ou resposta inconsistente;
- perda de rede/canal ou aplicacao sem responder;
- maquina sair do estado seguro;
- saida mudar de estado;
- responsavel, emergencia ou desconexao rapida deixarem de estar disponiveis.

## Encerramento

- [ ] Aplicacao cancelada.
- [ ] Canal fisico desconectado pelo responsavel.
- [ ] Conexoes reais registradas.
- [ ] Leituras reais registradas.
- [ ] Escritas reais confirmadas como zero.
- [ ] Comandos fisicos confirmados como zero.
- [ ] Saidas confirmadas sem alteracao.
- [ ] IP/configuracao do PC restaurados e conferidos.
- [ ] Logs preservados sem edicao.
- [ ] Resultado e incidentes assinados pelos participantes.
