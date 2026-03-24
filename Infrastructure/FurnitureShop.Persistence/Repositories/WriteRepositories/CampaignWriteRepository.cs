using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Persistence.Datas;
using FurnitureShop.Persistence.Repositories.Common;

namespace FurnitureShop.Persistence.Repositories.WriteRepositories;

public class CampaignWriteRepository : GenericWriteRepository<Campaign>, ICampaignWriteRepository
{
    public CampaignWriteRepository(AppDbContext context) : base(context) { }
}
