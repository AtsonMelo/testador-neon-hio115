(*
    Projeto: Testador CLP Neon / HIO115
    Arquivo: hio115_teste_base.st

    Objetivo:
    Testar automaticamente as saídas digitais D000 a D003
    e validar retorno nas entradas digitais DI00 a DI03.

    Mapa validado:
    D000 / DO00 -> Relé 00 -> DI00
    D001 / DO01 -> Relé 01 -> DI01
    D002 / DO02 -> Relé 02 -> DI02
    D003 / DO03 -> Relé 03 -> DI03

    Observação:
    As variáveis externas devem estar vinculadas às SysVars do HIO115
    conforme importação/configuração no HIstudio.
*)

VAR_EXTERNAL
    // Saídas digitais HIO115
    D000 : BOOL;
    D001 : BOOL;
    D002 : BOOL;
    D003 : BOOL;

    // Entradas digitais de retorno HIO115
    DI00 : BOOL;
    DI01 : BOOL;
    DI02 : BOOL;
    DI03 : BOOL;
END_VAR

VAR
    // Controle geral
    Teste_Iniciar      : BOOL;
    Teste_Parar        : BOOL;
    Teste_EmExecucao   : BOOL;
    Teste_Finalizado   : BOOL;

    // Etapa atual
    STEP_TESTE         : INT;

    // Temporizador da etapa
    TempoEtapa         : TON;

    // Resultados por canal
    OK_DO00            : BOOL;
    OK_DO01            : BOOL;
    OK_DO02            : BOOL;
    OK_DO03            : BOOL;

    FALHA_DO00         : BOOL;
    FALHA_DO01         : BOOL;
    FALHA_DO02         : BOOL;
    FALHA_DO03         : BOOL;

    ERRO_DO00          : BOOL;
    ERRO_DO01          : BOOL;
    ERRO_DO02          : BOOL;
    ERRO_DO03          : BOOL;

    // Resultado geral
    TESTE_OK_GERAL     : BOOL;
    TESTE_FALHA_GERAL  : BOOL;
    TESTE_ERRO_GERAL   : BOOL;
END_VAR


// Parada geral
IF Teste_Parar THEN
    Teste_EmExecucao := FALSE;
    Teste_Finalizado := FALSE;
    STEP_TESTE := 0;

    D000 := FALSE;
    D001 := FALSE;
    D002 := FALSE;
    D003 := FALSE;

    TempoEtapa(IN := FALSE);
END_IF;


// Início do teste automático
IF Teste_Iniciar AND NOT Teste_EmExecucao THEN
    Teste_EmExecucao := TRUE;
    Teste_Finalizado := FALSE;
    STEP_TESTE := 1;

    OK_DO00 := FALSE;
    OK_DO01 := FALSE;
    OK_DO02 := FALSE;
    OK_DO03 := FALSE;

    FALHA_DO00 := FALSE;
    FALHA_DO01 := FALSE;
    FALHA_DO02 := FALSE;
    FALHA_DO03 := FALSE;

    ERRO_DO00 := FALSE;
    ERRO_DO01 := FALSE;
    ERRO_DO02 := FALSE;
    ERRO_DO03 := FALSE;

    TESTE_OK_GERAL := FALSE;
    TESTE_FALHA_GERAL := FALSE;
    TESTE_ERRO_GERAL := FALSE;
END_IF;


// Sequência automática
IF Teste_EmExecucao THEN

    CASE STEP_TESTE OF

        1:
            // Teste D000 / DO00 -> DI00
            D000 := TRUE;
            D001 := FALSE;
            D002 := FALSE;
            D003 := FALSE;

            TempoEtapa(IN := TRUE, PT := T#1s);

            IF TempoEtapa.Q THEN
                OK_DO00 := DI00 AND NOT (DI01 OR DI02 OR DI03);
                FALHA_DO00 := NOT DI00;
                ERRO_DO00 := DI01 OR DI02 OR DI03;

                TempoEtapa(IN := FALSE);
                STEP_TESTE := 2;
            END_IF;

        2:
            // Teste D001 / DO01 -> DI01
            D000 := FALSE;
            D001 := TRUE;
            D002 := FALSE;
            D003 := FALSE;

            TempoEtapa(IN := TRUE, PT := T#1s);

            IF TempoEtapa.Q THEN
                OK_DO01 := DI01 AND NOT (DI00 OR DI02 OR DI03);
                FALHA_DO01 := NOT DI01;
                ERRO_DO01 := DI00 OR DI02 OR DI03;

                TempoEtapa(IN := FALSE);
                STEP_TESTE := 3;
            END_IF;

        3:
            // Teste D002 / DO02 -> DI02
            D000 := FALSE;
            D001 := FALSE;
            D002 := TRUE;
            D003 := FALSE;

            TempoEtapa(IN := TRUE, PT := T#1s);

            IF TempoEtapa.Q THEN
                OK_DO02 := DI02 AND NOT (DI00 OR DI01 OR DI03);
                FALHA_DO02 := NOT DI02;
                ERRO_DO02 := DI00 OR DI01 OR DI03;

                TempoEtapa(IN := FALSE);
                STEP_TESTE := 4;
            END_IF;

        4:
            // Teste D003 / DO03 -> DI03
            D000 := FALSE;
            D001 := FALSE;
            D002 := FALSE;
            D003 := TRUE;

            TempoEtapa(IN := TRUE, PT := T#1s);

            IF TempoEtapa.Q THEN
                OK_DO03 := DI03 AND NOT (DI00 OR DI01 OR DI02);
                FALHA_DO03 := NOT DI03;
                ERRO_DO03 := DI00 OR DI01 OR DI02;

                TempoEtapa(IN := FALSE);
                STEP_TESTE := 100;
            END_IF;

        100:
            // Finalização
            D000 := FALSE;
            D001 := FALSE;
            D002 := FALSE;
            D003 := FALSE;

            TESTE_FALHA_GERAL := FALHA_DO00 OR FALHA_DO01 OR FALHA_DO02 OR FALHA_DO03;
            TESTE_ERRO_GERAL := ERRO_DO00 OR ERRO_DO01 OR ERRO_DO02 OR ERRO_DO03;
            TESTE_OK_GERAL := NOT TESTE_FALHA_GERAL AND NOT TESTE_ERRO_GERAL;

            Teste_EmExecucao := FALSE;
            Teste_Finalizado := TRUE;
            STEP_TESTE := 0;

        ELSE
            STEP_TESTE := 0;

    END_CASE;

END_IF;
