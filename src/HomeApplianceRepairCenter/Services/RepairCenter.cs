using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Helpers;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// Main class of the system: stores the appliances and processes them polymorphically,
/// without caring whether an item is a refrigerator, a washing machine or an air conditioner.
/// </summary>
public class RepairCenter
{
    private readonly string _name;
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

    /// <summary>Read-only view: appliances can only be added or removed through the center.</summary>
    public IReadOnlyList<Appliance> Appliances => _appliances.AsReadOnly();

    public NotificationService Notifications => _notificationService;

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

    public IEnumerable<Appliance> GetByStatus(RepairStatus status)
        => _appliances.Where(a => a.Status == status);

    /// <summary>Total cost of all appliances; each one applies its own pricing rules.</summary>
    public decimal CalculateTotalCost() => SumCosts(_appliances);

    /// <summary>Total cost of the appliances that are in the given status.</summary>
    public decimal CalculateTotalCost(RepairStatus status) => SumCosts(GetByStatus(status));

    /// <summary>Prints the summary line of every appliance.</summary>
    public void DisplayAllStatuses()
    {
        Console.WriteLine($"=== {_name}: appliance status report ===");

        foreach (Appliance appliance in _appliances)
        {
            Console.WriteLine(appliance.GetStatusSummary());
        }
    }

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
