using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3ProfilePanelControl : UserControl
{
    private static readonly Color SurfaceColor = Color.FromArgb(24, 32, 42);
    private static readonly Color FieldColor = Color.FromArgb(18, 25, 34);
    private static readonly Color RaisedColor = Color.FromArgb(42, 52, 64);
    private static readonly Color DividerColor = Color.FromArgb(72, 96, 116);
    private static readonly Color TextColor = Color.FromArgb(226, 232, 240);
    private static readonly Color MutedTextColor = Color.FromArgb(170, 184, 198);
    private static readonly Color AccentBlueColor = Color.FromArgb(83, 151, 210);

    private readonly HardwareCatalog _catalog;
    private readonly TableLayoutPanel _profileFields;
    private readonly Label _validationLabel;
    private readonly TextBox _summaryTextBox;
    private readonly Button _copyButton;

    public Layout3ProfilePanelControl(HardwareCatalog hardwareCatalog)
    {
        _catalog = hardwareCatalog;
        BackColor = SurfaceColor;
        BorderStyle = BorderStyle.FixedSingle;

        _profileFields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = SurfaceColor,
            Margin = new Padding(0, 2, 0, 4)
        };
        for (int row = 0; row < 5; row++)
        {
            _profileFields.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        }

        _validationLabel = CreateLabel(string.Empty, 8F, FontStyle.Bold, AccentBlueColor);
        _summaryTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            BackColor = FieldColor,
            ForeColor = Color.FromArgb(190, 202, 213),
            Font = new Font("Consolas", 8F),
            Margin = new Padding(0, 4, 0, 4)
        };
        _copyButton = CreateLocalButton("COPIAR RESUMO");
        _copyButton.Click += (_, _) => CopySummary();

        BuildLayout();
        LoadProfileReference();
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = SurfaceColor,
            Padding = new Padding(14)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 154F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(_profileFields, 0, 1);
        root.Controls.Add(CreateValidationArea(), 0, 2);
        root.Controls.Add(_summaryTextBox, 0, 3);
        root.Controls.Add(_copyButton, 0, 4);
        Controls.Add(root);
    }

    private static Control CreateHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = SurfaceColor
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

        Label index = CreateLabel("03", 8F, FontStyle.Bold, AccentBlueColor, ContentAlignment.MiddleCenter);
        index.BackColor = FieldColor;
        index.Margin = new Padding(0, 4, 7, 4);
        header.Controls.Add(index, 0, 0);
        header.SetRowSpan(index, 2);
        header.Controls.Add(CreateLabel("PERFIL / PREPARACAO", 10F, FontStyle.Bold, TextColor), 1, 0);
        header.Controls.Add(CreateLabel("Referencia local somente leitura", 8F, FontStyle.Regular, MutedTextColor), 1, 1);
        return header;
    }

    private Control CreateValidationArea()
    {
        TableLayoutPanel validation = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = RaisedColor,
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(10, 3, 10, 3)
        };
        validation.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        validation.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        Label title = CreateLabel("STATUS DE VALIDACAO", 7F, FontStyle.Bold, MutedTextColor);
        title.BackColor = RaisedColor;
        _validationLabel.BackColor = RaisedColor;
        validation.Controls.Add(title, 0, 0);
        validation.Controls.Add(_validationLabel, 0, 1);
        return validation;
    }

    private void LoadProfileReference()
    {
        HardwareFamily? family = _catalog.Families
            .OrderBy(item => item.DisplayName)
            .FirstOrDefault();
        HardwareModel? model = family is null
            ? null
            : _catalog.GetModelsByFamily(family.Id).OrderBy(item => item.DisplayName).FirstOrDefault();
        IoModuleDefinition? module = family is null
            ? null
            : _catalog.GetIoModulesByFamily(family.Id).OrderBy(item => item.DisplayName).FirstOrDefault();
        CommunicationProfile? communication = family is null
            ? null
            : _catalog.GetCommunicationProfilesByFamily(family.Id).OrderBy(item => item.DisplayName).FirstOrDefault();
        TestProfile? test = module is null
            ? null
            : _catalog.GetTestProfilesByModule(module.Id).OrderBy(item => item.DisplayName).FirstOrDefault();

        _profileFields.Controls.Add(CreateProfileField("FAMILIA", Display(family?.DisplayName)), 0, 0);
        _profileFields.Controls.Add(CreateProfileField("MODELO", Display(model?.DisplayName)), 0, 1);
        _profileFields.Controls.Add(CreateProfileField("MODULO I/O", Display(module?.DisplayName)), 0, 2);
        _profileFields.Controls.Add(CreateProfileField("COMUNIC.", Display(communication?.DisplayName)), 0, 3);
        _profileFields.Controls.Add(CreateProfileField("TESTE", Display(test?.DisplayName)), 0, 4);

        string validationStatus = FirstNonEmpty(
            model?.ValidationStatus,
            module?.ValidationStatus,
            family?.ValidationStatus,
            "catalogo_indisponivel");
        bool pending = validationStatus.Contains("pending", StringComparison.OrdinalIgnoreCase)
            || validationStatus.Contains("manual", StringComparison.OrdinalIgnoreCase);
        _validationLabel.Text = pending
            ? "VALIDACAO MANUAL PENDENTE"
            : validationStatus.Replace('_', ' ').ToUpperInvariant();

        _summaryTextBox.Text =
            $"FAMILIA  {Display(family?.DisplayName)}\r\n" +
            $"MODELO   {Display(model?.DisplayName)}\r\n" +
            $"MODULO   {Display(module?.DisplayName)}\r\n" +
            $"COM.     {Display(communication?.DisplayName)}\r\n" +
            $"TESTE    {Display(test?.DisplayName)}\r\n" +
            "MODO     PREVIEW / 0 COMANDOS";
    }

    private static Control CreateProfileField(string title, string value)
    {
        TableLayoutPanel field = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = FieldColor,
            Margin = new Padding(0, 2, 0, 2),
            Padding = new Padding(10, 0, 10, 0)
        };
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        Label titleLabel = CreateLabel(title, 7.5F, FontStyle.Bold, MutedTextColor);
        Label valueLabel = CreateLabel(value, 8.5F, FontStyle.Bold, TextColor);
        titleLabel.BackColor = FieldColor;
        valueLabel.BackColor = FieldColor;
        field.Controls.Add(titleLabel, 0, 0);
        field.Controls.Add(valueLabel, 1, 0);
        return field;
    }

    private void CopySummary()
    {
        if (string.IsNullOrWhiteSpace(_summaryTextBox.Text))
        {
            return;
        }

        try
        {
            Clipboard.SetText(_summaryTextBox.Text);
            _copyButton.Text = "RESUMO COPIADO";
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

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.First(value => !string.IsNullOrWhiteSpace(value))!;
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Nao disponivel" : value;
    }

    private static Label CreateLabel(
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
            BackColor = SurfaceColor,
            TextAlign = alignment,
            AutoEllipsis = true
        };
    }

    private static Button CreateLocalButton(string text)
    {
        Button button = new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = TextColor,
            BackColor = RaisedColor,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 0),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = DividerColor;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 64, 78);
        return button;
    }
}
