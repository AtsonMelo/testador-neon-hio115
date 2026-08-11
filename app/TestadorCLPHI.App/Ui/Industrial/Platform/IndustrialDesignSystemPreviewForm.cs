using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

// QA-only surface used by offline validators and visual proof capture.
// It is intentionally not registered in Program startup routing.
internal sealed class IndustrialDesignSystemPreviewForm : Form
{
    internal IndustrialDesignSystemPreviewForm()
    {
        Name = "industrialDesignSystemPreview";
        Text = "Laboratório visual industrial";
        ClientSize = new Size(980, 680);
        MinimumSize = Size.Empty;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        BackColor = IndustrialTheme.Palette.Background;
        ForeColor = IndustrialTheme.Palette.TextPrimary;
        Font = IndustrialTypography.Body();
        Controls.Add(BuildContent());
    }

    private static Control BuildContent()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(IndustrialSpacing.Xl),
            ColumnCount = 2,
            RowCount = 4,
            BackColor = IndustrialTheme.Palette.Background,
            Name = "designSystemPreviewRoot"
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 216F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Label title = PlatformUi.PageTitle("Sistema visual industrial", "Laboratório do sistema visual");
        title.Name = "designSystemPreviewTitle";
        title.Margin = new Padding(IndustrialSpacing.Sm);
        root.Controls.Add(title, 0, 0);
        root.SetColumnSpan(title, 2);
        root.Controls.Add(BuildButtons(), 0, 1);
        root.Controls.Add(BuildFields(), 1, 1);
        root.Controls.Add(BuildStatuses(), 0, 2);
        root.Controls.Add(BuildNavigation(), 1, 2);
        root.Controls.Add(BuildTabs(), 0, 3);
        root.Controls.Add(BuildSection(), 1, 3);
        return root;
    }

    private static Control BuildButtons()
    {
        FlowLayoutPanel flow = CreateFlow("previewButtons");
        Button primary = PlatformUi.Button("Ação primária", "previewPrimaryButton", primary: true);
        Button secondary = PlatformUi.Button("Ação secundária", "previewSecondaryButton");
        Button ghost = PlatformUi.Button("Ação discreta", "previewGhostButton");
        Button danger = PlatformUi.Button("Cancelar", "previewDangerButton");
        Button disabled = PlatformUi.Button("Indisponível", "previewDisabledButton");
        PlatformUi.SetButtonTone(ghost, PlatformButtonTone.Ghost);
        PlatformUi.SetButtonTone(danger, PlatformButtonTone.Danger);
        disabled.Enabled = false;
        foreach (Button button in new[] { primary, secondary, ghost, danger, disabled })
        {
            button.Width = 176;
            button.Margin = new Padding(IndustrialSpacing.Xs);
            flow.Controls.Add(button);
        }

        return WrapSection("Botões", flow);
    }

    private static Control BuildFields()
    {
        FlowLayoutPanel flow = CreateFlow("previewFields");
        IndustrialComboBox combo = new() { Name = "previewCombo", Width = 176 };
        combo.Items.AddRange(["Perfil padrão", "Perfil alternativo"]);
        combo.SelectedIndex = 0;
        combo.ApplyTheme();
        TextBox text = new() { Name = "previewText", Text = "Valor técnico", Width = 176 };
        NumericUpDown number = new() { Name = "previewNumber", Value = 10, Width = 176 };
        PlatformUi.StyleField(text);
        PlatformUi.StyleField(number);
        foreach (Control field in new Control[] { combo, text, number })
        {
            field.Margin = new Padding(IndustrialSpacing.Xs);
            flow.Controls.Add(field);
        }

        return WrapSection("Campos", flow);
    }

    private static Control BuildStatuses()
    {
        FlowLayoutPanel flow = CreateFlow("previewStatuses");
        foreach ((string text, PlatformStatusTone tone) in new[]
                 {
                     ("Operação segura", PlatformStatusTone.Normal),
                     ("Atenção", PlatformStatusTone.Attention),
                     ("Falha", PlatformStatusTone.Fault),
                     ("Offline", PlatformStatusTone.Offline),
                     ("Read-only", PlatformStatusTone.Disabled)
                 })
        {
            flow.Controls.Add(PlatformUi.StatusChip(text, tone));
        }

        return WrapSection("Estados", flow);
    }

    private static Control BuildNavigation()
    {
        FlowLayoutPanel flow = CreateFlow("previewNavigation");
        foreach ((string text, bool selected) in new[]
                 {
                     ("Visão geral", false),
                     ("Testador", true),
                     ("Simulador", false)
                 })
        {
            IndustrialButton button = (IndustrialButton)PlatformUi.Button(text, $"preview{text.Replace(" ", string.Empty)}Navigation");
            button.Width = 138;
            button.IsNavigation = true;
            PlatformUi.SetButtonTone(button, PlatformButtonTone.Navigation);
            PlatformUi.StyleButton(button, selected: selected);
            flow.Controls.Add(button);
        }

        return WrapSection("Navegação", flow);
    }

    private static Control BuildTabs()
    {
        IndustrialTabControl tabs = new()
        {
            Dock = DockStyle.Fill,
            Name = "previewTabs",
            Margin = new Padding(IndustrialSpacing.Xs)
        };
        tabs.TabPages.Add("Entradas", "Entradas");
        tabs.TabPages.Add("Saídas", "Saídas");
        tabs.TabPages.Add("Diagnóstico", "Diagnóstico");
        tabs.ApplyTheme();
        return WrapSection("Abas", tabs);
    }

    private static Control BuildSection()
    {
        GroupBox group = PlatformUi.Group("Seção técnica flat", flatSection: true);
        group.Dock = DockStyle.Fill;
        FlowLayoutPanel rows = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = IndustrialTheme.Palette.SurfaceElevated
        };
        foreach ((string text, bool isChecked, string name) in new[]
                 {
                     ("Sinal normal", false, "previewSignalNormal"),
                     ("Sinal ativo", true, "previewSignalChecked"),
                     ("Sinal com foco", false, "previewSignalFocus")
                 })
        {
            CheckBox signal = new()
            {
                Name = name,
                Text = text,
                Checked = isChecked,
                Width = 360,
                Height = IndustrialSpacing.InteractiveHeight,
                Margin = Padding.Empty,
                AccessibleName = text,
                BackColor = IndustrialTheme.Palette.SurfaceElevated,
                ForeColor = IndustrialTheme.Palette.TextPrimary
            };
            rows.Controls.Add(signal);
        }

        group.Controls.Add(rows);
        return group;
    }

    private static FlowLayoutPanel CreateFlow(string name) => new()
    {
        Name = name,
        Dock = DockStyle.Fill,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        WrapContents = true,
        BackColor = IndustrialTheme.Palette.SurfaceElevated,
        Padding = new Padding(IndustrialSpacing.Xs)
    };

    private static Control WrapSection(string title, Control content)
    {
        GroupBox group = PlatformUi.Group(title);
        group.Dock = DockStyle.Fill;
        group.Margin = new Padding(IndustrialSpacing.Sm);
        content.Dock = DockStyle.Fill;
        group.Controls.Add(content);
        return group;
    }
}
