namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialConnectionPreviewControl : UserControl
{
    private static readonly Color PanelColor = Color.FromArgb(24, 32, 42);
    private static readonly Color PanelDarkColor = Color.FromArgb(18, 25, 32);
    private static readonly Color BorderColor = Color.FromArgb(64, 78, 92);
    private static readonly Color TextMutedColor = Color.FromArgb(170, 190, 210);
    private static readonly Color AccentBlueColor = Color.FromArgb(74, 144, 226);

    public IndustrialConnectionPreviewControl()
    {
        Dock = DockStyle.Fill;
        BackColor = PanelColor;
        ForeColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        Padding = new Padding(18);
        Font = new Font("Segoe UI", 10F);

        BuildLayout();
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = PanelColor
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));

        root.Controls.Add(CreateLeftColumn(), 0, 0);
        root.Controls.Add(CreateRightColumn(), 1, 0);

        Controls.Add(root);
    }

    private Control CreateLeftColumn()
    {
        TableLayoutPanel left = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Margin = new Padding(0, 0, 18, 0)
        };

        left.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Label title = CreateSectionTitle("Conexão com CLP");
        left.Controls.Add(title, 0, 0);
        left.SetColumnSpan(title, 2);

        AddFormRow(left, 1, "Porta COM:", "COM1");
        AddFormRow(left, 2, "Baud rate:", "9600");
        AddFormRow(left, 3, "Slave ID:", "1");

        Label summary = new()
        {
            Text = "Atual: COM1, 9600 bps, Slave 1\r\nParidade: None, Stop bits: One, Timeout: 1000 ms",
            Dock = DockStyle.Fill,
            BackColor = PanelDarkColor,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12, 8, 12, 8),
            ForeColor = TextMutedColor,
            TextAlign = ContentAlignment.MiddleLeft
        };

        left.Controls.Add(summary, 0, 4);
        left.SetColumnSpan(summary, 2);

        return left;
    }

    private Control CreateRightColumn()
    {
        TableLayoutPanel right = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Margin = new Padding(18, 38, 0, 0)
        };

        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

        Button refreshButton = CreateActionButton("Atualizar portas");
        refreshButton.Margin = new Padding(0, 0, 0, 6);

        CheckBox allBaudRatesCheckBox = new()
        {
            Text = "Todos",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            BackColor = PanelColor,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            Font = new Font("Segoe UI", 10F),
            Margin = new Padding(0)
        };

        CheckedListBox baudRateList = new()
        {
            Dock = DockStyle.Fill,
            BackColor = PanelDarkColor,
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            CheckOnClick = true,
            IntegralHeight = false,
            Font = new Font("Segoe UI", 10F),
            Margin = new Padding(0)
        };

        baudRateList.Items.Add("9600", true);
        baudRateList.Items.Add("19200", true);
        baudRateList.Items.Add("38400", true);

        Button detectButton = CreateActionButton("Detectar CLP");
        detectButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        detectButton.Margin = new Padding(0);

        right.Controls.Add(refreshButton, 0, 0);
        right.Controls.Add(allBaudRatesCheckBox, 0, 1);
        right.Controls.Add(baudRateList, 0, 2);
        right.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = PanelColor }, 0, 3);
        right.Controls.Add(detectButton, 0, 4);

        return right;
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

    private static void AddFormRow(TableLayoutPanel layout, int row, string labelText, string valueText)
    {
        Label label = new()
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };

        ComboBox combo = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 4, 0, 4),
            Font = new Font("Segoe UI", 10F)
        };

        combo.Items.Add(valueText);
        combo.SelectedIndex = 0;

        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(combo, 1, row);
    }

    private static Button CreateActionButton(string text)
    {
        Button button = new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(34, 42, 52),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        button.FlatAppearance.BorderColor = BorderColor;
        button.FlatAppearance.BorderSize = 1;

        return button;
    }
}
