using TestadorCLPHI.App.Hardware;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

public sealed class Layout3PreviewControl : UserControl
{
    private const int MinimumContentWidth = 1240;
    private const int MinimumContentHeight = 660;

    private static readonly Color BackgroundColor = Color.FromArgb(10, 16, 22);
    private static readonly Color PanelColor = Color.FromArgb(22, 31, 40);
    private static readonly Color FieldColor = Color.FromArgb(14, 22, 30);
    private static readonly Color ButtonColor = Color.FromArgb(30, 43, 56);
    private static readonly Color BorderColor = Color.FromArgb(58, 72, 86);
    private static readonly Color TextColor = Color.FromArgb(226, 232, 240);
    private static readonly Color MutedTextColor = Color.FromArgb(165, 185, 205);
    private static readonly Color AccentBlueColor = Color.FromArgb(74, 144, 226);
    private static readonly Color AccentGreenColor = Color.FromArgb(62, 210, 92);
    private static readonly Color AccentYellowColor = Color.FromArgb(245, 190, 45);
    private static readonly Color AccentRedColor = Color.FromArgb(245, 68, 68);

    private readonly HardwareCatalog _hardwareCatalog;
    private readonly TextBox _previewLogTextBox;
    private readonly Panel _content;

    public Layout3PreviewControl(HardwareCatalog? hardwareCatalog)
    {
        _hardwareCatalog = hardwareCatalog ?? HardwareCatalog.Empty;

        Dock = DockStyle.Fill;
        BackColor = BackgroundColor;
        AutoScroll = true;
        AutoScrollMinSize = new Size(MinimumContentWidth, MinimumContentHeight);

        _previewLogTextBox = CreatePreviewLogTextBox();
        _content = new Panel
        {
            BackColor = BackgroundColor,
            Location = Point.Empty,
            Margin = Padding.Empty,
            Size = new Size(MinimumContentWidth, MinimumContentHeight)
        };

        BuildLayout();
        Controls.Add(_content);
        ResizeContentToViewport();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ResizeContentToViewport();
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(8),
            BackColor = BackgroundColor
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 420F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(CreateTopBar(), 0, 0);
        root.Controls.Add(CreateMainArea(), 0, 1);
        root.Controls.Add(CreateTerminalArea(), 0, 2);

        _content.Controls.Add(root);
    }

