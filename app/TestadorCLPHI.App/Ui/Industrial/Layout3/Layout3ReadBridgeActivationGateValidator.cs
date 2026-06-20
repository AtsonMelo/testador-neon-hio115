using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Verificacao nao visual do gate de ativacao bloqueado do host Layout 3.
///
/// Garante, sem abrir UI e sem qualquer transporte, que o gate nasce e permanece
/// bloqueado: a unica implementacao (<see cref="Layout3BlockedReadBridgeActivationGate"/>)
/// produz decisoes locais com ativacao liberada = false, conexao ativa = false,
/// leituras reais = 0, escritas reais = 0 e comandos fisicos = 0 para cada perfil
/// conhecido do catalogo local, sempre com pelo menos um requisito futuro pendente
/// e nenhum requisito atendido. Nenhum cenario abre porta, fala protocolo de campo
/// ou executa comando fisico. Criterio central: nenhum perfil pode liberar ativacao.
/// </summary>
internal static class Layout3ReadBridgeActivationGateValidator
{
    private static readonly string[] PendingOfficialReferenceModels =
    [
        "NEON_5_CONTROLLER",
        "RION_5_CONTROLLER"
    ];

    public static int ValidateDefaultCatalog(TextWriter output, TextWriter error)
    {
        try
        {
            HardwareCatalog catalog = HardwareCatalogLoader.LoadDefault();
            HardwareCatalogValidationResult catalogValidation = HardwareCatalogValidator.Validate(catalog);

            if (!catalogValidation.IsValid)
            {
                error.WriteLine("Catalogo de hardware invalido.");
                error.WriteLine(catalogValidation.ToDisplayText());
                return 1;
            }

            return Validate(catalog, output, error);
        }
        catch (Exception ex)
        {
            error.WriteLine("Falha ao carregar catalogo de hardware.");
            error.WriteLine(ex.Message);
            return 1;
        }
    }

