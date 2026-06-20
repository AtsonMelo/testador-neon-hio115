# Layout 3 — roadmap operacional consolidado da Fase 3

## Objetivo e estado atual

Este documento consolida a evolução controlada do Host Layout 3 entre as Fases
3.1 e 3.6. O estado atual continua estritamente local e read-only: a leitura
real ainda não está ativa, nenhuma conexão física é aberta e os contadores de
leituras reais, escritas reais e comandos físicos permanecem em zero.

A base operacional é `ui/issue-24-liga-io-industrial-host`. A Fase 3.6 está na
branch `ui/layout-3-read-bridge-activation-gate`, em revisão no PR Draft #48, e
não deve ser tratada como parte já integrada da base.

## Resumo das Fases 3.1 a 3.6

| Fase | Entrega | Estado operacional |
| --- | --- | --- |
| 3.1 | Host Layout 3 opt-in, separado do preview e do `MainForm`, com estado local e guarda read-only. | Desconectado, saídas bloqueadas e zero comandos físicos. |
| 3.2 | Seleção de família, modelo, módulo, comunicação e perfil de teste a partir do catálogo local. | Seleção apenas informativa; não inicia transporte ou operação física. |
| 3.3 | Snapshot visual do estado de comunicação derivado da seleção local. | Estado read-only, sem conexão, polling ou leitura real. |
| 3.4 | Validador não visual das invariantes do host read-only e da guarda de comandos. | Seis cenários locais; tentativas reais e comandos físicos iguais a zero. |
| 3.5 | Contrato do bridge de leitura, implementação disabled/no-op e painel compacto. | Bridge desligado; conexão, leituras, escritas e comandos físicos iguais a zero. |
| 3.6 | Gate de ativação e requisitos futuros, com implementação sempre bloqueada. | Em PR Draft #48; ativação não liberada e todos os requisitos pendentes. |

## Checkpoints existentes

Os checkpoints observados no histórico da base são:

- `layout-3-preview-ok-20260619` — preview isolado aprovado;
- `layout-3-fase-3-planejamento-ok-20260619` — planejamento da integração;
- commit `64fd367` — Host Layout 3 read-only da Fase 3.1;
- `layout-3-fase-3-2-profile-selection-ok-20260619` — seleção local da Fase 3.2;
- commit `8d76d41` — estado de comunicação read-only da Fase 3.3;
- `layout-3-fase-3-4-readonly-safety-ok-20260620` — validador de segurança da Fase 3.4;
- `layout-3-fase-3-5-read-bridge-disabled-ok-20260620` — bridge disabled da Fase 3.5;
- commit `af14cae`, branch `ui/layout-3-read-bridge-activation-gate` — Fase 3.6 em PR Draft, ainda fora da base.

Tags registram checkpoints técnicos; não substituem revisão, aprovação ou
autorização de bancada.

## Flags e validadores

Flags de UI disponíveis na base:

- `--layout-3-host-readonly`;
- `--preview-layout-3`;
- `--preview-layout-3-light`;
- `--preview-layout-3-auto`.

Validadores disponíveis na base:

- `--validate-hardware-catalog`;
- `--validate-hardware-profile-selection`;
- `--validate-hardware-test-report`;
- `--validate-layout-3-host-readonly-safety`;
- `--validate-layout-3-read-bridge-disabled`.

O validador `--validate-layout-3-read-bridge-activation-gate` pertence à Fase
3.6 e, enquanto o PR Draft #48 não for integrado, existe apenas na branch dessa
milestone. Todos esses validadores são locais e não visuais.

## Controles de segurança vigentes

- entrada do Host Layout 3 somente por flag opt-in; o fluxo padrão permanece no
  `MainForm`;
- estado produzido exclusivamente pelo catálogo e pela seleção em memória;
- `Layout3ReadOnlyCommandGuard` nega toda intenção e não possui executor físico;
- bridge da Fase 3.5 implementado somente como disabled/no-op;
- gate da Fase 3.6 fail-closed, sempre bloqueado no escopo atual;
- ausência de transporte, polling real, worker de comunicação e acesso a
  registradores no Host Layout 3;
- validadores falham se detectarem conexão, tentativa real, leitura, escrita ou
  comando físico diferente de zero;
- QA local preserva a busca de arquivos proibidos e termos operacionais fortes;
- `MainForm.cs`, artefatos HIstudio e mapas/registradores permanecem fora do
  escopo destas milestones.

## Checklist para uma futura bancada

Este checklist é preparatório e não autoriza conexão ou leitura:

- [ ] milestone futura específica aberta e aprovada explicitamente;
- [ ] escopo limitado a leitura e contrato sem qualquer método de escrita;
- [ ] hardware, firmware, alimentação, aterramento e topologia identificados;
- [ ] perfil e protocolo confirmados em documentação oficial aplicável;
- [ ] dados/endereço a observar revisados por duas pessoas e vinculados ao
      perfil correto, somente dentro da milestone futura aprovada;
- [ ] configuração opt-in, padrão desligado e mecanismo de interrupção definidos;
- [ ] timeouts, cancelamento, reconexão limitada e tratamento de estado stale
      documentados;
- [ ] logs identificam origem, perfil, instante e qualidade sem ocultar falhas;
- [ ] testes com fake/simulação e validadores não visuais aprovados;
- [ ] build, diff-check, QA, smoke GUI e auditoria de dependências aprovados;
- [ ] roteiro de rollback testado sem depender de hardware real;
- [ ] bancada isolada, responsável técnico presente e janela de teste aprovada;
- [ ] evidência anterior confirma zero caminhos de escrita e zero comandos físicos.

## Critérios mínimos antes da primeira leitura real

A primeira leitura real só pode ser considerada após todos os itens abaixo:

1. autorização humana explícita para uma nova milestone de leitura real;
2. desenho técnico e análise de riscos revisados, incluindo origem dos dados,
   ciclo de vida, falhas, cancelamento e rollback;
3. implementação read-only isolada atrás de gate padrão-bloqueado e flag opt-in;
4. ausência comprovada de API ou caminho de escrita no novo componente;
5. testes automatizados com transporte fake cobrindo sucesso, timeout,
   desconexão, dado inválido, stale e cancelamento;
6. validação independente de que UI e fluxo padrão continuam seguros quando o
   recurso está desligado;
7. plano de bancada aprovado com limites, responsáveis, evidências e critério
   imediato de interrupção;
8. revisão final do diff confirmando que somente arquivos autorizados mudaram.

Até que esses critérios sejam cumpridos em milestone futura, o gate deve
permanecer bloqueado e a contagem de leituras reais deve continuar em zero.

## Limite operacional explícito

Nada de Serial, TCP, Modbus, PLC, mapas ou registradores deve ser criado,
aberto, consultado ou alterado sem milestone futura específica e aprovação
explícita. Este roadmap não autoriza leitura real, escrita, comando físico nem
qualquer conexão com bancada.
