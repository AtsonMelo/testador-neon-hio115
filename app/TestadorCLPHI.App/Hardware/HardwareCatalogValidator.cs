namespace TestadorCLPHI.App.Hardware;

public static class HardwareCatalogValidator
{
    private static readonly string[] RequiredFamilies =
    [
        "NEON_LEGACY",
        "NEON_5",
        "RION_LEGACY",
        "RION_PLUS",
        "RION_5"
    ];

    private static readonly string[] RequiredModels =
    [
        "NEON-1S",
        "NEON-2S",
        "NEON_5_CONTROLLER",
        "RION-502",
        "RION_5_CONTROLLER"
    ];

    private static readonly string[] RequiredIoModules =
    [
        "HIO115",
        "DIO605",
        "HIO130",
        "HIO140",
        "HIO165"
    ];

    private static readonly string[] RequiredCommunicationProfiles =
    [
        "SERIAL_RS232_38400_8N1_MODBUS_RTU",
        "SERIAL_RS485_38400_8N1_MODBUS_RTU",
        "SERIAL_RS485_57600_8N1_MODBUS_RTU",
        "ETHERNET_MODBUS_TCP",
        "WIRELESS_RADIO_TRANSPARENT"
    ];

    private static readonly string[] RequiredTestProfiles =
    [
        "DIGITAL_IO_BASIC",
        "DIGITAL_OUTPUT_MANUAL",
        "DIGITAL_INPUT_READ",
        "REMOTE_IO_RS485",
        "COMMUNICATION_DIAGNOSTIC",
        "COUNTER_ENCODER_PENDING_VALIDATION"
    ];

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "field_observed",
        "official_reference_pending",
        "pending_manual_validation",
        "verified_in_bench"
    };

    public static HardwareCatalogValidationResult Validate(HardwareCatalog catalog)
    {
        HardwareCatalogValidationResult result = new();

        ValidateRequiredCatalogFields(catalog, result);
        ValidateUniqueIds(catalog.Families, family => family.Id, "familia", result);
        ValidateUniqueIds(catalog.Models, model => model.Id, "modelo", result);
        ValidateUniqueIds(catalog.IoModules, module => module.Id, "modulo de I/O", result);
        ValidateUniqueIds(catalog.CommunicationProfiles, profile => profile.Id, "perfil de comunicacao", result);
        ValidateUniqueIds(catalog.TestProfiles, profile => profile.Id, "perfil de teste", result);
        ValidateGlobalUniqueIds(catalog, result);

        HashSet<string> familyIds = BuildIdSet(catalog.Families.Select(family => family.Id));
        HashSet<string> modelIds = BuildIdSet(catalog.Models.Select(model => model.Id));
        HashSet<string> ioModuleIds = BuildIdSet(catalog.IoModules.Select(module => module.Id));
        HashSet<string> communicationProfileIds = BuildIdSet(catalog.CommunicationProfiles.Select(profile => profile.Id));
        HashSet<string> testProfileIds = BuildIdSet(catalog.TestProfiles.Select(profile => profile.Id));

        ValidateRequiredIds(RequiredFamilies, familyIds, "familia obrigatoria", result);
        ValidateRequiredIds(RequiredModels, modelIds, "modelo obrigatorio", result);
        ValidateRequiredIds(RequiredIoModules, ioModuleIds, "modulo de I/O obrigatorio", result);
        ValidateRequiredIds(RequiredCommunicationProfiles, communicationProfileIds, "perfil de comunicacao obrigatorio", result);
        ValidateRequiredIds(RequiredTestProfiles, testProfileIds, "perfil de teste obrigatorio", result);

        ValidateFamilies(catalog.Families, communicationProfileIds, ioModuleIds, testProfileIds, result);
        ValidateModels(catalog.Models, familyIds, communicationProfileIds, ioModuleIds, testProfileIds, result);
        ValidateIoModules(catalog.IoModules, familyIds, communicationProfileIds, ioModuleIds, testProfileIds, result);
        ValidateCommunicationProfiles(catalog.CommunicationProfiles, familyIds, result);
        ValidateTestProfiles(catalog.TestProfiles, familyIds, ioModuleIds, result);

        return result;
    }

    private static void ValidateRequiredCatalogFields(
        HardwareCatalog catalog,
        HardwareCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(catalog.CatalogId))
        {
            result.AddError("catalogId deve ser informado.");
        }

        if (string.IsNullOrWhiteSpace(catalog.SchemaVersion))
        {
            result.AddError("schemaVersion deve ser informado.");
        }
    }

    private static void ValidateFamilies(
        IEnumerable<HardwareFamily> families,
        ISet<string> communicationProfileIds,
        ISet<string> ioModuleIds,
        ISet<string> testProfileIds,
        HardwareCatalogValidationResult result)
    {
        foreach (HardwareFamily family in families)
        {
            ValidateStatus(family.SourceStatus, $"familia {family.Id} sourceStatus", result);
            ValidateStatus(family.ValidationStatus, $"familia {family.Id} validationStatus", result);
            ValidateReferences(family.DefaultCommunicationProfiles, communicationProfileIds, $"familia {family.Id} defaultCommunicationProfiles", result);
            ValidateReferences(family.SupportedIoModules, ioModuleIds, $"familia {family.Id} supportedIoModules", result);
            ValidateReferences(family.TestProfiles, testProfileIds, $"familia {family.Id} testProfiles", result);
        }
    }

    private static void ValidateModels(
        IEnumerable<HardwareModel> models,
        ISet<string> familyIds,
        ISet<string> communicationProfileIds,
        ISet<string> ioModuleIds,
        ISet<string> testProfileIds,
        HardwareCatalogValidationResult result)
    {
        foreach (HardwareModel model in models)
        {
            ValidateReference(model.Family, familyIds, $"modelo {model.Id} family", result);
            ValidateStatus(model.SourceStatus, $"modelo {model.Id} sourceStatus", result);
            ValidateStatus(model.ValidationStatus, $"modelo {model.Id} validationStatus", result);
            ValidateReferences(model.DefaultCommunicationProfiles, communicationProfileIds, $"modelo {model.Id} defaultCommunicationProfiles", result);
            ValidateReferences(model.SupportedIoModules, ioModuleIds, $"modelo {model.Id} supportedIoModules", result);
            ValidateReferences(model.TestProfiles, testProfileIds, $"modelo {model.Id} testProfiles", result);

            if ((IsVerified(model.SourceStatus) || IsVerified(model.ValidationStatus)) &&
                string.IsNullOrWhiteSpace(model.ValidationNotes))
            {
                result.AddError($"modelo {model.Id} esta marcado como verified_in_bench sem validationNotes.");
            }
        }
    }

    private static void ValidateIoModules(
        IEnumerable<IoModuleDefinition> modules,
        ISet<string> familyIds,
        ISet<string> communicationProfileIds,
        ISet<string> ioModuleIds,
        ISet<string> testProfileIds,
        HardwareCatalogValidationResult result)
    {
        foreach (IoModuleDefinition module in modules)
        {
            ValidateReference(module.Family, familyIds, $"modulo {module.Id} family", result);
            ValidateStatus(module.SourceStatus, $"modulo {module.Id} sourceStatus", result);
            ValidateStatus(module.ValidationStatus, $"modulo {module.Id} validationStatus", result);
            ValidateReferences(module.SupportedFamilies, familyIds, $"modulo {module.Id} supportedFamilies", result);
            ValidateReferences(module.DefaultCommunicationProfiles, communicationProfileIds, $"modulo {module.Id} defaultCommunicationProfiles", result);
            ValidateReferences(module.SupportedIoModules, ioModuleIds, $"modulo {module.Id} supportedIoModules", result);
            ValidateReferences(module.TestProfiles, testProfileIds, $"modulo {module.Id} testProfiles", result);
        }
    }

    private static void ValidateCommunicationProfiles(
        IEnumerable<CommunicationProfile> profiles,
        ISet<string> familyIds,
        HardwareCatalogValidationResult result)
    {
        foreach (CommunicationProfile profile in profiles)
        {
            ValidateStatus(profile.SourceStatus, $"perfil de comunicacao {profile.Id} sourceStatus", result);
            ValidateStatus(profile.ValidationStatus, $"perfil de comunicacao {profile.Id} validationStatus", result);
            ValidateReferences(profile.ApplicableFamilies, familyIds, $"perfil de comunicacao {profile.Id} applicableFamilies", result);
        }
    }

    private static void ValidateTestProfiles(
        IEnumerable<TestProfile> profiles,
        ISet<string> familyIds,
        ISet<string> ioModuleIds,
        HardwareCatalogValidationResult result)
    {
        foreach (TestProfile profile in profiles)
        {
            ValidateStatus(profile.SourceStatus, $"perfil de teste {profile.Id} sourceStatus", result);
            ValidateStatus(profile.ValidationStatus, $"perfil de teste {profile.Id} validationStatus", result);
            ValidateReferences(profile.ApplicableFamilies, familyIds, $"perfil de teste {profile.Id} applicableFamilies", result);
            ValidateReferences(profile.ApplicableIoModules, ioModuleIds, $"perfil de teste {profile.Id} applicableIoModules", result);
        }
    }

    private static void ValidateUniqueIds<T>(
        IEnumerable<T> items,
        Func<T, string> getId,
        string itemType,
        HardwareCatalogValidationResult result)
    {
        HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);

        foreach (T item in items)
        {
            string id = getId(item);

            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddError($"Existe {itemType} sem id.");
                continue;
            }

            if (!ids.Add(id))
            {
                result.AddError($"Id duplicado em {itemType}: {id}.");
            }
        }
    }

    private static void ValidateGlobalUniqueIds(
        HardwareCatalog catalog,
        HardwareCatalogValidationResult result)
    {
        Dictionary<string, string> ownerById = new(StringComparer.OrdinalIgnoreCase);

        AddGlobalIds(catalog.Families.Select(family => (family.Id, Owner: "familia")), ownerById, result);
        AddGlobalIds(catalog.Models.Select(model => (model.Id, Owner: "modelo")), ownerById, result);
        AddGlobalIds(catalog.IoModules.Select(module => (module.Id, Owner: "modulo de I/O")), ownerById, result);
        AddGlobalIds(catalog.CommunicationProfiles.Select(profile => (profile.Id, Owner: "perfil de comunicacao")), ownerById, result);
        AddGlobalIds(catalog.TestProfiles.Select(profile => (profile.Id, Owner: "perfil de teste")), ownerById, result);
    }

    private static void AddGlobalIds(
        IEnumerable<(string Id, string Owner)> items,
        IDictionary<string, string> ownerById,
        HardwareCatalogValidationResult result)
    {
        foreach ((string id, string owner) in items)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            if (ownerById.TryGetValue(id, out string? existingOwner))
            {
                result.AddError($"Id global duplicado: {id} usado em {existingOwner} e {owner}.");
                continue;
            }

            ownerById.Add(id, owner);
        }
    }

    private static void ValidateRequiredIds(
        IEnumerable<string> requiredIds,
        ISet<string> actualIds,
        string description,
        HardwareCatalogValidationResult result)
    {
        foreach (string requiredId in requiredIds)
        {
            if (!actualIds.Contains(requiredId))
            {
                result.AddError($"{description} ausente: {requiredId}.");
            }
        }
    }

    private static void ValidateReference(
        string referencedId,
        ISet<string> validIds,
        string context,
        HardwareCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(referencedId))
        {
            result.AddError($"{context} deve ser informado.");
            return;
        }

        if (!validIds.Contains(referencedId))
        {
            result.AddError($"{context} referencia id inexistente: {referencedId}.");
        }
    }

    private static void ValidateReferences(
        IEnumerable<string> referencedIds,
        ISet<string> validIds,
        string context,
        HardwareCatalogValidationResult result)
    {
        foreach (string referencedId in referencedIds)
        {
            if (string.IsNullOrWhiteSpace(referencedId))
            {
                result.AddError($"{context} contem referencia vazia.");
                continue;
            }

            if (!validIds.Contains(referencedId))
            {
                result.AddError($"{context} referencia id inexistente: {referencedId}.");
            }
        }
    }

    private static void ValidateStatus(
        string status,
        string context,
        HardwareCatalogValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            result.AddError($"{context} deve ser informado.");
            return;
        }

        if (!AllowedStatuses.Contains(status))
        {
            result.AddError($"{context} usa status invalido: {status}.");
        }
    }

    private static HashSet<string> BuildIdSet(IEnumerable<string> ids)
    {
        return new HashSet<string>(
            ids.Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsVerified(string status)
    {
        return string.Equals(status, "verified_in_bench", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class HardwareCatalogValidationResult
{
    private readonly List<string> _errors = [];
    private readonly List<string> _warnings = [];

    public IReadOnlyList<string> Errors => _errors;

    public IReadOnlyList<string> Warnings => _warnings;

    public bool IsValid => _errors.Count == 0;

    public void AddError(string error)
    {
        _errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        _warnings.Add(warning);
    }

    public string ToDisplayText()
    {
        List<string> lines = [];

        foreach (string error in _errors)
        {
            lines.Add($"ERRO: {error}");
        }

        foreach (string warning in _warnings)
        {
            lines.Add($"AVISO: {warning}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
