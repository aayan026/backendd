using FurnitureShop.Application.Common.Responses;
using FurnitureShop.Application.Dtos.Campaign;

namespace FurnitureShop.Application.Services.Abstracts;

public interface ICampaignService
{
    Task<IEnumerable<CampaignDto>> GetActiveAsync();
    Task<PagedList<CampaignDto>> GetAllAsync(PaginationParams pagination);
    Task<int> CreateAsync(CreateCampaignDto dto);
    Task UpdateAsync(int id, CreateCampaignDto dto);
    Task ToggleAsync(int id);
    Task DeleteAsync(int id);
}