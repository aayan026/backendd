using AutoMapper;
using FurnitureShop.Application.Dtos.Address;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;
using Serilog;

namespace FurnitureShop.Persistence.Services.Concretes;

public class AddressService : IAddressService
{
    private readonly IAddressReadRepository  _readRepo;
    private readonly IAddressWriteRepository _writeRepo;
    private readonly ILanguageService        _langService;
    private readonly IMapper                 _mapper;
    private static readonly ILogger _log = Log.ForContext<AddressService>();

    private string lang => _langService.GetCurrentLanguage();

    public AddressService(
        IAddressReadRepository  readRepo,
        IAddressWriteRepository writeRepo,
        ILanguageService        langService,
        IMapper                 mapper)
    {
        _readRepo    = readRepo;
        _writeRepo   = writeRepo;
        _langService = langService;
        _mapper      = mapper;
    }

    public async Task<IEnumerable<AddressDto>> GetAllAsync(string userId)
    {
        _log.Information("Ünvanlar sorğusu — UserId: {UserId}", userId);
        var addresses = await _readRepo.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    public async Task<AddressDto> GetByIdAsync(string userId, int addressId)
    {
        var address = await _readRepo.GetByIdAsync(addressId);
        if (address is null || address.UserId != userId)
            throw new NotFoundException(ValidationMessages.Get(lang, "AddressNotFound"));

        return _mapper.Map<AddressDto>(address);
    }

    private const int MaxAddressLimit = 5;

    public async Task<AddressDto> CreateAsync(string userId, CreateAddressDto dto)
    {
        _log.Information("Yeni ünvan əlavə edilir — UserId: {UserId} Şəhər: {City}", userId, dto.City);

        var existing = await _readRepo.GetByUserIdAsync(userId);
        var isFirst  = !existing.Any();

        // ── Biznes məntiq: İstifadəçi maksimum 5 ünvan əlavə edə bilər ──
        if (existing.Count() >= MaxAddressLimit)
            throw new Application.Exceptions.ValidationException(
                new Dictionary<string, List<string>>
                {
                    { "address", new List<string> { ValidationMessages.Get(lang, "AddressLimitReached", MaxAddressLimit) } }
                });

        var address = _mapper.Map<Address>(dto);
        address.UserId = userId;

        if (isFirst)
            address.IsDefault = true;

        if (dto.IsDefault && !isFirst)
        {
            var allAddresses = existing.ToList();
            foreach (var a in allAddresses)
            {
                a.IsDefault = false;
                _writeRepo.Update(a);
            }
        }

        await _writeRepo.AddAsync(address);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Ünvan yaradıldı — UserId: {UserId} AddressId: {AddressId} Default: {IsDefault}",
            userId, address.Id, address.IsDefault);

        return _mapper.Map<AddressDto>(address);
    }

    public async Task SetDefaultAsync(string userId, int addressId)
    {
        var allAddresses = (await _readRepo.GetByUserIdAsync(userId)).ToList();

        var target = allAddresses.FirstOrDefault(x => x.Id == addressId);
        if (target is null)
            throw new NotFoundException(ValidationMessages.Get(lang, "AddressNotFound"));

        foreach (var a in allAddresses)
        {
            a.IsDefault = (a.Id == addressId);
            _writeRepo.Update(a);
        }

        await _writeRepo.SaveChangesAsync();
        _log.Information("Default ünvan dəyişdirildi — UserId: {UserId} AddressId: {AddressId}", userId, addressId);
    }

    public async Task UpdateAsync(string userId, int addressId, UpdateAddressDto dto)
    {
        var address = await _readRepo.GetByIdAsync(addressId);
        if (address is null || address.UserId != userId)
            throw new NotFoundException(ValidationMessages.Get(lang, "AddressNotFound"));

        _mapper.Map(dto, address);

        if (dto.IsDefault)
        {
            var allAddresses = (await _readRepo.GetByUserIdAsync(userId)).ToList();
            foreach (var a in allAddresses.Where(x => x.Id != addressId && x.IsDefault))
            {
                a.IsDefault = false;
                _writeRepo.Update(a);
            }
            address.IsDefault = true;
        }

        _writeRepo.Update(address);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Ünvan yeniləndi — UserId: {UserId} AddressId: {AddressId}", userId, addressId);
    }

    public async Task DeleteAsync(string userId, int addressId)
    {
        var address = await _readRepo.GetByIdAsync(addressId);
        if (address is null || address.UserId != userId)
            throw new NotFoundException(ValidationMessages.Get(lang, "AddressNotFound"));

        _writeRepo.Delete(address);
        await _writeRepo.SaveChangesAsync();

        _log.Information("Ünvan silindi — UserId: {UserId} AddressId: {AddressId} DefaultIdimi: {WasDefault}",
            userId, addressId, address.IsDefault);

        if (address.IsDefault)
        {
            var remaining = (await _readRepo.GetByUserIdAsync(userId)).FirstOrDefault();
            if (remaining is not null)
            {
                remaining.IsDefault = true;
                _writeRepo.Update(remaining);
                await _writeRepo.SaveChangesAsync();
                _log.Information("Yeni default ünvan təyin edildi — UserId: {UserId} AddressId: {AddressId}", userId, remaining.Id);
            }
        }
    }
}
