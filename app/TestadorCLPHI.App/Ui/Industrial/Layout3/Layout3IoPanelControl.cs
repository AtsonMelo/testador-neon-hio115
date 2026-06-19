namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3IoPanelControl : UserControl
{
    private readonly Layout3ThemePalette _palette;
    private readonly List<Image> _ownedImages = [];
    private readonly PictureBox[] _inputIndicators = new PictureBox[8];

    public Layout3IoPanelControl(Layout3ThemePalette palette)
    {
        _palette = palette;
        BackColor = _palette.Surface;
        BorderStyle = BorderStyle.FixedSingle;
        BuildLayout();
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = _palette.Surface,
            Padding = new Padding(14)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(CreateSafetyStrip(), 0, 1);
        root.Controls.Add(CreateIoArea(), 0, 2);
        Controls.Add(root);
    }

    private Control CreateHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = _palette.Surface
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

        Label index = CreateLabel("02", 8F, FontStyle.Bold, _palette.AccentBlue, ContentAlignment.MiddleCenter);
        index.BackColor = _palette.Field;
        index.Margin = new Padding(0, 4, 7, 4);
        header.Controls.Add(index, 0, 0);
        header.SetRowSpan(index, 2);
        header.Controls.Add(CreateLabel("I/O MANUAL", 10F, FontStyle.Bold, _palette.Text), 1, 0);
        header.Controls.Add(CreateLabel("Painel industrial em estado seguro", 8F, FontStyle.Regular, _palette.MutedText), 1, 1);
        return header;
    }

    private Control CreateSafetyStrip()
    {
        TableLayoutPanel strip = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Field,
            Margin = new Padding(0, 2, 0, 6),
            Padding = new Padding(12, 0, 12, 0)
        };
        strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
        strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        Label description = CreateLabel("ESCRITA FISICA BLOQUEADA", 8F, FontStyle.Bold, _palette.Text);
        Label state = CreateLabel("0 COMANDOS", 8F, FontStyle.Bold, _palette.MutedText, ContentAlignment.MiddleRight);
        description.BackColor = _palette.Field;
        state.BackColor = _palette.Field;
        strip.Controls.Add(description, 0, 0);
        strip.Controls.Add(state, 1, 0);
        return strip;
    }

    private Control CreateIoArea()
    {
        TableLayoutPanel area = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = _palette.Surface
        };
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        area.Controls.Add(CreateOutputs(), 0, 0);
        area.Controls.Add(new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _palette.Divider,
            Margin = new Padding(0, 8, 0, 4)
        }, 1, 0);
        area.Controls.Add(CreateInputs(), 2, 0);
        return area;
    }

    private Control CreateOutputs()
    {
        TableLayoutPanel outputs = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = _palette.Surface,
            Padding = new Padding(0, 0, 12, 0)
        };
        outputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        for (int row = 1; row < 5; row++)
        {
            outputs.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        }

        outputs.Controls.Add(CreateColumnHeader("SAIDAS DIGITAIS", "DO  /  bloqueadas"), 0, 0);
        for (int channel = 0; channel < 4; channel++)
        {
            outputs.Controls.Add(CreateOutputRow($"D{channel:000}"), 0, channel + 1);
        }
        return outputs;
    }

    private Control CreateOutputRow(string channel)
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = _palette.Row,
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(10, 2, 8, 2),
            Cursor = Cursors.Default
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));

        Label name = CreateLabel(channel, 10F, FontStyle.Bold, _palette.Text);
        name.BackColor = _palette.Row;

        Label state = CreateLabel("INATIVA", 7.5F, FontStyle.Bold, _palette.MutedText, ContentAlignment.MiddleRight);
        state.BackColor = _palette.Row;
        row.Controls.Add(name, 0, 0);
        row.Controls.Add(state, 1, 0);
        row.Controls.Add(CreateAssetPicture("push_button_green.png", 42), 2, 0);
        return row;
    }

    private Control CreateInputs()
    {
        TableLayoutPanel inputs = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = _palette.Surface,
            Padding = new Padding(12, 0, 0, 0)
        };
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        for (int row = 1; row < 5; row++)
        {
            inputs.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        }

        Control header = CreateColumnHeader("ENTRADAS DIGITAIS", "DI  /  sem leitura");
        inputs.Controls.Add(header, 0, 0);
        inputs.SetColumnSpan(header, 2);
        for (int channel = 0; channel < 8; channel++)
        {
            int column = channel / 4;
            int row = channel % 4 + 1;
            inputs.Controls.Add(CreateInputCell($"DI{channel:00}", channel), column, row);
        }
        return inputs;
    }

    private Control CreateInputCell(string channel, int channelIndex)
    {
        TableLayoutPanel cell = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Row,
            Margin = new Padding(4),
            Padding = new Padding(8, 2, 6, 2)
        };
        cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        cell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42F));
        Label name = CreateLabel(channel, 9F, FontStyle.Bold, _palette.Text);
        name.BackColor = _palette.Row;
        PictureBox indicator = CreateAssetPicture("led_off_gray.png", 36);
        _inputIndicators[channelIndex] = indicator;
        cell.Controls.Add(name, 0, 0);
        cell.Controls.Add(indicator, 1, 0);
        return cell;
    }

    public void SetInputState(int channel, bool active)
    {
        if (channel < 0 || channel >= _inputIndicators.Length)
        {
            return;
        }

        PictureBox indicator = _inputIndicators[channel];
        string fileName = active ? "led_on_green.png" : "led_off_gray.png";
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", fileName);
        if (!File.Exists(path))
        {
            return;
        }

        Image replacement = LoadImage(path);
        Image? previous = indicator.Image;
        indicator.Image = replacement;
        _ownedImages.Add(replacement);
        if (previous is not null)
        {
            _ownedImages.Remove(previous);
            previous.Dispose();
        }
    }

    private Control CreateColumnHeader(string title, string subtitle)
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Surface
        };
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        header.Controls.Add(CreateLabel(title, 8.5F, FontStyle.Bold, _palette.Text), 0, 0);
        header.Controls.Add(CreateLabel(subtitle, 7.5F, FontStyle.Regular, _palette.MutedText), 0, 1);
        return header;
    }

    private PictureBox CreateAssetPicture(string fileName, int size)
    {
        PictureBox picture = new()
        {
            Size = new Size(size, size),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = _palette.Row,
            Margin = new Padding(4),
            Cursor = Cursors.Default,
            TabStop = false,
            Anchor = AnchorStyles.None
        };

        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", fileName);
        if (File.Exists(path))
        {
            Image image = LoadImage(path);
            _ownedImages.Add(image);
            picture.Image = image;
        }
        return picture;
    }

    private static Bitmap LoadImage(string path)
    {
        using Image source = Image.FromFile(path);
        return new Bitmap(source);
    }

    private Label CreateLabel(
        string text,
        float size,
        FontStyle style,
        Color color,
        ContentAlignment alignment = ContentAlignment.MiddleLeft)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            BackColor = _palette.Surface,
            TextAlign = alignment,
            AutoEllipsis = true
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (Image image in _ownedImages)
            {
                image.Dispose();
            }
            _ownedImages.Clear();
        }
        base.Dispose(disposing);
    }
}
