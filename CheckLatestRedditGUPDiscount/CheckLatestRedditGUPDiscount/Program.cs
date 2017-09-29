﻿using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
                        SendEmail(validDiscounts, LastDiscount);
                    } 

                    return validDiscounts;
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
                SendEmail(validDiscounts, LastDiscount);
            }

            return validDiscounts;
        }

        private static void SendEmail(IDictionary<string, string> validDiscounts, string LastDiscount)
        {
            LastDiscount = validDiscounts.OrderBy(kvp => kvp.Key).First().Value;

            //var client = new SmtpClient("smtp.gmail.com", 587)
            //{
            //    Credentials = new NetworkCredential("myusername@gmail.com", "mypwd"),
            //    EnableSsl = true
            //};
            //client.Send("myusername@gmail.com", "myusername@gmail.com", "test", "testbody");
            //Console.WriteLine("Sent");
            //Console.ReadLine();

            StringBuilder sb = new StringBuilder();
            foreach (var validDiscount in validDiscounts)
            {
                sb.AppendLine("Discount - (" + validDiscount.Value + ")" + ", URL - (" + validDiscount.Key + ")");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Allen", "Allen0810@gmail.com"));
            message.To.Add(new MailboxAddress("Allen", "azhuo@fsco.com"));
            message.Subject = "Reddit GUP Discount";
            message.Body = new TextPart("plain")
            {
                Text = sb.ToString()
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                //client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(new NetworkCredential("xxx", "xxx"));
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}