using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Verificacao nao visual do bridge de leitura desabilitado do host Layout 3.
///
/// Garante, sem abrir UI e sem qualquer transporte, que o bridge nasce e
/// permanece desligado: a unica implementacao (<see cref="Layout3DisabledReadBridge"/>)
/// produz snapshots locais com conexao ativa = false, leituras reais = 0,
/// escritas reais = 0 e comandos fisicos = 0 para cada perfil conhecido do
/// catalogo local. Nenhum cenario abre porta, fala protocolo de campo ou executa
/// comando fisico.
/// </summary>
internal static class Layout3ReadBridgeSafetyValidator
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

        output.WriteLine("Verificacao nao visual do bridge de leitura desabilitado (host Layout 3)");
        output.WriteLine("Escopo: catalogo local + selecao de perfil + bridge no-op desligado.");
        output.WriteLine("Sem conexao fisica, sem porta serial, sem TCP, sem Modbus, sem PLC, sem registrador.");
        output.WriteLine();

        bool isValid = true;
        int totalReads = 0;
        int totalWrites = 0;
        int totalPhysicalCommands = 0;
        int activeConnections = 0;

        ILayout3ReadBridge bridge = new Layout3DisabledReadBridge();

        // O bridge deve nascer desligado, antes de qualquer cenario.
        if (bridge.Status != Layout3ReadBridgeStatus.Disabled)
        {
            isValid = false;
            output.WriteLine($"[ERRO] Bridge nao iniciou desligado: status {bridge.Status}.");
        }

        foreach (Layout3HostReadOnlySafetyValidationScenario scenario in scenarios)
        {
            Layout3ReadBridgeSafetyValidationResult result = ValidateScenario(catalog, bridge, scenario);
            totalReads += result.RealReads;
            totalWrites += result.RealWrites;
            totalPhysicalCommands += result.PhysicalCommands;
            if (result.ConnectionActive)
            {
                activeConnections++;
            }

            output.WriteLine($"[{result.ResultLabel}] {result.ScenarioName}");
            output.WriteLine($"  Perfil (contexto).....: {result.ProfileContext}");
            output.WriteLine($"  Protocolo (catalogo)..: {result.ProtocolContext}");
            output.WriteLine($"  Bridge................: {result.BridgeStatus} ({result.BridgeMode})");
            output.WriteLine($"  Conexao ativa.........: {(result.ConnectionActive ? "SIM" : "nao")}");
            output.WriteLine($"  Leituras reais........: {result.RealReads}");
            output.WriteLine($"  Escritas reais........: {result.RealWrites}");
            output.WriteLine($"  Comandos fisicos......: {result.PhysicalCommands}");

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
        output.WriteLine("Conexoes ativas: 0");
        output.WriteLine("Leituras reais: 0");
        output.WriteLine("Escritas reais: 0");
        output.WriteLine("Comandos fisicos executados: 0");
        output.WriteLine(isValid ? "Resultado: OK" : "Resultado: ERRO");

        if (!isValid)
        {
            error.WriteLine("Verificacao do bridge de leitura desabilitado do host Layout 3 falhou.");
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

    private static Layout3ReadBridgeSafetyValidationResult ValidateScenario(
        HardwareCatalog catalog,
        ILayout3ReadBridge bridge,
        Layout3HostReadOnlySafetyValidationScenario scenario)
    {
        List<string> failures = [];

        Layout3ProfileSelection selection;
        Layout3ReadBridgeSnapshot snapshot;

        try
        {
            selection = ResolveSelection(catalog, scenario);
            snapshot = bridge.CreateSnapshot(selection);
        }
        catch (Exception ex)
        {
            return new Layout3ReadBridgeSafetyValidationResult(
                ScenarioName: scenario.Name,
                ProfileContext: "indisponivel",
                ProtocolContext: "indisponivel",
                BridgeStatus: "indisponivel",
                BridgeMode: "indisponivel",
                ConnectionActive: false,
                RealReads: 0,
                RealWrites: 0,
                PhysicalCommands: 0,
                Failures: [$"Cenario gerou excecao: {ex.GetType().Name}: {ex.Message}"]);
        }

        // O bridge deve permanecer desligado em qualquer selecao.
        if (snapshot.Status != Layout3ReadBridgeStatus.Disabled)
        {
            failures.Add($"Status do bridge deveria ser desligado; foi {snapshot.Status}.");
        }

        // Nunca pode indicar conexao ativa.
        if (snapshot.ConnectionActive)
        {
            failures.Add("Snapshot indicou conexao ativa; o bridge deveria estar desligado.");
        }

        // Contadores reais fixados em 0 por construcao.
        if (snapshot.RealReads != 0)
        {
            failures.Add($"RealReads deveria ser 0; foi {snapshot.RealReads}.");
        }

        if (snapshot.RealWrites != 0)
        {
            failures.Add($"RealWrites deveria ser 0; foi {snapshot.RealWrites}.");
        }

        if (snapshot.PhysicalCommands != 0)
        {
            failures.Add($"PhysicalCommands deveria ser 0; foi {snapshot.PhysicalCommands}.");
        }

        // Modo deve declarar a natureza desligada/no-op/local.
        if (!snapshot.Mode.Contains("disabled", StringComparison.OrdinalIgnoreCase)
            && !snapshot.Mode.Contains("no-op", StringComparison.OrdinalIgnoreCase)
            && !snapshot.Mode.Contains("local", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"Modo do bridge nao confirma natureza desligada/no-op/local: '{snapshot.Mode}'.");
        }

        // Mensagem deve confirmar bloqueio/desligamento, nunca canal aberto.
        if (string.IsNullOrWhiteSpace(snapshot.Message)
            || (!snapshot.Message.Contains("desligado", StringComparison.OrdinalIgnoreCase)
                && !snapshot.Message.Contains("bloquead", StringComparison.OrdinalIgnoreCase)))
        {
            failures.Add($"Mensagem do bridge nao confirma desligado/bloqueado: '{snapshot.Message}'.");
        }

        // Cenarios pendentes (referencia oficial) nao podem habilitar nada.
        if (scenario.PendingOfficialReference)
        {
            if (selection.Model is null)
            {
                failures.Add("Cenario de referencia oficial pendente nao resolveu o modelo esperado.");
            }

            if (snapshot.Status != Layout3ReadBridgeStatus.Disabled || snapshot.ConnectionActive)
            {
                failures.Add("Cenario pendente (NEON_5_CONTROLLER/RION_5_CONTROLLER) ativou o bridge.");
            }
        }

        return new Layout3ReadBridgeSafetyValidationResult(
            ScenarioName: scenario.Name,
            ProfileContext: snapshot.ProfileContext,
            ProtocolContext: snapshot.ProtocolContext,
            BridgeStatus: snapshot.StatusDisplayName,
            BridgeMode: snapshot.Mode,
            ConnectionActive: snapshot.ConnectionActive,
            RealReads: snapshot.RealReads,
            RealWrites: snapshot.RealWrites,
            PhysicalCommands: snapshot.PhysicalCommands,
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
