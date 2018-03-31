using System;

namespace webscraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Scraper");

            var Scraper = new Scraper();

            Scraper.Work();

            while (true) {}
        }
    }
}
