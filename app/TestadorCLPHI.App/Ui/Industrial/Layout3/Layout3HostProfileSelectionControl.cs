using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Selecao local de perfil/hardware do host Layout 3 read-only. Le familia,
/// modelo, modulo e teste do catalogo local ja carregado e reflete a escolha na
/// tela. Nao abre comunicacao, nao envia comando e nao escreve registrador.
/// </summary>
internal sealed class Layout3HostProfileSelectionControl : UserControl
{
    private readonly Layout3ThemePalette _palette;
    private readonly HardwareCatalog _catalog;
    private readonly string _modeSummary;

    private readonly ComboBox _familyCombo;
    private readonly ComboBox _modelCombo;
    private readonly ComboBox _moduleCombo;
    private readonly ComboBox _testCombo;

    private readonly Label _communicationValue;
    private readonly Label _validationLabel;
    private readonly TextBox _summaryTextBox;

    private bool _suppressEvents;

    /// <summary>
    /// Disparado a cada selecao confirmada, com uma linha pronta para o log local.
    /// </summary>
    public event Action<string>? SelectionLogged;

    public Layout3ProfileSelection CurrentSelection { get; private set; } = Layout3ProfileSelection.Empty;

    public Layout3HostProfileSelectionControl(
        HardwareCatalog hardwareCatalog,
        Layout3ThemePalette palette,
        string modeSummary)
    {
        _palette = palette;
        _catalog = hardwareCatalog ?? HardwareCatalog.Empty;
        _modeSummary = modeSummary;
        BackColor = _palette.Surface;
        BorderStyle = BorderStyle.FixedSingle;

        _familyCombo = CreateCombo();
        _modelCombo = CreateCombo();
        _moduleCombo = CreateCombo();
        _testCombo = CreateCombo();

        _communicationValue = CreateLabel("Nao disponivel", 8.5F, FontStyle.Bold, _palette.Text);
        _validationLabel = CreateLabel(string.Empty, 8F, FontStyle.Bold, _palette.AccentBlue);
        _summaryTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            BackColor = _palette.Field,
            ForeColor = _palette.SummaryText,
            Font = new Font("Consolas", 8F),
            Margin = new Padding(0, 4, 0, 4)
        };

