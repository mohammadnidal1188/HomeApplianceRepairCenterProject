using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Refrigerator: cost depends on gas refilling and on the size of the unit.
/// </summary>
public sealed class Refrigerator : Appliance
{
    // They are declared as const because these pricing and threshold values are fixed constants that should never change during runtime.
    private const decimal LaborRatePerHour = 12m;
    private const decimal GasRefillFee = 25m;
    private const decimal LargeUnitSurcharge = 15m;   // heavy unit, needs two technicians
    private const int LargeUnitThresholdLiters = 500;

    // Capacity and NoFrost are unchangeable physical specifications of the refrigerator, while the need for a gas refill can change during service.
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
        // If capacityLiters is zero or negative, it throws an ArgumentOutOfRangeException to prevent creating a refrigerator with an impossible physical volume.
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
            // EnsureRepairIsOpen verifies that the repair job is still active, preventing any technical status updates once the appliance service is finalized.
            EnsureRepairIsOpen("The gas refill requirement");
            _needsGasRefill = value;
        }
    }

    public override string DeviceType => "Refrigerator";

    // It must be override to provide custom cost logic for Refrigerator, implementing the abstract method declared in the Appliance base class.
    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost(LaborRatePerHour);

        // This condition checks if the refrigerator requires a gas refill, adding the GasRefillFee to the total cost if true.
        if (_needsGasRefill)
        {
            cost += GasRefillFee;
        }

        // This adds a surcharge for large capacity fridges that meet or exceed the threshold, as they require more labor and parts.
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
