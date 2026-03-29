using Remp.Models.Enums;
using Remp.Service.DTOs.CaseContact;
using Remp.Service.DTOs.MediaAsset;

namespace Remp.Service.DTOs.ListingCase;

public class ListingCasePreviewResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int Postcode { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }
    public double? Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Garages { get; set; }
    public double? FloorArea { get; set; }
    public PropertyType PropertyType { get; set; }
    public SaleCategory SaleCategory { get; set; }
    public string? ShareableUrl { get; set; }

    // Hero image
    public MediaAssetResponse? HeroImage { get; set; }

    // Selected images only
    public List<MediaAssetResponse> SelectedMedia { get; set; } = new();

    // Contacts
    public List<CaseContactResponse> Contacts { get; set; } = new();
}