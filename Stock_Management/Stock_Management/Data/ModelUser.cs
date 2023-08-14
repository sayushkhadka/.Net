using System.Data;

namespace Stock_Management.Data;

//This class contains the details of the user in json file format.


public class ModelUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public bool HasInitialPassword { get; set; } = true;
    public TypeUser UType { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.Now;
    public Guid RegisterdBy { get; set; }
}
