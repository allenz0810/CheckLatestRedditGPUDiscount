using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Linq;
using System.Net;

namespace CheckLatestRedditGPUDiscount
{
    public class Email
    {
        public string Form { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public Email()
        {
            GetData();
        }

        private void GetData()
        {
            var EmailInfos = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"AppAettings.json"));
            Form = EmailInfos["EmailForm"];
            To = EmailInfos["EmailTo"];
            Subject = EmailInfos["EmailSubject"];
            Host = EmailInfos["EmailConnectHost"];
            Port = Convert.ToInt32(EmailInfos["EmailConnectPort"]);
            UserName = EmailInfos["EmailUserName"];
            Password = EmailInfos["EmailPassword"];
        }

        //using mailkit to connect to Gmail and send email or text message
        public void SendEmailByGmail(IDictionary<string, string> validDiscounts)
        {
            var LastDiscount = validDiscounts.First().Value;
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

        public void SendTextMessageByGmail(IDictionary<string, string> validDiscounts)
        {
            var LastDiscount = validDiscounts.First().Value;
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
                mes.To.Add(new MailboxAddress("xxx", "xxx@tmomail.net"));
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
    }
}