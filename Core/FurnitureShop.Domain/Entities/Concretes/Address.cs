using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Concretes;
public class Address :BaseEntity
{
    public string UserId { get; set; } = null!;
    public string Label { get; set; } = null!;       // "Ev", "İş"
    public string ContactName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? District { get; set; }
    public string AddressLine { get; set; } = null!;
    public string? ZipCode { get; set; }
    public bool IsDefault { get; set; } = false;

    public AppUser User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
