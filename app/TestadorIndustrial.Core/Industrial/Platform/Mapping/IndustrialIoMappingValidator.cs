using TestadorCLPHI.App.Industrial.Platform.Devices;
using TestadorCLPHI.App.Industrial.Platform.Simulation;

namespace TestadorCLPHI.App.Industrial.Platform.Mapping;

internal sealed record IndustrialIoMappingValidationResult(IReadOnlyList<string> Failures)
{
    internal bool IsValid => Failures.Count == 0;
    internal string ToDisplayText() => string.Join(Environment.NewLine, Failures);
}

internal static class DeviceProfileSimulationIoPolicy
{
    internal static bool TryResolve(
        IndustrialDeviceProfile deviceProfile,
        SimulationIoDirection direction,
        SimulationIoType ioType,
        int channel,
        out string registerAlias,
        out int documentedRegister)
    {
        registerAlias = string.Empty;
        documentedRegister = 0;
        if (direction == SimulationIoDirection.Input)
        {
            DeviceInputKind expectedKind = ioType == SimulationIoType.Digital
                ? DeviceInputKind.Digital
                : DeviceInputKind.Analog;
            DeviceRegisterPoint? point = deviceProfile.InputMap.Blocks
                .Where(block => block.Kind == expectedKind)
                .SelectMany(block => block.Points)
                .FirstOrDefault(item => item.Channel == channel);
            if (point is not null)
            {
                registerAlias = point.Alias;
                documentedRegister = point.DocumentedReference;
                return true;
            }
        }

        if (direction == SimulationIoDirection.Output
            && ioType == SimulationIoType.Digital)
        {
            DeviceRegisterPoint? point = deviceProfile.OutputMap.Outputs
                .FirstOrDefault(item => item.Channel == channel);
            if (point is not null)
            {
                registerAlias = point.Alias;
                documentedRegister = point.DocumentedReference;
                return true;
            }
        }

        return false;
    }

    internal static bool IsAllowListed(
        IndustrialDeviceProfile deviceProfile,
        SimulationIoDirection direction,
        int documentedRegister) =>
        direction switch
        {
            SimulationIoDirection.Input => deviceProfile.InputMap.ContainsReference(documentedRegister),
            SimulationIoDirection.Output => deviceProfile.OutputMap.ContainsReference(documentedRegister),
            _ => false
        };
}

internal static class IndustrialIoMappingValidator
{
    internal static IndustrialIoMappingValidationResult Validate(
        SimulationProfile profile,
        IndustrialDeviceProfile? deviceProfile = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        IndustrialDeviceProfile selectedDeviceProfile = deviceProfile ?? NeonHio115DeviceProfile.Current;
        List<string> failures = [];
        if (profile.IoBindings.Count == 0)
        {
            failures.Add("Perfil sem ioBindings para o fake HIO115.");
            return new(failures);
        }

        HashSet<string> signalIds = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> channelKeys = new(StringComparer.OrdinalIgnoreCase);
        foreach (SimulationIoBinding binding in profile.IoBindings)
        {
            SimulationSignalDefinition? signal = profile.Signals.FirstOrDefault(item =>
                string.Equals(item.Id, binding.SignalId, StringComparison.OrdinalIgnoreCase));
            if (signal is null)
            {
                failures.Add($"Binding referencia sinal inexistente: {binding.SignalId ?? "AUSENTE"}.");
                continue;
            }

            if (!signalIds.Add(binding.SignalId!))
            {
                failures.Add($"Sinal duplicado no mapa: {binding.SignalId}.");
            }

            string channelKey = $"{binding.Direction}:{binding.IoType}:{binding.Channel}";
            if (!channelKeys.Add(channelKey))
            {
                failures.Add($"Canal duplicado no mapa: {channelKey}.");
            }

            if (!IsCompatible(signal.Kind, binding.Direction, binding.IoType))
            {
                failures.Add($"Binding {binding.SignalId}: tipo incompativel com o sinal.");
            }

            if (binding.EvidenceStatus != SimulationIoEvidenceStatus.SimulationProfile)
            {
                failures.Add($"Binding {binding.SignalId}: evidenceStatus deve ser SimulationProfile.");
            }

            if (!DeviceProfileSimulationIoPolicy.TryResolve(
                    selectedDeviceProfile,
                    binding.Direction,
                    binding.IoType,
                    binding.Channel,
                    out string expectedAlias,
                    out int expectedRegister))
            {
                failures.Add($"Binding {binding.SignalId}: canal fora do fake HIO115 permitido.");
                continue;
            }

            if (!string.Equals(binding.RegisterAlias, expectedAlias, StringComparison.Ordinal))
            {
                failures.Add(
                    $"Binding {binding.SignalId}: alias {binding.RegisterAlias ?? "AUSENTE"}; esperado {expectedAlias}.");
            }

            if (binding.Register != expectedRegister)
            {
                failures.Add(
                    $"Binding {binding.SignalId}: registro {binding.Register}; esperado {expectedRegister}.");
            }

            if (!DeviceProfileSimulationIoPolicy.IsAllowListed(
                    selectedDeviceProfile,
                    binding.Direction,
                    binding.Register))
            {
                failures.Add($"Binding {binding.SignalId}: registro fora da allow-list.");
            }

            if (binding.RegisterAlias?.Contains("PWM", StringComparison.OrdinalIgnoreCase) == true
                || binding.Description?.Contains("reservado", StringComparison.OrdinalIgnoreCase) == true)
            {
                failures.Add($"Binding {binding.SignalId}: PWM/reservado nao permitido.");
            }
        }

        return new(failures);
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
}
