(* Programa gerado automaticamente para teste automatico do HIO115 *)
(* Nao editar diretamente no HIstudio sem atualizar o projeto Git *)

(* Variaveis locais esperadas no PROGRAM_01: *)
(* T_STEP      : TON  *)
(* STEP_TESTE  : INT  valor inicial 0 *)
(* RESET_TIMER : BOOL valor inicial FALSE *)

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

(* Desliga todas as saidas digitais *)
HILS.SET_SYSVAR(1128, 0);  (* D000 / DO00 *)
HILS.SET_SYSVAR(1129, 0);  (* D001 / DO01 *)
HILS.SET_SYSVAR(1130, 0);  (* D002 / DO02 *)
HILS.SET_SYSVAR(1131, 0);  (* D003 / DO03 *)

(* Liga uma saida por vez *)
IF STEP_TESTE = 1 THEN
    HILS.SET_SYSVAR(1128, 1);  (* D000 / DO00 *)
END_IF;

IF STEP_TESTE = 2 THEN
    HILS.SET_SYSVAR(1129, 1);  (* D001 / DO01 *)
END_IF;

IF STEP_TESTE = 3 THEN
    HILS.SET_SYSVAR(1130, 1);  (* D002 / DO02 *)
END_IF;

IF STEP_TESTE = 4 THEN
    HILS.SET_SYSVAR(1131, 1);  (* D003 / DO03 *)
END_IF;
