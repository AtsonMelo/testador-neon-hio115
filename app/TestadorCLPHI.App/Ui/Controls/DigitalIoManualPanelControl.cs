namespace TestadorCLPHI.App.Ui.Controls;

public sealed class DigitalIoManualPanelControl : UserControl
{
    private readonly GroupBox _groupBox;
    private readonly IndustrialPushButtonControl[] _outputButtons = new IndustrialPushButtonControl[4];
    private readonly IndustrialLedIndicatorControl[] _inputIndicators = new IndustrialLedIndicatorControl[8];
    private readonly Button _refreshInputsButton;

    public event Action<int>? OutputCommandClicked;

    public event EventHandler? RefreshInputsClicked;

    public DigitalIoManualPanelControl()
    {
        Width = 690;
        Height = 285;
        MinimumSize = new Size(720, 285);

        _groupBox = new GroupBox
        {
            Text = "Painel manual industrial 4DO/8DI",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 16, 10, 8)
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(4)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        TableLayoutPanel headerLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };

        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));

        Label instructionLabel = new()
        {
            Text = "1) Clique em Habilitar teste   2) Acione a saída",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        _refreshInputsButton = new Button
        {
            Text = "Atualizar DI",
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 0, 0, 4)
        };

        _refreshInputsButton.Click += (_, e) => RefreshInputsClicked?.Invoke(this, e);

        headerLayout.Controls.Add(instructionLabel, 0, 0);
        headerLayout.Controls.Add(_refreshInputsButton, 1, 0);

        FlowLayoutPanel outputsPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        string assetDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui");

        (string Title, string Description, string ImageName)[] outputs =
        [
            ("D000", "D000 -> DI00 + DI04", "push_button_red.png"),
            ("D001", "D001 -> DI01 + DI05", "push_button_green.png"),
            ("D002", "D002 -> DI02 + DI06", "push_button_yellow.png"),
            ("D003", "D003 -> DI03 + DI07", "push_button_blue.png")
        ];

        for (int channel = 0; channel < _outputButtons.Length; channel++)
        {
            int capturedChannel = channel;
            (string title, string description, string imageName) = outputs[channel];

            IndustrialPushButtonControl button = new()
            {
                Title = title,
                Description = description,
                ButtonImagePath = Path.Combine(assetDir, imageName),
                Width = 136,
                Height = 82,
                Margin = new Padding(0, 0, 10, 0),
                BackColor = Color.FromArgb(31, 31, 31),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.Click += (_, _) => OutputCommandClicked?.Invoke(capturedChannel);

            _outputButtons[channel] = button;
            outputsPanel.Controls.Add(button);
        }

        Label inputsTitleLabel = new()
        {
            Text = "Entradas digitais",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        FlowLayoutPanel inputsPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Margin = new Padding(0),
            Padding = new Padding(118, 0, 0, 0)
        };

        string onImagePath = Path.Combine(assetDir, "led_on_green.png");
        string offImagePath = Path.Combine(assetDir, "led_off_gray.png");

        for (int channel = 0; channel < _inputIndicators.Length; channel++)
        {
            IndustrialLedIndicatorControl indicator = new()
            {
                LabelText = $"DI{channel:00}",
                Width = 50,
                Height = 70,
                Margin = new Padding(0, 0, 10, 0),
                BackColor = Color.FromArgb(31, 31, 31),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7.8F, FontStyle.Bold)
            };

            indicator.LoadImages(onImagePath, offImagePath);

            _inputIndicators[channel] = indicator;
            inputsPanel.Controls.Add(indicator);
        }

        layout.Controls.Add(headerLayout, 0, 0);
        layout.Controls.Add(outputsPanel, 0, 1);
        layout.Controls.Add(inputsTitleLabel, 0, 2);
        layout.Controls.Add(inputsPanel, 0, 3);

        _groupBox.Controls.Add(layout);
        Controls.Add(_groupBox);
    }

    public void SetInputState(int channel, bool active)
    {
        if (channel < 0 || channel >= _inputIndicators.Length)
        {
            return;
        }

        _inputIndicators[channel].IsOn = active;
    }

    public void SetPanelEnabled(bool enabled)
    {
        foreach (IndustrialPushButtonControl button in _outputButtons)
        {
            button.Enabled = enabled;
        }

        _refreshInputsButton.Enabled = enabled;
    }
}
