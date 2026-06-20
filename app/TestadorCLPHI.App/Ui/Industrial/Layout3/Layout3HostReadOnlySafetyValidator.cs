using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Verificacao nao visual de seguranca do host Layout 3 read-only.
///
/// Garante, sem abrir UI e sem qualquer transporte, que o estado de comunicacao
/// exibido continua derivado apenas do catalogo local e da selecao de perfil:
/// sem conexao fisica, sem protocolo de campo ativo, sem leitura ou escrita real
/// de registrador e sem comando fisico. Toda tentativa real permanece em 0 e o
/// guard read-only bloqueia qualquer intencao de comando.
/// </summary>
internal static class Layout3HostReadOnlySafetyValidator
{
    private static readonly DateTime ReferenceTimestamp = new(2026, 6, 19, 0, 0, 0, DateTimeKind.Local);

    private static readonly string[] AllowedOrigins =
    [
        "Catalogo local",
        "Host local",
        "Sem hardware"
    ];

    private static readonly Layout3CommunicationStatus[] AllowedStatuses =
    [
        Layout3CommunicationStatus.NotConnected,
        Layout3CommunicationStatus.LocalReadOnly,
        Layout3CommunicationStatus.Simulated,
        Layout3CommunicationStatus.Blocked
    ];

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

        output.WriteLine("Verificacao nao visual de seguranca do host Layout 3 read-only");
        output.WriteLine("Escopo: catalogo local + selecao de perfil + guard read-only.");
        output.WriteLine("Sem conexao fisica, sem porta serial, sem Modbus, sem PLC, sem registrador.");
        output.WriteLine();

        bool isValid = true;
        int totalPhysicalCommands = 0;

