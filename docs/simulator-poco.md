# Simulador de Poço

## Escopo do MVP

O perfil `poco.json` reutiliza o mesmo `SimulationEngine`.

O conceito visual aprovado foi preservado. Nesta evolução o Poço recebeu apenas
o mesmo vocabulário de status, cards de grandezas, agrupamento de sinais,
aliases técnicos no Testador e Mapa de I/O. Não recebeu renderer de Pivô nem
regras específicas codificadas na UI.

Entradas digitais: Nível mínimo, Nível máximo, Falta de fase, Pressostato,
Emergência e Sensor inválido.

Analógicas: Nível, Pressão e Corrente.

Saídas virtuais: Bomba e Válvula.

## Estados e regras

- Emergência: estado Emergencia e saídas bloqueadas;
- Falta de fase: estado Falha e alarme `PhaseLoss`;
- Sensor inválido: estado Falha e alarme `InvalidSensor`;
- nível simulado abaixo de `20`: estado NivelBaixo;
- corrente simulada acima de `30`: estado Falha;
- bomba com pressão simulada abaixo de `2`: estado Falha;
- bomba aceita: estado Bombeando.

Os limites `20`, `30` e `2` são parâmetros do perfil de demonstração, não
valores industriais confirmados.

## Cenários

Normal, Bombeando, Falta d'água, Pressão baixa, Sobrecorrente, Falta de fase e
Sensor inválido.

## Mapeamento fake HIO115

- DI00..DI05 recebem as seis entradas;
- AI00 recebe Nível;
- AI01 recebe Pressão;
- AI02 recebe Corrente;
- DO00 controla Bomba;
- DO01 controla Válvula;
- DO02 e DO03 permanecem sem efeito no processo Poço.

Os bindings são declarados em `poco.json`. DI06, DI07, DO02 e DO03 continuam
visíveis tecnicamente no Testador como canais sem binding no perfil; não são
renomeados nem associados artificialmente a sinais de processo.

## Estado de revisão

- implementação: `STRUCTURALLY_VALIDATED`;
- aprovação visual desta evolução: `AWAITING_HUMAN_VISUAL_REVIEW`.
