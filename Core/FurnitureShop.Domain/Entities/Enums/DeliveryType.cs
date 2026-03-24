using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Enums;
public enum DeliveryType
{
    DoorDelivery = 1,       // Yalnız qapıya qədər
    RoomDelivery = 2,       // Otağa aparırlar
    AssemblyIncluded = 3    // Otağa + quraşdırma
}

