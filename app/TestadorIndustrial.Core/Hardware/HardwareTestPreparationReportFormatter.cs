using System.Text;

namespace TestadorCLPHI.App.Hardware;

public sealed class HardwareTestPreparationReportFormatter
{
    public HardwareTestPreparationReport Format(HardwareProfileResolution resolution)
    {
        return FormatCore(
            resolution.Family,
            resolution.Model,
            resolution.IoModule,
            resolution.CommunicationProfile,
            resolution.TestProfile,
            resolution.CompatibleModules,
            resolution.PossibleCommunicationProfiles,
            resolution.ApplicableTestProfiles,
            resolution.PendingItems,
            resolution.FieldObservedItems,
            resolution.BenchValidationNeeds,
            resolution.IsComplete,
            resolution.RequiresManualValidation);
    }

    public HardwareTestPreparationReport Format(SelectedHardwareProfile profile)
    {
        return FormatCore(
            profile.Family,
            profile.Model,
            profile.IoModule,
            profile.CommunicationProfile,
            profile.TestProfile,
            profile.CompatibleModules,
            profile.PossibleCommunicationProfiles,
            profile.ApplicableTestProfiles,
            profile.PendingItems,
            profile.FieldObservedItems,
            profile.BenchValidationNeeds,
            profile.IsComplete,
            profile.RequiresManualValidation);
    }

    private static HardwareTestPreparationReport FormatCore(
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? module,
        CommunicationProfile? communicationProfile,
        TestProfile? testProfile,
        IReadOnlyList<IoModuleDefinition> compatibleModules,
        IReadOnlyList<CommunicationProfile> possibleCommunicationProfiles,
        IReadOnlyList<TestProfile> applicableTestProfiles,
        IReadOnlyList<string> pendingItems,
        IReadOnlyList<string> fieldObservedItems,
        IReadOnlyList<string> benchValidationNeeds,
        bool isComplete,
        bool requiresManualValidation)
    {
        StringBuilder builder = new();

        builder.AppendLine("# Relatorio de preparacao de teste de hardware");
        builder.AppendLine();
        builder.AppendLine("Avisos de seguranca");
        builder.AppendLine("- A selecao de perfil e a geracao deste relatorio nao executam comandos fisicos.");
        builder.AppendLine("- Este relatorio nao altera parametros Modbus, mapa de registradores, logica de leitura/escrita ou configuracoes de conexao.");
        builder.AppendLine("- Validacao em bancada ainda e obrigatoria antes de uso operacional.");
        builder.AppendLine("- Conflito de COM: garanta que somente um software use a porta COM do CLP por vez.");
        builder.AppendLine("- HIstudio, XCTU e Testador nao devem disputar a mesma COM ao mesmo tempo.");
        builder.AppendLine();

        builder.AppendLine("Selecao atual");
        AppendSelectedItem(builder, "Familia selecionada", family?.DisplayName, family?.Id);
        AppendSelectedItem(builder, "Modelo selecionado", model?.DisplayName, model?.Id);
        builder.AppendLine($"- CPU informada no catalogo: {FormatValue(model?.ControllerCpu)}");
        AppendSelectedItem(builder, "Modulo de I/O selecionado", module?.DisplayName, module?.Id);
        AppendSelectedItem(
            builder,
            "Perfil de comunicacao selecionado",
            communicationProfile?.DisplayName,
            communicationProfile?.Id);
        AppendSelectedItem(builder, "Perfil de teste selecionado", testProfile?.DisplayName, testProfile?.Id);
        builder.AppendLine();

        builder.AppendLine("Status de validacao");
        builder.AppendLine($"- Resultado geral: {BuildValidationStatusText(isComplete, requiresManualValidation, pendingItems)}");
        AppendItemStatus(builder, "Familia", family?.SourceStatus, family?.ValidationStatus);
        AppendItemStatus(builder, "Modelo", model?.SourceStatus, model?.ValidationStatus);
        AppendItemStatus(builder, "Modulo", module?.SourceStatus, module?.ValidationStatus);
        AppendItemStatus(
            builder,
            "Comunicacao",
            communicationProfile?.SourceStatus,
            communicationProfile?.ValidationStatus);
        AppendItemStatus(builder, "Teste", testProfile?.SourceStatus, testProfile?.ValidationStatus);
        builder.AppendLine();

        builder.AppendLine("Itens pendentes/manual validation");
        AppendList(builder, pendingItems, "Nenhuma pendencia explicita no catalogo selecionado.");
        builder.AppendLine();

        builder.AppendLine("Itens observados em campo");
        AppendList(builder, fieldObservedItems, "Nenhum item observado em campo para a selecao atual.");
        builder.AppendLine();

        builder.AppendLine("Necessidades de validacao em bancada");
        AppendList(builder, benchValidationNeeds, "Nenhuma necessidade de bancada foi calculada para a selecao atual.");
        builder.AppendLine();

        builder.AppendLine("Checklist do operador antes do teste");
        AppendOperatorChecklist(builder, communicationProfile, testProfile, requiresManualValidation);
        builder.AppendLine();

        builder.AppendLine("Opcoes disponiveis no catalogo");
        builder.AppendLine("- Modulos compativeis:");
        AppendIndentedList(
            builder,
            compatibleModules.Select(item => $"{item.DisplayName} ({item.Id})"),
            "Nenhum modulo de I/O confirmado para este modelo. Isso nao indica falha de comunicacao.");
        builder.AppendLine("- Perfis de comunicacao possiveis:");
        AppendIndentedList(
            builder,
            possibleCommunicationProfiles.Select(item => $"{item.DisplayName} ({item.Id})"),
            "Nenhum perfil de comunicacao disponivel no catalogo.");
        builder.AppendLine("- Perfis de teste aplicaveis:");
        AppendIndentedList(
            builder,
            applicableTestProfiles.Select(item => $"{item.DisplayName} ({item.Id})"),
            "Nenhum perfil de teste disponivel no catalogo.");

        return new HardwareTestPreparationReport(builder.ToString());
    }