        BuildLayout();
        WireEvents();
        PopulateInitial();
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = _palette.Surface,
            Padding = new Padding(14)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 168F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(CreateSelectors(), 0, 1);
        root.Controls.Add(CreateCommunicationField(), 0, 2);
        root.Controls.Add(CreateValidationArea(), 0, 3);
        root.Controls.Add(_summaryTextBox, 0, 4);
        root.Controls.Add(CreateReadOnlyNote(), 0, 5);
        Controls.Add(root);
    }

    private Control CreateHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = _palette.Surface
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

        Label index = CreateLabel("03", 8F, FontStyle.Bold, _palette.AccentBlue, ContentAlignment.MiddleCenter);
        index.BackColor = _palette.Field;
        index.Margin = new Padding(0, 4, 7, 4);
        header.Controls.Add(index, 0, 0);
        header.SetRowSpan(index, 2);
        header.Controls.Add(CreateLabel("PERFIL / SELECAO LOCAL", 10F, FontStyle.Bold, _palette.Text), 1, 0);
        header.Controls.Add(CreateLabel("Catalogo local somente leitura", 8F, FontStyle.Regular, _palette.MutedText), 1, 1);
        return header;
    }

    private Control CreateSelectors()
    {
        TableLayoutPanel selectors = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            BackColor = _palette.Surface,
            Margin = new Padding(0, 2, 0, 4)
        };
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
        selectors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (int row = 0; row < 4; row++)
        {
            selectors.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        }

        AddSelectorRow(selectors, 0, "FAMILIA", _familyCombo);
        AddSelectorRow(selectors, 1, "MODELO", _modelCombo);
        AddSelectorRow(selectors, 2, "MODULO I/O", _moduleCombo);
        AddSelectorRow(selectors, 3, "TESTE", _testCombo);
        return selectors;
    }

    private void AddSelectorRow(TableLayoutPanel host, int row, string title, ComboBox combo)
    {
        Label label = CreateLabel(title, 7.5F, FontStyle.Bold, _palette.MutedText);
        label.Margin = new Padding(0, 2, 6, 2);
        host.Controls.Add(label, 0, row);
        host.Controls.Add(combo, 1, row);
    }

    private Control CreateCommunicationField()
    {
        TableLayoutPanel field = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Field,
            Margin = new Padding(0, 2, 0, 2),
            Padding = new Padding(10, 0, 10, 0)
        };
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        Label title = CreateLabel("COMUNIC.", 7.5F, FontStyle.Bold, _palette.MutedText);
        title.BackColor = _palette.Field;
        _communicationValue.BackColor = _palette.Field;
        field.Controls.Add(title, 0, 0);
        field.Controls.Add(_communicationValue, 1, 0);
        return field;
    }

    private Control CreateValidationArea()
    {
        TableLayoutPanel validation = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Raised,
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(10, 3, 10, 3)
        };
        validation.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        validation.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        Label title = CreateLabel("STATUS DE VALIDACAO", 7F, FontStyle.Bold, _palette.MutedText);
        title.BackColor = _palette.Raised;
        _validationLabel.BackColor = _palette.Raised;
        validation.Controls.Add(title, 0, 0);
        validation.Controls.Add(_validationLabel, 0, 1);
        return validation;
    }

    private Control CreateReadOnlyNote()
    {
        Label note = CreateLabel(
            "Selecao local: nenhuma comunicacao aberta, 0 comandos fisicos.",
            7.5F,
            FontStyle.Regular,
            _palette.Warning,
            ContentAlignment.MiddleLeft);
        note.BackColor = _palette.WarningBackground;
        note.Padding = new Padding(8, 0, 8, 0);
        return note;
    }

    private void WireEvents()
    {
        _familyCombo.SelectedIndexChanged += (_, _) => OnFamilyChanged();
        _modelCombo.SelectedIndexChanged += (_, _) => OnLeafChanged();
        _moduleCombo.SelectedIndexChanged += (_, _) => OnModuleChanged();
        _testCombo.SelectedIndexChanged += (_, _) => OnLeafChanged();
    }

    private void PopulateInitial()
    {
        _suppressEvents = true;
        SetEntries(_familyCombo, _catalog.Families
            .OrderBy(item => item.DisplayName)
            .Select(item => new ComboEntry(item.DisplayName, item)));
        HardwareFamily? family = Selected<HardwareFamily>(_familyCombo);
        PopulateDependents(family);
        _suppressEvents = false;
        ApplySelection();
    }

    private void OnFamilyChanged()
    {
        if (_suppressEvents)
        {
            return;
        }

        _suppressEvents = true;
        PopulateDependents(Selected<HardwareFamily>(_familyCombo));
        _suppressEvents = false;
        ApplySelection();
    }

    private void OnModuleChanged()
    {
        if (_suppressEvents)
        {
            return;
        }

        _suppressEvents = true;
        PopulateTests(Selected<IoModuleDefinition>(_moduleCombo));
        _suppressEvents = false;
        ApplySelection();
    }

    private void OnLeafChanged()
    {
        if (_suppressEvents)
        {
            return;
        }

        ApplySelection();
    }

    private void PopulateDependents(HardwareFamily? family)
    {
        SetEntries(_modelCombo, family is null
            ? Enumerable.Empty<ComboEntry>()
            : _catalog.GetModelsByFamily(family.Id)
                .OrderBy(item => item.DisplayName)
                .Select(item => new ComboEntry(item.DisplayName, item)));
        SetEntries(_moduleCombo, family is null
            ? Enumerable.Empty<ComboEntry>()
            : _catalog.GetIoModulesByFamily(family.Id)
                .OrderBy(item => item.DisplayName)
                .Select(item => new ComboEntry(item.DisplayName, item)));
        PopulateTests(Selected<IoModuleDefinition>(_moduleCombo));
    }

    private void PopulateTests(IoModuleDefinition? module)
    {
        SetEntries(_testCombo, module is null
            ? Enumerable.Empty<ComboEntry>()
            : _catalog.GetTestProfilesByModule(module.Id)
                .OrderBy(item => item.DisplayName)
                .Select(item => new ComboEntry(item.DisplayName, item)));
    }

    private void ApplySelection()
    {
        HardwareFamily? family = Selected<HardwareFamily>(_familyCombo);
        Layout3ProfileSelection selection = Layout3ProfileSelection.Create(
            family,
            Selected<HardwareModel>(_modelCombo),
            Selected<IoModuleDefinition>(_moduleCombo),
            Layout3ProfileSelection.ResolveCommunication(_catalog, family),
            Selected<TestProfile>(_testCombo));

        CurrentSelection = selection;
        UpdateDisplay(selection);
        SelectionLogged?.Invoke(BuildLogLine(selection));
    }

    private void UpdateDisplay(Layout3ProfileSelection selection)
    {
        _communicationValue.Text = selection.CommunicationName;
        _validationLabel.Text = selection.ValidationLabel;
        _validationLabel.ForeColor = selection.ValidationPending ? _palette.Warning : _palette.AccentBlue;
        _validationLabel.BackColor = selection.ValidationPending ? _palette.WarningBackground : _palette.Raised;

        _summaryTextBox.Text =
            $"FAMILIA  {selection.FamilyName}\r\n" +
            $"MODELO   {selection.ModelName}\r\n" +
            $"MODULO   {selection.ModuleName}\r\n" +
            $"COM.     {selection.CommunicationName}\r\n" +
            $"TESTE    {selection.TestName}\r\n" +
            $"STATUS   {selection.ValidationLabel}\r\n" +
            $"MODO     {_modeSummary}";
    }

    private static string BuildLogLine(Layout3ProfileSelection selection)
    {
        string pending = selection.ValidationPending ? " | PENDENTE" : string.Empty;
        return $"Perfil local selecionado: {selection.FamilyName} / {selection.ModelName} / " +
            $"{selection.ModuleName} / {selection.TestName}.{pending} Read-only, 0 comandos fisicos.";
    }

    private void SetEntries(ComboBox combo, IEnumerable<ComboEntry> entries)
    {
        combo.BeginUpdate();
        combo.Items.Clear();
        foreach (ComboEntry entry in entries)
        {
            combo.Items.Add(entry);
        }

        combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
        combo.Enabled = combo.Items.Count > 0;
        if (combo.Items.Count == 0)
        {
            combo.Text = "Nao disponivel";
        }

        combo.EndUpdate();
    }

    private static T? Selected<T>(ComboBox combo) where T : class
    {
        return combo.SelectedItem is ComboEntry entry ? entry.Value as T : null;
    }

    private ComboBox CreateCombo()
    {
        return new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = _palette.Field,
            ForeColor = _palette.Text,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Margin = new Padding(0, 3, 0, 3)
        };
    }

    private Label CreateLabel(
        string text,
        float size,
        FontStyle style,
        Color color,
        ContentAlignment alignment = ContentAlignment.MiddleLeft)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            BackColor = _palette.Surface,
            TextAlign = alignment,
            AutoEllipsis = true
        };
    }

    private sealed record ComboEntry(string Text, object? Value)
    {
        public override string ToString()
        {
            return Text;
        }
    }
}
