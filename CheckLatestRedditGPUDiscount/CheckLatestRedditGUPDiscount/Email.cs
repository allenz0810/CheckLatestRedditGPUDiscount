using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

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
    }
}