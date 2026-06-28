namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Painel estritamente visual do bridge de leitura. Recebe um snapshot pronto e
/// nao conhece nem instancia qualquer canal ou servico de hardware. Exibe, de
/// forma compacta, que o bridge esta preparado mas desligado/bloqueado nesta fase.
/// </summary>
internal sealed class Layout3ReadBridgePanelControl : UserControl
{
    private readonly Layout3ThemePalette _palette;
    private readonly Label _statusValue;
    private readonly Label _modeValue;
    private readonly Label _originValue;
    private readonly Label _connectionValue;
    private readonly Label _readsValue;
    private readonly Label _writesValue;
    private readonly Label _commandsValue;
    private readonly Label _profileValue;
    private readonly Label _protocolValue;
    private readonly Label _messageValue;

    public Layout3ReadBridgePanelControl(
        Layout3ThemePalette palette,
        Layout3ReadBridgeSnapshot snapshot)
    {
        _palette = palette;
        BackColor = _palette.Surface;
        BorderStyle = BorderStyle.FixedSingle;

        _statusValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _modeValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _originValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _connectionValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _readsValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _writesValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _commandsValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _profileValue = CreateValueLabel();
        _protocolValue = CreateValueLabel();
        _messageValue = CreateValueLabel();
        _messageValue.AutoEllipsis = false;

        BuildLayout();
        UpdateSnapshot(snapshot);
    }

    public void UpdateSnapshot(Layout3ReadBridgeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _statusValue.Text = snapshot.StatusDisplayName.ToUpperInvariant();
        _modeValue.Text = snapshot.Mode.ToUpperInvariant();
        _originValue.Text = snapshot.Origin.ToUpperInvariant();
        _connectionValue.Text = snapshot.ConnectionActiveDisplay.ToUpperInvariant();
        _readsValue.Text = snapshot.RealReads.ToString();
        _writesValue.Text = snapshot.RealWrites.ToString();
        _commandsValue.Text = snapshot.PhysicalCommands.ToString();
        _profileValue.Text = snapshot.ProfileContext.ToUpperInvariant();
        _protocolValue.Text = snapshot.ProtocolContext.ToUpperInvariant();
        _messageValue.Text = snapshot.Message;
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = _palette.Surface,
            Padding = new Padding(12, 6, 12, 6)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(CreateChips(), 0, 1);
        root.Controls.Add(CreateContext(), 0, 2);
        root.Controls.Add(CreateMessageArea(), 0, 3);

        Controls.Add(root);
    }

    private Control CreateHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Surface
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));

        Label title = CreateLabel(
            "04  BRIDGE DE LEITURA  -  PREPARADO / DESLIGADO",
            9F,
            FontStyle.Bold,
            _palette.Text);
        header.Controls.Add(title, 0, 0);

        Label guard = CreateLabel(
            "CONTRATO/ESTRUTURA | SEM LEITURA REAL",
            7.5F,
            FontStyle.Bold,
            _palette.Warning,
            ContentAlignment.MiddleCenter);
        guard.BackColor = _palette.WarningBackground;
        guard.Margin = new Padding(6, 1, 0, 1);
        header.Controls.Add(guard, 1, 0);
        return header;
    }

    private Control CreateChips()
    {
        TableLayoutPanel chips = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 7,
            RowCount = 1,
            BackColor = _palette.Surface,
            Margin = new Padding(0, 2, 0, 2)
        };
        chips.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        for (int i = 0; i < 7; i++)
        {
            chips.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 7F));
        }

        chips.Controls.Add(CreateChip("ESTADO", _statusValue), 0, 0);
        chips.Controls.Add(CreateChip("MODO", _modeValue), 1, 0);
        chips.Controls.Add(CreateChip("ORIGEM", _originValue), 2, 0);
        chips.Controls.Add(CreateChip("CONEXAO ATIVA", _connectionValue), 3, 0);
        chips.Controls.Add(CreateCounterChip("LEITURAS REAIS", _readsValue), 4, 0);
        chips.Controls.Add(CreateCounterChip("ESCRITAS REAIS", _writesValue), 5, 0);
        chips.Controls.Add(CreateCounterChip("COMANDOS FISICOS", _commandsValue), 6, 0);
        return chips;
    }

    private Control CreateChip(string title, Label value)
    {
        TableLayoutPanel chip = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Field,
            Margin = new Padding(2, 0, 2, 0),
            Padding = new Padding(4, 2, 4, 2)
        };
        chip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        chip.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        chip.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));

        Label titleLabel = CreateLabel(title, 6.5F, FontStyle.Bold, _palette.MutedText, ContentAlignment.MiddleCenter);
        titleLabel.BackColor = _palette.Field;
        value.BackColor = _palette.Field;
        value.Font = new Font("Segoe UI", 7.8F, FontStyle.Bold);
        value.ForeColor = _palette.AccentBlue;
        chip.Controls.Add(titleLabel, 0, 0);
        chip.Controls.Add(value, 0, 1);
        return chip;
    }

    private Control CreateCounterChip(string title, Label value)
    {
        Control chip = CreateChip(title, value);
        chip.BackColor = _palette.WarningBackground;
        value.BackColor = _palette.WarningBackground;
        value.ForeColor = _palette.Warning;
        value.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        foreach (Control inner in chip.Controls)
        {
            if (inner != value)
            {
                inner.BackColor = _palette.WarningBackground;
            }
        }
        return chip;
    }

    private Control CreateContext()
    {
        TableLayoutPanel context = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = _palette.Surface,
            Margin = new Padding(0, 1, 0, 1)
        };
        context.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        context.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        context.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        context.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

        _profileValue.Font = new Font("Segoe UI", 7.8F, FontStyle.Bold);
        _protocolValue.Font = new Font("Segoe UI", 7.8F, FontStyle.Bold);

        context.Controls.Add(
            CreateLabel("PERFIL (CONTEXTO):", 7F, FontStyle.Bold, _palette.MutedText), 0, 0);
        context.Controls.Add(_profileValue, 1, 0);
        context.Controls.Add(
            CreateLabel("PROTOCOLO (CATALOGO):", 7F, FontStyle.Bold, _palette.MutedText), 2, 0);
        context.Controls.Add(_protocolValue, 3, 0);
        return context;
    }

    private Control CreateMessageArea()
    {
        TableLayoutPanel area = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Raised,
            Margin = new Padding(0, 2, 0, 0),
            Padding = new Padding(9, 2, 9, 2)
        };
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        Label title = CreateLabel("MENSAGEM DO BRIDGE", 7F, FontStyle.Bold, _palette.MutedText);
        title.BackColor = _palette.Raised;
        _messageValue.BackColor = _palette.Raised;
        _messageValue.ForeColor = _palette.Text;
        _messageValue.Font = new Font("Segoe UI", 8F, FontStyle.Regular);

        Label note = CreateLabel(
            "Contrato/estrutura: sem conexao aberta e sem leitura real. "
            + "Ativacao real exigira milestone futura e aprovacao explicita.",
            7F,
            FontStyle.Italic,
            _palette.MutedText);
        note.AutoEllipsis = false;
        note.BackColor = _palette.Raised;

        TableLayoutPanel stack = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Raised
        };
        stack.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        stack.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        stack.Controls.Add(_messageValue, 0, 0);
        stack.Controls.Add(note, 0, 1);

        area.Controls.Add(title, 0, 0);
        area.Controls.Add(stack, 1, 0);
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
}
