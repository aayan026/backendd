using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Application.Repsitories.Common;

public interface IGenericRepository<T> where T : class
{
    DbSet<T> Table { get; }
}
