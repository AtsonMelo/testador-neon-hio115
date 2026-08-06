# Matriz de riscos da bancada - Layout 3 read-only

## Escala

- Probabilidade: baixa, media ou alta antes dos controles.
- Impacto: moderado, alto ou critico.
- A matriz e preliminar. Dados pendentes impedem reduzir risco por suposicao.
- Todo criterio de aborto encerra a tentativa e bloqueia repeticao automatica.

| Risco | Probabilidade | Impacto | Deteccao | Prevencao | Mitigacao | Criterio de aborto |
|---|---|---|---|---|---|---|
| Equipamento errado | Media | Critico | Etiqueta difere da ficha ou foto | Dupla conferencia de modelo, CPU, modulo e slot | Nao conectar; corrigir ficha e aprovacao | Qualquer divergencia de identidade |
| IP errado | Media | Alto | IP nao coincide com ficha aprovada | Copia controlada, revisao por duas pessoas e rede isolada | Cancelar e restaurar rede do PC | Resposta de host inesperado ou divergencia de IP |
| Porta errada | Media | Alto | Porta difere da ficha/mapa | Valor fechado na configuracao aprovada | Cancelar sem testar portas alternativas | Tentativa em porta nao aprovada |
| Protocolo errado | Media | Alto | Resposta invalida ou perfil divergente | Confirmar manual/programa e bloquear protocolos nao aprovados | Retirar cabo e revisar documentacao | Primeiro frame/resposta incompativel |
| Unit ID errado | Media | Alto | Endereco difere da ficha ou resposta vem de outra unidade | Unit ID fechado, sem varredura automatica | Cancelar; nao tentar IDs adjacentes | Resposta de unidade nao aprovada |
| Mapa incompativel | Alta | Critico | Endereco, tipo ou semantica divergem da evidencia | Mapa vinculado a firmware/programa e revisado por duas pessoas | Bloquear allow-list e retornar ao Gate C | Qualquer duvida sobre endereco ou significado |
| Firmware diferente | Media | Alto | Versao observada difere da ficha | Registrar firmware antes da conexao | Cancelar e revalidar mapa/protocolo | Divergencia de firmware |
| Leitura em area sensivel | Media | Critico | Endereco fora da allow-list ou classificado como comando | Allow-list minima, finalidade e evidencia por item | Cancelamento imediato e incidente | Qualquer acesso fora da allow-list |
| Escrita acidental | Baixa | Critico | Contador/log indica escrita ou funcao de escrita | Contrato sem escrita, feature OFF e testes negativos | Retirar cabo, observar saidas e abrir incidente | Tentativa ou contador de escrita diferente de zero |
| Biblioteca expondo escrita | Media | Critico | Revisao identifica API generica de escrita | Nao incluir biblioteca no preflight; futuro adapter sem API publica de escrita | Isolar/remover dependencia antes da bancada | Metodo de escrita acessivel pelo novo componente |
| Polling excessivo | Media | Alto | Leituras excedem limite ou intervalos repetidos no log | Single-shot, maximo de leituras e sem timer | Cancelar e revisar fluxo | Segunda rodada automatica ou limite excedido |
| Timeout | Media | Moderado | Operacao ultrapassa limite aprovado | Timeout curto e cancelamento testado com fake | Cancelar, desconectar e preservar log | Timeout ou operacao sem encerramento controlado |
| Reconexao automatica | Media | Alto | Nova tentativa aparece sem comando humano | Reconexao default OFF e contador de tentativas | Retirar cabo e bloquear execucao | Mais de uma tentativa nao autorizada |
| Aplicacao travada | Media | Alto | UI/console nao responde e cancelamento expira | Teste de cancelamento, timeout e log externo | Retirada do cabo pelo responsavel; encerramento autorizado | Cancelamento nao conclui no limite |
| Perda de rede/canal | Media | Moderado | Desconexao ou resposta incompleta | Cabo identificado, rede isolada e single-shot | Cancelar, nao reconectar, preservar evidencia | Perda do canal durante a operacao |
| Maquina ativa | Baixa | Critico | Estado fisico diverge do checklist | Bloqueio operacional, saidas isoladas e responsavel presente | Nao conectar ou retirar cabo imediatamente | Qualquer possibilidade de movimento/processo ativo |
| Saida nao isolada | Media | Critico | Inspecao eletrica mostra carga conectada/energizada | Desenergizar ou isolar saidas antes do teste | Interromper preparacao; responsavel corrige bancada | Saida ou carga real sem isolamento confirmado |
| Rede do PC nao restaurada | Media | Moderado | Configuracao final difere da captura inicial | Captura antes/depois e plano de rollback | Restaurar pelos valores registrados e revisar | Nao conseguir reproduzir configuracao original |

## Riscos adicionais do estado atual

| Risco | Probabilidade | Impacto | Deteccao | Prevencao | Mitigacao | Criterio de aborto |
|---|---|---|---|---|---|---|
| Fontes conflitantes para modelo/CPU/modulo | Alta | Critico | Inventario mostra combinacoes NEON/RION diferentes | Exigir foto da etiqueta e selecao explicita do conjunto | Manter configuracao bloqueada | Identidade ainda conflitante no preflight |
| Default de software tratado como dado real | Alta | Alto | Valor existe apenas no codigo/catalogo pendente | Marcar defaults como nao confirmados | Limpar campo e solicitar evidencia | Parametro sem fonte da unidade real |
| Mapa legado contendo comandos | Alta | Critico | `%MW` inclui habilitacao, reset e watchdog | Proibir copia automatica para allow-list | Criar allow-list nova e revisada | Inclusao de comando, coil ou endereco sem finalidade read-only |

## Aceitacao de risco

Nenhum risco critico pode ser aceito apenas por esta matriz. Reducao de risco
exige evidencia, controle implementado, reexecucao do preflight e aprovacao do
responsavel. A conexao continua bloqueada enquanto houver status `PENDENTE`,
`NÃO DISPONÍVEL` ou `CONFLITANTE`.
