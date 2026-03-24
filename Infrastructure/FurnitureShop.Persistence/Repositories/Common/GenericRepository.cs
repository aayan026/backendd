using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Persistence.Datas;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Persistence.Repositories.Common;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    public DbSet<T> Table => _context.Set<T>();
}
