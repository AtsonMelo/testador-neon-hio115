using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Simulation;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialIoMapControl : UserControl
{
    private readonly IndustrialPlatformSession _session;
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

    internal IndustrialIoMapControl(IndustrialPlatformSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Name = "industrialIoMapControl";
        AccessibleName = "Mapa de I/O do perfil de simulação";
        BackColor = PlatformUi.Background;
        ForeColor = PlatformUi.Text;
        Dock = DockStyle.Fill;
        Controls.Add(BuildLayout());
        ConfigureGrid();
        LoadBindings();
    }

    internal int BindingRowCount => _grid.Rows.Count;
    internal bool DeclaresSimulationEvidence => _grid.Rows.Cast<DataGridViewRow>().All(row =>
        string.Equals(row.Cells[4].Value?.ToString(), "PERFIL DE SIMULAÇÃO", StringComparison.Ordinal));

    private Control BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = PlatformUi.Background,
            Padding = new Padding(8)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Panel banner = new()
        {
            Dock = DockStyle.Fill,
            BackColor = PlatformUi.Surface,
            Padding = new Padding(14, 9, 14, 8),
            Margin = new Padding(0, 0, 0, 8)
        };
        Label title = PlatformUi.Label("MAPEAMENTO DO PERFIL DE SIMULAÇÃO", heading: true);
        title.Name = "ioMappingEvidenceLabel";
        title.Dock = DockStyle.Top;
        title.ForeColor = PlatformUi.Warning;
        Label detail = PlatformUi.Label(
            $"{_session.Profile.DisplayName} • vínculo conceitual em memória • não confirmado como ligação física");
        detail.Dock = DockStyle.Bottom;
        banner.Controls.Add(detail);
        banner.Controls.Add(title);

        root.Controls.Add(banner, 0, 0);
        root.Controls.Add(_grid, 0, 1);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.AccessibleName = "Tabela de mapeamento entre processo e canais do CLP simulado";
        _grid.DefaultCellStyle.BackColor = PlatformUi.Field;
        _grid.DefaultCellStyle.ForeColor = PlatformUi.Text;
        _grid.DefaultCellStyle.SelectionBackColor = PlatformUi.Accent;
        _grid.DefaultCellStyle.SelectionForeColor = PlatformUi.Text;
        _grid.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        _grid.ColumnHeadersDefaultCellStyle.BackColor = PlatformUi.Header;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = PlatformUi.Text;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
        _grid.GridColor = PlatformUi.ButtonSurface;
        _grid.RowTemplate.Height = 42;

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
            _grid.Rows.Add(
                signal?.Label ?? binding.SignalLabel ?? binding.SignalId,
                binding.RegisterAlias,
                binding.Register,
                FormatType(binding),
                "PERFIL DE SIMULAÇÃO",
                binding.Description);
        }

        _grid.ClearSelection();
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
            (SimulationIoDirection.Input, SimulationIoType.Digital) => "Entrada digital",
            (SimulationIoDirection.Input, SimulationIoType.Analog) => "Entrada analógica",
            (SimulationIoDirection.Output, SimulationIoType.Digital) => "Saída digital",
            _ => "Não suportado"
        };
}
