using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLatestRedditGPUDiscount
{
    internal class Program
    {
        private static string LastDiscount = "";

        private static void Main(string[] args)
        {
            LastDiscount = new LastDiscountOperation().GetLastDiscount();

            while (true)
            {
                ProcessRepositories().Wait();

                Console.WriteLine($"Current Time : { DateTime.Now.ToString()} ,Latest Discount : {LastDiscount}");
                Console.WriteLine();
                Thread.Sleep(90000);
            }
        }

        private static async Task<IDictionary<string, string>> ProcessRepositories()
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
                        SendTextMessageByGmail(validDiscounts);
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
                SendTextMessageByGmail(validDiscounts);
            }

            return validDiscounts;
        }

        //using mailkit to connect to Gmail and send email or text message
        private static void SendEmailByGmail(IDictionary<string, string> validDiscounts)
        {
            LastDiscount = validDiscounts.First().Value;
            new LastDiscountOperation().SetLastDiscount(LastDiscount);

            StringBuilder sb = new StringBuilder();
            foreach (var validDiscount in validDiscounts)
            {
                sb.AppendLine("Discount - (" + validDiscount.Value + ")" + ", URL - (" + validDiscount.Key + ")");
                sb.AppendLine("-----------------------------------------------------------------------------------");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("xxx", "xxx@gmail.com"));
            message.To.Add(new MailboxAddress("xxx", "xxx@fsco.com"));
            message.Subject = "Reddit GPU Discount";
            message.Body = new TextPart("plain")
            {
                Text = sb.ToString()
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate(new NetworkCredential("xxx@gmail.com", "xxx"));
                client.Send(message);
                client.Disconnect(true);
            }
        }

        private static void SendTextMessageByGmail(IDictionary<string, string> validDiscounts)
        {
            LastDiscount = validDiscounts.First().Value;
            new LastDiscountOperation().SetLastDiscount(LastDiscount);

            StringBuilder sb = new StringBuilder();
            foreach (var validDiscount in validDiscounts)
            {
                sb.AppendLine("Discount - (" + validDiscount.Value + ")" + ", URL - (" + validDiscount.Key + ")");
                sb.AppendLine("-----------------------------------------------------------------------------------");
            }

            using (var client = new SmtpClient())
            {
                MimeMessage mes = new MimeMessage();
                mes.From.Add(new MailboxAddress("xxx", "xxx@gmail.com"));
                mes.To.Add(new MailboxAddress("Allen", "6462202811@tmomail.net"));
                //mes.To.Add(new MailboxAddress("JiaJie", "6463594198@tmomail.net"));
                mes.Subject = "Reddit GPU Discount";
                mes.Body = new TextPart("plain")
                {
                    Text = sb.ToString()
                };

                client.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                client.Authenticate("xxx@gmail.com", "xxx");
                client.Send(mes);
                client.Disconnect(true);
            }
        }

        //use Send Grid to send mail or text message
        private async Task SendTextMessagebYSendGridAsync(IDictionary<string, string> validDiscounts)
        {
            LastDiscount = validDiscounts.First().Value;
            new LastDiscountOperation().SetLastDiscount(LastDiscount);

            StringBuilder sb = new StringBuilder();
            foreach (var validDiscount in validDiscounts)
            {
                sb.AppendLine("Discount - (" + validDiscount.Value + ")" + ", URL - (" + validDiscount.Key + ")");
                sb.AppendLine("-----------------------------------------------------------------------------------");
            }

            var apiKey = "xxx";
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("xx@gmail.com", "Allen Discount"),
                Subject = "Reddit GPU Discount",
                PlainTextContent = sb.ToString(),
            };
            msg.AddTo(new EmailAddress("xxx", "xxx@tmomail.net"));
            var response = await client.SendEmailAsync(msg);
        }
    }
}