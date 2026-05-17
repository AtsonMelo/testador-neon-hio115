using System.IO;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialDigitalIoPanelControl : UserControl
{
    private readonly IndustrialLedIndicatorControl[] _inputIndicators = new IndustrialLedIndicatorControl[8];

    public IndustrialDigitalIoPanelControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(18, 24, 32);
        Padding = new Padding(18);

        TableLayoutPanel rootLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        rootLayout.Controls.Add(CreateHeaderPanel(), 0, 0);
        rootLayout.Controls.Add(CreateOutputsPanel(), 0, 1);
        rootLayout.Controls.Add(CreateInputsPanel(), 0, 2);
        rootLayout.Controls.Add(CreateFooterPanel(), 0, 3);

        Controls.Add(rootLayout);
    }

    public void SetInputState(int channel, bool active)
    {
        if (channel < 0 || channel >= _inputIndicators.Length)
        {
            return;
        }

        _inputIndicators[channel].IsOn = active;
    }

    private static Panel CreateHeaderPanel()
    {
        Panel panel = CreateCardPanel();

        Label title = new()
        {
            Text = "Painel manual industrial 4DO/8DI",
            AutoSize = true,
            ForeColor = Color.FromArgb(226, 232, 240),
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            Left = 16,
            Top = 10
        };

        Label subtitle = new()
        {
            Text = "Protótipo visual isolado — sem lógica Modbus e sem alterar MainForm",
            AutoSize = true,
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9F),
            Left = 18,
            Top = 34
        };

        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);

        return panel;
    }

    private static Panel CreateOutputsPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(14);

        Label title = new()
        {
            Text = "Saídas digitais - DO",
            AutoSize = true,
            ForeColor = Color.FromArgb(226, 232, 240),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 24
        };

        FlowLayoutPanel flow = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.Transparent
        };

        string[] outputs =
        [
            "D000 -> DI00 + DI04",
            "D001 -> DI01 + DI05",
            "D002 -> DI02 + DI06",
            "D003 -> DI03 + DI07"
        ];

        for (int i = 0; i < outputs.Length; i++)
        {
            Button button = new()
            {
                Text = outputs[i],
                Width = 154,
                Height = 48,
                Margin = new Padding(0, 0, 12, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(35, 45, 58),
                ForeColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 65, 85);

            flow.Controls.Add(button);
        }

        panel.Controls.Add(flow);
        panel.Controls.Add(title);

        return panel;
    }

    private Panel CreateInputsPanel()
    {
        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(14);

        Label title = new()
        {
            Text = "Entradas digitais - DI",
            AutoSize = true,
            ForeColor = Color.FromArgb(226, 232, 240),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 24
        };

        FlowLayoutPanel flow = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(0, 6, 0, 0),
            BackColor = Color.Transparent
        };

        string onImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", "led_on_green.png");
        string offImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", "led_off_gray.png");

        for (int channel = 0; channel < _inputIndicators.Length; channel++)
        {
            IndustrialLedIndicatorControl indicator = new()
            {
                LabelText = $"DI{channel:00}",
                IsOn = channel is 0 or 3 or 4,
                Margin = new Padding(0, 0, 13, 0),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };

            _inputIndicators[channel] = indicator;
            indicator.LoadImages(onImagePath, offImagePath);
            flow.Controls.Add(indicator);
        }

        panel.Controls.Add(flow);
        panel.Controls.Add(title);

        return panel;
    }

    private static Panel CreateFooterPanel()
    {
        Panel panel = CreateCardPanel();

        Label label = new()
        {
            Text = "Área reservada para validação de espaçamento: o terminal/log do app principal deve permanecer visível após integração.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 16, 0),
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9F)
        };

        panel.Controls.Add(label);

        return panel;
    }

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            BackColor = Color.FromArgb(25, 33, 43)
        };
    }
}
