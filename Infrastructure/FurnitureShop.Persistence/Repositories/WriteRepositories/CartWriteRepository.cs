using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;

namespace FurnitureShop.Persistence.Repositories.WriteRepositories;

public class CartWriteRepository : GenericWriteRepository<Cart>, ICartWriteRepository
{
    public CartWriteRepository(AppDbContext context) : base(context) { }
}