    private static void AppendOperatorChecklist(
        StringBuilder builder,
        CommunicationProfile? communicationProfile,
        TestProfile? testProfile,
        bool requiresManualValidation)
    {
        builder.AppendLine("- Conferir no equipamento real a familia, modelo, CPU, modulo e slot.");
        builder.AppendLine("- Confirmar que o programa HIstudio correto esta carregado e documentado.");
        builder.AppendLine("- Confirmar que HIstudio, XCTU e Testador nao estao usando a mesma porta COM.");
        builder.AppendLine("- Ajustar porta, baud rate, slave ID, cabeamento e energia somente nos fluxos normais de conexao e bancada.");
        builder.AppendLine("- Comecar por COMMUNICATION_DIAGNOSTIC quando aplicavel, antes de qualquer perfil de I/O.");
        builder.AppendLine("- Registrar modelo, CPU, modulo, slot, perfil, porta, baud rate, slave ID e resultado.");

        if (communicationProfile is not null)
        {
            builder.AppendLine($"- Conferir a comunicacao selecionada: {communicationProfile.DisplayName} ({communicationProfile.Id}).");
        }

        if (testProfile is not null)
        {
            builder.AppendLine($"- Conferir o perfil de teste selecionado: {testProfile.DisplayName} ({testProfile.Id}).");
        }

        if (requiresManualValidation)
        {
            builder.AppendLine("- Tratar pending_manual_validation como pendencia aberta ate haver registro de bancada.");
        }
    }

    private static string BuildValidationStatusText(
        bool isComplete,
        bool requiresManualValidation,
        IReadOnlyList<string> pendingItems)
    {
        if (pendingItems.Any(item => item.Contains("Catalogo vazio", StringComparison.OrdinalIgnoreCase)))
        {
            return "Catalogo vazio ou indisponivel.";
        }

        if (requiresManualValidation)
        {
            return "pending_manual_validation - validar manualmente antes de uso operacional.";
        }

        return isComplete
            ? "Sem pendencias explicitas no catalogo selecionado."
            : "Selecao incompleta.";
    }

    private static void AppendSelectedItem(
        StringBuilder builder,
        string label,
        string? displayName,
        string? id)
    {
        builder.AppendLine($"- {label}: {FormatSelected(displayName, id)}");
    }

    private static void AppendItemStatus(
        StringBuilder builder,
        string label,
        string? sourceStatus,
        string? validationStatus)
    {
        builder.AppendLine(
            $"- {label}: source={FormatValue(sourceStatus)}; validation={FormatValue(validationStatus)}");
    }

    private static void AppendList(
        StringBuilder builder,
        IEnumerable<string> items,
        string emptyText)
    {
        bool hasItem = false;

        foreach (string item in items)
        {
            builder.AppendLine($"- {item}");
            hasItem = true;
        }

        if (!hasItem)
        {
            builder.AppendLine($"- {emptyText}");
        }
    }

    private static void AppendIndentedList(
        StringBuilder builder,
        IEnumerable<string> items,
        string emptyText)
    {
        bool hasItem = false;

        foreach (string item in items)
        {
            builder.AppendLine($"  - {item}");
            hasItem = true;
        }

        if (!hasItem)
        {
            builder.AppendLine($"  - {emptyText}");
        }
    }

    private static string FormatSelected(string? displayName, string? id)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return "pendente";
        }

        return string.IsNullOrWhiteSpace(id)
            ? displayName
            : $"{displayName} ({id})";
    }

    private static string FormatValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "pendente" : value;
    }
}
