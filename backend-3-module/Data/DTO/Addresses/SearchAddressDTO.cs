namespace backend_3_module.Data.DTO.Addresses;

public class SearchAddressDTO
{
    public long Objectid { get; set; }
    public Guid Objectguid { get; set; }
    public string? Text { get; set; }
    public string ObjectLevel { get; set; }
    public string? ObjectLevelText { get; set; } 
}