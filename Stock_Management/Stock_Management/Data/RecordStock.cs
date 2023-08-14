namespace Stock_Management.Data;

//The structure for the various actions taken as a record on the stock is contained in this class.

public class RecordStock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public Guid TakenBy { get; set; }
    public Guid ActionedBy { get; set; }
    public Status Action { get; set; } = Status.Pending;
    public int Quantity { get; set; }
    public DateTime DateRequested { get; set; } = DateTime.Now;
    public DateTime DateApproved { get; set; }
}
