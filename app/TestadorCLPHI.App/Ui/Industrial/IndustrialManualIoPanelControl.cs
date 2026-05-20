namespace TestadorCLPHI.App.Ui.Industrial;

public sealed class IndustrialManualIoPanelControl : UserControl
{
    private static readonly Color PanelColor = Color.FromArgb(22, 31, 40);
    private static readonly Color RowColor = Color.FromArgb(24, 34, 44);
    private static readonly Color BorderColor = Color.FromArgb(58, 72, 86);
    private static readonly Color TitleColor = Color.FromArgb(226, 232, 240);
    private static readonly Color SubTitleColor = Color.FromArgb(150, 205, 255);

    private readonly Control[] _outputRows = new Control[4];
    private readonly PictureBox[] _inputIndicators = new PictureBox[8];
    private readonly bool[] _inputStates = new bool[8];

    public event Action<int>? OutputCommandClicked;

    public event EventHandler? RefreshInputsClicked;

    public IndustrialManualIoPanelControl()
    {
        Dock = DockStyle.Fill;
        BackColor = PanelColor;
        MinimumSize = new Size(440, 260);

        Panel panel = CreateCardPanel();
        panel.Padding = new Padding(18);
        panel.Dock = DockStyle.Fill;

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = PanelColor
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Label ioTitle = CreateTitle("🎛  I/O Manual");
        ioTitle.TextAlign = ContentAlignment.MiddleCenter;

        layout.Controls.Add(ioTitle, 0, 0);
        layout.Controls.Add(CreateIoContent(), 0, 1);

        panel.Controls.Add(layout);
        Controls.Add(panel);
    }

    public void SetInputState(int channel, bool active)
    {
        if (channel < 0 || channel >= _inputIndicators.Length)
        {
            return;
        }

        _inputStates[channel] = active;

        PictureBox indicator = _inputIndicators[channel];
        indicator.Image?.Dispose();
        indicator.Image = LoadAssetBitmap(active ? "led_on_green.png" : "led_off_gray.png");
    }

    public void SetPanelEnabled(bool enabled)
    {
        foreach (Control row in _outputRows)
        {
            row.Enabled = enabled;
            row.Cursor = enabled ? Cursors.Hand : Cursors.Default;
        }
    }

    public void RequestRefreshInputs()
    {
        RefreshInputsClicked?.Invoke(this, EventArgs.Empty);
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
        doColumn.Controls.Add(CreateDoRow(0, "D000", "push_button_red.png"), 0, 1);
        doColumn.Controls.Add(CreateDoRow(1, "D001", "push_button_green.png"), 0, 2);
        doColumn.Controls.Add(CreateDoRow(2, "D002", "push_button_yellow.png"), 0, 3);
        doColumn.Controls.Add(CreateDoRow(3, "D003", "push_button_blue.png"), 0, 4);

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

        di.Controls.Add(CreateDiCell("DI00", 0), 0, 1);
        di.Controls.Add(CreateDiCell("DI04", 4), 1, 1);
        di.Controls.Add(CreateDiCell("DI01", 1), 0, 2);
        di.Controls.Add(CreateDiCell("DI05", 5), 1, 2);
        di.Controls.Add(CreateDiCell("DI02", 2), 0, 3);
        di.Controls.Add(CreateDiCell("DI06", 6), 1, 3);
        di.Controls.Add(CreateDiCell("DI03", 3), 0, 4);
        di.Controls.Add(CreateDiCell("DI07", 7), 1, 4);

        return di;
    }

    private Control CreateDoRow(int channel, string label, string imageFileName)
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = RowColor,
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(16, 0, 16, 0),
            Cursor = Cursors.Hand
        };

        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));

        Label name = new()
        {
            Text = label,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand
        };

        PictureBox buttonImage = CreatePngImage(imageFileName, 42);
        buttonImage.Cursor = Cursors.Hand;

        row.Controls.Add(name, 0, 0);
        row.Controls.Add(buttonImage, 1, 0);

        row.Click += (_, _) => OutputCommandClicked?.Invoke(channel);
        name.Click += (_, _) => OutputCommandClicked?.Invoke(channel);
        buttonImage.Click += (_, _) => OutputCommandClicked?.Invoke(channel);

        _outputRows[channel] = row;

        return row;
    }

    private Control CreateDiCell(string label, int channel)
    {
        TableLayoutPanel cell = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = RowColor,
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

        PictureBox ledImage = CreatePngImage("led_off_gray.png", 36);

        cell.Controls.Add(name, 0, 0);
        cell.Controls.Add(ledImage, 1, 0);

        _inputIndicators[channel] = ledImage;

        return cell;
    }

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
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
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = TitleColor,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Label CreateSubTitle(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = SubTitleColor,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Control CreateVerticalDivider()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = BorderColor,
            Margin = new Padding(0, 8, 0, 8)
        };
    }

    private static PictureBox CreatePngImage(string imageFileName, int size)
    {
        PictureBox picture = new()
        {
            Width = size,
            Height = size,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(8),
            Anchor = AnchorStyles.None,
            BackColor = RowColor
        };

        picture.Image = LoadAssetBitmap(imageFileName);

        return picture;
    }

    private static Bitmap? LoadAssetBitmap(string imageFileName)
    {
        string imagePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Ui",
            imageFileName);

        if (!File.Exists(imagePath))
        {
            return null;
        }

        using Image image = Image.FromFile(imagePath);
        return new Bitmap(image);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (PictureBox indicator in _inputIndicators)
            {
                indicator.Image?.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
