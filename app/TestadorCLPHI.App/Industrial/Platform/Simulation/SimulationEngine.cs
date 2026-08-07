namespace TestadorCLPHI.App.Industrial.Platform.Simulation;

internal sealed class SimulationEngine
{
    private readonly SimulationProfile _profile;
    private readonly Dictionary<string, SimulationSignalDefinition> _definitions;
    private readonly Dictionary<string, double> _values;
    private readonly List<SimulationLogEntry> _log = [];
    private readonly HashSet<string> _activeAlarms = new(StringComparer.OrdinalIgnoreCase);
    private bool _outputsBlocked;

    internal SimulationEngine(SimulationProfile profile, SimulationCounters? counters = null)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        SimulationProfileValidationResult validation = SimulationProfileValidator.Validate(profile);
        if (!validation.IsValid)
        {
            throw new ArgumentException(validation.ToDisplayText(), nameof(profile));
        }

        Counters = counters ?? new SimulationCounters();
        _definitions = profile.Signals.ToDictionary(signal => signal.Id!, StringComparer.OrdinalIgnoreCase);
        _values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        Reset();
    }

    internal SimulationCounters Counters { get; }
    internal IReadOnlyList<SimulationLogEntry> Log => _log.ToArray();
    internal SimulationProfile Profile => _profile;

    internal SimulationSnapshot Snapshot => new(
        _profile.Id!,
        CurrentState,
        new Dictionary<string, double>(_values, StringComparer.OrdinalIgnoreCase),
        _activeAlarms.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
        _outputsBlocked);

    internal string CurrentState { get; private set; } = string.Empty;

    internal void Reset()
    {
        _values.Clear();
        foreach (SimulationSignalDefinition signal in _profile.Signals)
        {
            _values[signal.Id!] = signal.DefaultValue;
        }

        Evaluate();
        AddLog("RESET", string.Empty, null, "OK");
    }

    internal void ApplyScenario(string scenarioId)
    {
        SimulationScenario scenario = _profile.Scenarios.FirstOrDefault(item =>
                string.Equals(item.Id, scenarioId, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Cenario inexistente: {scenarioId}.", nameof(scenarioId));

        _values.Clear();
        foreach (SimulationSignalDefinition signal in _profile.Signals)
        {
            _values[signal.Id!] = signal.DefaultValue;
        }

        foreach ((string signalId, double value) in scenario.Values)
        {
            SetValue(signalId, value, evaluate: false, operation: "SCENARIO_VALUE");
        }

        Evaluate();
        AddLog("APPLY_SCENARIO", scenario.Id!, null, "OK");
    }

    internal void SetDigitalInput(string signalId, bool value) =>
        SetInput(signalId, SimulationSignalKind.DigitalInput, value ? 1 : 0);

    internal void SetAnalogInput(string signalId, double value) =>
        SetInput(signalId, SimulationSignalKind.AnalogInput, value);

    internal bool SetVirtualOutput(string signalId, bool value)
    {
        SimulationSignalDefinition definition = GetDefinition(signalId);
        if (definition.Kind != SimulationSignalKind.VirtualOutput)
        {
            throw new InvalidOperationException($"{signalId} nao e saida virtual.");
        }

        if (value && _outputsBlocked)
        {
            AddLog("OUTPUT_COMMAND", signalId, 1, "BLOCKED");
            Counters.RecordCommand();
            return false;
        }

        if (value)
        {
            foreach (SimulationExclusiveOutputGroup group in _profile.ExclusiveOutputGroups.Where(
                         group => group.SignalIds.Contains(signalId, StringComparer.OrdinalIgnoreCase)))
            {
                foreach (string otherSignal in group.SignalIds.Where(item =>
                             !string.Equals(item, signalId, StringComparison.OrdinalIgnoreCase)))
                {
                    _values[otherSignal] = 0;
                }
            }
        }

        SetValue(signalId, value ? 1 : 0, evaluate: true, operation: "OUTPUT_COMMAND");
        Counters.RecordCommand();
        return true;
    }

    internal double GetValue(string signalId) =>
        _values.TryGetValue(signalId, out double value)
            ? value
            : throw new ArgumentException($"Sinal inexistente: {signalId}.", nameof(signalId));

    private void SetInput(string signalId, SimulationSignalKind expectedKind, double value)
    {
        SimulationSignalDefinition definition = GetDefinition(signalId);
        if (definition.Kind != expectedKind)
        {
            throw new InvalidOperationException($"Tipo invalido para o sinal {signalId}.");
        }

        SetValue(signalId, value, evaluate: true, operation: "INPUT_CHANGE");
    }

    private void SetValue(string signalId, double value, bool evaluate, string operation)
    {
        SimulationSignalDefinition definition = GetDefinition(signalId);
        if (value < definition.Minimum || value > definition.Maximum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"{signalId} deve permanecer em {definition.Minimum}..{definition.Maximum}.");
        }

        if (definition.Kind is SimulationSignalKind.DigitalInput or SimulationSignalKind.VirtualOutput
            && value is not 0 and not 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"{signalId} deve ser 0 ou 1.");
        }

        _values[signalId] = value;
        Counters.RecordOperation();
        AddLog(operation, signalId, value, "OK");
        if (evaluate)
        {
            Evaluate();
        }
    }

    private void Evaluate()
    {
        _activeAlarms.Clear();
        _outputsBlocked = false;
        CurrentState = _profile.InitialState!;

        SimulationRule[] matchingRules = _profile.Rules
            .Where(rule => rule.Conditions.All(Matches))
            .OrderByDescending(rule => rule.Priority)
            .ToArray();
        if (matchingRules.Length > 0)
        {
            CurrentState = matchingRules[0].ResultState!;
        }

        foreach (SimulationRule rule in matchingRules)
        {
            if (!string.IsNullOrWhiteSpace(rule.AlarmId))
            {
                _activeAlarms.Add(rule.AlarmId);
            }

            _outputsBlocked |= rule.BlockAllOutputs;
        }

        if (_outputsBlocked)
        {
            foreach (SimulationSignalDefinition output in _profile.Signals.Where(signal =>
                         signal.Kind == SimulationSignalKind.VirtualOutput))
            {
                _values[output.Id!] = 0;
            }
        }
    }

    private bool Matches(SimulationCondition condition)
    {
        double actual = GetValue(condition.SignalId!);
        return condition.Comparison switch
        {
            SimulationComparison.Equals => actual.Equals(condition.ExpectedValue),
            SimulationComparison.NotEquals => !actual.Equals(condition.ExpectedValue),
            SimulationComparison.LessThan => actual < condition.ExpectedValue,
            SimulationComparison.LessThanOrEqual => actual <= condition.ExpectedValue,
            SimulationComparison.GreaterThan => actual > condition.ExpectedValue,
            SimulationComparison.GreaterThanOrEqual => actual >= condition.ExpectedValue,
            _ => false
        };
    }

    private SimulationSignalDefinition GetDefinition(string signalId) =>
        _definitions.TryGetValue(signalId, out SimulationSignalDefinition? definition)
            ? definition
            : throw new ArgumentException($"Sinal inexistente: {signalId}.", nameof(signalId));

    private void AddLog(string operation, string signal, double? value, string result) =>
        _log.Add(new SimulationLogEntry(
            DateTimeOffset.UtcNow,
            _profile.Id!,
            operation,
            signal,
            value,
            result,
            Simulated: true));
}

