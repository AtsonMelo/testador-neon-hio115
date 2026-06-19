using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

public sealed class Layout3PreviewControl : UserControl
{
    private const int MinimumContentWidth = 1180;
    private const int MinimumContentHeight = 640;

    private static readonly Color BackgroundColor = Color.FromArgb(18, 24, 32);
    private static readonly Color SurfaceColor = Color.FromArgb(24, 32, 42);
    private static readonly Color SurfaceRaisedColor = Color.FromArgb(42, 52, 64);
    private static readonly Color FieldColor = Color.FromArgb(18, 25, 34);
    private static readonly Color DividerColor = Color.FromArgb(72, 96, 116);
    private static readonly Color TextColor = Color.FromArgb(226, 232, 240);
    private static readonly Color MutedTextColor = Color.FromArgb(170, 184, 198);
    private static readonly Color AccentBlueColor = Color.FromArgb(83, 151, 210);
    private static readonly Color WarningColor = Color.FromArgb(222, 178, 72);
    private static readonly Color EmergencyColor = Color.FromArgb(224, 92, 92);

    private readonly TextBox _previewLogTextBox;
    private readonly Panel _content;

    public Layout3PreviewControl(HardwareCatalog? hardwareCatalog)
    {
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

        BuildLayout(hardwareCatalog ?? HardwareCatalog.Empty);
        Controls.Add(_content);
        ResizeContentToViewport();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ResizeContentToViewport();
    }

