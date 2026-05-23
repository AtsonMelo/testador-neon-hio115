# Relatos Técnicos Sanitizados — HIstudio

## Objetivo

Registrar aprendizados técnicos, inconsistências observadas e cuidados práticos durante o uso do HIstudio no contexto do projeto Testador NEON HIO115.

Este documento tem finalidade profissional e educativa. Ele não expõe dados sensíveis, arquivos privados, comunicações internas ou críticas pessoais a fornecedores/desenvolvedores.

---

## Caso 001 — Alterações externas não refletem automaticamente no pacote .dpk

### Contexto

Durante a organização do projeto de bancada com CLP HI/Neon HIO115, foi observado que alterações feitas diretamente em arquivos auxiliares, como ST, CSV ou DMF, não necessariamente atualizam o pacote .dpk de forma automática.

### Comportamento observado

Alterações externas podem existir no Git ou na pasta do projeto, mas não aparecerem corretamente no fluxo final do HIstudio enquanto o projeto não for aberto, tratado, compilado e salvo/carregado pela ferramenta.

### Interpretação técnica

O .dpk deve ser tratado como pacote/projeto controlado pelo HIstudio. Alterações manuais em arquivos de apoio não substituem o fluxo normal da ferramenta.

### Procedimento seguro

Quando a alteração precisa refletir no projeto final:

1. Abrir o projeto no HIstudio.
2. Editar ou importar o recurso pelo fluxo da ferramenta.
3. Compilar.
4. Salvar ou gerar o pacote.
5. Carregar no CLP quando aplicável.
6. Validar em bancada.

### Aprendizado

Git ajuda a rastrear arquivos e histórico, mas não substitui o processo de edição, compilação e empacotamento do HIstudio.

---

## Caso 002 — Separação entre bug, limitação e fluxo incorreto

### Contexto

Durante os testes, alguns comportamentos podem parecer falhas da ferramenta, mas precisam ser classificados com cuidado.

### Critérios usados

Antes de considerar algo como bug:

1. Reproduzir o comportamento.
2. Registrar versão, ambiente e passos.
3. Comparar comportamento esperado e observado.
4. Verificar se existe procedimento correto documentado.
5. Separar erro de uso, limitação conhecida e possível inconsistência.

### Postura profissional

Quando houver indício real de bug, o caminho correto é gerar um relato técnico reproduzível para os responsáveis, evitando exposição pública indevida.

---

## Diretriz de publicação

Este repositório pode citar aprendizados e relatos sanitizados, mas não deve publicar:

- credenciais;
- arquivos de clientes;
- e-mails privados;
- prints com dados sensíveis;
- críticas pessoais;
- conteúdo que exponha fornecedor, empresa ou usuários.

A evidência profissional está na capacidade de diagnosticar, documentar e comunicar tecnicamente o problema.
