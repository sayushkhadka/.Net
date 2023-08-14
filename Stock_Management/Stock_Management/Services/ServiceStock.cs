using Stock_Management.Data;
using System.Text.Json;

namespace Stock_Management.Services;

//This class stores all the important methods for handling the stock data.

public class ServiceStock
{
    private static void SaveItem(List<ModelStock> items)
    {
        string directoryPath = AllUtilities.GetDirectoryPath();
        string itemFilePath = AllUtilities.GetItemsFilePath();

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var serializedUserJson = JsonSerializer.Serialize(items);
        File.WriteAllText(itemFilePath, serializedUserJson);
    }

    public static void SaveItemHistory(List<RecordStock> itemHistory)
    {
        string directoryPath = AllUtilities.GetDirectoryPath();
        string itemHistoryFilePath = AllUtilities.GetItemsHistoryFilePath();

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var serializedUserJson = JsonSerializer.Serialize(itemHistory);
        File.WriteAllText(itemHistoryFilePath, serializedUserJson);
    }

    public static List<ModelStock> FetchItem()
    {
        string itemFilePath = AllUtilities.GetItemsFilePath();

        if (!File.Exists(itemFilePath))
        {
            return new List<ModelStock>();
        }

        var items = File.ReadAllText(itemFilePath);

        return JsonSerializer.Deserialize<List<ModelStock>>(items);
    }

    public static ModelStock FetchItemById(Guid Id)
    {
        List<ModelStock> items = FetchItem();
        ModelStock item = items.FirstOrDefault(item => item.Id == Id);

        if (item == null)
        {
            throw new Exception("Item not found.");
        }

        return item;
    }

    public static List<RecordStock> FetchItemHistory()
    {
        string itemHistoryFilePath = AllUtilities.GetItemsHistoryFilePath();

        if (!File.Exists(itemHistoryFilePath))
        {
            return new List<RecordStock>();
        }

        var itemHistory = File.ReadAllText(itemHistoryFilePath);

        return JsonSerializer.Deserialize<List<RecordStock>>(itemHistory);
    }

    public static List<RecordStock> FetchRejectedItemHistory()
    {
        List<RecordStock> records = FetchItemHistory();

        List<RecordStock> rejectedRecord = records.FindAll(record => record.Action == Status.Reject);

        return rejectedRecord;
    }

    public static List<RecordStock> FetchApprovedItemHistory()
    {
        List<RecordStock> records = FetchItemHistory();

        List<RecordStock> approvedRecord = records.FindAll(record => record.Action == Status.Approve);

        return approvedRecord;
    }

    public static List<RecordStock> FetchPendingItemHistory()
    {
        List<RecordStock> records = FetchItemHistory();

        List<RecordStock> pendingRecord = records.FindAll(record => record.Action == Status.Pending);

        return pendingRecord;
    }

    public static List<ModelStock> AddItem(Guid addedBy, string itemName, int quantity, float unitPrice)
    {
        List<ModelStock> items = FetchItem();

        items.Add(
            new ModelStock
            {
                AddedBy = addedBy,
                Name = itemName,
                Quantity = quantity,
                UnitPrice = unitPrice
            }
        );

        SaveItem(items);
        return items;
    }
    public static List<RecordStock> AddItemHistory(Guid itemId, Guid takerId, int quantity)
    {
        List<RecordStock> itemHistory = FetchItemHistory();

        itemHistory.Add(
            new RecordStock
            {
                ItemId = itemId,
                TakenBy = takerId,
                Quantity = quantity
            }
        );

        SaveItemHistory(itemHistory);
        return itemHistory;
    }

    public static List<ModelStock> DeleteItem(Guid Id)
    {
        List<ModelStock> items = FetchItem();
        ModelStock item = items.FirstOrDefault(item => item.Id == Id);

        if (item == null)
        {
            throw new Exception("Item not found.");
        }

        List<RecordStock> Records = FetchItemHistory();

        Records.RemoveAll(record => item.Id == record.ItemId);
        SaveItemHistory(Records);

        items.Remove(item);
        SaveItem(items);
        return items;
    }

    public static List<ModelStock> UpdateItem(Guid Id, string itemName, int quantity, float unitPrice)
    {
        List<ModelStock> items = FetchItem();
        ModelStock item = items.FirstOrDefault(item => item.Id == Id);

        if (item == null)
        {
            throw new Exception("Item not found.");
        }

        item.UnitPrice = unitPrice;
        item.Quantity = quantity;
        item.Name = itemName;

        SaveItem(items);
        return items;
    }

    public static List<ModelStock> TakeOutItem(Guid itemId, Guid takerId, int quantity)
    {
        if (quantity < 1)
        {
            throw new Exception("Takeout quantity must be atleast 1!");
        }

        List<ModelStock> items = FetchItem();
        ModelStock item = items.FirstOrDefault(item => item.Id == itemId);

        if (item == null)
        {
            throw new Exception("Item not found.");
        }

        if (item.Quantity < quantity)
        {
            throw new Exception("Quantity not available!");
        }

        ModelUser user = ServiceUser.FetchUserById(takerId);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        item.Quantity -= quantity;
        SaveItem(items);
        AddItemHistory(itemId, takerId, quantity);
        return items;
    }

    public static List<RecordStock> HandleTakeAction(Guid RecordId, Guid actionId, Status action)
    {
        List<RecordStock> Records = FetchItemHistory();
        RecordStock record = Records.FirstOrDefault(record => record.Id == RecordId);


        if (record == null)
        {
            throw new Exception("Records not found.");
        }

        List<ModelStock> items = FetchItem();
        ModelStock item = items.FirstOrDefault(item => item.Id == record.ItemId);

        if (action == Status.Reject)
        {
            record.Action = Status.Reject;
            item.Quantity += record.Quantity;
        }

        if (action == Status.Approve)
        {
            string message = "You can only approve at (9am-4pm) from (Monday-Friday)";
            if (DateTime.Parse("9: 00 AM") >= DateTime.Now || DateTime.Parse("4: 00 PM") <= DateTime.Now)
            {
                throw new Exception(message);
            }

            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                throw new Exception(message);
            }

            record.Action = Status.Approve;
        }

        record.ActionedBy = actionId;
        record.DateApproved = DateTime.Now;

        SaveItem(items);
        SaveItemHistory(Records);
        return Records;
    }

    public static List<StockDataGraph> ItemTakenData()
    {
        List<RecordStock> records = FetchApprovedItemHistory();
        List<StockDataGraph> datas = new();
        foreach (RecordStock record in records)
        {
            var exist = datas.FirstOrDefault(data => data.ItemId == record.ItemId);
            if (exist != null)
            {
                exist.Quantity += record.Quantity;
                continue;
            }

            datas.Add(
                new StockDataGraph
                {
                    ItemId = record.ItemId,
                    Quantity = record.Quantity,
                }
                );
        }

        return datas;
    }
}
