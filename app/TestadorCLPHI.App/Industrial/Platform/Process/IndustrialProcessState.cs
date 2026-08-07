using TestadorCLPHI.App.Industrial.Platform.Simulation;

namespace TestadorCLPHI.App.Industrial.Platform.Process;

internal sealed record ProcessMetricState(
    string SignalId,
    string Label,
    double Value,
    string? Unit);

internal sealed record SimulationProcessState(
    string ProfileId,
    string DisplayName,
    SimulationVisualizationType VisualizationType,
    string OverallState,
    IReadOnlyDictionary<string, double> Values,
    IReadOnlyDictionary<string, string> SignalRoles,
    IReadOnlyList<ProcessMetricState> Metrics,
    IReadOnlyList<string> ActiveAlarms,
    bool OutputsBlocked)
{
    internal double? GetRoleValue(string role)
    {
        string? signalId = SignalRoles.FirstOrDefault(item =>
            string.Equals(item.Key, role, StringComparison.OrdinalIgnoreCase)).Value;
        return !string.IsNullOrWhiteSpace(signalId) && Values.TryGetValue(signalId, out double value)
            ? value
            : null;
    }

    internal bool HasRole(string role) => GetRoleValue(role) is not null;
    internal bool IsRoleActive(string role) => GetRoleValue(role) is > 0;
}

internal static class SimulationProcessStateProjector
{
    internal static SimulationProcessState Project(
        SimulationProfile profile,
        SimulationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!string.Equals(profile.Id, snapshot.ProfileId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Snapshot nao pertence ao perfil informado.", nameof(snapshot));
        }

        Dictionary<string, string> roles = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string role, string signalId) in profile.Visualization.SignalRoles)
        {
            roles[role] = signalId;
        }

        ProcessMetricState[] metrics = profile.Signals
            .Where(signal => signal.Kind == SimulationSignalKind.AnalogInput)
            .Select(signal => new ProcessMetricState(
                signal.Id!,
                signal.Label!,
                snapshot.Values[signal.Id!],
                signal.Unit))
            .ToArray();

        return new(
            snapshot.ProfileId,
            profile.DisplayName!,
            profile.Visualization.Type,
            snapshot.State,
            new Dictionary<string, double>(snapshot.Values, StringComparer.OrdinalIgnoreCase),
            roles,
            metrics,
            snapshot.ActiveAlarms.ToArray(),
            snapshot.OutputsBlocked);
    }
}

internal enum PivotMovementDirection
{
    Stopped,
    Forward,
    Reverse
}

internal enum PivotTowerStatus
{
    Ok,
    Moving,
    Misaligned,
    Fault,
    Unknown
}

internal sealed record PivotTowerState(int Number, PivotTowerStatus Status);

internal sealed record PivotProcessState(
    string OverallState,
    double PositionPercent,
    PivotMovementDirection Direction,
    bool PumpActive,
    bool WaterActive,
    bool EmergencyActive,
    bool OutputsBlocked,
    IReadOnlyList<PivotTowerState> Towers,
    IReadOnlyList<ProcessMetricState> Metrics,
    IReadOnlyList<string> ActiveAlarms);

internal static class PivotProcessStateProjector
{
    internal static PivotProcessState Project(
        SimulationProfile profile,
        SimulationProcessState processState)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(processState);
        if (profile.Visualization.Type != SimulationVisualizationType.Pivot)
        {
            throw new ArgumentException("Perfil nao possui visualizacao de pivo.", nameof(profile));
        }

        int towerCount = profile.Visualization.TowerCount
            ?? throw new ArgumentException("Perfil de pivo sem towerCount.", nameof(profile));
        bool forward = processState.IsRoleActive("forward");
        bool reverse = processState.IsRoleActive("reverse");
        PivotMovementDirection direction = (forward, reverse) switch
        {
            (true, false) => PivotMovementDirection.Forward,
            (false, true) => PivotMovementDirection.Reverse,
            _ => PivotMovementDirection.Stopped
        };

        bool emergency = processState.IsRoleActive("emergency");
        bool towerFault = processState.IsRoleActive("towerFault");
        bool misaligned = processState.HasRole("alignment")
            && !processState.IsRoleActive("alignment");
        int affectedTower = profile.Visualization.FaultTowerIndex ?? towerCount;
        PivotTowerState[] towers = Enumerable.Range(1, towerCount)
            .Select(number => new PivotTowerState(
                number,
                ResolveTowerStatus(
                    number,
                    affectedTower,
                    emergency,
                    towerFault,
                    misaligned,
                    direction)))
            .ToArray();

        double position = Math.Clamp(processState.GetRoleValue("position") ?? 0, 0, 100);
        return new(
            processState.OverallState,
            position,
            direction,
            processState.IsRoleActive("pump"),
            processState.IsRoleActive("water"),
            emergency,
            processState.OutputsBlocked,
            towers,
            processState.Metrics,
            processState.ActiveAlarms);
    }

    private static PivotTowerStatus ResolveTowerStatus(
        int towerNumber,
        int affectedTower,
        bool emergency,
        bool towerFault,
        bool misaligned,
        PivotMovementDirection direction)
    {
        if (emergency)
        {
            return PivotTowerStatus.Unknown;
        }

        if (towerFault && towerNumber == affectedTower)
        {
            return PivotTowerStatus.Fault;
        }

        if (misaligned && towerNumber == affectedTower)
        {
            return PivotTowerStatus.Misaligned;
        }

        return direction == PivotMovementDirection.Stopped
            ? PivotTowerStatus.Ok
            : PivotTowerStatus.Moving;
    }
}
