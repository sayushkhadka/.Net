namespace Stock_Management.Data;

//The specifics of the graph's structure are included in this class.

public class StockDataGraph
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
}
