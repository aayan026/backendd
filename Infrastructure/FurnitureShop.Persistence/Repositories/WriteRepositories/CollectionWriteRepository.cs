using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;

namespace FurnitureShop.Persistence.Repositories.WriteRepositories;

public class CollectionWriteRepository : GenericWriteRepository<Collection>, ICollectionWriteRepository
{
    public CollectionWriteRepository(AppDbContext context) : base(context) { }
}