internal sealed record SimulationProfileValidationResult(IReadOnlyList<string> Failures)
{
    internal bool IsValid => Failures.Count == 0;
    internal string ToDisplayText() => string.Join(Environment.NewLine, Failures);
}

internal static class SimulationProfileValidator
{
    internal static SimulationProfileValidationResult Validate(SimulationProfile profile)
    {
        List<string> failures = [];
        if (profile.SchemaVersion != 1)
        {
            failures.Add("schemaVersion deve ser 1.");
        }

        RequireText(profile.Id, "Id do perfil ausente.", failures);
        RequireText(profile.DisplayName, "Nome do perfil ausente.", failures);
        RequireText(profile.InitialState, "Estado inicial ausente.", failures);
        if (profile.Signals.Count == 0)
        {
            failures.Add("Perfil sem sinais.");
            return new(failures);
        }

        HashSet<string> signalIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (SimulationSignalDefinition signal in profile.Signals)
        {
            RequireText(signal.Id, "Sinal sem id.", failures);
            RequireText(signal.Label, $"Sinal {signal.Id}: label ausente.", failures);
            if (!string.IsNullOrWhiteSpace(signal.Id) && !signalIds.Add(signal.Id))
            {
                failures.Add($"Sinal duplicado: {signal.Id}.");
            }

            if (signal.Minimum > signal.Maximum
                || signal.DefaultValue < signal.Minimum
                || signal.DefaultValue > signal.Maximum)
            {
                failures.Add($"Faixa/default invalido em {signal.Id}.");
            }
        }

        ValidateVisualization(profile.Visualization, signalIds, failures);
        ValidateIoBindings(profile, signalIds, failures);

        foreach (SimulationRule rule in profile.Rules)
        {
            RequireText(rule.Id, "Regra sem id.", failures);
            RequireText(rule.ResultState, $"Regra {rule.Id}: estado ausente.", failures);
            if (rule.Conditions.Count == 0)
            {
                failures.Add($"Regra {rule.Id} sem condicoes.");
            }

            foreach (SimulationCondition condition in rule.Conditions)
            {
                if (string.IsNullOrWhiteSpace(condition.SignalId) || !signalIds.Contains(condition.SignalId))
                {
                    failures.Add($"Regra {rule.Id}: sinal inexistente {condition.SignalId ?? "AUSENTE"}.");
                }
            }
        }

        foreach (SimulationExclusiveOutputGroup group in profile.ExclusiveOutputGroups)
        {
            RequireText(group.Id, "Grupo exclusivo sem id.", failures);
            if (group.SignalIds.Count < 2 || group.SignalIds.Any(id =>
                    !signalIds.Contains(id)
                    || profile.Signals.First(signal => string.Equals(signal.Id, id, StringComparison.OrdinalIgnoreCase)).Kind
                    != SimulationSignalKind.VirtualOutput))
            {
                failures.Add($"Grupo exclusivo {group.Id} deve conter ao menos duas saidas validas.");
            }
        }

        HashSet<string> scenarioIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (SimulationScenario scenario in profile.Scenarios)
        {
            RequireText(scenario.Id, "Cenario sem id.", failures);
            RequireText(scenario.DisplayName, $"Cenario {scenario.Id}: nome ausente.", failures);
            if (!string.IsNullOrWhiteSpace(scenario.Id) && !scenarioIds.Add(scenario.Id))
            {
                failures.Add($"Cenario duplicado: {scenario.Id}.");
            }

            foreach ((string signalId, double value) in scenario.Values)
            {
                SimulationSignalDefinition? definition = profile.Signals.FirstOrDefault(signal =>
                    string.Equals(signal.Id, signalId, StringComparison.OrdinalIgnoreCase));
                if (definition is null || value < definition.Minimum || value > definition.Maximum)
                {
                    failures.Add($"Cenario {scenario.Id}: valor invalido para {signalId}.");
                }
            }
        }

        return new(failures);
    }

