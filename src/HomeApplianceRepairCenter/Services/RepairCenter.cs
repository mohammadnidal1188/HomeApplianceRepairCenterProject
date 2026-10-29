using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Helpers;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services;

// Keeping a single list enables polymorphism, allowing the system to process all appliance types uniformly using generic logic.
/// <summary>
/// Main class of the system: stores the appliances and processes them polymorphically,
/// without caring whether an item is a refrigerator, a washing machine or an air conditioner.
/// </summary>
public class RepairCenter
{
    private readonly string _name;
    // The list stores any object deriving from Appliance; it is private to enforce encapsulation and readonly to prevent instance replacement.
    private readonly List<Appliance> _appliances = new();
    private readonly NotificationService _notificationService = new();

    public RepairCenter(string name)
    {
        if (!ValidationHelper.IsValidText(name))
        {
            throw new ArgumentException("Center name is required.", nameof(name));
        }

        _name = name.Trim();
    }

    public string Name => _name;

    // Returning IReadOnlyList prevents external classes from directly adding or removing items, maintaining data integrity.
    /// <summary>Read-only view: appliances can only be added or removed through the center.</summary>
    public IReadOnlyList<Appliance> Appliances => _appliances.AsReadOnly();

    public NotificationService Notifications => _notificationService;

    // It ensures the appliance is not null and not duplicated, linking it to notification events to enable real-time tracking.
    /// <summary>Registers an appliance and subscribes the notification service to its events.</summary>
    public void AddAppliance(Appliance appliance)
    {
        ArgumentNullException.ThrowIfNull(appliance);

        if (FindBySerial(appliance.SerialNumber) != null)
        {
            throw new InvalidOperationException(
                $"An appliance with serial number {appliance.SerialNumber} is already registered.");
        }

        _appliances.Add(appliance);
        _notificationService.Attach(appliance);
    }

    // Detach is called first to unsubscribe from events while the reference is still safely available, preventing memory leaks.
    /// <summary>Removes an appliance (and its subscriptions); returns false if it is not found.</summary>
    public bool RemoveAppliance(string serialNumber)
    {
        Appliance? appliance = FindBySerial(serialNumber);
        if (appliance == null)
        {
            return false;
        }

        _notificationService.Detach(appliance);
        return _appliances.Remove(appliance);
    }

    public Appliance? FindBySerial(string serialNumber)
        => _appliances.FirstOrDefault(a => a.SerialNumber == serialNumber);

    // Where filters the appliances by their current repair status; example: GetByStatus(RepairStatus.UnderRepair).
    public IEnumerable<Appliance> GetByStatus(RepairStatus status)
        => _appliances.Where(a => a.Status == status);

    // This feature is called method overloading, allowing multiple methods with the same name but different signatures.
    /// <summary>Total cost of all appliances; each one applies its own pricing rules.</summary>
    public decimal CalculateTotalCost() => SumCosts(_appliances);

    /// <summary>Total cost of the appliances that are in the given status.</summary>
    public decimal CalculateTotalCost(RepairStatus status) => SumCosts(GetByStatus(status));

    // polymorphism : each appliance type overrides the virtual GetStatusSummary method.
    /// <summary>Prints the summary line of every appliance.</summary>
    public void DisplayAllStatuses()
    {
        Console.WriteLine($"=== {_name}: appliance status report ===");

        foreach (Appliance appliance in _appliances)
        {
            Console.WriteLine(appliance.GetStatusSummary());
        }
    }

    // Appliance raises DeliveryOverdue in CheckDeliveryDeadline(); this method counts the late ones.
    /// <summary>Checks every open repair against its promised date; late ones raise their event.</summary>
    /// <returns>Number of late appliances.</returns>
    public int CheckOverdueAppliances(DateTime today)
    {
        int overdueCount = 0;

        foreach (Appliance appliance in _appliances)
        {
            if (appliance.CheckDeliveryDeadline(today))
            {
                overdueCount++;
            }
        }

        return overdueCount;
    }

    // Polymorphism occurs during the CalculateRepairCost() call, executing the specific implementation for each derived appliance type at runtime.
    private static decimal SumCosts(IEnumerable<Appliance> appliances)
    {
        decimal total = 0m;

        foreach (Appliance appliance in appliances)
        {
            total += appliance.CalculateRepairCost();   // polymorphic call
        }

        return total;
    }
}
