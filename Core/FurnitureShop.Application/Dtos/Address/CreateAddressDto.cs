namespace FurnitureShop.Application.Dtos.Address;
public class CreateAddressDto
{
    public string Label { get; set; } = null!;
    public string ContactName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? District { get; set; }
    public string AddressLine { get; set; } = null!;
    public string? ZipCode { get; set; }
    public bool IsDefault { get; set; }
}
