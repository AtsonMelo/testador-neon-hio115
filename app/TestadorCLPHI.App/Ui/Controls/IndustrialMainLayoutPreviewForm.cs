namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialMainLayoutPreviewForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(16, 22, 28);
    private static readonly Color PanelColor = Color.FromArgb(24, 32, 42);
    private static readonly Color PanelDarkColor = Color.FromArgb(18, 25, 32);
    private static readonly Color BorderColor = Color.FromArgb(64, 78, 92);
    private static readonly Color TextMutedColor = Color.FromArgb(170, 190, 210);
    private static readonly Color AccentBlueColor = Color.FromArgb(74, 144, 226);
    private static readonly Color AccentGreenColor = Color.FromArgb(62, 210, 92);
    private static readonly Color AccentRedColor = Color.FromArgb(245, 68, 68);
    private static readonly Color AccentYellowColor = Color.FromArgb(245, 190, 45);

    public IndustrialMainLayoutPreviewForm()
    {
        Text = "Preview - Layout Alvo 1";
        Width = 1400;
        Height = 900;
        MinimumSize = new Size(1100, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BackgroundColor;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        AutoScroll = true;

        BuildLayout();
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(14),
            BackColor = BackgroundColor
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 300F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(CreateConnectionArea(), 0, 1);
        root.Controls.Add(CreateCommandsPanel(), 0, 2);
        root.Controls.Add(CreateIoPanel(), 0, 3);
        root.Controls.Add(CreateTerminalPanel(), 0, 4);

        Controls.Add(root);
    }

    private Control CreateHeader()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(22, 16, 22, 16);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));

        FlowLayoutPanel titlePanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        Label title = new()
        {
            Text = "Testador CLP HI",
            AutoSize = true,
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            ForeColor = Color.White
        };

        Label subtitle = new()
        {
            Text = "Comunicação Modbus RTU real em fase inicial.",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F),
            ForeColor = TextMutedColor
        };

        titlePanel.Controls.Add(title);
        titlePanel.Controls.Add(subtitle);

        TableLayoutPanel statusPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(10)
        };

        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        statusPanel.Controls.Add(CreateStatusBlock("CLP", "Desconectado", AccentRedColor), 0, 0);

        Panel divider = new()
        {
            Dock = DockStyle.Fill,
            BackColor = BorderColor,
            Margin = new Padding(6, 16, 6, 16)
        };

        statusPanel.Controls.Add(divider, 1, 0);
        statusPanel.Controls.Add(CreateStatusBlock("Teste", "Desabilitado", Color.Gray), 2, 0);

        TableLayoutPanel stopPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };

        stopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        stopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        string stopPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", "stop_emergency.png");

        PictureBox stopImage = new()
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = File.Exists(stopPath) ? Image.FromFile(stopPath) : null,
            Margin = new Padding(0)
        };

        FlowLayoutPanel stopTextPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(14, 24, 0, 0)
        };

        stopTextPanel.Controls.Add(new Label
        {
            Text = "STOP",
            AutoSize = true,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = AccentYellowColor
        });

        stopTextPanel.Controls.Add(new Label
        {
            Text = "Emergência",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.White
        });

        stopPanel.Controls.Add(stopImage, 0, 0);
        stopPanel.Controls.Add(stopTextPanel, 1, 0);

        layout.Controls.Add(titlePanel, 0, 0);
        layout.Controls.Add(statusPanel, 1, 0);
        layout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = BorderColor, Margin = new Padding(6, 8, 6, 8) }, 2, 0);
        layout.Controls.Add(stopPanel, 3, 0);

        panel.Controls.Add(layout);

        return panel;
    }

    private Control CreateConnectionArea()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 10, 0, 0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

        layout.Controls.Add(CreateStatePanel(), 0, 0);
        layout.Controls.Add(CreateClpConnectionPanel(), 1, 0);

        return layout;
    }

    private Control CreateStatePanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 0, 8, 0);
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));

        layout.Controls.Add(CreateSectionTitle("Estado da conexão"), 0, 0);

        FlowLayoutPanel info = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(4, 8, 0, 0)
        };

        info.Controls.Add(CreateInfoLine("Status:", "Disconnected", AccentRedColor));
        info.Controls.Add(CreateInfoLine("Mensagem:", "Desconectado", Color.White));
        info.Controls.Add(CreateInfoLine("Atualizado:", "15:21:10", Color.White));

        TableLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };

        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        buttons.Controls.Add(CreateActionButton("Conectar CLP", AccentGreenColor), 0, 0);
        buttons.Controls.Add(CreateActionButton("Simular erro", AccentYellowColor), 1, 0);
        buttons.Controls.Add(CreateActionButton("Desconectar", AccentRedColor), 0, 1);
        buttons.Controls.Add(CreateActionButton("Ler %MW70", AccentBlueColor), 1, 1);

        layout.Controls.Add(info, 0, 1);
        layout.Controls.Add(buttons, 0, 2);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateClpConnectionPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(8, 0, 0, 0);
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 5
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));

        layout.Controls.Add(CreateSectionTitle("Conexão com CLP"), 0, 0);
        layout.SetColumnSpan(layout.Controls[0], 3);

        AddFormRow(layout, 0, "Porta COM:", "COM1");
        AddFormRow(layout, 1, "Baud rate:", "9600");
        AddFormRow(layout, 2, "Slave ID:", "1");

        TableLayoutPanel searchPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(20, 0, 0, 0)
        };

        searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        searchPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        searchPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        searchPanel.Controls.Add(CreateActionButton("Atualizar portas", AccentBlueColor), 0, 0);
        searchPanel.SetColumnSpan(searchPanel.Controls[0], 2);

        CheckedListBox baudList = new()
        {
            Dock = DockStyle.Fill,
            BackColor = PanelDarkColor,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            CheckOnClick = true,
            IntegralHeight = false
        };

        baudList.Items.Add("9600", true);
        baudList.Items.Add("19200", true);
        baudList.Items.Add("38400", true);

        CheckBox all = new()
        {
            Text = "Todos",
            Dock = DockStyle.Fill,
            ForeColor = Color.White
        };

        searchPanel.Controls.Add(baudList, 0, 1);
        searchPanel.Controls.Add(all, 1, 1);

        layout.Controls.Add(searchPanel, 2, 1);
        layout.SetRowSpan(searchPanel, 3);

        Label summary = new()
        {
            Text = "Atual: COM1, 9600 bps, Slave 1\r\nParidade: None, Stop bits: One, Timeout: 1000 ms",
            Dock = DockStyle.Fill,
            BackColor = PanelDarkColor,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10, 8, 10, 8),
            ForeColor = TextMutedColor
        };

        Button detect = CreateActionButton("Detectar CLP", AccentBlueColor);
        detect.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

        layout.Controls.Add(summary, 0, 4);
        layout.SetColumnSpan(summary, 2);
        layout.Controls.Add(detect, 2, 4);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateCommandsPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 10, 0, 0);
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        layout.Controls.Add(CreateSectionTitle("Comandos do testador"), 0, 0);
        layout.SetColumnSpan(layout.Controls[0], 4);

        layout.Controls.Add(CreateActionButton("Habilitar teste", AccentGreenColor), 0, 1);
        layout.Controls.Add(CreateActionButton("Resetar saídas", AccentYellowColor), 1, 1);
        layout.Controls.Add(CreateActionButton("Teste automático", AccentBlueColor), 2, 1);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateIoPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 10, 0, 0);
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));

        FlowLayoutPanel outputs = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        string assetDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui");

        (string Title, string Description, string Image)[] buttons =
        [
            ("D000", "D000 -> DI00+DI04", "push_button_red.png"),
            ("D001", "D001 -> DI01+DI05", "push_button_green.png"),
            ("D002", "D002 -> DI02+DI06", "push_button_yellow.png"),
            ("D003", "D003 -> DI03+DI07", "push_button_blue.png")
        ];

        foreach ((string title, string description, string image) in buttons)
        {
            outputs.Controls.Add(new IndustrialPushButtonControl
            {
                Title = title,
                Description = description,
                ButtonImagePath = Path.Combine(assetDir, image),
                Width = 140,
                Height = 126,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = PanelColor,
                ForeColor = Color.White
            });
        }

        TableLayoutPanel inputs = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 2,
            Padding = new Padding(16, 4, 0, 0)
        };

        for (int i = 0; i < 8; i++)
        {
            inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
        }

        inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        inputs.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        string onImage = Path.Combine(assetDir, "led_on_green.png");
        string offImage = Path.Combine(assetDir, "led_off_gray.png");

        for (int i = 0; i < 8; i++)
        {
            Label label = new()
            {
                Text = $"DI{i:00}",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };

            IndustrialLedIndicatorControl led = new()
            {
                LabelText = string.Empty,
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                BackColor = PanelColor
            };

            led.LoadImages(onImage, offImage);
            led.IsOn = i is 0 or 2 or 4;

            inputs.Controls.Add(label, i, 0);
            inputs.Controls.Add(led, i, 1);
        }

        layout.Controls.Add(outputs, 0, 0);
        layout.Controls.Add(inputs, 1, 0);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateTerminalPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Margin = new Padding(0, 10, 0, 0);
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        layout.Controls.Add(CreateSectionTitle("Terminal / Log"), 0, 0);
        layout.SetColumnSpan(layout.Controls[0], 2);

        RichTextBox terminal = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(8, 12, 16),
            ForeColor = Color.FromArgb(210, 230, 240),
            Font = new Font("Consolas", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            Text =
                "[15:21:02.123] Sistema iniciado.\r\n" +
                "[15:21:05.450] Portas COM atualizadas: COM1, COM2, COM3, COM4\r\n" +
                "[15:21:10.250] Tentando abrir porta COM1 (9600 bps)...\r\n" +
                "[15:21:20.843] Detectar CLP solicitado em COM1\r\n" +
                "[15:21:21.355] >> 01 03 00 00 00 02 C4 0B\r\n" +
                "[15:21:21.612] << 01 03 04 12 34 56 78 9A BC\r\n"
        };

        Button clear = CreateActionButton("Limpar log", Color.Gray);

        layout.Controls.Add(terminal, 0, 1);
        layout.Controls.Add(clear, 1, 1);

        panel.Controls.Add(layout);
        return panel;
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
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Control CreateStatusBlock(string title, string value, Color dotColor)
    {
        TableLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };

        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        panel.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.BottomCenter
        }, 0, 0);

        panel.Controls.Add(new Label
        {
            Text = "●  " + value,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = dotColor,
            TextAlign = ContentAlignment.TopCenter
        }, 0, 1);

        return panel;
    }

    private static Control CreateInfoLine(string label, string value, Color valueColor)
    {
        Label line = new()
        {
            Text = $"{label}    {value}",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = valueColor,
            Margin = new Padding(0, 0, 0, 10)
        };

        return line;
    }

    private static Button CreateActionButton(string text, Color accentColor)
    {
        return new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 42, 52),
            Font = new Font("Segoe UI", 10F),
            Margin = new Padding(6)
        };
    }

    private static void AddFormRow(TableLayoutPanel layout, int row, string labelText, string valueText)
    {
        Label label = new()
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.White
        };

        ComboBox combo = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 4, 10, 4)
        };

        combo.Items.Add(valueText);
        combo.SelectedIndex = 0;

        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(combo, 1, row);
    }
}