        foreach (Layout3HostReadOnlySafetyValidationScenario scenario in scenarios)
        {
            Layout3HostReadOnlySafetyValidationResult result = ValidateScenario(catalog, scenario);
            totalPhysicalCommands += result.PhysicalCommandsExecuted;

            output.WriteLine($"[{result.ResultLabel}] {result.ScenarioName}");
            output.WriteLine($"  Perfil................: {result.ProfileLabel}");
            output.WriteLine($"  Protocolo (catalogo)..: {result.CatalogProtocol}");
            output.WriteLine($"  Estado calculado......: {result.CommunicationStatus}");
            output.WriteLine($"  Origem do estado......: {result.Origin}");
            output.WriteLine($"  Tentativas reais......: {result.RealConnectionAttempts}");
            output.WriteLine($"  Comandos fisicos......: {result.PhysicalCommandsExecuted}");
            output.WriteLine($"  Guard read-only.......: {(result.CommandGuardBlocked ? "bloqueado" : "PERMITIU")}");

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

        if (totalPhysicalCommands != 0)
        {
            isValid = false;
            output.WriteLine($"[ERRO] Comandos fisicos executados durante a verificacao: {totalPhysicalCommands}.");
        }

        output.WriteLine($"Cenarios verificados: {scenarios.Count}");
        output.WriteLine("Tentativas reais de conexao: 0");
        output.WriteLine("Comandos fisicos executados: 0");
        output.WriteLine(isValid ? "Resultado: OK" : "Resultado: ERRO");

        if (!isValid)
        {
            error.WriteLine("Verificacao de seguranca read-only do host Layout 3 falhou.");
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

    private static Layout3HostReadOnlySafetyValidationResult ValidateScenario(
        HardwareCatalog catalog,
        Layout3HostReadOnlySafetyValidationScenario scenario)
    {
        List<string> failures = [];

        Layout3ProfileSelection selection;
        Layout3CommunicationState communication;

        try
        {
            selection = ResolveSelection(catalog, scenario);
            communication = Layout3CommunicationState.FromSelection(selection, ReferenceTimestamp);
        }
        catch (Exception ex)
        {
            return new Layout3HostReadOnlySafetyValidationResult(
                ScenarioName: scenario.Name,
                ProfileLabel: "indisponivel",
                CatalogProtocol: "indisponivel",
                CommunicationStatus: "indisponivel",
                Origin: "indisponivel",
                RealConnectionAttempts: 0,
                PhysicalCommandsExecuted: 0,
                CommandGuardBlocked: false,
                Failures: [$"Cenario gerou excecao: {ex.GetType().Name}: {ex.Message}"]);
        }

        string profileLabel = $"{selection.FamilyName} / {selection.ModelName} / {selection.ModuleName}";

        // Estado deve nascer sem qualquer tentativa real ou comando fisico.
        if (communication.RealConnectionAttempts != 0)
        {
            failures.Add($"RealConnectionAttempts deveria ser 0; foi {communication.RealConnectionAttempts}.");
        }

        if (communication.PhysicalCommandsExecuted != 0)
        {
            failures.Add($"PhysicalCommandsExecuted deveria ser 0; foi {communication.PhysicalCommandsExecuted}.");
        }

        // Estado deve permanecer dentro do conjunto exclusivamente local.
        if (!AllowedStatuses.Contains(communication.Status))
        {
            failures.Add($"Status fora do conjunto local seguro: {communication.Status}.");
        }

        // Origem precisa apontar para catalogo/local/sem hardware, nunca conexao ativa.
        if (!AllowedOrigins.Contains(communication.Origin))
        {
            failures.Add($"Origem do estado nao reconhecida como catalogo/local: '{communication.Origin}'.");
        }

        // Coerencia entre presenca de hardware e estado derivado.
        if (scenario.ExpectsHardware)
        {
            if (communication.Status != Layout3CommunicationStatus.LocalReadOnly)
            {
                failures.Add("Com hardware do catalogo o estado deveria ser local read-only.");
            }

            if (string.Equals(communication.Origin, "Sem hardware", StringComparison.Ordinal))
            {
                failures.Add("Cenario com hardware nao deveria reportar origem 'Sem hardware'.");
            }
        }
        else
        {
            if (communication.Status != Layout3CommunicationStatus.NotConnected)
            {
                failures.Add("Sem hardware o estado deveria ser nao conectado.");
            }

            if (!string.Equals(communication.Origin, "Sem hardware", StringComparison.Ordinal))
            {
                failures.Add("Cenario sem hardware deveria reportar origem 'Sem hardware'.");
            }
        }

        // Protocolo exibido e dado de catalogo, nunca um canal aberto.
        if (selection.Communication is null)
        {
            if (!string.Equals(communication.ProfileProtocol, "Nao disponivel", StringComparison.Ordinal))
            {
                failures.Add("Sem perfil de comunicacao o protocolo deveria ser 'Nao disponivel'.");
            }
        }
        else if (string.Equals(communication.ProfileProtocol, "Nao disponivel", StringComparison.Ordinal))
        {
            failures.Add("Perfil de comunicacao do catalogo nao foi refletido no protocolo exibido.");
        }

        // Mensagem operacional nunca pode indicar canal aberto.
        bool declaresNoOpenChannel =
            communication.OperationalMessage.Contains("nenhuma conexao", StringComparison.OrdinalIgnoreCase)
            || communication.OperationalMessage.Contains("desconectado", StringComparison.OrdinalIgnoreCase)
            || communication.Status == Layout3CommunicationStatus.Blocked;
        if (!declaresNoOpenChannel)
        {
            failures.Add($"Mensagem operacional nao confirma ausencia de conexao: '{communication.OperationalMessage}'.");
        }

        // Cenarios pendentes (referencia oficial) nao podem gerar erro nem conexao.
        if (scenario.PendingOfficialReference && selection.Model is null)
        {
            failures.Add("Cenario de referencia oficial pendente nao resolveu o modelo esperado.");
        }

        // Guard read-only deve bloquear qualquer intencao de comando fisico.
        bool guardBlocked = EvaluateReadOnlyGuard(scenario, failures, out int physicalCommands);

        return new Layout3HostReadOnlySafetyValidationResult(
            ScenarioName: scenario.Name,
            ProfileLabel: profileLabel,
            CatalogProtocol: communication.ProfileProtocol,
            CommunicationStatus: communication.StatusDisplayName,
            Origin: communication.Origin,
            RealConnectionAttempts: communication.RealConnectionAttempts,
            PhysicalCommandsExecuted: physicalCommands,
            CommandGuardBlocked: guardBlocked,
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

    private static bool EvaluateReadOnlyGuard(
        Layout3HostReadOnlySafetyValidationScenario scenario,
        ICollection<string> failures,
        out int physicalCommands)
    {
        // Intencao puramente sintetica: nada e transmitido, apenas avaliado.
        ILayout3CommandGuard guard = new Layout3ReadOnlyCommandGuard();
        Layout3CommandIntent intent = Layout3CommandIntent.Create(
            action: "force-output",
            target: scenario.FamilyId ?? "no-hardware",
            origin: "safety-validation");

        Layout3CommandDecision decision = guard.Evaluate(intent);

        // O guard nunca executa: o numero de comandos fisicos permanece 0.
        physicalCommands = 0;

        if (decision.Allowed)
        {
            failures.Add("Guard read-only permitiu uma intencao de comando fisico.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(decision.Message)
            || !decision.Message.Contains("read-only", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Mensagem de bloqueio do guard nao confirma o modo read-only.");
        }

        return true;
    }
}
