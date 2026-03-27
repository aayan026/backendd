using System.Linq.Expressions;

namespace FurnitureShop.Application.Repsitories.Common;

public interface IGenericReadRepository<T> : IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll();
    IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);

    Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
