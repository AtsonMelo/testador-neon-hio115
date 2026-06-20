using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Selecao local e imutavel de perfil/hardware do host Layout 3 read-only.
/// Resolve familia, modelo, modulo, comunicacao e teste a partir do catalogo
/// local ja carregado pela aplicacao, sem qualquer comunicacao fisica.
/// </summary>
internal sealed record Layout3ProfileSelection(
    HardwareFamily? Family,
    HardwareModel? Model,
    IoModuleDefinition? Module,
    CommunicationProfile? Communication,
    TestProfile? Test,
    string ValidationStatus,
    bool ValidationPending)
{
    public static Layout3ProfileSelection Empty { get; } = new(
        Family: null,
        Model: null,
        Module: null,
        Communication: null,
        Test: null,
        ValidationStatus: "catalogo_indisponivel",
        ValidationPending: false);

    public string FamilyName => Display(Family?.DisplayName);

    public string ModelName => Display(Model?.DisplayName);

    public string ModuleName => Display(Module?.DisplayName);

    public string CommunicationName => Display(Communication?.DisplayName);

    public string TestName => Display(Test?.DisplayName);

    public string ValidationLabel => ValidationPending
        ? "VALIDACAO MANUAL PENDENTE"
        : ValidationStatus.Replace('_', ' ').ToUpperInvariant();

    /// <summary>
    /// Selecao inicial padrao: primeira familia do catalogo e seus dependentes.
    /// </summary>
    public static Layout3ProfileSelection CreateDefault(HardwareCatalog? catalog)
    {
        HardwareCatalog source = catalog ?? HardwareCatalog.Empty;
        HardwareFamily? family = source.Families
            .OrderBy(item => item.DisplayName)
            .FirstOrDefault();
        return ForFamily(source, family);
    }

    /// <summary>
    /// Resolve a selecao de uma familia escolhendo o primeiro dependente de cada
    /// tipo (modelo, modulo, comunicacao e teste) ainda em modo somente leitura.
    /// </summary>
    public static Layout3ProfileSelection ForFamily(HardwareCatalog catalog, HardwareFamily? family)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (family is null)
        {
            return Empty;
        }

        HardwareModel? model = catalog.GetModelsByFamily(family.Id)
            .OrderBy(item => item.DisplayName)
            .FirstOrDefault();
        IoModuleDefinition? module = catalog.GetIoModulesByFamily(family.Id)
            .OrderBy(item => item.DisplayName)
            .FirstOrDefault();
        CommunicationProfile? communication = ResolveCommunication(catalog, family);
        TestProfile? test = module is null
            ? null
            : catalog.GetTestProfilesByModule(module.Id)
                .OrderBy(item => item.DisplayName)
                .FirstOrDefault();

        return Create(family, model, module, communication, test);
    }

    /// <summary>
    /// Monta uma selecao a partir de entidades ja escolhidas, derivando o status
    /// de validacao e a sinalizacao de pendencia sem bloquear a interface.
    /// </summary>
    public static Layout3ProfileSelection Create(
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? module,
        CommunicationProfile? communication,
        TestProfile? test)
    {
        string status = FirstNonEmpty(
            model?.ValidationStatus,
            module?.ValidationStatus,
            test?.ValidationStatus,
            family?.ValidationStatus,
            "catalogo_indisponivel");
        bool pending = IsPending(status)
            || IsPending(test?.ValidationStatus)
            || IsPending(module?.ValidationStatus);

        return new Layout3ProfileSelection(
            family,
            model,
            module,
            communication,
            test,
            status,
            pending);
    }

    public static CommunicationProfile? ResolveCommunication(HardwareCatalog catalog, HardwareFamily? family)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (family is null)
        {
            return null;
        }

        return catalog.GetCommunicationProfilesByFamily(family.Id)
            .OrderBy(item => item.DisplayName)
            .FirstOrDefault();
    }

    private static bool IsPending(string? status)
    {
        return status is not null
            && (status.Contains("pending", StringComparison.OrdinalIgnoreCase)
                || status.Contains("manual", StringComparison.OrdinalIgnoreCase));
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.First(value => !string.IsNullOrWhiteSpace(value))!;
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Nao disponivel" : value;
    }
}
