# Layout 3 - conceito hibrido

## 1. Objetivo

O Layout 3 existe para combinar a confiabilidade operacional do Layout 1 com a
experiencia visual e ergonomica do Layout 2. Ele deve reorganizar a interface
sem substituir fluxos validados, sem criar uma segunda implementacao de regras
de comunicacao e sem voltar a concentrar layout e comportamento no
`MainForm`.

O resultado esperado e uma superficie industrial unica, legivel para o
operador, que preserve conexao, diagnostico, comandos, I/O manual, terminal e
preparacao do teste. A primeira entrega executavel deve ser uma previa isolada
e opt-in. A interface de producao somente podera ser substituida depois da
aprovacao visual, funcional e de seguranca dessa previa.

## 2. O que preservar do Layout 1

Os seguintes recursos formam a base operacional estavel e nao podem ser
perdidos nem reimplementados apenas para atender ao novo arranjo visual:

- selecao e atualizacao de porta COM;
- selecao de baud rate, conjunto de baud rates para busca e Slave ID;
- resumo dos parametros de conexao e validacao dos valores informados;
- deteccao automatica do CLP com progresso e tratamento de falha;
- conexao, validacao da resposta Modbus, desconexao e estado de comunicacao;
- fluxo de diagnostico, incluindo leitura ja existente e mensagens de erro;
- comandos ja validados de habilitacao de teste, reset de saidas e parada;
- painel de I/O manual, com habilitacao segura, acionamento de DO e leitura de
  DI pelos servicos existentes;
- terminal/log para rastreabilidade das acoes e respostas;
- selecao informativa de familia, modelo, modulo, comunicacao e perfil de
  teste;
- status de evidencia do catalogo, pendencias de validacao manual e
  necessidades de bancada;
- relatorio/checklist de preparacao de teste e copia do resumo;
- avisos sobre conflito de porta COM entre Testador, HIstudio, XCTU e outros
  softwares;
- separacao atual entre servicos de comunicacao, comandos, I/O e controles de
  interface.

Preservar significa continuar usando os mesmos servicos, validacoes, estados e
mapas que sustentam o Layout 1. Uma mudanca de posicao, tema ou tamanho de um
controle nao autoriza alterar seu significado operacional.

## 3. O que preservar do Layout 2

O Layout 3 deve reaproveitar do Layout 2:

- tema escuro industrial, contraste alto e hierarquia visual consistente;
- controles maiores e alvos de clique adequados ao operador;
- cartoes e bordas para separar comunicacao, comandos, I/O e status;
- estado de conexao visivel sem depender da leitura do terminal;
- painel industrial de I/O manual com DO e DI facilmente distinguiveis;
- cores de estado usadas com texto ou icone, sem depender apenas da cor;
- barra de comandos de acesso rapido para acoes ja existentes;
- espacamento, tipografia e agrupamento que reduzam procura visual;
- composicao por controles isolados, com rolagem quando a area disponivel for
  menor que o conteudo minimo;
- comportamento previsivel em redimensionamento e em escalas de DPI comuns.

`IndustrialMainContentControl` e `IndustrialMainHostControl` sao referencias
visuais importantes, mas nao devem ser promovidos integralmente a interface de
producao: hoje parte de comunicacao, status e terminal desse conteudo e
estatica. O Layout 3 deve reutilizar o padrao visual, nao os valores simulados.

## 4. Estrutura proposta para o Layout 3

A composicao deve ter cinco zonas com responsabilidades explicitas:

```text
+--------------------------------------------------------------------------+
| Barra superior: identidade | estado global | acoes seguras | emergencia  |
+--------------------+-----------------------------+-----------------------+
| Diagnostico e      | Operacao e I/O              | Perfil de hardware e  |
| conexao             |                             | preparacao do teste   |
|                    |                             |                       |
+--------------------+-----------------------------+-----------------------+
| Terminal / log: historico operacional em toda a largura                 |
+--------------------------------------------------------------------------+
```

### Barra superior

- Exibe nome da aplicacao/bancada, estado global da comunicacao e contexto do
  perfil selecionado.
- Hospeda apenas atalhos para comandos operacionais ja existentes e aprovados.
- Mantem a parada de emergencia claramente separada das acoes comuns.
- Nao transforma troca de layout, tema ou perfil em comando ao equipamento.

### Coluna esquerda - diagnostico e conexao

- Concentra porta COM, atualizacao de portas, baud rate, Slave ID e opcoes de
  deteccao.
- Exibe resumo de configuracao, estado da conexao e diagnosticos ja existentes.
- Mantem conectar, desconectar e detectar com os mesmos validadores e servicos
  usados pelo Layout 1.
- Diferencia visualmente erro de comunicacao, configuracao invalida e estado
  ainda nao conectado.

### Area central - operacao e I/O

- E a principal area de trabalho do operador.
- Reune habilitacao de teste, reset de saidas e demais comandos existentes,
  sem criar novos comandos nesta etapa.
- Usa o painel industrial manual para apresentar DO e DI com controles grandes
  e estados legiveis.
