using AutoMapper;
using FurnitureShop.Application.Dtos.DiscountCode;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Enums;

namespace FurnitureShop.Persistence.Services.Concretes;

public class DiscountCodeService : IDiscountCodeService
{
    private readonly IDiscountCodeReadRepository _readRepo;
    private readonly IDiscountCodeWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;

    private string Lang => _langService.GetCurrentLanguage();

    public DiscountCodeService(
        IDiscountCodeReadRepository readRepo,
        IDiscountCodeWriteRepository writeRepo,
        ILanguageService langService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
    }

    public async Task<DiscountCodeValidationResult> ValidateAsync(ValidateDiscountCodeDto dto)
    {
        var code = await _readRepo.GetByCodeAsync(dto.Code);

        if (code is null || code.Status != DiscountStatus.Active)
            return new DiscountCodeValidationResult
            {
                IsValid = false,
                Message = ValidationMessages.Get(Lang, "DiscountCodeNotFound")
            };

        if (code.ExpiresAt.HasValue && code.ExpiresAt < DateTime.UtcNow)
            return new DiscountCodeValidationResult
            {
                IsValid = false,
                Message = ValidationMessages.Get(Lang, "DiscountCodeExpired")
            };

        if (code.MaxUses.HasValue && code.UsedCount >= code.MaxUses)
            return new DiscountCodeValidationResult
            {
                IsValid = false,
                Message = ValidationMessages.Get(Lang, "DiscountCodeUsedUp")
            };

        if (code.MinOrderAmount.HasValue && dto.OrderTotal < code.MinOrderAmount)
            return new DiscountCodeValidationResult
            {
                IsValid = false,
                Message = ValidationMessages.Get(Lang, "DiscountCodeMinAmount", code.MinOrderAmount)
            };

        var discount = code.Type == DiscountType.Percent
            ? dto.OrderTotal * code.Value / 100
            : code.Value;

        discount = Math.Min(discount, dto.OrderTotal);

        return new DiscountCodeValidationResult
        {
            IsValid = true,
            Message = ValidationMessages.Get(Lang, "Success"),
            DiscountAmount = discount,
            FinalTotal = dto.OrderTotal - discount
        };
    }

    public async Task<IEnumerable<DiscountCodeDto>> GetAllAsync()
    {
        var codes = await _readRepo.GetAllAsync();
        return _mapper.Map<IEnumerable<DiscountCodeDto>>(codes);
    }

    public async Task<IEnumerable<DiscountCodeDto>> GetActiveAsync()
    {
        var codes = await _readRepo.GetActiveAsync();
        return _mapper.Map<IEnumerable<DiscountCodeDto>>(codes);
    }

    public async Task<DiscountCodeDto?> GetByIdAsync(int id)
    {
        var code = await _readRepo.GetByIdAsync(id);
        if (code is null) throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));
        return _mapper.Map<DiscountCodeDto>(code);
    }

    public async Task<int> CreateAsync(CreateDiscountCodeDto dto)
    {
        var code = _mapper.Map<DiscountCode>(dto);
        code.Status = DiscountStatus.Active;
        await _writeRepo.AddAsync(code);
        await _writeRepo.SaveChangesAsync();
        return code.Id;
    }

    public async Task DeactivateAsync(int id)
    {
        var code = await _readRepo.GetByIdAsync(id);
        if (code is null) throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));
        code.Status = DiscountStatus.Passive;
        _writeRepo.Update(code);
        await _writeRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var code = await _readRepo.GetByIdAsync(id);
        if (code is null) throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));
        _writeRepo.RemoveAsync(code);
        await _writeRepo.SaveChangesAsync();
    }
}
