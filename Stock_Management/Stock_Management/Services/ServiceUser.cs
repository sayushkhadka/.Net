 using Stock_Management.Data;
using System.Text.Json;

namespace Stock_Management.Services;

//This class stores all the important methods for handling the user data. 
public static class ServiceUser
{
    public const string DefaultUserName = "admin";
    public const string DefaultPassword = "test123";

    private static void SaveUser(List<ModelUser> users)
    {
        string directoryPath = AllUtilities.GetDirectoryPath();
        string userFilePath = AllUtilities.GetUsersFilePath();

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var serializedUserJson = JsonSerializer.Serialize(users);
        File.WriteAllText(userFilePath, serializedUserJson);
    }

    public static List<ModelUser> FetchUsers()
    {
        string userFilePath = AllUtilities.GetUsersFilePath();

        if (!File.Exists(userFilePath))
        {
            return new List<ModelUser>();
        }

        var userData = File.ReadAllText(userFilePath);
        return JsonSerializer.Deserialize<List<ModelUser>>(userData);
    }

    public static List<ModelUser> RegisterUser(Guid Id, string userName, string firstName, string lastName, string password, TypeUser type)
    {
        List<ModelUser> users = FetchUsers();
        bool userNameExistence = users.Any(user => user.UserName == userName);

        if (userNameExistence)
        {
            throw new Exception("Select a new username. Username unavailable.");
        }

        if (type == TypeUser.Admin)
        {
            int totalAdmin = users.Count(user => user.UType == TypeUser.Admin);

            if (totalAdmin > 1)
            {
                throw new Exception("Two admins have already been created.");
            }
        }

        users.Add(
            new ModelUser
            {
                UserName = userName,
                Password = AllUtilities.HashPassword(password),
                UType = type,
                RegisterdBy = Id,
                FirstName = firstName,
                LastName = lastName,
            }
        );

        SaveUser(users);
        return users;
    }

    public static ModelUser FetchUserById(Guid Id)
    {
        List<ModelUser> users = FetchUsers();

        return users.FirstOrDefault(user => user.Id == Id);
    }

    public static List<ModelUser> DeleteUser(Guid Id)
    {
        List<ModelUser> users = FetchUsers();
        ModelUser findUser = users.FirstOrDefault(user => user.Id == Id);

        if (findUser == null)
        {
            throw new Exception("Selected user not found.");
        }

        List<RecordStock> records = ServiceStock.FetchItemHistory();
        records.RemoveAll(record => findUser.Id == record.TakenBy || findUser.Id == record.ActionedBy);

        ServiceStock.SaveItemHistory(records);
        users.Remove(findUser);
        SaveUser(users);

        return users;
    }

    public static ModelUser Login(string username, string password)
    {
        var errorMessage = "Enter valid details.";

        List<ModelUser> users = FetchUsers();
        ModelUser user = users.FirstOrDefault(user => user.UserName == username);

        if (user == null)
        {
            throw new Exception(errorMessage);
        }

        bool checkPassword = AllUtilities.HashVerification(password, user.Password);

        if (!checkPassword)
        {
            throw new Exception(errorMessage);
        }

        return user;
    }

    public static ModelUser ChangeUserPassword(Guid id, string currentPassword, string newPassword)
    {
        List<ModelUser> users = FetchUsers();
        ModelUser user = users.FirstOrDefault(user => user.Id == id);

        if (user == null)
        {
            throw new Exception("User Not Found.");
        }

        bool verifyOldPassword = AllUtilities.HashVerification(currentPassword, user.Password);

        if (!verifyOldPassword)
        {
            throw new Exception("Enter valid details.");
        }

        if (currentPassword == newPassword)
        {
            throw new Exception("Current password was entered. Enter a new password.");
        }

        if (newPassword.Length < 1)
        {
            throw new Exception("Cannot enter empty password.");
        }

        user.Password = AllUtilities.HashPassword(newPassword);
        user.HasInitialPassword = false;

        SaveUser(users);
        return user;
    }

    public static ModelUser ChangeUserDetail(Guid id, string firstName, string lastname, string password)
    {
        List<ModelUser> users = FetchUsers();
        ModelUser user = users.FirstOrDefault(user => user.Id == id);

        if (user == null)
        {
            throw new Exception("User Not Found.");
        }

        bool verifyOldPassword = AllUtilities.HashVerification(password, user.Password);

        if (!verifyOldPassword)
        {
            throw new Exception("Enter valid details.");
        }

        if (firstName.Length < 1 || lastname.Length < 1)
        {
            throw new Exception("Name cannot be empty.");
        }

        user.FirstName = firstName;
        user.LastName = lastname;

        SaveUser(users);
        return user;
    }

    public static void SeedUserData()
    {
        var users = FetchUsers().FirstOrDefault(user => user.UType == TypeUser.Admin);

        if (users == null)
        {
            RegisterUser(Guid.Empty, DefaultUserName, "Admin", "Admin", DefaultPassword, TypeUser.Admin);
        }
    }
}
