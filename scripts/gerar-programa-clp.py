import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROFILE_PATH = ROOT / "profiles" / "hio115.json"
OUT_ST = ROOT / "src-st" / "generated" / "program_01_hio115_auto.st"
OUT_VARS = ROOT / "docs" / "variaveis-geradas-hio115.md"


def load_profile():
    with PROFILE_PATH.open("r", encoding="utf-8") as f:
        return json.load(f)


def generate_st(profile):
    total_do = int(profile["total_do"])
    tempo = int(profile.get("tempo_passo_segundos", 2))
    do_sysvars = profile["do_sysvars"]

    lines = []
    lines.append("(* Programa gerado automaticamente para teste do HIO115 *)")
    lines.append("(* Nao editar diretamente no HIstudio sem atualizar o projeto Git *)")
    lines.append("")
    lines.append("(* Variaveis locais esperadas no PROGRAM_01: *)")
    lines.append("(* T_STEP      : TON  *)")
    lines.append("(* STEP_TESTE  : INT  valor inicial 0 *)")
    lines.append("(* RESET_TIMER : BOOL valor inicial FALSE *)")
    lines.append("")
    lines.append(f"T_STEP(IN := NOT RESET_TIMER, PT := T#{tempo}s);")
    lines.append("")
    lines.append("IF T_STEP.Q THEN")
    lines.append("    STEP_TESTE := STEP_TESTE + 1;")
    lines.append("")
    lines.append(f"    IF STEP_TESTE > {total_do} THEN")
    lines.append("        STEP_TESTE := 0;")
    lines.append("    END_IF;")
    lines.append("")
    lines.append("    RESET_TIMER := TRUE;")
    lines.append("ELSE")
    lines.append("    RESET_TIMER := FALSE;")
    lines.append("END_IF;")
    lines.append("")
    lines.append("(* Desliga todas as saidas digitais *)")

    for index, sysvar in enumerate(do_sysvars):
        lines.append(f"HILS.SET_SYSVAR({sysvar}, 0);  (* DO{index:02d} *)")

    lines.append("")
    lines.append("(* Liga uma saida por vez *)")

    for index, sysvar in enumerate(do_sysvars):
        step = index + 1
        lines.append(f"IF STEP_TESTE = {step} THEN")
        lines.append(f"    HILS.SET_SYSVAR({sysvar}, 1);  (* DO{index:02d} *)")
        lines.append("END_IF;")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def generate_vars_doc(profile):
    lines = []
    lines.append("# Variáveis geradas - HIO115")
    lines.append("")
    lines.append(f"Modelo: `{profile['modelo']}`")
    lines.append(f"Descrição: {profile['descricao']}")
    lines.append("")
    lines.append("## Variáveis locais necessárias no PROGRAM_01")
    lines.append("")
    lines.append("| Nome | Classe | Tipo | Valor inicial | Função |")
    lines.append("|---|---|---|---|---|")
    lines.append("| T_STEP | Local | TON | - | Temporizador do passo automático |")
    lines.append("| STEP_TESTE | Local | INT | 0 | Passo atual do teste |")
    lines.append("| RESET_TIMER | Local | BOOL | FALSE | Reset lógico do TON |")
    lines.append("")
    lines.append("## Mapa de saídas digitais")
    lines.append("")
    lines.append("| Canal | SysVar |")
    lines.append("|---|---:|")

    for index, sysvar in enumerate(profile["do_sysvars"]):
        lines.append(f"| DO{index:02d} | {sysvar} |")

    lines.append("")
    lines.append("## Mapa de entradas digitais")
    lines.append("")
    lines.append("| Canal | SysVar |")
    lines.append("|---|---:|")

    for index, sysvar in enumerate(profile["di_sysvars"]):
        lines.append(f"| DI{index:02d} | {sysvar} |")

    lines.append("")
    lines.append("## Mapa de entradas analógicas")
    lines.append("")
    lines.append("| Canal | SysVar |")
    lines.append("|---|---:|")

    for index, sysvar in enumerate(profile["ai_sysvars"]):
        lines.append(f"| AI{index:02d} | {sysvar} |")

    lines.append("")
    lines.append("## Próximo uso")
    lines.append("")
    lines.append("Copiar o conteúdo de `src-st/generated/program_01_hio115_auto.st` para a aba Código do `PROGRAM_01` no HIstudio.")
    lines.append("As variáveis locais devem ser criadas manualmente ou por automação futura.")

    return "\n".join(lines).rstrip() + "\n"


def main():
    profile = load_profile()

    OUT_ST.parent.mkdir(parents=True, exist_ok=True)
    OUT_VARS.parent.mkdir(parents=True, exist_ok=True)

    OUT_ST.write_text(generate_st(profile), encoding="utf-8")
    OUT_VARS.write_text(generate_vars_doc(profile), encoding="utf-8")

    print(f"ST gerado: {OUT_ST}")
    print(f"Documento de variaveis gerado: {OUT_VARS}")


if __name__ == "__main__":
    main()
