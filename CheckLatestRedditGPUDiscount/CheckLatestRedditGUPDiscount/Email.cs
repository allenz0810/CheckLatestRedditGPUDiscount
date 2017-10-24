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
        public string FormName { get; set; }
        public string To { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string MessageForm { get; set; }
        public string MessageFormName { get; set; }
        public string MessageToMobileCompany { get; set; }
        public string MessageTo { get; set; }
        public string MessageToName { get; set; }
        public string MessageSubject { get; set; }
        public string MessageHost { get; set; }
        public int MessagePort { get; set; }
        public string MessageUserName { get; set; }
        public string MessagePassword { get; set; }

        public Email(int sendOption = 1)
        {
            if (sendOption == 1)
            {
                GetDataEmail();
            }
            else
            {
                GetMessageData();
            }
        }

        private void GetDataEmail()
        {
            var EmailInfos = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"AppAettings.json"));
            Form = EmailInfos["EmailForm"];
            FormName = EmailInfos["EmailFormName"];
            To = EmailInfos["EmailTo"];
            ToName = EmailInfos["EmailToName"];
            Subject = EmailInfos["EmailSubject"];
            Host = EmailInfos["EmailConnectHost"];
            Port = Convert.ToInt32(EmailInfos["EmailConnectPort"]);
            UserName = EmailInfos["EmailUserName"];
            Password = EmailInfos["EmailPassword"];
        }

        private void GetMessageData()
        {
            var EmailInfos = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"AppAettings.json"));
            MessageForm = EmailInfos["MessageForm"];
            MessageFormName = EmailInfos["MessageFormName"];
            MessageToMobileCompany = EmailInfos["MessageToMobileCompany"];
            MessageTo = EmailInfos["MessageTo"];
            MessageToName = EmailInfos["MessageToName"];
            MessageSubject = EmailInfos["MessageSubject"];
            MessageHost = EmailInfos["MessageConnectHost"];
            MessagePort = Convert.ToInt32(EmailInfos["MessageConnectPort"]);
            MessageUserName = EmailInfos["MessageUserName"];
            MessagePassword = EmailInfos["MessagePassword"];

            var mobileGateway = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"InternationalCarrierGateway.json"));
            MessageTo += mobileGateway[MessageToMobileCompany];
        }

        //using mailkit to connect to Gmail and send email or text message
        public string SendEmailByGmail(IDictionary<string, string> validDiscounts)
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
            message.From.Add(new MailboxAddress(FormName, Form));
            message.To.Add(new MailboxAddress(ToName, To));
            message.Subject = Subject;
            message.Body = new TextPart("plain")
            {
                Text = sb.ToString()
            };

            using (var client = new SmtpClient())
            {
                client.Connect(Host, Port, false);
                client.Authenticate(new NetworkCredential(UserName, Password));
                client.Send(message);
                client.Disconnect(true);
            }

            return LastDiscount;
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(MessageFormName, MessageForm));
            message.To.Add(new MailboxAddress(MessageToName, MessageTo));
            message.Subject = MessageSubject;
            message.Body = new TextPart("plain")
            {
                Text = sb.ToString()
            };

            using (var client = new SmtpClient())
            {
                client.Connect(MessageHost, MessagePort, false);
                client.Authenticate(new NetworkCredential(MessageUserName, MessagePassword));
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}