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
        Height = 220;

        _groupBox = new GroupBox
        {
            Text = "Painel manual industrial 4DO/8DI",
            Dock = DockStyle.Fill
        };

        Label outputsTitleLabel = new()
        {
            Text = "1) Clique em Habilitar teste  2) Acione a saída",
            AutoSize = true,
            Left = 15,
            Top = 24,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _groupBox.Controls.Add(outputsTitleLabel);

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
                Left = 15 + (channel * 145),
                Top = 46,
                Width = 132,
                Height = 92,
                BackColor = Color.FromArgb(31, 31, 31),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.Click += (_, _) => OutputCommandClicked?.Invoke(capturedChannel);

            _outputButtons[channel] = button;
            _groupBox.Controls.Add(button);
        }

        Label inputsTitleLabel = new()
        {
            Text = "Entradas digitais",
            AutoSize = true,
            Left = 15,
            Top = 150,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _groupBox.Controls.Add(inputsTitleLabel);

        string onImagePath = Path.Combine(assetDir, "led_on_green.png");
        string offImagePath = Path.Combine(assetDir, "led_off_gray.png");

        for (int channel = 0; channel < _inputIndicators.Length; channel++)
        {
            IndustrialLedIndicatorControl indicator = new()
            {
                LabelText = $"DI{channel:00}",
                Left = 135 + (channel * 58),
                Top = 142,
                Width = 54,
                Height = 68,
                BackColor = Color.FromArgb(31, 31, 31),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };

            indicator.LoadImages(onImagePath, offImagePath);

            _inputIndicators[channel] = indicator;
            _groupBox.Controls.Add(indicator);
        }

        _refreshInputsButton = new Button
        {
            Text = "Atualizar",
            Left = 585,
            Top = 170,
            Width = 85,
            Height = 28,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _refreshInputsButton.Click += (_, e) => RefreshInputsClicked?.Invoke(this, e);

        _groupBox.Controls.Add(_refreshInputsButton);
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
