# Layout 3 - Fase 3 - Integração gradual ao host real

## 1. Estado de partida

A Fase 3 parte da branch `ui/issue-24-liga-io-industrial-host`, no merge
commit `52a8c08`, resultado do PR #38. Esse estado está marcado pela tag de
checkpoint `layout-3-preview-ok-20260619`.

Naquele checkpoint, o Layout 3 estava disponível somente como preview isolado
e opt-in por três launchers, cobrindo paletas dark, clara e automática. Esses
launchers foram superseded e removidos pelo Project Cleanup 1; o host read-only
e o shell industrial oficial preservam a cobertura funcional atual.

Já foram validados o isolamento do modo padrão, a composição em zonas, os
temas dark e claro e a abertura explícita pelas três flags. O modo sem flag
continua abrindo o `MainForm`. O preview não cria serviços PLC/Modbus, seus
botões de saída são inertes e a sinalização `PREVIEW ONLY` deixa explícito que
os estados exibidos não representam a bancada real.

Esse checkpoint é o fallback visual e técnico da integração. A Fase 3 não
autoriza promover o preview diretamente ao modo padrão.

## 2. Objetivo da Fase 3

O objetivo é sair do preview isolado para um host real progressivo, mantendo a
integração opt-in e dividida em entregas pequenas. O `MainForm` atual deve
permanecer funcional e disponível como fallback durante toda a fase.

O primeiro incremento será estritamente read-only: ele reutilizará o visual
aprovado, receberá somente estado local, simulado ou fornecido por adaptadores
de leitura e não enviará comandos físicos. Integrações posteriores poderão
expor seleção de perfil, estado de comunicação e entradas digitais, sempre sem
escrita e sem assumir o ciclo de vida dos serviços existentes.

## 3. Princípios de segurança

1. Todo comando real permanece bloqueado até uma fase explicitamente aprovada
   para escrita controlada.
2. Nas Fases 3.1 a 3.5, `comandos físicos executados = 0` é uma invariável, não
   apenas uma expectativa de teste.
3. Qualquer escrita Modbus/PLC exige PR dedicado, revisão de segurança e
   autorização de bancada. Ela não pode entrar como detalhe de uma entrega de
   UI.
4. Nenhum botão visual pode acionar PLC sem uma camada de guarda explícita,
   configurada para falhar fechada (`deny by default`).
5. UI, estado apresentado, intenção operacional e execução física devem ficar
   em camadas e contratos diferentes.
6. O host do Layout 3 não conhece endereços, registradores, frames Modbus ou
   detalhes de porta serial.
7. Um adaptador de leitura não abre, fecha ou reinicia conexão e não cria uma
   instância concorrente de serviço. Ele apenas observa uma fonte já fornecida
   pela composição da aplicação.
8. Estado indisponível, desatualizado ou inválido deve ser exibido como tal;
   nunca deve ser convertido em um valor visual aparentemente normal.
9. O modo read-only deve ser visível na interface e também imposto na camada
   de guarda. Um rótulo sozinho não é controle de segurança.
10. Toda intenção, permissão ou negação deve produzir log/auditoria sem dados
    sensíveis e sem alterar o equipamento.

## 4. Reaproveitamento do preview

Podem ser reaproveitados, após separação entre apresentação e dados:

- a composição geral do antigo preview, como referência histórica;
- `Layout3IoPanelControl`, como apresentação de DO/DI;
- o antigo painel de perfil, como referência histórica para preparação;
- `Layout3PreviewTheme` e as paletas dark, light e auto;
- a organização em barra superior, três zonas e terminal inferior;
- o terminal/log visual;
- a sinalização `PREVIEW ONLY`, enquanto o preview continuar existindo;
- os painéis de perfil de hardware e preparação de teste;
- as regras de responsividade e rolagem já validadas.

O reaproveitamento deve favorecer extração ou composição de controles. Não se
deve duplicar a árvore visual inteira para criar o host real.

## 5. O que não deve ser reaproveitado diretamente

Não devem migrar para o host real como fonte de verdade:

- textos de mock apresentados como se fossem estado real;
- estados fixos de entrada, saída, comunicação ou diagnóstico;
- botões demonstrativos tratados como comando real;
- qualquer aparência de comando físico sem confirmação, estado confiável e
  decisão explícita da guarda;
- handlers do preview usados como atalho para serviços PLC;
- acoplamento direto entre controles WinForms e serviços PLC/Modbus;
- timers ou polling criados dentro da UI;
- lógica duplicada do `MainForm` ou cópias de mapas de registradores;
- mensagens de terminal demonstrativas misturadas ao log operacional.

