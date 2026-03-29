using FurnitureShop.Application.Repsitories.Common;
using FurnitureShop.Persistence.Datas;

namespace FurnitureShop.Persistence.Repositories.Common;

public class GenericWriteRepository<T> : GenericRepository<T>, IGenericWriteRepository<T> where T : class
{
    public GenericWriteRepository(AppDbContext context) : base(context) { }

    public async Task AddAsync(T entity)
        => await Table.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities)
        => await Table.AddRangeAsync(entities);

    public void Update(T entity)
        => Table.Update(entity);

    public void UpdateRange(IEnumerable<T> entities)
        => Table.UpdateRange(entities);

    public void Delete(T entity)
        => Table.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities)
        => Table.RemoveRange(entities);

    // FIX: async remove — DiscountCodeService-də await RemoveAsync istifadə olunur
    public Task RemoveAsync(T entity)
    {
        Table.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
