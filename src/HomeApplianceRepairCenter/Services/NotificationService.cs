using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services;

// This class acts as a subscriber that listens to appliance events to handle notifications and logging.
/// <summary>
/// Subscriber of the appliance events: turns status changes and late deliveries
/// into messages for the customer and keeps a log of them.
/// </summary>
public class NotificationService
{
    // The list is private to protect it from direct modification, exposing it safely as a read-only property via Log.
    private readonly List<string> _log = new();

    /// <summary>All messages sent so far, oldest first.</summary>
    public IReadOnlyList<string> Log => _log.AsReadOnly();

    // The += operator registers a method as an event handler, and Attach is called when a new appliance enters the system.
    /// <summary>Starts listening to the events of an appliance.</summary>
    public void Attach(Appliance appliance)
    {
        appliance.StatusChanged += OnStatusChanged;
        appliance.DeliveryOverdue += OnDeliveryOverdue;
    }

    // The -= operator unsubscribes the handler to prevent potential memory leaks when an appliance is removed.
    /// <summary>Stops listening to the events of an appliance.</summary>
    public void Detach(Appliance appliance)
    {
        appliance.StatusChanged -= OnStatusChanged;
        appliance.DeliveryOverdue -= OnDeliveryOverdue;
    }

    // This method is called automatically whenever the StatusChanged event is triggered by an appliance.
    private void OnStatusChanged(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus)
    {
        // This switch statement formats a customized notification text based on the specific lifecycle state of the repair job.
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

    // This method is triggered by the DeliveryOverdue event, using the serial number and overdue days to log a delay warning.
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
