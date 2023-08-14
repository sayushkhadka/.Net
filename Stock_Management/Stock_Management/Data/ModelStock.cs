using System;

namespace Stock_Management.Data;

//This class contains the data for the stocks in the json file format.

public class ModelStock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public int Quantity { get; set; }
    public float UnitPrice { get; set; }
    public Guid AddedBy { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.Now;
}
