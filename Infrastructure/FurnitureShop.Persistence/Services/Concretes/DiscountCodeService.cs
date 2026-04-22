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
        _log.Information("Endirim kodu yoxlanılır — Kod: {Code}", dto.Code);

        var code = await _readRepo.GetByCodeAsync(dto.Code);

        if (code is null || code.Status != DiscountStatus.Active)
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeNotFound") };

        if (code.ExpiresAt.HasValue && code.ExpiresAt < DateTime.UtcNow)
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeExpired") };

        if (code.MaxUses.HasValue && code.UsedCount >= code.MaxUses)
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeUsedUp") };

        if (code.MinOrderAmount.HasValue && dto.OrderTotal < code.MinOrderAmount)
            return new DiscountCodeValidationResult { IsValid = false, Message = ValidationMessages.Get(Lang, "DiscountCodeMinAmount", code.MinOrderAmount) };

        var discount = code.Type == DiscountType.Percent
            ? dto.OrderTotal * code.Value / 100
            : code.Value;
        discount = Math.Min(discount, dto.OrderTotal);

        return new DiscountCodeValidationResult
        {
            IsValid = true,
            Message = ValidationMessages.Get(Lang, "Success"),
            DiscountCodeId = code.Id,
            DiscountAmount = discount,
            FinalTotal = dto.OrderTotal - discount
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
        _log.Information("Yeni endirim kodu yaradılır — Kod: {Code}", dto.Code);

        var existing = await _readRepo.GetByCodeAsync(dto.Code);
        if (existing is not null)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "code", new List<string> { ValidationMessages.Get(Lang, "DiscountCodeDuplicate") } } });

        if (dto.Type == DiscountType.Percent && (dto.Value < 1 || dto.Value > 100))
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "value", new List<string> { ValidationMessages.Get(Lang, "DiscountCodeInvalidValue") } } });

        if (dto.Value <= 0)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "value", new List<string> { ValidationMessages.Get(Lang, "GreaterThanZero", "Endirim dəyəri") } } });

        if (dto.ExpiresAt.HasValue && dto.ExpiresAt <= DateTime.UtcNow)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "expiresAt", new List<string> { ValidationMessages.Get(Lang, "FutureDate", "Bitiş tarixi") } } });

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
        if (code is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));

        if (code.Status == DiscountStatus.Passive)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "code", new List<string> { ValidationMessages.Get(Lang, "DiscountCodeAlreadyInactive") } } });

        code.Status = DiscountStatus.Passive;
        _writeRepo.Update(code);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Endirim kodu deaktiv edildi — Id: {Id}", id);
    }

    public async Task DeleteAsync(int id)
    {
        var code = await _readRepo.GetByIdAsync(id);
        if (code is null)
            throw new NotFoundException(ValidationMessages.Get(Lang, "DiscountCodeNotFound"));

        if (code.UsedCount > 0)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>> { { "code", new List<string> { ValidationMessages.Get(Lang, "DiscountCodeHasBeenUsed") } } });

        await _writeRepo.RemoveAsync(code);
        await _writeRepo.SaveChangesAsync();
        _log.Information("Endirim kodu silindi — Id: {Id}", id);
    }
}
