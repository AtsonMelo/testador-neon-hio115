import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]

PROFILE_PATH = ROOT / "profiles" / "hio115.json"

OUT_ST_AUTO = ROOT / "src-st" / "generated" / "program_01_hio115_auto.st"
OUT_ST_AUTO_MANUAL = ROOT / "src-st" / "generated" / "program_01_hio115_auto_manual.st"

OUT_VARS_DOC = ROOT / "docs" / "variaveis-geradas-hio115.md"

OUT_GLOBALS_CSV = ROOT / "histudio-import" / "globais_completo_com_classe.csv"
OUT_PROGRAM_CSV = ROOT / "histudio-import" / "program_01_completo_com_classe.csv"


def load_profile():
    with PROFILE_PATH.open("r", encoding="utf-8") as f:
        return json.load(f)


def d_name(index: int) -> str:
    return f"D{index:03d}"


def di_name(index: int) -> str:
    return f"DI{index:02d}"


def generate_auto_st(profile):
    total_do = int(profile["total_do"])
    tempo = int(profile.get("tempo_passo_segundos", 2))
    do_sysvars = profile["do_sysvars"]

    lines = [
        "(* Programa gerado automaticamente para teste automatico do HIO115 *)",
        "(* Nao editar diretamente no HIstudio sem atualizar o projeto Git *)",
        "",
        "(* Variaveis locais esperadas no PROGRAM_01: *)",
        "(* T_STEP      : TON  *)",
        "(* STEP_TESTE  : INT  valor inicial 0 *)",
        "(* RESET_TIMER : BOOL valor inicial FALSE *)",
        "",
        f"T_STEP(IN := NOT RESET_TIMER, PT := T#{tempo}s);",
        "",
        "IF T_STEP.Q THEN",
        "    STEP_TESTE := STEP_TESTE + 1;",
        "",
        f"    IF STEP_TESTE > {total_do} THEN",
        "        STEP_TESTE := 0;",
        "    END_IF;",
        "",
        "    RESET_TIMER := TRUE;",
        "ELSE",
        "    RESET_TIMER := FALSE;",
        "END_IF;",
        "",
        "(* Desliga todas as saidas digitais *)",
    ]

    for index, sysvar in enumerate(do_sysvars):
        lines.append(f"HILS.SET_SYSVAR({sysvar}, 0);  (* {d_name(index)} / DO{index:02d} *)")

    lines.append("")
    lines.append("(* Liga uma saida por vez *)")

    for index, sysvar in enumerate(do_sysvars):
        step = index + 1
        lines.append(f"IF STEP_TESTE = {step} THEN")
        lines.append(f"    HILS.SET_SYSVAR({sysvar}, 1);  (* {d_name(index)} / DO{index:02d} *)")
        lines.append("END_IF;")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def generate_auto_manual_st(profile):
    total_do = int(profile["total_do"])
    tempo = int(profile.get("tempo_passo_segundos", 2))
    do_sysvars = profile["do_sysvars"]
    di_sysvars = profile["di_sysvars"]

    lines = [
        "(* Programa HIO115 - modo automatico, modo manual e diagnostico *)",
        "(* Gerado automaticamente por scripts/gerar-programa-clp.py *)",
        "(* Nao editar diretamente no HIstudio sem atualizar o projeto Git *)",
        "",
        "(* Leitura dos retornos digitais *)",
    ]

    for index in range(total_do):
        lines.append(
            f"RETORNO_{di_name(index)} := HILS.GET_SYSVAR({di_sysvars[index]});  (* {di_name(index)} *)"
        )

    lines += [
        "",
        "(* Zera comandos internos *)",
    ]

    for index in range(total_do):
        lines.append(f"COMANDO_{d_name(index)} := 0;")

    lines += [
        "",
        "(* Modo automatico *)",
        "IF MODO_AUTO = 1 THEN",
        "",
        f"    T_STEP(IN := NOT RESET_TIMER, PT := T#{tempo}s);",
        "",
        "    IF T_STEP.Q THEN",
        "        STEP_TESTE := STEP_TESTE + 1;",
        "",
        f"        IF STEP_TESTE > {total_do} THEN",
        "            STEP_TESTE := 0;",
        "        END_IF;",
        "",
        "        RESET_TIMER := TRUE;",
        "    ELSE",
        "        RESET_TIMER := FALSE;",
        "    END_IF;",
        "",
    ]

    for index in range(total_do):
        step = index + 1
        lines += [
            f"    IF STEP_TESTE = {step} THEN",
            f"        COMANDO_{d_name(index)} := 1;",
            "    END_IF;",
            "",
        ]

    lines += [
        "END_IF;",
        "",
        "(* Modo manual *)",
        "IF MODO_MANUAL = 1 THEN",
    ]

    for index in range(total_do):
        lines.append(f"    COMANDO_{d_name(index)} := CMD_MANUAL_{d_name(index)};")

    lines += [
        "END_IF;",
        "",
        "(* Escrita nas saidas digitais *)",
    ]

    for index, sysvar in enumerate(do_sysvars):
        lines.append(
            f"HILS.SET_SYSVAR({sysvar}, COMANDO_{d_name(index)});  (* {d_name(index)} / DO{index:02d} *)"
        )

    lines += [
        "",
        "(* Zera diagnosticos por ciclo *)",
    ]

    for index in range(total_do):
        lines.append(f"OK_{d_name(index)} := 0;")

    lines.append("")

    for index in range(total_do):
        lines.append(f"FALHA_{d_name(index)} := 0;")

    lines.append("")

    for index in range(total_do):
        lines.append(f"ERRO_{d_name(index)} := 0;")

    lines += [
        "",
        "TESTE_OK_GERAL := 0;",
        "TESTE_FALHA_GERAL := 0;",
        "TESTE_ERRO_GERAL := 0;",
        "",
        "(* Diagnostico de retorno correto, falha e retorno cruzado *)",
    ]

    for index in range(total_do):
        expected_di = di_name(index)
        other_dis = [di_name(other) for other in range(total_do) if other != index]
        other_expression = " OR ".join([f"RETORNO_{name} = 1" for name in other_dis])

        lines += [
            f"IF COMANDO_{d_name(index)} = 1 THEN",
            f"    IF (RETORNO_{expected_di} = 1) AND NOT ({other_expression}) THEN",
            f"        OK_{d_name(index)} := 1;",
            "    END_IF;",
            "",
            f"    IF RETORNO_{expected_di} = 0 THEN",
            f"        FALHA_{d_name(index)} := 1;",
            "    END_IF;",
            "",
            f"    IF {other_expression} THEN",
            f"        ERRO_{d_name(index)} := 1;",
            "    END_IF;",
            "END_IF;",
            "",
        ]

    falha_expression = " OR ".join([f"FALHA_{d_name(index)} = 1" for index in range(total_do)])
    erro_expression = " OR ".join([f"ERRO_{d_name(index)} = 1" for index in range(total_do)])
    ok_expression = " OR ".join([f"OK_{d_name(index)} = 1" for index in range(total_do)])

    lines += [
        "(* Resultado geral do teste *)",
        f"IF {falha_expression} THEN",
        "    TESTE_FALHA_GERAL := 1;",
        "END_IF;",
        "",
        f"IF {erro_expression} THEN",
        "    TESTE_ERRO_GERAL := 1;",
        "END_IF;",
        "",
        f"IF ({ok_expression}) AND (TESTE_FALHA_GERAL = 0) AND (TESTE_ERRO_GERAL = 0) THEN",
        "    TESTE_OK_GERAL := 1;",
        "END_IF;",
    ]

    return "\n".join(lines).rstrip() + "\n"
