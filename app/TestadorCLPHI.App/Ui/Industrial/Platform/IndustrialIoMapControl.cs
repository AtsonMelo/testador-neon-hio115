using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialIoMapControl : UserControl
{
    private readonly IndustrialPlatformSession _session;
    private readonly TextBox _search = new()
    {
        Name = "ioMappingSearch",
        PlaceholderText = "Buscar processo, canal ou registro",
        AccessibleName = "Buscar no mapa de I/O",
        Dock = DockStyle.Fill
    };
    private readonly IndustrialComboBox _typeFilter = new()
    {
        Name = "ioMappingTypeFilter",
        DropDownStyle = ComboBoxStyle.DropDownList,
        AccessibleName = "Filtrar tipo de I/O",
        Dock = DockStyle.Fill
    };
    private readonly Label _evidenceTitle = PlatformUi.Label(
        "MAPEAMENTO DO PERFIL DE SIMULAÇÃO",
        heading: true);
    private readonly DataGridView _grid = new()
    {
        Name = "ioMappingGrid",
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        MultiSelect = false,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = PlatformUi.Field,
        BorderStyle = BorderStyle.None,
        EnableHeadersVisualStyles = false,
        ColumnHeadersHeight = 38,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
    };
    private readonly List<IoMapRow> _rows = [];

    internal IndustrialIoMapControl(IndustrialPlatformSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Name = "industrialIoMapControl";
        AccessibleName = "Mapa de I/O do perfil de simulação";
        BackColor = PlatformUi.Background;
        ForeColor = PlatformUi.Text;
        Dock = DockStyle.Fill;
        Controls.Add(BuildLayout());
        _typeFilter.AccessibleDescription = "Restringe a tabela a DI, AI ou DO existentes no perfil";
        ConfigureGrid();
        LoadBindings();
        _search.TextChanged += (_, _) => ApplyFilter();
        _typeFilter.SelectedIndexChanged += (_, _) => ApplyFilter();
        ApplyTheme();
    }

    internal int BindingRowCount => _grid.Rows.Count;
    internal int TotalBindingCount => _rows.Count;
    internal IReadOnlyList<string> AvailableTypeFilters =>
        _typeFilter.Items.Cast<string>().ToArray();
    internal bool HasUnsupportedAnalogOutputBadge => _rows.Any(row =>
        row.Type.StartsWith("AO", StringComparison.Ordinal));
    internal bool DeclaresSimulationEvidence => _grid.Rows.Cast<DataGridViewRow>().All(row =>
        string.Equals(row.Cells[4].Value?.ToString(), "PERFIL DE SIMULAÇÃO", StringComparison.Ordinal));

    internal void Filter(string search, string? type)
    {
        _search.Text = search;
        if (!string.IsNullOrWhiteSpace(type) && _typeFilter.Items.Contains(type))
        {
            _typeFilter.SelectedItem = type;
        }
        else
        {
            _typeFilter.SelectedIndex = 0;
        }

        ApplyFilter();
    }

    private Control BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = PlatformUi.Background,
            Padding = new Padding(IndustrialSpacing.Sm)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        TableLayoutPanel banner = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = PlatformUi.Surface,
            Padding = new Padding(
                IndustrialSpacing.Md,
                IndustrialSpacing.Xs,
                IndustrialSpacing.Md,
                IndustrialSpacing.Xs),
            Margin = new Padding(0, 0, 0, IndustrialSpacing.Xs),
            Name = "ioMappingToolbar"
        };
        banner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        banner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        banner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        TableLayoutPanel identity = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
        _evidenceTitle.Name = "ioMappingEvidenceLabel";
        _evidenceTitle.Dock = DockStyle.Fill;
        _evidenceTitle.ForeColor = PlatformUi.Warning;
        _evidenceTitle.AccessibleName = "Evidência e quantidade do mapa de I/O";
        Label detail = PlatformUi.Label(
            $"{_session.Profile.DisplayName} • vínculo conceitual em memória • não confirmado como ligação física");
        detail.Dock = DockStyle.Fill;
        detail.AutoEllipsis = true;
        identity.Controls.Add(_evidenceTitle, 0, 0);
        identity.Controls.Add(detail, 0, 1);
        banner.Controls.Add(identity, 0, 0);
        banner.Controls.Add(_search, 1, 0);
        banner.Controls.Add(_typeFilter, 2, 0);

        root.Controls.Add(banner, 0, 0);
        root.Controls.Add(_grid, 0, 1);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.AccessibleName = "Tabela de mapeamento entre processo e canais do CLP simulado";
        _grid.AccessibleDescription =
            "Mapa somente leitura do perfil de simulação; não representa ligação física confirmada";
        _grid.DefaultCellStyle.Padding = new Padding(
            IndustrialSpacing.Sm,
            IndustrialSpacing.Xs,
            IndustrialSpacing.Sm,
            IndustrialSpacing.Xs);
        _grid.DefaultCellStyle.Font = IndustrialTypography.Body();
        _grid.ColumnHeadersDefaultCellStyle.Font = IndustrialTypography.BodyStrong();
        _grid.RowTemplate.Height = 34;
        _grid.ShowCellToolTips = true;
        _grid.CellToolTipTextNeeded += (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                e.ToolTipText = _grid[e.ColumnIndex, e.RowIndex].Value?.ToString() ?? string.Empty;
            }
        };

        AddColumn("Processo", 24F);
        AddColumn("Canal CLP", 12F);
        AddColumn("Registro", 11F);
        AddColumn("Tipo", 18F);
        AddColumn("Evidência", 18F);
        AddColumn("Descrição", 30F);
    }

    private void AddColumn(string text, float fillWeight)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = text,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            FillWeight = fillWeight,
            MinimumWidth = 90
        });
    }

    private void LoadBindings()
    {
        foreach (SimulationIoBinding binding in _session.Profile.IoBindings.OrderBy(BindingOrder))
        {
            SimulationSignalDefinition? signal = _session.FindSignal(binding.SignalId);
            _rows.Add(new IoMapRow(
                signal?.Label ?? binding.SignalLabel ?? binding.SignalId ?? string.Empty,
                binding.RegisterAlias ?? string.Empty,
                binding.Register,
                FormatType(binding),
                "PERFIL DE SIMULAÇÃO",
                binding.Description ?? string.Empty));
        }

        _typeFilter.Items.Add("TODOS OS TIPOS");
        foreach (string type in _rows.Select(row => row.Type.Split('•')[0].Trim()).Distinct(StringComparer.Ordinal))
        {
            _typeFilter.Items.Add(type);
        }

        _typeFilter.SelectedIndex = 0;
        ApplyFilter();
    }

    private static int BindingOrder(SimulationIoBinding binding) =>
        binding.Direction switch
        {
            SimulationIoDirection.Input when binding.IoType == SimulationIoType.Digital => binding.Channel,
            SimulationIoDirection.Input when binding.IoType == SimulationIoType.Analog => 100 + binding.Channel,
            SimulationIoDirection.Output => 200 + binding.Channel,
            _ => 1000
        };

    private static string FormatType(SimulationIoBinding binding) =>
        (binding.Direction, binding.IoType) switch
        {
            (SimulationIoDirection.Input, SimulationIoType.Digital) => "DI • Entrada digital",
            (SimulationIoDirection.Input, SimulationIoType.Analog) => "AI • Entrada analógica",
            (SimulationIoDirection.Output, SimulationIoType.Digital) => "DO • Saída digital",
            _ => "Não suportado"
        };

    internal void ApplyTheme()
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        BackColor = palette.Background;
        ForeColor = palette.TextPrimary;
        _search.BackColor = palette.Field;
        _search.ForeColor = palette.TextPrimary;
        _typeFilter.BackColor = palette.Field;
        _typeFilter.ForeColor = palette.TextPrimary;
        _typeFilter.ApplyTheme();
        _grid.BackgroundColor = palette.Field;
        _grid.GridColor = palette.Border;
        _grid.DefaultCellStyle.BackColor = palette.Field;
        _grid.DefaultCellStyle.ForeColor = palette.TextPrimary;
        _grid.DefaultCellStyle.SelectionBackColor = palette.SelectedSurface;
        _grid.DefaultCellStyle.SelectionForeColor = palette.SelectedText;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = palette.Surface;
        _grid.AlternatingRowsDefaultCellStyle.ForeColor = palette.TextPrimary;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = palette.SurfaceElevated;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = palette.TextPrimary;
        foreach (Control child in Controls)
        {
            ApplyThemeToTree(child, palette);
        }
    }

    private void ApplyFilter()
    {
        string query = _search.Text.Trim();
        string selectedType = _typeFilter.SelectedItem?.ToString() ?? "TODOS OS TIPOS";
        IEnumerable<IoMapRow> filtered = _rows.Where(row =>
            (selectedType == "TODOS OS TIPOS"
             || row.Type.StartsWith(selectedType + " ", StringComparison.Ordinal))
            && (query.Length == 0 || row.SearchableText.Contains(query, StringComparison.OrdinalIgnoreCase)));

        _grid.SuspendLayout();
        _grid.Rows.Clear();
        foreach (IoMapRow row in filtered)
        {
            _grid.Rows.Add(
                row.Process,
                row.Channel,
                row.Register,
                row.Type,
                row.Evidence,
                row.Description);
        }

        _grid.ClearSelection();
        _grid.ResumeLayout();
        _evidenceTitle.Text = $"MAPEAMENTO DO PERFIL DE SIMULAÇÃO • {_grid.Rows.Count}/{_rows.Count} SINAIS";
        _grid.AccessibleDescription =
            $"{_grid.Rows.Count} de {_rows.Count} sinais visíveis; somente leitura; ligação física não confirmada";
    }

    private static void ApplyThemeToTree(Control control, IndustrialPalette palette)
    {
        if (control is TableLayoutPanel panel)
        {
            panel.BackColor = panel.Name == "ioMappingToolbar"
                ? palette.SurfaceElevated
                : palette.Background;
        }

        if (control is Label label
            && label.Tag is not PlatformStatusTone
            && label.BorderStyle != BorderStyle.FixedSingle)
        {
            label.ForeColor = label.Name == "ioMappingEvidenceLabel"
                ? palette.Warning
                : palette.TextSecondary;
        }

        foreach (Control child in control.Controls)
        {
            ApplyThemeToTree(child, palette);
        }
    }

    private sealed record IoMapRow(
        string Process,
        string Channel,
        int Register,
        string Type,
        string Evidence,
        string Description)
    {
        internal string SearchableText =>
            $"{Process} {Channel} {Register} {Type} {Evidence} {Description}";
    }
}
