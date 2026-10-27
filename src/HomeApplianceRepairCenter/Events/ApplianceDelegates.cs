using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Events;

// A delegate is a type that referencs a method with a specific parameter list and return type.
// The StatusChanged event in Models/Appliance.cs uses this delegate, and it is raised inside the RepairStatus property setter.
/// <summary>
/// Signature of the handlers notified when an appliance changes its repair status.
/// </summary>
public delegate void StatusChangedHandler(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus);

// The daysOverdue parameter represents the number of days a repair is delayed, and NotificationService listens to this event.
/// <summary>
/// Signature of the handlers notified when an appliance misses its promised delivery date.
/// </summary>
public delegate void DeliveryOverdueHandler(Appliance appliance, int daysOverdue);
