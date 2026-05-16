(* Programa HIO115 - modo automatico e modo manual *)
(* Gerado automaticamente por scripts/gerar-programa-clp.py *)

(* Leitura dos retornos digitais *)
RETORNO_DI00 := HILS.GET_SYSVAR(1120);  (* DI00 *)
RETORNO_DI01 := HILS.GET_SYSVAR(1121);  (* DI01 *)
RETORNO_DI02 := HILS.GET_SYSVAR(1122);  (* DI02 *)
RETORNO_DI03 := HILS.GET_SYSVAR(1123);  (* DI03 *)

(* Zera comandos internos *)
COMANDO_D000 := 0;
COMANDO_D001 := 0;
COMANDO_D002 := 0;
COMANDO_D003 := 0;

(* Modo automatico *)
IF MODO_AUTO = 1 THEN

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

    IF STEP_TESTE = 1 THEN
        COMANDO_D000 := 1;
    END_IF;

    IF STEP_TESTE = 2 THEN
        COMANDO_D001 := 1;
    END_IF;

    IF STEP_TESTE = 3 THEN
        COMANDO_D002 := 1;
    END_IF;

    IF STEP_TESTE = 4 THEN
        COMANDO_D003 := 1;
    END_IF;

END_IF;

(* Modo manual *)
IF MODO_MANUAL = 1 THEN
    COMANDO_D000 := CMD_MANUAL_D000;
    COMANDO_D001 := CMD_MANUAL_D001;
    COMANDO_D002 := CMD_MANUAL_D002;
    COMANDO_D003 := CMD_MANUAL_D003;
END_IF;

(* Escrita nas saidas digitais *)
HILS.SET_SYSVAR(1128, COMANDO_D000);  (* D000 / DO00 *)
HILS.SET_SYSVAR(1129, COMANDO_D001);  (* D001 / DO01 *)
HILS.SET_SYSVAR(1130, COMANDO_D002);  (* D002 / DO02 *)
HILS.SET_SYSVAR(1131, COMANDO_D003);  (* D003 / DO03 *)

(* Zera diagnosticos *)
OK_D000 := 0;
OK_D001 := 0;
OK_D002 := 0;
OK_D003 := 0;

FALHA_D000 := 0;
FALHA_D001 := 0;
FALHA_D002 := 0;
FALHA_D003 := 0;

(* Diagnostico simples de retorno *)
IF COMANDO_D000 = 1 THEN
    IF RETORNO_DI00 = 1 THEN
        OK_D000 := 1;
    ELSE
        FALHA_D000 := 1;
    END_IF;
END_IF;

IF COMANDO_D001 = 1 THEN
    IF RETORNO_DI01 = 1 THEN
        OK_D001 := 1;
    ELSE
        FALHA_D001 := 1;
    END_IF;
END_IF;

IF COMANDO_D002 = 1 THEN
    IF RETORNO_DI02 = 1 THEN
        OK_D002 := 1;
    ELSE
        FALHA_D002 := 1;
    END_IF;
END_IF;

IF COMANDO_D003 = 1 THEN
    IF RETORNO_DI03 = 1 THEN
        OK_D003 := 1;
    ELSE
        FALHA_D003 := 1;
    END_IF;
END_IF;
