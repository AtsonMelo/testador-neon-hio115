# Issue #16 — Visual alvo do TestadorCLPHI.App

## Objetivo

Evoluir o TestadorCLPHI.App para um visual industrial dark, inspirado em painéis de controle, SCADA e bancadas de teste de CLP.

A imagem gerada nesta etapa passa a ser a referência visual principal para a evolução da UI.

## Direção visual

- Tema dark industrial.
- Cards/painéis com bordas suaves e aparência técnica.
- Hierarquia visual clara.
- Uso de cores com significado operacional:
  - vermelho: erro, emergência, stop;
  - verde: OK, conectado, ativo;
  - amarelo: atenção, teste pendente, alerta;
  - azul: ação técnica, detecção, comunicação;
  - cinza: desligado, inativo, neutro.
- Componentes inspirados em elementos reais de painel:
  - STOP como botoeira de emergência;
  - DO como botoeiras de comando;
  - DI como sinaleiras/LEDs de painel;
  - terminal/log como console técnico.

## Layout alvo

### Cabeçalho

- Título: Testador CLP HI.
- Subtítulo: Comunicação Modbus RTU real em fase inicial.
- Status compacto:
  - CLP: Conectado/Desconectado.
  - Teste: Habilitado/Desabilitado.
- STOP de emergência em destaque no lado direito.

### Estado da conexão

- Card técnico com:
  - Status;
  - Mensagem;
  - Atualizado;
  - botões Conectar CLP, Simular erro, Desconectar e Ler %MW70.

### Conexão com CLP

- Card técnico com:
  - Porta COM;
  - Baud rate;
  - Slave ID;
  - busca de baud rates;
  - botão Detectar CLP destacado;
  - resumo da configuração atual.

### Comandos do testador

- Botões:
  - Habilitar teste;
  - Resetar saídas;
  - Teste automático.

### Painel manual 4DO/8DI

- Instrução:
  - 1) Clique em Habilitar teste;
  - 2) Acione a saída.
- DO:
  - D000 -> DI00 + DI04;
  - D001 -> DI01 + DI05;
  - D002 -> DI02 + DI06;
  - D003 -> DI03 + DI07.
- DO deve ter aparência de botoeira de painel.
- DI00 a DI07 devem ter aparência de sinaleiras/LEDs de painel.

### Terminal / Log

- Área inferior com console técnico.
- Fonte monoespaçada.
- Linhas futuras:
  - eventos do app;
  - comandos Modbus;
  - respostas do CLP;
  - erros e timeouts.

## Ordem de implementação

1. Consolidar header e terminal/log inicial.
2. Criar base visual reutilizável para cores e botões industriais.
3. Estilizar DO como botoeiras.
4. Estilizar DI como sinaleiras.
5. Melhorar cards de Estado da conexão e Conexão com CLP.
6. Melhorar Comandos do testador e incluir visualmente Teste automático.
7. Evoluir terminal/log.
8. Revisar responsividade e espaçamento geral.

## Regras arquiteturais

- Não adicionar regra pesada no MainForm.
- MainForm deve apenas instanciar controles, ligar eventos e atualizar estado visual.
- Lógica técnica deve ficar em Services.
- Layout visual deve ficar em UserControls.
- Componentes visuais reutilizáveis devem ficar em Ui/Controls ou Ui/Theme.

## Pendências relacionadas

- Issue #17: deixar Habilitar teste mais chamativo.
- Issue #22: adicionar botão de Teste Automático HIO115.
- Issue #10: reduzir e reorganizar MainForm.
