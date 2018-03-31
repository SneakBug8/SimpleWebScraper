using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace webscraper
{
    public class Scraper
    {
        readonly string BaseUrl = "https://www.furaffinity.net/view/{0}/";

        public async void Work()
        {
            int i = 1;
            int q = 0;
            DateTime startTime = DateTime.Now;
            while (true)
            {

                if (q == 0)
                {
                    startTime = DateTime.Now;
                }
                i++;
                q++;

                Request(string.Format(BaseUrl, i.ToString()));
                // await Task.Delay(300);

                if (q >= 100)
                {
                    var requesttime = DateTime.Now - startTime;
                    Console.WriteLine(requesttime.TotalSeconds + " per 100 requests.");
                    q = 0;
                    break;
                }
            }
        }

        public void Request(string url)
        {
            var web = new HtmlWeb();
            web.PreRequest = OnPreRequest;
            var doc = web.Load(url);

            Console.WriteLine("Parsing page " + url);
            Parse(doc);
        }

        public bool OnPreRequest(HttpWebRequest request)
        {

            var CookieContainer = new CookieContainer();
            var cookiea = new Cookie();
            cookiea.Name = "a";
            cookiea.Value = "977435ee-b239-4962-a9fe-65f708baad9f";
            var cookieb = new Cookie();
            cookieb.Name = "b";
            cookieb.Value = "1905be73-6085-4e04-a98a-d8bd1c7b23b6";
            CookieContainer.Add(new Uri("https://www.furaffinity.net"), cookiea);
            CookieContainer.Add(new Uri("https://www.furaffinity.net"), cookieb);
            request.CookieContainer = CookieContainer;
            return true;
        }

        public void Parse(HtmlDocument document)
        {
            var tagsnodes = document.DocumentNode.SelectNodes("//div/span[@class='tags']");
            var tags = new List<string>();

            if (tagsnodes != null)
            {
                tags = (from i in tagsnodes.ToList()
                        select i.InnerText).ToList();
            }
            else
            {
                Console.WriteLine("Tags:");
                return;
            }

            // Console.WriteLine(document.DocumentNode.InnerHtml);

            Console.Write("Tags:");
            foreach (var tag in tags)
            {
                Console.Write(tag + ", ");
            }

            Console.WriteLine();


            if (tags.Contains("background") && tags.Contains("space"))
            {
                var image = document.GetElementbyId("submissionImg");
                var imageurl = image.GetAttributeValue("src", null);

                if (imageurl != null)
                {
                    DownloadImage(imageurl, image.GetAttributeValue("alt", "new") + ".jpg");
                }
            }

        }

        public void DownloadImage(string url, string name)
        {
            string localFilename = @"C:\Users\sneak\Pictures\space\" + name;
            url = "https:" + url;
            Console.WriteLine("Downloading image " + name);
            Console.WriteLine(url);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), localFilename);
            }
        }
    }
}