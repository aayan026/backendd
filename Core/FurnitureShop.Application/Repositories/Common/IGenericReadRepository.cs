using System.Linq.Expressions;

namespace FurnitureShop.Application.Repsitories.Common;

public interface IGenericReadRepository<T> : IGenericRepository<T> where T : class
{
    // Hamısını gətir (tracking olmadan — read-only üçün)
    IQueryable<T> GetAll();

    // Filter ilə gətir
    IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate);

    // Id ilə gətir
    Task<T?> GetByIdAsync(int id);

    // Şərt ilə tək bir element gətir
    Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate);

    // Mövcudluğu yoxla
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    // Sayını gətir
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
