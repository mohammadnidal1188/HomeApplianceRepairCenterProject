using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// Subscriber of the appliance events: turns status changes and late deliveries
/// into messages for the customer and keeps a log of them.
/// </summary>
public class NotificationService
{
    private readonly List<string> _log = new();

    /// <summary>All messages sent so far, oldest first.</summary>
    public IReadOnlyList<string> Log => _log.AsReadOnly();

    /// <summary>Starts listening to the events of an appliance.</summary>
    public void Attach(Appliance appliance)
    {
        appliance.StatusChanged += OnStatusChanged;
        appliance.DeliveryOverdue += OnDeliveryOverdue;
    }

    /// <summary>Stops listening to the events of an appliance.</summary>
    public void Detach(Appliance appliance)
    {
        appliance.StatusChanged -= OnStatusChanged;
        appliance.DeliveryOverdue -= OnDeliveryOverdue;
    }

    private void OnStatusChanged(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus)
    {
        string message = newStatus switch
        {
            RepairStatus.UnderRepair =>
                $"{appliance.DeviceType} {appliance.SerialNumber}: repair started.",
            RepairStatus.Completed =>
                $"Dear {appliance.CustomerName}, your {appliance.DeviceType} ({appliance.SerialNumber}) " +
                $"is repaired. Total cost: {appliance.CalculateRepairCost():N2}.",
            RepairStatus.Delivered =>
                $"{appliance.DeviceType} {appliance.SerialNumber} was delivered to {appliance.CustomerName}.",
            _ =>
                $"{appliance.SerialNumber}: {oldStatus} -> {newStatus}."
        };

        Publish(message);
    }

    private void OnDeliveryOverdue(Appliance appliance, int daysOverdue)
    {
        Publish($"WARNING: {appliance.DeviceType} {appliance.SerialNumber} of {appliance.CustomerName} " +
                $"is {daysOverdue} day(s) past its promised delivery date.");
    }

    private void Publish(string message)
    {
        _log.Add(message);
        Console.WriteLine($"  [NOTIFICATION] {message}");
    }
}
