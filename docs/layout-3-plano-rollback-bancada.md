# Plano de rollback da bancada - Layout 3 read-only

## Objetivo

Retornar aplicacao, comunicacao, PC e bancada ao estado anterior de forma
reprodutivel. O plano vale para um futuro teste autorizado; nesta fase nenhuma
conexao existe.

## Condicoes previas

- registrar configuracao de rede/serial atual do PC antes de qualquer ajuste;
- registrar cabos, conversores, fonte e estado visual das saidas;
- identificar quem pode retirar o cabo e desenergizar a bancada;
- preservar backup do projeto HIstudio e referencia do checkpoint Git;
- definir o diretorio imutavel de logs e o identificador do ensaio.

Sem essas evidencias, o teste deve ser cancelado antes da conexao.

## Cancelamento normal da aplicacao

1. Solicitar cancelamento pelo mecanismo previsto no futuro transporte.
2. Aguardar somente o tempo de encerramento aprovado.
3. Confirmar que nao ha operacao pendente.
4. Encerrar o processo pelo fluxo normal da aplicacao ou `Ctrl+C` quando a
   execucao for em console.
5. Nao usar encerramento forcado sem registrar PID, caminho, estado e obter
   autorizacao do responsavel.

## Interromper uma tentativa de conexao

1. Acionar o cancelamento uma unica vez.
2. Nao permitir reconexao automatica.
3. Se o cancelamento nao concluir no limite aprovado, declarar incidente.
4. O responsavel designado deve retirar o cabo pelo ponto de desconexao rapida.
5. Nao repetir a tentativa ate revisar logs, parametros e estado da bancada.

## Retirada do cabo

1. Confirmar que a pessoa designada esta em posicao segura.
2. Retirar somente o cabo de comunicacao identificado na ficha.
3. Nao manipular bornes energizados nem cabos de I/O como forma de rollback.
4. Confirmar visualmente que o canal ficou desconectado.
5. Registrar horario e responsavel pela retirada.

## Restaurar a configuracao do PC

1. Usar a evidencia capturada antes do teste como unica fonte de valores.
2. Restaurar DHCP ou os valores estaticos exatamente como registrados.
3. Remover apenas rotas ou ajustes criados para a bancada e identificados na
   evidencia.
4. Conferir interface, endereco, mascara, gateway e DNS contra o registro
   anterior.
5. Registrar a verificacao final; nao testar conectividade contra o CLP.

## Confirmar zero escritas

1. Verificar que o componente read-only nao expoe API de escrita.
2. Conferir o contador `Escritas reais` no log de inicio e encerramento.
3. Exigir valor `0` em todas as linhas e no resumo.
4. Procurar eventos `write`, `coil`, `command` ou equivalentes no log.
5. Qualquer valor diferente de zero torna o ensaio reprovado e abre incidente.

## Confirmar zero alteracoes em saidas

1. Comparar o estado visual/eletrico registrado antes e depois do teste.
2. Obter confirmacao independente do responsavel da bancada.
3. Nao acionar saida para realizar essa confirmacao.
4. Qualquer mudanca observada exige desconexao imediata e tratamento como
   incidente, mesmo que os contadores indiquem zero.

## Recuperar logs

1. Copiar os arquivos para o diretorio de evidencias do ensaio.
2. Registrar tamanho, data/hora e hash SHA-256.
3. Preservar stdout, stderr, preflight, parametros aprovados e resumo final.
4. Nao editar o arquivo original; redacoes ou anotacoes devem ficar em arquivo
   separado.
5. Registrar ausencia de log como falha do ensaio.

## Retornar ao checkpoint anterior

O checkpoint de software anterior a qualquer futuro transporte e:

```text
layout-3-fase-3-8-polimento-visual-completo-host-ok-20260806
```

Para inspecao local, usar checkout destacado ou nova branch a partir da tag.
Nao usar `reset --hard`, `clean`, force push ou reescrever a branch operacional.
O retorno de software nao altera nem restaura programa de CLP.

## Registro de incidente

Registrar no minimo:

- identificador, data/hora e participantes;
- equipamento e etiqueta;
- commit, configuracao e comando executado;
- ultimo passo concluido;
- sintoma e criterio de aborto acionado;
- contadores de conexao, leitura, escrita e comando;
- estado das saidas e da maquina;
- acao de desconexao;
- arquivos de log e respectivos hashes;
- configuracao do PC antes/depois;
- decisao sobre repeticao, sempre exigindo nova aprovacao.

## Criterio de rollback concluido

O rollback termina somente quando:

- processo encerrado;
- cabo retirado;
- PC restaurado;
- logs preservados;
- escritas e comandos fisicos confirmados em zero;
- saidas e maquina no estado seguro original;
- incidente registrado ou encerramento normal assinado.
