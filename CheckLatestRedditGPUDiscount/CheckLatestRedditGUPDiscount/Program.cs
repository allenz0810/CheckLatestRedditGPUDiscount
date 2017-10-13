using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLatestRedditGPUDiscount
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                ProcessRepositories().Wait();
                Thread.Sleep(90000);
            }
        }

        private static async Task ProcessRepositories()
        {
            IDictionary<string, string> validDiscounts = new Dictionary<string, string>();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var stringTask = client.GetStringAsync("https://www.reddit.com/r/buildapcsales/search?q=GPU&sort=new&restrict_sr=on&feature=legacy_search");

            var msg = await stringTask;
            var startPosition = msg.IndexOf("id=\"siteTable\"");
            var endpositionPosition = msg.IndexOf("id=\"archived-popup\"");
            var newMSG = msg.Substring(startPosition, endpositionPosition - startPosition);

            Regex r = new Regex("data-event-action=\"title\"", RegexOptions.IgnoreCase);
            Match m = r.Match(newMSG);
            var LastDiscount = new LastDiscountOperation().GetLastDiscount();

            while (m.Success)
            {
                var anchortagStartPoint = m.Index;
                var anchortagEndPoint = newMSG.IndexOf(@"</a>", anchortagStartPoint);
                var anchortagString = newMSG.Substring(anchortagStartPoint, anchortagEndPoint - anchortagStartPoint + 10);

                var urlStartPoint = anchortagString.IndexOf("href=\"") + 6;
                var urlEndPoint = anchortagString.IndexOf("\"", urlStartPoint);
                var currentURL = anchortagString.Substring(urlStartPoint, urlEndPoint - urlStartPoint);

                var textStatPoint = anchortagString.IndexOf(">") + 1;
                var textEndPoint = anchortagString.IndexOf("<", textStatPoint);
                var currentLastDiscount = anchortagString.Substring(textStatPoint, textEndPoint - textStatPoint);
                
                if (LastDiscount == currentLastDiscount)
                {
                    if (validDiscounts.Count != 0)
                    {
                        LastDiscount = new Email().SendEmailByGmail(validDiscounts);
                    }

                    Console.WriteLine($"Current Time : { DateTime.Now.ToString()} , Latest Discount : {LastDiscount}");
                    Console.WriteLine();
                    return;
                }
                else
                {
                    if (!validDiscounts.ContainsKey(currentURL))
                    {
                        validDiscounts.Add(currentURL, currentLastDiscount);
                    }
                }
                m = m.NextMatch();
            }

            if (validDiscounts.Count != 0)
            {
                LastDiscount = new Email().SendEmailByGmail(validDiscounts);
            }

            Console.WriteLine($"Current Time : { DateTime.Now.ToString()} , Latest Discount : {LastDiscount}");
            Console.WriteLine();
        }
    }
}