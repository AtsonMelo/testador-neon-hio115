using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Hardware;

public sealed class HardwareProfileSelectionControl : UserControl
{
    private const string EmptySelectionText = "(nao disponivel)";

    private readonly HardwareCatalog _catalog;
    private readonly HardwareProfileResolver _resolver;
    private readonly HardwareTestPreparationReportFormatter _reportFormatter;
    private readonly ComboBox _familyComboBox;
    private readonly ComboBox _modelComboBox;
    private readonly ComboBox _ioModuleComboBox;
    private readonly ComboBox _communicationProfileComboBox;
    private readonly ComboBox _testProfileComboBox;
    private readonly Label _validationStatusLabel;
    private readonly Button _copyReportButton;
    private readonly TextBox _summaryTextBox;

    private bool _updatingSelection;

    public event EventHandler<HardwareProfileSelectionChangedEventArgs>? SelectionChanged;

    public HardwareProfileSelectionControl(HardwareCatalog? catalog)
    {
        _catalog = catalog ?? HardwareCatalog.Empty;
        _resolver = new HardwareProfileResolver();
        _reportFormatter = new HardwareTestPreparationReportFormatter();
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
        _copyReportButton = new Button
        {
            Text = "Copiar resumo de teste",
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 0, 0, 0)
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
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 6)
        };

        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176F));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        statusLayout.Controls.Add(CreateSelectorLabel("Status de validacao:"), 0, 0);
        statusLayout.Controls.Add(_validationStatusLabel, 1, 0);
        statusLayout.Controls.Add(_copyReportButton, 2, 0);

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
        _copyReportButton.Click += (_, _) => CopyPreparationReportToClipboard();
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
        HardwareProfileResolution resolution = _resolver.Resolve(
            _catalog,
            family?.Id,
            model?.Id,
            null,
            null,
            null);

        _updatingSelection = true;
        PopulateComboBox(_ioModuleComboBox, resolution.CompatibleModules, FormatIoModule);
        _updatingSelection = false;
    }

    private void RefreshCommunicationProfileChoices()
    {
        HardwareFamily? family = GetSelected<HardwareFamily>(_familyComboBox);
        HardwareModel? model = GetSelected<HardwareModel>(_modelComboBox);
        HardwareProfileResolution resolution = _resolver.Resolve(
            _catalog,
            family?.Id,
            model?.Id,
            null,
            null,
            null);

        _updatingSelection = true;
        PopulateComboBox(
            _communicationProfileComboBox,
            resolution.PossibleCommunicationProfiles,
            FormatCommunicationProfile);
        _updatingSelection = false;
    }

    private void RefreshTestProfileChoices()
    {
        HardwareFamily? family = GetSelected<HardwareFamily>(_familyComboBox);
        HardwareModel? model = GetSelected<HardwareModel>(_modelComboBox);
        IoModuleDefinition? module = GetSelected<IoModuleDefinition>(_ioModuleComboBox);
        HardwareProfileResolution resolution = _resolver.Resolve(
            _catalog,
            family?.Id,
            model?.Id,
            module?.Id,
            null,
            null);

        _updatingSelection = true;
        PopulateComboBox(
            _testProfileComboBox,
            resolution.ApplicableTestProfiles,
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
        HardwareProfileResolution resolution = _resolver.Resolve(
            _catalog,
            family?.Id,
            model?.Id,
            module?.Id,
            communicationProfile?.Id,
            testProfile?.Id);

        SelectedProfile = new SelectedHardwareProfile(resolution);

        _validationStatusLabel.Text = BuildValidationStatusText(SelectedProfile);
        _summaryTextBox.Text = _reportFormatter.Format(SelectedProfile).Text;
        _copyReportButton.Text = "Copiar resumo de teste";

        SelectionChanged?.Invoke(
            this,
            new HardwareProfileSelectionChangedEventArgs(SelectedProfile));
    }

    private void CopyPreparationReportToClipboard()
    {
        string reportText = _summaryTextBox.Text;

        if (string.IsNullOrWhiteSpace(reportText))
        {
            reportText = _reportFormatter.Format(SelectedProfile).Text;
        }

        try
        {
            Clipboard.SetText(reportText);
            _copyReportButton.Text = "Resumo copiado";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Falha ao copiar resumo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
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
