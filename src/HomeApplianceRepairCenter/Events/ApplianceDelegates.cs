using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Events;

/// <summary>
/// Signature of the handlers notified when an appliance changes its repair status.
/// </summary>
public delegate void StatusChangedHandler(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus);

/// <summary>
/// Signature of the handlers notified when an appliance misses its promised delivery date.
/// </summary>
public delegate void DeliveryOverdueHandler(Appliance appliance, int daysOverdue);