    public static int Validate(HardwareCatalog catalog, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        IReadOnlyList<Layout3HostReadOnlySafetyValidationScenario> scenarios = BuildScenarios(catalog);

        output.WriteLine("Verificacao nao visual do gate de ativacao bloqueado (host Layout 3)");
        output.WriteLine("Escopo: catalogo local + selecao de perfil + gate de ativacao sempre bloqueado.");
        output.WriteLine("Sem conexao fisica, sem porta serial, sem TCP, sem Modbus, sem PLC, sem registrador.");
        output.WriteLine();

        bool isValid = true;
        int totalReads = 0;
        int totalWrites = 0;
        int totalPhysicalCommands = 0;
        int activeConnections = 0;
        int activationsAllowed = 0;

        ILayout3ReadBridgeActivationGate gate = new Layout3BlockedReadBridgeActivationGate();

        // O gate deve nascer bloqueado, antes de qualquer cenario.
        if (gate.Status != Layout3ReadBridgeActivationStatus.Blocked)
        {
            isValid = false;
            output.WriteLine($"[ERRO] Gate nao iniciou bloqueado: status {gate.Status}.");
        }

        foreach (Layout3HostReadOnlySafetyValidationScenario scenario in scenarios)
        {
            Layout3ReadBridgeActivationGateValidationResult result = ValidateScenario(catalog, gate, scenario);
            totalReads += result.RealReads;
            totalWrites += result.RealWrites;
            totalPhysicalCommands += result.PhysicalCommands;
            if (result.ConnectionActive)
            {
                activeConnections++;
            }

            if (result.ActivationAllowed)
            {
                activationsAllowed++;
            }

            output.WriteLine($"[{result.ResultLabel}] {result.ScenarioName}");
            output.WriteLine($"  Perfil (contexto).....: {result.ProfileContext}");
            output.WriteLine($"  Protocolo (catalogo)..: {result.ProtocolContext}");
            output.WriteLine($"  Gate..................: {result.GateStatus}");
            output.WriteLine($"  Ativacao liberada.....: {(result.ActivationAllowed ? "SIM" : "nao")}");
            output.WriteLine($"  Conexao ativa.........: {(result.ConnectionActive ? "SIM" : "nao")}");
            output.WriteLine($"  Leituras reais........: {result.RealReads}");
            output.WriteLine($"  Escritas reais........: {result.RealWrites}");
            output.WriteLine($"  Comandos fisicos......: {result.PhysicalCommands}");
            output.WriteLine($"  Requisitos pendentes..: {result.PendingRequirements}");
            output.WriteLine($"  Requisitos atendidos..: {result.SatisfiedRequirements}");

            if (!result.IsSafe)
            {
                isValid = false;
                foreach (string failure in result.Failures)
                {
                    output.WriteLine($"  - {failure}");
                }
            }

            output.WriteLine();
        }

        if (activationsAllowed != 0)
        {
            isValid = false;
            output.WriteLine($"[ERRO] Cenarios que liberaram ativacao: {activationsAllowed}.");
        }

        if (activeConnections != 0)
        {
            isValid = false;
            output.WriteLine($"[ERRO] Cenarios indicando conexao ativa: {activeConnections}.");
        }

        if (totalReads != 0 || totalWrites != 0 || totalPhysicalCommands != 0)
        {
            isValid = false;
            output.WriteLine(
                $"[ERRO] Totais reais diferentes de 0 (leituras {totalReads}, " +
                $"escritas {totalWrites}, comandos fisicos {totalPhysicalCommands}).");
        }

        output.WriteLine($"Cenarios verificados: {scenarios.Count}");
        output.WriteLine("Ativacoes liberadas: 0");
        output.WriteLine("Conexoes ativas: 0");
        output.WriteLine("Leituras reais: 0");
        output.WriteLine("Escritas reais: 0");
        output.WriteLine("Comandos fisicos executados: 0");
        output.WriteLine(isValid ? "Resultado: OK" : "Resultado: ERRO");

        if (!isValid)
        {
            error.WriteLine("Verificacao do gate de ativacao bloqueado do host Layout 3 falhou.");
        }

        return isValid ? 0 : 1;
    }

    private static IReadOnlyList<Layout3HostReadOnlySafetyValidationScenario> BuildScenarios(HardwareCatalog catalog)
    {
        List<Layout3HostReadOnlySafetyValidationScenario> scenarios = [];

        foreach (HardwareFamily family in catalog.Families.OrderBy(item => item.DisplayName))
        {
            Layout3ProfileSelection selection = Layout3ProfileSelection.ForFamily(catalog, family);
            bool pending = selection.Model is not null
                && PendingOfficialReferenceModels.Any(id =>
                    string.Equals(id, selection.Model.Id, StringComparison.OrdinalIgnoreCase));

            scenarios.Add(new Layout3HostReadOnlySafetyValidationScenario(
                Name: $"{family.DisplayName} (perfil padrao do catalogo)",
                FamilyId: family.Id,
                ExpectsHardware: true,
                PendingOfficialReference: pending));
        }

        scenarios.Add(Layout3HostReadOnlySafetyValidationScenario.NoHardware);
        return scenarios;
    }

