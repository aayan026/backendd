using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Enums;
using FurnitureShop.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;


public class Order:BaseEntity
{
    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    public int? DiscountCodeId { get; set; }
    public DiscountCode? DiscountCode { get; set; }
    public DeliveryInfo DeliveryInfo { get; set; }
    public OrderType Type { get; set; }
  public OrderStatus Status { get; set; }

    public decimal ShippingCost { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? Note { get; set; }
    public string UserId { get; set; }
    public AppUser? User { get; set; }
    public decimal TotalPrice { get; set; }
    public ICollection<OrderItem> Items { get; set; }
}
