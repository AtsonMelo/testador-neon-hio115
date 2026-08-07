# Ficha de parametros de conexao - Layout 3 read-only

## Instrucao de preenchimento

Preencher somente a partir de etiqueta, programa carregado, manual aplicavel ou
confirmacao do responsavel da bancada. Registrar a fonte. Nao copiar defaults do
app como se fossem valores reais.

Status permitidos: `CONFIRMADO`, `PENDENTE`, `NÃO DISPONÍVEL`, `CONFLITANTE`.
`PROVÁVEL` e aceito somente como classificacao explicita de evidencia parcial
e nunca satisfaz um campo que exige confirmacao.

## Identificacao

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Identidade no HIstudio | `NEON5-1S / CPU450 / HIO115` | `CONFIRMADO` nessa fonte | Evidencia direta do HIstudio conectado |  |
| Identificacao frontal | `PIVODRIP` | `OBSERVADO` | Inspecao fisica frontal |  |
| Fabricante/marca frontal | `OMNICONTROL` | `OBSERVADO` | Inspecao fisica frontal |  |
| Modelo frontal | `OMNI-PLC2` | `OBSERVADO` | Inspecao fisica frontal |  |
| Numero de serie | `111.20023` | `OBSERVADO` | Inspecao fisica frontal |  |
| Part number | `300.111.622.801` | `OBSERVADO` | Inspecao fisica frontal |  |
| Identificacao adicional | `Slot 1115` | `OBSERVADO` | Compativel com HIO115; nao e prova definitiva |  |
| Relacao OMNI-PLC2 / NEON5-1S |  | `NÃO CONFIRMADA` | Exige comprovacao documental |  |
| Resultado da identidade | `OMNI-PLC2 versus NEON5-1S` | `CONFLITANTE` | Nao deduzir equivalencia |  |
| CPU | `CPU450` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Slot da CPU | `0` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Maximo de modulos | `2` | `CONFIRMADO` | Valor exibido no HIstudio |  |
| Modulos detectados | `2` | `CONFIRMADO` | Valor exibido no HIstudio |  |
| Revisao de hardware da CPU exibida | `1` | `CONFIRMADO` | Valor exibido; nao e firmware |  |
| Revisao de firmware do modulo CPU exibida | `0` | `CONFIRMADO` | Valor exibido; nao promove o firmware da CPU |  |
| Status funcional da CPU | `operacional` | `CONFIRMADO` | HIstudio; inicializacao, operacao, intermitente e configuracao sem falhas |  |
| Modelo do modulo | `HIO115` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Slot do modulo | `1` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Revisoes exibidas do HIO115 | `hardware 0 / firmware 0` | `CONFIRMADO` | Valores exibidos no HIstudio |  |
| Status funcional do HIO115 | `operacional` | `CONFIRMADO` | Valor exibido no HIstudio |  |
| Foto/etiqueta |  | `NÃO DISPONÍVEL` |  |  |
| Firmware | `3.3.11` | `PROVÁVEL` | Valor na barra de status; falta evidencia especifica da CPU |  |
| HIstudio | `2.4.03` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Programa HIstudio carregado | `MOTOR_HIDRO:PROD_NEON5_HIO115` | `CONFIRMADO` | Condicao observada: Programa rodando |  |
| Versao do programa | `3220` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Identificador do programa | `31134` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| CRC do programa | `23248` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Modo de inicializacao observado | `Cold restart` | `CONFIRMADO` | Evidencia direta do HIstudio conectado |  |
| Backup do programa | `OK` | `DECLARADO PELO RESPONSÁVEL` | Atson Melo; caminho/nome ainda ausentes |  |
| Algoritmo e hash do backup |  | `PENDENTE` |  |  |

## Interfaces e capacidades observadas

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Interfaces disponiveis | `ITF-A1 RS232-C; ITF-A2 RS232-C; ITF-B RS485` | `CONFIRMADO` | Inventario exibido no HIstudio; nao identifica a interface ativa |  |
| Interface efetivamente utilizada | `ITF-A1_OR_ITF-A2` | `NÃO IDENTIFICADA` | Ambas sao RS232-C; diferenciar fisicamente |  |
| Conector/cabo atual | `DB9 Serial / RS-232` | `CONFIRMADO` | Observacao fisica e informacao do responsavel |  |
| RS-485 disponivel | `Bornes D+ / D-; chave de terminacao presente` | `OBSERVADO` | Nao indica uso atual de RS-485 |  |
| Entradas digitais disponiveis | `8: I00-I07` | `CONFIRMADO` | Capacidade exibida do HIO115; estado atual nao confirmado |  |
| Saidas digitais disponiveis | `4: O00-O03` | `CONFIRMADO` | Capacidade exibida do HIO115; estado atual nao confirmado |  |
| Entradas analogicas disponiveis | `3: AI00-AI02; apresentacao 4-20 mA` | `CONFIRMADO` | Capacidade/apresentacao exibidas; valores atuais nao confirmados |  |
| Contadores rapidos disponiveis | `3: FCT0-FCT2` | `CONFIRMADO` | Capacidade exibida; valores atuais nao confirmados |  |
| PWM disponivel | `1: PWM00` | `CONFIRMADO` | Capacidade exibida; estado atual nao confirmado |  |
| Condicao das capturas | `Equipamento remoto offline; sem base de hardware definida no ambiente` | `CONFIRMADO` | Limita a evidencia a inventario/configuracao, sem leitura fisica |  |

