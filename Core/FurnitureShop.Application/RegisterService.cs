using AutoMapper;
using FurnitureShop.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace FurnitureShop.Application;

public static class RegisterService
{
    public static void AddApplicationRegister(this IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }
}