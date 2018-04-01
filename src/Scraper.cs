using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using CsvHelper;
using System.IO;

namespace webscraper
{
    public class Scraper
    {
        List<Product> Products = new List<Product>();
        List<string> UrlsToParse = new List<string>();
        public async void Work()
        {
            var starttime = DateTime.Now;
            RequestThroughXml("https://www.verybest.ru/sitemap_iblock_12.xml");
            // RequestThroughXml("https://www.verybest.ru/sitemap_iblock_19.xml");
            StartThreads();

            while (UrlsToParse.Count > 0) { }
            await Task.Delay(5000);
            EndParsing();
            Console.WriteLine((DateTime.Now - starttime).TotalMinutes);
            System.IO.File.WriteAllText(@"C:\Users\sneak\Documents\time.json", (DateTime.Now - starttime).TotalMinutes.ToString());
        }

        async void StartThreads()
        {
            for (int i = 0; i < 10; i++)
            {
                ThreadPool.QueueUserWorkItem(AutoRequest);
                await Task.Delay(100);
            }
        }

        async void RequestThroughXml(string url)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(url);

            Console.WriteLine(xmlDoc["urlset"].ChildNodes.Count);

            web = new HtmlWeb();

            foreach (XmlNode node in xmlDoc["urlset"].ChildNodes)
            {
                UrlsToParse.Add(node["loc"].InnerText);
            }
        }


        void AutoRequest(object sender)
        {
            if (UrlsToParse.Count > 0)
            {
                var url = UrlsToParse.First();
                UrlsToParse.RemoveAt(0);

                Request(url);
                ThreadPool.QueueUserWorkItem(AutoRequest);
            }
        }

        HtmlWeb web;

        public void Request(string url)
        {
            Console.WriteLine("Parsing page " + url);

            var doc = web.Load(url);

            if (url.Contains("/catalog/") && url.Count(x => x == '/') == 6)
            {
                Parse(doc, url);
            }
        }

        public void Parse(HtmlDocument document, string url)
        {
            if (document.DocumentNode.SelectNodes("//section[@class='not-found']") != null)
            {
                ParseNonActiveProduct(url);
                return;
            }

            if (document.DocumentNode.SelectSingleNode("//article[@class='offer_card']") != null)
            {
                ParseProduct(document);
            }
        }

        void ParseProduct(HtmlDocument document)
        {
            try
            {
                var product = new Product();
                var coststring = document.DocumentNode.SelectNodes("//div/div[@class='rub actual']").Single().InnerText;
                coststring = coststring.Replace(" ", "");
                Console.WriteLine(coststring);
                product.Cost = coststring;
                product.Name = document.DocumentNode.SelectNodes("//header/h1[@class='accent']").Single().InnerText;
                product.Available = document.DocumentNode.SelectNodes("//div[@class='block_buy']/div[@class='presence yes sprite accent']") != null;

                product.VendorCode = document.DocumentNode
                .SelectNodes("//table[@class='characteristic']/tbody/tr/td").First().InnerText;

                var image = document.DocumentNode.SelectNodes("//a[@class='img modal-open']/image").Single();
                if (image != null)
                {
                    var imageurl = image.GetAttributeValue("src", null);
                    if (imageurl != null)
                    {
                        ThreadPool.QueueUserWorkItem((obj) => DownloadImage(imageurl, product.Name + ".jpg"));
                    }
                }
                Products.Add(product);
                Console.WriteLine(Products.Count + " - " + product.Name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ": " + e.StackTrace);
            }
        }

        void ParseNonActiveProduct(string url)
        {
            var product = new Product();
            product.Available = false;
            Products.Add(product);
        }


        void EndParsing()
        {
            var csv = new CsvWriter(new System.IO.StreamWriter(@"C:\Users\sneak\Documents\result.csv"));
            csv.WriteRecords(Products);
        }

        public void DownloadImage(string url, string name)
        {
            string localFilename = @"C:\Users\sneak\Pictures\images\" + name;
            url = "https:" + url;
            Console.WriteLine("Downloading image " + name + " from " + url);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), localFilename);
            }
        }
    }
}