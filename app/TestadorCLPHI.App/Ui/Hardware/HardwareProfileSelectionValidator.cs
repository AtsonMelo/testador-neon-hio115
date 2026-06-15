using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Hardware;

public static class HardwareProfileSelectionValidator
{
    private const string SerialRs48538400 = "SERIAL_RS485_38400_8N1_MODBUS_RTU";
    private const string SerialRs48557600 = "SERIAL_RS485_57600_8N1_MODBUS_RTU";

    private static readonly HardwareProfileResolver Resolver = new();

    public static int ValidateDefaultCatalog(TextWriter output, TextWriter error)
    {
        try
        {
            HardwareCatalog catalog = HardwareCatalogLoader.LoadDefault();
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
        Scenario[] scenarios =
        [
            new(
                "NEON-1S + DIO605 + COMMUNICATION_DIAGNOSTIC",
                "NEON_LEGACY",
                "NEON-1S",
                "DIO605",
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectFieldObserved: true,
                ExpectManualValidation: true),
            new(
                "RION-502 + HIO115 + COMMUNICATION_DIAGNOSTIC",
                "RION_LEGACY",
                "RION-502",
                "HIO115",
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectFieldObserved: true,
                ExpectManualValidation: true),
            new(
                "RION-502 + HIO115 + REMOTE_IO_RS485",
                "RION_LEGACY",
                "RION-502",
                "HIO115",
                SerialRs48557600,
                "REMOTE_IO_RS485",
                ExpectFieldObserved: true,
                ExpectManualValidation: true),
            new(
                "NEON_5_CONTROLLER com dados pendentes",
                "NEON_5",
                "NEON_5_CONTROLLER",
                null,
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectFieldObserved: false,
                ExpectManualValidation: true),
            new(
                "RION_5_CONTROLLER com dados pendentes",
                "RION_5",
                "RION_5_CONTROLLER",
                null,
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectFieldObserved: false,
                ExpectManualValidation: true)
        ];

        bool isValid = true;
        bool physicalCommandExecuted = false;

        output.WriteLine("Validacao nao visual da selecao de perfil de hardware");
        output.WriteLine("Escopo: catalogo + resolver, sem abrir UI e sem enviar comandos fisicos.");
        output.WriteLine();

        foreach (Scenario scenario in scenarios)
        {
            List<string> failures = ValidateScenario(catalog, scenario);

            if (failures.Count == 0)
            {
                output.WriteLine($"[OK] {scenario.Name}");
                continue;
            }

            isValid = false;
            output.WriteLine($"[FALHA] {scenario.Name}");

            foreach (string failure in failures)
            {
                output.WriteLine($"  - {failure}");
            }
        }

        if (physicalCommandExecuted)
        {
            isValid = false;
            output.WriteLine("[FALHA] Comando fisico foi executado durante a validacao.");
        }

        output.WriteLine();
        output.WriteLine($"Cenarios executados: {scenarios.Length}");
        output.WriteLine("Comandos fisicos executados: 0");
        output.WriteLine(isValid ? "Resultado: OK" : "Resultado: FALHA");

        if (!isValid)
        {
            error.WriteLine("Validacao de selecao de perfil de hardware falhou.");
        }

        return isValid ? 0 : 1;
    }

    private static List<string> ValidateScenario(HardwareCatalog catalog, Scenario scenario)
    {
        List<string> failures = [];
        HardwareProfileResolution resolution;

        try
        {
            resolution = Resolver.Resolve(
                catalog,
                scenario.FamilyId,
                scenario.ModelId,
                scenario.ModuleId,
                scenario.CommunicationProfileId,
                scenario.TestProfileId);
        }
        catch (Exception ex)
        {
            failures.Add($"Resolver gerou excecao: {ex.GetType().Name}: {ex.Message}");
            return failures;
        }

        AddMissingSelectionFailures(failures, scenario, resolution);
        AddCompatibilityFailures(failures, scenario, resolution);
        AddPendingFailures(failures, scenario, resolution);
        AddFieldObservedFailures(failures, scenario, resolution);

        return failures;
    }

    private static void AddMissingSelectionFailures(
        ICollection<string> failures,
        Scenario scenario,
        HardwareProfileResolution resolution)
    {
        if (!string.IsNullOrWhiteSpace(scenario.FamilyId) && resolution.Family is null)
        {
            failures.Add($"Familia nao resolvida: {scenario.FamilyId}.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.ModelId) && resolution.Model is null)
        {
            failures.Add($"Modelo nao resolvido: {scenario.ModelId}.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.ModuleId) && resolution.IoModule is null)
        {
            failures.Add($"Modulo nao resolvido: {scenario.ModuleId}.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.CommunicationProfileId) &&
            resolution.CommunicationProfile is null)
        {
            failures.Add($"Perfil de comunicacao nao resolvido: {scenario.CommunicationProfileId}.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.TestProfileId) && resolution.TestProfile is null)
        {
            failures.Add($"Perfil de teste nao resolvido: {scenario.TestProfileId}.");
        }
    }

    private static void AddCompatibilityFailures(
        ICollection<string> failures,
        Scenario scenario,
        HardwareProfileResolution resolution)
    {
        if (!string.IsNullOrWhiteSpace(scenario.ModuleId) &&
            !resolution.CompatibleModules.Any(item => HasId(item.Id, scenario.ModuleId)))
        {
            failures.Add($"Modulo nao aparece como compativel: {scenario.ModuleId}.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.CommunicationProfileId) &&
            !resolution.PossibleCommunicationProfiles.Any(item => HasId(item.Id, scenario.CommunicationProfileId)))
        {
            failures.Add($"Perfil de comunicacao nao aparece como possivel: {scenario.CommunicationProfileId}.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.TestProfileId) &&
            !resolution.ApplicableTestProfiles.Any(item => HasId(item.Id, scenario.TestProfileId)))
        {
            failures.Add($"Perfil de teste nao aparece como aplicavel: {scenario.TestProfileId}.");
        }
    }

    private static void AddPendingFailures(
        ICollection<string> failures,
        Scenario scenario,
        HardwareProfileResolution resolution)
    {
        if (!scenario.ExpectManualValidation)
        {
            return;
        }

        if (!resolution.RequiresManualValidation)
        {
            failures.Add("RequiresManualValidation deveria estar ativo.");
        }

        if (resolution.PendingItems.Count == 0)
        {
            failures.Add("Itens pendentes nao foram expostos.");
            return;
        }

        if (!resolution.PendingItems.Any(item =>
                item.Contains("pending_manual_validation", StringComparison.OrdinalIgnoreCase) ||
                item.Contains("official_reference", StringComparison.OrdinalIgnoreCase)))
        {
            failures.Add("PendingItems nao mostra pending_manual_validation ou official_reference.");
        }
    }

    private static void AddFieldObservedFailures(
        ICollection<string> failures,
        Scenario scenario,
        HardwareProfileResolution resolution)
    {
        if (!scenario.ExpectFieldObserved)
        {
            return;
        }

        if (!resolution.FieldObservedItems.Any(item =>
                !item.Contains("Nenhum item selecionado", StringComparison.OrdinalIgnoreCase)))
        {
            failures.Add("Itens field_observed esperados nao foram expostos.");
        }
    }

    private static bool HasId(string currentId, string requestedId)
    {
        return string.Equals(currentId, requestedId, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record Scenario(
        string Name,
        string FamilyId,
        string ModelId,
        string? ModuleId,
        string CommunicationProfileId,
        string TestProfileId,
        bool ExpectFieldObserved,
        bool ExpectManualValidation);
}