No host read-only, controles que representam ação física devem permanecer
desabilitados ou claramente marcados como indisponíveis. Na Fase 3.5 eles
podem gerar somente uma intenção auditável, nunca uma chamada de escrita.

## 6. Arquitetura proposta

```text
Layout3 UI
    | eventos visuais / renderização
    v
Layout3 ViewModel/State
    | estado normalizado e intenções sem efeito colateral
    v
Layout3 Host Adapter -------- Hardware Profile Adapter
    | leitura e tradução                 | seleção/preparação
    v                                    v
ILayout3PlcBridge (somente leitura nas Fases 3.1-3.5)
    |
    v
PLC/Modbus Services existentes

Layout3CommandIntent --> Safety/Command Guard --> [sempre DENY em read-only]
                              |
                              v
                        Logging/Audit
```

### 6.1. Layout3 UI

Responsável por composição, foco, acessibilidade, tema, renderização e captura
de eventos do operador. Recebe snapshots de estado e publica eventos/intenção.
Não consulta PLC, não resolve perfil, não decide permissão e não executa
efeitos físicos.

### 6.2. Layout3 ViewModel/State

Mantém o estado normalizado que a UI consegue renderizar: modo, atualidade do
dado, perfil selecionado, estado de comunicação, DI, DO exibidas e mensagens.
Não contém controles WinForms nem referências a serviços de comunicação.
Snapshots imutáveis ou atualizações atômicas são preferíveis para evitar que a
tela combine dados de instantes diferentes.

### 6.3. Layout3 Host Adapter

É a fronteira entre o host e as fontes da aplicação. Traduz eventos e estados
existentes para o `Layout3ViewModel`, controla assinaturas e descarte e impede
que a UI dependa de implementações concretas. Não é proprietário do ciclo de
conexão e não instancia uma segunda pilha PLC/serial.

### 6.4. Hardware Profile Adapter

Converte a seleção e a resolução já existentes em estado somente leitura para
o Layout 3. Reutiliza catálogo, resolução e formatação de preparação; não
duplica regras, não altera parâmetros de comunicação e não aciona hardware.

### 6.5. PLC/Modbus Services existentes

Continuam como única fonte do comportamento operacional. Não devem ser
alterados nas Fases 3.1 a 3.5. Quando alguma instância existente for exposta ao
host, isso ocorrerá por contrato estreito e injetado, inicialmente apenas para
consulta/observação.

### 6.6. Safety/Command Guard

Avalia cada `Layout3CommandIntent` fora da UI. Em `Layout3ReadOnlyMode`, toda
intenção que possa produzir leitura ativa, conexão, desconexão ou escrita é
negada. Ausência de guarda, exceção, estado desconhecido ou dependência
indisponível também resulta em negação.

Uma futura habilitação de escrita não deve ser implementada adicionando um
`if` na UI. Ela exigirá contrato, política, pré-condições, confirmação,
auditoria e PR próprios.

### 6.7. Logging/Audit

Registra mudança de modo, atualização de estado, intenção gerada, decisão da
guarda e erro de adaptação. O log deve distinguir `PREVIEW`, `READ-ONLY` e uma
eventual operação real. Registrar uma intenção não significa executá-la.

## 7. Contratos sugeridos

Os nomes abaixo são propostas de responsabilidade; não fazem parte desta
entrega de documentação.

| Contrato | Responsabilidade sugerida |
| --- | --- |
| `Layout3HostForm` | Janela opt-in do host gradual. Compõe o controle, aplica tema e encerra assinaturas ao fechar. Não cria serviços PLC. |
| `Layout3HostControl` | Superfície visual reutilizável que recebe estado e publica eventos/intenção, sem conhecer infraestrutura. |
| `Layout3ViewModel` | Estado apresentável do host, incluindo modo, perfil, comunicação, I/O, mensagens e validade temporal. |
| `Layout3HardwareState` | Snapshot do perfil, modelo, módulo, comunicação configurada, evidências e preparação de teste. |
| `Layout3IoState` | Snapshot de DI/DO com valor, disponibilidade, origem e instante da atualização; não oferece métodos de escrita. |
| `Layout3CommandIntent` | Objeto de dados que descreve ação pretendida, origem, alvo lógico, instante e correlação, sem executá-la. |
| `ILayout3CommandGuard` | Retorna decisão explícita de permitir/negar e motivo. A implementação read-only sempre nega efeitos físicos. |
| `ILayout3PlcBridge` | Fachada estreita sobre estado já existente. Até a Fase 3.5 expõe apenas observação/leitura e não contém métodos de escrita. |
| `Layout3ReadOnlyMode` | Política/capacidade explícita que força UI não operacional e guarda fechada, independentemente do tema ou do rótulo visual. |

