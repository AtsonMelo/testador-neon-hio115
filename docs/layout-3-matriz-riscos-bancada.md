# Layout 3 - Matriz de riscos de bancada RTU

Escala: probabilidade `Baixa/Media/Alta`; impacto `Moderado/Alto/Critico`.

| Risco | Prob. | Impacto | Deteccao | Prevencao | Mitigacao | Criterio de aborto |
|---|---|---|---|---|---|---|
| Equipamento errado | Media | Critico | etiqueta + assinatura | reconciliar OMNI-PLC2/NEON5-1S | desconectar e registrar | identidade nao comprovada |
| Identidade frontal conflitante | Alta | Alto | comparar documento/HIstudio | exigir evidencia do fabricante | manter Gate C bloqueado | relacao nao confirmada |
| Porta COM errada | Media | Alto | inventario do PC/cabo | selecao explicita | cancelar sem reconectar | porta divergente |
| RS-232/RS-485 errado | Media | Critico | inspecao de cabo/interface | perfil e cabo aprovados | retirar cabo | camada fisica ambigua |
| Protocolo errado | Media | Alto | configuracao do canal | confirmar Modbus RTU | parar descoberta | protocolo nao confirmado |
| Endereco errado | Alta | Alto | assinatura combinada | iniciar em 1 e validar | classificar nao reconhecido | ID/CRC divergem |
| Faixa fora de 1..247 | Baixa | Alto | preflight | limites fechados | rejeicao local | 0 ou 248..255 |
| Mapa/offset incompatível | Media | Critico | referencia e PDU revisados | nao inventar base/offset | bloquear Gate D | conversao ambigua |
| Firmware diferente | Media | Alto | F10/F11 + assinatura | confirmar mapa e versao | bloquear I/O | firmware diverge |
| Programa ID/CRC diferente | Media | Critico | F12/F13 | exigir ambos | registrar resposta nao reconhecida | qualquer divergencia |
| F21 critico | Media | Critico | decodificar bits | diagnostico antes do I/O | abortar sessao | qualquer bit critico |
| Leitura em area sensivel | Baixa | Alto | auditoria da allow-list | contrato read-only fechado | cancelar | referencia nao permitida |
| Escrita durante identificacao | Baixa | Critico | log/contador | servico sem API de saida | isolamento fisico | escrita > 0 |
| Escrita durante entradas | Baixa | Critico | log/contador | modo sem caminho de saida | isolamento fisico | escrita > 0 |
| Escrita arbitraria | Baixa | Critico | self-test e revisao API | enum DO00..DO03 | rejeitar localmente | endereco arbitrario |
| PWM ou reservado | Baixa | Critico | allow-list negativa | bloqueio explicito | rejeitar e registrar | 31137/40/43/44/45 |
| Duas saidas simultaneas | Baixa | Critico | estado de sessao | exclusao de canal ativo | desligar canal conhecido | segundo canal solicitado |
| Saida excede duracao | Media | Critico | temporizador/log | limite obrigatorio | OFF + isolamento | limite atingido |
| Saida nao desliga | Baixa | Critico | retorno + verificacao fisica | modo momentaneo | emergencia/desconexao | OFF nao confirmado |
| Polling excessivo | Media | Alto | contador/timestamps | single-shot, sem loop | cancelar | repeticao inesperada |
| Reconexao automatica | Baixa | Alto | log de tentativas | default OFF | cancelar processo | tentativa nao solicitada |
| Timeout/resposta invalida | Media | Alto | timeout e parser | limite curto | abortar sem avancar | primeira falha |
| Aplicacao travada | Baixa | Critico | UI/log sem progresso | cancelamento independente | desconexao fisica | cancelamento nao responde |
| Perda de comunicacao | Media | Critico | erro/timeout | cabo e energia controlados | nao assumir saida OFF | estado de saida desconhecido |
| Maquina ativa | Baixa | Critico | checklist/inspecao | impedir operacao | emergencia | movimento/energia ativa |
| Saida nao isolada | Media | Critico | medicao/inspecao | isolamento comprovado | desconectar energia/cabo | evidencia ausente |
| Tensao incorreta | Media | Critico | medicao independente | medir antes do cabo | desligar fonte | valor nao aprovado |
| Aterramento inadequado | Media | Critico | evidencia/medicao | checklist eletrico | interromper | aterramento nao comprovado |
| Responsavel ausente | Baixa | Critico | presenca registrada | agenda e gate | nao iniciar | responsavel sai |
| Backup inexistente | Media | Alto | caminho + SHA-256 | verificar antes | nao testar | arquivo/hash ausente |
| Configuracao do PC nao restaurada | Media | Moderado | evidencia antes/depois | registrar baseline | restaurar manualmente | baseline ausente |
| Log incompleto | Media | Alto | contadores vs eventos | log obrigatorio | preservar incidente | operacao sem evento |

Risco residual de maior severidade: perda de comunicacao com uma saida fisica
possivelmente ativa. Software nao substitui isolamento, emergencia e desconexao.
