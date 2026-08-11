# Issue #24 - Plano de host limpo para integrar Layout Alvo 2

## Decisao tecnica

A integracao real do Layout Alvo 2 na MainForm deve usar um host/painel limpo, em vez de tentar encaixar o novo layout nos blocos antigos.

## Problema evitado

A MainForm ainda monta diretamente o layout antigo no construtor, incluindo rootLayout, connectionLayout, workTabs, GroupBox de conexao, abas de I/O Manual e Terminal.

Remendar o Layout Alvo 2 dentro dessa estrutura tende a gerar conflito visual, retrabalho e crescimento da MainForm.

## Direcao

Criar componente separado em app/TestadorCLPHI.App/Ui/Industrial.

Nome sugerido:

- IndustrialMainHostControl.cs

Responsabilidades do host:

- hospedar a experiencia visual industrial principal;
- preservar o Layout Alvo 2 como referencia visual;
- preparar pontos de ligacao para eventos e services;
- evitar dependência do TabControl e GroupBox antigos;
- permitir substituicao controlada da area principal da MainForm.

## Responsabilidade da MainForm

A MainForm deve ficar apenas com:

- criacao dos services;
- ligacao de eventos;
- atualizacao de estado;
- chamada de metodos existentes;
- composicao minima do host.

A MainForm nao deve receber layout detalhado novo.

## Arquivos que nao devem ser alterados sem necessidade

- stashes antigos;
- arquivos HIstudio .dpk/.dmf;
- previews antigos que ja servem como referencia.

## Primeiro ciclo recomendado

Criar o IndustrialMainHostControl isolado e validar por preview ou instanciacao controlada, sem remover imediatamente o layout antigo.

## Segundo ciclo recomendado

Fazer a MainForm hospedar o novo host com alteracao minima e reversivel.

## Criterios de validacao

- MainForm.cs nao deve crescer de forma relevante;
- dotnet build sem avisos;
- check-csharp-literals.ps1 OK;
- git diff --check OK;
- app principal abre;
- Layout Alvo 2 visualmente preservado;
- stashes antigos nao aplicados.
