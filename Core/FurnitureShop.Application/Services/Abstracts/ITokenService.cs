using FurnitureShop.Application.Dtos.Auth;
using FurnitureShop.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureShop.Application.Services.Abstracts;
public interface ITokenService
{
    Task<TokenResponseDto> CreateTokenAsync(AppUser user);
    string CreateRefreshToken();
    Task<string> CreateAccessTokenAsync(AppUser user);
}