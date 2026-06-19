using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3ProfilePanelControl : UserControl
{
    private readonly Layout3ThemePalette _palette;
    private readonly HardwareCatalog _catalog;
    private readonly TableLayoutPanel _profileFields;
    private readonly Label _validationLabel;
    private readonly TextBox _summaryTextBox;
    private readonly Button _copyButton;

    public Layout3ProfilePanelControl(
        HardwareCatalog hardwareCatalog,
        Layout3ThemePalette palette)
    {
        _palette = palette;
        _catalog = hardwareCatalog;
        BackColor = _palette.Surface;
        BorderStyle = BorderStyle.FixedSingle;

        _profileFields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = _palette.Surface,
            Margin = new Padding(0, 2, 0, 4)
        };
        for (int row = 0; row < 5; row++)
        {
            _profileFields.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        }

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
            BackColor = _palette.Surface,
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
        header.Controls.Add(CreateLabel("PERFIL / PREPARACAO", 10F, FontStyle.Bold, _palette.Text), 1, 0);
        header.Controls.Add(CreateLabel("Referencia local somente leitura", 8F, FontStyle.Regular, _palette.MutedText), 1, 1);
        return header;
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

    private Control CreateProfileField(string title, string value)
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
        Label titleLabel = CreateLabel(title, 7.5F, FontStyle.Bold, _palette.MutedText);
        Label valueLabel = CreateLabel(value, 8.5F, FontStyle.Bold, _palette.Text);
        titleLabel.BackColor = _palette.Field;
        valueLabel.BackColor = _palette.Field;
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

    private Button CreateLocalButton(string text)
    {
        Button button = new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = _palette.Text,
            BackColor = _palette.Raised,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 0),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = _palette.Divider;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = _palette.ProfileButtonHover;
        return button;
    }
}
