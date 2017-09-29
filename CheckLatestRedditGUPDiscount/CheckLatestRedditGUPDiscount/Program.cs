using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLatestRedditGUPDiscount
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string LastDiscount = "";
            while (true)
            {
                ProcessRepositories(LastDiscount).Wait();
                
                Thread.Sleep(90000);
            }
        }

        private static async Task<IDictionary<string, string>> ProcessRepositories(string LastDiscount)
        {
            IDictionary<string, string> validDiscount = new Dictionary<string, string>();
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var stringTask = client.GetStringAsync("https://www.reddit.com/r/buildapcsales/search?q=GPU&sort=new&restrict_sr=on&feature=legacy_search");

            var msg = await stringTask;
            var startPosition = msg.IndexOf("id=\"siteTable\"");
            var endpositionPosition = msg.IndexOf("id=\"archived-popup\"");
            var newMSG = msg.Substring(startPosition, endpositionPosition - startPosition);

            Regex r = new Regex("data-event-action=\"title\"", RegexOptions.IgnoreCase);
            Match m = r.Match(newMSG);
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
                    if (validDiscount.Count != 0)
                    {
                        SendEmail(validDiscount, LastDiscount);
                    } 

                    return validDiscount;
                }
                else
                {
                    if (!validDiscount.ContainsKey(currentURL))
                    {
                        validDiscount.Add(currentURL, currentLastDiscount);
                    }
                }
                m = m.NextMatch();
            }

            if (validDiscount.Count != 0)
            {
                SendEmail(validDiscount, LastDiscount);
            }

            return validDiscount;
        }

        private static void SendEmail(IDictionary<string, string> validDiscount, string LastDiscount)
        {
            LastDiscount = validDiscount.OrderBy(kvp => kvp.Key).First().Value;
        }
    }
}