    private static Layout3ReadBridgeActivationGateValidationResult ValidateScenario(
        HardwareCatalog catalog,
        ILayout3ReadBridgeActivationGate gate,
        Layout3HostReadOnlySafetyValidationScenario scenario)
    {
        List<string> failures = [];

        Layout3ProfileSelection selection;
        Layout3ReadBridgeActivationDecision decision;

        try
        {
            selection = ResolveSelection(catalog, scenario);
            decision = gate.Evaluate(selection);
        }
        catch (Exception ex)
        {
            return new Layout3ReadBridgeActivationGateValidationResult(
                ScenarioName: scenario.Name,
                ProfileContext: "indisponivel",
                ProtocolContext: "indisponivel",
                GateStatus: "indisponivel",
                ActivationAllowed: false,
                ConnectionActive: false,
                RealReads: 0,
                RealWrites: 0,
                PhysicalCommands: 0,
                PendingRequirements: 0,
                SatisfiedRequirements: 0,
                Failures: [$"Cenario gerou excecao: {ex.GetType().Name}: {ex.Message}"]);
        }

        // O gate deve permanecer bloqueado em qualquer selecao.
        if (decision.Status != Layout3ReadBridgeActivationStatus.Blocked)
        {
            failures.Add($"Status do gate deveria ser bloqueado; foi {decision.Status}.");
        }

        // Nenhum perfil pode liberar ativacao real.
        if (decision.ActivationAllowed)
        {
            failures.Add("Decisao liberou ativacao; o gate deveria estar bloqueado.");
        }

        // Nunca pode indicar conexao ativa.
        if (decision.ConnectionActive)
        {
            failures.Add("Decisao indicou conexao ativa; o gate deveria estar bloqueado.");
        }

        // Contadores reais fixados em 0 por construcao.
        if (decision.RealReads != 0)
        {
            failures.Add($"RealReads deveria ser 0; foi {decision.RealReads}.");
        }

        if (decision.RealWrites != 0)
        {
            failures.Add($"RealWrites deveria ser 0; foi {decision.RealWrites}.");
        }

        if (decision.PhysicalCommands != 0)
        {
            failures.Add($"PhysicalCommands deveria ser 0; foi {decision.PhysicalCommands}.");
        }

        // Deve existir pelo menos um requisito futuro e nenhum pode estar atendido.
        if (decision.Requirements.Count == 0)
        {
            failures.Add("Decisao nao listou requisitos futuros pendentes.");
        }

        if (decision.PendingRequirements == 0)
        {
            failures.Add("Decisao deveria manter requisitos futuros pendentes; nenhum estava pendente.");
        }

        if (decision.SatisfiedRequirements != 0)
        {
            failures.Add(
                $"Nenhum requisito poderia estar atendido nesta fase; atendidos {decision.SatisfiedRequirements}.");
        }

        // Mensagem deve confirmar bloqueio, nunca ativacao concedida.
        if (string.IsNullOrWhiteSpace(decision.Message)
            || !decision.Message.Contains("bloquead", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"Mensagem do gate nao confirma bloqueio: '{decision.Message}'.");
        }

        // Cenarios pendentes (referencia oficial) nao podem habilitar nada.
        if (scenario.PendingOfficialReference)
        {
            if (selection.Model is null)
            {
                failures.Add("Cenario de referencia oficial pendente nao resolveu o modelo esperado.");
            }

            if (decision.Status != Layout3ReadBridgeActivationStatus.Blocked
                || decision.ActivationAllowed
                || decision.ConnectionActive)
            {
                failures.Add("Cenario pendente (NEON_5_CONTROLLER/RION_5_CONTROLLER) liberou o gate.");
            }
        }

        return new Layout3ReadBridgeActivationGateValidationResult(
            ScenarioName: scenario.Name,
            ProfileContext: decision.ProfileContext,
            ProtocolContext: decision.ProtocolContext,
            GateStatus: decision.StatusDisplayName,
            ActivationAllowed: decision.ActivationAllowed,
            ConnectionActive: decision.ConnectionActive,
            RealReads: decision.RealReads,
            RealWrites: decision.RealWrites,
            PhysicalCommands: decision.PhysicalCommands,
            PendingRequirements: decision.PendingRequirements,
            SatisfiedRequirements: decision.SatisfiedRequirements,
            Failures: failures);
    }

    private static Layout3ProfileSelection ResolveSelection(
        HardwareCatalog catalog,
        Layout3HostReadOnlySafetyValidationScenario scenario)
    {
        if (scenario.IsNoHardware)
        {
            return Layout3ProfileSelection.Empty;
        }

        HardwareFamily? family = catalog.FindFamily(scenario.FamilyId!);
        return Layout3ProfileSelection.ForFamily(catalog, family);
    }
}
