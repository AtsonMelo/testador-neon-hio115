using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Hardware;

public sealed class HardwareProfileResolver
{
    public HardwareProfileResolution Resolve(
        HardwareCatalog? catalog,
        string? selectedFamilyId,
        string? selectedModelId,
        string? selectedModuleId,
        string? selectedCommunicationProfileId,
        string? selectedTestProfileId)
    {
        HardwareCatalog safeCatalog = catalog ?? HardwareCatalog.Empty;
        HardwareFamily? family = FindFamily(safeCatalog, selectedFamilyId);
        HardwareModel? model = FindModel(safeCatalog, selectedModelId);
        IoModuleDefinition? module = FindIoModule(safeCatalog, selectedModuleId);
        CommunicationProfile? communicationProfile =
            FindCommunicationProfile(safeCatalog, selectedCommunicationProfileId);
        TestProfile? testProfile = FindTestProfile(safeCatalog, selectedTestProfileId);

        IReadOnlyList<IoModuleDefinition> compatibleModules = GetCompatibleModules(safeCatalog, family, model);
        IReadOnlyList<CommunicationProfile> possibleCommunicationProfiles =
            GetPossibleCommunicationProfiles(safeCatalog, family, model);
        IReadOnlyList<TestProfile> applicableTestProfiles =
            GetApplicableTestProfiles(safeCatalog, family, model, module);

        string[] pendingItems = BuildPendingItems(
            safeCatalog,
            family,
            model,
            module,
            communicationProfile,
            testProfile);

        string[] fieldObservedItems = BuildFieldObservedItems(family, model, module);
        string[] benchValidationNeeds = BuildBenchValidationNeeds(
            model,
            module,
            communicationProfile,
            testProfile,
            pendingItems);

        return new HardwareProfileResolution(
            family,
            model,
            module,
            communicationProfile,
            testProfile,
            compatibleModules,
            possibleCommunicationProfiles,
            applicableTestProfiles,
            pendingItems,
            fieldObservedItems,
            benchValidationNeeds);
    }

    private static HardwareFamily? FindFamily(HardwareCatalog catalog, string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : catalog.FindFamily(id);
    }

    private static HardwareModel? FindModel(HardwareCatalog catalog, string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : catalog.FindModel(id);
    }

    private static IoModuleDefinition? FindIoModule(HardwareCatalog catalog, string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : catalog.FindIoModule(id);
    }

    private static CommunicationProfile? FindCommunicationProfile(HardwareCatalog catalog, string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : catalog.FindCommunicationProfile(id);
    }

    private static TestProfile? FindTestProfile(HardwareCatalog catalog, string? id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : catalog.FindTestProfile(id);
    }

    private static IReadOnlyList<IoModuleDefinition> GetCompatibleModules(
        HardwareCatalog catalog,
        HardwareFamily? family,
        HardwareModel? model)
    {
        List<IoModuleDefinition> modules = [];

        if (model is not null)
        {
            AddModulesById(catalog, modules, model.SupportedIoModules);
            return modules.OrderBy(item => item.DisplayName).ToArray();
        }

        if (modules.Count == 0 && family is not null)
        {
            AddModulesById(catalog, modules, family.SupportedIoModules);

            foreach (IoModuleDefinition module in catalog.GetIoModulesByFamily(family.Id))
            {
                AddUniqueModule(modules, module);
            }
        }

        if (modules.Count == 0 && family is null)
        {
            foreach (IoModuleDefinition module in catalog.IoModules)
            {
                AddUniqueModule(modules, module);
            }
        }

        return modules.OrderBy(item => item.DisplayName).ToArray();
    }

    private static IReadOnlyList<CommunicationProfile> GetPossibleCommunicationProfiles(
        HardwareCatalog catalog,
        HardwareFamily? family,
        HardwareModel? model)
    {
        List<CommunicationProfile> profiles = [];

        if (model is not null)
        {
            AddCommunicationProfilesById(catalog, profiles, model.DefaultCommunicationProfiles);
        }

        if (family is not null)
        {
            AddCommunicationProfilesById(catalog, profiles, family.DefaultCommunicationProfiles);

            foreach (CommunicationProfile profile in catalog.GetCommunicationProfilesByFamily(family.Id))
            {
                AddUniqueCommunicationProfile(profiles, profile);
            }
        }

        if (profiles.Count == 0 && family is null)
        {
            foreach (CommunicationProfile profile in catalog.CommunicationProfiles)
            {
                AddUniqueCommunicationProfile(profiles, profile);
            }
        }

        return profiles.OrderBy(item => item.DisplayName).ToArray();
    }

