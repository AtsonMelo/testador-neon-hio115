# Simulador de Pivô Central

## Escopo do Simulador Industrial 2.0

O perfil `pivo-central.json` demonstra o motor genérico. Todos os valores abaixo
são simulados e editáveis.

A visualização usa um `PivotProcessControl` WinForms nativo com GDI+:

- centro, linha do pivô e torres;
- sentido Frente/Reverso/Parado;
- posição percentual `0..100`;
- indicação conceitual de bomba e água;
- estados de torre `OK`, `MOVING`, `MISALIGNED`, `FAULT` e `UNKNOWN`;
- barra de posição e textos acessíveis, sem depender somente de cor.

O controle é double-buffered, reutiliza recursos de desenho e só invalida quando
o estado visual muda. Não existe animação contínua, timer, thread, WebView,
imagem grande ou dependência gráfica externa.

Entradas digitais: Emergência, Pressostato, Alinhamento, Fim de curso, Falha de
torre e Permissivo de água.

Analógicas: Pressão, Corrente, Velocidade e Posição percentual.

Saídas virtuais: Bomba, Frente, Reverso e Válvula de água.

## Estados e regras

- Emergência: bloqueia todas as saídas e ativa `Emergency`;
- Falha de torre: bloqueia saídas e ativa `TowerFault`;
- desalinhamento: bloqueia saídas e ativa `AlignmentFault`;
- bomba ligada com pressão simulada abaixo de `2`: ativa `PressureLow`;
- fim de curso: ativa `EndOfTravel` e bloqueia saídas;
- Frente e Reverso: grupo mutuamente exclusivo;
- demais estados: Parado, Preparando, Irrigando, MovendoFrente e
  MovendoReverso.

O threshold `2` pertence ao perfil de demonstração. Não deve ser aplicado a um
pivô real sem engenharia e aprovação próprias.

## Torres configuráveis

`visualization.towerCount` define a quantidade de torres. O perfil distribuído
usa `4`; o renderer aceita quantidades de `1..16`. `faultTowerIndex` identifica
a torre conceitual usada pelos cenários Falha de torre e Desalinhamento. Isso é
estado de processo simulado, não protocolo ou diagnóstico de um pivô real.

## Cenários

Normal, Irrigando, Movendo frente, Movendo reverso, Emergência,
Desalinhamento, Falha de torre, Pressão baixa e Fim de curso.

Os sinais permanecem editáveis manualmente e são agrupados em Segurança, Água,
Movimento e Saídas conforme o próprio perfil.

## Mapeamento fake HIO115

- DI00..DI05 recebem as seis entradas digitais do perfil;
- AI00 recebe Pressão;
- AI01 recebe Corrente;
- AI02 recebe Posição percentual;
- DO00 controla Bomba;
- DO01 controla Frente;
- DO02 controla Reverso;
- DO03 controla Válvula de água.

Esse mapeamento existe somente em memória e não altera programa de CLP.

O sinal Velocidade permanece disponível no processo, mas não recebe binding no
fake atual porque o HIO115 simulado expõe somente AI00..AI02 e esses canais estão
ocupados por Pressão, Corrente e Posição. Nenhum canal adicional foi inventado.

## Estado de revisão

- implementação: `STRUCTURALLY_VALIDATED`;
- aprovação visual: `AWAITING_HUMAN_VISUAL_REVIEW`.

Este documento não afirma equivalência física, angular ou estrutural com um
pivô real.
