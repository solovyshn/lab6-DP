using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace LibraryService
{
    public class LibraryClient
    {
        private HubConnection hubConnection;
        private Random random = new Random();
        List<BookDTO> books;

        public void InitializeConnection()
        {
            string jsonFilePath = "C:/Alaska/studying/university/7 Sem/declarative programming/lab6/lab6/LibraryService/books.json";

            // Read the entire JSON file
            string jsonString = File.ReadAllText(jsonFilePath);

            // Deserialize the JSON string into a list of BookDTO objects using Newtonsoft.Json
            books = JsonConvert.DeserializeObject<List<BookDTO>>(jsonString);
            Console.WriteLine(books.Count);


            Console.WriteLine("Initializing connection to SignalR Hub...");

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/library")
                .Build();

            hubConnection.On<string>("ReceiveMessage", (message) =>
            {
                Console.WriteLine("Received Message: " + message);
            });
        }

        public async Task StartConnectionAsync()
        {
            try
            {
                await hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to hub: {ex.Message}");
            }
        }

        public async Task SendLibraryDataAsync()
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            Console.WriteLine("Press any key to stop sending messages...");
            while (!Console.KeyAvailable)
            {
                await timer.WaitForNextTickAsync();

                if (random.NextDouble() < 0.7) // 70% probability
                {
                    var libraryData = GenerateLibraryData();
                    var libraryJson = JsonConvert.SerializeObject(libraryData);
                    try
                    {
                        await hubConnection.InvokeAsync("ProcessMessage", libraryJson);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        Console.WriteLine($"Exception in ProcessMessage: {ex.Message}");
                        throw; // Rethrow the exception if needed
                    }

                }
            }
        }

        private OrderDTO GenerateLibraryData()
        {
            var order = random.Next(0, 3);
            var bookIndex = random.Next(0, books.Count);
            if(order == 0)
            {
                return new OrderDTO
                {
                    orderStatus = OrderStatus.took,
                    User = GetRandomName(),
                    Book = books[bookIndex]
                };

            } else if(order == 1)
            {
                return new OrderDTO
                {
                    orderStatus = OrderStatus.returned,
                    User = GetRandomName(),
                    Book = books[bookIndex]
                };
            }
            else
            {
                return new OrderDTO
                {
                    orderStatus = OrderStatus.delayed,
                    User = GetRandomName(),
                    Book = books[bookIndex]
                };
            }
        }
        private string GetRandomName()
        {
            string[] names = { "John", "Jane", "Bob", "Alice", "Charlie", "Emma" };
            string[] surnames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis" };
            int indexName = random.Next(names.Length);
            int indexSurname = random.Next(surnames.Length);
            return names[indexName] + " " + surnames[indexSurname];
        }
        public async Task CloseConnectionAsync()
        {
            await hubConnection.DisposeAsync();
        }
    }
}
