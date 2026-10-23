using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Refrigerator: cost depends on gas refilling and on the size of the unit.
/// </summary>
public sealed class Refrigerator : Appliance
{
    private const decimal LaborRatePerHour = 12m;
    private const decimal GasRefillFee = 25m;
    private const decimal LargeUnitSurcharge = 15m;   // heavy unit, needs two technicians
    private const int LargeUnitThresholdLiters = 500;

    private readonly int _capacityLiters;
    private readonly bool _isNoFrost;
    private bool _needsGasRefill;

    public Refrigerator(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string faultDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        int capacityLiters,
        bool isNoFrost,
        bool needsGasRefill = false)
        : base(serialNumber, brand, model, customerName, faultDescription, receivedDate, expectedDeliveryDate)
    {
        if (!ValidationHelper.IsPositive(capacityLiters))
        {
            throw new ArgumentOutOfRangeException(nameof(capacityLiters), "Capacity must be greater than zero.");
        }

        _capacityLiters = capacityLiters;
        _isNoFrost = isNoFrost;
        _needsGasRefill = needsGasRefill;
    }

    public int CapacityLiters => _capacityLiters;

    public bool IsNoFrost => _isNoFrost;

    /// <summary>Set by the technician after diagnosis; locked once the repair is completed.</summary>
    public bool NeedsGasRefill
    {
        get => _needsGasRefill;
        set
        {
            EnsureRepairIsOpen("The gas refill requirement");
            _needsGasRefill = value;
        }
    }

    public override string DeviceType => "Refrigerator";

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost(LaborRatePerHour);

        if (_needsGasRefill)
        {
            cost += GasRefillFee;
        }

        if (_capacityLiters >= LargeUnitThresholdLiters)
        {
            cost += LargeUnitSurcharge;
        }

        return cost;
    }

    public override string GetStatusSummary()
    {
        string frostType = _isNoFrost ? "No-Frost" : "Frost";
        string gas = _needsGasRefill ? "yes" : "no";
        return $"{base.GetStatusSummary()} | {_capacityLiters} L, {frostType}, gas refill: {gas}";
    }
}