## Protocolo e topologia

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Perfis suportados pelo testador | `Modbus RTU / Modbus TCP` | `CONFIRMADO` | Decisao tecnica do projeto; selecao explicita |  |
| Perfil inicial selecionado | `RTU` | `CONFIRMADO` | Perfil offline; nao confirma protocolo ativo |  |
| Protocolo realmente ativado no canal |  | `PENDENTE` | Confirmar antes de qualquer transporte |  |
| Transporte observado | `Serial` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Camada fisica atual | `RS232` | `CONFIRMADO` | Evidencia tecnica recebida |  |
| Interface do controlador | `ITF-A1_OR_ITF-A2` | `NÃO IDENTIFICADA` | Ambas sao RS232-C; diferenca exata e pendencia documental |  |
| Conversor/cabo real | `RS-232` | `CONFIRMADO` | Perfil atual confirmado; identificacao/foto do cabo permanece pendente |  |
| Outro mestre ausente |  | `PENDENTE` |  |  |
| Rede/canal isolado |  | `PENDENTE` |  |  |

## Parametros TCP, quando aplicavel

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| IP do CLP |  | `NÃO DISPONÍVEL` |  |  |
| Porta TCP |  | `NÃO DISPONÍVEL` | Nao preencher por default |  |
| Topologia TCP isolada |  | `PENDENTE` | Obrigatoria somente quando TCP for selecionado |  |
| Unit ID/endereco inicial TCP | `10` | `CONFIRMADO` | Endereco atual conhecido; campo editavel |  |
| Timeout TCP |  | `PENDENTE` | Exigido somente quando TCP for selecionado |  |
| Maximo de tentativas TCP | `1` | `CONFIRMADO` | Politica de uma tentativa por operacao/endereco |  |
| IP do PC antes do ajuste |  | `NÃO DISPONÍVEL` |  |  |
| IP do PC para bancada |  | `NÃO DISPONÍVEL` |  |  |
| Mascara |  | `NÃO DISPONÍVEL` |  |  |
| Gateway/DNS alterados |  | `NÃO DISPONÍVEL` |  |  |

## Parametros seriais, quando aplicavel

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Driver | `SERIAL_DRIVER` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Canal | `Channel_01` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Porta serial | `COM8` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Camada fisica selecionada | `RS232` | `CONFIRMADO` | Perfil atual; opcoes futuras RS232 ou RS485 |  |
| Interface do controlador | `ITF-A1_OR_ITF-A2` | `NÃO IDENTIFICADA` | Nao bloqueia o perfil de software; confirmar fisicamente |  |
| Conector fisico atual | `DB9 Serial` | `CONFIRMADO` | Identificacao frontal observada |  |
| Baud rate | `38400` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Data bits | `8` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Paridade | `None` | `CONFIRMADO` | Nenhuma, conforme HIstudio |  |
| Stop bits | `One` | `CONFIRMADO` | 1 stop bit, conforme HIstudio |  |
| Timeout entre caracteres | `50 ms` | `CONFIRMADO` | Nao e o timeout de leitura do preflight |  |
| Atraso para transmissao | `2 ms` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Atraso para remover portadora | `0 ms` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Tamanho maximo do frame | `256` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Remapeamento de endereco | `Nao` | `CONFIRMADO` | Configuracao observada no HIstudio |  |
| Bornes RS-485 disponiveis | `D+ / D-` | `OBSERVADO` | Nao usados pelo cabo atual |  |
| Chave de terminacao RS-485 | `Presente` | `OBSERVADO` | Posicao/estado nao confirmados |  |
| Endereco RTU | `10` | `CONFIRMADO` | Endereco atual conhecido |  |
| Timeout RTU |  | `PENDENTE` | Deve ficar entre 100 e 5000 ms |  |
| Maximo de tentativas RTU | `1` | `CONFIRMADO` | Politica de uma tentativa por operacao/endereco |  |
| Intervalo entre tentativas RTU |  | `PENDENTE` | Valor deve ser aprovado antes do Gate D |  |