Contratos de estado devem carregar origem e atualidade do dado. A UI precisa
distinguir pelo menos `Unavailable`, `Stale`, `Disconnected` e `Available`,
sem inferir um estado a partir de cor ou texto de mock.

## 8. Fases sugeridas e critérios de aceite

As listas de arquivos permitidos abaixo definem o teto de cada PR. Arquivos
gerados (`bin/`, `obj/`) nunca entram no commit. Qualquer necessidade fora do
teto exige parar, revisar o plano e separar outra entrega.

### 8.1. Fase 3.1 - host Layout 3 read-only

Criar o host e o estado mínimo em modo read-only. Reaproveitar o visual, usar
somente estado local, simulado ou controlado e manter todas as ações físicas
indisponíveis.

- **Arquivos permitidos:** novos arquivos e ajustes estritamente visuais em
  `app/TestadorCLPHI.App/Ui/Industrial/Layout3/`; `Program.cs` somente para
  registrar uma nova flag opt-in de read-only, se essa entrada for aprovada no
  próprio PR; documentação e testes offline específicos do host.
- **Arquivos bloqueados:** `MainForm.cs`, `Plc/**`, mapas, serviços de UI do
  `MainForm`, `Data/Hardware/**` e artefatos HIstudio.
- **Validações obrigatórias:** validação mínima da seção 10; smoke offline das
  três flags de preview e da nova flag, se criada; inspeção visual dark/light;
  busca/diff confirmando ausência de referências a serviço PLC/Modbus no novo
  host; fechamento da janela sem efeitos.
- **Riscos:** duplicar a composição do preview; modo read-only existir apenas
  como rótulo; alterar o fluxo padrão via `Program.cs`.
- **Critério de sucesso:** host abre somente por opt-in, mostra `READ-ONLY`, o
  modo padrão continua no `MainForm` e os comandos físicos executados são zero.
- **Critério de rollback:** remover a entrada opt-in e os arquivos novos do
  host, preservando intactas as três flags e o checkpoint do preview.

### 8.2. Fase 3.2 - perfil real e preparação

Ligar a seleção de perfil existente ao estado do Layout 3, exibir o perfil
resolvido e a preparação do teste, sem alterar comunicação.

- **Arquivos permitidos:** `Ui/Industrial/Layout3/**`; adaptador novo e
  específico em `Ui/Hardware/**`; testes offline e documentação. Alterações em
  controles de hardware existentes só podem ser extensões read-only e
  compatíveis com o `MainForm`.
- **Arquivos bloqueados:** `MainForm.cs`, `Program.cs`, `Plc/**`, catálogo JSON,
  mapas e artefatos HIstudio.
- **Validações obrigatórias:** seção 10; seleção de famílias/modelos disponíveis
  e estados sem catálogo; relatório/checklist inalterado; smoke dark/light;
  confirmação de que trocar perfil não muda COM, Slave ID nem PLC.
- **Riscos:** duplicar o resolvedor/formatador; confundir perfil selecionado com
  equipamento detectado; seleção alterar parâmetros operacionais.
- **Critério de sucesso:** perfil e preparação vêm dos componentes existentes,
  têm origem/estado explícitos e trocar seleção produz zero comando físico.
- **Critério de rollback:** retirar o adaptador e retornar o host ao estado
  local da Fase 3.1, sem modificar dados ou lógica compartilhada.

### 8.3. Fase 3.3 - estado de comunicação somente leitura

Expor conectado/desconectado e erros conhecidos por meio de uma instância já
existente e injetada. O host não conecta, desconecta, detecta nem faz polling.

- **Arquivos permitidos:** `Ui/Industrial/Layout3/**`; contratos/adaptadores
  novos de observação em pasta própria; composição opt-in mínima em
  `Program.cs` somente se indispensável; testes com fake existente.
- **Arquivos bloqueados:** implementações em `Plc/**`, `MainForm.cs`, mapas,
  Serial, Modbus, comandos e artefatos HIstudio.
- **Validações obrigatórias:** seção 10; testes/smoke com conectado,
  desconectado, erro e fonte indisponível usando fake; descarte das assinaturas;
  evidência de zero escrita e zero nova conexão.
- **Riscos:** o adaptador assumir ciclo de vida; criar serviço concorrente;
  estado antigo parecer atual; dependência circular com o host.
