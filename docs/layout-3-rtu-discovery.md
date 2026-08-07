# Descoberta Modbus RTU

## Faixa

- [Certo] Endereço inicial padrão: `1`.
- [Certo] Endereço final padrão: `247`.
- [Certo] O endereço `10` é somente evidência histórica.
- [Certo] Endereço `0` é proibido.
- [Certo] Endereços `248..255` não participam da descoberta automática.

## Algoritmo

```text
validar configuração
para endereço = inicial até final
    verificar cancelamento
    registrar uma tentativa simulada
    ler assinatura read-only
    se assinatura válida: parar e retornar identificado
    se respondeu mas divergiu: registrar e continuar
    aguardar intervalo configurado
retornar sem correspondência
```

O algoritmo não faz polling contínuo, reconexão automática ou repetição de
ciclos. A ação parte de comando explícito do operador e aceita
`CancellationToken` e progresso por tentativa.

## Assinatura consultada

Somente as referências documentadas abaixo são usadas:

| Referência | Alias | Acesso |
|---|---|---|
| `30012` | `PROG_ID` | R |
| `30013` | `PROG_CRC` | R |
| `30021` | `DEV_GFAIL_STS` | R |

F10 e F11 não são consultados porque seus endereços ainda não estão sustentados
por documentação versionada.

## Resultados

Os estados modelados incluem `NoResponse`, `ModbusResponse`, `UnknownDevice`,
`KnownFamily`, `KnownProgram`, `SignatureMismatch`, `CriticalFault`,
`Identified`, `Cancelled` e `Timeout`.

Uma resposta RTU com ID ou CRC divergente prova somente que houve resposta. Ela
não identifica o equipamento e não libera testes de I/O.

## Provas offline

O validador cobre fake nos endereços `1`, `10` e `247`, ausência de fake,
assinatura divergente, dois dispositivos, cancelamento e progresso
determinístico. Nenhum cenário enumera ou abre COM.
