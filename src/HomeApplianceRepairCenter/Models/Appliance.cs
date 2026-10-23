using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Helpers;
using HomeApplianceRepairCenter.Interfaces;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Abstract base class of every appliance handled by the center.
/// It owns the shared data and the repair workflow; each subclass decides
/// how its own repair cost is calculated.
/// </summary>
public abstract class Appliance : IRepairable
{
    // ----- Static statistics shared by all appliances -----
    private static int _underRepairCount;
    private static int _completedRepairsCount;
    private static decimal _totalRevenue;

    /// <summary>Number of appliances that are currently under repair.</summary>
    public static int UnderRepairCount => _underRepairCount;

    /// <summary>Number of appliances whose repair is finished (Completed or Delivered).</summary>
    public static int CompletedRepairsCount => _completedRepairsCount;

    /// <summary>Sum of the final costs (USD) of all completed repairs.</summary>
    public static decimal TotalRevenue => _totalRevenue;

    // ----- Instance state: every field is private -----
    private readonly string _serialNumber;
    private readonly string _brand;
    private readonly string _model;
    private readonly DateTime _receivedDate;
    private string _customerName = string.Empty;
    private string _faultDescription = string.Empty;
    private DateTime _expectedDeliveryDate;
    private decimal _partsCost;
    private decimal _laborHours;
    private RepairStatus _status = RepairStatus.Received;
    private DateTime? _completedDate;

    // ----- Events -----

    /// <summary>Raised every time the repair status changes.</summary>
    public event StatusChangedHandler? StatusChanged;

    /// <summary>Raised when the promised delivery date has passed and the repair is not finished.</summary>
    public event DeliveryOverdueHandler? DeliveryOverdue;

    /// <summary>
    /// Creates an appliance. Serial number, brand, model and received date are fixed
    /// for the whole life of the object.
    /// </summary>
    protected Appliance(
        string serialNumber,
        string brand,
        string model,
        string customerName,
        string faultDescription,
        DateTime receivedDate,
        DateTime expectedDeliveryDate)
    {
        if (!ValidationHelper.IsValidSerialNumber(serialNumber))
        {
            throw new ArgumentException(
                "Serial number must look like AAA-123456 (3 capital letters, a dash, 6 digits).",
                nameof(serialNumber));
        }

        if (!ValidationHelper.IsValidText(brand))
        {
            throw new ArgumentException("Brand is required.", nameof(brand));
        }

        if (!ValidationHelper.IsValidText(model))
        {
            throw new ArgumentException("Model is required.", nameof(model));
        }

        _serialNumber = serialNumber;
        _brand = brand.Trim();
        _model = model.Trim();
        _receivedDate = receivedDate.Date;

        // The setters below validate their own values.
        CustomerName = customerName;
        FaultDescription = faultDescription;
        ExpectedDeliveryDate = expectedDeliveryDate;
    }

    // ----- Read-only data (cannot change after creation) -----

    public string SerialNumber => _serialNumber;

    public string Brand => _brand;

    public string Model => _model;

    public DateTime ReceivedDate => _receivedDate;

    // ----- Data changed only through the workflow methods -----

    public RepairStatus Status => _status;

    public DateTime? CompletedDate => _completedDate;

    // ----- Data that can be edited while the repair is still open -----

    public string CustomerName
    {
        get => _customerName;
        set
        {
            if (!ValidationHelper.IsValidText(value))
            {
                throw new ArgumentException("Customer name must have at least 2 characters.", nameof(value));
            }

            _customerName = value.Trim();
        }
    }

    public string FaultDescription
    {
        get => _faultDescription;
        set
        {
            if (!ValidationHelper.IsValidText(value, 3))
            {
                throw new ArgumentException("Fault description must have at least 3 characters.", nameof(value));
            }

            EnsureRepairIsOpen("The fault description");
            _faultDescription = value.Trim();
        }
    }

    public DateTime ExpectedDeliveryDate
    {
        get => _expectedDeliveryDate;
        set
        {
            if (!ValidationHelper.IsValidDeliveryDate(_receivedDate, value))
            {
                throw new ArgumentException("Delivery date cannot be earlier than the received date.", nameof(value));
            }

            EnsureRepairIsOpen("The delivery date");
            _expectedDeliveryDate = value.Date;
        }
    }

