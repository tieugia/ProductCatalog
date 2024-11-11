using System.Text;

string filePath = "D:\\repos\\ProductCatalog\\ProductCatalog.Tests\\Products_100000.csv";
int totalRecords = 100000;

// Headers for CSV
StringBuilder csvContent = new StringBuilder();
csvContent.AppendLine("Id,Name,Description,Price,InventoryLevel,CategoryId,ImageUrl,CreatedAt,UpdatedAt");

// Random data generation
var random = new Random();

for (int i = 0; i < totalRecords; i++)
{
    var id = Guid.NewGuid();
    var name = $"Product {i + 1}";
    var description = $"Description for Product {i + 1}";
    var price = (random.NextDouble() * 100).ToString("F2");  // Random price between 0.00 and 100.00
    var inventoryLevel = random.Next(1, 500);  // Random inventory level between 1 and 500
    var categoryId = Guid.NewGuid();  // Random category ID
    var imageUrl = $"https://example.com/images/product{i + 1}.jpg";
    var createdAt = DateTime.UtcNow.AddDays(-random.Next(0, 100)).ToString("o");  // Random creation date within last 100 days
    var updatedAt = DateTime.UtcNow.ToString("o");  // Random updated date within last 100 days

    csvContent.AppendLine($"{id},{name},{description},{price},{inventoryLevel},{categoryId},{imageUrl},{createdAt},{updatedAt}");
}

// Write to file
File.WriteAllText(filePath, csvContent.ToString());

Console.WriteLine($"File with {totalRecords} records created successfully.");