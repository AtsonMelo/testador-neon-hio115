# Matriz de riscos da bancada - Layout 3 read-only

## Escala

- Probabilidade: baixa, media ou alta antes dos controles.
- Impacto: moderado, alto ou critico.
- A matriz e preliminar. Dados pendentes impedem reduzir risco por suposicao.
- Todo criterio de aborto encerra a tentativa e bloqueia repeticao automatica.

| Risco | Probabilidade | Impacto | Deteccao | Prevencao | Mitigacao | Criterio de aborto |
|---|---|---|---|---|---|---|
| Equipamento errado | Media | Critico | Etiqueta difere da ficha ou foto | Dupla conferencia de modelo, CPU, modulo e slot | Nao conectar; corrigir ficha e aprovacao | Qualquer divergencia de identidade |
| OMNI-PLC2 tratado como NEON5-1S sem prova | Alta | Critico | Frontal e HIstudio exibem identidades diferentes | Exigir documento que relacione modelo, serie e part number | Manter Gate C bloqueado | Relacao documental ausente ou divergente |
| IP errado | Media | Alto | IP nao coincide com ficha aprovada | Copia controlada, revisao por duas pessoas e rede isolada | Cancelar e restaurar rede do PC | Resposta de host inesperado ou divergencia de IP |
| Porta errada | Media | Alto | Porta difere da ficha/mapa | Valor fechado na configuracao aprovada | Cancelar sem testar portas alternativas | Tentativa em porta nao aprovada |
| Perfil RTU/TCP errado | Media | Alto | Seletor diverge da ficha ou exige campos do outro perfil | Selecao explicita, validacao local e nenhuma conexao automatica | Cancelar; nao alternar perfil durante a tentativa | Perfil selecionado diferente do aprovado |
| Camada/interface fisica errada | Media | Critico | RS232/RS485 ou cabo diverge da ficha | Validar camada, cabo e interface separadamente; selecao nao altera hardware | Nao conectar; corrigir identificacao fisica | Qualquer divergencia de camada ou interface |
| Bornes RS-485 confundidos com cabo atual | Media | Alto | Cabo atual esta no DB9 Serial, mas D+/D- existem no frontal | Registrar conector e camada separadamente | Nao mover cabo nem chave; revisar ficha | Tentativa de usar D+/D- no perfil RS-232 aprovado |
| Protocolo errado | Media | Alto | Resposta invalida ou perfil divergente | Confirmar manual/programa e bloquear protocolos nao aprovados | Retirar cabo e revisar documentacao | Primeiro frame/resposta incompativel |
| Unit ID errado | Media | Alto | Endereco difere da ficha ou resposta vem de outra unidade | Unit ID fechado; descoberta OFF e allow-list explicita | Cancelar; nao tentar IDs adjacentes manualmente | Resposta de unidade nao aprovada |
| Broadcast ou endereco reservado | Baixa | Critico | Configuracao contem 0 ou 248..255 sem aprovacao | 0 proibido; automatico limitado a 1..247; 255 nunca sondado | Bloquear preflight e registrar incidente | Qualquer tentativa em 0 ou endereco reservado nao aprovado |
| Faixa de descoberta excessiva | Media | Alto | Range excede allow-list ou mais de uma tentativa aparece | Default 10..10, maximo uma tentativa por endereco e cancelamento | Cancelar e revisar lista/intervalo | Endereco fora da allow-list ou segunda tentativa automatica |
| Identificacao por evidencia parcial | Media | Alto | Apenas ID ou CRC coincide | Exigir F12/30012 = 31134 e F13/30013 = 23248 em conjunto | Classificar como nao identificado e encerrar | Declaracao de identidade com apenas um valor |
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
| Declaracao tratada como evidencia | Alta | Critico | Campo marcado OK sem foto, medicao, caminho ou hash | Separar `DECLARADO PELO RESPONSÁVEL` de `CONFIRMADO` no preflight | Rebaixar status e solicitar evidencia | Qualquer gate liberado somente por declaracao verbal |
| Indicacao 1030 VDC interpretada como medicao/faixa | Media | Critico | Valor foi normalizado ou usado sem medicao | Preservar texto literal e exigir tensao medida/instrumento | Nao energizar ou conectar; revisar etiqueta/documentacao | Tensao aplicada com base em interpretacao nao comprovada |
| Rede do PC nao restaurada | Media | Moderado | Configuracao final difere da captura inicial | Captura antes/depois e plano de rollback | Restaurar pelos valores registrados e revisar | Nao conseguir reproduzir configuracao original |

## Riscos adicionais do estado atual

| Risco | Probabilidade | Impacto | Deteccao | Prevencao | Mitigacao | Criterio de aborto |
|---|---|---|---|---|---|---|
| Fontes conflitantes para modelo/CPU/modulo | Alta | Critico | HIstudio mostra NEON5-1S/CPU450/HIO115 e frontal mostra OMNI-PLC2 | Exigir foto e comprovacao documental da relacao | Manter configuracao bloqueada | Identidade ainda conflitante no preflight |
| Default de software tratado como dado real | Alta | Alto | Valor existe apenas no codigo/catalogo pendente | Marcar defaults como nao confirmados | Limpar campo e solicitar evidencia | Parametro sem fonte da unidade real |
| Mapa legado contendo comandos | Alta | Critico | `%MW` inclui habilitacao, reset e watchdog | Proibir copia automatica para allow-list | Criar allow-list nova e revisada | Inclusao de comando, coil ou endereco sem finalidade read-only |
| Referencia 30012/30013 convertida por suposicao | Alta | Critico | Endereco de dados foi preenchido sem fonte do mapa | Manter `protocolDataAddress` vazio ate revisao do mapa | Remover traducao inferida e bloquear descoberta | Qualquer offset deduzido sem evidencia aprovada |
| Tela offline tratada como leitura fisica | Alta | Alto | Captura indica equipamento remoto offline/sem base de hardware | Separar capacidade declarada de estado atual | Descartar valores/estados e solicitar evidencia valida | Uso de I/O, analogico, contador ou PWM como estado real |

## Aceitacao de risco

Nenhum risco critico pode ser aceito apenas por esta matriz. Reducao de risco
exige evidencia, controle implementado, reexecucao do preflight e aprovacao do
responsavel. A conexao continua bloqueada enquanto houver status `PENDENTE`,
`NÃO DISPONÍVEL` ou `CONFLITANTE`.
