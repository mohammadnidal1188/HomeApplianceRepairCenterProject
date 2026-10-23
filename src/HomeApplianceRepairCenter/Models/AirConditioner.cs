using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Air conditioner: cost depends on the on-site visit, the inverter board and the unit size.
/// </summary>
public sealed class AirConditioner : Appliance
{
    private const decimal LaborRatePerHour = 15m;
    private const decimal OnSiteVisitFee = 30m;       // wall-mounted units are repaired at the customer's home
    private const decimal InverterBoardFee = 25m;     // inverter units need a board diagnosis
    private const decimal LargeUnitSurcharge = 20m;
    private const int LargeUnitThresholdBtu = 24000;

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
            EnsureRepairIsOpen("The on-site visit requirement");
            _requiresOnSiteVisit = value;
        }
    }

    public override string DeviceType => "Air Conditioner";

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost(LaborRatePerHour);

        if (_requiresOnSiteVisit)
        {
            cost += OnSiteVisitFee;
        }

        if (_isInverter)
        {
            cost += InverterBoardFee;
        }

        if (_coolingCapacityBtu >= LargeUnitThresholdBtu)
        {
            cost += LargeUnitSurcharge;
        }

        return cost;
    }

    public override string GetStatusSummary()
    {
        string type = _isInverter ? "Inverter" : "Non-inverter";
        string visit = _requiresOnSiteVisit ? "yes" : "no";
        return $"{base.GetStatusSummary()} | {_coolingCapacityBtu} BTU, {type}, on-site visit: {visit}";
    }
}
