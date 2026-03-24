using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Enums;
public enum OrderStatus
{
    Pending,      // Gözləyir
    Confirmed,    // Təsdiqləndi
    InProgress,   // Hazırlanır
    Delivered,    // Çatdırıldı
    Cancelled     // Ləğv edildi
}