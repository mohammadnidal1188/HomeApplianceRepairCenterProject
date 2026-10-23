using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Interfaces;

/// <summary>
/// Operations shared by every appliance that can be repaired in the center.
/// </summary>
public interface IRepairable
{
    /// <summary>Unique identifier printed on the appliance.</summary>
    string SerialNumber { get; }

    /// <summary>Current stage of the repair.</summary>
    RepairStatus Status { get; }

    /// <summary>Moves the appliance from Received to UnderRepair.</summary>
    void StartRepair();

    /// <summary>Moves the appliance from UnderRepair to Completed.</summary>
    void CompleteRepair();

    /// <summary>Moves the appliance from Completed to Delivered.</summary>
    void DeliverToCustomer();

    /// <summary>Calculates the repair cost (USD) according to the appliance type.</summary>
    decimal CalculateRepairCost();

    /// <summary>Returns a one-line description of the appliance and its repair state.</summary>
    string GetStatusSummary();
}