- Mantem saidas desabilitadas quando as pre-condicoes atuais nao forem
  satisfeitas e preserva o fluxo seguro implementado pelos servicos existentes.

### Coluna direita - perfil de hardware e preparacao do teste

- Reutiliza a selecao de familia, modelo, modulo, comunicacao e perfil de teste.
- Mostra status de validacao, pendencias, evidencias e checklist de bancada.
- Mantem a acao de copiar o relatorio de preparacao.
- E estritamente informativa: selecao, geracao de relatorio e copia nao alteram
  parametros de conexao, nao executam escrita e nao acionam hardware.
- Em largura reduzida, pode recolher detalhes ou mover a coluna para uma aba
  secundaria, mas nao pode ocultar o status de seguranca da selecao.

### Area inferior - terminal/log

- Ocupa toda a largura para preservar leitura de linhas longas e sequencias de
  requisicao/resposta.
- Reutiliza o terminal real do Layout 1; textos demonstrativos do Layout 2 nao
  sao fonte de dados.
- Deve permitir redimensionamento vertical ou recolhimento controlado, mantendo
  indicacao visivel de novas mensagens e erros.

### Comportamento responsivo

Em telas amplas, as tres colunas aparecem simultaneamente. Em notebook ou com
escala de DPI elevada, a prioridade e manter barra superior, conexao e operacao
utilizaveis; perfil e terminal podem ser apresentados em paineis recolhiveis ou
abas. Rolagem e aceitavel, mas nenhum comando critico pode ficar cortado,
sobreposto ou inacessivel.

## 5. Responsabilidade dos componentes

| Componente existente | Direcao para o Layout 3 |
| --- | --- |
| `MainForm` | Permanecer como composicao, ciclo de vida e ligacao de eventos/servicos. Nao receber detalhes do novo layout. |
| Servicos de conexao, comando e I/O usados pelo `MainForm` | Reutilizar como unica fonte de comportamento operacional. A previa visual nao deve duplica-los. |
| `ConnectionStatePanelControl`, controles de configuracao e `TesterCommandPanelControl` | Preservar comportamento e validacoes; adaptar apenas por composicao em componentes dedicados nas fases futuras. |
| `TerminalLogPanelControl` | Reutilizar como fonte do terminal/log real na zona inferior. |
| `IndustrialManualIoPanelControl` | Reutilizar como apresentacao preferencial de I/O porque implementa `IDigitalIoManualPanel`; conectar eventos reais somente na Fase 4. |
| `DigitalIoManualPanelControl` | Manter isolado e funcional como referencia/fallback do Layout 1 durante a validacao do painel industrial. |
| `HardwareProfileSelectionControl` | Reutilizar como proprietario da selecao, resolucao, status e copia do relatorio. A previa deve validar sua acomodacao na coluna direita, pois o controle atual possui largura minima propria. |
| `HardwareTestPreparationReportFormatter` | Reutilizar sem duplicar texto ou regras de seguranca no novo layout. |
| `IndustrialMainContentControl` | Manter isolado como referencia visual. Nao reutilizar seus valores estaticos de conexao, terminal ou DI como estado real. |
| `IndustrialMainHostControl` | Manter como host/referencia do Layout 2 ate a aprovacao. O Layout 3 deve ter host proprio e isolado, sem substituir diretamente o modo normal. |
| `Program` | Permanecer inalterado nesta milestone. Em uma fase futura, expor a previa somente por entrada opt-in explicita, sem mudar o modo padrao. |

O novo host de previa deve ser um componente de composicao. Ele recebe ou
exibe controles especializados e publica intencoes de usuario; nao conhece
enderecos de registradores, nao cria servicos Modbus e nao decide regras de
habilitacao operacional.

## 6. Fora de escopo

- Nenhuma alteracao na logica Modbus.
- Nenhuma alteracao no mapa de registradores.
- Nenhum comando fisico disparado por selecao de layout, tema, familia, modelo,
  modulo, comunicacao ou perfil de teste.
- Nenhuma alteracao em arquivos HIstudio, incluindo `.dpk`, `.dmf` e arquivos
  do projeto `TESTADOR_NEON_HIO115_1_1`.
- Nenhuma reescrita direta do `MainForm` antes da aprovacao da previa isolada.
- Nenhuma criacao de novo comando de PLC ou novo fluxo de teste nesta proposta.
- Nenhuma mudanca do modo de producao nesta milestone de documentacao.

## 7. Fases de implementacao

### Fase 1 - documento/design

- Aprovar zonas, responsabilidades, criterios de seguranca e comportamento em
  telas menores.
- Nao alterar codigo de aplicacao.

### Fase 2 - previa isolada do Layout 3

- Criar host e formulario de previa separados do `MainForm`.
- Usar dados visuais inertes e entrada opt-in explicita.
- Garantir que abrir, trocar selecoes ou fechar a previa nao acione o PLC.

### Fase 3 - ligar status somente leitura

- Alimentar estado de conexao, diagnosticos, DI, contexto de perfil e log por
  adaptadores somente leitura.
