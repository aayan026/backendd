using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;

namespace FurnitureShop.Persistence.Repositories.WriteRepositories;

public class OrderWriteRepository : GenericWriteRepository<Order>, IOrderWriteRepository
{
    public OrderWriteRepository(AppDbContext context) : base(context) { }
}