- **Critério de sucesso:** o indicador reflete apenas o estado recebido, marca
  indisponibilidade/defasagem e não altera a conexão ao abrir ou fechar.
- **Critério de rollback:** desconectar somente a assinatura do adaptador e
  voltar ao estado `Unavailable` da Fase 3.2; nenhuma conexão física é tocada.

### 8.4. Fase 3.4 - entradas digitais somente leitura

Mapear DI00-DI07 para `Layout3IoState` por uma ponte de leitura. Saídas
permanecem sem acionamento e devem ser exibidas como indisponíveis ou apenas
observadas quando houver fonte confiável.

- **Arquivos permitidos:** `Ui/Industrial/Layout3/**`; contratos/adaptadores
  read-only novos; testes com fake. Reutilizar definições existentes sem
  editá-las.
- **Arquivos bloqueados:** mapas de registradores, `Plc/**`, `MainForm.cs`,
  escrita/polling novo, comandos físicos e artefatos HIstudio.
- **Validações obrigatórias:** seção 10; matriz offline para 8 DI, fonte
  indisponível, dado defasado e transições; smoke responsivo em 1280x720 e
  1366x768; confirmação de que clicar em DO não produz efeito.
- **Riscos:** índice DI incorreto; inversão de estado; leitura ativa escondida
  na UI; DO parecer habilitada; atualização fora da thread de UI.
- **Critério de sucesso:** as oito DI representam exatamente o snapshot fake
  recebido, com origem/tempo visíveis ou rastreáveis, e há zero acionamento de
  saída.
- **Critério de rollback:** retirar a ponte de DI e renderizar todas as entradas
  como `Unavailable`, mantendo as demais fases funcionais.

### 8.5. Fase 3.5 - intenção de comando sem execução

Preparar o caminho de interação: botões geram `Layout3CommandIntent`, a guarda
read-only nega a intenção e o resultado é registrado. Não existe chamada de
escrita nessa fase.

- **Arquivos permitidos:** `Ui/Industrial/Layout3/**`; contratos e implementação
  read-only de guarda/auditoria em pasta própria; testes offline.
- **Arquivos bloqueados:** `Plc/**`, `MainForm.cs`, Serial, Modbus, mapas,
  handlers de comando existentes e artefatos HIstudio.
- **Validações obrigatórias:** seção 10; testes de negação para todos os botões,
  dependência ausente, exceção e estado desconhecido; uma intenção gera um log
  correlacionado e zero chamada ao fake de escrita.
- **Riscos:** intenção chamar serviço diretamente; guarda permissiva por
  padrão; clique duplo; log sugerir execução que não ocorreu.
- **Critério de sucesso:** toda interação prevista produz no máximo intenção e
  decisão `DENY`, com motivo, e os comandos físicos executados permanecem zero.
- **Critério de rollback:** desabilitar/remover a publicação de intenções e
  retornar os botões ao estado indisponível da Fase 3.4.

### 8.6. Fase 3.6 - discussão de escrita controlada

Somente depois da aprovação das fases anteriores, abrir PR separado para
discutir desenho, ameaças, pré-condições e validação em bancada. A aprovação
desse planejamento não autoriza implementação nem execução de escrita.

- **Arquivos permitidos:** inicialmente apenas documentação de arquitetura,
  segurança, matriz comando/pré-condição e roteiro de bancada. Arquivos de
  código só podem ser definidos por um plano e autorização posteriores.
- **Arquivos bloqueados:** qualquer alteração ou execução física enquanto o PR
  ainda for de discussão.
- **Validações obrigatórias:** seção 10 para qualquer mudança futura, revisão
  específica de segurança, testes com fake, plano de bancada e autorização
  humana explícita antes de hardware real.
- **Riscos:** reutilizar handler sem pré-condições; bypass da guarda; endereço
  ou perfil incorreto; comando repetido; falha sem auditoria; confundir aprovação
  de UI com autorização operacional.
- **Critério de sucesso:** decisão documentada e revisada sobre se e como a
  escrita poderá existir, sem comando físico durante o PR de planejamento.
- **Critério de rollback:** manter `Layout3ReadOnlyMode` obrigatório e encerrar
  a proposta sem alterar os serviços existentes.

## 9. Estratégia Git/GitHub

- Usar uma branch por fase, sempre criada a partir do último checkpoint
  aprovado.
- Manter PRs pequenos, com um único incremento arquitetural e lista explícita
  dos arquivos permitidos.
