using HomeApplianceRepairCenter.Helpers;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Washing machine: cost depends on the loading type and on drum replacement.
/// </summary>
public sealed class WashingMachine : Appliance
{
    // The labor rate is lower because washing machine repairs are generally less complex than refrigerators, and it is const because it remains fixed.
    private const decimal LaborRatePerHour = 10m;
    private const decimal FrontLoadSurcharge = 15m;   // front-load models need a longer disassembly
    private const decimal DrumReplacementFee = 60m;

    // Capacity and loading type are unchangeable structural designs of the machine, while the need for a drum replacement depends on diagnostic findings.
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
        // If loadCapacityKg is zero or negative, an ArgumentOutOfRangeException is thrown to prevent initializing a washing machine with an invalid capacity weight.
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
            // EnsureRepairIsOpen checks if the repair job is active, blocking any hardware component updates once the service is officially completed.
            EnsureRepairIsOpen("The drum replacement requirement");
            _needsDrumReplacement = value;
        }
    }

    public override string DeviceType => "Washing Machine";

    // It must be override to provide specialized calculation logic for WashingMachine, which is declared as abstract in the Appliance base class.
    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost(LaborRatePerHour);

        // This condition adds a front-load surcharge because front-load units require more complex disassembly and labor compared to top-loaders.
        if (_isFrontLoad)
        {
            cost += FrontLoadSurcharge;
        }

        // This checks if a drum replacement is required, adding the heavy DrumReplacementFee to the total cost if true.
        if (_needsDrumReplacement)
        {
            cost += DrumReplacementFee;
        }

        return cost;
    }

    // It retrieves the core appliance details from the parent class to combine them with washing machine-specific specs.
    public override string GetStatusSummary()
    {
        string loading = _isFrontLoad ? "Front-load" : "Top-load";
        string drum = _needsDrumReplacement ? "yes" : "no";
        return $"{base.GetStatusSummary()} | {_loadCapacityKg} kg, {loading}, drum replacement: {drum}";
    }
}
