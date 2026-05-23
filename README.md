# Testador NEON HIO115

Software industrial em desenvolvimento para teste e diagnóstico de bancada do CLP HI Tecnologia NEON com módulo HIO115.

## Objetivo

Criar uma ferramenta prática para apoiar testes de entradas, saídas e comunicação do conjunto NEON/HIO115, aproximando automação industrial, bancada de testes, diagnóstico técnico e desenvolvimento em C#/.NET.

## Problema que o projeto busca resolver

Em campo e bancada, a validação de I/O de CLPs pode exigir testes repetitivos, registros manuais e conferência de retorno de sinais.

Este projeto organiza esse processo em uma aplicação desktop, permitindo evoluir para uma bancada mais padronizada de teste, diagnóstico e documentação.

## Escopo inicial

O projeto contempla estudos e validações para:

- saídas digitais;
- entradas digitais;
- entradas analógicas;
- interface manual de I/O;
- fluxo de teste em bancada;
- integração futura com comunicação serial/Modbus;
- documentação técnica do processo.

## Hardware de referência

- CLP HI Tecnologia NEON;
- módulo HIO115;
- fonte 24 Vcc;
- interface de bancada;
- relés, LEDs, resistores e sinais de teste;
- computador Windows para execução da aplicação.

## Software

- C#;
- .NET;
- Windows Forms;
- PowerShell para automação de apoio;
- Git/GitHub para versionamento;
- HIstudio para projeto, compilação e carga no CLP.

## Estado atual

O projeto está em fase de evolução da interface industrial e organização da arquitetura visual.

Há trabalho em andamento para integrar gradualmente o novo layout industrial ao fluxo principal da aplicação, mantendo o layout legado preservado durante a transição.

## Evidências técnicas

Este projeto busca evidenciar:

- aplicação real de programação em contexto industrial;
- evolução de interface WinForms;
- diagnóstico de I/O;
- organização por issues, branches, commits e PRs;
- documentação de decisões técnicas;
- validação prática em bancada;
- aprendizado progressivo em automação e software.

## HIstudio e relatos técnicos

Durante o desenvolvimento, foram registrados aprendizados sobre o fluxo de trabalho com HIstudio, incluindo cuidados com arquivos do projeto, pacote .dpk, compilação e carga no CLP.

Relatos técnicos sanitizados estão em:

docs/histudio/relatos-tecnicos-sanitizados.md

Esses relatos têm finalidade educativa e profissional. Não expõem dados sensíveis, comunicações privadas ou críticas pessoais.

## Organização do repositório

| Pasta | Função |
|---|---|
| app/ | Aplicação desktop em C#/.NET |
| docs/ | Documentação técnica e decisões do projeto |
| scripts/ | Scripts auxiliares |
| src-st/ | Estudos e arquivos relacionados a Structured Text |
| TESTADOR_NEON_HIO115_1_1/ | Estrutura do projeto HIstudio |
| tools/ | Ferramentas auxiliares |

## Direção profissional

Este projeto faz parte do meu portfólio de evolução em tecnologia, unindo experiência prática em automação industrial com desenvolvimento de software.

O foco é demonstrar evolução real em:

- programação;
- diagnóstico técnico;
- documentação;
- organização com Git/GitHub;
- aplicação industrial;
- comunicação profissional de problemas e soluções.

## Próximos passos

- Melhorar documentação da arquitetura da aplicação.
- Adicionar prints da interface.
- Documentar fluxo de teste de I/O.
- Organizar instruções de build e execução.
- Evoluir integração do layout industrial.
- Registrar validações práticas de bancada.
