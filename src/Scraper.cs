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
                product._PRICE_ = coststring;
                product._NAME_ = document.DocumentNode.SelectNodes("//header/h1[@class='accent']").Single().InnerText;
                product._STATUS_ = Convert.ToInt16(document.DocumentNode.SelectNodes("//div[@class='block_buy']/div[@class='presence yes sprite accent']") != null);

                product._MODEL_ = document.DocumentNode
                .SelectNodes("//tr/td[../th='Артикул']").First().InnerText;

                /* var image = document.DocumentNode.SelectSingleNode("//a[@class='img modal-open']/img");
                if (image != null)
                {
                    var imageurl = image.GetAttributeValue("src", null);
                    if (imageurl != null)
                    {
                        ThreadPool.QueueUserWorkItem((obj) => DownloadImage(imageurl, product.VendorCode + ".jpg"));
                    }
                } */
                Products.Add(product);
                Console.WriteLine(Products.Count + " - " + product._NAME_);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ": " + e.StackTrace);
                Console.WriteLine(document.DocumentNode.InnerHtml);
                
            }
        }

        void ParseNonActiveProduct(string url)
        {
            var product = new Product();
            product._STATUS_ = 0;
            Products.Add(product);
        }


        void EndParsing()
        {
            var csv = new CsvWriter(new System.IO.StreamWriter(@"C:\Users\sneak\Documents\result.csv"));
            csv.WriteRecords(Products);
        }

        public void DownloadImage(string url, string name)
        {
            name = name.Replace("&", "");
            name = name.Replace("-", "");
            name = name.Replace(";", "");
            
            string localFilename = @"C:\Users\sneak\Pictures\images\" + name;
            url = "https://www.verybest.ru" + url;
            Console.WriteLine("Downloading image " + name + " from " + url);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), localFilename);
            }
        }
    }
}