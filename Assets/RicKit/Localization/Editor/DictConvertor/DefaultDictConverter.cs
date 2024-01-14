using System.Collections.Generic;

namespace RicKit.Localization.DictConvertor
{
    public class DefaultDictConverter : IDictConverter
    {
        public Dictionary<string, string> Convert(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public string Convert(Dictionary<string, string> dict)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(dict);
        }
    }
}

