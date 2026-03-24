using FurnitureShop.Application.Dtos.Campaign;

namespace FurnitureShop.Application.Services.Abstracts;

public interface ICampaignService
{
    Task<IEnumerable<CampaignDto>> GetActiveAsync();
    Task<IEnumerable<CampaignDto>> GetAllAsync();
    Task<int> CreateAsync(CreateCampaignDto dto);
    Task DeleteAsync(int id);
}
