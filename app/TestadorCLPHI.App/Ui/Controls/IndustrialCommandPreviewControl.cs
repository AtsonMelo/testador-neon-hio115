namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialCommandPreviewControl : UserControl
{
    private static readonly Color PanelColor = Color.FromArgb(24, 32, 42);
    private static readonly Color BorderColor = Color.FromArgb(64, 78, 92);
    private static readonly Color AccentGreenColor = Color.FromArgb(62, 210, 92);
    private static readonly Color AccentYellowColor = Color.FromArgb(245, 190, 45);
    private static readonly Color AccentBlueColor = Color.FromArgb(74, 144, 226);

    public IndustrialCommandPreviewControl()
    {
        Dock = DockStyle.Fill;
        BackColor = PanelColor;
        ForeColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        Padding = new Padding(10, 8, 10, 8);
        Font = new Font("Segoe UI", 10F);

        BuildLayout();
    }

    private void BuildLayout()
    {
        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0),
            BackColor = PanelColor
        };

        buttons.Controls.Add(CreateCommandButton("Habilitar teste", AccentGreenColor));
        buttons.Controls.Add(CreateCommandButton("Resetar saídas", AccentYellowColor));
        buttons.Controls.Add(CreateCommandButton("Teste automático", AccentBlueColor));

        Controls.Add(buttons);
    }

    private static Button CreateCommandButton(string text, Color accentColor)
    {
        Button button = new()
        {
            Text = text,
            Width = 210,
            Height = 34,
            Dock = DockStyle.None,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 42, 52),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 10, 0)
        };

        button.FlatAppearance.BorderColor = BorderColor;
        button.FlatAppearance.BorderSize = 1;

        Panel accentStrip = new()
        {
            Width = 4,
            Dock = DockStyle.Left,
            BackColor = accentColor
        };

        button.Controls.Add(accentStrip);

        return button;
    }
}
