using Newtonsoft.Json;

namespace AI.Demo.Domain.Models;

public class Product
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("product_name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("category_id")]
    public int CategoryId { get; set; }

    [JsonProperty("category_name")]
    public string CategoryName { get; set; }

    [JsonProperty("supplier_id")]
    public int SupplierId { get; set; }

    [JsonProperty("supplier_name")]
    public string SupplierName { get; set; }
}