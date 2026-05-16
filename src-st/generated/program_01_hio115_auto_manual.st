(* Programa gerado - HIO115 com modo automatico e modo manual *)

(* Variaveis esperadas no PROGRAM_01:

T_STEP            : TON
STEP_TESTE        : INT   inicial 0
RESET_TIMER       : BOOL  inicial FALSE

MODO_AUTO         : INT   inicial 1
MODO_MANUAL       : INT   inicial 0

CMD_MANUAL_DO00   : INT   inicial 0
CMD_MANUAL_DO01   : INT   inicial 0
CMD_MANUAL_DO02   : INT   inicial 0
CMD_MANUAL_DO03   : INT   inicial 0

COMANDO_DO00      : INT   inicial 0
COMANDO_DO01      : INT   inicial 0
COMANDO_DO02      : INT   inicial 0
COMANDO_DO03      : INT   inicial 0

RETORNO_DI00      : INT   inicial 0
RETORNO_DI01      : INT   inicial 0
RETORNO_DI02      : INT   inicial 0
RETORNO_DI03      : INT   inicial 0

OK_DO00           : INT   inicial 0
OK_DO01           : INT   inicial 0
OK_DO02           : INT   inicial 0
OK_DO03           : INT   inicial 0

FALHA_DO00        : INT   inicial 0
FALHA_DO01        : INT   inicial 0
FALHA_DO02        : INT   inicial 0
FALHA_DO03        : INT   inicial 0
*)

(* Leitura dos retornos digitais *)
RETORNO_DI00 := HILS.GET_SYSVAR(1120);  (* DI00 *)
RETORNO_DI01 := HILS.GET_SYSVAR(1121);  (* DI01 *)
RETORNO_DI02 := HILS.GET_SYSVAR(1122);  (* DI02 *)
RETORNO_DI03 := HILS.GET_SYSVAR(1123);  (* DI03 *)

(* Zera comandos internos *)
COMANDO_DO00 := 0;
COMANDO_DO01 := 0;
COMANDO_DO02 := 0;
COMANDO_DO03 := 0;

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
        COMANDO_DO00 := 1;
    END_IF;

    IF STEP_TESTE = 2 THEN
        COMANDO_DO01 := 1;
    END_IF;

    IF STEP_TESTE = 3 THEN
        COMANDO_DO02 := 1;
    END_IF;

    IF STEP_TESTE = 4 THEN
        COMANDO_DO03 := 1;
    END_IF;

END_IF;

(* Modo manual *)
IF MODO_MANUAL = 1 THEN
    COMANDO_DO00 := CMD_MANUAL_DO00;
    COMANDO_DO01 := CMD_MANUAL_DO01;
    COMANDO_DO02 := CMD_MANUAL_DO02;
    COMANDO_DO03 := CMD_MANUAL_DO03;
END_IF;

(* Escrita nas saidas digitais *)
HILS.SET_SYSVAR(1128, COMANDO_DO00);  (* DO00 *)
HILS.SET_SYSVAR(1129, COMANDO_DO01);  (* DO01 *)
HILS.SET_SYSVAR(1130, COMANDO_DO02);  (* DO02 *)
HILS.SET_SYSVAR(1131, COMANDO_DO03);  (* DO03 *)

(* Zera diagnosticos *)
OK_DO00 := 0;
OK_DO01 := 0;
OK_DO02 := 0;
OK_DO03 := 0;

FALHA_DO00 := 0;
FALHA_DO01 := 0;
FALHA_DO02 := 0;
FALHA_DO03 := 0;

(* Diagnostico simples de retorno *)
IF COMANDO_DO00 = 1 THEN
    IF RETORNO_DI00 = 1 THEN
        OK_DO00 := 1;
    ELSE
        FALHA_DO00 := 1;
    END_IF;
END_IF;

IF COMANDO_DO01 = 1 THEN
    IF RETORNO_DI01 = 1 THEN
        OK_DO01 := 1;
    ELSE
        FALHA_DO01 := 1;
    END_IF;
END_IF;

IF COMANDO_DO02 = 1 THEN
    IF RETORNO_DI02 = 1 THEN
        OK_DO02 := 1;
    ELSE
        FALHA_DO02 := 1;
    END_IF;
END_IF;

IF COMANDO_DO03 = 1 THEN
    IF RETORNO_DI03 = 1 THEN
        OK_DO03 := 1;
    ELSE
        FALHA_DO03 := 1;
    END_IF;
END_IF;
