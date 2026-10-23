using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Washing machine: cost depends on the loading type and on drum replacement.
/// </summary>
public sealed class WashingMachine : Appliance
{
    private const decimal LaborRatePerHour = 10m;
    private const decimal FrontLoadSurcharge = 15m;   // front-load models need a longer disassembly
    private const decimal DrumReplacementFee = 60m;

    private readonly int _loadCapacityKg;
    private readonly bool _isFrontLoad;
    private bool _needsDrumReplacement;

    public WashingMachine(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string faultDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate,
        int loadCapacityKg,
        bool isFrontLoad,
        bool needsDrumReplacement = false)
        : base(serialNumber, brand, model, customerName, faultDescription, receivedDate, expectedDeliveryDate)
    {
        if (!ValidationHelper.IsPositive(loadCapacityKg))
        {
            throw new ArgumentOutOfRangeException(nameof(loadCapacityKg), "Load capacity must be greater than zero.");
        }

        _loadCapacityKg = loadCapacityKg;
        _isFrontLoad = isFrontLoad;
        _needsDrumReplacement = needsDrumReplacement;
    }

    public int LoadCapacityKg => _loadCapacityKg;

    public bool IsFrontLoad => _isFrontLoad;

    /// <summary>Set by the technician after diagnosis; locked once the repair is completed.</summary>
    public bool NeedsDrumReplacement
    {
        get => _needsDrumReplacement;
        set
        {
            EnsureRepairIsOpen("The drum replacement requirement");
            _needsDrumReplacement = value;
        }
    }

    public override string DeviceType => "Washing Machine";

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost(LaborRatePerHour);

        if (_isFrontLoad)
        {
            cost += FrontLoadSurcharge;
        }

        if (_needsDrumReplacement)
        {
            cost += DrumReplacementFee;
        }

        return cost;
    }

    public override string GetStatusSummary()
    {
        string loading = _isFrontLoad ? "Front-load" : "Top-load";
        string drum = _needsDrumReplacement ? "yes" : "no";
        return $"{base.GetStatusSummary()} | {_loadCapacityKg} kg, {loading}, drum replacement: {drum}";
    }
}
