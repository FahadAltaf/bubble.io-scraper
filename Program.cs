using CsvHelper;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace bubble.io_scraper
{
    public class DataModel
    {
        public string Title { get; set; }//
        public string DetailsUrl { get; set; }//
        public string Author { get; set; }//
        public string Price { get; set; }
        public string PreviewLink { get; set; }//
        public string CoverImage { get; set; }//
        public string Details { get; set; }//
        public string Category { get; set; }//
        public string PublishedDate { get; set; }//
        public string UpdatedDate { get; set; }//
        public string Installs { get; set; }//
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<DataModel> entries = new List<DataModel>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText("template.html"));

            var nodes = doc.DocumentNode.SelectSingleNode("//div[@class='bubble-element RepeatingGroup']");
            var records = nodes.ChildNodes[0].ChildNodes.Where(x => !string.IsNullOrEmpty(x.InnerText)).ToList();
            foreach (var item in records)
            {
                DataModel entry = new DataModel();
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(item.InnerHtml);

                entry.Title = HttpUtility.HtmlDecode(sub.DocumentNode.SelectSingleNode("/div[1]/div[1]/div[1]/div[2]/div[1]").InnerText);
                Console.WriteLine(entry.Title);
                var urlNode = sub.DocumentNode.SelectSingleNode("/div[1]/div[1]/div[1]/a[1]").Attributes.FirstOrDefault(x => x.Name == "href");

                if (urlNode != null)
                {
                    entry.DetailsUrl = HttpUtility.HtmlDecode(urlNode.Value);
                }
                else
                {
                    urlNode = sub.DocumentNode.SelectSingleNode("/div[1]/div[1]/div[1]/a[2]").Attributes.FirstOrDefault(x => x.Name == "href");
                    if (urlNode != null)
                    {
                        entry.DetailsUrl = HttpUtility.HtmlDecode(urlNode.Value);
                    }
                    else
                    {
                    }
                }
                Console.WriteLine(entry.DetailsUrl);
                entries.Add(entry);
            }

          


            using (var driver = new ChromeDriver())
            {
                int count = 0;
                driver.Manage().Window.Maximize();
                foreach (var item in entries)
                {
                    try
                    {
                        driver.Navigate().GoToUrl(item.DetailsUrl);
                        IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
                        wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                        Thread.Sleep(3000);
                        HtmlDocument docs = new HtmlDocument();
                        docs.LoadHtml(driver.PageSource);

                        var byNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[3]/div[1]/div/div[1]/div[1]/div/div[2]/div/a/div");
                        if (byNode != null)
                        {
                            item.Author = HttpUtility.HtmlDecode(byNode.InnerText);
                        }

                        var PriceNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[3]/div[1]/div/div[2]/div/div[2]");

                        if (PriceNode != null)
                        {
                            item.Price = HttpUtility.HtmlDecode(PriceNode.InnerText);
                        }
                        if(string.IsNullOrEmpty(item.Price))
                        {
                            PriceNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[3]/div[1]/div/div[2]/div/div[1]/div");
                            if (PriceNode != null)
                            {
                                item.Price = HttpUtility.HtmlDecode(PriceNode.InnerText);
                            }
                        }


                        var installsNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[3]/div[1]/div/div[1]/div[2]/div/div[2]/div/div");
                        if (installsNode != null)
                        {
                            item.Installs = HttpUtility.HtmlDecode(installsNode.InnerText);
                        }

                        var publishedNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[3]/div[1]/div/div[1]/div[3]/div/div/div[1]/div/div[2]/div/div");
                        if (publishedNode != null)
                        {
                            item.PublishedDate = HttpUtility.HtmlDecode(publishedNode.InnerText);
                        }

                        var updatedNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[3]/div[1]/div/div[1]/div[3]/div/div/div[2]/div/div[2]/div/div");
                        if (updatedNode != null)
                        {
                            item.UpdatedDate = HttpUtility.HtmlDecode(updatedNode.InnerText);
                        }

                        var previewNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[3]/div[2]/div/div/div[2]/a");
                        if (previewNode != null)
                        {
                            item.PreviewLink = HttpUtility.HtmlDecode(previewNode.Attributes.FirstOrDefault(x => x.Name == "href").Value);
                        }

                        var detailsNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[1]/div[1]/div/div/div/div/div/div[3]/div/div/div");
                        if (detailsNode != null)
                        {
                            item.Details = HttpUtility.HtmlDecode(detailsNode.InnerText);
                        }

                        var categoryNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[1]/div[1]/div/div/div/div/div/div[5]/div/div/div");
                        if (categoryNode != null)
                        {
                            item.Category = HttpUtility.HtmlDecode(categoryNode.InnerText);
                        }

                        var coverNode = docs.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/div/div[2]/div[4]/div/div/div/div/div/img");
                        if (coverNode != null)
                        {
                            item.CoverImage = HttpUtility.HtmlDecode(coverNode.Attributes.FirstOrDefault(x => x.Name == "src").Value);
                        }
                    }
                    catch (Exception)
                    {

                    }
                    

                    
                }


            }


            using (var writer = new StreamWriter("file.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(entries);
            }

            Console.ReadKey();
        }
    }
}
