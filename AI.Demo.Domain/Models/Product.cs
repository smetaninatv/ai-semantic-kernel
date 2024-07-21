namespace AI.Demo.Domain.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; }

    public int SupplierId { get; set; }

    public string SupplierName { get; set; }
}