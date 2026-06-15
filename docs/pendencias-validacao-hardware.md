# Pendencias de validacao de hardware

## Pendencias gerais

- Confirmar referencias oficiais para `NEON_5`, `RION_PLUS` e `RION_5`.
- Confirmar CPU e slots de `NEON-2S`, `NEON 5` e `RION 5`.
- Confirmar mapas e procedimentos de teste antes de cadastrar pontos de I/O.
- Confirmar aplicabilidade de `HIO130`, `HIO140` e `HIO165`.
- Confirmar se cada perfil de comunicacao e suportado por cada modelo real.
- Conferir na aba `Perfil hardware` se novos cadastros aparecem como
  preparacao e nao como validacao operacional.

## NEON-1S + DIO605

- Validar comunicacao em bancada.
- Confirmar mapa usado pelo programa HIstudio carregado.
- Confirmar ligacao de retorno antes de usar `DIGITAL_IO_BASIC`.
- Registrar resultado antes de mudar o modelo para `verified_in_bench`.

## RION-502 + HIO115

- Validar topologia RS485 e endereco.
- Confirmar que nao existe outro mestre na rede durante o teste.
- Confirmar se o mapa atual do app se aplica ao conjunto RION-502 + HIO115.
- Registrar resultado por perfil antes de marcar o conjunto como validado.

## RION 5 e NEON 5

- Obter referencia oficial.
- Confirmar comunicacao disponivel.
- Definir modulos aplicaveis.
- Validar `COMMUNICATION_DIAGNOSTIC`.
- Criar perfis de I/O somente apos confirmar mapa e bancada.

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

A validacao em bancada exige CLP real identificado, programa HIstudio correto,
porta COM livre, comunicacao confirmada, execucao controlada do perfil e
registro do resultado. So depois disso um item deve sair de
`pending_manual_validation`.

## Comandos de apoio

```powershell
dotnet build .\testador-neon-hio115.sln
dotnet run --project .\app\TestadorCLPHI.App\TestadorCLPHI.App.csproj -- --validate-hardware-catalog
git diff --check
git status --short
git diff --stat
```
