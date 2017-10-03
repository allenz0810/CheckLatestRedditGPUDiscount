using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace CheckLatestRedditGPUDiscount
{
    public class LastDiscountOperation
    {
        public string GetLastDiscount()
        {
            return ReadFromJsonFile();
        }

        public void SetLastDiscount(string LastDiscount)
        {
            WriteToJsonFile(LastDiscount);
        }

        private string ReadFromJsonFile()
        {
            var LastDiscount = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"data.json"));
            return LastDiscount["LastDiscount"];
        }

        private void WriteToJsonFile(string LastDiscount)
        {
            string LastDiscountText = File.ReadAllText("data.json");
            dynamic jsonLastDiscount = JsonConvert.DeserializeObject(LastDiscountText);
            jsonLastDiscount["LastDiscount"] = LastDiscount;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonLastDiscount, Formatting.Indented);
            File.WriteAllText("data.json", output);
        }
    }
}