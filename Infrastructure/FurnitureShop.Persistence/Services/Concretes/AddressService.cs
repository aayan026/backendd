using AutoMapper;
using FurnitureShop.Application.Dtos.Address;
using FurnitureShop.Application.Exceptions;
using FurnitureShop.Application.Repsitories.ReadRepositories;
using FurnitureShop.Application.Repsitories.WriteRepositories;
using FurnitureShop.Application.Services.Abstracts;
using FurnitureShop.Application.Validation;
using FurnitureShop.Domain.Entities.Concretes;

namespace FurnitureShop.Persistence.Services.Concretes;

public class AddressService : IAddressService
{
    private readonly IAddressReadRepository _readRepo;
    private readonly IAddressWriteRepository _writeRepo;
    private readonly ILanguageService _langService;
    private readonly IMapper _mapper;

    private string lang => _langService.GetCurrentLanguage();


    public AddressService(
        IAddressReadRepository readRepo,
        IAddressWriteRepository writeRepo,
        ILanguageService langService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _langService = langService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AddressDto>> GetAllAsync(string userId)
    {
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

    public async Task<AddressDto> CreateAsync(string userId, CreateAddressDto dto)
    {
        // əgər bu birinci ünvandırsa avtomatik default et
        var existing = await _readRepo.GetByUserIdAsync(userId);
        var isFirst = !existing.Any();

        var address = _mapper.Map<Address>(dto);
        address.UserId = userId;

        if (isFirst)
            address.IsDefault = true;

        // yeni ünvan default olaraq seçilibsə köhnəni sıfırla
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
    }

    public async Task DeleteAsync(string userId, int addressId)
    {

        var address = await _readRepo.GetByIdAsync(addressId);
        if (address is null || address.UserId != userId)
            throw new NotFoundException(ValidationMessages.Get(lang, "AddressNotFound"));

        _writeRepo.Delete(address);
        await _writeRepo.SaveChangesAsync();

        // silinən default idi isə başqa birinə default ver
        if (address.IsDefault)
        {
            var remaining = (await _readRepo.GetByUserIdAsync(userId)).FirstOrDefault();
            if (remaining is not null)
            {
                remaining.IsDefault = true;
                _writeRepo.Update(remaining);
                await _writeRepo.SaveChangesAsync();
            }
        }
    }
}
