using System.Text;
using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Hardware;

public sealed class HardwareProfileSelectionControl : UserControl
{
    private const string EmptySelectionText = "(nao disponivel)";

    private readonly HardwareCatalog _catalog;
    private readonly ComboBox _familyComboBox;
    private readonly ComboBox _modelComboBox;
    private readonly ComboBox _ioModuleComboBox;
    private readonly ComboBox _communicationProfileComboBox;
    private readonly ComboBox _testProfileComboBox;
    private readonly Label _validationStatusLabel;
    private readonly TextBox _summaryTextBox;

    private bool _updatingSelection;

    public event EventHandler<HardwareProfileSelectionChangedEventArgs>? SelectionChanged;

    public HardwareProfileSelectionControl(HardwareCatalog? catalog)
    {
        _catalog = catalog ?? HardwareCatalog.Empty;
        SelectedProfile = SelectedHardwareProfile.Empty;

        Dock = DockStyle.Fill;
        MinimumSize = new Size(720, 420);

        _familyComboBox = CreateComboBox();
        _modelComboBox = CreateComboBox();
        _ioModuleComboBox = CreateComboBox();
        _communicationProfileComboBox = CreateComboBox();
        _testProfileComboBox = CreateComboBox();
        _validationStatusLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoEllipsis = true
        };
        _summaryTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 9F),
            WordWrap = true
        };

        BuildLayout();
        WireEvents();
        LoadCatalogSelections();
    }

    public SelectedHardwareProfile SelectedProfile { get; private set; }

    private void BuildLayout()
    {
        GroupBox groupBox = new()
        {
            Text = "Selecao de perfil de hardware",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 16, 10, 10)
        };

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 154F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Label safetyLabel = new()
        {
            Text = "Selecao informativa: nao altera parametros Modbus e nao envia comandos ao CLP.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoEllipsis = true
        };

        TableLayoutPanel selectors = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Margin = new Padding(0, 0, 0, 8)
        };

        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (int row = 0; row < 5; row++)
        {
            selectors.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
        }

        AddSelectionRow(selectors, 0, "Familia:", _familyComboBox);
        AddSelectionRow(selectors, 1, "Modelo:", _modelComboBox);
        AddSelectionRow(selectors, 2, "Modulo de I/O:", _ioModuleComboBox);
        AddSelectionRow(selectors, 3, "Comunicacao:", _communicationProfileComboBox);
        AddSelectionRow(selectors, 4, "Perfil de teste:", _testProfileComboBox);

        TableLayoutPanel statusLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 6)
        };

        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        statusLayout.Controls.Add(CreateSelectorLabel("Status de validacao:"), 0, 0);
        statusLayout.Controls.Add(_validationStatusLabel, 1, 0);

        root.Controls.Add(safetyLabel, 0, 0);
        root.Controls.Add(selectors, 0, 1);
        root.Controls.Add(statusLayout, 0, 2);
        root.Controls.Add(_summaryTextBox, 0, 3);

        groupBox.Controls.Add(root);
        Controls.Add(groupBox);
    }

    private void WireEvents()
    {
        _familyComboBox.SelectedIndexChanged += (_, _) => FamilySelectionChanged();
        _modelComboBox.SelectedIndexChanged += (_, _) => ModelSelectionChanged();
        _ioModuleComboBox.SelectedIndexChanged += (_, _) => IoModuleSelectionChanged();
        _communicationProfileComboBox.SelectedIndexChanged += (_, _) => UpdateSelectedProfile();
        _testProfileComboBox.SelectedIndexChanged += (_, _) => UpdateSelectedProfile();
    }

    private void LoadCatalogSelections()
    {
        _updatingSelection = true;
        PopulateComboBox(
            _familyComboBox,
            _catalog.Families.OrderBy(item => item.DisplayName).ToArray(),
            FormatFamily);
        _updatingSelection = false;

        RefreshModelChoices();
        RefreshIoModuleChoices();
        RefreshCommunicationProfileChoices();
        RefreshTestProfileChoices();
        UpdateSelectedProfile();
    }

    private void FamilySelectionChanged()
    {
        if (_updatingSelection)
        {
            return;
        }

        RefreshModelChoices();
        RefreshIoModuleChoices();
        RefreshCommunicationProfileChoices();
        RefreshTestProfileChoices();
        UpdateSelectedProfile();
    }

    private void ModelSelectionChanged()
    {
        if (_updatingSelection)
        {
            return;
        }

        RefreshIoModuleChoices();
        RefreshCommunicationProfileChoices();
        RefreshTestProfileChoices();
        UpdateSelectedProfile();
    }

    private void IoModuleSelectionChanged()
    {
        if (_updatingSelection)
        {
            return;
        }

        RefreshTestProfileChoices();
        UpdateSelectedProfile();
    }

    private void RefreshModelChoices()
    {
        HardwareFamily? family = GetSelected<HardwareFamily>(_familyComboBox);
        IReadOnlyList<HardwareModel> models = family is null
            ? _catalog.Models.OrderBy(item => item.DisplayName).ToArray()
            : _catalog.GetModelsByFamily(family.Id).OrderBy(item => item.DisplayName).ToArray();

        _updatingSelection = true;
        PopulateComboBox(_modelComboBox, models, FormatModel);
        _updatingSelection = false;
    }

    private void RefreshIoModuleChoices()
    {
        HardwareFamily? family = GetSelected<HardwareFamily>(_familyComboBox);
        HardwareModel? model = GetSelected<HardwareModel>(_modelComboBox);

        _updatingSelection = true;
        PopulateComboBox(_ioModuleComboBox, GetCompatibleModules(family, model), FormatIoModule);
        _updatingSelection = false;
    }

    private void RefreshCommunicationProfileChoices()
    {
        HardwareFamily? family = GetSelected<HardwareFamily>(_familyComboBox);
        HardwareModel? model = GetSelected<HardwareModel>(_modelComboBox);

        _updatingSelection = true;
        PopulateComboBox(
            _communicationProfileComboBox,
            GetPossibleCommunicationProfiles(family, model),
            FormatCommunicationProfile);
        _updatingSelection = false;
    }

    private void RefreshTestProfileChoices()
    {
        HardwareFamily? family = GetSelected<HardwareFamily>(_familyComboBox);
        HardwareModel? model = GetSelected<HardwareModel>(_modelComboBox);
        IoModuleDefinition? module = GetSelected<IoModuleDefinition>(_ioModuleComboBox);

        _updatingSelection = true;
        PopulateComboBox(
            _testProfileComboBox,
            GetApplicableTestProfiles(family, model, module),
            FormatTestProfile);
        _updatingSelection = false;
    }

    private void UpdateSelectedProfile()
    {
        if (_updatingSelection)
        {
            return;
        }

        HardwareFamily? family = GetSelected<HardwareFamily>(_familyComboBox);
        HardwareModel? model = GetSelected<HardwareModel>(_modelComboBox);
        IoModuleDefinition? module = GetSelected<IoModuleDefinition>(_ioModuleComboBox);
        CommunicationProfile? communicationProfile =
            GetSelected<CommunicationProfile>(_communicationProfileComboBox);
        TestProfile? testProfile = GetSelected<TestProfile>(_testProfileComboBox);

        IReadOnlyList<IoModuleDefinition> compatibleModules = GetCompatibleModules(family, model);
        IReadOnlyList<CommunicationProfile> possibleCommunicationProfiles =
            GetPossibleCommunicationProfiles(family, model);
        IReadOnlyList<TestProfile> applicableTestProfiles =
            GetApplicableTestProfiles(family, model, module);

        string[] pendingItems = BuildPendingItems(
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

        SelectedProfile = new SelectedHardwareProfile(
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

        _validationStatusLabel.Text = BuildValidationStatusText(SelectedProfile);
        _summaryTextBox.Text = BuildSummaryText(SelectedProfile);

        SelectionChanged?.Invoke(
            this,
            new HardwareProfileSelectionChangedEventArgs(SelectedProfile));
    }

    private IReadOnlyList<IoModuleDefinition> GetCompatibleModules(
        HardwareFamily? family,
        HardwareModel? model)
    {
        List<IoModuleDefinition> modules = [];

        if (model is not null)
        {
            AddModulesById(modules, model.SupportedIoModules);
            return modules.OrderBy(item => item.DisplayName).ToArray();
        }

        if (modules.Count == 0 && family is not null)
        {
            AddModulesById(modules, family.SupportedIoModules);

            foreach (IoModuleDefinition module in _catalog.GetIoModulesByFamily(family.Id))
            {
                AddUniqueModule(modules, module);
            }
        }

        if (modules.Count == 0 && family is null)
        {
            foreach (IoModuleDefinition module in _catalog.IoModules)
            {
                AddUniqueModule(modules, module);
            }
        }

        return modules.OrderBy(item => item.DisplayName).ToArray();
    }

    private IReadOnlyList<CommunicationProfile> GetPossibleCommunicationProfiles(
        HardwareFamily? family,
        HardwareModel? model)
    {
        List<CommunicationProfile> profiles = [];

        if (model is not null)
        {
            AddCommunicationProfilesById(profiles, model.DefaultCommunicationProfiles);
        }

        if (family is not null)
        {
            AddCommunicationProfilesById(profiles, family.DefaultCommunicationProfiles);

            foreach (CommunicationProfile profile in _catalog.GetCommunicationProfilesByFamily(family.Id))
            {
                AddUniqueCommunicationProfile(profiles, profile);
            }
        }

        if (profiles.Count == 0 && family is null)
        {
            foreach (CommunicationProfile profile in _catalog.CommunicationProfiles)
            {
                AddUniqueCommunicationProfile(profiles, profile);
            }
        }

        return profiles.OrderBy(item => item.DisplayName).ToArray();
    }

    private IReadOnlyList<TestProfile> GetApplicableTestProfiles(
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? module)
    {
        List<TestProfile> profiles = [];

        if (model is not null)
        {
            AddTestProfilesById(profiles, model.TestProfiles);
        }

        if (module is not null)
        {
            AddTestProfilesById(profiles, module.TestProfiles);

            foreach (TestProfile profile in _catalog.GetTestProfilesByModule(module.Id))
            {
                AddUniqueTestProfile(profiles, profile);
            }
        }

        if (profiles.Count == 0 && family is not null)
        {
            AddTestProfilesById(profiles, family.TestProfiles);

            foreach (TestProfile profile in _catalog.TestProfiles)
            {
                if (profile.ApplicableFamilies.Any(id => HasId(id, family.Id)))
                {
                    AddUniqueTestProfile(profiles, profile);
                }
            }
        }

        if (profiles.Count == 0 && family is null)
        {
            foreach (TestProfile profile in _catalog.TestProfiles)
            {
                AddUniqueTestProfile(profiles, profile);
            }
        }

        return profiles.OrderBy(item => item.DisplayName).ToArray();
    }

    private string[] BuildPendingItems(
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? module,
        CommunicationProfile? communicationProfile,
        TestProfile? testProfile)
    {
        List<string> pending = [];

        if (_catalog.Families.Count == 0)
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

    private string[] BuildFieldObservedItems(
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

    private string[] BuildBenchValidationNeeds(
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

    private string BuildSummaryText(SelectedHardwareProfile profile)
    {
        StringBuilder builder = new();

        builder.AppendLine("Perfil selecionado");
        builder.AppendLine($"Familia: {FormatSelected(profile.Family?.DisplayName, profile.Family?.Id)}");
        builder.AppendLine($"Modelo: {FormatSelected(profile.Model?.DisplayName, profile.Model?.Id)}");
        builder.AppendLine($"CPU: {FormatValue(profile.Model?.ControllerCpu)}");
        builder.AppendLine($"Modulo de I/O: {FormatSelected(profile.IoModule?.DisplayName, profile.IoModule?.Id)}");
        builder.AppendLine(
            $"Comunicacao: {FormatSelected(profile.CommunicationProfile?.DisplayName, profile.CommunicationProfile?.Id)}");
        builder.AppendLine($"Perfil de teste: {FormatSelected(profile.TestProfile?.DisplayName, profile.TestProfile?.Id)}");
        builder.AppendLine();

        builder.AppendLine("Status do catalogo");
        AppendItemStatus(builder, "Familia", profile.Family?.SourceStatus, profile.Family?.ValidationStatus);
        AppendItemStatus(builder, "Modelo", profile.Model?.SourceStatus, profile.Model?.ValidationStatus);
        AppendItemStatus(builder, "Modulo", profile.IoModule?.SourceStatus, profile.IoModule?.ValidationStatus);
        AppendItemStatus(
            builder,
            "Comunicacao",
            profile.CommunicationProfile?.SourceStatus,
            profile.CommunicationProfile?.ValidationStatus);
        AppendItemStatus(builder, "Teste", profile.TestProfile?.SourceStatus, profile.TestProfile?.ValidationStatus);
        builder.AppendLine();

        builder.AppendLine("Modulos compativeis");
        AppendList(builder, profile.CompatibleModules.Select(item => $"{item.DisplayName} ({item.Id})"));
        builder.AppendLine();

        builder.AppendLine("Perfis de comunicacao possiveis");
        AppendList(builder, profile.PossibleCommunicationProfiles.Select(item => $"{item.DisplayName} ({item.Id})"));
        builder.AppendLine();

        builder.AppendLine("Testes aplicaveis");
        AppendList(builder, profile.ApplicableTestProfiles.Select(item => $"{item.DisplayName} ({item.Id})"));
        builder.AppendLine();

        builder.AppendLine("Itens pendentes");
        AppendList(builder, profile.PendingItems);
        builder.AppendLine();

        builder.AppendLine("Itens observados em campo/bancada");
        AppendList(builder, profile.FieldObservedItems);
        builder.AppendLine();

        builder.AppendLine("Necessidades de validacao em bancada");
        AppendList(builder, profile.BenchValidationNeeds);
        builder.AppendLine();

        builder.AppendLine("Seguranca operacional");
        builder.AppendLine("- Esta tela nao altera porta, baud rate, slave ID, paridade ou timeout.");
        builder.AppendLine("- Esta tela nao envia comandos fisicos ao CLP.");
        builder.AppendLine("- Parametros reais continuam no painel de conexao normal.");

        return builder.ToString();
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

    private static void AppendList(StringBuilder builder, IEnumerable<string> items)
    {
        bool hasItem = false;

        foreach (string item in items)
        {
            builder.AppendLine($"- {item}");
            hasItem = true;
        }

        if (!hasItem)
        {
            builder.AppendLine("- Nenhum item disponivel no catalogo.");
        }
    }

    private static string BuildValidationStatusText(SelectedHardwareProfile profile)
    {
        if (profile.PendingItems.Any(item => item.Contains("Catalogo vazio", StringComparison.OrdinalIgnoreCase)))
        {
            return "Catalogo vazio ou indisponivel.";
        }

        if (profile.RequiresManualValidation)
        {
            return "pending_manual_validation - validar manualmente antes de uso operacional.";
        }

        return profile.IsComplete
            ? "Sem pendencias explicitas no catalogo selecionado."
            : "Selecao incompleta.";
    }

    private static void AddSelectionRow(
        TableLayoutPanel layout,
        int row,
        string labelText,
        ComboBox comboBox)
    {
        layout.Controls.Add(CreateSelectorLabel(labelText), 0, row);
        layout.Controls.Add(comboBox, 1, row);
    }

    private static Label CreateSelectorLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F),
            AutoEllipsis = true,
            Margin = new Padding(0, 0, 8, 0)
        };
    }

    private static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            IntegralHeight = false,
            Margin = new Padding(0, 1, 0, 3)
        };
    }

    private static void PopulateComboBox<T>(
        ComboBox comboBox,
        IReadOnlyList<T> items,
        Func<T, string> formatText)
        where T : class
    {
        comboBox.BeginUpdate();
        comboBox.Items.Clear();

        if (items.Count == 0)
        {
            comboBox.Items.Add(new SelectionItem<T>(null, EmptySelectionText));
            comboBox.SelectedIndex = 0;
            comboBox.Enabled = false;
            comboBox.EndUpdate();
            return;
        }

        foreach (T item in items)
        {
            comboBox.Items.Add(new SelectionItem<T>(item, formatText(item)));
        }

        comboBox.Enabled = true;
        comboBox.SelectedIndex = 0;
        comboBox.EndUpdate();
    }

    private static T? GetSelected<T>(ComboBox comboBox)
        where T : class
    {
        return (comboBox.SelectedItem as SelectionItem<T>)?.Value;
    }

    private void AddModulesById(ICollection<IoModuleDefinition> modules, IEnumerable<string> moduleIds)
    {
        foreach (string moduleId in moduleIds)
        {
            IoModuleDefinition? module = _catalog.FindIoModule(moduleId);

            if (module is not null)
            {
                AddUniqueModule(modules, module);
            }
        }
    }

    private void AddCommunicationProfilesById(
        ICollection<CommunicationProfile> profiles,
        IEnumerable<string> profileIds)
    {
        foreach (string profileId in profileIds)
        {
            CommunicationProfile? profile = _catalog.FindCommunicationProfile(profileId);

            if (profile is not null)
            {
                AddUniqueCommunicationProfile(profiles, profile);
            }
        }
    }

    private void AddTestProfilesById(ICollection<TestProfile> profiles, IEnumerable<string> profileIds)
    {
        foreach (string profileId in profileIds)
        {
            TestProfile? profile = _catalog.FindTestProfile(profileId);

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

    private static string FormatFamily(HardwareFamily family)
    {
        return $"{family.DisplayName} ({family.Id})";
    }

    private static string FormatModel(HardwareModel model)
    {
        string cpu = string.IsNullOrWhiteSpace(model.ControllerCpu)
            ? "CPU pendente"
            : model.ControllerCpu;

        return $"{model.DisplayName} ({model.Id}) - {cpu}";
    }

    private static string FormatIoModule(IoModuleDefinition module)
    {
        return $"{module.DisplayName} ({module.Id})";
    }

    private static string FormatCommunicationProfile(CommunicationProfile profile)
    {
        return $"{profile.DisplayName} ({profile.Id})";
    }

    private static string FormatTestProfile(TestProfile profile)
    {
        return $"{profile.DisplayName} ({profile.Id})";
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

    private sealed class SelectionItem<T>
        where T : class
    {
        public SelectionItem(T? value, string text)
        {
            Value = value;
            Text = text;
        }

        public T? Value { get; }

        private string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }
}