    private void BuildLayout(HardwareCatalog hardwareCatalog)
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10),
            BackColor = BackgroundColor
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));

        root.Controls.Add(CreateTopBar(), 0, 0);
        root.Controls.Add(CreateMainArea(hardwareCatalog), 0, 1);
        root.Controls.Add(CreateTerminalArea(), 0, 2);

        _content.Controls.Add(root);
    }

    private static Control CreateTopBar()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(16, 8, 10, 8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = SurfaceColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));

        TableLayoutPanel identity = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = SurfaceColor,
            Margin = Padding.Empty
        };
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        identity.Controls.Add(new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AccentBlueColor,
            Margin = new Padding(0, 5, 0, 5)
        }, 0, 0);

        Label identityText = CreateLabel(
            "TESTADOR CLP HI\r\nLAYOUT 3  /  CONSOLE INDUSTRIAL DE BANCADA",
            11F,
            FontStyle.Bold,
            TextColor);
        identityText.Padding = new Padding(12, 0, 0, 0);
        identity.Controls.Add(identityText, 1, 0);

        layout.Controls.Add(identity, 0, 0);
        layout.Controls.Add(CreateGeneralStatus(), 1, 0);
        layout.Controls.Add(CreatePreviewBadge(), 2, 0);
        layout.Controls.Add(CreateEmergencyPreviewArea(), 3, 0);

        panel.Controls.Add(layout);
        return panel;
    }

    private static Control CreateGeneralStatus()
    {
        Label status = CreateLabel(
            "STATUS GERAL\r\n●  DESCONECTADO",
            8.5F,
            FontStyle.Bold,
            TextColor,
            ContentAlignment.MiddleLeft);
        status.BackColor = FieldColor;
        status.Margin = new Padding(8, 3, 8, 3);
        status.Padding = new Padding(14, 0, 8, 0);
        return status;
    }

    private static Control CreatePreviewBadge()
    {
        Label badge = CreateLabel(
            "PREVIEW ONLY\r\n0 COMANDOS FISICOS",
            8.5F,
            FontStyle.Bold,
            WarningColor,
            ContentAlignment.MiddleCenter);
        badge.BackColor = Color.FromArgb(53, 45, 27);
        badge.Margin = new Padding(8, 3, 8, 3);
        return badge;
    }

    private static Control CreateEmergencyPreviewArea()
    {
        TableLayoutPanel emergency = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(43, 30, 33),
            Margin = new Padding(8, 3, 0, 3),
            Padding = new Padding(8, 3, 6, 3)
        };
        emergency.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        emergency.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        emergency.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));

        Label text = CreateLabel(
            "PARADA\r\nVISUAL",
            8.5F,
            FontStyle.Bold,
            EmergencyColor,
            ContentAlignment.MiddleRight);
        text.BackColor = emergency.BackColor;
        emergency.Controls.Add(text, 0, 0);

        PictureBox stop = new()
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = emergency.BackColor,
            Margin = new Padding(5, 0, 0, 0),
            Cursor = Cursors.Default,
            TabStop = false
        };
        string imagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", "stop_emergency.png");
        if (File.Exists(imagePath))
        {
            using Image source = Image.FromFile(imagePath);
            stop.Image = new Bitmap(source);
            stop.Disposed += (_, _) => stop.Image?.Dispose();
        }
        emergency.Controls.Add(stop, 1, 0);
        return emergency;
    }

    private static Control CreateMainArea(HardwareCatalog hardwareCatalog)
    {
        TableLayoutPanel main = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 8),
            BackColor = BackgroundColor
        };

        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        Control connection = CreateConnectionArea();
        connection.Margin = new Padding(0, 0, 6, 0);
        main.Controls.Add(connection, 0, 0);

        Layout3IoPanelControl ioPanel = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 6, 0)
        };
        main.Controls.Add(ioPanel, 1, 0);

        Layout3ProfilePanelControl profilePanel = new(hardwareCatalog)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 0, 0)
        };
        main.Controls.Add(profilePanel, 2, 0);

        return main;
    }

    private static Control CreateConnectionArea()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9,
            BackColor = SurfaceColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        layout.Controls.Add(CreateSectionHeader("01", "CONEXAO / DIAGNOSTICO", "Configuracao local inerte"), 0, 0);
        layout.Controls.Add(CreateCommunicationState(), 0, 1);
        layout.Controls.Add(CreateReferenceRow("PORTA", "Nao selecionada"), 0, 2);
        layout.Controls.Add(CreateReferenceRow("VELOCIDADE", "9600  /  referencia"), 0, 3);
        layout.Controls.Add(CreateReferenceRow("ENDERECO", "Slave 1  /  referencia"), 0, 4);
        layout.Controls.Add(CreateDivider(), 0, 5);
        layout.Controls.Add(CreateDiagnosticItem("CONFIGURACAO", "Referencia local", AccentBlueColor), 0, 6);
        layout.Controls.Add(CreateDiagnosticNote(), 0, 7);

        Label unavailable = CreateLabel(
            "CONEXAO INDISPONIVEL NESTA PREVIEW",
            8F,
            FontStyle.Bold,
            MutedTextColor,
            ContentAlignment.MiddleCenter);
        unavailable.BackColor = FieldColor;
        unavailable.Margin = new Padding(0, 5, 0, 0);
        layout.Controls.Add(unavailable, 0, 8);

        panel.Controls.Add(layout);
        return panel;
    }

    private static Control CreateCommunicationState()
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = SurfaceRaisedColor,
            Margin = new Padding(0, 2, 0, 6),
            Padding = new Padding(10, 0, 10, 0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        Label label = CreateLabel("COMUNICACAO", 8F, FontStyle.Bold, MutedTextColor);
        Label value = CreateLabel("DESCONECTADA", 9F, FontStyle.Bold, TextColor, ContentAlignment.MiddleRight);
        label.BackColor = row.BackColor;
        value.BackColor = row.BackColor;
        row.Controls.Add(label, 0, 0);
        row.Controls.Add(value, 1, 0);
        return row;
    }

    private static Control CreateReferenceRow(string title, string value)
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = SurfaceColor,
            Margin = Padding.Empty,
            Padding = new Padding(2, 0, 2, 0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        row.Controls.Add(CreateFieldLabel(title), 0, 0);
        row.Controls.Add(CreateLabel(value, 8.5F, FontStyle.Regular, TextColor, ContentAlignment.MiddleRight), 1, 0);
        return row;
    }

    private static Control CreateDiagnosticItem(string title, string value, Color valueColor)
    {
        TableLayoutPanel item = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = FieldColor,
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(10, 3, 10, 3)
        };
        item.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        item.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        item.Controls.Add(CreateFieldLabel(title), 0, 0);
        Label valueLabel = CreateLabel(value, 9F, FontStyle.Bold, valueColor);
        valueLabel.BackColor = FieldColor;
        item.Controls.Add(valueLabel, 0, 1);
        return item;
    }

    private static Control CreateDiagnosticNote()
    {
        TableLayoutPanel note = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = SurfaceColor,
            Padding = new Padding(2, 8, 2, 0)
        };
        note.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        note.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        note.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        note.Controls.Add(CreateLabel("CATALOGO", 8F, FontStyle.Bold, MutedTextColor), 0, 0);
        note.Controls.Add(CreateLabel("Disponivel somente em memoria local", 9F, FontStyle.Regular, TextColor), 0, 1);
        note.Controls.Add(CreateLabel(
            "Nenhuma deteccao automatica. Nenhum canal de comunicacao e aberto.",
            8.5F,
            FontStyle.Regular,
            MutedTextColor), 0, 2);
        return note;
    }

    private Control CreateTerminalArea()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(12, 6, 12, 8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = SurfaceColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = SurfaceColor
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));

        header.Controls.Add(CreateLabel("TERMINAL / LOG", 9F, FontStyle.Bold, TextColor), 0, 0);
        header.Controls.Add(CreateLabel(
            "PREVIEW ONLY  |  sem leitura, escrita ou comandos fisicos",
            8.5F,
            FontStyle.Bold,
            WarningColor,
            ContentAlignment.MiddleCenter), 1, 0);

        Button clearButton = CreateLocalButton("LIMPAR LOG");
        clearButton.Click += (_, _) => _previewLogTextBox.Clear();
        header.Controls.Add(clearButton, 2, 0);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_previewLogTextBox, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private static Control CreateSectionHeader(string number, string title, string detail)
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = SurfaceColor,
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));

        Label index = CreateLabel(number, 8F, FontStyle.Bold, AccentBlueColor, ContentAlignment.MiddleCenter);
        index.BackColor = FieldColor;
        index.Margin = new Padding(0, 4, 7, 4);
        header.Controls.Add(index, 0, 0);
        header.SetRowSpan(index, 2);
        header.Controls.Add(CreateLabel(title, 10F, FontStyle.Bold, TextColor), 1, 0);
        header.Controls.Add(CreateLabel(detail, 8F, FontStyle.Regular, MutedTextColor), 1, 1);
        return header;
    }

    private static TextBox CreatePreviewLogTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(5, 9, 13),
            ForeColor = Color.FromArgb(178, 191, 203),
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 8.5F),
            Text =
                "[PREVIEW] Layout 3 iniciado em modo isolado.\r\n" +
                "[SEGURANCA] Sem leitura, escrita ou comunicacao fisica.  Comandos executados: 0."
        };
    }

    private static Panel CreateSurfacePanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = SurfaceColor,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static Label CreateFieldLabel(string text)
    {
        Label label = CreateLabel(text, 8F, FontStyle.Bold, MutedTextColor);
        label.BackColor = label.Parent?.BackColor ?? SurfaceColor;
        return label;
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
            ForeColor = MutedTextColor,
            BackColor = SurfaceRaisedColor,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            Margin = new Padding(4, 1, 0, 1),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = DividerColor;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 45, 58);
        return button;
    }

    private static Control CreateDivider()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = DividerColor,
            Margin = new Padding(0, 8, 0, 9)
        };
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
