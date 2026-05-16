(* Sequenciador automatico HIO115 - referencia externa ao HIstudio *)

(* Variaveis locais esperadas no PROGRAM_01:
   T_STEP      : TON
   STEP_TESTE  : INT := 0
   RESET_TIMER : BOOL := FALSE
*)

T_STEP(IN := NOT RESET_TIMER, PT := T#2s);

IF T_STEP.Q THEN
    STEP_TESTE := STEP_TESTE + 1;

    IF STEP_TESTE > 4 THEN
        STEP_TESTE := 0;
    END_IF;

    RESET_TIMER := TRUE;
ELSE
    RESET_TIMER := FALSE;
END_IF;

(* Desliga todas as saidas *)
HILS.SET_SYSVAR(1128, 0);  (* DO00 *)
HILS.SET_SYSVAR(1129, 0);  (* DO01 *)
HILS.SET_SYSVAR(1130, 0);  (* DO02 *)
HILS.SET_SYSVAR(1131, 0);  (* DO03 *)

(* Liga uma saida por vez *)
IF STEP_TESTE = 1 THEN
    HILS.SET_SYSVAR(1128, 1);  (* DO00 *)
END_IF;

IF STEP_TESTE = 2 THEN
    HILS.SET_SYSVAR(1129, 1);  (* DO01 *)
END_IF;

IF STEP_TESTE = 3 THEN
    HILS.SET_SYSVAR(1130, 1);  (* DO02 *)
END_IF;

IF STEP_TESTE = 4 THEN
    HILS.SET_SYSVAR(1131, 1);  (* DO03 *)
END_IF;