- Manter botoes de escrita desabilitados ou sem ligacao operacional.
- Confirmar que ausencia de catalogo e falha de comunicacao sao estados
  independentes.

### Fase 4 - ligar comandos seguros ja existentes

- Ligar apenas intencoes de interface aos mesmos handlers/servicos validados do
  Layout 1.
- Preservar validacao de configuracao, pre-condicoes, tratamento de erro e
  registro no log.
- Nao adicionar comando, endereco ou regra Modbus.

### Fase 5 - validacao visual e manual

- Validar redimensionamento, DPI, navegacao por teclado, contraste, textos,
  foco, rolagem e acessibilidade dos controles.
- Executar roteiro manual sem hardware primeiro e, quando autorizado, validacao
  controlada em bancada com conjunto e programa HIstudio identificados.
- Registrar separadamente resultados para HIO115, DIO605, RION e NEON.

### Fase 6 - decisao sobre modo de producao

- Comparar a previa aprovada com o modo normal, incluindo regressao dos fluxos
  do Layout 1.
- Decidir entre manter a previa opcional, oferecer alternancia controlada ou
  promover o Layout 3 a modo padrao.
- Planejar a integracao sem transferir construcao detalhada de UI para o
  `MainForm`.

## 8. Criterios de aceitacao da previa

A previa pode ser aprovada quando todos os criterios abaixo forem atendidos:

1. Abre apenas por opcao explicita e nao altera o modo normal da aplicacao.
2. Abrir a previa, selecionar layout/tema/perfil e fecha-la gera zero leitura ou
   escrita Modbus e zero acionamento fisico.
3. As cinco zonas definidas neste documento sao identificaveis sem ambiguidade.
4. Porta, baud rate, Slave ID, estado de conexao, comandos, DO, DI, perfil,
   status de validacao e terminal possuem local previsto e rotulo legivel.
5. Estados de catalogo pendente, catalogo indisponivel, desconectado e falha de
   comunicacao apresentam mensagens distintas.
6. O painel industrial representa 4 DO e 8 DI sem sobreposicao, corte ou alvo
   de clique ambiguo.
7. Nenhum botao visual sem comportamento aprovado aparenta estar operacional;
   ele deve estar removido, marcado como indisponivel ou desabilitado.
8. Em 1280 x 720 e 1366 x 768, com escala de 100%, todos os comandos criticos
   ficam acessiveis por layout direto ou rolagem clara.
9. Em 1366 x 768 com escala de 125%, nenhum controle fica sobreposto e as zonas
   recolhidas continuam acessiveis.
10. O redimensionamento repetido nao perde controles, estados ou conteudo do
    terminal.
11. O perfil de hardware continua exibindo o aviso de que a selecao nao altera
    Modbus nem envia comandos, e o relatorio continua copiavel.
12. A previa reutiliza contratos/componentes existentes e nao introduz logica
    de registrador, comunicacao ou catalogo no host visual.
13. Revisao de codigo confirma que `MainForm`, `Program.cs`, mapas, logica
    Modbus e arquivos HIstudio nao foram alterados antes da aprovacao exigida
    para a fase correspondente.

## 9. Riscos e mitigacoes

| Risco | Mitigacao proposta |
| --- | --- |
| `MainForm` voltar a crescer | Criar host de Layout 3 e componentes de zona separados; manter no formulario apenas composicao e ligacao de dependencias. |
| Confundir status de catalogo com falha de comunicacao | Usar fontes, rotulos e cores independentes; declarar explicitamente que modulo nao confirmado nao significa erro de COM ou Modbus. |
| Disparar comandos fisicos acidentalmente | Manter a Fase 2 inerte, a Fase 3 somente leitura e ligar escrita apenas na Fase 4 aos servicos existentes, com botoes indisponiveis ate entao. |
| Tela pequena ou DPI elevado cortar controles | Validar dimensoes objetivas, usar layout adaptativo, paineis recolhiveis/abas e rolagem; priorizar acesso a conexao, operacao e emergencia. |
| Misturar premissas de HIO115, DIO605, RION e NEON | Exibir conjunto selecionado e nivel de evidencia; nao inferir mapa, pinagem, modulo, CPU ou resultado de bancada entre familias e conjuntos. |
| Valores demonstrativos do Layout 2 parecerem reais | Nao promover status, DI ou terminal estaticos do `IndustrialMainContentControl`; dados reais devem vir dos controles e servicos do Layout 1. |
| A coluna de hardware nao caber na composicao | Prototipar a coluna com o `HardwareProfileSelectionControl`, validar sua largura minima e adotar painel expansivel/aba sem duplicar sua logica. |
| Dois caminhos de UI divergirem durante a transicao | Manter o Layout 1 como referencia funcional, compartilhar contratos e executar regressao antes de qualquer decisao de producao. |

## 10. Proxima branch sugerida

Para a Fase 2, usar:

`ui/layout-3-preview-isolado`

Essa branch deve partir do estado aprovado deste documento e limitar o escopo a
uma previa opt-in, sem substituir a interface de producao.
