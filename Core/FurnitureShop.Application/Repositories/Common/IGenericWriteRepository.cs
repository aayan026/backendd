namespace FurnitureShop.Application.Repsitories.Common;

public interface IGenericWriteRepository<T> : IGenericRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    Task RemoveAsync(T entity);
    Task<int> SaveChangesAsync();
}
