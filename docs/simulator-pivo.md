# Simulador de Pivô Central

## Escopo do MVP

O perfil `pivo-central.json` demonstra o motor genérico. Todos os valores abaixo
são simulados e editáveis.

Entradas digitais: Emergência, Pressostato, Alinhamento, Fim de curso, Falha de
torre e Permissivo de água.

Analógicas: Pressão, Corrente, Velocidade e Posição percentual.

Saídas virtuais: Bomba, Frente, Reverso e Válvula de água.

## Estados e regras

- Emergência: bloqueia todas as saídas e ativa `Emergency`;
- Falha de torre: bloqueia saídas e ativa `TowerFault`;
- desalinhamento: bloqueia saídas e ativa `AlignmentFault`;
- bomba ligada com pressão simulada abaixo de `2`: ativa `PressureLow`;
- Frente e Reverso: grupo mutuamente exclusivo;
- demais estados: Parado, Preparando, Irrigando, MovendoFrente e
  MovendoReverso.

O threshold `2` pertence ao perfil de demonstração. Não deve ser aplicado a um
pivô real sem engenharia e aprovação próprias.

## Cenários

Normal, Emergência, Falha de torre, Pressão baixa e Desalinhado.

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
