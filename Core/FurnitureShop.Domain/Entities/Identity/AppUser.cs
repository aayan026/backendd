using FurnitureShop.Domain.Entities.Common;
using FurnitureShop.Domain.Entities.Concretes;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Identity;

public class AppUser : IdentityUser
{
    public AppUser()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string Name { get; set; }
    public string Surname { get; set; }
    public ICollection<Order> Orders { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

}
