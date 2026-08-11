using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TestadorCLPHI.App.Hardware;
using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Safety;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Industrial.Layout3;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal static class IndustrialPlatformUiValidator
{
    internal static int Validate(TextWriter output, TextWriter error)
    {
        CanonicalCounters? canonicalCounters = null;
        List<(string Name, Func<bool> Validate)> scenarios =
        [
            ("startup sem argumentos seleciona a plataforma industrial", StartupWithoutArgumentsSelectsIndustrial),
            ("argumento industrial resolve para a plataforma industrial", IndustrialArgumentSelectsPlatform),
            ("argumento industrial ignora diferenca entre maiusculas e minusculas", IndustrialArgumentIsCaseInsensitive),
            ("argumento legacy resolve exclusivamente para MainForm", LegacyArgumentSelectsMainForm),
            ("argumento legacy ignora diferenca entre maiusculas e minusculas", LegacyArgumentIsCaseInsensitive),
            ("validadores preservam prioridade sobre o launcher industrial", ValidatorsKeepStartupPriority),
            ("argumento desconhecido e rejeitado sem abrir MainForm", UnknownArgumentIsRejected),
            ("rejeicao de argumento desconhecido permanece offline", UnknownArgumentStaysOffline),
            ("launcher industrial cria o host da plataforma", IndustrialLauncherCreatesPlatformHost),
            ("host expoe Home e somente os dois modos operacionais", HostHasHomeAndTwoModes),
            ("navegacao preserva instancias e estado dos modos", NavigationPreservesModeInstances),
            ("shell nao expoe launcher legacy", ShellDoesNotExposeLegacy),
            ("SafetyChain permanece visualmente read-only", SafetyChainIsVisuallyReadOnly),
            ("fundacao UI2 fornece temas dark e light", Ui2ThemeProvidesDarkAndLight),
            ("papeis tipograficos de producao seguem contrato unico", ProductionTypographyUsesSemanticRoles),
            ("texto operacional de producao respeita minimo de 8.5 pontos", OperationalTextUsesMinimumRuntimeSize),
            ("laboratorio visual instancia contratos sem entrar no startup", DesignSystemPreviewIsQaOnly),
            ("tema dark preserva contraste operacional", () => ThemeContrastIsAccessible(IndustrialPalette.Dark)),
            ("tema light preserva contraste operacional", () => ThemeContrastIsAccessible(IndustrialPalette.Light)),
            ("controles industriais expoem estado e acessibilidade", IndustrialControlsExposeAccessibleStates),
            ("checkbox industrial preserva teclado tema foco e acessibilidade", IndustrialCheckBoxPreservesNativeSemantics),
            ("controles industriais nao apresentam crescimento GDI continuo", IndustrialControlsKeepGdiResourcesStable),
            ("host Layout 3 existente carrega Layout3HostControl", Layout3HostLoadsLayout3HostControl),
            ("ciclo de vida do launcher industrial permanece offline", IndustrialLauncherLifecycleStaysOffline),
            ("modo Testador carrega sob demanda", TesterLoadsLazily),
            ("Testador UI2 preserva todas as acoes funcionais", TesterFeatureParityIsPreserved),
            ("modo Simulador carrega sob demanda", SimulatorLoadsLazily),
            ("Simulador UI2 preserva acoes e editores entre cenarios", SimulatorFeatureParityIsPreserved),
            ("Testador exibe aliases e Mapa de I/O do Pivo", PivotAliasesAndIoMapAreVisible),
            ("Mapa de I/O e aliases acompanham o perfil Poco", WellAliasesAndIoMapAreVisible),
            ("Mapa de I/O UI2 filtra sem alterar bindings", IoMapFiltersWithoutChangingBindings),
            ("renderer leve do Pivo carrega quatro torres sem timer", PivotRendererLoadsWithoutTimer),
            ("renderer do Pivo atualiza somente por mudanca de estado", PivotRendererTracksStateChanges),
            ("renderer UI2 do Pivo usa cards e linguagem operacional", PivotRendererUsesUi2VisualLanguage),
            ("perfil Poco preserva editor agrupado sem renderer de Pivo", WellUsesGenericGroupedEditor),
            ("layout estrutural permanece utilizavel em 1366x768", () => LayoutFits(new Size(1366, 768))),
            ("layout estrutural permanece utilizavel em 1600x900", () => LayoutFits(new Size(1600, 900))),
            ("layout estrutural permanece utilizavel em 1920x1080", () => LayoutFits(new Size(1920, 1080))),
            ("layout estrutural permanece utilizavel em 1920x1200", () => LayoutFits(new Size(1920, 1200))),
            ("layout estrutural permanece utilizavel em 2560x1440", () => LayoutFits(new Size(2560, 1440))),
            ("shell responsivo preserva limites em 1366x768 @100%", () => ResponsiveShellFits(new Size(1366, 768), 96)),
            ("shell responsivo preserva limites em 1366x768 @125%", () => ResponsiveShellFits(new Size(1366, 768), 120)),
            ("shell responsivo preserva limites em 1366x768 @150%", () => ResponsiveShellFits(new Size(1366, 768), 144)),
            ("shell responsivo preserva limites em 1600x900 @100%", () => ResponsiveShellFits(new Size(1600, 900), 96)),
            ("shell responsivo preserva limites em 1600x900 @125%", () => ResponsiveShellFits(new Size(1600, 900), 120)),
            ("shell responsivo preserva limites em 1600x900 @150%", () => ResponsiveShellFits(new Size(1600, 900), 144)),
            ("shell responsivo preserva limites em 1920x1080 @100%", () => ResponsiveShellFits(new Size(1920, 1080), 96)),
            ("shell responsivo preserva limites em 1920x1080 @125%", () => ResponsiveShellFits(new Size(1920, 1080), 120)),
            ("shell responsivo preserva limites em 1920x1080 @150%", () => ResponsiveShellFits(new Size(1920, 1080), 144)),
            ("shell responsivo preserva limites em 1920x1200 @100%", () => ResponsiveShellFits(new Size(1920, 1200), 96)),
            ("shell responsivo preserva limites em 2560x1440 @100%", () => ResponsiveShellFits(new Size(2560, 1440), 96)),
            ("shell responsivo preserva limites em 2560x1440 @125%", () => ResponsiveShellFits(new Size(2560, 1440), 120)),
            ("shell responsivo preserva limites em 2560x1440 @150%", () => ResponsiveShellFits(new Size(2560, 1440), 144)),
            ("shell compacto preserva navegacao e seguranca", CompactShellPreservesCriticalUi),
            ("titulo do shell permanece integral em Home Testador e Simulador", ShellTitleFitsAllPagesAndThemes),
            ("contexto e equipamento do header permanecem integrais", ShellSecondaryHeaderTextFits),
            ("titulo do shell possui orcamento vertical seguro em 100 125 e 150%", ShellTitleDpiBudgetIsSafe),
            ("status tema sidebar e footer permanecem contidos", ShellCriticalRegionsRemainContained),
            ("acoes da Home permanecem contidas em modo amplo e compacto", HomeActionsRemainContained),
            ("cards da Home usam altura orientada a conteudo e acoes alinhadas", HomeCardsUseContentDrivenHeight),
            ("status da Home permanece informativo e nao parece botao", HomeStatusIsNotButtonLike),
            ("sidebar separa navegacao de estado fisico nao interativo", SidebarSeparatesNavigationFromPhysicalStatus),
            ("sidebar compacta trata conexao offline como informacao passiva", SidebarUsesCompactPassiveConnectionEvidence),
            ("sidebar usa selecao plana com indicador de acento", NavigationUsesTaskManagerSelectionContract),
            ("Visao Geral usa semantica informativa sem success verde", OverviewUsesInformationalTone),
            ("resumo global usa estados passivos compactos", GlobalSummaryUsesPassiveCompactStatuses),
            ("controles e cabecalhos do Testador permanecem visiveis", TesterControlsRemainContained),
            ("campos RTU do Testador usam chrome industrial tematico", TesterRtuFieldsUseIndustrialChrome),
            ("header RTU usa titulo subtitulo e divisor sem legenda de GroupBox", TesterRtuHeaderUsesFlatSectionComposition),
            ("layout RTU preserva ordem e breakpoints de cinco duas e uma coluna", TesterRtuLayoutIsResponsive),
            ("configuracao RTU usa altura compacta orientada ao conteudo", TesterRtuUsesContentDrivenHeight),
            ("RTU usa alturas medidas e contem todos os filhos visiveis", TesterRtuUsesMeasuredRuntimeHeight),
            ("pilha do Testador nao apresenta intersecoes verticais", TesterRuntimeRegionsDoNotOverlap),
            ("acoes RTU compartilham metrica tipografica e semantica visual", TesterRtuActionsUseOneVisualSpecification),
            ("botoes e superficies principais usam cantos modernos moderados", ProductionActionsAndCardsUseModerateRoundedCorners),
            ("estado desconhecido permanece neutro e nao usa success", UnknownInputStateIsNeutral),
            ("linhas do Testador preservam densidade operacional compacta", TesterSignalRowsUseCompactDensity),
            ("listas do Testador alinham colunas Canal Sinal e Estado", TesterSignalListsExposeAlignedColumns),
            ("tema light preserva profundidade entre superficies", LightThemePreservesSurfaceDepth),
            ("footer permanece compacto e sem marcadores de iteracao UI", FooterHasOnlyProductRuntimeEvidence),
            ("Pivo e SafetyChain do Simulador permanecem visiveis", SimulatorCriticalUiRemainsVisible),
            ("quatro metricas do Simulador permanecem visiveis sem rolagem horizontal", SimulatorMetricsRemainVisible),
            ("Simulador light preserva contraste efetivo dos sinais", SimulatorLightThemeHasEffectiveContrast),
            ("Simulador evita regioes de rolagem aninhadas", SimulatorAvoidsNestedScrollRegions),
            ("scrolls de producao usam chrome fino tematico sem eixo horizontal", ProductionScrollbarsUseSlimThemedChrome),
            ("Testador evita scrolls aninhados simultaneos em 1366", TesterAvoidsActiveNestedScrollAtStandardViewport),
            ("faixa restante das tabs acompanha tema dark e light", TesterTabStripRemainderMatchesTheme),
            ("configuracao do Simulador cabe em 1366 sem rolagem propria", SimulatorConfigurationFitsStandardViewport),
            ("secoes do Simulador usam chrome tematico sem GroupBox classico", SimulatorSectionsUseThemedChrome),
            ("secoes internas do Simulador usam contrato flat sem borda aninhada", FlatSectionsAvoidNestedChrome),
            ("linhas de sinais compartilham surface sem caixas individuais", SignalRowsAvoidExcessiveBoxing),
            ("sinais do Simulador usam densidade responsiva de duas e tres colunas", SimulatorSignalLayoutIsResponsive),
            ("metricas do Simulador preservam hierarquia label valor contexto", SimulatorMetricsExposeVisualHierarchy),
            ("acoes primarias preservam alinhamento e texto", PrimaryActionsRemainAligned),
            ("controles criticos expoem nomes acessiveis", CriticalAccessibleNamesArePresent),
            ("navegacao tema e resize nao ampliam a arvore visual", NavigationThemeAndResizeKeepStructureStable),
            ("controles UI2 respeitam contrato de escala DPI", Ui2ControlsRespectDpiScalingContract),
            ("troca de tema preserva sessao, pagina e instancias", ThemeSwitchPreservesUiState),
            ("seletor de tema reflete o modo efetivamente ativo", ThemeSelectorMatchesActiveTheme),
            ("troca de tema e Pivo mantem recursos GDI estaveis", ThemeAndPivotKeepGdiResourcesStable),
            ("textos operacionais da UI2 permanecem em PT-BR", OperatorTextIsConsistent),
            ("sessao padrao usa endereco fake 1", SessionUsesFakeAddressOne),
            ("perfil Pivo pode ser manipulado offline", PivotProfileIsInteractive),
            ("perfil Poco pode ser carregado offline", WellProfileLoads),
            ("Testador identifica fake sem transporte fisico", TesterIdentifiesFake),
            ("saida simulada usa contrato fechado e retorna OFF", SimulatedOutputIsMomentary),
            ("novas camadas nao dependem de API serial ou TCP", NewLayersHaveNoPhysicalTransportTypes),
            ("grafo Core Application Desktop permanece aciclico", SharedAssemblyGraphIsAcyclic),
            ("Desktop referencia explicitamente as camadas compartilhadas", DesktopReferencesSharedLayers),
            ("contadores simulados do ciclo canonico sao deterministicos", () =>
            {
                canonicalCounters = CaptureCanonicalCounters();
                return canonicalCounters == new CanonicalCounters(1, 5, 2, 2, 2, 2);
            }),
            ("contadores fisicos permanecem zero", PhysicalCountersRemainZero)
        ];

        int passed = 0;
        foreach ((string name, Func<bool> validate) in scenarios)
        {
            try
            {
                if (!validate())
                {
                    error.WriteLine($"[FAIL] {name}");
                    continue;
                }

                output.WriteLine($"[OK] {name}");
                passed++;
            }
            catch (Exception ex)
            {
                error.WriteLine($"[FAIL] {name}: {ex.Message}");
            }
        }

        output.WriteLine($"Industrial platform UI: {passed}/{scenarios.Count}");
        if (canonicalCounters is not null)
        {
            output.WriteLine(
                $"SimulatedConnections={canonicalCounters.RtuConnections} "
                + $"SimulatedReads={canonicalCounters.RtuReads} "
                + $"SimulatedWrites={canonicalCounters.RtuWrites} "
                + $"SimulatedCommands={canonicalCounters.RtuCommands} "
                + $"SimulationOperations={canonicalCounters.SimulationOperations} "
                + $"SimulationCommands={canonicalCounters.SimulationCommands}");
        }

        output.WriteLine("PhysicalConnections=0 PhysicalReads=0 PhysicalWrites=0 PhysicalCommands=0");
        if (passed != scenarios.Count)
        {
            return 1;
        }

        try
        {
            if (IndustrialVisualProofCapture.CaptureRequested())
            {
                output.WriteLine("VisualProofCapture=PASS");
            }
        }
        catch (Exception ex)
        {
            error.WriteLine($"VisualProofCapture=FAIL: {ex.Message}");
            return 1;
        }

        return 0;
    }

    private static bool StartupWithoutArgumentsSelectsIndustrial() =>
        Program.ResolveStartupMode([]) == StartupMode.Industrial;

    private static bool IndustrialArgumentSelectsPlatform() =>
        Program.ResolveStartupMode(["--industrial"]) == StartupMode.Industrial;

    private static bool IndustrialArgumentIsCaseInsensitive() =>
        Program.ResolveStartupMode(["--INDUSTRIAL"]) == StartupMode.Industrial;

    private static bool LegacyArgumentSelectsMainForm()
    {
        StartupMode mode = Program.ResolveStartupMode(["--legacy"]);
        return mode == StartupMode.Legacy
            && Program.GetStartupFormType(mode) == typeof(MainForm);
    }

    private static bool LegacyArgumentIsCaseInsensitive()
    {
        StartupMode mode = Program.ResolveStartupMode(["--LEGACY"]);
        return mode == StartupMode.Legacy
            && Program.GetStartupFormType(mode) == typeof(MainForm);
    }

    private static bool ValidatorsKeepStartupPriority()
    {
        string[] validators =
        [
            "--validate-hardware-catalog",
            "--validate-hardware-profile-selection",
            "--validate-hardware-test-report",
            "--validate-layout-3-host-readonly-safety",
            "--validate-layout-3-read-bridge-disabled",
            "--validate-layout-3-read-bridge-activation-gate",
            "--validate-layout-3-bench-readiness-self-tests",
            "--validate-layout-3-simulation-engine",
            "--validate-layout-3-test-simulator-integration",
            "--validate-layout-3-industrial-platform-ui",
            "--validate-industrial-io-mapping",
            "--validate-layout-3-rtu-offline",
            "--validate-layout-3-bench-readiness"
        ];
        return validators.All(argument =>
                Program.ResolveStartupMode([argument]) == StartupMode.Validator)
            && Program.ResolveStartupMode(
                ["--industrial", "--validate-layout-3-industrial-platform-ui"])
                == StartupMode.Validator
            && Program.ResolveStartupMode(
                ["--legacy", "--validate-layout-3-industrial-platform-ui"])
                == StartupMode.Validator
            && Program.ResolveStartupMode(
                ["--unknown-startup-mode", "--validate-layout-3-industrial-platform-ui"])
                == StartupMode.Validator;
    }

    private static bool UnknownArgumentIsRejected()
    {
        StartupMode mode = Program.ResolveStartupMode(["--unknown-startup-mode"]);
        using StringWriter error = new();
        int exitCode = Program.RejectUnknownStartupArguments(
            ["--unknown-startup-mode"],
            error);
        return mode == StartupMode.Unknown
            && Program.GetStartupFormType(mode) is null
            && Program.ResolveStartupMode(["--use-industrial-host"]) == StartupMode.Unknown
            && exitCode != 0
            && error.ToString().Contains("--unknown-startup-mode", StringComparison.Ordinal);
    }

    private static bool UnknownArgumentStaysOffline()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        StartupMode mode = Program.ResolveStartupMode(["--unknown-startup-mode"]);
        using StringWriter error = new();
        int exitCode = Program.RejectUnknownStartupArguments(
            ["--unknown-startup-mode"],
            error);
        return mode == StartupMode.Unknown
            && exitCode != 0
            && PhysicalCountersAreZero(session);
    }

    private static bool IndustrialLauncherCreatesPlatformHost()
    {
        using Form form = Program.CreateStartupForm(StartupMode.Industrial);
        return form is IndustrialPlatformForm;
    }

    private static bool HostHasHomeAndTwoModes()
    {
        using IndustrialPlatformForm form = new();
        return form.HomeModeButton.AccessibleName == "Início"
            && form.TesterModeButton.AccessibleName == "Testador"
            && form.SimulatorModeButton.AccessibleName == "Simulador";
    }

    private static bool NavigationPreservesModeInstances()
    {
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        IndustrialTesterControl? tester = form.TesterInstance;
        form.ShowSimulator();
        IndustrialSimulatorControl? simulator = form.SimulatorInstance;
        int controlCount = CountControls(form);
        for (int index = 0; index < 8; index++)
        {
            form.ShowHome();
            form.ShowTester();
            form.ShowSimulator();
        }

        return tester is not null
            && simulator is not null
            && ReferenceEquals(tester, form.TesterInstance)
            && ReferenceEquals(simulator, form.SimulatorInstance)
            && FindAll<IndustrialTesterControl>(form).Count == 1
            && FindAll<IndustrialSimulatorControl>(form).Count == 1
            && CountControls(form) == controlCount;
    }

    private static bool ShellDoesNotExposeLegacy()
    {
        using IndustrialPlatformForm form = new();
        return FindAll<Button>(form).All(button =>
            !button.Text.Contains("LEGACY", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(button.AccessibleName, "LEGACY", StringComparison.OrdinalIgnoreCase));
    }

    private static bool SafetyChainIsVisuallyReadOnly()
    {
        using IndustrialPlatformForm form = new();
        _ = form.Session;
        Label? safety = form.Controls.Find("platformSafetyStatus", searchAllChildren: true)
            .OfType<Label>()
            .FirstOrDefault();
        return safety is not null
            && safety.Text.Contains("READ-ONLY", StringComparison.Ordinal)
            && safety.AccessibleDescription?.Contains("derivada", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool Ui2ThemeProvidesDarkAndLight() =>
        IndustrialPalette.Dark.IsDark
        && !IndustrialPalette.Light.IsDark
        && IndustrialPalette.Dark.Background != IndustrialPalette.Light.Background
        && IndustrialPalette.Dark.Accent != IndustrialPalette.Light.Accent;

    private static bool ProductionTypographyUsesSemanticRoles()
    {
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        Label? brand = form.Controls.Find("industrialBrand", searchAllChildren: true)
            .OfType<Label>()
            .FirstOrDefault();
        Label? pageTitle = form.Controls.Find("testerPageTitle", searchAllChildren: true)
            .OfType<Label>()
            .FirstOrDefault();
        Label? rtuTitle = form.Controls.Find("rtuConfigurationTitle", searchAllChildren: true)
            .OfType<Label>()
            .FirstOrDefault();
        Button? navigation = form.Controls.Find("modeTesterButton", searchAllChildren: true)
            .OfType<Button>()
            .FirstOrDefault();
        Button? action = form.Controls.Find("validateRtuButton", searchAllChildren: true)
            .OfType<Button>()
            .FirstOrDefault();
        Label? status = form.Controls.Find("platformOfflineStatus", searchAllChildren: true)
            .OfType<Label>()
            .FirstOrDefault();
        using Font productFont = IndustrialTypography.ProductTitle();
        using Font pageFont = IndustrialTypography.PageTitle();
        using Font sectionFont = IndustrialTypography.SectionTitle();
        using Font navigationFont = IndustrialTypography.Navigation();
        using Font buttonFont = IndustrialTypography.Button();
        using Font statusFont = IndustrialTypography.Status();
        return Matches(brand?.Font, productFont)
            && Matches(pageTitle?.Font, pageFont)
            && Matches(rtuTitle?.Font, sectionFont)
            && Matches(navigation?.Font, navigationFont)
            && Matches(action?.Font, buttonFont)
            && Matches(status?.Font, statusFont);

        static bool Matches(Font? actual, Font expected) => actual is not null
            && actual.FontFamily.Name == expected.FontFamily.Name
            && Math.Abs(actual.SizeInPoints - expected.SizeInPoints) < 0.1F
            && actual.Style == expected.Style;
    }

    private static bool OperationalTextUsesMinimumRuntimeSize()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowTester();
        form.ShowSimulator();
        form.Show();
        PerformLayoutTree(form);
        return FindAll<Control>(form)
            .Where(control => control is Label or Button or CheckBox or TextBox or ComboBox or NumericUpDown)
            .Where(control => !string.IsNullOrWhiteSpace(control.Text))
            .All(control => control.Font.SizeInPoints >= 8.49F);
    }

    private static bool DesignSystemPreviewIsQaOnly()
    {
        using IndustrialDesignSystemPreviewForm preview = new();
        using Form startup = Program.CreateStartupForm(StartupMode.Industrial);
        IndustrialButton[] buttons = FindAll<IndustrialButton>(preview).ToArray();
        IndustrialGroupBox[] sections = FindAll<IndustrialGroupBox>(preview).ToArray();
        return preview.Name == "industrialDesignSystemPreview"
            && buttons.Length >= 8
            && buttons.Any(button => button.IsNavigation && button.IsSelected)
            && sections.Length >= 5
            && preview.Controls.Find("previewListHeader", true).Length == 1
            && preview.Controls.Find("previewListRow", true).Length == 1
            && startup is IndustrialPlatformForm;
    }

    private static bool ThemeContrastIsAccessible(IndustrialPalette palette) =>
        IndustrialTheme.CriticalContrastRatios(palette).Values.All(ratio => ratio >= 4.5D);

    private static bool IndustrialControlsExposeAccessibleStates()
    {
        using IndustrialLedIndicatorControl led = new() { LabelText = "DI01", IsOn = false };
        using IndustrialPushButtonControl button = new() { Title = "DO01", IsActive = false };
        using EmergencyStopButtonControl emergency = new();
        int clicks = 0;
        button.Click += (_, _) => clicks++;
        InvokeKey(button, "OnKeyDown", Keys.Space);
        InvokeKey(button, "OnKeyUp", Keys.Space);
        return led.AccessibleDescription?.Contains("desligado", StringComparison.OrdinalIgnoreCase) == true
            && button.TabStop
            && emergency.TabStop
            && !led.TabStop
            && clicks == 1;
    }

    private static bool IndustrialCheckBoxPreservesNativeSemantics()
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            using IndustrialCheckBox checkBox = new()
            {
                Text = "Sinal simulado",
                AccessibleName = "Sinal simulado",
                ClientSize = new Size(180, IndustrialSpacing.InteractiveHeight)
            };
            using Panel host = new()
            {
                BackColor = IndustrialPalette.Dark.SurfaceElevated,
                ClientSize = new Size(220, 48)
            };
            host.Controls.Add(checkBox);
            InvokeKey(checkBox, "OnKeyDown", Keys.Space);
            InvokeKey(checkBox, "OnKeyUp", Keys.Space);
            bool toggledByKeyboard = checkBox.Checked;
            foreach (IndustrialThemeMode mode in new[]
                     {
                         IndustrialThemeMode.Dark,
                         IndustrialThemeMode.Light,
                         IndustrialThemeMode.Dark
                     })
            {
                IndustrialTheme.SetMode(mode);
                checkBox.ApplyTheme();
                using Bitmap bitmap = new(checkBox.Width, checkBox.Height);
                checkBox.DrawToBitmap(bitmap, checkBox.ClientRectangle);
            }

            return checkBox.UsesIndustrialChrome
                && toggledByKeyboard
                && checkBox.TabStop
                && checkBox.AccessibleRole == AccessibleRole.CheckButton
                && checkBox.AccessibleName == "Sinal simulado"
                && typeof(IndustrialCheckBox)
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .All(field => field.FieldType != typeof(System.Windows.Forms.Timer));
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static bool IndustrialControlsKeepGdiResourcesStable()
    {
        RenderIndustrialControls(12);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        int afterWarmup = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
        RenderIndustrialControls(12);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        int afterRepeat = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
        return afterWarmup > 0 && afterRepeat - afterWarmup <= 4;
    }

    private static void RenderIndustrialControls(int iterations)
    {
        for (int index = 0; index < iterations; index++)
        {
            using IndustrialLedIndicatorControl led = new() { IsOn = index % 2 != 0 };
            using IndustrialPushButtonControl button = new() { IsActive = index % 2 != 0 };
            using EmergencyStopButtonControl emergency = new();
            using IndustrialCheckBox checkBox = new()
            {
                Text = "Sinal simulado",
                Checked = index % 2 != 0,
                Size = new Size(180, IndustrialSpacing.InteractiveHeight)
            };
            foreach (Control control in new Control[] { led, button, emergency, checkBox })
            {
                control.Size = new Size(control.Width + (index % 3), control.Height + (index % 2));
                using Bitmap bitmap = new(Math.Max(1, control.Width), Math.Max(1, control.Height));
                control.DrawToBitmap(bitmap, control.ClientRectangle);
            }
        }
    }

    private static void InvokeKey(Control control, string methodName, Keys key)
    {
        MethodInfo? method = control.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        method?.Invoke(control, [new KeyEventArgs(key)]);
    }

    [DllImport("user32.dll")]
    private static extern int GetGuiResources(IntPtr process, int flags);

    private static bool Layout3HostLoadsLayout3HostControl()
    {
        using Layout3HostForm form = new(HardwareCatalog.Empty);
        return Find<Layout3HostControl>(form) is not null;
    }

    private static bool IndustrialLauncherLifecycleStaysOffline()
    {
        using Form form = Program.CreateStartupForm(StartupMode.Industrial);
        if (form is not IndustrialPlatformForm platform)
        {
            return false;
        }

        return PhysicalCountersAreZero(platform.Session);
    }

    private static bool TesterLoadsLazily()
    {
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        IndustrialTesterControl? tester = Find<IndustrialTesterControl>(form);
        return tester is { UsesOnlyInMemoryTransport: true, RealCommunicationEnabled: false };
    }

    private static bool SimulatorLoadsLazily()
    {
        using IndustrialPlatformForm form = new();
        form.ShowSimulator();
        return Find<IndustrialSimulatorControl>(form) is { HasPhysicalTransport: false };
    }

    private static bool PivotAliasesAndIoMapAreVisible()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session);
        return tester.HasIoMappingTab
            && tester.IoMappingRowCount == session.Profile.IoBindings.Count
            && tester.IoMappingDeclaresSimulationEvidence
            && tester.ShowsProcessAlias("DI00", "Emergência")
            && tester.ShowsProcessAlias("AI00", "Pressão")
            && tester.ShowsProcessAlias("DO00", "Bomba");
    }

    private static bool WellAliasesAndIoMapAreVisible()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("poco"));
        using IndustrialTesterControl tester = new(session);
        return tester.IoMappingRowCount == session.Profile.IoBindings.Count
            && tester.ShowsProcessAlias("DI00", "Nível mínimo")
            && tester.ShowsProcessAlias("AI00", "Nível")
            && tester.ShowsProcessAlias("DO01", "Válvula");
    }

    private static bool PivotRendererLoadsWithoutTimer()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        return simulator.HasPivotRenderer
            && simulator.RenderedTowerCount == 4
            && !simulator.UsesContinuousAnimation
            && simulator.UsesGroupedSignalEditor;
    }

    private static bool PivotRendererTracksStateChanges()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        int initialRevision = simulator.PivotStateRevision;
        session.ApplyScenario("movendo-frente");
        int changedRevision = simulator.PivotStateRevision;
        session.ApplyScenario("movendo-frente");
        return initialRevision > 0
            && changedRevision == initialRevision + 1
            && simulator.PivotStateRevision == changedRevision;
    }

    private static bool WellUsesGenericGroupedEditor()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("poco"));
        using IndustrialSimulatorControl simulator = new(session);
        return !simulator.HasPivotRenderer
            && simulator.RenderedTowerCount == 0
            && simulator.UsesGroupedSignalEditor;
    }

    private static bool LayoutFits(Size clientSize)
    {
        using IndustrialPlatformForm form = new()
        {
            ClientSize = clientSize,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        form.ShowSimulator();
        form.Show();
        PerformLayoutTree(form);
        IndustrialSimulatorControl? simulator = Find<IndustrialSimulatorControl>(form);
        PivotProcessControl? pivot = Find<PivotProcessControl>(form);
        bool simulatorFits = simulator is not null
            && simulator.ClientSize.Width >= 680
            && simulator.ClientSize.Height >= 560
            && pivot is { Width: >= 420, Height: >= 240 };

        form.ShowTester();
        PerformLayoutTree(form);
        IndustrialTesterControl? tester = Find<IndustrialTesterControl>(form);
        TabControl? tabs = Find<TabControl>(form);
        TabPage? mapPage = tabs?.TabPages.Cast<TabPage>().FirstOrDefault(page => page.Name == "ioMappingTab");
        if (tabs is not null && mapPage is not null)
        {
            tabs.SelectedTab = mapPage;
            mapPage.CreateControl();
            PerformLayoutTree(form);
        }

        DataGridView? map = form.Controls.Find("ioMappingGrid", searchAllChildren: true)
            .OfType<DataGridView>()
            .FirstOrDefault();
        int minimumMapHeight = clientSize.Height <= 768 ? 128 : 180;
        bool result = simulatorFits
            && tester is not null
            && tester.ClientSize.Width >= 900
            && tester.ClientSize.Height >= 560
            && map is { Width: >= 700 }
            && map.Height >= minimumMapHeight
            && map.ScrollBars is ScrollBars.Vertical or ScrollBars.Both;
        if (!result)
        {
            throw new InvalidOperationException(
                $"cliente={clientSize.Width}x{clientSize.Height}; "
                + $"simulador={simulator?.ClientSize.Width ?? -1}x{simulator?.ClientSize.Height ?? -1}; "
                + $"pivo={pivot?.Width ?? -1}x{pivot?.Height ?? -1}; "
                + $"testador={tester?.ClientSize.Width ?? -1}x{tester?.ClientSize.Height ?? -1}; "
                + $"mapa={map?.Width ?? -1}x{map?.Height ?? -1}; minimo={minimumMapHeight}.");
        }

        return true;
    }

    private static bool CompactShellPreservesCriticalUi()
    {
        using IndustrialPlatformForm form = new()
        {
            ClientSize = new Size(1024, 680),
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        form.ShowTester();
        IndustrialTesterControl? tester = form.TesterInstance;
        form.ShowSimulator();
        IndustrialSimulatorControl? simulator = form.SimulatorInstance;
        form.Show();
        PerformLayoutTree(form);

        Label? safety = form.Controls.Find("platformSafetyStatus", searchAllChildren: true)
            .OfType<Label>()
            .FirstOrDefault();
        return form.IsCompactNavigation
            && tester is not null
            && simulator is not null
            && form.TesterModeButton.Visible
            && form.SimulatorModeButton.Visible
            && safety is { Visible: true, Width: > 120, Height: >= 16 }
            && safety.Text.Contains("READ-ONLY", StringComparison.Ordinal);
    }

    private static bool ResponsiveShellFits(Size physicalViewport, int dpi)
    {
        Size logicalViewport = new(
            Math.Max(760, (int)Math.Floor(physicalViewport.Width * 96D / dpi)),
            Math.Max(500, (int)Math.Floor(physicalViewport.Height * 96D / dpi)));
        using IndustrialPlatformForm form = new()
        {
            MinimumSize = Size.Empty,
            ClientSize = logicalViewport,
            FormBorderStyle = FormBorderStyle.None,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        _ = form.Session;
        form.Show();
        form.ClientSize = logicalViewport;
        PerformLayoutTree(form);

        Control[] primaryRegions =
        [
            form.HeaderRegion,
            form.BodyRegion,
            form.ContentRegion,
            form.FooterRegion,
            form.ThemeSelector
        ];
        Control? outside = primaryRegions.FirstOrDefault(control => !IsInsideParent(control));
        if (outside is not null)
        {
            throw new InvalidOperationException(
                $"{physicalViewport.Width}x{physicalViewport.Height}@{dpi}: "
                + $"{outside.Name} fora do parent; bounds={outside.Bounds}; "
                + $"parent={outside.Parent?.ClientRectangle}.");
        }

        Rectangle headerBounds = BoundsRelativeTo(form.HeaderRegion, form);
        Rectangle bodyBounds = BoundsRelativeTo(form.BodyRegion, form);
        Rectangle footerBounds = BoundsRelativeTo(form.FooterRegion, form);
        if (headerBounds.Height < IndustrialSpacing.HeaderHeight
            || headerBounds.Bottom > bodyBounds.Top
            || bodyBounds.Bottom > footerBounds.Top
            || footerBounds.Bottom > form.ClientRectangle.Bottom)
        {
            throw new InvalidOperationException(
                $"{physicalViewport.Width}x{physicalViewport.Height}@{dpi}: "
                + $"header={headerBounds}; body={bodyBounds}; footer={footerBounds}; "
                + $"client={form.ClientRectangle}.");
        }

        IReadOnlyList<Label> statuses = form.CriticalHeaderStatuses;
        Rectangle[] statusBounds = statuses
            .Select(status => BoundsRelativeTo(status, form))
            .ToArray();
        bool statusTextFits = statuses.All(StatusTextFits);
        bool statusesDoNotOverlap = statusBounds
            .SelectMany((left, index) => statusBounds.Skip(index + 1)
                .Select(right => !left.IntersectsWith(right)))
            .All(value => value);
        bool result = statuses.All(status => status.Visible && IsInsideParent(status))
            && statusTextFits
            && statusesDoNotOverlap
            && form.ThemeSelector.Visible
            && form.ThemeSelector.Width >= 96
            && form.TesterModeButton.Visible
            && form.SimulatorModeButton.Visible;
        if (!result)
        {
            throw new InvalidOperationException(
                $"{physicalViewport.Width}x{physicalViewport.Height}@{dpi}: "
                + $"compact={form.IsCompactNavigation}; statusText={statusTextFits}; "
                + $"statusOverlap={!statusesDoNotOverlap}; theme={form.ThemeSelector.Bounds}; "
                + $"statuses={string.Join(", ", statuses.Select(status => $"{status.Name}:{status.Bounds}:{StatusTextFits(status)}"))}.");
        }

        return true;
    }

    private static bool StatusTextFits(Label label)
    {
        Size measured = TextRenderer.MeasureText(
            label.Text,
            label.Font,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        return measured.Width <= label.ClientSize.Width - label.Padding.Horizontal;
    }

    private static bool ShellTitleFitsAllPagesAndThemes()
    {
        IndustrialThemeMode originalMode = IndustrialTheme.Mode;
        try
        {
            foreach (Size viewport in new[] { new Size(1366, 768), new Size(1920, 1080) })
            {
                foreach (IndustrialThemeMode mode in new[] { IndustrialThemeMode.Dark, IndustrialThemeMode.Light })
                {
                    using IndustrialPlatformForm form = new()
                    {
                        ClientSize = viewport,
                        Opacity = 0,
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.Manual,
                        Location = new Point(-32000, -32000)
                    };
                    form.SetTheme(mode);
                    form.Show();
                    PerformLayoutTree(form);
                    EnsureShellTitleFits(form, viewport, mode, "Home");

                    form.ShowTester();
                    PerformLayoutTree(form);
                    EnsureShellTitleFits(form, viewport, mode, "Testador");

                    form.ShowSimulator();
                    PerformLayoutTree(form);
                    EnsureShellTitleFits(form, viewport, mode, "Simulador");
                }
            }

            return true;
        }
        finally
        {
            IndustrialTheme.SetMode(originalMode);
        }
    }

    private static void EnsureShellTitleFits(
        IndustrialPlatformForm form,
        Size viewport,
        IndustrialThemeMode mode,
        string page)
    {
        Label title = form.BrandTitle;
        Label context = form.BrandContext;
        Size measured = TextRenderer.MeasureText(
            title.Text,
            title.Font,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        Rectangle titleBounds = BoundsRelativeTo(title, form.HeaderRegion);
        Rectangle contextBounds = BoundsRelativeTo(context, form.HeaderRegion);
        bool fits = title.Visible
            && IsInsideParent(title)
            && IsContainedByAllAncestors(title, form.HeaderRegion)
            && measured.Width <= title.ClientSize.Width - title.Padding.Horizontal
            && measured.Height + IndustrialSpacing.Xs <= title.ClientSize.Height - title.Padding.Vertical
            && form.HeaderRegion.ClientRectangle.Contains(titleBounds)
            && !titleBounds.IntersectsWith(contextBounds)
            && (!context.Visible || IsContainedByAllAncestors(context, form.HeaderRegion));
        if (!fits)
        {
            throw new InvalidOperationException(
                $"{viewport.Width}x{viewport.Height} {mode} {page}: "
                + $"titulo={titleBounds}; contexto={contextBounds}; medido={measured}; "
                + $"clienteTitulo={title.ClientSize}; header={form.HeaderRegion.ClientRectangle}.");
        }
    }

    private static bool ShellTitleDpiBudgetIsSafe()
    {
        using IndustrialPlatformForm form = new();
        Size measured = TextRenderer.MeasureText(
            form.BrandTitle.Text,
            form.BrandTitle.Font,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        foreach (Size viewport in new[] { new Size(1366, 768), new Size(1920, 1080) })
        {
            foreach (int dpi in new[] { 96, 120, 144 })
            {
                double logicalWidth = viewport.Width * 96D / dpi;
                bool compact = logicalWidth < 1180D;
                int headerHeight = IndustrialPlatformForm.ScaleLogicalMetric(
                    compact ? IndustrialSpacing.HeaderCompactHeight : IndustrialSpacing.HeaderHeight,
                    dpi);
                int verticalPadding = IndustrialPlatformForm.ScaleLogicalMetric(
                    IndustrialSpacing.Xs * 2,
                    dpi);
                int titleBudget = headerHeight
                    - verticalPadding
                    - IndustrialPlatformForm.ScaleLogicalMetric(24, dpi);
                int requiredHeight = IndustrialPlatformForm.ScaleLogicalMetric(
                    measured.Height + IndustrialSpacing.Xs,
                    dpi);
                if (titleBudget < requiredHeight)
                {
                    throw new InvalidOperationException(
                        $"{viewport.Width}x{viewport.Height}@{dpi}: "
                        + $"budget={titleBudget}; required={requiredHeight}; header={headerHeight}.");
                }
            }
        }

        return true;
    }

    private static bool ShellCriticalRegionsRemainContained()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        _ = form.Session;
        form.Show();
        PerformLayoutTree(form);
        Control[] critical =
        [
            form.HeaderRegion,
            form.BodyRegion,
            form.FooterRegion,
            form.ThemeSelector,
            .. form.CriticalHeaderStatuses
        ];
        bool contained = critical.All(control => control.Visible && IsInsideParent(control));
        bool statusesSeparate = form.CriticalHeaderStatuses
            .Select(status => BoundsRelativeTo(status, form))
            .SelectMany((left, index) => form.CriticalHeaderStatuses
                .Skip(index + 1)
                .Select(right => !left.IntersectsWith(BoundsRelativeTo(right, form))))
            .All(value => value);
        bool footerContained = form.FooterRegion.Controls.Cast<Control>()
            .All(control => control.Visible && IsInsideParent(control));
        return contained && statusesSeparate && footerContained;
    }

    private static bool HomeActionsRemainContained()
    {
        foreach (Size viewport in new[] { new Size(1024, 680), new Size(1366, 768) })
        {
            using IndustrialPlatformForm form = CreateOffscreenForm(viewport);
            form.Show();
            PerformLayoutTree(form);
            Button? tester = form.Controls.Find("welcomeTesterButton", true).OfType<Button>().FirstOrDefault();
            Button? simulator = form.Controls.Find("welcomeSimulatorButton", true).OfType<Button>().FirstOrDefault();
            TableLayoutPanel? welcome = form.Controls.Find("industrialWelcome", true)
                .OfType<TableLayoutPanel>()
                .FirstOrDefault();
            if (tester is null || simulator is null || welcome is null
                || !IsInsideParent(tester) || !IsInsideParent(simulator)
                || tester.Height < 36 || simulator.Height < 36
                || BoundsRelativeTo(tester, welcome).IntersectsWith(BoundsRelativeTo(simulator, welcome)))
            {
                return false;
            }

            bool expectedStacked = viewport.Width < 1100;
            bool stacked = welcome.GetCellPosition(tester.Parent!.Parent!).Column == 0
                && welcome.GetCellPosition(simulator.Parent!.Parent!).Column == 0;
            if (stacked != expectedStacked)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HomeCardsUseContentDrivenHeight()
    {
        foreach (Size viewport in new[] { new Size(1366, 768), new Size(1920, 1080) })
        {
            using IndustrialPlatformForm form = CreateOffscreenForm(viewport);
            form.ShowHome();
            form.Show();
            PerformLayoutTree(form);
            Panel[] cards = new[] { "welcomeTestadorCard", "welcomeSimuladorCard" }
                .Select(name => form.Controls.Find(name, true).OfType<Panel>().SingleOrDefault())
                .Where(card => card is not null)
                .Cast<Panel>()
                .ToArray();
            Button[] actions = new[] { "welcomeTesterButton", "welcomeSimulatorButton" }
                .Select(name => form.Controls.Find(name, true).OfType<Button>().SingleOrDefault())
                .Where(button => button is not null)
                .Cast<Button>()
                .ToArray();
            if (cards.Length != 2
                || actions.Length != 2
                || cards.Select(card => card.Height).Distinct().Count() != 1
                || cards.Any(card => card.Height > 240 || card.Height < 200)
                || actions.Select(action => BoundsRelativeTo(action, form).Top).Distinct().Count() != 1)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HomeStatusIsNotButtonLike()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowHome();
        form.Show();
        PerformLayoutTree(form);
        Label[] statuses = FindAll<Label>(form)
            .Where(label => label.Name.StartsWith("welcome", StringComparison.Ordinal)
                && label.Name.EndsWith("Status", StringComparison.Ordinal))
            .ToArray();
        return statuses.Length == 2
            && statuses.All(status => status.AutoSize
                && !status.TabStop
                && status.Cursor != Cursors.Hand
                && status.Parent is not null
                && status.Width < status.Parent.ClientSize.Width * 0.8F);
    }

    private static bool SidebarSeparatesNavigationFromPhysicalStatus()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.Show();
        PerformLayoutTree(form);
        Label physical = form.SidebarPhysicalStatus;
        Label offline = form.SidebarOfflineStatus;
        Button[] navigation = [form.HomeModeButton, form.TesterModeButton, form.SimulatorModeButton];
        return physical.Parent == offline.Parent
            && physical is { TabStop: false, AccessibleRole: AccessibleRole.StaticText }
            && offline is { TabStop: false, AccessibleRole: AccessibleRole.StaticText }
            && physical.Cursor != Cursors.Hand
            && offline.Cursor != Cursors.Hand
            && navigation.All(button => button.TabStop && button.Cursor == Cursors.Hand)
            && BoundsRelativeTo(offline, form).Top > navigation.Max(button => BoundsRelativeTo(button, form).Bottom);
    }

    private static bool SidebarUsesCompactPassiveConnectionEvidence()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.Show();
        PerformLayoutTree(form);
        Label[] statuses = [form.SidebarOfflineStatus, form.SidebarPhysicalStatus];
        return statuses.All(status => status.Parent is not null
            && status.Height <= IndustrialPlatformForm.ScaleLogicalMetric(28, status.DeviceDpi)
            && status.BackColor == status.Parent.BackColor
            && status.TextAlign == ContentAlignment.MiddleLeft
            && !status.TabStop);
    }

    private static bool OverviewUsesInformationalTone()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowHome();
        form.Show();
        PerformLayoutTree(form);
        Label? mode = form.Controls.Find("platformModeStatus", true).OfType<Label>().SingleOrDefault();
        return mode?.Tag is PlatformStatusTone.Active
            && mode.BackColor == IndustrialTheme.Palette.SurfaceInteractive
            && mode.BackColor != IndustrialTheme.Palette.SuccessSurface;
    }

    private static bool GlobalSummaryUsesPassiveCompactStatuses()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        _ = form.Session;
        form.ShowTester();
        form.Show();
        PerformLayoutTree(form);
        Label[] statuses =
        [
            .. form.CriticalHeaderStatuses,
            form.EquipmentStatus
        ];
        return statuses.Length == 4
            && statuses.All(status => !status.TabStop
                && status.Cursor == Cursors.Default
                && status.BackColor == IndustrialTheme.Palette.SurfaceInteractive
                && status.Height <= IndustrialPlatformForm.ScaleLogicalMetric(28, status.DeviceDpi));
    }

    private static bool TesterControlsRemainContained()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowTester();
        form.Show();
        PerformLayoutTree(form);
        string[] actions =
        [
            "refreshPortsButton",
            "validateRtuButton",
            "identifyButton",
            "discoverButton",
            "cancelButton"
        ];
        Button?[] actionButtons = actions
            .Select(name => form.Controls.Find(name, true).OfType<Button>().FirstOrDefault())
            .ToArray();
        bool actionsFit = actionButtons
            .All(button => button is not null && button.Visible && IsInsideParent(button));
        TabControl? tabs = form.Controls.Find("testerTabs", true).OfType<TabControl>().FirstOrDefault();
        Rectangle selectedTab = tabs is null || tabs.SelectedIndex < 0
            ? Rectangle.Empty
            : tabs.GetTabRect(tabs.SelectedIndex);
        bool result = actionsFit
            && tabs is { Visible: true, TabCount: 6 }
            && tabs.ClientRectangle.Contains(selectedTab)
            && selectedTab.Height >= 30;
        if (!result)
        {
            throw new InvalidOperationException(
                $"actions={actionsFit} ["
                + string.Join(", ", actionButtons.Select(button =>
                    $"{button?.Name}:{button?.Visible}:{button?.Bounds}/{button?.Parent?.ClientRectangle}"))
                + $"]; tabs={tabs?.Bounds}; count={tabs?.TabCount}; "
                + $"selected={selectedTab}; client={tabs?.ClientRectangle}.");
        }

        return true;
    }

    private static bool TesterRtuFieldsUseIndustrialChrome()
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            foreach (IndustrialThemeMode mode in new[] { IndustrialThemeMode.Dark, IndustrialThemeMode.Light })
            {
                IndustrialTheme.SetMode(mode);
                using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
                using IndustrialTesterControl tester = new(session);
                tester.ApplyTheme();
                ComboBox[] fields = FindAll<ComboBox>(tester).ToArray();
                if (fields.Length < 6
                    || fields.Any(field => field is not IndustrialComboBox
                        || field.DropDownStyle != ComboBoxStyle.DropDownList
                        || field.BackColor != IndustrialTheme.Palette.Field
                        || IndustrialTheme.ContrastRatio(field.ForeColor, field.BackColor) < 4.5D))
                {
                    throw new InvalidOperationException(
                        $"mode={mode}; count={fields.Length}; "
                        + string.Join(" | ", fields.Select(field =>
                            $"{field.GetType().Name}:{field.DropDownStyle}:"
                            + $"back={field.BackColor}:expected={IndustrialTheme.Palette.Field}:"
                            + $"ratio={IndustrialTheme.ContrastRatio(field.ForeColor, field.BackColor):0.00}")));
                }
            }

            return true;
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static bool TesterRtuHeaderUsesFlatSectionComposition()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session);
        Label? title = tester.Controls.Find("rtuConfigurationTitle", true).OfType<Label>().SingleOrDefault();
        Label? subtitle = tester.Controls.Find("rtuConfigurationSubtitle", true).OfType<Label>().SingleOrDefault();
        Panel? divider = tester.Controls.Find("rtuConfigurationTitleDivider", true).OfType<Panel>().SingleOrDefault();
        TableLayoutPanel? header = tester.Controls.Find("rtuConfigurationSectionHeader", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        return title is not null
            && subtitle is not null
            && divider is not null
            && header is not null
            && title.Parent == header
            && subtitle.Parent == header
            && divider.Parent == header
            && title.Text == "Parâmetros RTU"
            && subtitle.Text == "Referência offline"
            && !title.Text.Contains('•')
            && title.Parent?.Parent?.Parent is ResponsiveRtuConfigurationControl;
    }

    private static bool TesterRtuLayoutIsResponsive()
    {
        string[] expectedOrder =
        [
            "COM (informativa)",
            "Camada física",
            "Baud",
            "Data bits",
            "Paridade",
            "Stop bits",
            "Timeout (ms)",
            "Intervalo (ms)",
            "Endereço inicial",
            "Endereço final"
        ];
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session) { Dock = DockStyle.Fill };
        using Form host = new()
        {
            ClientSize = new Size(1112, 610),
            FormBorderStyle = FormBorderStyle.None,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        host.Controls.Add(tester);
        host.Show();
        ResponsiveRtuConfigurationControl? rtu = Find<ResponsiveRtuConfigurationControl>(tester);
        if (rtu is null || !rtu.FieldLabels.SequenceEqual(expectedOrder, StringComparer.Ordinal))
        {
            return false;
        }

        (int Width, int Height, int Fields, int Actions)[] layouts =
        [
            (1112, 610, 5, 5),
            (700, 900, 2, 2),
            (480, 1300, 1, 1)
        ];
        foreach ((int width, int height, int fields, int actions) in layouts)
        {
            host.ClientSize = new Size(width, height);
            PerformLayoutTree(host);
            TableLayoutPanel? fieldGrid = tester.Controls.Find("rtuFieldGrid", true)
                .OfType<TableLayoutPanel>()
                .SingleOrDefault();
            TableLayoutPanel? actionGrid = tester.Controls.Find("rtuActionGrid", true)
                .OfType<TableLayoutPanel>()
                .SingleOrDefault();
            if (rtu.FieldColumnCount != fields
                || rtu.ActionColumnCount != actions
                || rtu.UsesHorizontalScroll
                || fieldGrid is null
                || actionGrid is null
                || fieldGrid.Controls.Count != expectedOrder.Length
                || actionGrid.Controls.Count != 5
                || fieldGrid.Controls.Cast<Control>().Any(control => !IsInsideParent(control))
                || actionGrid.Controls.Cast<Control>().Any(control => !IsInsideParent(control)))
            {
                throw new InvalidOperationException(
                    $"viewport={width}x{height}; fields={rtu.FieldColumnCount}/{fields}; "
                    + $"actions={rtu.ActionColumnCount}/{actions}; horizontal={rtu.UsesHorizontalScroll}; "
                    + $"fieldControls={fieldGrid?.Controls.Count}; actionControls={actionGrid?.Controls.Count}.");
            }
        }

        return true;
    }

    private static bool TesterRtuActionsUseOneVisualSpecification()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session);
        Dictionary<string, PlatformButtonTone> expected = new(StringComparer.Ordinal)
        {
            ["refreshPortsButton"] = PlatformButtonTone.Secondary,
            ["validateRtuButton"] = PlatformButtonTone.Primary,
            ["identifyButton"] = PlatformButtonTone.Secondary,
            ["discoverButton"] = PlatformButtonTone.Secondary,
            ["cancelButton"] = PlatformButtonTone.Danger
        };
        Button[] buttons = expected.Keys
            .Select(name => tester.Controls.Find(name, true).OfType<Button>().SingleOrDefault())
            .Where(button => button is not null)
            .Cast<Button>()
            .ToArray();
        return buttons.Length == expected.Count
            && buttons.Select(button => button.Font.Name).Distinct(StringComparer.Ordinal).Count() == 1
            && buttons.Select(button => button.Font.Size).Distinct().Count() == 1
            && buttons.Select(button => button.Font.Style).Distinct().Count() == 1
            && buttons.Select(button => button.Height).Distinct().Single() == IndustrialSpacing.InteractiveHeight
            && buttons.Select(button => button.Padding).Distinct().Count() == 1
            && buttons.All(button => button.TextAlign == ContentAlignment.MiddleCenter
                && button.AccessibleRole == AccessibleRole.PushButton
                && button.TabStop
                && !button.UseCompatibleTextRendering
                && button.Tag is PlatformButtonTone tone
                && tone == expected[button.Name]);
    }

    private static bool TesterRtuUsesContentDrivenHeight()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session) { Dock = DockStyle.Fill };
        using Form host = new()
        {
            ClientSize = new Size(1112, 610),
            FormBorderStyle = FormBorderStyle.None,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        host.Controls.Add(tester);
        host.Show();
        PerformLayoutTree(host);
        ResponsiveRtuConfigurationControl? rtu = Find<ResponsiveRtuConfigurationControl>(tester);
        TableLayoutPanel? root = tester.Controls.Find("rtuConfigurationLayout", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        TableLayoutPanel? fields = tester.Controls.Find("rtuFieldGrid", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        TableLayoutPanel? actions = tester.Controls.Find("rtuActionGrid", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        bool result = rtu is not null
            && root is { AutoSize: true, AutoSizeMode: AutoSizeMode.GrowAndShrink }
            && root.RowStyles.Cast<RowStyle>().All(style => style.SizeType == SizeType.AutoSize)
            && fields is { AutoSize: true }
            && actions is { AutoSize: true }
            && fields.RowStyles.Cast<RowStyle>().All(style => style.SizeType == SizeType.AutoSize)
            && actions.RowStyles.Cast<RowStyle>().All(style => style.SizeType == SizeType.AutoSize)
            && Math.Abs(rtu.Height - rtu.PreferredLayoutHeight) <= IndustrialSpacing.Sm;
        if (!result)
        {
            throw new InvalidOperationException(
                $"rtu={rtu?.Bounds}; preferred={rtu?.PreferredLayoutHeight}; root={root?.Bounds}/pref={root?.PreferredSize}; "
                + $"fields={fields?.Bounds}/pref={fields?.PreferredSize}/rows={string.Join(',', fields?.RowStyles.Cast<RowStyle>().Select(style => $"{style.SizeType}:{style.Height}") ?? [])}; "
                + $"actions={actions?.Bounds}/pref={actions?.PreferredSize}/rows={string.Join(',', actions?.RowStyles.Cast<RowStyle>().Select(style => $"{style.SizeType}:{style.Height}") ?? [])}.");
        }

        return true;
    }

    private static bool TesterRtuUsesMeasuredRuntimeHeight()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session) { Dock = DockStyle.Fill };
        using Form host = CreateOffscreenHost(tester, new Size(1112, 625));
        host.Show();
        PerformLayoutTree(host);

        ResponsiveRtuConfigurationControl? rtu = Find<ResponsiveRtuConfigurationControl>(tester);
        TableLayoutPanel? root = tester.Controls.Find("rtuConfigurationLayout", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        TableLayoutPanel? header = tester.Controls.Find("rtuConfigurationSectionHeader", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        TableLayoutPanel? fields = tester.Controls.Find("rtuFieldGrid", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        TableLayoutPanel? actions = tester.Controls.Find("rtuActionGrid", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        Label? status = tester.Controls.Find("testerOfflineSafetyStatus", true)
            .OfType<Label>()
            .SingleOrDefault();
        if (rtu is null || root is null || header is null || fields is null || actions is null || status is null)
        {
            return false;
        }

        TableLayoutPanel[] fieldContainers = fields.Controls.OfType<TableLayoutPanel>().ToArray();
        bool rowContracts = header.RowStyles.Count == 3
            && header.RowStyles[0].SizeType == SizeType.AutoSize
            && header.RowStyles[1].SizeType == SizeType.AutoSize
            && header.RowStyles[2].SizeType == SizeType.Absolute
            && header.RowStyles[2].Height >= 1F
            && fieldContainers.Length == 10
            && fieldContainers.All(container =>
                container.AutoSize
                && container.RowStyles.Cast<RowStyle>().All(style => style.SizeType == SizeType.AutoSize));
        Control[] critical = [header, fields, actions, status];
        bool contained = critical.All(control =>
            control.Visible && IsContainedByAllAncestors(control, rtu));
        Rectangle actionBounds = BoundsRelativeTo(actions, rtu);
        Rectangle statusBounds = BoundsRelativeTo(status, rtu);
        bool result = rowContracts
            && contained
            && rtu.Height >= rtu.PreferredLayoutHeight - 1
            && root.Height >= root.PreferredSize.Height
            && actionBounds.Bottom <= statusBounds.Top
            && SingleLineTextFits(status);
        if (!result)
        {
            throw new InvalidOperationException(
                $"rtu={rtu.Bounds}/preferred={rtu.PreferredLayoutHeight}; root={root.Bounds}/preferred={root.PreferredSize}; "
                + $"header={header.Bounds}; fields={fields.Bounds}; actions={actions.Bounds}; status={status.Bounds}; "
                + $"rows={rowContracts}; contained={contained}; statusText={SingleLineTextFits(status)}.");
        }

        return true;
    }

    private static bool TesterRuntimeRegionsDoNotOverlap()
    {
        foreach (Size viewport in new[]
                 {
                     new Size(1366, 768),
                     new Size(1600, 900),
                     new Size(1920, 1080),
                     new Size(1920, 1200),
                     new Size(2560, 1440)
                 })
        {
            using IndustrialPlatformForm form = CreateOffscreenForm(viewport);
            form.ShowTester();
            form.Show();
            PerformLayoutTree(form);
            IndustrialTesterControl? tester = form.TesterInstance;
            if (tester is null)
            {
                return false;
            }

            Control? hero = tester.Controls.Find("testerHero", true).SingleOrDefault();
            Control? rtu = tester.Controls.Find("rtuConfigurationPanel", true).SingleOrDefault();
            Control? equipment = tester.Controls.Find("testerStatus", true).SingleOrDefault();
            IndustrialTabControl? tabs = tester.Controls.Find("testerTabs", true)
                .OfType<IndustrialTabControl>()
                .SingleOrDefault();
            IndustrialScrollPanel? scroll = tester.Controls.Find("testerMainScrollHost", true)
                .OfType<IndustrialScrollPanel>()
                .SingleOrDefault();
            if (hero is null || rtu is null || equipment is null || tabs is null || scroll is null)
            {
                return false;
            }

            Rectangle[] regions = [
                BoundsRelativeTo(hero, tester),
                BoundsRelativeTo(rtu, tester),
                BoundsRelativeTo(equipment, tester),
                BoundsRelativeTo(tabs, tester)
            ];
            bool ordered = regions.Zip(regions.Skip(1), (top, bottom) => top.Bottom <= bottom.Top)
                .All(value => value);
            bool tabsFit = tabs.TabCount == 6
                && Enumerable.Range(0, tabs.TabCount)
                    .Select(tabs.GetTabRect)
                    .All(tab => tabs.ClientRectangle.Contains(tab));
            if (!ordered || !tabsFit || scroll.HasHorizontalScroll)
            {
                throw new InvalidOperationException(
                    $"viewport={viewport.Width}x{viewport.Height}; regions={string.Join(" | ", regions.AsEnumerable())}; "
                    + $"tabsFit={tabsFit}; horizontal={scroll.HasHorizontalScroll}.");
            }
        }

        return true;
    }

    private static Form CreateOffscreenHost(Control content, Size clientSize)
    {
        Form host = new()
        {
            ClientSize = clientSize,
            FormBorderStyle = FormBorderStyle.None,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        host.Controls.Add(content);
        return host;
    }

    private static bool ProductionActionsAndCardsUseModerateRoundedCorners()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowTester();
        form.ShowHome();
        form.Show();
        PerformLayoutTree(form);
        IndustrialButton[] buttons = FindAll<IndustrialButton>(form).ToArray();
        IndustrialSurfacePanel[] cards = FindAll<IndustrialSurfacePanel>(form).ToArray();
        ResponsiveRtuConfigurationControl? rtu = Find<ResponsiveRtuConfigurationControl>(form);
        return buttons.Length >= 5
            && buttons.All(button => button.Region is not null
                && button.FlatStyle == FlatStyle.Flat
                && button.TextAlign is ContentAlignment.MiddleCenter or ContentAlignment.MiddleLeft)
            && cards.Length >= 2
            && cards.All(card => card.Region is not null)
            && rtu?.Region is not null;
    }

    private static bool NavigationUsesTaskManagerSelectionContract()
    {
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        return form.TesterModeButton is IndustrialButton tester
            && form.HomeModeButton is IndustrialButton home
            && form.SimulatorModeButton is IndustrialButton simulator
            && tester.IsNavigation
            && home.IsNavigation
            && simulator.IsNavigation
            && tester.IsSelected
            && !home.IsSelected
            && !simulator.IsSelected
            && tester.FlatAppearance.BorderSize == 0
            && tester.TextAlign == ContentAlignment.MiddleLeft
            && tester.BackColor == IndustrialTheme.Palette.SelectedSurface;
    }

    private static bool UnknownInputStateIsNeutral()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session);
        Label[] unknown = FindAll<Label>(tester)
            .Where(label => label.Text.Contains("Desconhecido", StringComparison.Ordinal))
            .ToArray();
        return unknown.Length == 11
            && unknown.All(label => label.Tag is PlatformStatusTone.Disabled
                && label.BackColor != IndustrialTheme.Palette.SuccessSurface
                && label.ForeColor != IndustrialTheme.Palette.Success);
    }

    private static bool TesterSignalRowsUseCompactDensity()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session);
        FlowLayoutPanel[] rows = FindAll<FlowLayoutPanel>(tester)
            .Where(row => string.Equals(row.Tag as string, "signal-row", StringComparison.Ordinal))
            .ToArray();
        return rows.Length >= 11
            && rows.All(row => row.Height <= IndustrialPlatformForm.ScaleLogicalMetric(38, row.DeviceDpi)
                && row.Margin.Bottom <= IndustrialPlatformForm.ScaleLogicalMetric(4, row.DeviceDpi));
    }

    private static bool TesterSignalListsExposeAlignedColumns()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowTester();
        form.Show();
        PerformLayoutTree(form);
        FlowLayoutPanel? list = form.Controls.Find("digitalSignalList", true)
            .OfType<FlowLayoutPanel>()
            .SingleOrDefault();
        FlowLayoutPanel? header = list?.Controls.OfType<FlowLayoutPanel>()
            .SingleOrDefault(control => Equals(control.Tag, "signal-header"));
        FlowLayoutPanel[] rows = list?.Controls.OfType<FlowLayoutPanel>()
            .Where(control => Equals(control.Tag, "signal-row"))
            .ToArray() ?? [];
        if (list is null || header is null || rows.Length != 8 || header.Controls.Count != 3)
        {
            return false;
        }

        string[] labels = header.Controls.OfType<Label>().Select(label => label.Text).ToArray();
        return labels.SequenceEqual(["Canal", "Sinal", "Estado"], StringComparer.Ordinal)
            && rows.All(row => row.Controls.Count >= 3
                && Math.Abs(row.Controls[0].Left - header.Controls[0].Left) <= 1
                && Math.Abs(row.Controls[1].Left - header.Controls[1].Left) <= 1
                && Math.Abs(row.Controls[2].Left - header.Controls[2].Left) <= 1
                && row.Controls[2].Left > row.Controls[1].Left);
    }

    private static bool LightThemePreservesSurfaceDepth()
    {
        IndustrialPalette palette = IndustrialPalette.Light;
        Color[] surfaces =
        [
            palette.Background,
            palette.Surface,
            palette.SurfaceElevated,
            palette.SurfaceInteractive,
            palette.Field
        ];
        return surfaces.Distinct().Count() == surfaces.Length
            && palette.Background.R < palette.Surface.R
            && palette.Surface.R < palette.SurfaceElevated.R
            && palette.SurfaceInteractive.R < palette.Surface.R
            && palette.Field.R >= palette.SurfaceElevated.R
            && IndustrialTheme.ContrastRatio(palette.AccentText, palette.Accent) >= 4.5D
            && IndustrialTheme.ContrastRatio(palette.TextPrimary, palette.Background) >= 4.5D;
    }

    private static bool FooterHasOnlyProductRuntimeEvidence()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.Show();
        PerformLayoutTree(form);
        string footerText = string.Join(
            " | ",
            FindAll<Label>(form.FooterRegion).Select(label => label.Text));
        return footerText.Contains("Perfil:", StringComparison.Ordinal)
            && footerText.Contains("Transporte:", StringComparison.Ordinal)
            && footerText.Contains("Endereço:", StringComparison.Ordinal)
            && footerText.Contains("Física:", StringComparison.Ordinal)
            && footerText.Contains(".NET 10", StringComparison.Ordinal)
            && !footerText.Contains("UI2", StringComparison.OrdinalIgnoreCase)
            && !footerText.Contains("UI3", StringComparison.OrdinalIgnoreCase)
            && !footerText.Contains("UI4", StringComparison.OrdinalIgnoreCase);
    }

    private static bool SimulatorCriticalUiRemainsVisible()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowSimulator();
        form.Show();
        PerformLayoutTree(form);
        PivotProcessControl? pivot = Find<PivotProcessControl>(form);
        Label? safety = form.Controls.Find("simulationSafetyStatus", true).OfType<Label>().FirstOrDefault();
        return pivot is { Visible: true, Width: >= 420, Height: >= 240 }
            && IsInsideParent(pivot)
            && safety is { Visible: true, Width: >= 180, Height: >= 30 }
            && IsInsideParent(safety)
            && safety.Text.Contains("READ-ONLY", StringComparison.Ordinal);
    }

    private static bool ShellSecondaryHeaderTextFits()
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            foreach (IndustrialThemeMode mode in new[] { IndustrialThemeMode.Dark, IndustrialThemeMode.Light })
            {
                using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
                form.SetTheme(mode);
                form.Show();
                foreach (Action showPage in new Action[] { form.ShowHome, form.ShowTester, form.ShowSimulator })
                {
                    showPage();
                    PerformLayoutTree(form);
                    if (form.BrandContext.Visible
                        && (!SingleLineTextFits(form.BrandContext)
                            || !IsContainedByAllAncestors(form.BrandContext, form.HeaderRegion)))
                    {
                        return false;
                    }

                    if (form.EquipmentStatus.Visible
                        && (!SingleLineTextFits(form.EquipmentStatus)
                            || !IsContainedByAllAncestors(form.EquipmentStatus, form.HeaderRegion)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static bool SimulatorMetricsRemainVisible()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session) { ClientSize = new Size(1040, 610) };
        using Form host = new()
        {
            ClientSize = simulator.ClientSize,
            FormBorderStyle = FormBorderStyle.None,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        simulator.Dock = DockStyle.Fill;
        host.Controls.Add(simulator);
        host.Show();
        PerformLayoutTree(host);
        TableLayoutPanel? metrics = simulator.Controls.Find("simulationMetricsGrid", true)
            .OfType<TableLayoutPanel>()
            .SingleOrDefault();
        return metrics is { Visible: true, AutoScroll: false }
            && simulator.MetricCount == 4
            && !simulator.MetricsUseHorizontalScroll
            && metrics.Controls.Cast<Control>().All(card => card.Visible && IsInsideParent(card));
    }

    private static bool SimulatorLightThemeHasEffectiveContrast()
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            IndustrialTheme.SetMode(IndustrialThemeMode.Light);
            using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
            using IndustrialSimulatorControl simulator = new(session) { ClientSize = new Size(1040, 610) };
            simulator.ApplyTheme();
            IReadOnlyList<CheckBox> editors = FindAll<CheckBox>(simulator);
            return editors.Count > 0
                && editors.All(editor =>
                    IndustrialTheme.ContrastRatio(editor.ForeColor, EffectiveBackground(editor)) >= 4.5D);
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static bool SimulatorAvoidsNestedScrollRegions()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        ScrollableControl[] scrolling = FindAll<ScrollableControl>(simulator)
            .Where(control => control.AutoScroll)
            .ToArray();
        return scrolling.Length <= 2
            && scrolling.All(control => !scrolling.Any(other =>
                !ReferenceEquals(control, other) && IsDescendantOf(control, other)));
    }

    private static bool ProductionScrollbarsUseSlimThemedChrome()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowTester();
        form.ShowSimulator();
        form.Show();
        PerformLayoutTree(form);
        IndustrialScrollPanel[] panels = FindAll<IndustrialScrollPanel>(form).ToArray();
        IndustrialFlowLayoutPanel[] flows = FindAll<IndustrialFlowLayoutPanel>(form).ToArray();
        TextBox? log = FindAll<TextBox>(form).FirstOrDefault(textBox =>
            textBox.Multiline
            && string.Equals(textBox.AccessibleName, "Log técnico incremental do Testador", StringComparison.Ordinal));
        IndustrialPalette palette = IndustrialTheme.Palette;
        bool result = panels.Length >= 2
            && flows.Length >= 4
            && panels.All(panel => panel.UsesSlimThemedScrollbar && !panel.HasHorizontalScroll)
            && flows.All(flow => flow.UsesSlimThemedScrollbar && !flow.HasHorizontalScroll)
            && IndustrialScrollPanel.ScrollbarThumbWidth is > 0 and <= 6
            && IndustrialFlowLayoutPanel.ScrollbarThumbWidth == IndustrialScrollPanel.ScrollbarThumbWidth
            && palette.ScrollThumb != palette.Background
            && palette.ScrollThumb != palette.Surface
            && palette.ScrollThumbHover != palette.ScrollThumb
            && log is { ScrollBars: ScrollBars.Vertical, WordWrap: true };
        if (!result)
        {
            throw new InvalidOperationException(
                $"panels={panels.Length} [{string.Join(",", panels.Select(panel => $"{panel.Name}:h={panel.HasHorizontalScroll}:v={panel.IsVerticalScrollRequired}"))}]; "
                + $"flows={flows.Length} [{string.Join(",", flows.Select(flow => $"{flow.Name}:h={flow.HasHorizontalScroll}:v={flow.IsVerticalScrollRequired}"))}]; "
                + $"thumb={IndustrialScrollPanel.ScrollbarThumbWidth}; log={log?.ScrollBars}/{log?.WordWrap}.");
        }

        return true;
    }

    private static bool TesterAvoidsActiveNestedScrollAtStandardViewport()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowTester();
        form.Show();
        PerformLayoutTree(form);
        IndustrialScrollPanel[] panels = FindAll<IndustrialScrollPanel>(form)
            .Where(panel => panel.Visible)
            .ToArray();
        IndustrialFlowLayoutPanel[] flows = FindAll<IndustrialFlowLayoutPanel>(form)
            .Where(flow => flow.Visible)
            .ToArray();
        ScrollableControl[] active = panels.Where(panel => panel.IsVerticalScrollRequired)
            .Cast<ScrollableControl>()
            .Concat(flows.Where(flow => flow.IsVerticalScrollRequired))
            .ToArray();
        IndustrialScrollPanel? main = panels.SingleOrDefault(panel =>
            string.Equals(panel.Name, "testerMainScrollHost", StringComparison.Ordinal));
        bool result = main is { IsVerticalScrollRequired: false }
            && active.All(control => !active.Any(other =>
                !ReferenceEquals(control, other) && IsDescendantOf(control, other)));
        if (!result)
        {
            throw new InvalidOperationException(
                $"main={main?.IsVerticalScrollRequired}; active="
                + string.Join(",", active.Select(control => $"{control.Name}:{control.GetType().Name}")));
        }

        return true;
    }

    private static bool TesterTabStripRemainderMatchesTheme()
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            foreach (IndustrialThemeMode mode in new[] { IndustrialThemeMode.Dark, IndustrialThemeMode.Light })
            {
                IndustrialTheme.SetMode(mode);
                using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
                using IndustrialTesterControl tester = new(session);
                IndustrialTabControl? tabs = Find<IndustrialTabControl>(tester);
                if (tabs is null || tabs.HeaderRemainderColor != IndustrialTheme.Palette.Background)
                {
                    return false;
                }
            }

            return true;
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static bool SimulatorConfigurationFitsStandardViewport()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowSimulator();
        form.Show();
        PerformLayoutTree(form);
        IndustrialSimulatorControl? simulator = Find<IndustrialSimulatorControl>(form);
        return simulator is not null
            && !simulator.ConfigurationUsesScroll
            && simulator.ConfigurationContentFits;
    }

    private static bool SimulatorSignalLayoutIsResponsive()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session) { ClientSize = new Size(1120, 610) };
        using Form host = new()
        {
            ClientSize = simulator.ClientSize,
            FormBorderStyle = FormBorderStyle.None,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        simulator.Dock = DockStyle.Fill;
        host.Controls.Add(simulator);
        host.Show();
        PerformLayoutTree(host);
        bool standard = simulator.SignalColumnCount == 2 && simulator.SignalItemsAreCompact;
        host.ClientSize = new Size(1600, 900);
        PerformLayoutTree(host);
        bool wide = simulator.SignalColumnCount == 3 && simulator.SignalItemsAreCompact;
        Control? derived = simulator.Controls.Find("simulationSafetyChainSignalSurface", true).FirstOrDefault();
        bool derivedReadOnly = derived?.Parent is TableLayoutPanel grid
            && grid.GetColumnSpan(derived) == grid.ColumnCount
            && derived.Controls.Find("simulationSafetyChainInput", true).Length == 0;
        return standard && wide && derivedReadOnly;
    }

    private static bool SimulatorMetricsExposeVisualHierarchy()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        Panel[] cards = FindAll<Panel>(simulator)
            .Where(panel => panel.Name.StartsWith("simulationMetricCard", StringComparison.Ordinal))
            .ToArray();
        return cards.Length == 4
            && cards.All(card => card.Controls.OfType<TableLayoutPanel>().SingleOrDefault() is
                { Name: "simulationMetricHierarchy", RowCount: 3 } hierarchy
                && hierarchy.Controls.OfType<Label>().Count() == 3);
    }

    private static bool PrimaryActionsRemainAligned()
    {
        using IndustrialPlatformForm form = CreateOffscreenForm(new Size(1366, 768));
        form.ShowTester();
        form.ShowSimulator();
        form.ShowHome();
        form.Show();
        PerformLayoutTree(form);
        string[][] groups =
        [
            ["welcomeTesterButton", "welcomeSimulatorButton"],
            ["validateRtuButton", "identifyButton", "discoverButton", "cancelButton"],
            ["applyScenarioButton", "resetScenarioButton"]
        ];
        foreach (string[] group in groups)
        {
            Button[] buttons = group
                .Select(name => form.Controls.Find(name, true).OfType<Button>().SingleOrDefault())
                .Where(button => button is not null)
                .Cast<Button>()
                .ToArray();
            if (buttons.Length != group.Length
                || buttons.Select(button => button.Height).Distinct().Count() != 1
                || buttons.Any(button => button.Height < 36 || !SingleLineTextFits(button)))
            {
                return false;
            }
        }

        return true;
    }

    private static Color EffectiveBackground(Control control)
    {
        for (Control? current = control; current is not null; current = current.Parent)
        {
            Color color = current.BackColor;
            if (color.A == byte.MaxValue && color != Color.Transparent)
            {
                return color;
            }
        }

        return IndustrialTheme.Palette.Background;
    }

    private static bool IsDescendantOf(Control control, Control possibleAncestor)
    {
        for (Control? parent = control.Parent; parent is not null; parent = parent.Parent)
        {
            if (ReferenceEquals(parent, possibleAncestor))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsContainedByAllAncestors(Control control, Control stopAt)
    {
        for (Control current = control; current.Parent is not null; current = current.Parent)
        {
            if (!current.Parent.ClientRectangle.Contains(current.Bounds))
            {
                return false;
            }

            if (ReferenceEquals(current.Parent, stopAt))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SingleLineTextFits(Control control)
    {
        Size measured = TextRenderer.MeasureText(
            control.Text,
            control.Font,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        return measured.Width <= Math.Max(0, control.ClientSize.Width - control.Padding.Horizontal - 8)
            && measured.Height <= Math.Max(0, control.ClientSize.Height - control.Padding.Vertical);
    }

    private static bool CriticalAccessibleNamesArePresent()
    {
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        form.ShowSimulator();
        string[] names =
        [
            "industrialBrand",
            "industrialThemeSelector",
            "modeHomeButton",
            "modeTesterButton",
            "modeSimulatorButton",
            "platformOfflineStatus",
            "platformSafetyStatus",
            "testerOfflineSafetyStatus",
            "simulationSafetyStatus",
            "pivotProcessControl",
            "ioMappingGrid"
        ];
        return names
            .Select(name => form.Controls.Find(name, true).FirstOrDefault())
            .All(control => control is not null
                && !string.IsNullOrWhiteSpace(control.AccessibleName));
    }

    private static bool NavigationThemeAndResizeKeepStructureStable()
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            using IndustrialPlatformForm form = new();
            form.ShowTester();
            IndustrialTesterControl tester = form.TesterInstance!;
            form.ShowSimulator();
            IndustrialSimulatorControl simulator = form.SimulatorInstance!;
            int count = CountControls(form);
            for (int index = 0; index < 10; index++)
            {
                form.ClientSize = index % 2 == 0 ? new Size(1024, 680) : new Size(1366, 768);
                form.SetTheme(index % 2 == 0 ? IndustrialThemeMode.Light : IndustrialThemeMode.Dark);
                form.ShowTester();
                form.ShowSimulator();
                PerformLayoutTree(form);
            }

            return ReferenceEquals(tester, form.TesterInstance)
                && ReferenceEquals(simulator, form.SimulatorInstance)
                && CountControls(form) == count
                && simulator.SignalStructureBuildCount == 1;
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static IndustrialPlatformForm CreateOffscreenForm(Size clientSize) => new()
    {
        MinimumSize = Size.Empty,
        ClientSize = clientSize,
        Opacity = 0,
        ShowInTaskbar = false,
        StartPosition = FormStartPosition.Manual,
        Location = new Point(-32000, -32000)
    };

    private static bool IsInsideParent(Control control) =>
        control.Parent is not null
        && control.Parent.ClientRectangle.Contains(control.Bounds);

    private static Rectangle BoundsRelativeTo(Control control, Control root)
    {
        Point location = control.Location;
        for (Control? parent = control.Parent; parent is not null && !ReferenceEquals(parent, root); parent = parent.Parent)
        {
            location.Offset(parent.Location);
        }

        return new Rectangle(location, control.Size);
    }

    private static bool Ui2ControlsRespectDpiScalingContract()
    {
        using IndustrialPlatformForm form = new();
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session);
        using IndustrialSimulatorControl simulator = new(session);
        if (form.AutoScaleMode != AutoScaleMode.Dpi
            || tester.AutoScaleMode != AutoScaleMode.Dpi
            || simulator.AutoScaleMode != AutoScaleMode.Dpi)
        {
            return false;
        }

        using IndustrialLedIndicatorControl led = new() { LabelText = "DI00", IsOn = true };
        using IndustrialPushButtonControl button = new() { Title = "DO00", IsActive = true };
        foreach (float scale in new[] { 1F, 1.25F, 1.5F })
        {
            led.ClientSize = new Size((int)Math.Round(160F * scale), (int)Math.Round(44F * scale));
            button.ClientSize = new Size((int)Math.Round(160F * scale), (int)Math.Round(44F * scale));
            using Bitmap ledBitmap = new(led.ClientSize.Width, led.ClientSize.Height);
            using Bitmap buttonBitmap = new(button.ClientSize.Width, button.ClientSize.Height);
            led.DrawToBitmap(ledBitmap, led.ClientRectangle);
            button.DrawToBitmap(buttonBitmap, button.ClientRectangle);
        }

        return led.Font.SizeInPoints >= 8.5F
            && button.Font.SizeInPoints >= 8.5F;
    }

    private static bool ThemeSwitchPreservesUiState()
    {
        IndustrialTheme.SetMode(IndustrialThemeMode.Dark);
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        IndustrialTesterControl tester = form.TesterInstance!;
        tester.SelectView(5);
        _ = form.Session.IdentifyAsync(1, CancellationToken.None).GetAwaiter().GetResult();
        form.Session.ApplyScenario("falha-torre");
        string log = tester.RenderedLogText;
        double safetyChain = form.Session.Simulation.GetValue("SafetyChain");
        form.ShowSimulator();
        IndustrialSimulatorControl simulator = form.SimulatorInstance!;
        int structureBuilds = simulator.SignalStructureBuildCount;
        int editorCount = simulator.EditableSignalCount;
        int controlCount = CountControls(form);

        foreach (IndustrialThemeMode mode in new[]
                 {
                     IndustrialThemeMode.Dark,
                     IndustrialThemeMode.Light,
                     IndustrialThemeMode.Dark,
                     IndustrialThemeMode.System,
                     IndustrialThemeMode.Dark
                 })
        {
            form.SetTheme(mode);
            form.ShowTester();
            form.ShowSimulator();
        }

        return ReferenceEquals(tester, form.TesterInstance)
            && ReferenceEquals(simulator, form.SimulatorInstance)
            && tester.GetSelectedViewIndex() == 5
            && tester.RenderedLogText == log
            && log.Length > 0
            && form.Session.Simulation.GetValue("SafetyChain") == safetyChain
            && simulator.SignalStructureBuildCount == structureBuilds
            && simulator.EditableSignalCount == editorCount
            && CountControls(form) == controlCount
            && form.ThemeMode == IndustrialThemeMode.Dark;
    }

    private static bool ThemeSelectorMatchesActiveTheme()
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            using IndustrialPlatformForm form = new();
            form.ShowSimulator();
            IndustrialSimulatorControl? simulator = form.SimulatorInstance;
            IndustrialPlatformSession session = form.Session;
            foreach ((IndustrialThemeMode mode, string text) in new[]
                     {
                         (IndustrialThemeMode.Dark, "Escuro"),
                         (IndustrialThemeMode.Light, "Claro"),
                         (IndustrialThemeMode.Dark, "Escuro"),
                         (IndustrialThemeMode.System, "Sistema"),
                         (IndustrialThemeMode.Dark, "Escuro")
                     })
            {
                form.SetTheme(mode);
                if (form.ThemeMode != mode
                    || form.ThemeSelector.Text != text
                    || !ReferenceEquals(simulator, form.SimulatorInstance)
                    || !ReferenceEquals(session, form.Session))
                {
                    return false;
                }
            }

            return true;
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static bool ThemeAndPivotKeepGdiResourcesStable()
    {
        RenderThemeAndPivotCycles(8);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        int afterWarmup = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
        RenderThemeAndPivotCycles(8);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        int afterRepeat = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
        IndustrialTheme.SetMode(IndustrialThemeMode.Dark);
        return afterWarmup > 0 && afterRepeat - afterWarmup <= 4;
    }

    private static void RenderThemeAndPivotCycles(int iterations)
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        PivotProcessControl pivot = Find<PivotProcessControl>(simulator)!;
        for (int index = 0; index < iterations; index++)
        {
            IndustrialTheme.SetMode(index % 2 == 0
                ? IndustrialThemeMode.Light
                : IndustrialThemeMode.Dark);
            pivot.ApplyTheme();
            pivot.Size = new Size(560 + index, 360 + index);
            using Bitmap bitmap = new(pivot.Width, pivot.Height);
            pivot.DrawToBitmap(bitmap, pivot.ClientRectangle);
        }
    }

    private static bool SimulatorSectionsUseThemedChrome()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        GroupBox[] sections = FindAll<GroupBox>(simulator).ToArray();
        return sections.Length >= 2
            && sections.All(section => section is IndustrialGroupBox)
            && sections.All(section => section.Padding.Top >= IndustrialSpacing.Xxl);
    }

    private static bool FlatSectionsAvoidNestedChrome()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        IndustrialGroupBox[] sections = FindAll<IndustrialGroupBox>(simulator).ToArray();
        IndustrialGroupBox[] flat = sections.Where(section => section.IsFlatSection).ToArray();
        return flat.Length >= 2
            && flat.All(section => section.Parent is TableLayoutPanel)
            && sections.Any(section => !section.IsFlatSection);
    }

    private static bool SignalRowsAvoidExcessiveBoxing()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        Panel[] rows = FindAll<Panel>(simulator)
            .Where(panel => string.Equals(panel.Tag as string, "signal-row-flat", StringComparison.Ordinal))
            .ToArray();
        return rows.Length >= 6
            && rows.All(row => row.Margin == Padding.Empty)
            && rows.All(row => row.Height >= IndustrialPlatformForm.ScaleLogicalMetric(32, row.DeviceDpi)
                && row.Height <= IndustrialPlatformForm.ScaleLogicalMetric(36, row.DeviceDpi))
            && rows.All(row => row.Parent is not null && row.BackColor == row.Parent.BackColor);
    }

    private static bool OperatorTextIsConsistent()
    {
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        form.ShowSimulator();
        string visibleText = string.Join(
            "\n",
            FindAll<Control>(form)
                .Select(control => control.Text));
        return visibleText.Contains("Testador Industrial HI", StringComparison.Ordinal)
            && visibleText.Contains("SIMULAÇÃO", StringComparison.Ordinal)
            && !visibleText.Contains("TESTADOR CLP", StringComparison.OrdinalIgnoreCase)
            && !visibleText.Contains("SIMULATION_READY", StringComparison.OrdinalIgnoreCase)
            && !visibleText.Contains("DEVELOPMENT", StringComparison.OrdinalIgnoreCase);
    }

    private static void PerformLayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls)
        {
            PerformLayoutTree(child);
        }
    }

    private static bool SessionUsesFakeAddressOne()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        return session.Device.Address == 1;
    }

    private static bool PivotProfileIsInteractive()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        session.SetDigitalInput("Emergencia", true);
        bool emergency = session.Simulation.Snapshot.OutputsBlocked;
        session.ResetSimulation();
        return emergency && !session.Simulation.Snapshot.OutputsBlocked;
    }

    private static bool WellProfileLoads()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("poco"));
        session.ApplyScenario("falta-fase");
        return session.Simulation.Snapshot.State == "Falha";
    }

    private static bool TesterIdentifiesFake()
    {
        return Task.Run(async () =>
        {
            using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
            RtuIdentificationResult result = await session.IdentifyAsync(1, CancellationToken.None);
            return result.State == RtuIdentificationState.Identified;
        }).GetAwaiter().GetResult();
    }

    private static bool SimulatedOutputIsMomentary()
    {
        return Task.Run(async () =>
        {
            using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
            await session.IdentifyAsync(1, CancellationToken.None);
            RtuOutputTestResult result = await session.ActivateOutputAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(2),
                operatorExplicitlyEnabled: true,
                CancellationToken.None);
            return result.State == RtuOutputTestState.Completed
                && result.TurnOffConfirmed
                && !session.Device.GetDigitalOutput(Layout3OutputChannel.DO00);
        }).GetAwaiter().GetResult();
    }

    private static bool NewLayersHaveNoPhysicalTransportTypes()
    {
        Type[] types = typeof(IndustrialPlatformSession).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith(
                "TestadorCLPHI.App.Industrial.Platform",
                StringComparison.Ordinal) == true
                || type.Namespace?.StartsWith(
                    "TestadorCLPHI.App.Ui.Industrial.Platform",
                    StringComparison.Ordinal) == true)
            .ToArray();
        string[] forbidden = ["System.IO.Ports", "System.Net.Sockets", "TcpClient", "SerialPort"];
        return types.SelectMany(type => type.GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .All(field => forbidden.All(item =>
                field.FieldType.FullName?.Contains(item, StringComparison.OrdinalIgnoreCase) != true));
    }

    private static bool PhysicalCountersRemainZero()
    {
        return Task.Run(async () =>
        {
            using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
            await session.IdentifyAsync(1, CancellationToken.None);
            return PhysicalCountersAreZero(session);
        }).GetAwaiter().GetResult();
    }

    private static bool SharedAssemblyGraphIsAcyclic()
    {
        string coreName = typeof(SimulationEngine).Assembly.GetName().Name!;
        string applicationName = typeof(IndustrialPlatformSession).Assembly.GetName().Name!;
        string desktopName = typeof(IndustrialPlatformUiValidator).Assembly.GetName().Name!;
        string[] coreReferences = typeof(SimulationEngine).Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name!)
            .ToArray();
        string[] applicationReferences = typeof(IndustrialPlatformSession).Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name!)
            .ToArray();

        return !coreReferences.Contains(applicationName, StringComparer.Ordinal)
            && !coreReferences.Contains(desktopName, StringComparer.Ordinal)
            && applicationReferences.Contains(coreName, StringComparer.Ordinal)
            && !applicationReferences.Contains(desktopName, StringComparer.Ordinal);
    }

    private static bool DesktopReferencesSharedLayers()
    {
        string[] references = typeof(IndustrialPlatformUiValidator).Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name!)
            .ToArray();
        return references.Contains(
                typeof(SimulationEngine).Assembly.GetName().Name!,
                StringComparer.Ordinal)
            && references.Contains(
                typeof(IndustrialPlatformSession).Assembly.GetName().Name!,
                StringComparer.Ordinal);
    }

    private static bool PivotRendererUsesUi2VisualLanguage()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        PivotProcessControl? pivot = Find<PivotProcessControl>(simulator);
        session.ApplyScenario("falha-torre");
        return pivot is
            {
                UsesUi2Theme: true,
                HasTowerStatusCards: true,
                UsesContinuousAnimation: false
            }
            && pivot.AccessibleDescription?.Contains("FALHA", StringComparison.Ordinal) == true;
    }

    private static bool IoMapFiltersWithoutChangingBindings()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialIoMapControl map = new(session);
        int total = map.TotalBindingCount;
        map.Filter("31120", "DI");
        bool narrowed = map.BindingRowCount == 1;
        map.Filter(string.Empty, null);
        return total == session.Profile.IoBindings.Count
            && narrowed
            && map.BindingRowCount == total
            && map.AvailableTypeFilters.Contains("DI")
            && map.AvailableTypeFilters.Contains("AI")
            && map.AvailableTypeFilters.Contains("DO")
            && !map.HasUnsupportedAnalogOutputBadge;
    }

    private static bool SimulatorFeatureParityIsPreserved()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialSimulatorControl simulator = new(session);
        int buildCount = simulator.SignalStructureBuildCount;
        int editorCount = simulator.EditableSignalCount;
        session.ApplyScenario("movendo-frente");
        session.ApplyScenario("falha-torre");
        session.ResetSimulation();
        return simulator.Controls.Find("applyScenarioButton", searchAllChildren: true).Length == 1
            && simulator.Controls.Find("resetScenarioButton", searchAllChildren: true).Length == 1
            && simulator.Controls.Find("simulationSafetyStatus", searchAllChildren: true).Length == 1
            && buildCount == 1
            && simulator.SignalStructureBuildCount == buildCount
            && editorCount > 0
            && simulator.EditableSignalCount == editorCount;
    }

    private static bool TesterFeatureParityIsPreserved()
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        using IndustrialTesterControl tester = new(session);
        string[] actions =
        [
            "refreshPortsButton",
            "validateRtuButton",
            "identifyButton",
            "discoverButton",
            "cancelButton",
            "readInputsButton",
            "activateDO00Button",
            "turnOffDO00Button"
        ];
        string[] pages =
        [
            "Entradas digitais",
            "Entradas analógicas",
            "Saídas digitais",
            "Mapa de I/O",
            "Diagnóstico",
            "Log"
        ];
        TabControl? tabs = Find<TabControl>(tester);
        return actions.All(name => tester.Controls.Find(name, searchAllChildren: true).Length == 1)
            && tabs is not null
            && pages.All(page => tabs.TabPages.Cast<TabPage>().Any(tab => tab.Text == page))
            && tester.UsesIncrementalLogUpdates;
    }

    private static bool PhysicalCountersAreZero(IndustrialPlatformSession session) =>
        session.Counters.PhysicalConnections == 0
        && session.Counters.PhysicalReads == 0
        && session.Counters.PhysicalWrites == 0
        && session.Counters.PhysicalCommands == 0
        && session.Simulation.Counters.PhysicalConnections == 0
        && session.Simulation.Counters.PhysicalReads == 0
        && session.Simulation.Counters.PhysicalWrites == 0
        && session.Simulation.Counters.PhysicalCommands == 0;

    private static CanonicalCounters CaptureCanonicalCounters() => Task.Run(async () =>
    {
        using IndustrialPlatformSession session = new(SimulationProfileLoader.Load("pivo-central"));
        await session.DiscoverAsync(1, 1, TimeSpan.Zero, progress: null, CancellationToken.None);
        await session.ReadInputsAsync(1, CancellationToken.None);
        await session.ActivateOutputAsync(
            1,
            Layout3OutputChannel.DO00,
            TimeSpan.FromMilliseconds(2),
            operatorExplicitlyEnabled: true,
            CancellationToken.None);
        return new CanonicalCounters(
            session.Counters.SimulatedConnections,
            session.Counters.SimulatedReads,
            session.Counters.SimulatedWrites,
            session.Counters.SimulatedCommands,
            session.Simulation.Counters.SimulatedOperations,
            session.Simulation.Counters.SimulatedCommands);
    }).GetAwaiter().GetResult();

    private static T? Find<T>(Control root)
        where T : Control
    {
        if (root is T match)
        {
            return match;
        }

        foreach (Control child in root.Controls)
        {
            T? found = Find<T>(child);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static IReadOnlyList<T> FindAll<T>(Control root)
        where T : Control
    {
        List<T> matches = [];
        Collect(root, matches);
        return matches;

        static void Collect(Control control, List<T> items)
        {
            if (control is T match)
            {
                items.Add(match);
            }

            foreach (Control child in control.Controls)
            {
                Collect(child, items);
            }
        }
    }

    private static int CountControls(Control root) =>
        1 + root.Controls.Cast<Control>().Sum(CountControls);

    private sealed record CanonicalCounters(
        int RtuConnections,
        int RtuReads,
        int RtuWrites,
        int RtuCommands,
        int SimulationOperations,
        int SimulationCommands);
}
