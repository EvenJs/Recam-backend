using Remp.Models.Enums;

namespace Remp.Service.DTOs.ListingCase;

public class ListingCaseResponse
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
  public ListcaseStatus ListcaseStatus { get; set; }
  public DateTime CreatedAt { get; set; }
  public string UserId { get; set; } = string.Empty;
}
