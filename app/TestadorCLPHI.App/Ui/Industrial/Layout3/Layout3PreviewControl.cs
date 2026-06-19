using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

public sealed class Layout3PreviewControl : UserControl
{
    private const int MinimumContentWidth = 1000;
    private const int MinimumContentHeight = 560;

    private readonly Layout3ThemePalette _palette;
    private readonly TextBox _previewLogTextBox;
    private readonly Panel _content;

    public Layout3PreviewControl(
        HardwareCatalog? hardwareCatalog,
        Layout3PreviewTheme theme = Layout3PreviewTheme.Dark)
    {
        _palette = Layout3ThemePalette.For(theme);
        Dock = DockStyle.Fill;
        BackColor = _palette.Background;
        DoubleBuffered = true;
        ResizeRedraw = true;
        AutoScroll = true;
        AutoScrollMinSize = new Size(MinimumContentWidth, MinimumContentHeight);

        _previewLogTextBox = CreatePreviewLogTextBox();
        _content = new Panel
        {
            BackColor = _palette.Background,
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
            BackColor = _palette.Background
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));

        root.Controls.Add(CreateTopBar(), 0, 0);
        root.Controls.Add(CreateMainArea(hardwareCatalog), 0, 1);
        root.Controls.Add(CreateTerminalArea(), 0, 2);

