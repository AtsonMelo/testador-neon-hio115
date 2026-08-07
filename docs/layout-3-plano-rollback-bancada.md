# Layout 3 - Plano de cancelamento e rollback de bancada

## Estado atual

O plano e documental. Nao houve conexao, leitura, escrita ou comando fisico. O
Gate D e o gate fisico de saidas permanecem bloqueados.

## Abortagem imediata

1. Acionar `Cancelar` na sessao quando essa funcao existir.
2. Impedir nova tentativa, polling ou reconexao.
3. Se uma saida supervisionada tiver sido acionada, solicitar OFF somente pelo
   contrato fechado do mesmo canal e registrar retorno.
4. Se a resposta for ausente ou invalida, nao assumir OFF; entrar em estado de
   atencao e executar isolamento/desconexao fisica pelo responsavel.
5. Retirar o cabo serial pelo ponto de desconexao rapida previamente identificado.
6. Impedir que a maquina opere e usar a emergencia conforme procedimento local.
7. Preservar logs, horario, operador, canal, ultimo estado conhecido e erro.

## Restauracao do PC

1. Fechar somente a sessao do testador, sem encerrar processos nao identificados.
2. Restaurar configuracao original de porta serial/rede a partir da evidencia
   coletada antes do teste.
3. Confirmar que nenhuma porta ficou aberta pelo processo.
4. Confirmar feature, comunicacao, polling, reconexao e modo de saida em OFF.
5. Arquivar diff da configuracao temporaria, sem gravar dados do equipamento.

## Confirmacao de zero ou de operacoes controladas

- antes do gate fisico: conexoes, leituras, escritas e comandos devem ser zero;
- apos identificacao/entradas autorizadas: revisar contadores de conexao/leitura;
- apos saida autorizada: confrontar cada escrita e comando com canal, valor,
  duracao, retorno e autorizacao;
- qualquer operacao sem log correspondente e incidente;
- comparar estado visual/eletrico das saidas com a evidencia anterior ao teste;
- nunca inferir desligamento por perda da comunicacao.

## Retorno de software

1. Preservar os logs e o commit do teste.
2. Parar a aplicacao.
3. Retornar ao checkpoint/tag anterior somente por procedimento Git nao
   destrutivo aprovado.
4. Executar build e validadores locais.
5. Confirmar branch limpa antes de nova sessao.

Nenhum `reset --hard`, `clean`, force push, merge ou tag faz parte deste plano.

## Recuperacao de evidencias

Coletar log integral, configuracao usada, SHA do commit, horario de inicio/fim,
responsavel, endereco tentado, assinatura recebida, F21, leituras, escritas,
comandos, cancelamento, timeout e estado final observado.

## Registro de incidente

Registrar imediatamente:

- identificador e horario;
- pessoas presentes;
- equipamento/etiqueta;
- configuracao RTU;
- ultimo comando e resposta;
- saida possivelmente ativa;
- acao de isolamento/desconexao;
- contadores;
- logs e hashes;
- impacto observado;
- autorizacao para qualquer retomada.

Nao retomar teste no mesmo incidente sem nova avaliacao e autorizacao explicita.
