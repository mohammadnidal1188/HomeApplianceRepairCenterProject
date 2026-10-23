using System.Globalization;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Helpers;
using HomeApplianceRepairCenter.Interfaces;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;

namespace HomeApplianceRepairCenter;

/// <summary>
/// Console demo that walks through the full life of the appliances in the center.
/// </summary>
internal static class Program
{
    private static void Main()
    {
        // Same number and date format on every computer.
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        DateTime today = DateTime.Today;
        var center = new RepairCenter("City Home Appliance Repair Center");

        // ---------- 1. Register appliances ----------
        Section("1. Registering appliances (abstract class + 3 subclasses)");

        var fridge = new Refrigerator(
            serialNumber: "REF-100234", brand: "Samsung", model: "RT50",
            customerName: "Ahmad Khalil", faultDescription: "Not cooling",
            receivedDate: today.AddDays(-5), expectedDeliveryDate: today.AddDays(2),
            capacityLiters: 450, isNoFrost: true, needsGasRefill: true)
        {
            PartsCost = 40m,
            LaborHours = 2m
        };

        var washer = new WashingMachine(
            serialNumber: "WSH-200415", brand: "LG", model: "F4V5",
            customerName: "Layla Hassan", faultDescription: "Drum does not spin",
            receivedDate: today.AddDays(-4), expectedDeliveryDate: today.AddDays(3),
            loadCapacityKg: 8, isFrontLoad: true, needsDrumReplacement: true)
        {
            PartsCost = 55m,
            LaborHours = 3m
        };

        var airConditioner = new AirConditioner(
            serialNumber: "ACN-300871", brand: "Gree", model: "Inverter 24",
            customerName: "Omar Nasser", faultDescription: "Outdoor fan not working",
            receivedDate: today.AddDays(-10), expectedDeliveryDate: today.AddDays(-3),
            coolingCapacityBtu: 24000, isInverter: true, requiresOnSiteVisit: true)
        {
            PartsCost = 30m,
            LaborHours = 2.5m
        };

        var secondFridge = new Refrigerator(
            serialNumber: "REF-100999", brand: "LG", model: "GR-600",
            customerName: "Rania Saad", faultDescription: "Door seal damaged",
            receivedDate: today.AddDays(-1), expectedDeliveryDate: today.AddDays(5),
            capacityLiters: 600, isNoFrost: true)
        {
            PartsCost = 120m,
            LaborHours = 4m
        };

        center.AddAppliance(fridge);
        center.AddAppliance(washer);
        center.AddAppliance(airConditioner);
        center.AddAppliance(secondFridge);
        center.DisplayAllStatuses();

        // ---------- 2. Workflow and events ----------
        Section("2. Repair workflow (StatusChanged event)");

        fridge.StartRepair();
        washer.StartRepair();
        airConditioner.StartRepair();
        fridge.CompleteRepair();
        washer.CompleteRepair();
        fridge.DeliverToCustomer();

        // ---------- 3. Late deliveries ----------
        Section("3. Checking promised delivery dates (DeliveryOverdue event)");

        int overdueCount = center.CheckOverdueAppliances(today);
        Console.WriteLine($"  Late appliances: {overdueCount}");

        // ---------- 4. Polymorphism ----------
        Section("4. Polymorphic processing of the list");

        center.DisplayAllStatuses();
        Console.WriteLine($"Total cost of all appliances : {center.CalculateTotalCost():N2}");

        foreach (RepairStatus status in Enum.GetValues<RepairStatus>())
        {
            int count = center.GetByStatus(status).Count();
            Console.WriteLine($"  {status,-12} : {count} appliance(s), cost {center.CalculateTotalCost(status):N2}");
        }

        IRepairable repairable = airConditioner;   // the interface is enough to use any appliance
        Console.WriteLine($"Through IRepairable          : {repairable.SerialNumber} costs {repairable.CalculateRepairCost():N2}");

        // ---------- 5. Static members ----------
        Section("5. Static statistics");

        Console.WriteLine($"Under repair now       : {Appliance.UnderRepairCount}");
        Console.WriteLine($"Completed repairs      : {Appliance.CompletedRepairsCount}");
        Console.WriteLine($"Revenue of completed   : {Appliance.TotalRevenue:N2}");

        bool validDate = ValidationHelper.TryParseDate("2026-13-45", out DateTime parsedDate);
        Console.WriteLine($"Is \"2026-13-45\" a valid date? {validDate}");
        Console.WriteLine($"Is \"REF-100234\" a valid serial number? {ValidationHelper.IsValidSerialNumber("REF-100234")}");

        // ---------- 6. Encapsulation and validation ----------
        Section("6. Invalid operations are rejected");

        // fridge.SerialNumber = "REF-000000";   // does not compile: the property is read-only
        TryAction("Invalid serial number", () => new WashingMachine(
            "12-ABC", "Bosch", "Serie 4", "Test Customer", "Leaking",
            today, today.AddDays(2), 7, true));
        TryAction("Negative capacity", () => new Refrigerator(
            "REF-555555", "Bosch", "KGN", "Test Customer", "Noisy",
            today, today.AddDays(2), -10, false));
        TryAction("Delivery date before received date", () => new AirConditioner(
            "ACN-555555", "Midea", "Mission", "Test Customer", "Leaking",
            today, today.AddDays(-1), 12000, false));
        TryAction("Duplicate serial number", () => center.AddAppliance(new AirConditioner(
            "ACN-300871", "Midea", "Mission", "Test Customer", "Leaking",
            today, today.AddDays(2), 12000, false)));
        TryAction("Change parts cost after completion", () => fridge.PartsCost = 999m);
        TryAction("Change gas refill after completion", () => fridge.NeedsGasRefill = false);
        TryAction("Complete a repair that never started", () => secondFridge.CompleteRepair());

        Console.WriteLine();
        Console.WriteLine($"Notifications sent: {center.Notifications.Log.Count}");
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {title} ---");
    }

    /// <summary>Runs an action that must fail and prints the reason it was rejected.</summary>
    private static void TryAction(string description, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  [UNEXPECTED] {description}: no error was raised.");
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            Console.WriteLine($"  [REJECTED] {description}: {ex.Message}");
        }
    }
}
