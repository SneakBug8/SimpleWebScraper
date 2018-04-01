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

namespace webscraper.gradusplus
{
    public class Scraper
    {
        List<Product> Products = new List<Product>();
        List<string> UrlsToParse = new List<string>();
        public async void Work()
        {
            var starttime = DateTime.Now;
            RequestThroughXml("http://gradys-plus.ru/sitemap.xml");
            StartThreads();

            while (UrlsToParse.Count > 0) { }
            await Task.Delay(5000);
            EndParsing();
            Console.WriteLine("Scraping took " + (DateTime.Now - starttime).TotalMinutes + " minutes.");
        }

        async void StartThreads()
        {
            for (int i = 0; i < 10; i++)
            {
                ThreadPool.QueueUserWorkItem(AutoRequest);
                await Task.Delay(100);
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
            Parse(doc, url);
        }

        void RequestThroughXml(string url)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(url);

            Console.WriteLine("Will parse " + xmlDoc["urlset"].ChildNodes.Count + " pages");

            web = new HtmlWeb();

            Console.WriteLine("Started scraping");

            for (int i = 0; i < xmlDoc["urlset"].ChildNodes.Count; i++)
            {
                var node = xmlDoc["urlset"].ChildNodes[i];
                UrlsToParse.Add(node["loc"].InnerText);
            }
        }

        public void Parse(HtmlDocument document, string url)
        {
            if (document.DocumentNode.SelectSingleNode("//div[@class='product-info']") != null)
            {
                ParseProduct(document);
            }
        }

        void ParseProduct(HtmlDocument document)
        {
            var product = new Product();

            var coststring = (document.DocumentNode.SelectSingleNode("//span[@class='price-new']") ??
            document.DocumentNode.SelectSingleNode("//span[@class='price']") ??
            document.DocumentNode.SelectSingleNode("//span[@class='price-old']")).InnerText;
            coststring = coststring.Replace(" ", "");
            coststring = coststring.Replace("руб.", "");

            product._PRICE_ = coststring;
            product._NAME_ = document.DocumentNode.SelectSingleNode("//div[@class='span6']/h1").InnerText;
            product._NAME_ = product._NAME_.Replace("&quot;", "''");
            product._STATUS_ = Convert.ToInt16(document.DocumentNode.SelectSingleNode("//div[@class='prod-stock']").InnerHtml == "Есть в наличии");

            product._MODEL_ = document.DocumentNode
            .SelectSingleNode("//div[@class='product-section'/span[1]").InnerText;

            Products.Add(product);
            Console.WriteLine(Products.Count + " - " + product._NAME_);
        }

        void EndParsing()
        {
            Console.WriteLine("Please, write path to datafile, including .csv");
            var csv = new CsvWriter(new System.IO.StreamWriter(Console.ReadLine()));
            csv.Configuration.Delimiter = ";";
            csv.WriteRecords(Products);
        }
    }
}