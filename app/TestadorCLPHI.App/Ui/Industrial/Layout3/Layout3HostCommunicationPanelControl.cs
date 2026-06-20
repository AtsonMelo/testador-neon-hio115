namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Painel estritamente visual do estado local de comunicacao. Recebe estados
/// prontos e nao conhece nem instancia qualquer canal ou servico de hardware.
/// </summary>
internal sealed class Layout3HostCommunicationPanelControl : UserControl
{
    private readonly Layout3ThemePalette _palette;
    private readonly Label _statusValue;
    private readonly Label _originValue;
    private readonly Label _protocolValue;
    private readonly Label _messageValue;
    private readonly Label _updatedValue;
    private readonly Label _countersValue;

    public event Action? BlockedIntentRequested;

    public Layout3HostCommunicationPanelControl(
        Layout3ThemePalette palette,
        Layout3CommunicationState state)
    {
        _palette = palette;
        BackColor = _palette.Surface;
        BorderStyle = BorderStyle.FixedSingle;

        _statusValue = CreateValueLabel();
        _originValue = CreateValueLabel();
        _protocolValue = CreateValueLabel();
        _protocolValue.AutoEllipsis = false;
        _messageValue = CreateValueLabel();
        _updatedValue = CreateValueLabel();
        _countersValue = CreateValueLabel(ContentAlignment.MiddleCenter);

        BuildLayout();
        UpdateState(state);
    }

    public void UpdateState(Layout3CommunicationState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _statusValue.Text = state.StatusDisplayName.ToUpperInvariant();
        _statusValue.ForeColor = state.Status == Layout3CommunicationStatus.Blocked
            ? _palette.Warning
            : _palette.AccentBlue;
        _originValue.Text = state.Origin.ToUpperInvariant();
        _protocolValue.Text = state.ProfileProtocol.ToUpperInvariant();
        _messageValue.Text = state.OperationalMessage;
        _updatedValue.Text = $"{state.LastUpdatedDisplay} | LOCAL";
        _countersValue.Text =
            $"TENTATIVAS REAIS  {state.RealConnectionAttempts}   |   " +
            $"COMANDOS FISICOS  {state.PhysicalCommandsExecuted}";
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9,
            BackColor = _palette.Surface,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(CreateField("ESTADO GERAL", _statusValue), 0, 1);
        root.Controls.Add(CreateField("ORIGEM", _originValue), 0, 2);
        root.Controls.Add(CreateField("PROTOCOLO DO PERFIL (CATALOGO)", _protocolValue), 0, 3);
        root.Controls.Add(CreateOperationalArea(), 0, 4);

        _countersValue.BackColor = _palette.WarningBackground;
        _countersValue.ForeColor = _palette.Warning;
        _countersValue.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        _countersValue.Margin = new Padding(0, 3, 0, 3);
        root.Controls.Add(_countersValue, 0, 6);

        Button intentButton = CreateLocalButton("REGISTRAR INTENCAO BLOQUEADA");
        intentButton.Click += (_, _) => BlockedIntentRequested?.Invoke();
        root.Controls.Add(intentButton, 0, 7);

        Label guard = CreateLabel(
            "READ-ONLY | SEM CONEXAO ABERTA",
            7.5F,
            FontStyle.Bold,
            _palette.Warning,
            ContentAlignment.MiddleCenter);
        guard.BackColor = _palette.WarningBackground;
        guard.Margin = new Padding(0, 3, 0, 0);
        root.Controls.Add(guard, 0, 8);

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

        Label index = CreateLabel("01", 8F, FontStyle.Bold, _palette.AccentBlue, ContentAlignment.MiddleCenter);
        index.BackColor = _palette.Field;
        index.Margin = new Padding(0, 4, 7, 4);
        header.Controls.Add(index, 0, 0);
        header.SetRowSpan(index, 2);
        header.Controls.Add(CreateLabel("HOST / COMUNICACAO", 9F, FontStyle.Bold, _palette.Text), 1, 0);
        header.Controls.Add(CreateLabel("Diagnostico local read-only", 7.5F, FontStyle.Regular, _palette.MutedText), 1, 1);
        return header;
    }

    private Control CreateField(string title, Label value)
    {
        TableLayoutPanel field = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Field,
            Margin = new Padding(0, 3, 0, 3),
            Padding = new Padding(9, 2, 9, 2)
        };
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));

        Label titleLabel = CreateLabel(title, 7F, FontStyle.Bold, _palette.MutedText);
        titleLabel.BackColor = _palette.Field;
        value.BackColor = _palette.Field;
        field.Controls.Add(titleLabel, 0, 0);
        field.Controls.Add(value, 0, 1);
        return field;
    }

    private Control CreateOperationalArea()
    {
        TableLayoutPanel area = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = _palette.Raised,
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(9, 3, 9, 3)
        };
        area.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        area.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        area.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));

        Label title = CreateLabel("MENSAGEM OPERACIONAL", 7F, FontStyle.Bold, _palette.MutedText);
        title.BackColor = _palette.Raised;
        _messageValue.BackColor = _palette.Raised;
        _messageValue.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
        _messageValue.AutoEllipsis = false;
        _updatedValue.BackColor = _palette.Raised;
        _updatedValue.ForeColor = _palette.MutedText;
        _updatedValue.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
        area.Controls.Add(title, 0, 0);
        area.Controls.Add(_messageValue, 0, 1);
        area.Controls.Add(_updatedValue, 0, 2);
        return area;
    }

    private Label CreateValueLabel(ContentAlignment alignment = ContentAlignment.MiddleLeft)
    {
        return CreateLabel(string.Empty, 8.2F, FontStyle.Bold, _palette.Text, alignment);
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

    private Button CreateLocalButton(string text)
    {
        Button button = new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = _palette.Text,
            BackColor = _palette.Raised,
            Font = new Font("Segoe UI", 7.3F, FontStyle.Bold),
            Margin = new Padding(0, 3, 0, 3),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = _palette.Divider;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = _palette.ButtonHover;
        return button;
    }
}
