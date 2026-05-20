namespace TestadorCLPHI.App.Ui.Industrial;

public sealed class IndustrialMainContentControl : UserControl
{
    private static readonly Color BackgroundColor = Color.FromArgb(10, 16, 22);
    private static readonly Color PanelColor = Color.FromArgb(22, 31, 40);
    private static readonly Color BorderColor = Color.FromArgb(58, 72, 86);
    private static readonly Color TextMutedColor = Color.FromArgb(165, 185, 205);
    private static readonly Color AccentBlueColor = Color.FromArgb(74, 144, 226);
    private static readonly Color AccentGreenColor = Color.FromArgb(62, 210, 92);
    private static readonly Color AccentYellowColor = Color.FromArgb(245, 190, 45);
    private static readonly Color AccentRedColor = Color.FromArgb(245, 68, 68);

    public IndustrialMainContentControl()
    {
        Dock = DockStyle.Fill;
        MinimumSize = new Size(1120, 720);
        BackColor = BackgroundColor;

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
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));

        layout.Controls.Add(CreateTitle("🔗  Comunicação com CLP"), 0, 0);
        layout.Controls.Add(CreateStatusText("Status:    ● Disconnected", AccentRedColor, true), 0, 1);
        layout.Controls.Add(CreateDivider(), 0, 2);
        layout.Controls.Add(CreateCommunicationFields(), 0, 3);
        layout.Controls.Add(CreateDivider(), 0, 4);
        layout.Controls.Add(CreateCommunicationFooter(), 0, 5);

        panel.Controls.Add(layout);

        return panel;
    }
    private Control CreateCommunicationFields()
    {
        Panel host = new()
        {
            Dock = DockStyle.Fill,
            BackColor = PanelColor,
            Padding = new Padding(0, 14, 0, 14)
        };

        TableLayoutPanel fields = new()
        {
            Width = 560,
            Height = 150,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = PanelColor
        };

        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390F));

        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

        AddFieldRow(fields, 0, "Porta COM", "COM1");
        AddFieldRow(fields, 1, "Baud rate", "9600");
        AddFieldRow(fields, 2, "Slave ID", "1");

        host.Controls.Add(fields);

        return host;
    }


    private Control CreateCommunicationFooter()
    {
        TableLayoutPanel footer = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = PanelColor
        };

        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));

        footer.Controls.Add(CreateFooterLabel("●  Pronto para conectar", AccentGreenColor), 0, 0);
        footer.Controls.Add(CreateFooterButton("⏻  Conectar CLP", AccentBlueColor), 1, 0);
        footer.Controls.Add(CreateFooterButton("⚙  Avançado", AccentBlueColor), 2, 0);

        return footer;
    }



    private Control CreateIoPanel()
    {
        IndustrialManualIoPanelControl panel = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(5, 0, 0, 0)
        };

        panel.SetInputState(0, true);
        panel.SetInputState(4, true);
        panel.SetInputState(1, true);
        panel.SetInputState(2, true);

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
        doColumn.Controls.Add(CreateDoRow("D000", "push_button_red.png"), 0, 1);
        doColumn.Controls.Add(CreateDoRow("D001", "push_button_green.png"), 0, 2);
        doColumn.Controls.Add(CreateDoRow("D002", "push_button_yellow.png"), 0, 3);
        doColumn.Controls.Add(CreateDoRow("D003", "push_button_blue.png"), 0, 4);

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

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
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
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));

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
            BackColor = Color.FromArgb(3, 6, 9),
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
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 12, 0)
        };

        ComboBox combo = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F),
            Margin = new Padding(0, 6, 0, 6)
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
            Width = 190,
            Height = 36,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(24, 34, 44),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 10, 0)
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(82, 104, 126);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 45, 58);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 34, 44);

        button.Paint += (_, e) =>
        {
            using SolidBrush accentBrush = new(accentColor);
            e.Graphics.FillRectangle(accentBrush, 0, 0, 3, button.Height);
        };

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
            BackColor = Color.FromArgb(24, 34, 44),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(4, 5, 4, 5)
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

    private static Control CreateDoRow(string label, string imageFileName)
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(24, 34, 44),
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

        PictureBox buttonImage = CreatePngImage(imageFileName, 42);

        row.Controls.Add(name, 0, 0);
        row.Controls.Add(buttonImage, 1, 0);

        return row;
    }

    private static Control CreateDiCell(string label, bool on)
    {
        TableLayoutPanel cell = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(24, 34, 44),
            Margin = new Padding(8, 5, 8, 5),
            Padding = new Padding(12, 0, 12, 0)
        };

        cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        cell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54F));

        Label name = new()
        {
            Text = label,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };

        PictureBox ledImage = CreatePngImage(
            on ? "led_on_green.png" : "led_off_gray.png",
            36);

        cell.Controls.Add(name, 0, 0);
        cell.Controls.Add(ledImage, 1, 0);

        return cell;
    }

    private static PictureBox CreatePngImage(string imageFileName, int size)
    {
        string imagePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Ui",
            imageFileName);

        PictureBox picture = new()
        {
            Width = size,
            Height = size,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(8),
            Anchor = AnchorStyles.None,
            BackColor = Color.FromArgb(24, 34, 44)
        };

        if (File.Exists(imagePath))
        {
            using Image image = Image.FromFile(imagePath);
            picture.Image = new Bitmap(image);
        }

        return picture;
    }
}