- Executar build e validadores em toda fase, mesmo quando a alteração parecer
  somente visual.
- Fazer smoke visual obrigatório da flag afetada, em modo offline antes de
  qualquer validação com hardware.
- Fazer merge somente após revisão de arquitetura, segurança e diff.
- Criar tags em checkpoints relevantes, especialmente após a aprovação do host
  read-only e antes de qualquer proposta de escrita.
- Não misturar refatoração ampla, mudança visual e integração operacional no
  mesmo PR.
- Manter o PR como Draft enquanto critérios, evidências ou smoke estiverem
  incompletos.

Sugestões de branches:

- `ui/layout-3-fase-3-1-host-read-only`;
- `ui/layout-3-fase-3-2-perfil-real`;
- `ui/layout-3-fase-3-3-status-comunicacao`;
- `ui/layout-3-fase-3-4-di-read-only`;
- `ui/layout-3-fase-3-5-command-intent`;
- `docs/layout-3-fase-3-6-escrita-controlada`.

## 10. Validação mínima obrigatória para qualquer PR da Fase 3

Executar, nesta ordem:

```powershell
git status --short --branch
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-catalog
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-profile-selection
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-test-report
dotnet build .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj
git diff --check
```

Além disso, cada PR deve ter:

- smoke da flag afetada, sem conexão a hardware nas Fases 3.1 a 3.5;
- evidência visual compatível com o escopo e, quando houver UI, dark/light e
  resoluções menores;
- `git diff --name-only` revisado antes do commit;
- confirmação de que não existem arquivos gerados ou fora de escopo;
- status limpo após commit/push.

Falha em validação bloqueia o merge. Uma justificativa escrita não substitui
build, validador ou critério de segurança.

## 11. Riscos principais

| Risco | Controle proposto |
| --- | --- |
| Acoplamento acidental com PLC | Dependências por contratos estreitos, adaptadores passivos e proibição de referências PLC na UI. |
| Botão visual virar comando real cedo demais | `Layout3CommandIntent` sem executor e `ILayout3CommandGuard` fail-closed em read-only. |
| Duplicação de lógica do `MainForm` | Reutilizar estado/serviços existentes por adaptadores; manter o `MainForm` como fallback e referência funcional. |
| UI bonita, mas sem estado confiável | Snapshot com origem, atualidade e estados `Unavailable`/`Stale`; proibir mock como fonte real. |
| Regressão visual em resoluções menores | Smoke em 1280x720, 1366x768, DPI 125%, redimensionamento, foco e rolagem. |
| Confusão entre preview, read-only e modo real | Selo de modo inequívoco, log com origem e flags opt-in distintas; o modo padrão não muda. |
| Serviço ou conexão duplicados | Host Adapter não possui ciclo de vida e recebe uma fonte já composta; revisão do grafo de dependências. |
| Estado atualizado fora da thread de UI | Normalizar eventos no adaptador e aplicar snapshots de forma segura/atômica no WinForms. |
| Escrita entrar disfarçada como leitura | Contrato `ILayout3PlcBridge` sem métodos de escrita até a Fase 3.6 e inspeção obrigatória do diff. |

## 12. Próxima entrega recomendada

**Fase 3.1 - criar host Layout 3 read-only, sem comando físico.**

Essa entrega deve criar `Layout3HostForm`, `Layout3HostControl`, o estado mínimo
e `Layout3ReadOnlyMode` dentro da área do Layout 3. O acesso deve permanecer
opt-in, o `MainForm` deve continuar como modo padrão e fallback, e os botões com
significado físico devem ficar indisponíveis. Nenhum serviço real, leitura,
escrita, conexão ou comando deve ser criado nessa entrega.

## 13. Checklist final desta entrega de planejamento

- [x] `MainForm.cs` não alterado.
- [x] `Program.cs` não alterado.
- [x] Modbus e Serial não alterados.
- [x] Mapas de registradores não alterados.
- [x] Serviços PLC não alterados.
- [x] Arquivos `.dpk`, `.dmf`, `.hst`, `.prj` e
  `TESTADOR_NEON_HIO115_1_1` não alterados.
- [x] Comandos físicos executados = 0.
- [x] Documento de planejamento criado.
- [ ] Build OK.
- [ ] Validadores OK.
- [ ] `git diff --check` OK.
- [ ] PR Draft aberto contra `ui/issue-24-liga-io-industrial-host`.

Os itens de validação e publicação devem ser marcados no resumo do PR após a
execução; o documento não deve ser editado apenas para registrar resultados
transitórios da máquina local.
