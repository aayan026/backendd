using AutoMapper;
using FurnitureShop.Application.Dtos.DiscountCode;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using FurnitureShop.Domain.Entities.Enums;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class DiscountCodeService : IDiscountCodeService
{
    private readonly IDiscountCodeReadRepository  _readRepo;
    private readonly IDiscountCodeWriteRepository _writeRepo;
    private readonly ILanguageService             _langService;
    private readonly IMapper                      _mapper;
    private static readonly ILogger _log = Log.ForContext<DiscountCodeService>();

    private string Lang => _langService.GetCurrentLanguage();

    public DiscountCodeService(
        IDiscountCodeReadRepository  readRepo,
        IDiscountCodeWriteRepository writeRepo,
        ILanguageService             langService,
        IMapper                      mapper)
    {
        _readRepo    = readRepo;
        _writeRepo   = writeRepo;
        _langService = langService;
        _mapper      = mapper;
    }

    public async Task<DiscountCodeValidationResult> ValidateAsync(ValidateDiscountCodeDto dto)
    {
        _log.Information("Endirim kodu yoxlanılır — Kod: {Code} SifarişMəbləği: {OrderTotal}", dto.Code, dto.OrderTotal);

        var code = await _readRepo.GetByCodeAsync(dto.Code);

        if (code is null || code.Status != DiscountStatus.Active)
        {
            _log.Warning("Endirim kodu etibarsızdır — Kod: {Code}", dto.Code);
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeNotFound") };
        }

        if (code.ExpiresAt.HasValue && code.ExpiresAt < DateTime.UtcNow)
        {
            _log.Warning("Endirim kodu müddəti bitib — Kod: {Code} BitməTarixi: {ExpiresAt}", dto.Code, code.ExpiresAt);
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeExpired") };
        }

        if (code.MaxUses.HasValue && code.UsedCount >= code.MaxUses)
        {
            _log.Warning("Endirim kodu istifadə limiti dolub — Kod: {Code} Limit: {MaxUses}", dto.Code, code.MaxUses);
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeUsedUp") };
        }

        if (code.MinOrderAmount.HasValue && dto.OrderTotal < code.MinOrderAmount)
        {
            _log.Warning("Endirim kodu üçün minimum məbləğ çatmır — Kod: {Code} MinMəbləğ: {Min} SifarişMəbləği: {Total}", dto.Code, code.MinOrderAmount, dto.OrderTotal);
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeMinAmount", code.MinOrderAmount) };
        }

        var discount = code.Type == DiscountType.Percent
            ? dto.OrderTotal * code.Value / 100
            : code.Value;
        discount = Math.Min(discount, dto.OrderTotal);

        _log.Information("Endirim kodu qəbul edildi — Kod: {Code} EndiriMəbləği: {Discount} YekünMəbləğ: {Final}",
            dto.Code, discount, dto.OrderTotal - discount);

        return new DiscountCodeValidationResult
        {
            IsValid        = true,
            Message        = ValidationMessages.Get(Lang, "Success"),
            DiscountCodeId = code.Id,
            DiscountAmount = discount,
            FinalTotal     = dto.OrderTotal - discount
        };
    }

    public async Task<IEnumerable<DiscountCodeDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<DiscountCodeDto>>(await _readRepo.GetAllAsync());

    public async Task<IEnumerable<DiscountCodeDto>> GetActiveAsync()
        => _mapper.Map<IEnumerable<DiscountCodeDto>>(await _readRepo.GetActiveAsync());

    public async Task<DiscountCodeDto?> GetByIdAsync(int id)
    {
        var code = await _readRepo.GetByIdAsync(id);
        if (code is null) throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));
        return _mapper.Map<DiscountCodeDto>(code);
    }

    public async Task<int> CreateAsync(CreateDiscountCodeDto dto)
    {
        _log.Information("Yeni endirim kodu yaradılır — Kod: {Code} Növ: {Type} Dəyər: {Value}", dto.Code, dto.Type, dto.Value);
        var code = _mapper.Map<DiscountCode>(dto);
        code.Status = DiscountStatus.Active;
        await _writeRepo.AddAsync(code);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Endirim kodu yaradıldı — Id: {Id} Kod: {Code}", code.Id, code.Code);
        return code.Id;
    }

    public async Task DeactivateAsync(int id)
    {
        var code = await _readRepo.GetByIdAsync(id);
        if (code is null) throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));
        code.Status = DiscountStatus.Passive;
        _writeRepo.Update(code);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Endirim kodu deaktiv edildi — Id: {Id} Kod: {Code}", id, code.Code);
    }

    public async Task DeleteAsync(int id)
    {
        var code = await _readRepo.GetByIdAsync(id);
        if (code is null) throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));
        await _writeRepo.RemoveAsync(code);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Endirim kodu silindi — Id: {Id} Kod: {Code}", id, code.Code);
    }
}