        _content.Controls.Add(root);
    }

    private Control CreateTopBar()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(16, 8, 10, 8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = _palette.Surface
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
            BackColor = _palette.Surface,
            Margin = Padding.Empty
        };
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        identity.Controls.Add(new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _palette.AccentBlue,
            Margin = new Padding(0, 5, 0, 5)
        }, 0, 0);

        Label identityText = CreateLabel(
            "TESTADOR CLP HI\r\nLAYOUT 3  /  CONSOLE INDUSTRIAL DE BANCADA",
            11F,
            FontStyle.Bold,
            _palette.Text);
        identityText.Padding = new Padding(12, 0, 0, 0);
        identity.Controls.Add(identityText, 1, 0);

        layout.Controls.Add(identity, 0, 0);
        layout.Controls.Add(CreateGeneralStatus(), 1, 0);
        layout.Controls.Add(CreatePreviewBadge(), 2, 0);
        layout.Controls.Add(CreateEmergencyPreviewArea(), 3, 0);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateGeneralStatus()
    {
        Label status = CreateLabel(
            "STATUS GERAL\r\n●  DESCONECTADO",
            8.5F,
            FontStyle.Bold,
            _palette.Text,
            ContentAlignment.MiddleLeft);
        status.BackColor = _palette.Field;
        status.Margin = new Padding(8, 3, 8, 3);
        status.Padding = new Padding(14, 0, 8, 0);
        return status;
    }

    private Control CreatePreviewBadge()
    {
        Label badge = CreateLabel(
            "PREVIEW ONLY\r\n0 COMANDOS FISICOS",
            8.5F,
            FontStyle.Bold,
            _palette.Warning,
            ContentAlignment.MiddleCenter);
        badge.BackColor = _palette.WarningBackground;
        badge.Margin = new Padding(8, 3, 8, 3);
        return badge;
    }

    private Control CreateEmergencyPreviewArea()
    {
        TableLayoutPanel emergency = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.EmergencyBackground,
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
            _palette.Emergency,
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

    private Control CreateMainArea(HardwareCatalog hardwareCatalog)
    {
        TableLayoutPanel main = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 8),
            BackColor = _palette.Background
        };

        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        Control connection = CreateConnectionArea();
        connection.Margin = new Padding(0, 0, 6, 0);
        main.Controls.Add(connection, 0, 0);

        Layout3IoPanelControl ioPanel = new(_palette)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 6, 0)
        };
        main.Controls.Add(ioPanel, 1, 0);

        Layout3ProfilePanelControl profilePanel = new(hardwareCatalog, _palette)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 0, 0)
        };
        main.Controls.Add(profilePanel, 2, 0);

        return main;
    }

    private Control CreateConnectionArea()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9,
            BackColor = _palette.Surface
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
        layout.Controls.Add(CreateDiagnosticItem("CONFIGURACAO", "Referencia local", _palette.AccentBlue), 0, 6);
        layout.Controls.Add(CreateDiagnosticNote(), 0, 7);

        Label unavailable = CreateLabel(
            "CONEXAO INDISPONIVEL NESTA PREVIEW",
            8F,
            FontStyle.Bold,
            _palette.MutedText,
            ContentAlignment.MiddleCenter);
        unavailable.BackColor = _palette.Field;
        unavailable.Margin = new Padding(0, 5, 0, 0);
        layout.Controls.Add(unavailable, 0, 8);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateCommunicationState()
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Raised,
            Margin = new Padding(0, 2, 0, 6),
            Padding = new Padding(10, 0, 10, 0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        Label label = CreateLabel("COMUNICACAO", 8F, FontStyle.Bold, _palette.MutedText);
        Label value = CreateLabel("DESCONECTADA", 9F, FontStyle.Bold, _palette.Text, ContentAlignment.MiddleRight);
        label.BackColor = row.BackColor;
        value.BackColor = row.BackColor;
        row.Controls.Add(label, 0, 0);
        row.Controls.Add(value, 1, 0);
        return row;
    }

    private Control CreateReferenceRow(string title, string value)
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(2, 0, 2, 0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        row.Controls.Add(CreateFieldLabel(title), 0, 0);
        row.Controls.Add(CreateLabel(value, 8.5F, FontStyle.Regular, _palette.Text, ContentAlignment.MiddleRight), 1, 0);
        return row;
    }

    private Control CreateDiagnosticItem(string title, string value, Color valueColor)
    {
        TableLayoutPanel item = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Field,
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(10, 3, 10, 3)
        };
        item.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        item.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        item.Controls.Add(CreateFieldLabel(title), 0, 0);
        Label valueLabel = CreateLabel(value, 9F, FontStyle.Bold, valueColor);
        valueLabel.BackColor = _palette.Field;
        item.Controls.Add(valueLabel, 0, 1);
        return item;
    }

    private Control CreateDiagnosticNote()
    {
        TableLayoutPanel note = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = _palette.Surface,
            Padding = new Padding(2, 8, 2, 0)
        };
        note.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        note.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        note.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        note.Controls.Add(CreateLabel("CATALOGO", 8F, FontStyle.Bold, _palette.MutedText), 0, 0);
        note.Controls.Add(CreateLabel("Disponivel somente em memoria local", 9F, FontStyle.Regular, _palette.Text), 0, 1);
        note.Controls.Add(CreateLabel(
            "Nenhuma deteccao automatica. Nenhum canal de comunicacao e aberto.",
            8.5F,
            FontStyle.Regular,
            _palette.MutedText), 0, 2);
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
            BackColor = _palette.Surface
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = _palette.Surface
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));

        header.Controls.Add(CreateLabel("TERMINAL / LOG", 9F, FontStyle.Bold, _palette.Text), 0, 0);
        header.Controls.Add(CreateLabel(
            "PREVIEW ONLY  |  sem leitura, escrita ou comandos fisicos",
            8.5F,
            FontStyle.Bold,
            _palette.Warning,
            ContentAlignment.MiddleCenter), 1, 0);

        Button clearButton = CreateLocalButton("LIMPAR LOG");
        clearButton.Click += (_, _) => _previewLogTextBox.Clear();
        header.Controls.Add(clearButton, 2, 0);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_previewLogTextBox, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateSectionHeader(string number, string title, string detail)
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = _palette.Surface,
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));

        Label index = CreateLabel(number, 8F, FontStyle.Bold, _palette.AccentBlue, ContentAlignment.MiddleCenter);
        index.BackColor = _palette.Field;
        index.Margin = new Padding(0, 4, 7, 4);
        header.Controls.Add(index, 0, 0);
        header.SetRowSpan(index, 2);
        header.Controls.Add(CreateLabel(title, 10F, FontStyle.Bold, _palette.Text), 1, 0);
        header.Controls.Add(CreateLabel(detail, 8F, FontStyle.Regular, _palette.MutedText), 1, 1);
        return header;
    }

    private TextBox CreatePreviewLogTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = _palette.TerminalBackground,
            ForeColor = _palette.TerminalText,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 8.5F),
            Text =
                "[PREVIEW] Layout 3 iniciado em modo isolado.\r\n" +
                "[SEGURANCA] Sem leitura, escrita ou comunicacao fisica.  Comandos executados: 0."
        };
    }

    private Panel CreateSurfacePanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _palette.Surface,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private Label CreateFieldLabel(string text)
    {
        Label label = CreateLabel(text, 8F, FontStyle.Bold, _palette.MutedText);
        label.BackColor = label.Parent?.BackColor ?? _palette.Surface;
        return label;
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
            ForeColor = _palette.MutedText,
            BackColor = _palette.Raised,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            Margin = new Padding(4, 1, 0, 1),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = _palette.Divider;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = _palette.ButtonHover;
        return button;
    }

    private Control CreateDivider()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _palette.Divider,
            Margin = new Padding(0, 8, 0, 9)
        };
    }

    private void ResizeContentToViewport()
    {
        Size desiredSize = new(
            Math.Max(ClientSize.Width, MinimumContentWidth),
            Math.Max(ClientSize.Height, MinimumContentHeight));

        if (_content.Size == desiredSize && _content.Location == Point.Empty)
        {
            return;
        }

        _content.SuspendLayout();
        try
        {
            _content.Bounds = new Rectangle(Point.Empty, desiredSize);
        }
        finally
        {
            _content.ResumeLayout(performLayout: true);
        }
    }
}
