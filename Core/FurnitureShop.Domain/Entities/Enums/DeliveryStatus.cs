using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Enums;

public enum DeliveryStatus
{
    Scheduled = 1,    
    OutForDelivery = 2, 
    Delivered = 3,   
    Failed = 4,       
    Rescheduled = 5 
}
