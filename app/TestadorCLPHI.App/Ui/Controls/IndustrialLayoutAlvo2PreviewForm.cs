namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialLayoutAlvo2PreviewForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(12, 18, 24);
    private static readonly Color PanelColor = Color.FromArgb(24, 32, 42);
    private static readonly Color BorderColor = Color.FromArgb(64, 78, 92);
    private static readonly Color TextMutedColor = Color.FromArgb(170, 190, 210);
    private static readonly Color AccentBlueColor = Color.FromArgb(74, 144, 226);
    private static readonly Color AccentGreenColor = Color.FromArgb(62, 210, 92);
    private static readonly Color AccentYellowColor = Color.FromArgb(245, 190, 45);
    private static readonly Color AccentRedColor = Color.FromArgb(245, 68, 68);

    public IndustrialLayoutAlvo2PreviewForm()
    {
        Text = "Preview - Layout Alvo 2 | Testador CLP HI";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 760);
        ClientSize = new Size(1366, 820);
        BackColor = BackgroundColor;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.None;

        BuildLayout();
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10),
            BackColor = BackgroundColor
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));

        root.Controls.Add(CreateTopCommandBar(), 0, 0);
        root.Controls.Add(CreateMainArea(), 0, 1);
        root.Controls.Add(CreateTerminalPanel(), 0, 2);

        Controls.Add(root);
    }

    private Control CreateTopCommandBar()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(10);

        FlowLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = PanelColor
        };

        row.Controls.Add(CreateTopButton("▷", "Habilitar teste", AccentGreenColor));
        row.Controls.Add(CreateTopButton("↻", "Resetar saídas", AccentYellowColor));
        row.Controls.Add(CreateTopButton("⚙", "Teste automático", AccentBlueColor));

        panel.Controls.Add(row);

        return panel;
    }

    private Control CreateMainArea()
    {
        TableLayoutPanel main = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 0),
            BackColor = BackgroundColor
        };

        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        main.Controls.Add(CreateCommunicationPanel(), 0, 0);
        main.Controls.Add(CreateIoPanel(), 1, 0);

        return main;
    }

    private Control CreateCommunicationPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 0, 5, 0);
        panel.Padding = new Padding(18);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            BackColor = PanelColor
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));

        layout.Controls.Add(CreateTitle("🔗  Comunicação com CLP"), 0, 0);
        layout.Controls.Add(CreateStatusText("Status:    ● Disconnected", AccentRedColor, true), 0, 1);
        layout.Controls.Add(CreateStatusText("Mensagem:  Desconectado", Color.White, false), 0, 2);
        layout.Controls.Add(CreateDivider(), 0, 3);
        layout.Controls.Add(CreateCommunicationFields(), 0, 4);
        layout.Controls.Add(CreateDivider(), 0, 5);
        layout.Controls.Add(CreateCommunicationFooter(), 0, 6);

        panel.Controls.Add(layout);

        return panel;
    }

    private Control CreateCommunicationFields()
    {
        TableLayoutPanel fields = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(0, 14, 0, 14),
            BackColor = PanelColor
        };

        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

        AddFieldRow(fields, 0, "Porta COM", "COM1");
        AddFieldRow(fields, 1, "Baud rate", "9600");
        AddFieldRow(fields, 2, "Slave ID", "1");

        return fields;
    }

    private Control CreateCommunicationFooter()
    {
        TableLayoutPanel footer = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = PanelColor
        };

        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23F));

        footer.Controls.Add(CreateFooterLabel("●  Pronto para conectar", AccentGreenColor), 0, 0);
        footer.Controls.Add(CreateFooterButton("⏻  Conectar CLP", AccentBlueColor), 1, 0);
        footer.Controls.Add(CreateFooterLabel("▣  Configuração salva", TextMutedColor), 2, 0);
        footer.Controls.Add(CreateFooterButton("⚙  Avançado", AccentBlueColor), 3, 0);

        return footer;
    }

    private Control CreateIoPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(5, 0, 0, 0);
        panel.Padding = new Padding(18);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = PanelColor
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        layout.Controls.Add(CreateTitle("🎛  I/O Manual"), 0, 0);
        layout.Controls.Add(CreateIoContent(), 0, 1);

        panel.Controls.Add(layout);

        return panel;
    }

    private Control CreateIoContent()
    {
        TableLayoutPanel content = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = PanelColor
        };

        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));

        content.Controls.Add(CreateDoColumn(), 0, 0);
        content.Controls.Add(CreateVerticalDivider(), 1, 0);
        content.Controls.Add(CreateDiGrid(), 2, 0);

        return content;
    }

    private Control CreateDoColumn()
    {
        TableLayoutPanel doColumn = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(10, 0, 18, 0),
            BackColor = PanelColor
        };

        doColumn.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        doColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        doColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        doColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        doColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

        doColumn.Controls.Add(CreateSubTitle("Saídas digitais (D)"), 0, 0);
        doColumn.Controls.Add(CreateDoRow("D000", AccentRedColor), 0, 1);
        doColumn.Controls.Add(CreateDoRow("D001", Color.FromArgb(75, 180, 75)), 0, 2);
        doColumn.Controls.Add(CreateDoRow("D002", AccentYellowColor), 0, 3);
        doColumn.Controls.Add(CreateDoRow("D003", Color.DimGray), 0, 4);

        return doColumn;
    }

    private Control CreateDiGrid()
    {
        TableLayoutPanel di = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(18, 0, 10, 0),
            BackColor = PanelColor
        };

        di.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        di.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        di.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        di.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        di.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        di.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        di.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

        Label title = CreateSubTitle("Entradas digitais (DI)");
        di.Controls.Add(title, 0, 0);
        di.SetColumnSpan(title, 2);

        di.Controls.Add(CreateDiCell("DI00", true), 0, 1);
        di.Controls.Add(CreateDiCell("DI04", true), 1, 1);
        di.Controls.Add(CreateDiCell("DI01", true), 0, 2);
        di.Controls.Add(CreateDiCell("DI05", false), 1, 2);
        di.Controls.Add(CreateDiCell("DI02", true), 0, 3);
        di.Controls.Add(CreateDiCell("DI06", false), 1, 3);
        di.Controls.Add(CreateDiCell("DI03", false), 0, 4);
        di.Controls.Add(CreateDiCell("DI07", false), 1, 4);

        return di;
    }

    private Control CreateTerminalPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 8, 0, 0);
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = PanelColor
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        layout.Controls.Add(CreateTerminalHeader(), 0, 0);
        layout.Controls.Add(CreateTerminalBox(), 0, 1);

        panel.Controls.Add(layout);

        return panel;
    }

    private Control CreateTerminalHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = PanelColor
        };

        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));

        header.Controls.Add(CreateTitle("▣  Terminal / Log"), 0, 0);
        header.Controls.Add(CreateFooterButton("🧹  Limpar log", TextMutedColor), 1, 0);

        return header;
    }

    private Control CreateTerminalBox()
    {
        TextBox terminal = new()
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BackColor = Color.FromArgb(4, 7, 10),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 10F),
            Text =
                "[15:21:02.123] Sistema iniciado.\r\n" +
                "[15:21:05.450] Portas COM atualizadas: COM1, COM2, COM3, COM4\r\n" +
                "[15:21:10.250] Tentando abrir porta COM1 (9600 bps)...\r\n" +
                "[15:21:20.843] Detectar CLP solicitado em COM1\r\n" +
                "[15:21:21.355] >> 01 03 00 00 00 02 C4 0B\r\n" +
                "[15:21:21.612] << 01 03 04 12 34 56 78 9A BC"
        };

        return terminal;
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

    private static Label CreateTitle(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Label CreateSubTitle(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(150, 205, 255),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Label CreateStatusText(string text, Color color, bool bold)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, bold ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = color,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Control CreateDivider()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            Height = 1,
            BackColor = BorderColor,
            Margin = new Padding(0)
        };
    }

    private static Control CreateVerticalDivider()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            Width = 1,
            BackColor = BorderColor,
            Margin = new Padding(0, 8, 0, 8)
        };
    }

    private static void AddFieldRow(TableLayoutPanel layout, int row, string labelText, string valueText)
    {
        Label label = new()
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };

        ComboBox combo = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F),
            Margin = new Padding(0, 5, 0, 5)
        };

        combo.Items.Add(valueText);
        combo.SelectedIndex = 0;

        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(combo, 1, row);
    }

    private static Button CreateTopButton(string icon, string text, Color accentColor)
    {
        Button button = new()
        {
            Text = $"{icon}  {text}",
            Width = 170,
            Height = 36,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(18, 25, 32),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 10, 0)
        };

        button.FlatAppearance.BorderColor = accentColor;
        button.FlatAppearance.BorderSize = 1;

        return button;
    }

    private static Button CreateFooterButton(string text, Color borderColor)
    {
        Button button = new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 42, 52),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(4, 8, 4, 8)
        };

        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.BorderSize = 1;

        return button;
    }

    private static Label CreateFooterLabel(string text, Color color)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = color,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(4, 0, 4, 0)
        };
    }

    private static Control CreateDoRow(string label, Color color)
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(18, 25, 32),
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(16, 0, 16, 0)
        };

        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));

        Label name = new()
        {
            Text = label,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };

        Panel led = CreateRoundLed(color, 34);

        row.Controls.Add(name, 0, 0);
        row.Controls.Add(led, 1, 0);

        return row;
    }

    private static Control CreateDiCell(string label, bool on)
    {
        TableLayoutPanel cell = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(18, 25, 32),
            Margin = new Padding(8, 5, 8, 5),
            Padding = new Padding(12, 0, 12, 0)
        };

        cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        cell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));

        Label name = new()
        {
            Text = label,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };

        Panel led = CreateRoundLed(on ? Color.LimeGreen : Color.DimGray, 24);

        cell.Controls.Add(name, 0, 0);
        cell.Controls.Add(led, 1, 0);

        return cell;
    }

    private static Panel CreateRoundLed(Color color, int size)
    {
        Panel led = new()
        {
            Width = size,
            Height = size,
            BackColor = color,
            Margin = new Padding(8),
            Anchor = AnchorStyles.None
        };

        led.Paint += (_, e) =>
        {
            using Pen border = new(Color.FromArgb(130, 130, 130), 3F);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(new SolidBrush(color), 2, 2, size - 4, size - 4);
            e.Graphics.DrawEllipse(border, 1, 1, size - 3, size - 3);
        };

        return led;
    }
}
