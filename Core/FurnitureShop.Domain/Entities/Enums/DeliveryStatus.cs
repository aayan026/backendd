using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Enums;

public enum DeliveryStatus
{
    Scheduled = 1,    // Planlaşdırıldı
    OutForDelivery = 2, // Yoldadır
    Delivered = 3,    // Çatdırıldı
    Failed = 4,       // Çatdırılmadı (evdə yox idi)
    Rescheduled = 5   // Yenidən planlaşdırıldı
}