    private static void ValidateVisualization(
        SimulationVisualizationDefinition? visualization,
        IReadOnlySet<string> signalIds,
        ICollection<string> failures)
    {
        if (visualization is null)
        {
            failures.Add("Definicao de visualizacao ausente.");
            return;
        }

        if (visualization.Type == SimulationVisualizationType.Pivot)
        {
            if (visualization.TowerCount is null or < 1 or > 16)
            {
                failures.Add("Visualizacao de pivo exige towerCount em 1..16.");
            }

            if (visualization.FaultTowerIndex is not null
                && (visualization.TowerCount is null
                    || visualization.FaultTowerIndex < 1
                    || visualization.FaultTowerIndex > visualization.TowerCount))
            {
                failures.Add("faultTowerIndex deve apontar para uma torre configurada.");
            }
        }
        else if (visualization.TowerCount is not null || visualization.FaultTowerIndex is not null)
        {
            failures.Add("Torres somente podem ser configuradas na visualizacao de pivo.");
        }

        HashSet<string> roleIds = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string role, string signalId) in visualization.SignalRoles)
        {
            if (string.IsNullOrWhiteSpace(role) || !roleIds.Add(role))
            {
                failures.Add($"Papel visual invalido ou duplicado: {role}.");
            }

            if (string.IsNullOrWhiteSpace(signalId) || !signalIds.Contains(signalId))
            {
                failures.Add($"Papel visual {role}: sinal inexistente {signalId}.");
            }
        }
    }

    private static void ValidateIoBindings(
        SimulationProfile profile,
        IReadOnlySet<string> signalIds,
        ICollection<string> failures)
    {
        HashSet<string> boundSignals = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> channels = new(StringComparer.OrdinalIgnoreCase);
        foreach (SimulationIoBinding binding in profile.IoBindings)
        {
            RequireText(binding.SignalId, "Binding sem SignalId.", failures);
            RequireText(binding.RegisterAlias, $"Binding {binding.SignalId}: alias ausente.", failures);
            RequireText(binding.Description, $"Binding {binding.SignalId}: descricao ausente.", failures);
            if (string.IsNullOrWhiteSpace(binding.SignalId) || !signalIds.Contains(binding.SignalId))
            {
                failures.Add($"Binding referencia sinal inexistente: {binding.SignalId ?? "AUSENTE"}.");
                continue;
            }

            if (!boundSignals.Add(binding.SignalId))
            {
                failures.Add($"Sinal com mais de um binding: {binding.SignalId}.");
            }

            SimulationSignalDefinition definition = profile.Signals.First(signal =>
                string.Equals(signal.Id, binding.SignalId, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(binding.SignalLabel)
                && !string.Equals(binding.SignalLabel, definition.Label, StringComparison.Ordinal))
            {
                failures.Add($"Binding {binding.SignalId}: SignalLabel diverge do perfil.");
            }

            if (!IsCompatible(definition.Kind, binding.Direction, binding.IoType))
            {
                failures.Add($"Binding {binding.SignalId}: direcao/tipo incompativel com {definition.Kind}.");
            }

            if (binding.Channel < 0 || binding.Register <= 0)
            {
                failures.Add($"Binding {binding.SignalId}: canal ou registro invalido.");
            }

            if (binding.Direction == SimulationIoDirection.Unspecified
                || binding.IoType == SimulationIoType.Unspecified
                || binding.EvidenceStatus == SimulationIoEvidenceStatus.Unspecified)
            {
                failures.Add($"Binding {binding.SignalId}: metadados obrigatorios ausentes.");
            }

            string channelKey = $"{binding.Direction}:{binding.IoType}:{binding.Channel}";
            if (!channels.Add(channelKey))
            {
                failures.Add($"Canal duplicado no perfil: {channelKey}.");
            }
        }
    }

    private static bool IsCompatible(
        SimulationSignalKind signalKind,
        SimulationIoDirection direction,
        SimulationIoType ioType) =>
        (signalKind, direction, ioType) switch
        {
            (SimulationSignalKind.DigitalInput, SimulationIoDirection.Input, SimulationIoType.Digital) => true,
            (SimulationSignalKind.AnalogInput, SimulationIoDirection.Input, SimulationIoType.Analog) => true,
            (SimulationSignalKind.VirtualOutput, SimulationIoDirection.Output, SimulationIoType.Digital) => true,
            _ => false
        };

    private static void RequireText(string? value, string failure, ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add(failure);
        }
    }
}
