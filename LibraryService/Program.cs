using LibraryService;

class Program
{
    static async Task Main(string[] args)
    {
        var libraryClient = new LibraryClient();

        libraryClient.InitializeConnection();
        await libraryClient.StartConnectionAsync();
        await libraryClient.SendLibraryDataAsync();
        await libraryClient.CloseConnectionAsync();
    }
}