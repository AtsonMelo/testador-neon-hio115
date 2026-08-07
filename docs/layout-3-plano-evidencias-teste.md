# Layout 3 - Plano de evidencias dos testes

## Objetivo

Provar o estado de configuracao, cada gate e cada operacao sem confundir dados
offline com leitura fisica. Nesta fase, todos os contadores fisicos permanecem 0.

## Pacote pre-bancada

- branch, commit e Draft PR;
- build Release e validadores;
- self-tests offline e preflight;
- configuracao JSON usada;
- sete documentos obrigatorios;
- varredura de ausencia de transporte executavel;
- foto/etiqueta e relacao documental das identidades;
- caminho e SHA-256 do backup;
- medicao de tensao e evidencia de aterramento/isolamento;
- baseline da configuracao do PC;
- autorizacoes Gate D e gate fisico, quando existirem.

## Evidencias de identificacao

Registrar por tentativa: horario, endereco, resultado e duracao. Para resposta,
registrar familia/versao, F12/ID, F13/CRC, F21 bruto e bits decodificados. O
equipamento so e `IDENTIFICADO` quando toda a assinatura aprovada coincide.

Resultados permitidos:

- IDENTIFICADO;
- RESPONDEU MAS NAO RECONHECIDO;
- SEM RESPOSTA;
- ASSINATURA DIVERGENTE;
- FALHA CRITICA;
- CANCELADO.

ID ou CRC divergentes comprovam apenas que houve resposta, nunca que o
equipamento correto foi encontrado.

## Evidencias de entradas

Para DI00..DI07: horario, canal, referencia documental, valor bruto e resultado.
Para AI00..AI02: horario, canal, referencia, valor bruto e, somente com escala
confirmada, valor convertido/unidade. A apresentacao 4-20 mA deve ser registrada
como configuracao observada, nao como leitura atual.

## Evidencias de saidas supervisionadas

Somente apos gate fisico: autorizacao, operador, canal DO00..DO03, estado inicial,
valor solicitado, horario, duracao limite, resposta, comando de desligamento,
retorno e estado final observado. Registrar separadamente contador de escritas e
contador de comandos fisicos.

Timeout, cancelamento ou retorno invalido encerram a sequencia. Se OFF nao for
confirmado, registrar estado `UNKNOWN`, isolar fisicamente e abrir incidente.

## Evidencias do fake server

Depois do Gate D, cobrir sem hardware:

- endereco correto/incorreto e sem resposta;
- firmware, ID, CRC e F21 esperados/divergentes;
- DI, AI e DO;
- timeout, cancelamento e resposta invalida;
- tentativa em 31137, 31140, 31143, 31144, 31145 e endereco arbitrario;
- saida durante identificacao/entradas;
- duas saidas simultaneas;
- duracao maxima e desligamento final.

Anexar saida dos testes, configuracao do fake, commit e prova de que COM8 nao foi
aberta.

## Nomenclatura de sessao

`YYYYMMDD-HHMMSS_LAYOUT3_<commit-curto>_<modo>`

Cada artefato deve referenciar o identificador da sessao. Hashes SHA-256 devem
ser registrados para backup e logs finais, sem alterar o projeto do controlador.

## Criterios de aceite

- eventos e contadores reconciliados;
- nenhuma operacao fora da allow-list;
- nenhuma escrita nos modos 1 e 2;
- saidas apenas no modo 3 e com gate fisico;
- abortos e cancelamentos preservam motivo;
- rollback executavel;
- conexao fisica somente apos autorizacao final especifica.