    /// <summary>Cost (USD) of the spare parts used in the repair.</summary>
    public decimal PartsCost
    {
        get => _partsCost;
        set
        {
            if (!ValidationHelper.IsNonNegative(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parts cost cannot be negative.");
            }

            EnsureRepairIsOpen("The parts cost");
            _partsCost = value;
        }
    }

    /// <summary>Technician working hours spent on the repair.</summary>
    public decimal LaborHours
    {
        get => _laborHours;
        set
        {
            if (!ValidationHelper.IsNonNegative(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Labor hours cannot be negative.");
            }

            EnsureRepairIsOpen("The labor hours");
            _laborHours = value;
        }
    }

    // ----- Members every subclass must provide -----

    /// <summary>Human-readable appliance type, e.g. "Refrigerator".</summary>
    public abstract string DeviceType { get; }

    /// <summary>Repair cost (USD); every appliance type has its own pricing rules.</summary>
    public abstract decimal CalculateRepairCost();

    // ----- Repair workflow -----

    public void StartRepair()
    {
        if (_status != RepairStatus.Received)
        {
            throw new InvalidOperationException(
                $"Cannot start the repair of {_serialNumber}: its status is {_status}.");
        }

        ChangeStatus(RepairStatus.UnderRepair);
    }

    public void CompleteRepair()
    {
        if (_status != RepairStatus.UnderRepair)
        {
            throw new InvalidOperationException(
                $"Cannot complete the repair of {_serialNumber}: its status is {_status}.");
        }

        _completedDate = DateTime.Today;
        _totalRevenue += CalculateRepairCost();
        ChangeStatus(RepairStatus.Completed);
    }

    public void DeliverToCustomer()
    {
        if (_status != RepairStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Cannot deliver {_serialNumber}: its status is {_status}.");
        }

        ChangeStatus(RepairStatus.Delivered);
    }

    /// <summary>True when the repair is still open after the promised delivery date.</summary>
    public bool IsOverdue(DateTime today)
        => _status < RepairStatus.Completed && today.Date > _expectedDeliveryDate;

    /// <summary>Raises <see cref="DeliveryOverdue"/> if the appliance is late; returns true in that case.</summary>
    public bool CheckDeliveryDeadline(DateTime today)
    {
        if (!IsOverdue(today))
        {
            return false;
        }

        OnDeliveryOverdue((today.Date - _expectedDeliveryDate).Days);
        return true;
    }

    /// <summary>One-line description; subclasses extend it with their own details.</summary>
    public virtual string GetStatusSummary()
    {
        return $"[{DeviceType}] {_serialNumber} | {_brand} {_model} | Customer: {_customerName} | " +
               $"Status: {_status} | Due: {_expectedDeliveryDate.ToString(ValidationHelper.DateFormat)} | " +
               $"Cost: {CalculateRepairCost():N2}";
    }

    // ----- Helpers available to subclasses -----

    /// <summary>Parts cost plus labor hours multiplied by the hourly rate of the appliance type.</summary>
    protected decimal CalculateBaseCost(decimal hourlyRate) => _partsCost + (_laborHours * hourlyRate);

    /// <summary>Blocks changes once the repair is completed, so the final cost stays fixed.</summary>
    protected void EnsureRepairIsOpen(string what)
    {
        if (_status >= RepairStatus.Completed)
        {
            throw new InvalidOperationException($"{what} cannot be changed after the repair is completed.");
        }
    }

    protected virtual void OnStatusChanged(RepairStatus oldStatus, RepairStatus newStatus)
        => StatusChanged?.Invoke(this, oldStatus, newStatus);

    protected virtual void OnDeliveryOverdue(int daysOverdue)
        => DeliveryOverdue?.Invoke(this, daysOverdue);

    // ----- Private implementation -----

    private void ChangeStatus(RepairStatus newStatus)
    {
        RepairStatus oldStatus = _status;
        _status = newStatus;

        // Keep the static statistics in sync with the status change.
        if (oldStatus == RepairStatus.UnderRepair)
        {
            _underRepairCount--;
        }

        if (newStatus == RepairStatus.UnderRepair)
        {
            _underRepairCount++;
        }

        if (newStatus == RepairStatus.Completed)
        {
            _completedRepairsCount++;
        }

        OnStatusChanged(oldStatus, newStatus);
    }
}