    private static IReadOnlyList<TestProfile> GetApplicableTestProfiles(
        HardwareCatalog catalog,
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? module)
    {
        List<TestProfile> profiles = [];

        if (model is not null)
        {
            AddTestProfilesById(catalog, profiles, model.TestProfiles);
        }

        if (module is not null)
        {
            AddTestProfilesById(catalog, profiles, module.TestProfiles);

            foreach (TestProfile profile in catalog.GetTestProfilesByModule(module.Id))
            {
                AddUniqueTestProfile(profiles, profile);
            }
        }

        if (profiles.Count == 0 && family is not null)
        {
            AddTestProfilesById(catalog, profiles, family.TestProfiles);

            foreach (TestProfile profile in catalog.TestProfiles)
            {
                if (profile.ApplicableFamilies.Any(id => HasId(id, family.Id)))
                {
                    AddUniqueTestProfile(profiles, profile);
                }
            }
        }

        if (profiles.Count == 0 && family is null)
        {
            foreach (TestProfile profile in catalog.TestProfiles)
            {
                AddUniqueTestProfile(profiles, profile);
            }
        }

        return profiles.OrderBy(item => item.DisplayName).ToArray();
    }

    private static string[] BuildPendingItems(
        HardwareCatalog catalog,
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? module,
        CommunicationProfile? communicationProfile,
        TestProfile? testProfile)
    {
        List<string> pending = [];

        if (catalog.Families.Count == 0)
        {
            pending.Add("Catalogo vazio ou indisponivel.");
        }

        AddStatusPending(pending, "Familia", family?.DisplayName, family?.SourceStatus, family?.ValidationStatus);
        AddStatusPending(pending, "Modelo", model?.DisplayName, model?.SourceStatus, model?.ValidationStatus);
        AddStatusPending(pending, "Modulo", module?.DisplayName, module?.SourceStatus, module?.ValidationStatus);
        AddStatusPending(
            pending,
            "Perfil de comunicacao",
            communicationProfile?.DisplayName,
            communicationProfile?.SourceStatus,
            communicationProfile?.ValidationStatus);
        AddStatusPending(
            pending,
            "Perfil de teste",
            testProfile?.DisplayName,
            testProfile?.SourceStatus,
            testProfile?.ValidationStatus);

        if (family is null)
        {
            AddUnique(pending, "Familia nao selecionada.");
        }

        if (model is null)
        {
            AddUnique(pending, "Modelo nao selecionado.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.ControllerCpu))
            {
                AddUnique(pending, $"Modelo {model.DisplayName}: CPU pendente.");
            }

            if (model.SupportedIoModules.Count == 0)
            {
                AddUnique(pending, $"Modelo {model.DisplayName}: modulos compativeis pendentes.");
            }
        }

        if (module is null)
        {
            AddUnique(pending, "Modulo de I/O nao selecionado.");
        }

        if (communicationProfile is null)
        {
            AddUnique(pending, "Perfil de comunicacao nao selecionado.");
        }

        if (testProfile is null)
        {
            AddUnique(pending, "Perfil de teste nao selecionado.");
        }

        return pending.ToArray();
    }

    private static string[] BuildFieldObservedItems(
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? module)
    {
        List<string> observed = [];

        if (IsFieldObserved(family?.SourceStatus))
        {
            AddUnique(observed, $"Familia observada: {family!.DisplayName}.");
        }

        if (IsFieldObserved(model?.SourceStatus))
        {
            string cpu = string.IsNullOrWhiteSpace(model!.ControllerCpu)
                ? "CPU pendente"
                : model.ControllerCpu;
            AddUnique(observed, $"Modelo observado: {model.DisplayName} + {cpu}.");
        }

        if (IsFieldObserved(module?.SourceStatus))
        {
            AddUnique(observed, $"Modulo observado: {module!.DisplayName}.");
        }

        if (IsFieldObserved(model?.SourceStatus) && IsFieldObserved(module?.SourceStatus))
        {
            string cpu = string.IsNullOrWhiteSpace(model!.ControllerCpu)
                ? "CPU pendente"
                : model.ControllerCpu;
            AddUnique(observed, $"Conjunto observado: {model.DisplayName} + {cpu} + {module!.DisplayName}.");
        }

        if (observed.Count == 0)
        {
            observed.Add("Nenhum item selecionado esta marcado como field_observed.");
        }

        return observed.ToArray();
    }

    private static string[] BuildBenchValidationNeeds(
        HardwareModel? model,
        IoModuleDefinition? module,
        CommunicationProfile? communicationProfile,
        TestProfile? testProfile,
        IReadOnlyList<string> pendingItems)
    {
        List<string> needs =
        [
            "Confirmar programa HIstudio carregado e compativel com o conjunto.",
            "Garantir que HIstudio, XCTU e Testador nao usem a mesma COM ao mesmo tempo.",
            "Conferir porta, baud rate, slave ID, cabeamento e energia antes do teste.",
            "Registrar modelo, CPU, modulo, slot, perfil, porta, baud rate, slave ID e resultado."
        ];

        if (pendingItems.Count > 0)
        {
            needs.Add("Manter pending_manual_validation ate haver registro de bancada.");
        }

        if (module is not null && IsPending(module.ValidationStatus))
        {
            needs.Add($"Confirmar mapa e ligacao de bancada do modulo {module.DisplayName}.");
        }

        if (model is not null && IsPending(model.ValidationStatus))
        {
            needs.Add($"Validar o conjunto real antes de marcar {model.DisplayName} como verified_in_bench.");
        }

        if (communicationProfile is not null &&
            communicationProfile.Transport.Contains("wireless", StringComparison.OrdinalIgnoreCase))
        {
            needs.Add("Configurar radio no XCTU fora do app; esta tela nao configura radio.");
        }

        if (testProfile is not null && IsPending(testProfile.ValidationStatus))
        {
            needs.Add($"Executar {testProfile.Id} manualmente antes de tratar o perfil como validado.");
        }

        return needs.ToArray();
    }

    private static void AddModulesById(
        HardwareCatalog catalog,
        ICollection<IoModuleDefinition> modules,
        IEnumerable<string> moduleIds)
    {
        foreach (string moduleId in moduleIds)
        {
            IoModuleDefinition? module = catalog.FindIoModule(moduleId);

            if (module is not null)
            {
                AddUniqueModule(modules, module);
            }
        }
    }

    private static void AddCommunicationProfilesById(
        HardwareCatalog catalog,
        ICollection<CommunicationProfile> profiles,
        IEnumerable<string> profileIds)
    {
        foreach (string profileId in profileIds)
        {
            CommunicationProfile? profile = catalog.FindCommunicationProfile(profileId);

            if (profile is not null)
            {
                AddUniqueCommunicationProfile(profiles, profile);
            }
        }
    }

    private static void AddTestProfilesById(
        HardwareCatalog catalog,
        ICollection<TestProfile> profiles,
        IEnumerable<string> profileIds)
    {
        foreach (string profileId in profileIds)
        {
            TestProfile? profile = catalog.FindTestProfile(profileId);

            if (profile is not null)
            {
                AddUniqueTestProfile(profiles, profile);
            }
        }
    }

    private static void AddUniqueModule(ICollection<IoModuleDefinition> modules, IoModuleDefinition module)
    {
        if (!modules.Any(item => HasId(item.Id, module.Id)))
        {
            modules.Add(module);
        }
    }

    private static void AddUniqueCommunicationProfile(
        ICollection<CommunicationProfile> profiles,
        CommunicationProfile profile)
    {
        if (!profiles.Any(item => HasId(item.Id, profile.Id)))
        {
            profiles.Add(profile);
        }
    }

    private static void AddUniqueTestProfile(ICollection<TestProfile> profiles, TestProfile profile)
    {
        if (!profiles.Any(item => HasId(item.Id, profile.Id)))
        {
            profiles.Add(profile);
        }
    }

    private static void AddStatusPending(
        ICollection<string> pending,
        string itemType,
        string? displayName,
        string? sourceStatus,
        string? validationStatus)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return;
        }

        if (IsOfficialReferencePending(sourceStatus))
        {
            AddUnique(pending, $"{itemType} {displayName}: official_reference_pending.");
        }

        if (IsPending(sourceStatus) || IsPending(validationStatus))
        {
            AddUnique(pending, $"{itemType} {displayName}: pending_manual_validation.");
        }
    }

    private static void AddUnique(ICollection<string> items, string value)
    {
        if (!items.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(value);
        }
    }

    private static bool IsPending(string? status)
    {
        return string.Equals(status, "pending_manual_validation", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOfficialReferencePending(string? status)
    {
        return string.Equals(status, "official_reference_pending", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFieldObserved(string? status)
    {
        return string.Equals(status, "field_observed", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasId(string currentId, string requestedId)
    {
        return string.Equals(currentId, requestedId, StringComparison.OrdinalIgnoreCase);
    }
}
