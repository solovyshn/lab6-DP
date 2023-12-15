using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Server
{
    public class LibraryHub : Hub
    {
        private static readonly ISubject<Order> _librarySubject = new Subject<Order>();

        public LibraryHub()
        {
        }

        static LibraryHub()
        {
            _librarySubject
                .Select(order => order.Book.Year)
                .Take(11)
                .Aggregate(0, (acc, next) =>  acc+ next)
                .Subscribe(x => {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Average year of books: {x/11}"); });
            _librarySubject
                .Where(IsDelayed)
                .Buffer(TimeSpan.FromSeconds(10))
                .Subscribe(books => HandleDelayed(books));
            _librarySubject
                .Where(IsTook)
                .Skip(3)
                .Subscribe(books => HandleTook(books));
            _librarySubject
                .Where(IsReturned)
                .Take(20)
                .Subscribe(books => HandleReturned(books));
            _librarySubject
                .Where(IsDelayed)
                .Select(delayedBecomeReturned)
                .Take(10)
                .Subscribe(book => HandleSelection(book));
            _librarySubject
                .Where(IsDelayed)
                .Merge(_librarySubject.Where(IsTook))
                .Take(15)
                .Subscribe(x =>
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"Book {x.Book.Title} with status {x.orderStatus}");
                });

        }

        
        private static bool IsDelayed(Order data)
        {
            return data.orderStatus==OrderStatus.delayed;
        }
        private static bool IsTook(Order data)
        {
            return data.orderStatus == OrderStatus.took;
        }
        private static bool IsReturned(Order data)
        {
            return data.orderStatus == OrderStatus.returned;
        }
        private static Order delayedBecomeReturned(Order data)
        {
            if (data.orderStatus == OrderStatus.delayed)
                data.orderStatus = OrderStatus.returned;

            return data;

        }
        public async Task ProcessMessage(string message)
        {
            try
            {
                var libraryData = JsonConvert.DeserializeObject<Order>(message);
                _librarySubject.OnNext(libraryData);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception in ProcessMessage: {ex.Message}");
                throw; // Rethrow the exception if needed
            }
        }


        private static void HandleDelayed(IList<Order> data)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"5 delayed books:");
            foreach (var item in data)
            {
                Console.WriteLine($"User {item.User} {item.orderStatus} the book {item.Book.Title} by {item.Book.Author}.");
            }   
        }
        private static void HandleReturned(Order item)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"User {item.User} {item.orderStatus} the book {item.Book.Title} by {item.Book.Author}.");
        }
        private static void HandleTook(Order item)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"User {item.User} {item.orderStatus} the book {item.Book.Title} by {item.Book.Author}.");
        }
        private static void HandleSelection(Order data)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Book with changed delayed status to returned:");
            Console.WriteLine($"User {data.User} {data.orderStatus} the book {data.Book.Title} by {data.Book.Author}.");

        }
    }
    public class Order
    {
        public string User { get; set; }
        public Book Book { get; set; }
        public OrderStatus orderStatus { get; set; }
    }

    public enum OrderStatus
    {
        took,
        returned,
        delayed
    }

    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
    }
}

