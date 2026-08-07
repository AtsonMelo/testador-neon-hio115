namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal static class Layout3BenchReadinessValidator
{
    public static int ValidateDefaultConfiguration(TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        output.WriteLine("Validador local de prontidao da bancada Layout 3");
        output.WriteLine("Escopo: configuracao versionada e documentos locais; transporte inexistente.");
        output.WriteLine();

        try
        {
            int scenarioResult = Layout3BenchReadinessScenarioValidator.Validate(output, error);
            if (scenarioResult != 0)
            {
                return scenarioResult;
            }

            output.WriteLine();
            output.WriteLine("Avaliacao da configuracao versionada");

            Layout3BenchReadinessLoadResult loaded =
                Layout3BenchReadinessConfigurationLoader.LoadDefault();
            Layout3BenchReadinessEvaluationResult result =
                Layout3BenchReadinessEvaluator.Evaluate(
                    loaded.Configuration,
                    loaded.AvailableDocuments);

            output.WriteLine($"Configuracao: {loaded.ConfigurationPath}");
            output.WriteLine(
                $"Documentos locais: {loaded.AvailableDocuments.Count}/" +
                $"{Layout3BenchReadinessEvaluator.RequiredDocuments.Count}");
            output.WriteLine(
                $"Transporte selecionado: {loaded.Configuration.Connection?.SelectedTransport ?? "AUSENTE"}");
            output.WriteLine(
                $"Protocolo ativo confirmado: {FormatConfirmation(loaded.Configuration.Connection?.ActiveProtocolConfirmed)}");
            output.WriteLine($"Feature flag: {FormatFlag(loaded.Configuration.Safety?.FeatureEnabled)}");
            output.WriteLine(
                $"Comunicacao real: {FormatFlag(loaded.Configuration.Safety?.RealCommunicationEnabled)}");
            output.WriteLine($"Escrita: {FormatFlag(loaded.Configuration.Safety?.WritesEnabled)}");
            output.WriteLine(
                $"Modo supervisionado de saida: {FormatFlag(loaded.Configuration.Safety?.OutputModeEnabled)}");
            output.WriteLine($"Polling: {FormatFlag(loaded.Configuration.Safety?.PollingEnabled)}");
            output.WriteLine(
                $"Reconexao automatica: {FormatFlag(loaded.Configuration.Safety?.AutomaticReconnectEnabled)}");
            output.WriteLine(
                $"Gate D offline autorizado: {FormatConfirmation(loaded.Configuration.Safety?.OfflineGateDAuthorized)}");
            output.WriteLine(
                $"Transporte fisico autorizado: {FormatConfirmation(loaded.Configuration.Safety?.PhysicalTransportAuthorized)}");
            output.WriteLine(
                $"Gate fisico de saida autorizado: {FormatConfirmation(loaded.Configuration.Safety?.PhysicalOutputGateAuthorized)}");
            output.WriteLine();

            if (result.IsReady)
            {
                output.WriteLine("Pendencias: 0");
                Layout3BenchReadinessScenarioValidator.WriteZeroCounters(output);
                output.WriteLine("Resultado: READY FOR BENCH TEST");
                return 0;
            }

            output.WriteLine($"Pendencias: {result.Failures.Count}");
            foreach (string failure in result.Failures)
            {
                output.WriteLine($"- {failure}");
            }

            Layout3BenchReadinessScenarioValidator.WriteZeroCounters(output);
            output.WriteLine("Resultado: BLOQUEADO - dados de bancada incompletos ou conflitantes");
            error.WriteLine("Prontidao de bancada nao aprovada; consulte as pendencias listadas.");
            return 2;
        }
        catch (Exception ex)
        {
            error.WriteLine("Falha fechada ao validar prontidao da bancada.");
            error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            Layout3BenchReadinessScenarioValidator.WriteZeroCounters(output);
            output.WriteLine("Resultado: BLOQUEADO - falha local de validacao");
            return 1;
        }
    }

    private static string FormatFlag(bool? value)
    {
        return value switch
        {
            false => "OFF",
            true => "ON",
            null => "AUSENTE"
        };
    }

    private static string FormatConfirmation(bool? value)
    {
        return value switch
        {
            true => "SIM",
            false => "NAO",
            null => "AUSENTE"
        };
    }
}
