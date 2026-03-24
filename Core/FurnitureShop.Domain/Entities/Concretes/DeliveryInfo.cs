using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;

public class DeliveryInfo :BaseEntity
{
    public int OrderId { get; set; }

    public DeliveryType DeliveryType { get; set; }

    public DateTime ScheduledDate { get; set; }   // Müştərinin seçdiyi gün
    public string TimeSlot { get; set; } = null!; // "10:00-14:00", "14:00-18:00"

    public int Floor { get; set; } = 0;           // Neçənci mərtəbə
    public bool HasElevator { get; set; } = false;
    public bool RemoveOldFurniture { get; set; } = false; // Köhnəni aparsınlar?

    public string? DeliveryNote { get; set; }     // "Zəng etməyin, körpə yatır"
    public decimal DeliveryCost { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.Scheduled;

    // Navigation
    public Order Order { get; set; } = null!;
}