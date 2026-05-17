using System.IO;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialLedIndicatorControl : UserControl
{
    private readonly PictureBox _pictureBox;
    private readonly Label _label;
    private Image? _onImage;
    private Image? _offImage;
    private bool _isOn;

    public IndustrialLedIndicatorControl()
    {
        Width = 64;
        Height = 72;
        BackColor = Color.Transparent;
        Margin = new Padding(0, 0, 14, 0);

        _pictureBox = new PictureBox
        {
            Width = 48,
            Height = 48,
            Left = 8,
            Top = 0,
            BackColor = Color.Transparent,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        _label = new Label
        {
            Width = 64,
            Height = 18,
            Left = 0,
            Top = 50,
            Text = "DI",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(203, 213, 225),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            BackColor = Color.Transparent
        };

        Controls.Add(_pictureBox);
        Controls.Add(_label);
    }

    public string LabelText
    {
        get => _label.Text;
        set => _label.Text = string.IsNullOrWhiteSpace(value) ? "DI" : value.Trim();
    }

    public bool IsOn
    {
        get => _isOn;
        set
        {
            _isOn = value;
            UpdateImage();
        }
    }

    public void LoadImages(string onImagePath, string offImagePath)
    {
        _onImage?.Dispose();
        _offImage?.Dispose();

        _onImage = File.Exists(onImagePath) ? new Bitmap(onImagePath) : null;
        _offImage = File.Exists(offImagePath) ? new Bitmap(offImagePath) : null;

        UpdateImage();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pictureBox.Image = null;
            _onImage?.Dispose();
            _offImage?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void UpdateImage()
    {
        _pictureBox.Image = _isOn ? _onImage : _offImage;
    }
}
