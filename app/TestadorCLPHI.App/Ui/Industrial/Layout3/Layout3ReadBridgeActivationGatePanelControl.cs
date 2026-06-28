namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Painel estritamente visual do gate de ativacao do bridge de leitura. Recebe uma
/// decisao pronta e nao conhece nem instancia qualquer canal ou servico de
/// hardware. Exibe, de forma compacta, que a ativacao real esta bloqueada nesta
/// fase e quantos requisitos futuros seguem pendentes.
/// </summary>
internal sealed class Layout3ReadBridgeActivationGatePanelControl : UserControl
{
    private readonly Layout3ThemePalette _palette;
    private readonly Label _statusValue;
    private readonly Label _activationValue;
    private readonly Label _connectionValue;
    private readonly Label _readsValue;
    private readonly Label _writesValue;
    private readonly Label _commandsValue;
    private readonly Label _requirementsValue;
    private readonly Label _messageValue;

    public Layout3ReadBridgeActivationGatePanelControl(
        Layout3ThemePalette palette,
        Layout3ReadBridgeActivationDecision decision)
    {
        _palette = palette;
        BackColor = _palette.Surface;
        BorderStyle = BorderStyle.FixedSingle;

        _statusValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _activationValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _connectionValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _readsValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _writesValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _commandsValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _requirementsValue = CreateValueLabel(ContentAlignment.MiddleCenter);
        _messageValue = CreateValueLabel();
        _messageValue.AutoEllipsis = false;

        BuildLayout();
        UpdateDecision(decision);
    }

    public void UpdateDecision(Layout3ReadBridgeActivationDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        _statusValue.Text = decision.StatusDisplayName.ToUpperInvariant();
        _activationValue.Text = decision.ActivationAllowedDisplay.ToUpperInvariant();
        _connectionValue.Text = decision.ConnectionActiveDisplay.ToUpperInvariant();
        _readsValue.Text = decision.RealReads.ToString();
        _writesValue.Text = decision.RealWrites.ToString();
        _commandsValue.Text = decision.PhysicalCommands.ToString();
        _requirementsValue.Text =
            $"{decision.PendingRequirements} PENDENTES / {decision.SatisfiedRequirements} ATENDIDOS";
        _messageValue.Text = decision.Message;
    }

    private void BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = _palette.Surface,
            Padding = new Padding(12, 6, 12, 6)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(CreateChips(), 0, 1);
        root.Controls.Add(CreateMessageArea(), 0, 2);

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
            "05  GATE DE ATIVACAO  -  BLOQUEADO",
            9F,
            FontStyle.Bold,
            _palette.Text);
        header.Controls.Add(title, 0, 0);

        Label guard = CreateLabel(
            "ATIVACAO REAL BLOQUEADA | SEM LIBERACAO",
            7.5F,
            FontStyle.Bold,
            _palette.Emergency,
            ContentAlignment.MiddleCenter);
        guard.BackColor = _palette.EmergencyBackground;
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
        chips.Controls.Add(CreateChip("ATIVACAO LIBERADA", _activationValue), 1, 0);
        chips.Controls.Add(CreateChip("CONEXAO ATIVA", _connectionValue), 2, 0);
        chips.Controls.Add(CreateCounterChip("LEITURAS REAIS", _readsValue), 3, 0);
        chips.Controls.Add(CreateCounterChip("ESCRITAS REAIS", _writesValue), 4, 0);
        chips.Controls.Add(CreateCounterChip("COMANDOS FISICOS", _commandsValue), 5, 0);
        chips.Controls.Add(CreateChip("REQUISITOS", _requirementsValue), 6, 0);
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
        value.Font = new Font("Segoe UI", 7.6F, FontStyle.Bold);
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

        Label title = CreateLabel("DECISAO DO GATE", 7F, FontStyle.Bold, _palette.MutedText);
        title.BackColor = _palette.Raised;
        _messageValue.BackColor = _palette.Raised;
        _messageValue.ForeColor = _palette.Text;
        _messageValue.Font = new Font("Segoe UI", 8F, FontStyle.Regular);

        Label note = CreateLabel(
            "Gate/decisao: nenhum perfil libera ativacao. Requisitos futuros seguem pendentes. "
            + "Liberacao real exigira milestone futura e aprovacao explicita.",
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
