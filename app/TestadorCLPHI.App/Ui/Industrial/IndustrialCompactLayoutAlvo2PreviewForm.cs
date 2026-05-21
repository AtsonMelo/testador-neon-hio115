namespace TestadorCLPHI.App.Ui.Industrial;

public sealed class IndustrialCompactLayoutAlvo2PreviewForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(10, 16, 22);
    private static readonly Color PanelColor = Color.FromArgb(22, 31, 40);
    private static readonly Color BorderColor = Color.FromArgb(58, 72, 86);
    private static readonly Color TextMutedColor = Color.FromArgb(165, 185, 205);
    private static readonly Color AccentBlueColor = Color.FromArgb(74, 144, 226);
    private static readonly Color AccentGreenColor = Color.FromArgb(62, 210, 92);
    private static readonly Color AccentYellowColor = Color.FromArgb(245, 190, 45);
    private static readonly Color AccentRedColor = Color.FromArgb(245, 68, 68);

    public IndustrialCompactLayoutAlvo2PreviewForm()
    {
        Text = "Preview - Layout Alvo 2 Compacto | Testador CLP HI";
        Width = 1280;
        Height = 800;
        MinimumSize = new Size(1180, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BackgroundColor;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);
        AutoScroll = true;

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

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 442F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(CreateTopCommandBar(), 0, 0);
        root.Controls.Add(CreateMainArea(), 0, 1);
        root.Controls.Add(CreateTerminalPanel(), 0, 2);

        Controls.Add(root);
    }

    private Control CreateTopCommandBar()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(8);

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

        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));

        main.Controls.Add(CreateCommunicationPanel(), 0, 0);
        main.Controls.Add(CreateIoPanel(), 1, 0);

        return main;
    }

    private Control CreateCommunicationPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 0, 6, 0);
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = PanelColor
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));

        layout.Controls.Add(CreateTitle("🔗  Comunicação com CLP"), 0, 0);
        layout.Controls.Add(CreateStatusText("Status:    ● Disconnected", AccentRedColor), 0, 1);
        layout.Controls.Add(CreateDivider(), 0, 2);
        layout.Controls.Add(CreateCommunicationFields(), 0, 3);
        layout.Controls.Add(CreateDivider(), 0, 4);
        layout.Controls.Add(CreateCommunicationFooter(), 0, 5);

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
            BackColor = PanelColor
        };

        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

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
            ColumnCount = 3,
            RowCount = 1,
            BackColor = PanelColor
        };

        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));

        footer.Controls.Add(CreateFooterLabel("● Pronto p/ conectar", AccentGreenColor), 0, 0);
        footer.Controls.Add(CreateFooterButton("⏻  Conectar CLP", AccentBlueColor), 1, 0);
        footer.Controls.Add(CreateFooterButton("⚙  Avançado", AccentBlueColor), 2, 0);

        return footer;
    }

    private Control CreateIoPanel()
    {
        IndustrialManualIoPanelControl panel = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 0, 0, 0)
        };

        panel.SetInputState(0, true);
        panel.SetInputState(1, true);
        panel.SetInputState(2, true);
        panel.SetInputState(4, true);

        return panel;
    }

    private Control CreateTerminalPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 8, 0, 0);
        panel.Padding = new Padding(12);

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
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));

        header.Controls.Add(CreateTitle("▣  Terminal / Log"), 0, 0);
        Button clearLogButton = CreateFooterButton("🧹  Limpar log", TextMutedColor);
        clearLogButton.Height = 32;
        clearLogButton.Margin = new Padding(4, 4, 4, 4);
        clearLogButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        header.Controls.Add(clearLogButton, 1, 0);

        return header;
    }

    private Control CreateTerminalBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BackColor = Color.FromArgb(3, 6, 9),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 9F),
            Text =
                "[15:21:02.123] Sistema iniciado.\r\n" +
                "[15:21:05.450] Portas COM atualizadas: COM1, COM2, COM3, COM4\r\n" +
                "[15:21:10.250] Tentando abrir porta COM1 (9600 bps)...\r\n" +
                "[15:21:20.843] Detectar CLP solicitado em COM1\r\n" +
                "[15:21:21.355] >> 01 03 00 00 00 02 C4 0B\r\n" +
                "[15:21:21.612] << 01 03 04 12 34 56 78 9A BC"
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

    private static Label CreateTitle(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Label CreateStatusText(string text, Color color)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
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

    private static void AddFieldRow(TableLayoutPanel layout, int row, string labelText, string valueText)
    {
        Label label = new()
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 8, 0)
        };

        ComboBox combo = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.5F),
            Margin = new Padding(0, 4, 0, 4)
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
            Width = 180,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(24, 34, 44),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 8, 0)
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
            Dock = DockStyle.None,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(24, 34, 44),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(4, 8, 4, 2)
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
}
