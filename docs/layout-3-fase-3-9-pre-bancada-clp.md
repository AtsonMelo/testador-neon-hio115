# Layout 3 - Fase 3.9 - Preparacao offline para bancada

## Estado

`STATUS: SOFTWARE EM PREPARACAO OFFLINE - BANCADA FISICA BLOQUEADA`

Esta fase prepara somente configuracao, validacao e arquitetura locais. Nao existe
transporte RTU executavel, acesso a porta serial, leitura de equipamento, escrita
fisica ou comando fisico.

Contadores desta fase:

- conexoes reais: 0;
- leituras reais: 0;
- escritas reais: 0;
- comandos fisicos: 0.

## Escopo vigente

- transporte do escopo atual: Modbus RTU;
- Modbus TCP: backlog, sem contrato ativo, IP, porta ou descoberta Ethernet;
- camada fisica selecionavel no futuro: RS-232 ou RS-485;
- perfil observado: COM8, RS-232, 38400, 8-N-1;
- endereco manual inicial sugerido: 1;
- descoberta planejada: 1 a 247, crescente, uma tentativa por endereco;
- endereco 10: evidencia historica, nunca default operacional;
- descoberta, feature, comunicacao, polling e reconexao: OFF;
- Gate D offline: autorizado em 2026-08-06;
- transporte e teste fisicos: nao autorizados.

## Evidencias do equipamento

| Item | Valor | Estado |
|---|---|---|
| Identidade HIstudio | NEON5-1S / CPU450 / HIO115 slot 1 | CONFIRMADO como exibido |
| Identidade frontal | PIVODRIP / OMNICONTROL / OMNI-PLC2 | CONFIRMADO como observado |
| Relacao OMNI-PLC2 / NEON5-1S | Nao comprovada | CONFLITANTE |
| Numero de serie | 111.20023 | CONFIRMADO como observado |
| Part number | 300.111.622.801 | CONFIRMADO como observado |
| Identificacao adicional | Slot 1-115 | COMPATIVEL com HIO115, nao conclusivo |
| HIstudio | 2.4.03 | CONFIRMADO como exibido |
| Firmware/familia exibida | G5PLC.C950.ST [3.3.11] | PROVAVEL para a CPU |
| Programa | MOTOR_HIDRO:PROD_NEON5_HIO115, versao 3220 | CONFIRMADO como exibido |
| ID / CRC observados | 31134 / 23248 | CONFIRMADO como exibido |
| Condicao observada | Programa rodando; Cold restart | CONFIRMADO como exibido |
| Alimentacao nominal indicada | `10-30 VDC` | DECLARADO/OBSERVADO; medir antes da bancada |

As telas tambem registraram equipamento remoto offline e ausencia de base de
hardware no ambiente. Portanto, nenhum estado atual de DI, DO, AI, contador ou
PWM foi promovido a leitura fisica.

## Tres modos planejados

### 1. Identificacao automatica

Somente leitura. Usa resposta, assinatura de firmware, versao, F12/30012
`PROG_ID`, F13/30013 `PROG_CRC` e F21/30021 `DEV_GFAIL_STS`. F10 e F11 continuam
sem referencia de protocolo confirmada. Uma assinatura divergente bloqueia todos
os testes de I/O.

Estados previstos: `IDENTIFICADO`, `RESPONDEU MAS NAO RECONHECIDO`, `SEM
RESPOSTA`, `ASSINATURA DIVERGENTE`, `FALHA CRITICA` e `CANCELADO`.

### 2. Teste de entradas

Somente leitura. Allow-list fechada: DI00..DI07 em 31120..31127 e AI00..AI02 em
31132..31134. A conversao de AI permanece bloqueada ate a escala de engenharia
ser confirmada; a apresentacao 4-20 mA observada nao define essa escala.

### 3. Teste supervisionado de saidas

Previsto para o produto final, mas desabilitado nesta fase. A unica allow-list de
saida e DO00..DO03 em 31128..31131. O contrato de dominio usa canais fechados,
nunca endereco arbitrario. Exige identificacao valida, F21 sem falha critica,
checklist aprovado, habilitacao explicita do operador e gate fisico separado.

Somente uma saida por vez, modo momentaneo, duracao maxima, cancelamento,
desligamento no encerramento e validacao do retorno. Perda de comunicacao nunca
e tratada como prova de que uma saida fisica desligou.

## Bloqueios tecnicos

- F1137/31137, F1140/31140 e F1143/31143: reservados, bloqueados;
- F1144/31144 e F1145/31145: PWM, bloqueados;
- contadores e encoder: fora do primeiro teste operacional;
- coils: proibidas;
- escrita generica: proibida;
- enderecos 0 e 248..255: fora da descoberta automatica;
- transporte real: inexistente e nao autorizado.

## Pendencias para READY

- confirmar documentalmente a relacao OMNI-PLC2 / NEON5-1S;
- confirmar o protocolo realmente ativado no canal observado;
- confirmar referencia de protocolo de F10/F11 e conversao de referencias para PDU;
- aprovar para bancada os defaults de software: timeout 500 ms, intervalo 100 ms
  e duracao maxima de saida 1000 ms;
- anexar foto/evidencia da etiqueta;
- medir tensao e registrar instrumento/resultado;
- registrar caminho do backup e SHA-256;
- comprovar aterramento, isolamento, estado seguro, emergencia e desconexao;
- concluir e validar o Gate D offline com fake antes de qualquer bancada;
- autorizar separadamente qualquer teste fisico supervisionado de saida.

## Comandos locais

```powershell
dotnet run --project app/TestadorCLPHI.App/TestadorCLPHI.App.csproj -c Release --no-build -- --validate-layout-3-bench-readiness-self-tests
dotnet run --project app/TestadorCLPHI.App/TestadorCLPHI.App.csproj -c Release --no-build -- --validate-layout-3-bench-readiness
```

O segundo comando deve retornar exit code 2 enquanto as pendencias e os gates
permanecerem abertos.
