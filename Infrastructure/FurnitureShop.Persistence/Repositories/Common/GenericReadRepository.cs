using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Persistence.Datas;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FurnitureShop.Persistence.Repositories.Common;

public class GenericReadRepository<T> : GenericRepository<T>, IGenericReadRepository<T> where T : class
{
    public GenericReadRepository(AppDbContext context) : base(context) { }

    public IQueryable<T> GetAll()
        => Table.AsNoTracking();

    public IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate)
        => Table.AsNoTracking().Where(predicate);

    public async Task<T?> GetByIdAsync(int id)
        => await Table.FindAsync(id);

    public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate)
        => await Table.AsNoTracking().FirstOrDefaultAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await Table.AnyAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null
            ? await Table.CountAsync()
            : await Table.CountAsync(predicate);
}