def generate_globals_csv(profile):
    total_do = int(profile["total_do"])

    rows = [
        "Nome; Classe; Tipo; Endereço; Dimensão; Valor Inicial; Opções; Descrição;",
    ]

    # Comandos antigos preservados para compatibilidade
    for index in range(total_do):
        rows.append(
            f"G_CMD_{d_name(index)}; Global; INT; %MW{index}; ; 0; publ; Comando antigo {d_name(index)};"
        )

    rows += [
        "MODO_AUTO; Global; INT; %MW10; ; 1; publ; Modo automatico habilitado;",
        "MODO_MANUAL; Global; INT; %MW11; ; 0; publ; Modo manual habilitado;",
    ]

    for index in range(total_do):
        rows.append(
            f"CMD_MANUAL_{d_name(index)}; Global; INT; %MW{12 + index}; ; 0; publ; Comando manual {d_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"RETORNO_{di_name(index)}; Global; INT; %MW{20 + index}; ; 0; publ; Retorno {di_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"OK_{d_name(index)}; Global; INT; %MW{30 + index}; ; 0; publ; Diagnostico OK {d_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"FALHA_{d_name(index)}; Global; INT; %MW{40 + index}; ; 0; publ; Falha {d_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"ERRO_{d_name(index)}; Global; INT; %MW{50 + index}; ; 0; publ; Retorno cruzado {d_name(index)};"
        )

    rows += [
        "TESTE_OK_GERAL; Global; INT; %MW60; ; 0; publ; Resultado geral OK;",
        "TESTE_FALHA_GERAL; Global; INT; %MW61; ; 0; publ; Resultado geral com falha;",
        "TESTE_ERRO_GERAL; Global; INT; %MW62; ; 0; publ; Resultado geral com erro de retorno cruzado;",
    ]

    return "\n".join(rows) + "\n"
def generate_program_vars_csv(profile):
    total_do = int(profile["total_do"])

    rows = [
        "Nome; Classe; Tipo; Endereço; Dimensão; Valor Inicial; Opções; Descrição;",
        "T_STEP; Local; TON; ; ; 0; publ; Temporizador do sequenciador;",
        "STEP_TESTE; Local; INT; ; ; 0; publ; Passo atual do teste;",
        "RESET_TIMER; Local; BOOL; ; ; FALSE; publ; Reset do temporizador;",
    ]

    for index in range(total_do):
        rows.append(
            f"COMANDO_{d_name(index)}; Local; INT; ; ; 0; publ; Comando interno {d_name(index)};"
        )

    rows += [
        "MODO_AUTO; Externa; INT; ; ; 0; publ; Modo automatico habilitado;",
        "MODO_MANUAL; Externa; INT; ; ; 0; publ; Modo manual habilitado;",
    ]

    for index in range(total_do):
        rows.append(
            f"CMD_MANUAL_{d_name(index)}; Externa; INT; ; ; 0; publ; Comando manual {d_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"RETORNO_{di_name(index)}; Externa; INT; ; ; 0; publ; Retorno {di_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"OK_{d_name(index)}; Externa; INT; ; ; 0; publ; Diagnostico OK {d_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"FALHA_{d_name(index)}; Externa; INT; ; ; 0; publ; Falha {d_name(index)};"
        )

    for index in range(total_do):
        rows.append(
            f"ERRO_{d_name(index)}; Externa; INT; ; ; 0; publ; Retorno cruzado {d_name(index)};"
        )

    rows += [
        "TESTE_OK_GERAL; Externa; INT; ; ; 0; publ; Resultado geral OK;",
        "TESTE_FALHA_GERAL; Externa; INT; ; ; 0; publ; Resultado geral com falha;",
        "TESTE_ERRO_GERAL; Externa; INT; ; ; 0; publ; Resultado geral com erro de retorno cruzado;",
    ]

    return "\n".join(rows) + "\n"
def generate_vars_doc(profile):
    lines = [
        "# Variáveis geradas - HIO115",
        "",
        f"Modelo: `{profile['modelo']}`",
        f"Descrição: {profile['descricao']}",
        "",
        "## Arquivos gerados",
        "",
        "- `src-st/generated/program_01_hio115_auto.st`",
        "- `src-st/generated/program_01_hio115_auto_manual.st`",
        "- `histudio-import/globais_completo_com_classe.csv`",
        "- `histudio-import/program_01_completo_com_classe.csv`",
        "",
        "## Regra importante do HIstudio",
        "",
        "Para o vínculo Global/Externa funcionar, o CSV precisa conter a coluna `Classe`.",
        "",
        "Nas variáveis globais:",
        "",
        "- `Classe = Global`",
        "",
        "No `PROGRAM_01`:",
        "",
        "- `Classe = Local` para variáveis internas",
        "- `Classe = Externa` para referências às globais",
        "",
        "## Mapa de saídas digitais",
        "",
        "| Canal | Nome lógico | SysVar |",
        "|---|---|---:|",
    ]

    for index, sysvar in enumerate(profile["do_sysvars"]):
        lines.append(f"| DO{index:02d} | {d_name(index)} | {sysvar} |")

    lines += [
        "",
        "## Mapa de entradas digitais usadas como retorno",
        "",
        "| Canal | Nome lógico | SysVar |",
        "|---|---|---:|",
    ]

    for index, sysvar in enumerate(profile["di_sysvars"]):
        lines.append(f"| DI{index:02d} | {di_name(index)} | {sysvar} |")

    lines += [
        "",
        "## Diagnósticos gerados",
        "",
        "| Variável | Função |",
        "|---|---|",
        "| `OK_Dxxx` | Indica que a saída acionada recebeu retorno correto e nenhum retorno cruzado. |",
        "| `FALHA_Dxxx` | Indica que a saída foi acionada, mas o retorno esperado não apareceu. |",
        "| `ERRO_Dxxx` | Indica que apareceu retorno em entrada diferente da esperada. |",
        "| `TESTE_OK_GERAL` | Indica que há canal OK e não há falha nem erro ativo. |",
        "| `TESTE_FALHA_GERAL` | Indica falha de retorno em pelo menos um canal ativo. |",
        "| `TESTE_ERRO_GERAL` | Indica retorno cruzado em pelo menos um canal ativo. |",
    ]

    return "\n".join(lines).rstrip() + "\n"
def main():
    profile = load_profile()

    for path in [
        OUT_ST_AUTO,
        OUT_ST_AUTO_MANUAL,
        OUT_VARS_DOC,
        OUT_GLOBALS_CSV,
        OUT_PROGRAM_CSV,
    ]:
        path.parent.mkdir(parents=True, exist_ok=True)

    OUT_ST_AUTO.write_text(generate_auto_st(profile), encoding="utf-8")
    OUT_ST_AUTO_MANUAL.write_text(generate_auto_manual_st(profile), encoding="utf-8")
    OUT_VARS_DOC.write_text(generate_vars_doc(profile), encoding="utf-8")

    # UTF-8 com BOM ajuda o HIstudio/Windows a reconhecer acentos corretamente.
    OUT_GLOBALS_CSV.write_text(generate_globals_csv(profile), encoding="utf-8-sig")
    OUT_PROGRAM_CSV.write_text(generate_program_vars_csv(profile), encoding="utf-8-sig")

    print(f"ST automatico gerado: {OUT_ST_AUTO}")
    print(f"ST automatico/manual gerado: {OUT_ST_AUTO_MANUAL}")
    print(f"CSV globais gerado: {OUT_GLOBALS_CSV}")
    print(f"CSV PROGRAM_01 gerado: {OUT_PROGRAM_CSV}")
    print(f"Documento de variaveis gerado: {OUT_VARS_DOC}")


if __name__ == "__main__":
    main()
