using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService
{
    public enum OrderStatus
    {
        took,
        returned,
        delayed
    }

    public class BookDTO
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
    }

    public class OrderDTO
    {
        public string User { get; set; }
        public BookDTO Book { get; set; }
        public OrderStatus orderStatus { get; set; }
    }

}
