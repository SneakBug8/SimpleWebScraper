using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace webscraper
{
    public class Scraper
    {
        Dictionary<string, Product> Products = new Dictionary<string, Product>();
        List<string> Urls404 = new List<string>();
        public void Work()
        {
            var starttime = DateTime.Now;
            RequestThroughXml("https://www.verybest.ru/sitemap_iblock_12.xml");
            // RequestThroughXml("https://www.verybest.ru/sitemap_iblock_19.xml");

            EndParsing();
            Console.WriteLine((DateTime.Now - starttime).TotalMinutes);
            System.IO.File.WriteAllText(@"C:\Users\sneak\Documents\time.json", (DateTime.Now - starttime).TotalMinutes.ToString());
        }

        async void RequestThroughXml(string url)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(url);

            Console.WriteLine(xmlDoc["urlset"].ChildNodes.Count);

            web = new HtmlWeb();

            foreach (XmlNode node in xmlDoc["urlset"].ChildNodes)
            {
                Request(node["loc"].InnerText);
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

            ParseProduct(document);
        }

        void ParseProduct(HtmlDocument document) {
            try
            {

                var product = new Product();
                product.Cost = document.DocumentNode.SelectNodes("//div/div[@class='rub actual']").Single().InnerText; ;
                product.Name = document.DocumentNode.SelectNodes("//header/h1[@class='accent']").Single().InnerText;
                product.Available = document.DocumentNode.SelectNodes("//div[@class='block_buy']/div[@class='presence yes sprite accent']") != null;

                if (!Products.ContainsKey(product.Name))
                {
                    Products.Add(product.Name, product);
                    Console.WriteLine(Products.Count + " - " + product.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        void ParseNonActiveProduct(string url) {
            Console.WriteLine(Urls404.Count + " - " + url);
            Urls404.Add(url);
        }


        void EndParsing()
        {
            var list = new List<Product>();

            foreach (var keyvalue in Products)
            {
                list.Add(keyvalue.Value);
            }

            var res = JsonConvert.SerializeObject(list);

            System.IO.File.WriteAllText(@"C:\Users\sneak\Documents\products.json", res);

            res = JsonConvert.SerializeObject(Urls404);
            System.IO.File.WriteAllText(@"C:\Users\sneak\Documents\404.json", res);
        }
    }
}