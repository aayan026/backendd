using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;

namespace FurnitureShop.Persistence.Repositories.WriteRepositories;

public class AddressWriteRepository : GenericWriteRepository<Address>, IAddressWriteRepository
{
    public AddressWriteRepository(AppDbContext context) : base(context) { }
}
