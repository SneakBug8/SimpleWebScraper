using System;

namespace webscraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Scraper");
            Console.WriteLine("-----");
            Console.WriteLine("Please, write command to execute");
            Console.WriteLine("1) Scrape verybest.ru");
            Console.WriteLine("2) Scrape gradys-plus.ru");

            var command = Console.ReadLine();

            if (command == "1") {
                var Scraper = new galser.Scraper();
                Scraper.Work();
            }
            
            if (command == "2") {
                var Scraper = new gradusplus.Scraper();
                Scraper.Work();
            }

            while (true) {
                
            }
        }
    }
}
