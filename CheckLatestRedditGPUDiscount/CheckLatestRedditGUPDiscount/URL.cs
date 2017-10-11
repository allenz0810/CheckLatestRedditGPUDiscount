using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CheckLatestRedditGPUDiscount
{
    public class URL
    {
        public string DestinationURL { get; set; }

        public URL()
        {
            GetData();
        }

        public void GetData()
        {
            var EmailInfos = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"AppAettings.json"));
            DestinationURL = EmailInfos["URL"];
        }
    }
}
