using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Domain.Entities.Identity;
public class RefreshToken
{
    public int Id { get; set; }          
    public string Token { get; set; } = null!;
    public DateTime ExpireDate { get; set; }
    public bool IsRevoked { get; set; }
    public string UserId { get; set; } = null!; 
    public AppUser User { get; set; } = null!;
}