using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Air conditioner: cost depends on the on-site visit, the inverter board and the unit size.
/// </summary>
public sealed class AirConditioner : Appliance
{
    // These fees represent fixed costs for onsite visits, inverter board repairs, and large capacity units, declared as const because they never change.
    private const decimal LaborRatePerHour = 15m;
    private const decimal OnSiteVisitFee = 30m;       // wall-mounted units are repaired at the customer's home
    private const decimal InverterBoardFee = 25m;     // inverter units need a board diagnosis
    private const decimal LargeUnitSurcharge = 20m;
    private const int LargeUnitThresholdBtu = 24000;

    // Capacity and inverter type are permanent hardware specs that cannot change after creation, while onsite visit requirements can be updated dynamically.
    private readonly int _coolingCapacityBtu;
    private readonly bool _isInverter;
    private bool _requiresOnSiteVisit;

    public AirConditioner(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string faultDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        int coolingCapacityBtu,
        bool isInverter,
        bool requiresOnSiteVisit = false)
        : base(serialNumber, brand, model, customerName, faultDescription, receivedDate, expectedDeliveryDate)
    {
        // If coolingCapacityBtu is zero or negative, an ArgumentOutOfRangeException is thrown to prevent creating an appliance with invalid capacity specs.
        if (!ValidationHelper.IsPositive(coolingCapacityBtu))
        {
            throw new ArgumentOutOfRangeException(nameof(coolingCapacityBtu), "Cooling capacity must be greater than zero.");
        }

        _coolingCapacityBtu = coolingCapacityBtu;
        _isInverter = isInverter;
        _requiresOnSiteVisit = requiresOnSiteVisit;
    }

    public int CoolingCapacityBtu => _coolingCapacityBtu;

    public bool IsInverter => _isInverter;

    /// <summary>Set when the unit cannot be moved to the workshop; locked once the repair is completed.</summary>
    public bool RequiresOnSiteVisit
    {
        get => _requiresOnSiteVisit;
        set
        {
            // EnsureRepairIsOpen checks if the appliance is still undergoing repair, locking any modification once the job is marked as completed.
            EnsureRepairIsOpen("The on-site visit requirement");
            _requiresOnSiteVisit = value;
        }
    }

    public override string DeviceType => "Air Conditioner";

    // It must be override to provide specialized logic for AirConditioner, which is originally declared as abstract in the Appliance base class.
    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost(LaborRatePerHour);

        // This checks if the repair requires an onsite technician, adding a visit fee to cover travel and labor expenses.
        if (_requiresOnSiteVisit)
        {
            cost += OnSiteVisitFee;
        }

        // This checks if the AC has an inverter technology, adding a technical fee for diagnosing and repairing complex inverter control boards.
        if (_isInverter)
        {
            cost += InverterBoardFee;
        }

        // This adds a surcharge for large capacity units; a 24000 BTU unit is charged since it equals the threshold.
        if (_coolingCapacityBtu >= LargeUnitThresholdBtu)
        {
            cost += LargeUnitSurcharge;
        }

        return cost;
    }

    // It returns a formatted text containing common details from the Appliance class, extending it with specific AirConditioner specs.
    public override string GetStatusSummary()
    {
        string type = _isInverter ? "Inverter" : "Non-inverter";
        string visit = _requiresOnSiteVisit ? "yes" : "no";
        return $"{base.GetStatusSummary()} | {_coolingCapacityBtu} BTU, {type}, on-site visit: {visit}";
    }
}
