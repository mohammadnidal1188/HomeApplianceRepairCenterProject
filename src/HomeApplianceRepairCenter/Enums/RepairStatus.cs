namespace HomeApplianceRepairCenter.Enums;

/// <summary>
/// Stages of an appliance inside the repair center.
/// The order matters: a status can only move forward (Received -> ... -> Delivered).
/// </summary>
public enum RepairStatus
{
    /// <summary>Handed in by the customer, repair not started yet.</summary>
    Received = 0,

    /// <summary>A technician is working on the appliance.</summary>
    UnderRepair = 1,

    /// <summary>Repair finished, waiting for the customer.</summary>
    Completed = 2,

    /// <summary>Returned to the customer.</summary>
    Delivered = 3
}