    private Control CreateTopBar()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(14, 8, 10, 8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = PanelColor
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));

        TableLayoutPanel identity = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = PanelColor
        };
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        identity.Controls.Add(CreateLabel("TESTADOR CLP HI | LAYOUT 3", 15F, FontStyle.Bold, TextColor), 0, 0);
        identity.Controls.Add(CreateLabel("Conceito hibrido: diagnostico + operacao industrial", 8.5F, FontStyle.Regular, MutedTextColor), 0, 1);

        layout.Controls.Add(identity, 0, 0);
        layout.Controls.Add(CreateStatusCard("STATUS GERAL", "DESCONECTADO", AccentRedColor), 1, 0);
        layout.Controls.Add(CreatePreviewBadge(), 2, 0);
        layout.Controls.Add(CreateEmergencyPreviewArea(), 3, 0);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateMainArea()
    {
        TableLayoutPanel main = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 0),
            BackColor = BackgroundColor
        };

        main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390F));

        main.Controls.Add(CreateConnectionArea(), 0, 0);
        main.Controls.Add(CreateOperationArea(), 1, 0);
        main.Controls.Add(CreateHardwareArea(), 2, 0);

        return main;
    }

    private Control CreateConnectionArea()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 0, 6, 0);
        panel.Padding = new Padding(12);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            BackColor = PanelColor
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

        layout.Controls.Add(CreateSectionTitle("CONEXAO E DIAGNOSTICO"), 0, 0);
        layout.Controls.Add(CreateStatusCard("COMUNICACAO", "Nao conectada", AccentRedColor), 0, 1);
        layout.Controls.Add(CreateReadOnlyField("Porta COM", "Nao selecionada"), 0, 2);
        layout.Controls.Add(CreateReadOnlyField("Baud rate", "9600 (referencia)"), 0, 3);
        layout.Controls.Add(CreateReadOnlyField("Slave ID", "1 (referencia)"), 0, 4);
        layout.Controls.Add(CreateDivider(), 0, 5);
        layout.Controls.Add(CreateDiagnosticSummary(), 0, 6);

        Button unavailableButton = CreateActionButton(
            "Conectar indisponivel na preview",
            AccentBlueColor,
            enabled: false);
        layout.Controls.Add(unavailableButton, 0, 7);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateOperationArea()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 6, 0),
            BackColor = BackgroundColor
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = BackgroundColor
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.Controls.Add(CreatePreviewCommandBar(), 0, 0);

        IndustrialManualIoPanelControl ioPanel = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 0)
        };
        ioPanel.SetPanelEnabled(false);
        layout.Controls.Add(ioPanel, 0, 1);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreatePreviewCommandBar()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = PanelColor
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));

        Button enablePreviewButton = CreateActionButton("PREVIEW | Habilitar teste", AccentGreenColor);
        Button resetPreviewButton = CreateActionButton("PREVIEW | Resetar saidas", AccentYellowColor);
        enablePreviewButton.Click += (_, _) => AppendPreviewLog("Intencao visual: habilitar teste. Nenhum comando foi enviado.");
        resetPreviewButton.Click += (_, _) => AppendPreviewLog("Intencao visual: resetar saidas. Nenhum comando foi enviado.");

        layout.Controls.Add(enablePreviewButton, 0, 0);
        layout.Controls.Add(resetPreviewButton, 1, 0);
        layout.Controls.Add(CreateLabel("I/O sem escrita", 9F, FontStyle.Bold, AccentYellowColor, ContentAlignment.MiddleCenter), 2, 0);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateHardwareArea()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(6);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = PanelColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Label pendingLabel = CreateLabel(
            "PERFIL E PREPARACAO | RION 5 / NEON 5: pending_manual_validation",
            8.5F,
            FontStyle.Bold,
            AccentYellowColor);
        pendingLabel.Padding = new Padding(4, 0, 4, 0);
        pendingLabel.AutoEllipsis = true;

        HardwareProfileSelectionControl profileSelection = new(_hardwareCatalog)
        {
            Dock = DockStyle.Fill,
            MinimumSize = new Size(360, 350),
            Margin = Padding.Empty
        };
        AppThemeService.ApplyControlTree(
            profileSelection,
            PanelColor,
            TextColor,
            FieldColor,
            ButtonColor,
            TextColor);

        layout.Controls.Add(pendingLabel, 0, 0);
        layout.Controls.Add(profileSelection, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateTerminalArea()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 8, 0, 0);
        panel.Padding = new Padding(10, 6, 10, 8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = PanelColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = PanelColor
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));

        header.Controls.Add(CreateSectionTitle("TERMINAL / LOG"), 0, 0);
        header.Controls.Add(CreateLabel(
            "PREVIEW ISOLADA - sem leitura, escrita ou comandos fisicos",
            9F,
            FontStyle.Bold,
            AccentYellowColor,
            ContentAlignment.MiddleCenter), 1, 0);

        Button clearButton = CreateActionButton("Limpar log", MutedTextColor);
        clearButton.Click += (_, _) => _previewLogTextBox.Clear();
        header.Controls.Add(clearButton, 2, 0);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_previewLogTextBox, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private static Control CreatePreviewBadge()
    {
        Label label = CreateLabel(
            "PREVIEW ONLY\r\nSEM COMANDOS FISICOS",
            9.5F,
            FontStyle.Bold,
            Color.FromArgb(255, 228, 135),
            ContentAlignment.MiddleCenter);
        label.BackColor = Color.FromArgb(75, 58, 18);
        label.BorderStyle = BorderStyle.FixedSingle;
        label.Margin = new Padding(6, 2, 6, 2);
        return label;
    }

    private static Control CreateEmergencyPreviewArea()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = PanelColor,
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));

        layout.Controls.Add(CreateLabel(
            "PARADA\nVISUAL",
            8.5F,
            FontStyle.Bold,
            AccentRedColor,
            ContentAlignment.MiddleRight), 0, 0);

        EmergencyStopButtonControl emergency = new()
        {
            Dock = DockStyle.Fill,
            Enabled = false,
            Title = "SEM ACAO",
            ButtonImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", "stop_emergency.png")
        };
        layout.Controls.Add(emergency, 1, 0);
        return layout;
    }

    private static Control CreateDiagnosticSummary()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(0, 10, 0, 4),
            BackColor = PanelColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        layout.Controls.Add(CreateSectionTitle("RESUMO DE DIAGNOSTICO"), 0, 0);
        layout.Controls.Add(CreateStatusCard("Configuracao", "Somente referencia visual", AccentBlueColor), 0, 1);
        layout.Controls.Add(CreateStatusCard("Catalogo", "Informativo e local", AccentGreenColor), 0, 2);
        layout.Controls.Add(CreateLabel(
            "Deteccao, conexao e leitura nao sao executadas nesta etapa.",
            8.5F,
            FontStyle.Regular,
            MutedTextColor), 0, 3);
        return layout;
    }

    private static Control CreateReadOnlyField(string labelText, string valueText)
    {
        TableLayoutPanel field = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 2, 0, 2),
            Padding = new Padding(8, 2, 8, 2),
            BackColor = FieldColor,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        field.Controls.Add(CreateLabel(labelText.ToUpperInvariant(), 7.5F, FontStyle.Bold, MutedTextColor), 0, 0);
        field.Controls.Add(CreateLabel(valueText, 9F, FontStyle.Bold, TextColor), 0, 1);
        return field;
    }

    private static Control CreateStatusCard(string title, string status, Color statusColor)
    {
        TableLayoutPanel card = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(3),
            Padding = new Padding(7, 0, 7, 0),
            BackColor = FieldColor,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        card.Controls.Add(CreateLabel(title, 7.5F, FontStyle.Bold, MutedTextColor), 0, 0);
        card.Controls.Add(CreateLabel(status, 8.5F, FontStyle.Bold, statusColor, ContentAlignment.MiddleRight), 1, 0);
        return card;
    }

    private static TextBox CreatePreviewLogTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(3, 6, 9),
            ForeColor = TextColor,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 8.5F),
            Text =
                "[PREVIEW] Layout 3 iniciado em modo isolado.\r\n" +
                "[SEGURANCA] Nenhum servico PLC/Modbus foi criado ou conectado.\r\n" +
                "[INFO] Botoes PREVIEW registram apenas mensagens neste log."
        };
    }

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PanelColor,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static Label CreateSectionTitle(string text)
    {
        return CreateLabel(text, 9F, FontStyle.Bold, TextColor);
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
            BackColor = PanelColor,
            TextAlign = alignment
        };
    }

    private static Button CreateActionButton(string text, Color borderColor, bool enabled = true)
    {
        Button button = new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            Enabled = enabled,
            FlatStyle = FlatStyle.Flat,
            ForeColor = TextColor,
            BackColor = ButtonColor,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Margin = new Padding(4)
        };
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 53, 68);
        return button;
    }

    private static Control CreateDivider()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = BorderColor,
            Margin = Padding.Empty
        };
    }

    private void AppendPreviewLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _previewLogTextBox.AppendText($"{Environment.NewLine}[{timestamp}] [PREVIEW] {message}");
    }

    private void ResizeContentToViewport()
    {
        Size desiredSize = new(
            Math.Max(ClientSize.Width, MinimumContentWidth),
            Math.Max(ClientSize.Height, MinimumContentHeight));

        if (_content.Size != desiredSize)
        {
            _content.Size = desiredSize;
        }

        AutoScrollMinSize = desiredSize;
    }
}