## Endereco e politica de leitura

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Unit ID/endereco atual | `10` | `CONFIRMADO` | Evidencia tecnica recebida |  |
| Referencia do mapa |  | `CONFLITANTE` | `%MW10..74` e SysVars 1120..1134 nao formam allow-list aprovada |  |
| Revisao independente do mapa |  | `PENDENTE` |  |  |
| Maximo de leituras |  | `PENDENTE` | Deve ser positivo e nao exceder a allow-list |  |
| Single-shot | `true` | `CONFIRMADO` | Politica de software da Fase 3.9 |  |
| Polling | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |
| Reconexao automatica | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |
| Escrita | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |
| Comunicacao real default | `false` | `CONFIRMADO` | Configuracao fail-closed da Fase 3.9 |  |

## Allow-list read-only

Nao preencher a partir de `Hio115MemoryMap.cs` sem aprovacao do mapa real.

| Nome | Area | Endereco | Finalidade | Evidencia de aprovacao | Revisor |
|---|---|---:|---|---|---|
|  |  |  |  |  |  |

Areas aceitas pelo preflight: `input_register` e `holding_register`, sempre com
`access = read`. Coil, escrita, comando, setpoint e endereco sem evidencia sao
proibidos.

Os nomes de canais de I/O observados no HIstudio nao sao enderecos desta
allow-list. Nao converter `I00-I07`, `O00-O03`, `AI00-AI02`, `FCT0-FCT2`,
`PWM00`, `%MW10..74` ou SysVars em registradores autorizados sem mapa aprovado.

## Politica de enderecos e descoberta planejada

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Faixa representavel | `1..255` | `CONFIRMADO` | Decisao tecnica do projeto |  |
| Faixa automatica segura | `1..247` | `CONFIRMADO` | Decisao tecnica do projeto |  |
| Reservados/vendor-specific | `248..255` | `CONFIRMADO` | Exigem modo avancado e aprovacao manual |  |
| Broadcast | `0` | `CONFIRMADO` | Proibido |  |
| Nunca sondar automaticamente | `255` | `CONFIRMADO` | Regra explicita |  |
| Faixa default de descoberta | `10..10` | `CONFIRMADO` | Endereco unico recomendado |  |
| Allow-list de descoberta default | `[10]` | `CONFIRMADO` | Nenhum outro endereco permitido por default |  |
| Descoberta habilitada | `false` | `CONFIRMADO` | Default OFF; nao implementada |  |
| Maximo de tentativas por endereco | `1` | `CONFIRMADO` | Sem repeticao continua |  |
| Intervalo entre tentativas |  | `PENDENTE` | Nao inventar valor |  |
| Funcao planejada | `FC03` | `CONFIRMADO` | Somente leitura; sem coil/escrita |  |
| Candidato 1 | `F12 / 30012; R; esperado 31134` | `CONFIRMADO` | ID do programa observado |  |
| Candidato 2 | `F13 / 30013; R; esperado 23248` | `CONFIRMADO` | CRC do programa observado |  |
| Endereco de dados do protocolo |  | `PENDENTE` | Nao deduzir offset de 30012/30013; depende do mapa aprovado |  |

Os candidatos de descoberta nao integram a allow-list operacional. Um futuro
endereco so pode ser identificado quando ID e CRC coincidirem simultaneamente.
Nao usar I/O do HIO115 para descoberta.

## Seguranca da bancada

| Campo | Valor | Status atual | Fonte/evidencia | Revisor |
|---|---|---|---|---|
| Indicacao nominal frontal | `1030 VDC` | `OBSERVADO` | Texto registrado literalmente; nao e medicao nem faixa deduzida |  |
| Tensao efetivamente medida |  | `PENDENTE` | Exige valor, instrumento e evidencia |  |
| Aterramento | `OK` | `DECLARADO PELO RESPONSÁVEL` | Atson Melo; falta evidencia |  |
| Saidas desenergizadas/isoladas | `OK` | `DECLARADO PELO RESPONSÁVEL` | Atson Melo; falta evidencia |  |
| Maquina impedida de operar | `OK` | `DECLARADO PELO RESPONSÁVEL` | Atson Melo; falta evidencia |  |
| Estado seguro da maquina | `OK` | `DECLARADO PELO RESPONSÁVEL` | Atson Melo; falta evidencia |  |
| Responsavel presente | `Atson Melo` | `DECLARADO PELO RESPONSÁVEL` | Falta registro/assinatura de evidencia |  |
| Emergencia identificada | `OK` | `DECLARADO PELO RESPONSÁVEL` | Falta foto/identificacao |  |
| Desconexao rapida definida | `OK` | `DECLARADO PELO RESPONSÁVEL` | Falta meio e evidencia |  |
| Configuracao original de rede do PC |  | `PENDENTE` | Exige evidencia antes de qualquer ajuste |  |

## Aprovacoes

- Preenchido por:
- Revisado por:
- Data/hora:
- Evidencia da aprovacao do mapa:
- Evidencia da aprovacao da bancada:
- Gate D para implementar transporte: nao autorizado.
- Autorizacao para conectar ao CLP: nao concedida nesta ficha.
