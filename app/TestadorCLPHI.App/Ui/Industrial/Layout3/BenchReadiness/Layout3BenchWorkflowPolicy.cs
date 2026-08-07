namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

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
    internal const string ExpectedFirmwareFamily = "G5PLC.C950.ST";
    internal const string ExpectedFirmwareVersion = "3.3.11";
    internal const int ExpectedProgramId = 31134;
    internal const int ExpectedProgramCrc = 23248;

    private const ushort CriticalF21Mask =
        (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) |
        (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) |
        (1 << 12) | (1 << 13) | (1 << 14);

    private static readonly HashSet<int> IdentificationReferences = [30012, 30013, 30021];
    private static readonly HashSet<int> DigitalInputReferences = [31120, 31121, 31122, 31123, 31124, 31125, 31126, 31127];
    private static readonly HashSet<int> AnalogInputReferences = [31132, 31133, 31134];
    private static readonly HashSet<int> OutputReferences = [31128, 31129, 31130, 31131];

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
        (generalFailureStatus & CriticalF21Mask) != 0;

    internal static bool IsReadReferenceAllowed(Layout3BenchMode mode, int documentedReference) =>
        mode switch
        {
            Layout3BenchMode.Identification => IdentificationReferences.Contains(documentedReference),
            Layout3BenchMode.InputTest =>
                DigitalInputReferences.Contains(documentedReference)
                || AnalogInputReferences.Contains(documentedReference),
            _ => false
        };

    internal static int GetOutputDocumentedReference(Layout3OutputChannel channel) =>
        channel switch
        {
            Layout3OutputChannel.DO00 => 31128,
            Layout3OutputChannel.DO01 => 31129,
            Layout3OutputChannel.DO02 => 31130,
            Layout3OutputChannel.DO03 => 31131,
            _ => throw new ArgumentOutOfRangeException(nameof(channel))
        };

    internal static bool IsOutputReferenceAllowed(int documentedReference) =>
        OutputReferences.Contains(documentedReference);

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
