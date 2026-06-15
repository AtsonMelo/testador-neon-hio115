# Pendencias de validacao de hardware

## Pendencias gerais

- Confirmar referencias oficiais para `RION_PLUS`.
- Validar em bancada as referencias oficiais agora registradas para `NEON_5`
  e `RION_5`.
- Confirmar CPU e slots de `NEON-2S`, `NEON 5` e `RION 5`.
- Confirmar mapas e procedimentos de teste antes de cadastrar pontos de I/O.
- Confirmar aplicabilidade de `HIO130`, `HIO140` e `HIO165` em bancada.
- Confirmar se cada perfil de comunicacao e suportado por cada modelo real.
- Conferir na aba `Perfil hardware` se novos cadastros aparecem como
  preparacao e nao como validacao operacional.
- Conferir se o relatorio/checklist copiado da aba mantem pendencias e avisos
  de seguranca antes de qualquer teste real.

## NEON-1S + DIO605

- Validar comunicacao em bancada.
- Gerar ou copiar o relatorio de preparacao na aba `Perfil hardware` antes de
  conectar.
- Confirmar mapa usado pelo programa HIstudio carregado.
- Confirmar ligacao de retorno antes de usar `DIGITAL_IO_BASIC`.
- Registrar resultado antes de mudar o modelo para `verified_in_bench`.

## RION-502 + HIO115

- Validar topologia RS485 e endereco.
- Gerar ou copiar o relatorio de preparacao na aba `Perfil hardware` antes de
  executar `COMMUNICATION_DIAGNOSTIC` ou `REMOTE_IO_RS485`.
- Confirmar que nao existe outro mestre na rede durante o teste.
- Confirmar se o mapa atual do app se aplica ao conjunto RION-502 + HIO115.
- Registrar resultado por perfil antes de marcar o conjunto como validado.

## RION 5 e NEON 5

- Referencias oficiais foram registradas no catalogo.
- Para RION 5, a referencia oficial informa CLP e/ou I/O remoto, suporte para
  1 modulo, ate 16 pontos de I/O e ate 14 RION 5 como I/O remoto ao CLP NEON.
- Para NEON 5, a navegacao oficial lista ate 240 pontos de I/O.
- Confirmar comunicacao disponivel em bancada.
- Definir modulos aplicaveis por conjunto real.
- Validar `COMMUNICATION_DIAGNOSTIC`.
- Criar perfis de I/O somente apos confirmar mapa e bancada.
- Quando a UI mostrar ausencia de modulo, interpretar como "nenhum modulo
  confirmado para este modelo", nao como falha de comunicacao.

## Referencia oficial vs bancada

- `official_reference` registra fonte oficial HI Tecnologia consultada.
- `field_observed` preserva evidencia observada no projeto.
- `verified_in_bench` exige resultado de bancada documentado.

Referencia oficial nao valida mapa, pinagem, comando, topologia, slave ID,
programa HIstudio carregado ou retorno fisico. Por isso `NEON_5_CONTROLLER`,
`RION_5_CONTROLLER`, `HIO130`, `HIO140`, `HIO165` e os conjuntos ainda sem
teste permanecem `pending_manual_validation`.

Referencias de modulos adicionadas:

- HIO115: especificacao oficial HIO115.
- HIO130: especificacao oficial HIO130.
- HIO140: especificacao oficial HIO140.
- HIO165: especificacao oficial HIO165.
- DIO605: especificacao oficial DIO605.

## Radio transparente

- O app nao configura radio XBee/R9X307 nesta milestone.
- Usar XCTU para configuracao profunda.
- Documentar baud rate, canal, PAN/rede e demais parametros fora do catalogo
  ate haver decisao de produto.
- Garantir que XCTU nao esteja usando a mesma COM que o Testador.

## Criterio para remover pendencia

1. O item tem referencia oficial ou observacao de bancada documentada.
2. O conjunto real foi identificado: modelo, CPU, modulo, slot e perfil.
3. O teste foi executado sem erro de comunicacao.
4. O resultado esperado foi observado.
5. `validationNotes` foi preenchido quando `validationStatus` mudar para
   `verified_in_bench`.
6. O validador e o build passaram.

## Selecionar perfil vs validar hardware

A selecao na UI organiza informacao do catalogo. Ela ajuda o operador a ver
testes aplicaveis, modulos compativeis, perfis possiveis e pendencias, mas nao
remove nenhuma pendencia por si so.

O relatorio de preparacao gerado na mesma aba transforma a selecao em texto
copiavel. Ele inclui selecao atual, status de validacao, pendencias, itens
observados em campo, necessidades de bancada, checklist do operador, aviso de
conflito de COM, aviso HIstudio/XCTU/Testador, aviso de que nao executa comando
fisico e aviso de que bancada ainda e obrigatoria. Copiar esse texto nao muda
parametro, nao abre conexao, nao escreve em PLC e nao substitui bancada.

O comando `--validate-hardware-profile-selection` cobre somente a consistencia
nao visual entre catalogo, resolver e UI. Ele verifica cenarios de NEON-1S,
RION-502, NEON 5 e RION 5, confirma que pendencias continuam visiveis e que
itens `field_observed` aparecem quando aplicavel. Ele nao abre UI, nao altera
parametros, nao escreve em PLC e nao envia comando fisico.

O comando `--validate-hardware-test-report` cobre a geracao nao visual do
relatorio. Ele carrega o catalogo, gera os cinco cenarios principais e verifica
que os relatorios nao estao vazios, contem avisos de seguranca e mantem casos
pendentes explicitos.

A validacao em bancada exige CLP real identificado, programa HIstudio correto,
porta COM livre, comunicacao confirmada, execucao controlada do perfil e
registro do resultado. So depois disso um item deve sair de
`pending_manual_validation`.

## Comandos de apoio

```powershell
dotnet build .\testador-neon-hio115.sln
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-catalog
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-profile-selection
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-test-report
git diff --check
git status --short
git diff --stat
```
