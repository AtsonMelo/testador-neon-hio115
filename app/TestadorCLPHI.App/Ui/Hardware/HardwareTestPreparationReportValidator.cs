using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Hardware;

public static class HardwareTestPreparationReportValidator
{
    private const string SerialRs48538400 = "SERIAL_RS485_38400_8N1_MODBUS_RTU";
    private const string SerialRs48557600 = "SERIAL_RS485_57600_8N1_MODBUS_RTU";
    private const string NoPhysicalCommandWarning = "nao executam comandos fisicos";
    private const string BenchValidationWarning = "Validacao em bancada ainda e obrigatoria";
    private const string ComConflictWarning = "Conflito de COM";
    private const string HistudioConflictWarning = "HIstudio, XCTU e Testador";

    private static readonly HardwareProfileResolver Resolver = new();
    private static readonly HardwareTestPreparationReportFormatter Formatter = new();

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
        Scenario[] scenarios =
        [
            new(
                "NEON-1S + DIO605 + COMMUNICATION_DIAGNOSTIC",
                "NEON_LEGACY",
                "NEON-1S",
                "DIO605",
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectPendingExplicit: true),
            new(
                "RION-502 + HIO115 + COMMUNICATION_DIAGNOSTIC",
                "RION_LEGACY",
                "RION-502",
                "HIO115",
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectPendingExplicit: true),
            new(
                "RION-502 + HIO115 + REMOTE_IO_RS485",
                "RION_LEGACY",
                "RION-502",
                "HIO115",
                SerialRs48557600,
                "REMOTE_IO_RS485",
                ExpectPendingExplicit: true),
            new(
                "NEON_5_CONTROLLER pending case",
                "NEON_5",
                "NEON_5_CONTROLLER",
                null,
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectPendingExplicit: true),
            new(
                "RION_5_CONTROLLER pending case",
                "RION_5",
                "RION_5_CONTROLLER",
                null,
                SerialRs48538400,
                "COMMUNICATION_DIAGNOSTIC",
                ExpectPendingExplicit: true)
        ];

        bool isValid = true;

        output.WriteLine("Validacao nao visual do relatorio de preparacao de teste de hardware");
        output.WriteLine("Escopo: catalogo + resolver + formatter, sem abrir UI e sem enviar comandos fisicos.");
        output.WriteLine();

        foreach (Scenario scenario in scenarios)
        {
            List<string> failures = ValidateScenario(catalog, scenario, out int reportLength);

            if (failures.Count == 0)
            {
                output.WriteLine($"[OK] {scenario.Name} ({reportLength} caracteres)");
                continue;
            }

            isValid = false;
            output.WriteLine($"[FALHA] {scenario.Name}");

            foreach (string failure in failures)
            {
                output.WriteLine($"  - {failure}");
            }
        }

        output.WriteLine();
        output.WriteLine($"Relatorios gerados: {scenarios.Length}");
        output.WriteLine("Comandos fisicos executados: 0");
        output.WriteLine(isValid ? "Resultado: OK" : "Resultado: FALHA");

        if (!isValid)
        {
            error.WriteLine("Validacao do relatorio de preparacao de teste de hardware falhou.");
        }

        return isValid ? 0 : 1;
    }

    private static List<string> ValidateScenario(
        HardwareCatalog catalog,
        Scenario scenario,
        out int reportLength)
    {
        List<string> failures = [];
        reportLength = 0;
        HardwareProfileResolution resolution;
        string reportText;

        try
        {
            resolution = Resolver.Resolve(
                catalog,
                scenario.FamilyId,
                scenario.ModelId,
                scenario.ModuleId,
                scenario.CommunicationProfileId,
                scenario.TestProfileId);
            reportText = Formatter.Format(resolution).Text;
            reportLength = reportText.Length;
        }
        catch (Exception ex)
        {
            failures.Add($"Geracao do relatorio gerou excecao: {ex.GetType().Name}: {ex.Message}");
            return failures;
        }

        if (string.IsNullOrWhiteSpace(reportText))
        {
            failures.Add("Relatorio gerado vazio.");
            return failures;
        }

        AddMissingSelectionFailures(failures, scenario, resolution);
        AddRequiredWarningFailures(failures, reportText);
        AddPendingExplicitFailures(failures, scenario, resolution, reportText);

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

    private static void AddRequiredWarningFailures(ICollection<string> failures, string reportText)
    {
        if (!Contains(reportText, NoPhysicalCommandWarning))
        {
            failures.Add("Relatorio nao contem aviso de que nao executa comando fisico.");
        }

        if (!Contains(reportText, BenchValidationWarning))
        {
            failures.Add("Relatorio nao contem aviso de validacao obrigatoria em bancada.");
        }

        if (!Contains(reportText, ComConflictWarning))
        {
            failures.Add("Relatorio nao contem aviso de conflito de COM.");
        }

        if (!Contains(reportText, HistudioConflictWarning))
        {
            failures.Add("Relatorio nao contem aviso de conflito HIstudio/XCTU/Testador.");
        }
    }

    private static void AddPendingExplicitFailures(
        ICollection<string> failures,
        Scenario scenario,
        HardwareProfileResolution resolution,
        string reportText)
    {
        if (!scenario.ExpectPendingExplicit)
        {
            return;
        }

        if (!resolution.RequiresManualValidation)
        {
            failures.Add("RequiresManualValidation deveria estar ativo.");
        }

        if (resolution.PendingItems.Count == 0)
        {
            failures.Add("Itens pendentes nao foram expostos pela resolucao.");
            return;
        }

        if (!Contains(reportText, "pending_manual_validation") &&
            !Contains(reportText, "official_reference"))
        {
            failures.Add("Relatorio nao mantem pendencias explicitas.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.ModuleId))
        {
            return;
        }

        if (!Contains(reportText, "Nenhum modulo de I/O confirmado para este modelo"))
        {
            failures.Add("Caso pendente sem modulo nao permaneceu explicito no relatorio.");
        }

        if (!Contains(reportText, "Isso nao indica falha de comunicacao"))
        {
            failures.Add("Relatorio nao diferencia ausencia de modulo de falha de comunicacao.");
        }
    }

    private static bool Contains(string text, string value)
    {
        return text.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record Scenario(
        string Name,
        string FamilyId,
        string ModelId,
        string? ModuleId,
        string CommunicationProfileId,
        string TestProfileId,
        bool ExpectPendingExplicit);
}
