# Arquitetura do Testador de CLPs HI

## Objetivo

Criar um testador modular para CLPs e módulos HI Tecnologia, começando pelo NEON com módulo HIO115.

## Estratégia

O HIstudio será usado para:

- abrir o projeto
- configurar hardware
- compilar aplicação
- carregar aplicação no CLP

O Git será usado para:

- guardar documentação
- guardar modelos de código ST
- guardar perfis de teste
- registrar evolução do projeto
- evitar perda de lógica por bugs do HIstudio

## Estrutura planejada

- docs/: documentação técnica do projeto
- src-st/: códigos ST de referência
- src-st/templates/: modelos reutilizáveis de código
- scripts/: automações PowerShell auxiliares

## Modos de teste

### Modo automático

Aciona as saídas digitais em sequência e verifica retorno nas entradas digitais.

### Modo manual

Permite acionar cada saída individualmente para diagnóstico de bancada.

### Modo por perfil

Seleciona o modelo do CLP/módulo e ajusta automaticamente a quantidade de entradas, saídas e testes aplicáveis.

## Perfil inicial validado

### HIO115

- 4 saídas digitais PNP
- 8 entradas digitais PNP
- 3 entradas analógicas 4–20 mA
- Relés acionados automaticamente
- Retorno das entradas validado via contato 14 dos relés
- Proteção/limitação aplicada na entrada 11

## Próximas etapas

1. Consolidar o teste automático do HIO115
2. Criar modo manual para DO00 a DO03
3. Criar seleção de perfil MODELO_TESTE
4. Criar flags de diagnóstico OK/FALHA por canal
5. Expandir para módulos com 8 e 16 pontos
6. Estudar RION
7. Criar simulador 4–20 mA
8. Futuramente criar aplicativo Windows

## Complemento - diagnóstico físico por relés

O teste do HIO115 valida o caminho físico completo:

DOxx -> relé de interface -> contato do relé -> DIxx -> diagnóstico no programa.

Isso evita depender apenas da indicação visual do HIstudio.

Resultado esperado por canal:

- OK: saída acionada e entrada correta recebeu retorno.
- FALHA: saída acionada, mas entrada esperada não recebeu retorno.
- ERRO: retorno apareceu em entrada errada.

Mapa base validado:

| Saída | Nome lógico | Relé | Entrada de retorno |
|---|---|---|---|
| DO00 | D000 | Relé 00 | DI00 |
| DO01 | D001 | Relé 01 | DI01 |
| DO02 | D002 | Relé 02 | DI02 |
| DO03 | D003 | Relé 03 | DI03 |
