using TestadorCLPHI.App.Industrial.Platform.Devices;

namespace TestadorCLPHI.App.Industrial.Platform.Safety;

internal enum Layout3BenchMode
{
    Identification,
    InputTest,
    SupervisedOutputTest
}

internal enum Layout3EquipmentIdentificationState
{
    Identified,
    RespondedButNotRecognized,
    NoResponse,
    SignatureMismatch,
    CriticalFailure,
    Cancelled
}

internal enum Layout3OutputChannel
{
    DO00,
    DO01,
    DO02,
    DO03
}

internal sealed record Layout3ObservedSignature(
    bool Responded,
    string? FirmwareFamily,
    string? FirmwareVersion,
    int? ProgramId,
    int? ProgramCrc,
    ushort GeneralFailureStatus,
    bool Cancelled);

internal sealed record Layout3OutputAuthorization(
    bool EquipmentIdentified,
    bool SignatureValid,
    bool CriticalFailureAbsent,
    bool BenchChecklistApproved,
    bool OperatorExplicitlyEnabled,
    bool PhysicalGateAuthorized,
    bool SimulationOnly = false);

internal static class Layout3BenchWorkflowPolicy
{
    private static IndustrialDeviceProfile DeviceProfile => NeonHio115DeviceProfile.Current;

    internal static string ExpectedFirmwareFamily =>
        DeviceProfile.IdentificationPolicy.ExpectedFirmwareFamily ?? string.Empty;
    internal static string ExpectedFirmwareVersion =>
        DeviceProfile.IdentificationPolicy.ExpectedFirmwareVersion ?? string.Empty;
    internal static int ExpectedProgramId =>
        DeviceProfile.IdentificationPolicy.GetExpectedValue(DeviceIdentificationProbeRole.ProgramId);
    internal static int ExpectedProgramCrc =>
        DeviceProfile.IdentificationPolicy.GetExpectedValue(DeviceIdentificationProbeRole.ProgramCrc);

    internal static Layout3EquipmentIdentificationState Classify(Layout3ObservedSignature signature)
    {
        if (signature.Cancelled)
        {
            return Layout3EquipmentIdentificationState.Cancelled;
        }

        if (!signature.Responded)
        {
            return Layout3EquipmentIdentificationState.NoResponse;
        }

        if (HasCriticalFailure(signature.GeneralFailureStatus))
        {
            return Layout3EquipmentIdentificationState.CriticalFailure;
        }

        bool idOrCrcMissing = signature.ProgramId is null || signature.ProgramCrc is null;
        if (idOrCrcMissing)
        {
            return Layout3EquipmentIdentificationState.RespondedButNotRecognized;
        }

        bool matches = string.Equals(signature.FirmwareFamily, ExpectedFirmwareFamily, StringComparison.Ordinal)
            && string.Equals(signature.FirmwareVersion, ExpectedFirmwareVersion, StringComparison.Ordinal)
            && signature.ProgramId == ExpectedProgramId
            && signature.ProgramCrc == ExpectedProgramCrc;
        return matches
            ? Layout3EquipmentIdentificationState.Identified
            : Layout3EquipmentIdentificationState.SignatureMismatch;
    }

    internal static bool HasCriticalFailure(ushort generalFailureStatus) =>
        (generalFailureStatus
            & DeviceProfile.IdentificationPolicy.GetCriticalMask(
                DeviceIdentificationProbeRole.GeneralFailureStatus)) != 0;

    internal static bool IsReadReferenceAllowed(Layout3BenchMode mode, int documentedReference) =>
        mode switch
        {
            Layout3BenchMode.Identification => DeviceProfile.IdentificationPolicy.Probes.Any(
                probe => probe.DocumentedReference == documentedReference),
            Layout3BenchMode.InputTest => DeviceProfile.InputMap.ContainsReference(documentedReference),
            _ => false
        };

    internal static int GetOutputDocumentedReference(Layout3OutputChannel channel)
    {
        if (!DeviceProfile.OutputMap.TryResolve(channel.ToString(), out DeviceRegisterPoint output))
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        return output.DocumentedReference;
    }

    internal static bool IsOutputReferenceAllowed(int documentedReference) =>
        DeviceProfile.OutputMap.ContainsReference(documentedReference);

    internal static bool CanEnableSupervisedOutput(Layout3OutputAuthorization authorization) =>
        authorization.EquipmentIdentified
        && authorization.SignatureValid
        && authorization.CriticalFailureAbsent
        && authorization.BenchChecklistApproved
        && authorization.OperatorExplicitlyEnabled
        && authorization.PhysicalGateAuthorized
        && !authorization.SimulationOnly;

    internal static bool CanEnableSimulatedOutput(Layout3OutputAuthorization authorization) =>
        authorization.EquipmentIdentified
        && authorization.SignatureValid
        && authorization.CriticalFailureAbsent
        && authorization.BenchChecklistApproved
        && authorization.OperatorExplicitlyEnabled
        && !authorization.PhysicalGateAuthorized
        && authorization.SimulationOnly;

    internal static bool CanActivateOutput(
        Layout3BenchMode mode,
        Layout3OutputAuthorization authorization,
        Layout3OutputChannel? activeChannel,
        bool cancelled,
        bool timedOut,
        int requestedDurationMilliseconds,
        int maximumDurationMilliseconds) =>
        mode == Layout3BenchMode.SupervisedOutputTest
        && CanEnableSupervisedOutput(authorization)
        && activeChannel is null
        && !cancelled
        && !timedOut
        && maximumDurationMilliseconds > 0
        && requestedDurationMilliseconds > 0
        && requestedDurationMilliseconds <= maximumDurationMilliseconds;

    internal static bool MustTurnOffAfterActivation(bool activationStarted) => activationStarted;
}
